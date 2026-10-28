(() => {
    'use strict';
    const $ = id => document.getElementById(id);
    const money = value => new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(value);
    const escape = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
    const round = value => Math.round((value + Number.EPSILON) * 100) / 100;
    const form = $('venta-form');
    const telefono = window.matchMedia('(max-width: 575.98px)');
    $('venta-detalle-toggle').addEventListener('click', () => {
        const abierto = $('venta-detalle-contenedor').classList.toggle('venta-detalle-abierto');
        $('venta-detalle-toggle').setAttribute('aria-expanded', String(abierto));
        $('venta-detalle-toggle').textContent = abierto ? 'Ocultar detalle del producto' : 'Ver detalle del producto';
    });
    const tabs = [$('venta-tab-productos'), $('venta-tab-carrito')];
    let tabActivo = form.dataset.esPost === 'true' && form.dataset.ventaIngresada !== 'true' ? 1 : 0;
    function mostrarTab(indice, enfocar = false) {
        tabActivo = indice;
        tabs.forEach((tab, i) => {
            const activo = i === indice;
            tab.classList.toggle('active', activo);
            tab.setAttribute('aria-selected', String(activo));
            tab.tabIndex = activo ? 0 : -1;
            const panel = $(tab.getAttribute('aria-controls'));
            panel.classList.toggle('venta-panel-inactivo', !activo);
            if (telefono.matches) {
                panel.setAttribute('role', 'tabpanel');
                panel.setAttribute('aria-labelledby', tab.id);
            } else {
                panel.removeAttribute('role');
                panel.removeAttribute('aria-labelledby');
            }
        });
        if (enfocar) tabs[indice].focus();
    }
    tabs.forEach((tab, i) => {
        tab.addEventListener('click', () => mostrarTab(i));
        tab.addEventListener('keydown', event => {
            let indice;
            if (event.key === 'ArrowRight' || event.key === 'ArrowLeft') indice = 1 - i;
            else if (event.key === 'Home') indice = 0;
            else if (event.key === 'End') indice = 1;
            else return;
            event.preventDefault();
            mostrarTab(indice, true);
        });
    });
    telefono.addEventListener('change', () => mostrarTab(tabActivo));
    form.addEventListener('invalid', () => { if (telefono.matches) mostrarTab(1); }, true);
    mostrarTab(tabActivo);
    const claveBorrador = `tornillo:venta:borrador:${location.pathname}:${form.dataset.borradorUsuario}`;
    let conservarBorrador = false;
    function guardarBorrador() {
        conservarBorrador = true;
        try {
            localStorage.setItem(claveBorrador, JSON.stringify({
                carrito, clienteId: $('ClienteId').value, iva: $('IvaPorcentaje').value, buscar: $('buscar').value
            }));
        } catch { /* La venta continúa disponible si el navegador no permite almacenamiento local. */ }
    }
    function borrarBorrador() {
        conservarBorrador = false;
        try { localStorage.removeItem(claveBorrador); } catch { /* Almacenamiento no disponible. */ }
    }
    let carrito = [], productos = [], pagina = 1, total = 0, seleccionado = null, request;
    try {
        const saved = JSON.parse($('CarritoJson').value);
        if (Array.isArray(saved)) carrito = saved.filter(p => Number.isInteger(p.productoId) && Number.isInteger(p.cantidad) && p.cantidad > 0 && Number.isFinite(p.precio));
    } catch { /* El servidor muestra el error de validación del carrito. */ }
    if (form.dataset.ventaIngresada === 'true') {
        borrarBorrador();
        carrito = [];
    } else if (form.dataset.esPost === 'true') {
        // Una respuesta con errores conserva los valores enviados, sin sustituirlos por un borrador anterior.
        guardarBorrador();
    } else {
        try {
            const borrador = JSON.parse(localStorage.getItem(claveBorrador));
            if (borrador && Array.isArray(borrador.carrito)) {
                carrito = borrador.carrito.filter(p => Number.isInteger(p.productoId) && p.productoId > 0 &&
                    Number.isInteger(p.cantidad) && p.cantidad > 0 && Number.isFinite(p.precio) && p.precio >= 0 &&
                    Number.isInteger(p.stock) && p.stock >= 0).slice(0, 200);
                $('ClienteId').value = borrador.clienteId ?? '';
                $('IvaPorcentaje').value = borrador.iva ?? '21';
                $('buscar').value = borrador.buscar ?? '';
                conservarBorrador = true;
            }
        } catch { /* Un borrador inválido no impide usar la pantalla. */ }
    }
    const error = message => { $('venta-error').textContent = message; $('venta-error').classList.toggle('d-none', !message); };
    function render(persistir = true) {
        $('carrito-lista').innerHTML = carrito.length ? carrito.map(p => `<tr>
            <td class="ps-3 venta-nombre fw-semibold">${escape(p.nombre)}</td>
            <td data-label="Cantidad"><div class="d-flex align-items-center justify-content-center gap-1">
                <button type="button" class="btn btn-sm btn-outline-secondary" data-id="${p.productoId}" data-action="menos" aria-label="Restar cantidad de ${escape(p.nombre)}">−</button>
                <input class="form-control form-control-sm venta-cantidad" type="number" min="1" max="${p.stock}" value="${p.cantidad}" data-id="${p.productoId}" aria-label="Cantidad de ${escape(p.nombre)}" />
                <button type="button" class="btn btn-sm btn-outline-primary" data-id="${p.productoId}" data-action="mas" aria-label="Sumar cantidad de ${escape(p.nombre)}">+</button>
            </div></td><td class="text-end text-nowrap" data-label="P. unitario">${money(p.precio)}</td><td class="text-end text-nowrap" data-label="Subtotal">${money(round(p.precio * p.cantidad))}</td>
            <td><button type="button" class="btn btn-sm btn-outline-danger" data-id="${p.productoId}" data-action="quitar" aria-label="Quitar ${escape(p.nombre)}"><i class="bi bi-trash"></i></button></td></tr>`).join('') : '<tr><td colspan="5" class="text-center text-muted p-5"><i class="bi bi-cart3 fs-1 d-block mb-2"></i>Tu carrito está vacío.<br>Agregá productos desde el buscador.</td></tr>';
        const tasa = Number($('IvaPorcentaje').value) || 0;
        const porcentajeDescuento = Number($('ClienteId').selectedOptions[0]?.dataset.descuento) || 0;
        const subtotal = carrito.reduce((sum, p) => sum + round(p.precio * p.cantidad), 0);
        const descuento = carrito.reduce((sum, p) => sum + round(round(p.precio * p.cantidad) * porcentajeDescuento / 100), 0);
        const iva = carrito.reduce((sum, p) => {
            const bruto = round(p.precio * p.cantidad);
            const neto = round(bruto - round(bruto * porcentajeDescuento / 100));
            return sum + round(neto * tasa / 100);
        }, 0);
        $('descuento-porcentaje').textContent = new Intl.NumberFormat('es-AR', { maximumFractionDigits: 2 }).format(porcentajeDescuento);
        $('descuento').textContent = money(-descuento);
        $('subtotal').textContent = money(subtotal); $('iva').textContent = money(iva); $('total').textContent = money(subtotal - descuento + iva);
        $('carrito-cuenta').textContent = carrito.length;
        $('carrito-tab-cuenta').textContent = carrito.length;
        $('ingresar').disabled = !carrito.length;
        $('CarritoJson').value = JSON.stringify(carrito);
        if (persistir) guardarBorrador();
    }
    function detalle(p) {
        seleccionado = p;
        document.querySelectorAll('#productos-lista tr').forEach(row => row.classList.toggle('table-primary', Number(row.dataset.id) === p.id));
        const img = p.imagen ? `<img class="venta-foto" src="${escape(new URL(`../../../img/productos/${encodeURIComponent(p.imagen)}`, location.href).pathname)}" alt="${escape(p.nombre)}" />` : '<div class="text-center text-muted p-4"><i class="bi bi-image fs-1"></i><p>Sin imagen</p></div>';
        $('detalle').innerHTML = `<div class="row g-3"><div class="col-7"><h2 class="h6 fw-bold">${escape(p.nombre)}</h2><dl class="small mb-2"><dt>Rubros</dt><dd>${escape(p.rubros || 'Sin rubros')}</dd><dt>Sinónimos</dt><dd>${escape(p.sinonimos || 'Sin sinónimos')}</dd><dt>Descripción detallada</dt><dd style="white-space:pre-wrap">${escape(p.descripcion || 'Sin descripción.')}</dd></dl></div><div class="col-5">${img}</div></div>`;
        $('detalle').scrollTop = 0;
    }
    function agregar(p) {
        const actual = carrito.find(x => x.productoId === p.id);
        if ((actual?.cantidad || 0) >= p.stock) return error('No hay más stock disponible para este producto.');
        if (!actual && carrito.length >= 200) return error('El carrito admite hasta 200 productos distintos.');
        if (actual) actual.cantidad++;
        else carrito.push({ productoId: p.id, cantidad: 1, nombre: p.nombre, precio: p.precio, stock: p.stock });
        error(''); render();
    }
    async function buscar(nuevaPagina) {
        request?.abort(); request = new AbortController();
        $('anterior').disabled = $('siguiente').disabled = true;
        try {
            const url = new URL(location.href); url.search = '';
            url.searchParams.set('handler', 'Productos'); url.searchParams.set('buscar', $('buscar').value); url.searchParams.set('pagina', nuevaPagina);
            const response = await fetch(url, { signal: request.signal, headers: { Accept: 'application/json' } });
            if (!response.ok || !response.headers.get('content-type')?.includes('application/json')) throw new Error();
            const data = await response.json();
            productos = data.productos; pagina = data.pagina; total = data.total;
            $('productos-total').textContent = total;
            $('pagina').textContent = `${total ? (pagina - 1) * 10 + 1 : 0}–${Math.min(pagina * 10, total)} de ${total}`;
            $('productos-lista').innerHTML = productos.length ? productos.map(p => `<tr data-id="${p.id}"><td class="ps-3">${escape(p.codigo || '—')}</td><td><button type="button" class="btn btn-link p-0 text-start text-decoration-none fw-semibold small" data-id="${p.id}">${escape(p.nombre)}</button></td><td class="text-end text-nowrap">${money(p.precio)}</td><td class="text-end pe-3"><button type="button" class="btn btn-sm btn-outline-primary" data-id="${p.id}" data-action="agregar" aria-label="Agregar ${escape(p.nombre)} al carrito" title="${p.stock > 0 ? 'Agregar al carrito' : 'Sin stock'}" ${p.stock <= 0 ? 'disabled' : ''}><i class="bi bi-cart-plus"></i></button></td></tr>`).join('') : '<tr><td colspan="4" class="text-center text-muted p-4">Sin productos encontrados.</td></tr>';
            $('productos-lista').parentElement.parentElement.scrollTop = 0;
            $('anterior').disabled = pagina <= 1; $('siguiente').disabled = pagina * 10 >= total;
            if (productos.length) detalle(productos.find(p => p.id === seleccionado?.id) || productos[0]);
            else { seleccionado = null; $('detalle').textContent = 'Probá con otro nombre o código.'; }
            error('');
        } catch (ex) {
            if (ex.name !== 'AbortError') error('No se pudieron cargar los productos. Volvé a buscar para reintentar.');
        }
    }
    $('buscador').onsubmit = event => { event.preventDefault(); buscar(1); };
    $('anterior').onclick = () => buscar(pagina - 1);
    $('siguiente').onclick = () => buscar(pagina + 1);
    $('productos-lista').onclick = event => { const button = event.target.closest('button[data-id]'); if (!button) return; const p = productos.find(p => p.id === Number(button.dataset.id)); detalle(p); if (button.dataset.action === 'agregar') agregar(p); };
    $('carrito-lista').onclick = event => {
        const button = event.target.closest('button[data-action]'); if (!button) return;
        const p = carrito.find(x => x.productoId === Number(button.dataset.id));
        if (button.dataset.action === 'quitar') carrito = carrito.filter(x => x !== p);
        if (button.dataset.action === 'menos' && p.cantidad > 1) p.cantidad--;
        if (button.dataset.action === 'mas') {
            if (p.cantidad >= p.stock) return error('No hay más stock disponible para este producto.');
            p.cantidad++;
        }
        error(''); render();
    };
    $('carrito-lista').onchange = event => {
        const input = event.target.closest('input[data-id]'); if (!input) return;
        const p = carrito.find(x => x.productoId === Number(input.dataset.id)), cantidad = Number(input.value);
        if (!Number.isInteger(cantidad) || cantidad < 1 || cantidad > p.stock) error('Ingresá una cantidad válida dentro del stock disponible.');
        else { p.cantidad = cantidad; error(''); }
        render();
    };
    $('IvaPorcentaje').oninput = render;
    $('ClienteId').onchange = render;
    $('buscar').addEventListener('input', guardarBorrador);
    $('limpiar').onclick = () => { carrito = []; error(''); borrarBorrador(); render(false); };
    form.onsubmit = () => { guardarBorrador(); $('CarritoJson').value = JSON.stringify(carrito); $('ingresar').disabled = true; };
    window.addEventListener('pagehide', () => { if (conservarBorrador) guardarBorrador(); });
    window.addEventListener('pageshow', event => {
        // Al volver con Atrás, cargar el estado vigente evita recuperar una venta ya ingresada o limpiada.
        if (event.persisted) location.reload();
    });
    // El contenedor flex y la grilla ajustan el alto al espacio disponible, incluso al redimensionar.
    render(false); buscar(1);
})();
