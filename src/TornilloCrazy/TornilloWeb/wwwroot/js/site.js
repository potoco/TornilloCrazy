// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const sidebar = document.querySelector('.app-sidebar');
const btnCollapse = document.querySelector('[aria-label="Colapsar menu lat izq"]');

btnCollapse.addEventListener('click', () => {
    document.body.classList.toggle('sidebar-collapsed');
});

sidebar.addEventListener('transitionend', e => {

    if (e.propertyName !== 'width')
        return;

    const collapsed =
        document.body.classList.contains('sidebar-collapsed');

    localStorage.setItem('sidebar-collapsed', collapsed);
});

document.addEventListener('DOMContentLoaded', () => {

    const sidebar = document.querySelector('.app-sidebar');

    // Desactivamos temporalmente la animación
    sidebar.classList.add('no-transition');

    // Restauramos la preferencia
    const collapsed =
        localStorage.getItem('sidebar-collapsed') === 'true';

    document.body.classList.toggle('sidebar-collapsed', collapsed);

    // Forzamos al navegador a aplicar el estado sin transición
    sidebar.offsetHeight;

    // Volvemos a habilitar las animaciones
    sidebar.classList.remove('no-transition');
});

window.addEventListener('load', () => {

    requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            document.body.classList.remove('no-transition');
        });
    });

});