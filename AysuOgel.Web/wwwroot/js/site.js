// Navbar: kaydırınca gölge
(() => {
  const nav = document.querySelector('.site-nav');
  if (!nav) return;
  const onScroll = () => nav.classList.toggle('scrolled', window.scrollY > 8);
  onScroll();
  window.addEventListener('scroll', onScroll, { passive: true });
})();

// Görününce beliren bloklar
(() => {
  const items = document.querySelectorAll('.reveal');
  if (!items.length) return;
  if (!('IntersectionObserver' in window)) {
    items.forEach(el => el.classList.add('visible'));
    return;
  }
  const io = new IntersectionObserver((entries) => {
    entries.forEach(en => {
      if (en.isIntersecting) {
        en.target.classList.add('visible');
        io.unobserve(en.target);
      }
    });
  }, { threshold: 0.12 });
  items.forEach(el => io.observe(el));
})();

// Hareketi azaltmak isteyenlerde slider otomatik kaymasın
(() => {
  const banner = document.getElementById('homeBanner');
  if (banner && window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
    banner.removeAttribute('data-bs-ride');
  }
})();

// Galeri: tıklayınca büyük görsel
document.addEventListener('click', (e) => {
  const a = e.target.closest('a[data-lightbox]');
  if (!a) return;
  e.preventDefault();
  const caption = a.dataset.caption || '';
  const img = document.getElementById('lightboxImg');
  img.src = a.href;
  img.alt = caption;
  document.getElementById('lightboxCap').textContent = caption;
  bootstrap.Modal.getOrCreateInstance(document.getElementById('lightbox')).show();
});

// Kart kaydırıcı: oklar, uçlarda pasif, taşma yoksa oklar gizli
(() => {
    const reduce = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    document.querySelectorAll('[data-card-slider]').forEach(slider => {
        const track = slider.querySelector('.slider-track');
        const prev = slider.querySelector('.slider-arrow.prev');
        const next = slider.querySelector('.slider-arrow.next');
        if (!track || !prev || !next) return;

        let ticking = false;
        const update = () => {
            const max = track.scrollWidth - track.clientWidth;
            slider.classList.toggle('no-scroll', max <= 2);
            prev.disabled = track.scrollLeft <= 2;
            next.disabled = track.scrollLeft >= max - 2;
            ticking = false;
        };
        const queueUpdate = () => {
            if (!ticking) { ticking = true; requestAnimationFrame(update); }
        };

        const move = dir => {
            const item = track.querySelector('.slider-item');
            if (!item) return;
            const gap = parseFloat(getComputedStyle(track).columnGap) || 0;
            const itemWidth = item.getBoundingClientRect().width + gap;
            const perView = Math.max(1, Math.round(track.clientWidth / itemWidth));
            track.scrollBy({ left: dir * itemWidth * perView, behavior: reduce ? 'auto' : 'smooth' });
        };

        prev.addEventListener('click', () => move(-1));
        next.addEventListener('click', () => move(1));
        track.addEventListener('scroll', queueUpdate, { passive: true });
        window.addEventListener('resize', queueUpdate);
        window.addEventListener('load', update);
        update();
    });
})();