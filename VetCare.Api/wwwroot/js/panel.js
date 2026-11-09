(() => {
  'use strict';

  const sesion = Sesion.obtener();
  if (!sesion) {
    location.replace('index.html');
    return;
  }

  // ---------- Utilidades ----------
  const $ = (selector, raiz = document) => raiz.querySelector(selector);
  const esc = (texto) => String(texto ?? '').replace(/[&<>"']/g,
    (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

  const EMOJIS = { Perro: '🐶', Gato: '🐱', Ave: '🐦', Conejo: '🐰' };
  const emojiDe = (especie) => EMOJIS[especie] || '🐾';

  function fechaLegible(iso) {
    if (!iso) return 'Sin dato';
    const [a, m, d] = iso.split('-');
    return `${d}/${m}/${a}`;
  }

  function hoyLocal() {
    return new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0, 10);
  }

  // Edad entre dos fechas ISO (si no hay "hasta", usa hoy)
  function edad(desdeIso, hastaIso) {
    if (!desdeIso) return 'Edad no registrada';
    const [a, m, d] = desdeIso.split('-').map(Number);
    const fin = hastaIso ? new Date(hastaIso + 'T00:00:00') : new Date();
    let anios = fin.getFullYear() - a;
    let meses = fin.getMonth() + 1 - m;
    if (fin.getDate() < d) meses--;
    if (meses < 0) { anios--; meses += 12; }
    const tAnios = `${anios} ${anios === 1 ? 'año' : 'años'}`;
    const tMeses = `${meses} ${meses === 1 ? 'mes' : 'meses'}`;
    if (anios <= 0) return tMeses;
    return meses ? `${tAnios} y ${tMeses}` : tAnios;
  }

  function aviso(texto, error = false) {
    const nodo = document.createElement('div');
    nodo.className = 'aviso' + (error ? ' aviso--error' : '');
    nodo.textContent = texto;
    $('#avisos').appendChild(nodo);
    setTimeout(() => nodo.remove(), 4000);
  }

  const textoError = (e) => (e instanceof ErrorApi ? e.textoCompleto() : 'Error inesperado');

  // ---------- Permisos segun el rol (el servidor tambien los valida) ----------
  const rol = sesion.rol;
  const puedeEscribir = ['Administrador', 'Recepcionista'].includes(rol);
  const puedeEliminar = rol === 'Administrador';
  const puedeRegistrarFallecimiento = ['Administrador', 'Veterinario'].includes(rol);

  $('#nombreUsuario').textContent = sesion.nombre;
  $('#rolUsuario').textContent = rol;
  document.querySelectorAll('[data-escritura]').forEach((n) => { n.hidden = !puedeEscribir; });
  $('#botonSalir').addEventListener('click', () => { Sesion.cerrar(); location.replace('index.html'); });

  // ---------- Estado ----------
  const estado = { clientes: [], todosClientes: [], mascotas: [], memorial: [], especies: [], razas: [] };

  // ---------- Navegacion entre vistas ----------
  function mostrarVista(nombre) {
    document.querySelectorAll('.pestana').forEach((p) => p.setAttribute('aria-selected', String(p.dataset.vista === nombre)));
    ['clientes', 'mascotas', 'memorial'].forEach((v) => { $('#vista-' + v).hidden = v !== nombre; });
    if (nombre === 'clientes') cargarClientes();
    if (nombre === 'mascotas') cargarMascotas();
    if (nombre === 'memorial') cargarMemorial();
  }
  document.querySelectorAll('.pestana').forEach((p) => p.addEventListener('click', () => mostrarVista(p.dataset.vista)));

  // ---------- Dialogos ----------
  document.querySelectorAll('dialog [data-cerrar]').forEach((b) => b.addEventListener('click', () => b.closest('dialog').close()));

  function mostrarErrorForm(id, texto) {
    const nodo = $('#' + id);
    nodo.textContent = texto;
    nodo.hidden = !texto;
  }

  // ---------- Clientes ----------
  async function cargarClientes() {
    try {
      const busqueda = $('#buscarCliente').value.trim();
      estado.clientes = await llamarApi('/clientes' + (busqueda ? '?busqueda=' + encodeURIComponent(busqueda) : ''));
      renderClientes();
    } catch (e) { aviso(textoError(e), true); }
  }

  function renderClientes() {
    $('#cuerpoClientes').innerHTML = estado.clientes.map((c) => `
      <tr>
        <td><strong>${esc(c.nombre)} ${esc(c.apellido)}</strong></td>
        <td>${esc(c.documento)}</td>
        <td>${esc(c.telefono)}</td>
        <td>${esc(c.correo || '—')}</td>
        <td class="acciones">
          ${puedeEscribir ? `<button class="boton boton--suave boton--chico" data-accion="editar-cliente" data-id="${c.id}">Editar</button>` : ''}
          ${puedeEliminar ? `<button class="boton boton--peligro boton--chico" data-accion="eliminar-cliente" data-id="${c.id}">Eliminar</button>` : ''}
        </td>
      </tr>`).join('');
    $('#vacioClientes').hidden = estado.clientes.length > 0;
  }

  function abrirDialogoCliente(cliente) {
    mostrarErrorForm('errorCliente', '');
    $('#tituloCliente').textContent = cliente ? 'Editar cliente' : 'Nuevo cliente';
    $('#clienteId').value = cliente ? cliente.id : '';
    $('#clienteNombre').value = cliente ? cliente.nombre : '';
    $('#clienteApellido').value = cliente ? cliente.apellido : '';
    $('#clienteDocumento').value = cliente ? cliente.documento : '';
    $('#clienteTelefono').value = cliente ? cliente.telefono : '';
    $('#clienteCorreo').value = cliente ? (cliente.correo || '') : '';
    $('#clienteDireccion').value = cliente ? (cliente.direccion || '') : '';
    $('#dialogoCliente').showModal();
  }

  $('#formCliente').addEventListener('submit', async (evento) => {
    evento.preventDefault();
    const id = $('#clienteId').value;
    const cuerpo = {
      nombre: $('#clienteNombre').value.trim(),
      apellido: $('#clienteApellido').value.trim(),
      documento: $('#clienteDocumento').value.trim(),
      telefono: $('#clienteTelefono').value.trim(),
      correo: $('#clienteCorreo').value.trim() || null,
      direccion: $('#clienteDireccion').value.trim() || null
    };
    try {
      await llamarApi(id ? '/clientes/' + id : '/clientes', { metodo: id ? 'PUT' : 'POST', cuerpo });
      $('#dialogoCliente').close();
      aviso(id ? 'Cliente actualizado' : 'Cliente creado');
      await cargarClientes();
      await cargarTodosClientes();
    } catch (e) { mostrarErrorForm('errorCliente', textoError(e)); }
  });

  $('#botonNuevoCliente').addEventListener('click', () => abrirDialogoCliente(null));
  let temporizador;
  $('#buscarCliente').addEventListener('input', () => { clearTimeout(temporizador); temporizador = setTimeout(cargarClientes, 300); });

  $('#cuerpoClientes').addEventListener('click', async (evento) => {
    const boton = evento.target.closest('button[data-accion]');
    if (!boton) return;
    const id = Number(boton.dataset.id);
    const cliente = estado.clientes.find((c) => c.id === id);
    if (boton.dataset.accion === 'editar-cliente') abrirDialogoCliente(cliente);
    if (boton.dataset.accion === 'eliminar-cliente' && confirm(`¿Eliminar a ${cliente.nombre} ${cliente.apellido}?`)) {
      try {
        await llamarApi('/clientes/' + id, { metodo: 'DELETE' });
        aviso('Cliente eliminado');
        await cargarClientes();
        await cargarTodosClientes();
      } catch (e) { aviso(textoError(e), true); }
    }
  });

  // ---------- Catalogos ----------
  async function cargarTodosClientes() {
    estado.todosClientes = await llamarApi('/clientes');
    const opciones = estado.todosClientes.map((c) => `<option value="${c.id}">${esc(c.nombre)} ${esc(c.apellido)}</option>`).join('');
    const filtro = $('#filtroCliente');
    const seleccionado = filtro.value;
    filtro.innerHTML = '<option value="">Todos los clientes</option>' + opciones;
    filtro.value = seleccionado;
    $('#mascotaCliente').innerHTML = opciones;
  }

  async function cargarCatalogos() {
    estado.especies = await llamarApi('/catalogos/especies');
    estado.razas = await llamarApi('/catalogos/razas');
    $('#mascotaEspecie').innerHTML = estado.especies.map((e) => `<option value="${e.id}">${esc(e.nombre)}</option>`).join('');
    llenarRazas();
  }

  function llenarRazas(razaIdSeleccionada) {
    const especieId = Number($('#mascotaEspecie').value);
    $('#mascotaRaza').innerHTML = estado.razas
      .filter((r) => r.especieId === especieId)
      .map((r) => `<option value="${r.id}">${esc(r.nombre)}</option>`).join('');
    if (razaIdSeleccionada) $('#mascotaRaza').value = razaIdSeleccionada;
  }
  $('#mascotaEspecie').addEventListener('change', () => llenarRazas());

  // ---------- Mascotas ----------
  async function cargarMascotas() {
    try {
      const filtro = $('#filtroCliente').value;
      estado.mascotas = await llamarApi('/mascotas' + (filtro ? '?clienteId=' + filtro : ''));
      renderMascotas();
    } catch (e) { aviso(textoError(e), true); }
  }

  function renderMascotas() {
    $('#cuadriculaMascotas').innerHTML = estado.mascotas.map((m) => `
      <article class="mascota">
        <div class="mascota__cabecera">
          <div class="mascota__avatar" aria-hidden="true">${emojiDe(m.especieNombre)}</div>
          <div><h3>${esc(m.nombre)}</h3><div class="raza">${esc(m.especieNombre)} · ${esc(m.razaNombre)}</div></div>
        </div>
        <dl>
          <dt>Dueño</dt><dd>${esc(m.clienteNombre)}</dd>
          <dt>Edad</dt><dd>${esc(edad(m.fechaNacimiento))}</dd>
          <dt>Sexo</dt><dd>${esc(m.sexo)}</dd>
          <dt>Peso</dt><dd>${m.pesoKg != null ? esc(m.pesoKg) + ' kg' : 'Sin dato'}</dd>
          <dt>Color</dt><dd>${esc(m.color || 'Sin dato')}</dd>
        </dl>
        <div class="mascota__acciones">
          ${puedeEscribir ? `<button class="boton boton--suave boton--chico" data-accion="editar-mascota" data-id="${m.id}">Editar</button>` : ''}
          ${puedeRegistrarFallecimiento ? `<button class="boton boton--suave boton--chico" data-accion="fallecimiento" data-id="${m.id}">Registrar fallecimiento</button>` : ''}
          ${puedeEliminar ? `<button class="boton boton--peligro boton--chico" data-accion="eliminar-mascota" data-id="${m.id}">Eliminar</button>` : ''}
        </div>
      </article>`).join('');
    $('#vacioMascotas').hidden = estado.mascotas.length > 0;
  }

  function abrirDialogoMascota(mascota) {
    if (!estado.todosClientes.length) {
      aviso('Primero registre un cliente para poder agregar mascotas', true);
      return;
    }
    mostrarErrorForm('errorMascota', '');
    $('#tituloMascota').textContent = mascota ? 'Editar mascota' : 'Nueva mascota';
    $('#mascotaId').value = mascota ? mascota.id : '';
    $('#mascotaNombre').value = mascota ? mascota.nombre : '';
    $('#mascotaCliente').value = mascota ? mascota.clienteId : (estado.todosClientes[0]?.id ?? '');
    if (mascota) {
      const raza = estado.razas.find((r) => r.id === mascota.razaId);
      $('#mascotaEspecie').value = raza ? raza.especieId : $('#mascotaEspecie').value;
      llenarRazas(mascota.razaId);
    } else {
      llenarRazas();
    }
    $('#mascotaSexo').value = mascota ? mascota.sexo : 'Macho';
    $('#mascotaNacimiento').value = mascota?.fechaNacimiento || '';
    $('#mascotaNacimiento').max = hoyLocal();
    $('#mascotaPeso').value = mascota?.pesoKg ?? '';
    $('#mascotaColor').value = mascota?.color || '';
    $('#dialogoMascota').showModal();
  }

  $('#formMascota').addEventListener('submit', async (evento) => {
    evento.preventDefault();
    const id = $('#mascotaId').value;
    const peso = $('#mascotaPeso').value;
    const cuerpo = {
      nombre: $('#mascotaNombre').value.trim(),
      clienteId: Number($('#mascotaCliente').value),
      razaId: Number($('#mascotaRaza').value),
      fechaNacimiento: $('#mascotaNacimiento').value || null,
      sexo: $('#mascotaSexo').value,
      pesoKg: peso === '' ? null : Number(peso),
      color: $('#mascotaColor').value.trim() || null
    };
    try {
      await llamarApi(id ? '/mascotas/' + id : '/mascotas', { metodo: id ? 'PUT' : 'POST', cuerpo });
      $('#dialogoMascota').close();
      aviso(id ? 'Mascota actualizada' : 'Mascota registrada');
      await cargarMascotas();
    } catch (e) { mostrarErrorForm('errorMascota', textoError(e)); }
  });

  $('#botonNuevaMascota').addEventListener('click', () => abrirDialogoMascota(null));
  $('#filtroCliente').addEventListener('change', cargarMascotas);

  $('#cuadriculaMascotas').addEventListener('click', async (evento) => {
    const boton = evento.target.closest('button[data-accion]');
    if (!boton) return;
    const id = Number(boton.dataset.id);
    const mascota = estado.mascotas.find((m) => m.id === id);
    if (boton.dataset.accion === 'editar-mascota') abrirDialogoMascota(mascota);
    if (boton.dataset.accion === 'fallecimiento') abrirDialogoFallecimiento(mascota);
    if (boton.dataset.accion === 'eliminar-mascota' && confirm(`¿Eliminar a ${mascota.nombre}?`)) {
      try {
        await llamarApi('/mascotas/' + id, { metodo: 'DELETE' });
        aviso('Mascota eliminada');
        await cargarMascotas();
      } catch (e) { aviso(textoError(e), true); }
    }
  });

  // ---------- Fallecimiento y memorial ----------
  function abrirDialogoFallecimiento(mascota) {
    mostrarErrorForm('errorFallecimiento', '');
    $('#tituloFallecimiento').textContent = 'Registrar fallecimiento de ' + mascota.nombre;
    $('#fallecimientoId').value = mascota.id;
    $('#fallecimientoFecha').value = hoyLocal();
    $('#fallecimientoFecha').max = hoyLocal();
    $('#fallecimientoCausa').value = '';
    $('#fallecimientoObservaciones').value = '';
    $('#dialogoFallecimiento').showModal();
  }

  $('#formFallecimiento').addEventListener('submit', async (evento) => {
    evento.preventDefault();
    const id = $('#fallecimientoId').value;
    const cuerpo = {
      fechaFallecimiento: $('#fallecimientoFecha').value || null,
      causa: $('#fallecimientoCausa').value.trim() || null,
      observaciones: $('#fallecimientoObservaciones').value.trim() || null
    };
    try {
      await llamarApi(`/mascotas/${id}/fallecimiento`, { metodo: 'POST', cuerpo });
      $('#dialogoFallecimiento').close();
      aviso('Fallecimiento registrado. La mascota paso al Memorial');
      await cargarMascotas();
    } catch (e) { mostrarErrorForm('errorFallecimiento', textoError(e)); }
  });

  async function cargarMemorial() {
    try {
      estado.memorial = await llamarApi('/mascotas?fallecidas=true');
      renderMemorial();
    } catch (e) { aviso(textoError(e), true); }
  }

  function renderMemorial() {
    $('#cuadriculaMemorial').innerHTML = estado.memorial.map((m) => {
      const f = m.fallecimiento;
      return `
      <article class="mascota mascota--memorial">
        <div class="mascota__cabecera">
          <div class="mascota__avatar" aria-hidden="true">${emojiDe(m.especieNombre)}</div>
          <div><h3>${esc(m.nombre)}</h3><div class="raza">${esc(m.especieNombre)} · ${esc(m.razaNombre)}</div></div>
        </div>
        <div class="recuerdo">
          <strong>Fallecio el ${esc(fechaLegible(f?.fechaFallecimiento))}</strong>
          ${m.fechaNacimiento ? `Vivio ${esc(edad(m.fechaNacimiento, f?.fechaFallecimiento))}<br>` : ''}
          ${f?.causa ? `Causa: ${esc(f.causa)}<br>` : ''}
          ${f?.observaciones ? esc(f.observaciones) : ''}
        </div>
        <dl><dt>Dueño</dt><dd>${esc(m.clienteNombre)}</dd></dl>
        ${puedeEliminar ? `<div class="mascota__acciones"><button class="boton boton--suave boton--chico" data-accion="revertir" data-id="${m.id}">Revertir registro</button></div>` : ''}
      </article>`;
    }).join('');
    $('#vacioMemorial').hidden = estado.memorial.length > 0;
  }

  $('#cuadriculaMemorial').addEventListener('click', async (evento) => {
    const boton = evento.target.closest('button[data-accion="revertir"]');
    if (!boton) return;
    const mascota = estado.memorial.find((m) => m.id === Number(boton.dataset.id));
    if (!confirm(`¿Revertir el fallecimiento de ${mascota.nombre}? Use esta opcion solo si se registro por error.`)) return;
    try {
      await llamarApi(`/mascotas/${mascota.id}/fallecimiento`, { metodo: 'DELETE' });
      aviso('Registro revertido. La mascota volvio a la lista activa');
      await cargarMemorial();
    } catch (e) { aviso(textoError(e), true); }
  });

  // ---------- Inicio ----------
  (async () => {
    try {
      await Promise.all([cargarTodosClientes(), cargarCatalogos()]);
    } catch (e) { aviso(textoError(e), true); }
    mostrarVista('clientes');
  })();
})();
