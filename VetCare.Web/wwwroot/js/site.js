// oculta el menú lateral en pantallas grandes y lo muestra en pantallas pequeñas
document.getElementById('vcMenuToggle')?.addEventListener('click', () => {
    const clase = window.innerWidth < 992 ? 'vc-menu-abierto' : 'vc-menu-cerrado';
    document.body.classList.toggle(clase);
});