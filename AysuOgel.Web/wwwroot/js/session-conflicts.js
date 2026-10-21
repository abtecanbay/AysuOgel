// Seans kaydetmeden önce ±1 saat içindeki diğer seansları kontrol eder; varsa onay penceresi açar.
// Form: data-conflict-check, data-conflict-url, isteğe bağlı data-except-id / data-original-at / data-original-status.
(function () {
    const modalEl = document.getElementById('sessionConflictModal');
    if (!modalEl || typeof bootstrap === 'undefined') return;

    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    const message = document.getElementById('sessionConflictMessage');
    const rows = document.getElementById('sessionConflictRows');
    const confirmBtn = document.getElementById('sessionConflictConfirm');
    const isCancelled = v => v === '3' || v === 'IptalEdildi';
    let pendingForm = null;

    function submitConfirmed(form) {
        form.querySelector('input[name="ConfirmConflicts"]').value = 'true';
        form.submit();   // submit olayı tekrar tetiklenmez
    }

    function apart(minutes) {
        if (minutes === 0) return 'Aynı saat';
        return `${Math.abs(minutes)} dk ${minutes < 0 ? 'önce' : 'sonra'}`;
    }

    function cell(tr, text, className) {
        const td = document.createElement('td');
        if (className) td.className = className;
        td.textContent = text;
        tr.appendChild(td);
        return td;
    }

    function render(conflicts) {
        message.replaceChildren();
        const strong = t => { const b = document.createElement('strong'); b.textContent = t; return b; };

        if (conflicts.length === 1) {
            const c = conflicts[0];
            message.append(strong(c.memberName), ' üyesinin ', strong(c.scheduledAt),
                ' tarihinde seansı vardır. Yine de planı kaydetmek istiyor musunuz?');
        } else {
            message.append('Seçtiğiniz saatin 1 saat öncesi / sonrası içinde ', strong(`${conflicts.length} seans`),
                ' planlı. Cihaz ve hizmet kapasiteniz yeterliyse planı yine de kaydedebilirsiniz.');
        }

        rows.replaceChildren();
        for (const c of conflicts) {
            const tr = document.createElement('tr');
            const nameTd = cell(tr, '', 'fw-semibold');
            const link = document.createElement('a');
            link.href = c.detailsUrl;
            link.target = '_blank';
            link.rel = 'noopener';
            link.textContent = c.memberName;
            nameTd.appendChild(link);
            cell(tr, c.packageName);
            cell(tr, `${c.sessionNumber}. seans · ${c.status}`, 'text-nowrap');
            cell(tr, c.scheduledAt, 'text-nowrap');
            cell(tr, apart(c.minutesApart), 'text-nowrap small text-muted');
            rows.appendChild(tr);
        }
    }

    confirmBtn.addEventListener('click', () => {
        if (!pendingForm) return;
        confirmBtn.disabled = true;
        submitConfirmed(pendingForm);
    });
    modalEl.addEventListener('hidden.bs.modal', () => { pendingForm = null; confirmBtn.disabled = false; });

    document.querySelectorAll('form[data-conflict-check]').forEach(form => {
        form.addEventListener('submit', async e => {
            if (form.querySelector('input[name="ConfirmConflicts"]').value === 'true') return;

            const at = (form.querySelector('[name="ScheduledAt"]').value || '').slice(0, 16);
            const status = form.querySelector('[name="Status"]').value;
            if (!at || isCancelled(status)) return;

            // Düzenlemede saat değişmediyse tekrar sorma.
            if (form.dataset.originalAt === at && !isCancelled(form.dataset.originalStatus || '')) return;

            // Geçersiz formda jQuery Validate zaten gönderimi durdurur.
            if (window.jQuery && jQuery(form).valid && !jQuery(form).valid()) return;

            e.preventDefault();
            const url = new URL(form.dataset.conflictUrl, location.origin);
            url.searchParams.set('at', at);
            if (form.dataset.exceptId) url.searchParams.set('exceptId', form.dataset.exceptId);

            let conflicts;
            try {
                const res = await fetch(url, { headers: { 'Accept': 'application/json' }, credentials: 'same-origin' });
                if (!res.ok) throw new Error(res.status);
                conflicts = await res.json();
            } catch {
                form.submit();   // kontrol yapılamadı; sunucu yine de çakışmayı denetler
                return;
            }

            if (conflicts.length === 0) { form.submit(); return; }

            pendingForm = form;
            render(conflicts);
            modal.show();
        });
    });
})();
