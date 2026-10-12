(function () {
    'use strict';

    var POLL_MS = 30000;
    var bell = document.getElementById('notifBell');
    var timer = null;
    var lastUnread = null;

    var urls = {
        summary: '/Notifications/Summary',
        markRead: '/Notifications/MarkRead/',
        markAll: '/Notifications/MarkAllRead',
        remove: '/Notifications/Delete/'
    };

    function token() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function post(url) {
        return fetch(url, {
            method: 'POST',
            headers: { 'RequestVerificationToken': token() },
            credentials: 'same-origin',
            keepalive: true            // يكمل حتى لو الصفحة بتتغير بعد الضغطة
        });
    }

    function esc(s) {
        return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) {
            return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
        });
    }

    /* ---------- الجرس (Dropdown) ---------- */
    if (bell) {
        var badge = document.getElementById('notifBadge');
        var list = document.getElementById('notifList');
        var markAll = document.getElementById('notifMarkAll');
        var btn = bell.querySelector('.notif-btn');

        function setBadge(n) {
            if (n > 0) { badge.textContent = n > 99 ? '99+' : n; badge.hidden = false; }
            else { badge.hidden = true; }
            markAll.disabled = n === 0;
            document.title = (n > 0 ? '(' + n + ') ' : '') + document.title.replace(/^\(\d+\+?\)\s/, '');
        }

        function render(items) {
            if (!items.length) {
                list.innerHTML = '<div class="notif-empty"><i class="bi bi-bell-slash d-block fs-4 mb-1"></i>No notifications yet</div>';
                return;
            }
            list.innerHTML = items.map(function (n) {
                return '<div class="notif-row"><a ' + (n.url ? 'href="' + esc(n.url) + '" ' : '') +
                    'class="notif-item' + (n.isRead ? '' : ' unread') + '" data-notif-id="' + n.id + '" data-read="' + (n.isRead ? 1 : 0) + '">' +
                    '<span class="notif-icon tone-' + esc(n.tone) + '"><i class="bi ' + esc(n.icon) + '"></i></span>' +
                    '<span class="notif-body"><span class="notif-msg">' + esc(n.message) + '</span>' +
                    '<span class="notif-time" title="' + esc(n.fullTime) + '">' + esc(n.timeAgo) + '</span></span>' +
                    (n.isRead ? '' : '<span class="notif-dot"></span>') + '</a>' +
                    '<button type="button" class="notif-del" data-notif-delete="' + n.id + '" aria-label="Delete notification" title="Delete">' +
                    '<i class="bi bi-x-lg"></i></button></div>';
            }).join('');
        }

        function refresh() {
            // التبويب مخفي = مفيش داعي نضغط على السيرفر
            if (document.hidden) return Promise.resolve();
            return fetch(urls.summary, { credentials: 'same-origin', headers: { 'Accept': 'application/json' } })
                .then(function (res) {
                    // الجلسة انتهت: الـ Cookie Auth بيحوّل لصفحة Login، فنوقف الـ Polling بهدوء
                    if (res.redirected || !res.ok) { stop(); return null; }
                    return res.json();
                })
                .then(function (data) {
                    if (!data) return;
                    if (lastUnread !== null && data.unreadCount > lastUnread) {
                        btn.classList.remove('ring'); void btn.offsetWidth; btn.classList.add('ring');
                    }
                    lastUnread = data.unreadCount;
                    setBadge(data.unreadCount);
                    render(data.items);
                })
                .catch(function () { /* شبكة وقعت لحظيًا: المحاولة الجاية بعد 30 ثانية */ });
        }

        function start() { if (!timer) timer = setInterval(refresh, POLL_MS); }
        function stop() { if (timer) { clearInterval(timer); timer = null; } }

        markAll.addEventListener('click', function (e) {
            e.stopPropagation();
            post(urls.markAll).then(function () { lastUnread = 0; refresh(); });
        });

        // بعد مسح إشعار (من الجرس أو من الصفحة) نحدّث العداد والقائمة
        document.addEventListener('notif:changed', function () { refresh(); });

        // فتح الجرس يحدّث القائمة فورًا
        bell.addEventListener('show.bs.dropdown', refresh);
        // رجوع للتبويب بعد ما كان مخفي
        document.addEventListener('visibilitychange', function () { if (!document.hidden) refresh(); });

        refresh();
        start();
    }

    /* ---------- الضغط على إشعار (الجرس + الصفحة الكاملة) ---------- */
    document.addEventListener('click', function (e) {
        var item = e.target.closest('.notif-item[data-notif-id]');
        if (!item || item.getAttribute('data-read') === '1') return;
        post(urls.markRead + item.getAttribute('data-notif-id'));
        item.setAttribute('data-read', '1');
        item.classList.remove('unread');
    });

    /* ---------- مسح إشعار واحد (الجرس + الصفحة الكاملة) ---------- */
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-notif-delete]');
        if (!btn || btn.disabled) return;
        e.preventDefault();
        e.stopPropagation();

        var row = btn.closest('.notif-row');
        if (!row) return;
        var item = row.querySelector('.notif-item');
        var wasUnread = !!item && item.getAttribute('data-read') !== '1';

        btn.disabled = true;
        post(urls.remove + btn.getAttribute('data-notif-delete'))
            .then(function (res) {
                if (!res.ok || res.redirected) { btn.disabled = false; return; }

                row.classList.add('removing');
                setTimeout(function () {
                    var host = row.parentElement;
                    row.remove();
                    onPageRowRemoved(host, wasUnread);
                    document.dispatchEvent(new CustomEvent('notif:changed'));
                }, 180);
            })
            .catch(function () { btn.disabled = false; });
    });

    // الصفحة الكاملة بس: تحديث عداد الـ unread، ولو الصفحة فضيت نروح للي قبلها
    function onPageRowRemoved(host, wasUnread) {
        if (!host || !host.classList.contains('notif-page')) return;

        var pageBadge = document.getElementById('notifPageBadge');
        if (wasUnread && pageBadge) {
            var left = Math.max(0, (parseInt(pageBadge.textContent, 10) || 0) - 1);
            if (left === 0) {
                pageBadge.remove();
                var markBtn = document.getElementById('notifMarkAllPage');
                if (markBtn) markBtn.remove();
            } else {
                pageBadge.textContent = left;
            }
        }

        if (!host.querySelector('.notif-row')) {
            var params = new URLSearchParams(location.search);
            var page = parseInt(params.get('page'), 10) || 1;
            if (page > 1) params.set('page', page - 1);
            location.search = params.toString();
        }
    }

    var markAllPage = document.getElementById('notifMarkAllPage');
    if (markAllPage) {
        markAllPage.addEventListener('click', function () {
            markAllPage.disabled = true;
            post(urls.markAll).then(function () { location.reload(); });
        });
    }
})();
