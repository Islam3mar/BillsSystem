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
    el.innerHTML = `
        <i class="bi ${icon} toast-icon"></i>
        <div class="toast-text">${message}</div>
        <button type="button" class="toast-close">&times;</button>
    `;

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
    const modal = new bootstrap.Modal(modalEl);
    const okBtn = document.getElementById('confirmModalOkBtn');

    const onConfirm = () => {
        okBtn.removeEventListener('click', onConfirm);
        modal.hide();
        form.submit();
    };
    okBtn.addEventListener('click', onConfirm);

    modal.show();
    return false; // يمنع الـ submit الفوري لحد ما المستخدم يأكد
}