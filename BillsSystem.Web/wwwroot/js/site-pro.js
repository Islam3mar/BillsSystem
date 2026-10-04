// ============================================================
// Bills System — Pro Layer JS
// بيوفر: Toast Notifications + Confirm Modal بدل confirm() الافتراضية
// ============================================================

function showToast(type, message) {
    const stack = document.getElementById('toastStack');
    if (!stack || !message) return;

    const icon = type === 'error' ? 'bi-x-circle-fill' : 'bi-check-circle-fill';
    const el = document.createElement('div');
    el.className = 'pro-toast' + (type === 'error' ? ' toast-error' : '');

    // الهيكل ثابت (مفيش بيانات مستخدم جواه)
    el.innerHTML = `
        <i class="bi ${icon} toast-icon"></i>
        <div class="toast-text"></div>
        <button type="button" class="toast-close">&times;</button>
    `;
    // الرسالة نفسها نص عادي مش HTML، فأي اسم عميل فيه < أو > مش هيتنفذ
    el.querySelector('.toast-text').textContent = message;

    const remove = () => {
        el.classList.add('toast-out');
        setTimeout(() => el.remove(), 200);
    };

    el.querySelector('.toast-close').addEventListener('click', remove);
    stack.appendChild(el);
    setTimeout(remove, 4500);
}

// ---------- Confirm Modal (بديل confirm()) ----------
// الاستخدام في أي form حذف:
// <form asp-action="Delete" asp-route-id="@item.Id" method="post"
//       onsubmit="return confirmDelete(this, 'Delete this client permanently?')">
function confirmDelete(form, message) {
    const modalEl = document.getElementById('confirmModal');
    if (!modalEl || !window.bootstrap) return confirm(message); // fallback

    document.getElementById('confirmModalBody').textContent = message || 'This action cannot be undone.';
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    const okBtn = document.getElementById('confirmModalOkBtn');

    const onConfirm = () => {
        cleanup();
        modal.hide();
        form.submit();
    };
    // بيتنادى عند التأكيد، وكمان لما المودال يتقفل بـ Cancel أو X أو Esc
    const cleanup = () => {
        okBtn.removeEventListener('click', onConfirm);
        modalEl.removeEventListener('hidden.bs.modal', cleanup);
    };

    okBtn.addEventListener('click', onConfirm);
    modalEl.addEventListener('hidden.bs.modal', cleanup);

    modal.show();
    return false; // يمنع الـ submit الفوري لحد ما المستخدم يأكد
}

// ============================================================
// منع الضغط المزدوج على أي فورم POST + إعادة تفعيل الأزرار لما المتصفح يرجّع الصفحة من الـ Back
// ============================================================
document.addEventListener('submit', function (e) {
    const form = e.target;
    if (!(form instanceof HTMLFormElement)) return;
    if ((form.method || '').toLowerCase() !== 'post') return;
    if (e.defaultPrevented) return;            // الفورم اتمنع (تأكيد اتلغى / Validation فشل)

    if (form.dataset.submitted === '1') { e.preventDefault(); return; }
    form.dataset.submitted = '1';

    form.querySelectorAll('button[type=submit], button:not([type])').forEach(function (b) {
        if (!b.disabled) { b.disabled = true; b.dataset.autoDisabled = '1'; }
    });
}, false);

window.addEventListener('pageshow', function (e) {
    if (!e.persisted) return;
    document.querySelectorAll('form[data-submitted="1"]').forEach(function (f) {
        delete f.dataset.submitted;
        f.querySelectorAll('button[data-auto-disabled="1"]').forEach(function (b) {
            b.disabled = false;
            delete b.dataset.autoDisabled;
        });
    });
});