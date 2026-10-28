(() => {
  document.querySelectorAll('.password-toggle').forEach((btn) => {
    btn.addEventListener('click', () => {
      const id = btn.getAttribute('data-target');
      const input = id ? document.getElementById(id) : btn.parentElement?.querySelector('input');
      if (!input) return;
      const show = input.type === 'password';
      input.type = show ? 'text' : 'password';
      btn.setAttribute('aria-pressed', show ? 'true' : 'false');
      btn.classList.toggle('is-on', show);
    });
  });
})();
