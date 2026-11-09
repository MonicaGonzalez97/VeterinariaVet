(() => {
  'use strict';

  if (Sesion.obtener()) {
    location.replace('panel.html');
    return;
  }

  const $ = (selector) => document.querySelector(selector);
  const mensajeError = $('#mensajeError');

  function mostrarError(texto) {
    mensajeError.textContent = texto;
    mensajeError.hidden = !texto;
  }

  // Cambio entre "Iniciar sesion" y "Crear cuenta"
  document.querySelectorAll('.pestana').forEach((pestana) => {
    pestana.addEventListener('click', () => {
      document.querySelectorAll('.pestana').forEach((p) => p.setAttribute('aria-selected', String(p === pestana)));
      ['formLogin', 'formRegistro'].forEach((id) => { $('#' + id).hidden = id !== pestana.dataset.formulario; });
      mostrarError('');
    });
  });

  async function enviar(formulario, ruta, cuerpo) {
    const boton = formulario.querySelector('button[type="submit"]');
    boton.disabled = true;
    mostrarError('');
    try {
      const datos = await llamarApi(ruta, { metodo: 'POST', cuerpo });
      Sesion.guardar(datos);
      location.replace('panel.html');
    } catch (error) {
      mostrarError(error instanceof ErrorApi ? error.textoCompleto() : 'Error inesperado');
      boton.disabled = false;
    }
  }

  $('#formLogin').addEventListener('submit', (evento) => {
    evento.preventDefault();
    enviar(evento.target, '/autenticacion/login', {
      correo: $('#loginCorreo').value.trim(),
      contraseña: $('#loginContraseña').value
    });
  });

  $('#formRegistro').addEventListener('submit', (evento) => {
    evento.preventDefault();
    enviar(evento.target, '/autenticacion/registro', {
      nombre: $('#registroNombre').value.trim(),
      correo: $('#registroCorreo').value.trim(),
      contraseña: $('#registroContraseña').value
    });
  });
})();
