(() => {

    // Onay kutusu için istemci tarafı doğrulama (MustBeTrue)
if (window.jQuery && jQuery.validator && jQuery.validator.unobtrusive) {
  jQuery.validator.addMethod('mustbetrue', (value, element) => element.checked);
  jQuery.validator.unobtrusive.adapters.addBool('mustbetrue');
}

// Sayfa yeniden çizildiğinde başarı mesajına ya da ilk hataya kaydır
window.addEventListener('load', () => {
  const target = document.querySelector('.alert-success, .field-validation-error, .input-validation-error');
  if (!target) return;
  const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  target.scrollIntoView({ behavior: reduce ? 'auto' : 'smooth', block: 'center' });
});

  const panel = document.getElementById('apptPanel');
  const subject = document.getElementById('Subject');
  if (!panel || !subject) return;

  const slotInput = document.getElementById('AppointmentSlot');
  const calendarEl = document.getElementById('apptCalendar');
  const grid = document.getElementById('calendar');
  const dateText = document.getElementById('apptDateText');
  const timeBtn = document.getElementById('apptTimeBtn');
  const timeText = document.getElementById('apptTimeText');
  const timeMenu = document.getElementById('apptTimeMenu');

  const apptValue = panel.dataset.apptValue;
  const slotsUrl = panel.dataset.slotsUrl;
  const closed = (panel.dataset.closed || '').split(',').filter(Boolean).map(Number);

  const pad = n => String(n).padStart(2, '0');
  const parse = s => { const [y, m, d] = s.split('-').map(Number); return new Date(y, m - 1, d); };
  const iso = d => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
  const human = d => `${pad(d.getDate())}.${pad(d.getMonth() + 1)}.${d.getFullYear()}`;

  const min = parse(panel.dataset.min);
  const max = parse(panel.dataset.max);
  const monthFmt = new Intl.DateTimeFormat('tr-TR', { month: 'long', year: 'numeric' });
  const dayFmt = new Intl.DateTimeFormat('tr-TR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
  const WEEKDAYS = ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'];

  const panelCollapse = bootstrap.Collapse.getOrCreateInstance(panel, { toggle: false });
  const calCollapse = bootstrap.Collapse.getOrCreateInstance(calendarEl, { toggle: false });

  let selected = null;
  let view = new Date(min.getFullYear(), min.getMonth(), 1);
  let loadToken = 0;

  const isDisabled = d => d < min || d > max || closed.includes(d.getDay());

  // ---- Panel: konu "Randevu talebi" ise aç, değilse kapat
  const syncPanel = () => {
    if (subject.value === apptValue) panelCollapse.show();
    else panelCollapse.hide();
  };
  subject.addEventListener('change', syncPanel);
  window.addEventListener('pageshow', () => {
    const shown = panel.classList.contains('show');
    if ((subject.value === apptValue) !== shown) syncPanel();
  });

  // ---- Takvim
  function navButton(text, label, dir, onClick) {
    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'cal-nav';
    b.textContent = text;
    b.dataset.dir = dir;
    b.setAttribute('aria-label', label);
    b.addEventListener('click', onClick);
    return b;
  }

  function renderCalendar(focusDir) {
    grid.replaceChildren();

    const head = document.createElement('div');
    head.className = 'cal-head';
    const prev = navButton('‹', 'Önceki ay', 'prev', () => {
      view = new Date(view.getFullYear(), view.getMonth() - 1, 1);
      renderCalendar('prev');
    });
    const next = navButton('›', 'Sonraki ay', 'next', () => {
      view = new Date(view.getFullYear(), view.getMonth() + 1, 1);
      renderCalendar('next');
    });
    prev.disabled = view <= new Date(min.getFullYear(), min.getMonth(), 1);
    next.disabled = view >= new Date(max.getFullYear(), max.getMonth(), 1);

    const title = document.createElement('div');
    title.className = 'cal-title';
    title.setAttribute('aria-live', 'polite');
    title.textContent = monthFmt.format(view);
    head.append(prev, title, next);

    const days = document.createElement('div');
    days.className = 'cal-grid';
    WEEKDAYS.forEach(w => {
      const s = document.createElement('span');
      s.className = 'cal-wd';
      s.textContent = w;
      days.appendChild(s);
    });

    const y = view.getFullYear();
    const m = view.getMonth();
    const offset = (new Date(y, m, 1).getDay() + 6) % 7;      // hafta Pazartesi başlar
    for (let i = 0; i < offset; i++) days.appendChild(document.createElement('span'));

    const count = new Date(y, m + 1, 0).getDate();
    for (let day = 1; day <= count; day++) {
      const d = new Date(y, m, day);
      const b = document.createElement('button');
      b.type = 'button';
      b.className = 'cal-day';
      b.textContent = day;
      b.disabled = isDisabled(d);
      b.setAttribute('aria-label', dayFmt.format(d));
      if (selected && iso(selected) === iso(d)) {
        b.classList.add('selected');
        b.setAttribute('aria-pressed', 'true');
      }
      b.addEventListener('click', () => pickDate(d));
      days.appendChild(b);
    }

    grid.append(head, days);
    if (focusDir) grid.querySelector(`[data-dir="${focusDir}"]:not(:disabled)`)?.focus();
  }

  function pickDate(d) {
    selected = d;
    slotInput.value = '';
    dateText.textContent = human(d);
    dateText.classList.remove('placeholder-text');
    renderCalendar();
    calCollapse.hide();          // takvim accordion gibi kapanır
    loadSlots(d);
  }

  // ---- Saatler
  function chooseTime(d, time, item) {
    slotInput.value = `${iso(d)}T${time}`;
    timeText.textContent = `${human(d)} - ${time}`;
    timeText.classList.remove('placeholder-text');
    timeMenu.querySelectorAll('.dropdown-item.active').forEach(el => el.classList.remove('active'));
    item.classList.add('active');
    const err = document.querySelector('[data-valmsg-for="AppointmentSlot"]');
    if (err) err.textContent = '';
  }

  function slotItem(d, s) {
    const li = document.createElement('li');
    const b = document.createElement('button');
    b.type = 'button';
    b.className = 'dropdown-item slot-item';
    b.dataset.time = s.time;
    b.disabled = !s.available;

    const label = document.createElement('span');
    label.textContent = `${human(d)} - ${s.time} -`;
    const state = document.createElement('span');
    state.className = 'slot-state ' + (s.available ? 'available' : 'full');
    state.textContent = s.available ? 'Müsait' : 'Dolu';
    b.append(label, state);

    b.addEventListener('click', () => chooseTime(d, s.time, b));
    li.appendChild(b);
    return li;
  }

  async function loadSlots(d, preselect) {
    const token = ++loadToken;
    timeMenu.replaceChildren();
    timeBtn.disabled = true;
    timeText.textContent = 'Saatler yükleniyor...';
    timeText.classList.add('placeholder-text');

    try {
      const res = await fetch(`${slotsUrl}?date=${iso(d)}`, { headers: { Accept: 'application/json' } });
      if (!res.ok) throw new Error('slots');
      const slots = await res.json();
      if (token !== loadToken) return;

      if (!slots.length) {
        timeText.textContent = 'Bu tarihte randevu verilemiyor';
        return;
      }
      slots.forEach(s => timeMenu.appendChild(slotItem(d, s)));
      timeBtn.disabled = false;
      timeText.textContent = 'Saat seçin';

      if (preselect) {
        timeMenu.querySelector(`[data-time="${preselect}"]:not(:disabled)`)?.click();
      }
    } catch {
      if (token === loadToken) timeText.textContent = 'Saatler alınamadı, tarihi yeniden seçin';
    }
  }

  // ---- Sayfa yeniden çizildiğinde (doğrulama hatası) seçimi geri yükle
  function restore() {
    const m = /^(\d{4}-\d{2}-\d{2})T(\d{2}:\d{2})$/.exec(slotInput.value || '');
    if (!m) return;
    const d = parse(m[1]);
    slotInput.value = '';
    if (isDisabled(d)) return;

    selected = d;
    view = new Date(d.getFullYear(), d.getMonth(), 1);
    dateText.textContent = human(d);
    dateText.classList.remove('placeholder-text');
    loadSlots(d, m[2]);
  }

  restore();
  renderCalendar();
})();