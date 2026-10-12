// ============================================================
// Sidebar toggle
// ديسكتوب: بيقفل/يفتح الـ sidebar ويحفظ اختيارك.
// موبايل (<= 768px): الـ sidebar مخفي، والزرار بيفتحه فوق المحتوى.
// بيتحمّل في <head> عشان الحالة المحفوظة تتطبق قبل أول رسم (مفيش وميض).
// ============================================================
(function () {
    var root = document.documentElement;
    var KEY = 'sidebar-collapsed';
    var mobile = window.matchMedia('(max-width: 768px)');

    try { if (localStorage.getItem(KEY) === '1') root.classList.add('sidebar-collapsed'); } catch (e) { }

    document.addEventListener('click', function (e) {
        if (e.target.closest('[data-sidebar-toggle]')) {
            if (mobile.matches) { root.classList.toggle('sidebar-open'); return; }
            var collapsed = root.classList.toggle('sidebar-collapsed');
            try { localStorage.setItem(KEY, collapsed ? '1' : '0'); } catch (err) { }
            return;
        }
        // موبايل: الضغط برّه الـ sidebar بيقفله
        if (root.classList.contains('sidebar-open') && !e.target.closest('.sidebar')) {
            root.classList.remove('sidebar-open');
        }
    });
})();
