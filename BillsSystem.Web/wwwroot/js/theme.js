// ============================================================
// Dark / Light theme
// بيتحمّل في <head> قبل الـ CSS عشان مفيش وميض أبيض عند التحميل.
// الأولوية: اختيار المستخدم المحفوظ، وبعدين إعداد النظام (prefers-color-scheme).
// ============================================================
(function () {
    var root = document.documentElement;
    var KEY = 'theme';
    var mq = window.matchMedia('(prefers-color-scheme: dark)');

    function saved() {
        try { var v = localStorage.getItem(KEY); return v === 'light' || v === 'dark' ? v : null; }
        catch (e) { return null; }
    }
    function current() { return saved() || (mq.matches ? 'dark' : 'light'); }
    function apply(t) { root.setAttribute('data-bs-theme', t); }

    apply(current());

    // لو المستخدم ماختارش بإيده، نمشي مع تغيير إعداد النظام
    mq.addEventListener('change', function () { if (!saved()) apply(current()); });

    // الطباعة (Sales Report) دايمًا فاتحة: خلفية بيضا وكلام غامق
    window.addEventListener('beforeprint', function () { apply('light'); });
    window.addEventListener('afterprint', function () { apply(current()); });

    document.addEventListener('click', function (e) {
        if (!e.target.closest('[data-theme-toggle]')) return;
        var next = root.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
        try { localStorage.setItem(KEY, next); } catch (err) { /* private mode */ }
        apply(next);
    });
})();
