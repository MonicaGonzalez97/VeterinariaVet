// Modulos adicionales del panel: veterinarios, citas, consultas, vacunacion, vacunas, medicamentos, productos y facturas.
// Cada modulo se describe con un objeto de configuracion y una sola logica generica construye su pantalla.
(() => {
  'use strict';

  const sesion = Sesion.obtener();
  if (!sesion) return;

  const rol = sesion.rol;
  const ADMIN = 'Administrador';
  const VETE = 'Veterinario';
  const RECEP = 'Recepcionista';

  const $ = (selector, raiz = document) => raiz.querySelector(selector);
  const esc = (texto) => String(texto ?? '').replace(/[&<>"']/g,
    (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  const puede = (roles) => !roles || roles.includes(rol);
  const textoError = (e) => (e instanceof ErrorApi ? e.textoCompleto() : 'Error inesperado');
  const quetzales = (n) => 'Q ' + Number(n).toFixed(2);
  const fecha = (iso) => (iso ? `${iso.slice(8, 10)}/${iso.slice(5, 7)}/${iso.slice(0, 4)}` : '—');
  const fechaHora = (iso) => (iso ? `${fecha(iso)} ${iso.slice(11, 16)}` : '—');
  const hoy = () => new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 10);
  const insignia = (estado) => `<span class="estado estado--${esc(String(estado).toLowerCase())}">${esc(estado)}</span>`;

  function aviso(texto, error = false) {
    const nodo = document.createElement('div');
    nodo.className = 'aviso' + (error ? ' aviso--error' : '');
    nodo.textContent = texto;
    $('#avisos').appendChild(nodo);
    setTimeout(() => nodo.remove(), 4000);
  }

  // ---------- Origenes de las listas desplegables ----------
  const origenes = {
    mascotas: async () => (await llamarApi('/mascotas')).map((m) => ({ valor: m.id, texto: `${m.nombre} (${m.clienteNombre})` })),
    veterinarios: async () => (await llamarApi('/veterinarios')).map((v) => ({ valor: v.id, texto: v.nombreCompleto })),
    clientes: async () => (await llamarApi('/clientes')).map((c) => ({ valor: c.id, texto: `${c.nombre} ${c.apellido}` })),
    vacunas: async () => (await llamarApi('/vacunas')).map((v) => ({ valor: v.id, texto: v.nombre })),
    medicamentos: async () => (await llamarApi('/medicamentos')).map((m) => ({ valor: m.id, texto: `${m.nombre} (${m.stock} disp.)` })),
    productos: async () => (await llamarApi('/productos')).map((p) => ({ valor: p.id, texto: `${p.nombre} — ${quetzales(p.precio)} (${p.stock} disp.)` })),
    citasProgramadas: async () => (await llamarApi('/citas?estado=Programada'))
      .map((c) => ({ valor: c.id, texto: `${fechaHora(c.fechaHora)} · ${c.mascotaNombre} · ${c.veterinarioNombre}` }))
  };

  async function resolverOpciones(fuente) {
    if (Array.isArray(fuente)) return fuente;
    if (typeof fuente === 'string') return origenes[fuente]();
    return fuente();
  }

  async function llenarSelect(elemento, fuente, textoVacio, valorActual) {
    const opciones = await resolverOpciones(fuente);
    elemento.innerHTML = `<option value="">${esc(textoVacio)}</option>` +
      opciones.map((o) => `<option value="${esc(o.valor)}">${esc(o.texto)}</option>`).join('');
    if (valorActual !== undefined && valorActual !== null) elemento.value = String(valorActual);
  }

  // ---------- Lineas de factura (hasta 3 productos) ----------
  const lineasFactura = [1, 2, 3].flatMap((i) => [
    { id: 'producto' + i, etiqueta: `Producto ${i}`, tipo: 'select', opciones: 'productos', opcional: true, entero: true },
    { id: 'cantidad' + i, etiqueta: `Cantidad ${i}`, tipo: 'number', min: 1, paso: 1, inicial: () => '1', entero: true }
  ]);

  // ---------- Configuracion de los modulos ----------
  const modulos = [
    {
      id: 'veterinarios', pestana: 'Veterinarios', titulo: 'Veterinarios', singular: 'veterinario',
      descripcion: 'Profesionales que atienden en la clínica.', ruta: '/veterinarios',
      columnas: [
        { t: 'Nombre', v: (r) => r.nombreCompleto },
        { t: 'Colegiado', v: (r) => r.numeroColegiado },
        { t: 'Especialidad', v: (r) => r.especialidad || '—' }
      ],
      campos: [
        { id: 'nombre', etiqueta: 'Nombre', requerido: true },
        { id: 'apellido', etiqueta: 'Apellido', requerido: true },
        { id: 'numeroColegiado', etiqueta: 'Número de colegiado', requerido: true },
        { id: 'especialidad', etiqueta: 'Especialidad' }
      ],
      crear: [ADMIN], editar: [ADMIN], eliminar: [ADMIN]
    },
    {
      id: 'citas', pestana: 'Citas', titulo: 'Citas', singular: 'cita',
      descripcion: 'Agenda de la clínica. Un veterinario no puede tener dos citas con menos de 30 minutos de diferencia.',
      ruta: '/citas',
      filtros: [
        { id: 'estado', param: 'estado', etiqueta: 'Todos los estados', opciones: [
          { valor: 'Programada', texto: 'Programadas' }, { valor: 'Completada', texto: 'Completadas' }, { valor: 'Cancelada', texto: 'Canceladas' }] },
        { id: 'veterinario', param: 'veterinarioId', etiqueta: 'Todos los veterinarios', opciones: 'veterinarios' }
      ],
      columnas: [
        { t: 'Fecha y hora', v: (r) => fechaHora(r.fechaHora) },
        { t: 'Mascota', v: (r) => `${r.mascotaNombre} · ${r.clienteNombre}` },
        { t: 'Veterinario', v: (r) => r.veterinarioNombre },
        { t: 'Motivo', v: (r) => r.motivo },
        { t: 'Estado', html: (r) => insignia(r.estado) }
      ],
      campos: [
        { id: 'mascotaId', etiqueta: 'Mascota', tipo: 'select', opciones: 'mascotas', requerido: true, entero: true },
        { id: 'veterinarioId', etiqueta: 'Veterinario', tipo: 'select', opciones: 'veterinarios', requerido: true, entero: true },
        { id: 'fechaHora', etiqueta: 'Fecha y hora', tipo: 'datetime-local', requerido: true, leer: (r) => r.fechaHora.slice(0, 16) },
        { id: 'motivo', etiqueta: 'Motivo', requerido: true }
      ],
      crear: [ADMIN, RECEP], editar: [ADMIN, RECEP], editarVisible: (r) => r.estado === 'Programada',
      acciones: [{
        texto: 'Cancelar', clase: 'boton--peligro', roles: [ADMIN, RECEP], visible: (r) => r.estado === 'Programada',
        confirmar: (r) => `¿Cancelar la cita de ${r.mascotaNombre}?`,
        ejecutar: (r) => llamarApi(`/citas/${r.id}/cancelar`, { metodo: 'POST' }), mensaje: 'Cita cancelada'
      }]
    },
    {
      id: 'consultas', pestana: 'Consultas', titulo: 'Consultas médicas', singular: 'consulta',
      descripcion: 'Historial clínico. Al registrar una consulta se descuenta el medicamento, se actualiza el peso de la mascota y se completa la cita.',
      ruta: '/consultas',
      filtros: [{ id: 'mascota', param: 'mascotaId', etiqueta: 'Todas las mascotas', opciones: 'mascotas' }],
      columnas: [
        { t: 'Fecha', v: (r) => fecha(r.fecha) },
        { t: 'Mascota', v: (r) => r.mascotaNombre },
        { t: 'Veterinario', v: (r) => r.veterinarioNombre },
        { t: 'Diagnóstico', v: (r) => r.diagnosticos.map((d) => d.descripcion + (d.gravedad ? ` (${d.gravedad})` : '')).join('; ') },
        { t: 'Tratamiento', v: (r) => r.tratamientos.map((t) => (t.medicamentoNombre ? t.medicamentoNombre + ': ' : '') + t.indicaciones).join('; ') || '—' },
        { t: 'Peso / Temp.', v: (r) => `${r.pesoKg != null ? r.pesoKg + ' kg' : '—'} / ${r.temperatura != null ? r.temperatura + ' °C' : '—'}` }
      ],
      campos: [
        { id: 'mascotaId', etiqueta: 'Mascota', tipo: 'select', opciones: 'mascotas', requerido: true, entero: true },
        { id: 'veterinarioId', etiqueta: 'Veterinario', tipo: 'select', opciones: 'veterinarios', requerido: true, entero: true },
        { id: 'citaId', etiqueta: 'Cita que se atiende', tipo: 'select', opciones: 'citasProgramadas', opcional: true, entero: true, ancho: 'completo' },
        { id: 'pesoKg', etiqueta: 'Peso (kg)', tipo: 'number', paso: '0.01', min: 0.01, ayuda: 'Actualiza el peso de la mascota' },
        { id: 'temperatura', etiqueta: 'Temperatura (°C)', tipo: 'number', paso: '0.1', min: 30, max: 45 },
        { id: 'observaciones', etiqueta: 'Observaciones', tipo: 'textarea', ancho: 'completo' },
        { id: 'diagnostico', etiqueta: 'Diagnóstico', requerido: true },
        { id: 'gravedad', etiqueta: 'Gravedad', tipo: 'select', opciones: [
          { valor: 'Leve', texto: 'Leve' }, { valor: 'Moderada', texto: 'Moderada' }, { valor: 'Grave', texto: 'Grave' }], opcional: true },
        { id: 'medicamentoId', etiqueta: 'Medicamento (descuenta 1 unidad)', tipo: 'select', opciones: 'medicamentos', opcional: true, entero: true },
        { id: 'dosis', etiqueta: 'Dosis' },
        { id: 'indicaciones', etiqueta: 'Indicaciones del tratamiento', ancho: 'completo' },
        { id: 'duracionDias', etiqueta: 'Duración (días)', tipo: 'number', min: 1, paso: 1, inicial: () => '7', entero: true }
      ],
      armar: (v) => ({
        mascotaId: v.mascotaId, veterinarioId: v.veterinarioId, citaId: v.citaId,
        pesoKg: v.pesoKg, temperatura: v.temperatura, observaciones: v.observaciones,
        diagnosticos: [{ descripcion: v.diagnostico, gravedad: v.gravedad }],
        tratamientos: (v.medicamentoId || v.indicaciones)
          ? [{ medicamentoId: v.medicamentoId, indicaciones: v.indicaciones || 'Según indicación del veterinario', dosis: v.dosis, duracionDias: v.duracionDias || 1 }]
          : []
      }),
      crear: [ADMIN, VETE]
    },
    {
      id: 'vacunacion', pestana: 'Vacunación', titulo: 'Carné de vacunación', singular: 'aplicación de vacuna',
      descripcion: 'Vacunas aplicadas a las mascotas. Si no se indica la próxima dosis, se calcula con el intervalo de la vacuna.',
      ruta: '/vacunaciones',
      filtros: [{ id: 'mascota', param: 'mascotaId', etiqueta: 'Todas las mascotas', opciones: 'mascotas' }],
      columnas: [
        { t: 'Aplicada', v: (r) => fecha(r.fechaAplicacion) },
        { t: 'Mascota', v: (r) => r.mascotaNombre },
        { t: 'Vacuna', v: (r) => r.vacunaNombre },
        { t: 'Próxima dosis', v: (r) => fecha(r.proximaDosis) },
        { t: 'Veterinario', v: (r) => r.veterinarioNombre },
        { t: 'Lote', v: (r) => r.lote || '—' }
      ],
      campos: [
        { id: 'mascotaId', etiqueta: 'Mascota', tipo: 'select', opciones: 'mascotas', requerido: true, entero: true },
        { id: 'vacunaId', etiqueta: 'Vacuna', tipo: 'select', opciones: 'vacunas', requerido: true, entero: true },
        { id: 'veterinarioId', etiqueta: 'Veterinario', tipo: 'select', opciones: 'veterinarios', requerido: true, entero: true },
        { id: 'fechaAplicacion', etiqueta: 'Fecha de aplicación', tipo: 'date', requerido: true, inicial: hoy },
        { id: 'proximaDosis', etiqueta: 'Próxima dosis (opcional)', tipo: 'date' },
        { id: 'lote', etiqueta: 'Lote' }
      ],
      crear: [ADMIN, VETE], eliminar: [ADMIN]
    },
    {
      id: 'vacunas', pestana: 'Vacunas', titulo: 'Catálogo de vacunas', singular: 'vacuna',
      descripcion: 'Vacunas disponibles y cada cuántos días se repite la dosis.', ruta: '/vacunas',
      columnas: [
        { t: 'Vacuna', v: (r) => r.nombre },
        { t: 'Intervalo entre dosis', v: (r) => (r.intervaloDias > 0 ? `${r.intervaloDias} días` : 'Dosis única') }
      ],
      campos: [
        { id: 'nombre', etiqueta: 'Nombre', requerido: true },
        { id: 'intervaloDias', etiqueta: 'Intervalo (días)', tipo: 'number', min: 0, paso: 1, requerido: true, entero: true, ayuda: '0 = sin próxima dosis' }
      ],
      crear: [ADMIN, VETE], editar: [ADMIN, VETE], eliminar: [ADMIN]
    },
    {
      id: 'medicamentos', pestana: 'Medicamentos', titulo: 'Medicamentos', singular: 'medicamento',
      descripcion: 'Inventario de medicamentos para tratamientos.', ruta: '/medicamentos', buscador: true,
      columnas: [
        { t: 'Medicamento', v: (r) => r.nombre },
        { t: 'Presentación', v: (r) => r.presentacion || '—' },
        { t: 'Existencias', html: (r) => `<strong class="${r.stock <= 5 ? 'stock-bajo' : ''}">${esc(r.stock)}</strong>` }
      ],
      campos: [
        { id: 'nombre', etiqueta: 'Nombre', requerido: true },
        { id: 'presentacion', etiqueta: 'Presentación' },
        { id: 'stock', etiqueta: 'Existencias', tipo: 'number', min: 0, paso: 1, requerido: true, entero: true }
      ],
      crear: [ADMIN, VETE], editar: [ADMIN, VETE], eliminar: [ADMIN]
    },
    {
      id: 'productos', pestana: 'Productos', titulo: 'Productos', singular: 'producto',
      descripcion: 'Productos a la venta: alimentos, higiene y accesorios.', ruta: '/productos', buscador: true,
      columnas: [
        { t: 'Producto', v: (r) => r.nombre },
        { t: 'Categoría', v: (r) => r.categoria || '—' },
        { t: 'Precio', v: (r) => quetzales(r.precio) },
        { t: 'Existencias', html: (r) => `<strong class="${r.stock <= 5 ? 'stock-bajo' : ''}">${esc(r.stock)}</strong>` }
      ],
      campos: [
        { id: 'nombre', etiqueta: 'Nombre', requerido: true, ancho: 'completo' },
        { id: 'categoria', etiqueta: 'Categoría' },
        { id: 'precio', etiqueta: 'Precio (Q)', tipo: 'number', paso: '0.01', min: 0.01, requerido: true },
        { id: 'stock', etiqueta: 'Existencias', tipo: 'number', min: 0, paso: 1, requerido: true, entero: true }
      ],
      crear: [ADMIN, RECEP], editar: [ADMIN, RECEP], eliminar: [ADMIN]
    },
    {
      id: 'facturas', pestana: 'Facturas', titulo: 'Facturación', singular: 'factura',
      descripcion: 'Facturas a clientes. Los productos vendidos se descuentan del inventario y se devuelven al anular.',
      ruta: '/facturas',
      filtros: [
        { id: 'cliente', param: 'clienteId', etiqueta: 'Todos los clientes', opciones: 'clientes' },
        { id: 'estado', param: 'estado', etiqueta: 'Todos los estados', opciones: [
          { valor: 'Pendiente', texto: 'Pendientes' }, { valor: 'Pagada', texto: 'Pagadas' }, { valor: 'Anulada', texto: 'Anuladas' }] }
      ],
      columnas: [
        { t: 'No.', v: (r) => r.id },
        { t: 'Fecha', v: (r) => fecha(r.fecha) },
        { t: 'Cliente', v: (r) => r.clienteNombre },
        { t: 'Total', v: (r) => quetzales(r.total) },
        { t: 'Estado', html: (r) => insignia(r.estado) }
      ],
      campos: [
        { id: 'clienteId', etiqueta: 'Cliente', tipo: 'select', opciones: 'clientes', requerido: true, entero: true, ancho: 'completo' },
        ...lineasFactura,
        { id: 'servicioDescripcion', etiqueta: 'Servicio (opcional)', ayuda: 'Por ejemplo: Consulta veterinaria' },
        { id: 'servicioPrecio', etiqueta: 'Precio del servicio (Q)', tipo: 'number', paso: '0.01', min: 0.01 }
      ],
      armar: (v) => {
        const detalles = [];
        [1, 2, 3].forEach((i) => {
          if (v['producto' + i]) detalles.push({ productoId: v['producto' + i], cantidad: v['cantidad' + i] || 1 });
        });
        if (v.servicioDescripcion || v.servicioPrecio) {
          detalles.push({ descripcion: v.servicioDescripcion, cantidad: 1, precioUnitario: v.servicioPrecio });
        }
        return { clienteId: v.clienteId, detalles };
      },
      crear: [ADMIN, RECEP],
      acciones: [
        {
          texto: 'Detalle', roles: null,
          ver: (r) => ({
            titulo: `Factura No. ${r.id} · ${r.clienteNombre}`,
            html: `<table><thead><tr><th>Descripción</th><th>Cant.</th><th>Precio</th><th>Subtotal</th></tr></thead><tbody>` +
              r.detalles.map((d) => `<tr><td>${esc(d.descripcion)}</td><td>${esc(d.cantidad)}</td><td>${quetzales(d.precioUnitario)}</td><td>${quetzales(d.subtotal)}</td></tr>`).join('') +
              `</tbody></table><p><strong>Total: ${quetzales(r.total)}</strong> · ${esc(r.estado)}</p>`
          })
        },
        {
          texto: 'Pagar', clase: 'boton--naranja', roles: [ADMIN, RECEP], visible: (r) => r.estado === 'Pendiente',
          ejecutar: (r) => llamarApi(`/facturas/${r.id}/pagar`, { metodo: 'POST' }), mensaje: 'Factura pagada'
        },
        {
          texto: 'Anular', clase: 'boton--peligro', roles: [ADMIN], visible: (r) => r.estado !== 'Anulada',
          confirmar: (r) => `¿Anular la factura No. ${r.id}? Los productos volverán al inventario.`,
          ejecutar: (r) => llamarApi(`/facturas/${r.id}/anular`, { metodo: 'POST' }), mensaje: 'Factura anulada'
        }
      ]
    }
  ];

  const datos = {};
  const secciones = {};
  const dialogos = {};
  const iniciados = new Set();

  // ---------- Construccion de la pantalla ----------
  function htmlCampo(m, c) {
    const id = `${m.id}_${c.id}`;
    const clase = 'campo' + (c.ancho === 'completo' ? ' campo--completo' : '');
    let control;
    if (c.tipo === 'select') {
      control = `<select id="${id}"></select>`;
    } else if (c.tipo === 'textarea') {
      control = `<textarea id="${id}" rows="3" maxlength="1000"></textarea>`;
    } else {
      control = `<input id="${id}" type="${c.tipo || 'text'}"` +
        (c.paso ? ` step="${c.paso}"` : '') + (c.min != null ? ` min="${c.min}"` : '') + (c.max != null ? ` max="${c.max}"` : '') + '>';
    }
    return `<div class="${clase}"><label for="${id}">${esc(c.etiqueta)}${c.requerido ? ' *' : ''}</label>${control}` +
      (c.ayuda ? `<small>${esc(c.ayuda)}</small>` : '') + '</div>';
  }

  function crearSeccion(m) {
    const filtros = (m.filtros || []).map((f) => `<select id="f-${m.id}-${f.id}" aria-label="${esc(f.etiqueta)}"></select>`).join('');
    const buscador = m.buscador ? `<input id="b-${m.id}" type="search" placeholder="Buscar por nombre" aria-label="Buscar ${esc(m.singular)}">` : '';
    const nuevo = m.crear && puede(m.crear) ? `<button class="boton boton--naranja" type="button" data-nuevo>Nuevo ${esc(m.singular)}</button>` : '';

    const seccion = document.createElement('section');
    seccion.id = 'vista-' + m.id;
    seccion.hidden = true;
    seccion.innerHTML =
      `<div class="encabezado-vista"><div><h2>${esc(m.titulo)}</h2><p>${esc(m.descripcion)}</p></div>` +
      `<div class="herramientas">${filtros}${buscador}${nuevo}</div></div>` +
      `<div class="tabla-envoltura"><table><thead><tr>${m.columnas.map((c) => `<th>${esc(c.t)}</th>`).join('')}<th></th></tr></thead><tbody></tbody></table></div>` +
      `<div class="vacio" hidden><svg class="icono"><use href="#huella"/></svg><p>No hay registros para mostrar.</p></div>`;
    $('main.contenido').appendChild(seccion);
    secciones[m.id] = seccion;

    const boton = $('[data-nuevo]', seccion);
    if (boton) boton.addEventListener('click', () => abrirFormulario(m, null));

    (m.filtros || []).forEach((f) => $(`#f-${m.id}-${f.id}`, seccion).addEventListener('change', () => cargar(m)));
    if (m.buscador) {
      let temporizador;
      $(`#b-${m.id}`, seccion).addEventListener('input', () => { clearTimeout(temporizador); temporizador = setTimeout(() => cargar(m), 300); });
    }

    $('tbody', seccion).addEventListener('click', (evento) => manejarAccion(m, evento));
  }

  function crearDialogo(m) {
    if (!m.campos) return;
    const dialogo = document.createElement('dialog');
    dialogo.innerHTML =
      '<form novalidate><div class="dialogo__cabecera"><h3 data-titulo></h3></div>' +
      '<div class="dialogo__cuerpo"><p class="mensaje-error" data-error role="alert" hidden></p>' +
      `<div class="rejilla-2">${m.campos.map((c) => htmlCampo(m, c)).join('')}</div></div>` +
      '<div class="dialogo__pie"><button class="boton boton--suave" type="button" data-cerrar>Cancelar</button>' +
      '<button class="boton" type="submit">Guardar</button></div></form>';
    document.body.appendChild(dialogo);
    dialogos[m.id] = dialogo;

    $('[data-cerrar]', dialogo).addEventListener('click', () => dialogo.close());
    $('form', dialogo).addEventListener('submit', (evento) => { evento.preventDefault(); guardar(m); });
  }

  // Dialogo de solo lectura (detalle de factura)
  const dialogoInfo = document.createElement('dialog');
  dialogoInfo.innerHTML =
    '<div class="dialogo__cabecera"><h3 data-titulo></h3></div><div class="dialogo__cuerpo" data-cuerpo></div>' +
    '<div class="dialogo__pie"><button class="boton" type="button" data-cerrar>Cerrar</button></div>';
  document.body.appendChild(dialogoInfo);
  $('[data-cerrar]', dialogoInfo).addEventListener('click', () => dialogoInfo.close());

  // ---------- Formularios ----------
  async function abrirFormulario(m, registro) {
    const dialogo = dialogos[m.id];
    const error = $('[data-error]', dialogo);
    error.hidden = true;
    $('[data-titulo]', dialogo).textContent = (registro ? 'Editar ' : 'Nueva ') + m.singular;
    dialogo.dataset.id = registro ? registro.id : '';

    try {
      for (const c of m.campos) {
        const elemento = $(`#${m.id}_${c.id}`, dialogo);
        const valor = registro
          ? (c.leer ? c.leer(registro) : registro[c.id])
          : (c.inicial ? c.inicial() : '');
        if (c.tipo === 'select') {
          await llenarSelect(elemento, c.opciones, c.opcional ? 'Ninguno' : 'Seleccione…', valor);
        } else {
          elemento.value = valor ?? '';
        }
      }
    } catch (e) {
      aviso(textoError(e), true);
      return;
    }
    dialogo.showModal();
  }

  function leerValor(m, c, dialogo) {
    const crudo = $(`#${m.id}_${c.id}`, dialogo).value;
    if (crudo === '' || crudo == null) return null;
    if (c.tipo === 'number' || c.entero) return Number(crudo);
    return crudo.trim() === '' ? null : crudo.trim();
  }

  async function guardar(m) {
    const dialogo = dialogos[m.id];
    const error = $('[data-error]', dialogo);
    const valores = {};

    for (const c of m.campos) {
      valores[c.id] = leerValor(m, c, dialogo);
      if (c.requerido && valores[c.id] === null) {
        error.textContent = `El campo "${c.etiqueta}" es obligatorio.`;
        error.hidden = false;
        return;
      }
    }

    const cuerpo = m.armar ? m.armar(valores) : valores;
    const id = dialogo.dataset.id;
    try {
      await llamarApi(id ? `${m.ruta}/${id}` : m.ruta, { metodo: id ? 'PUT' : 'POST', cuerpo });
      dialogo.close();
      aviso(id ? 'Cambios guardados' : 'Registro creado');
      await cargar(m);
    } catch (e) {
      error.textContent = textoError(e);
      error.hidden = false;
    }
  }

  // ---------- Tablas ----------
  function botonesFila(m, r) {
    let html = '';
    (m.acciones || []).forEach((a, i) => {
      if (puede(a.roles) && (!a.visible || a.visible(r))) {
        html += `<button class="boton ${a.clase || 'boton--suave'} boton--chico" data-accion="a${i}" data-id="${r.id}">${esc(a.texto)}</button>`;
      }
    });
    if (m.editar && puede(m.editar) && (!m.editarVisible || m.editarVisible(r))) {
      html += `<button class="boton boton--suave boton--chico" data-accion="editar" data-id="${r.id}">Editar</button>`;
    }
    if (m.eliminar && puede(m.eliminar)) {
      html += `<button class="boton boton--peligro boton--chico" data-accion="eliminar" data-id="${r.id}">Eliminar</button>`;
    }
    return html;
  }

  function dibujar(m) {
    const seccion = secciones[m.id];
    const filas = datos[m.id] || [];
    $('tbody', seccion).innerHTML = filas.map((r) =>
      '<tr>' + m.columnas.map((c) => `<td>${c.html ? c.html(r) : esc(c.v(r))}</td>`).join('') +
      `<td class="acciones">${botonesFila(m, r)}</td></tr>`).join('');
    $('.vacio', seccion).hidden = filas.length > 0;
  }

  async function cargar(m) {
    const parametros = new URLSearchParams();
    (m.filtros || []).forEach((f) => {
      const valor = $(`#f-${m.id}-${f.id}`, secciones[m.id]).value;
      if (valor) parametros.append(f.param, valor);
    });
    if (m.buscador) {
      const texto = $(`#b-${m.id}`, secciones[m.id]).value.trim();
      if (texto) parametros.append('busqueda', texto);
    }
    const consulta = parametros.toString();

    try {
      datos[m.id] = await llamarApi(m.ruta + (consulta ? '?' + consulta : ''));
      dibujar(m);
    } catch (e) {
      aviso(textoError(e), true);
    }
  }

  async function manejarAccion(m, evento) {
    const boton = evento.target.closest('button[data-accion]');
    if (!boton) return;
    const registro = (datos[m.id] || []).find((r) => r.id === Number(boton.dataset.id));
    if (!registro) return;
    const accion = boton.dataset.accion;

    if (accion === 'editar') return abrirFormulario(m, registro);

    if (accion === 'eliminar') {
      if (!confirm(`¿Eliminar este registro de ${m.titulo.toLowerCase()}?`)) return;
      try {
        await llamarApi(`${m.ruta}/${registro.id}`, { metodo: 'DELETE' });
        aviso('Registro eliminado');
        await cargar(m);
      } catch (e) { aviso(textoError(e), true); }
      return;
    }

    const definicion = m.acciones[Number(accion.slice(1))];
    if (definicion.ver) {
      const info = definicion.ver(registro);
      $('[data-titulo]', dialogoInfo).textContent = info.titulo;
      $('[data-cuerpo]', dialogoInfo).innerHTML = info.html;
      dialogoInfo.showModal();
      return;
    }
    if (definicion.confirmar && !confirm(definicion.confirmar(registro))) return;
    try {
      await definicion.ejecutar(registro);
      aviso(definicion.mensaje || 'Listo');
      await cargar(m);
    } catch (e) { aviso(textoError(e), true); }
  }

  async function iniciar(m) {
    if (!iniciados.has(m.id)) {
      try {
        for (const f of (m.filtros || [])) {
          await llenarSelect($(`#f-${m.id}-${f.id}`, secciones[m.id]), f.opciones, f.etiqueta, '');
        }
        iniciados.add(m.id);
      } catch (e) {
        aviso(textoError(e), true);
      }
    }
    await cargar(m);
  }

  // ---------- Navegacion ----------
  const navegacion = $('nav.pestanas');
  const pestanasOriginales = Array.from(document.querySelectorAll('.pestana'));

  function mostrar(nombre) {
    document.querySelectorAll('.pestana').forEach((p) => p.setAttribute('aria-selected', String(p.dataset.vista === nombre)));
    ['clientes', 'mascotas', 'memorial'].forEach((v) => { $('#vista-' + v).hidden = v !== nombre; });
    modulos.forEach((m) => { secciones[m.id].hidden = m.id !== nombre; });
    const modulo = modulos.find((m) => m.id === nombre);
    if (modulo) iniciar(modulo);
  }

  // Al volver a las pestanas originales (que maneja panel.js) se ocultan las pantallas de este archivo
  pestanasOriginales.forEach((p) => p.addEventListener('click', () => {
    modulos.forEach((m) => { secciones[m.id].hidden = true; });
  }));

  modulos.forEach((m) => {
    crearSeccion(m);
    crearDialogo(m);

    const boton = document.createElement('button');
    boton.className = 'pestana';
    boton.setAttribute('role', 'tab');
    boton.setAttribute('aria-selected', 'false');
    boton.dataset.vista = m.id;
    boton.textContent = m.pestana;
    boton.addEventListener('click', () => mostrar(m.id));
    navegacion.appendChild(boton);
  });
})();
