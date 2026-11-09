// Utilidades compartidas: sesion en el navegador y llamadas a la API de VetCare.
const Sesion = {
  guardar(datos) { sessionStorage.setItem('vetcare_sesion', JSON.stringify(datos)); },
  obtener() {
    try { return JSON.parse(sessionStorage.getItem('vetcare_sesion')); } catch { return null; }
  },
  cerrar() { sessionStorage.removeItem('vetcare_sesion'); }
};

class ErrorApi extends Error {
  constructor(estado, mensaje, detalles) {
    super(mensaje);
    this.estado = estado;
    this.detalles = detalles;
  }
  // Une el mensaje principal con los detalles de validacion (si existen)
  textoCompleto() {
    if (!this.detalles) return this.message;
    const lineas = Object.values(this.detalles).flat();
    return lineas.length ? lineas.join(' ') : this.message;
  }
}

async function llamarApi(ruta, { metodo = 'GET', cuerpo } = {}) {
  const sesion = Sesion.obtener();
  const cabeceras = { 'Content-Type': 'application/json' };
  if (sesion && sesion.token) cabeceras['Authorization'] = 'Bearer ' + sesion.token;

  let respuesta;
  try {
    respuesta = await fetch('/api' + ruta, {
      method: metodo,
      headers: cabeceras,
      body: cuerpo !== undefined ? JSON.stringify(cuerpo) : undefined
    });
  } catch {
    throw new ErrorApi(0, 'No se pudo conectar con el servidor. Verifique que la API este en ejecucion.');
  }

  let datos = null;
  const texto = await respuesta.text();
  if (texto) {
    try { datos = JSON.parse(texto); } catch { datos = null; }
  }

  if (!respuesta.ok) {
    // Token vencido o invalido: volver al acceso (excepto al intentar iniciar sesion)
    if (respuesta.status === 401 && sesion && !ruta.startsWith('/autenticacion')) {
      Sesion.cerrar();
      location.replace('index.html');
    }
    throw new ErrorApi(
      respuesta.status,
      (datos && datos.mensaje) || 'Ocurrio un error (codigo ' + respuesta.status + ')',
      datos && datos.detalles
    );
  }
  return datos;
}
