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