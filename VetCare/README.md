# VetCare · API de gestión veterinaria

Proyecto final de **Arquitectura de Sistemas** · Universidad Mesoamericana Quetzaltenango (UMES).

VetCare ("Vet" de veterinaria + "Care", cuidado) es una API pública protegida con JWT, con arquitectura en capas, Entity Framework Core sobre PostgreSQL, documentación Swagger, pruebas unitarias con mocks y un frontend web funcional.

## Requisitos

- .NET SDK 10 (probado con 10.0.300)
- PostgreSQL 18
- Visual Studio 2026 (o `dotnet` en la terminal)

## Cómo levantar el proyecto

1. Abra `VetCare.sln` con Visual Studio 2026 (o siga la sección *Sin Visual Studio* más abajo).
2. Abra `VetCare.Api/appsettings.Development.json` y escriba la contraseña de su usuario de PostgreSQL en `Password=`. El usuario configurado es `sharoncanellon`; cámbielo si usa otro.
3. Presione **F5** (perfil `http`). La base de datos `vetcare` se crea sola al arrancar, junto con los roles, los usuarios de prueba y el catálogo de especies y razas.
4. Se abre el frontend en `http://localhost:5080`. La documentación Swagger está en `http://localhost:5080/swagger`.

Desde la terminal:

```
dotnet run --project VetCare.Api
dotnet test
```

## Sin Visual Studio (VS Code o terminal)

El proyecto no necesita Visual Studio. Basta con el SDK de .NET 10, PostgreSQL y cualquier editor.

1. Compruebe el SDK en una terminal: `dotnet --version` debe mostrar `10.x`. Si no se reconoce, instale el **SDK de .NET 10** desde https://dotnet.microsoft.com/download/dotnet/10.0 (opción *SDK*, no *Runtime*) y abra una terminal nueva.
2. Abra `VetCare.Api/appsettings.Development.json` y escriba **su propio usuario y contraseña de PostgreSQL** en `Username=` y `Password=` (el usuario suele ser `postgres`). Puede abrirlo con el Bloc de notas o con VS Code.
3. En una terminal, dentro de la carpeta del proyecto (donde está `VetCare.sln`):
   ```
   dotnet restore
   dotnet run --project VetCare.Api
   ```
4. Abra en el navegador `http://localhost:5080` (frontend) o `http://localhost:5080/swagger` (documentación). Para detener la API, presione **Ctrl+C** en la terminal.
5. Pruebas unitarias, en otra terminal: `dotnet test VetCare.Pruebas`

Con **VS Code** instale la extensión *C# Dev Kit*, abra la carpeta `VetCare` y use la terminal integrada (Ver → Terminal) para los comandos anteriores.

## Usuarios de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin@vetcare.com | Admin12345* |
| Veterinario | veterinario@vetcare.com | Vet12345* |
| Recepcionista | recepcion@vetcare.com | Recepcion12345* |

Cambie estas contraseñas en `appsettings.json` antes de publicar la API en la web.

## Probar los endpoints uno por uno (Swagger)

1. Abra `/swagger` y ejecute `POST /api/autenticacion/login` con un usuario de prueba.
2. Copie el valor de `token` de la respuesta.
3. Presione el botón **Authorize**, pegue el token y confirme.
4. Ejecute cualquier endpoint protegido. Sin token la API responde `401`; con un rol sin permiso responde `403`.

## Endpoints implementados en este avance

| Método | Ruta | Acceso |
|---|---|---|
| POST | /api/autenticacion/registro | Público |
| POST | /api/autenticacion/login | Público |
| GET | /api/clientes?busqueda= | Autenticado |
| GET | /api/clientes/{id} | Autenticado |
| POST / PUT | /api/clientes · /api/clientes/{id} | Administrador, Recepcionista |
| DELETE | /api/clientes/{id} | Administrador |
| GET | /api/mascotas?clienteId=&fallecidas= | Autenticado |
| GET | /api/mascotas/{id} | Autenticado |
| POST / PUT | /api/mascotas · /api/mascotas/{id} | Administrador, Recepcionista |
| DELETE | /api/mascotas/{id} | Administrador |
| POST | /api/mascotas/{id}/fallecimiento | Administrador, Veterinario |
| DELETE | /api/mascotas/{id}/fallecimiento | Administrador |
| GET | /api/catalogos/especies · /api/catalogos/razas?especieId= | Autenticado |

## Mascotas fallecidas (memorial)

Una mascota fallecida **no se borra**: se marca como fallecida y se guarda su registro (fecha, causa y observaciones) en la tabla `registros_fallecimiento`. Sale de la lista activa (`GET /api/mascotas`) y aparece en el memorial (`GET /api/mascotas?fallecidas=true`). Su historial clínico se conserva y ya no se puede modificar. Un administrador puede revertir el registro si se hizo por error.

## Peso en kilogramos y libras

Al registrar una mascota, el peso se escribe en la unidad elegida (**kg** o **lb**) y el formulario muestra al instante el equivalente en la otra unidad. En la API se envían los campos `peso` y `unidadPeso` (`"kg"` o `"lb"`, por defecto `kg`). El sistema guarda siempre en kilogramos y responde con `pesoKg` y `pesoLb`.

Ejemplo de petición:

```json
{ "nombre": "Firulais", "clienteId": 1, "razaId": 1, "sexo": "Macho", "peso": 27.56, "unidadPeso": "lb" }
```

Respuesta (fragmento): `"pesoKg": 12.50, "pesoLb": 27.56`. El peso debe equivaler a un valor entre 0.01 kg y 200 kg; si no, la API responde `422`.

## Códigos HTTP

| Código | Cuándo se devuelve |
|---|---|
| 200 / 201 / 204 | Consulta o actualización correcta / creación correcta / eliminación correcta |
| 400 | Datos inválidos (campo vacío, correo mal formado, DPI sin 13 dígitos, sexo distinto de Macho o Hembra, unidad distinta de kg o lb) |
| 401 | Falta el token, venció o es inválido, o las credenciales de login son incorrectas |
| 403 | El token es válido pero el rol no tiene permiso para la acción |
| 404 | El recurso solicitado no existe |
| 409 | Conflicto: correo o documento duplicado, mascota repetida para el mismo dueño, fallecimiento ya registrado |
| 422 | Regla de negocio incumplida: fecha futura, cliente o raza inexistentes, peso fuera de rango, eliminar un cliente con mascotas |
| 500 | Error inesperado del servidor (el detalle se registra en el log, no se expone) |

Todos los errores usan el mismo formato: `{ "estado": 422, "mensaje": "...", "detalles": { ... } }`.

## Pruebas unitarias

Se ejecutan con `dotnet test` o desde el Explorador de pruebas de Visual Studio. Usan xUnit y Moq sobre la capa de servicios, sin base de datos.

| Módulo | Pruebas |
|---|---|
| Autenticación | 7 |
| Clientes | 8 |
| Mascotas, memorial y peso | 24 |
| Conversión de peso | 6 |
| **Total** | **45** |

## Sin Visual Studio (Visual Studio Code o terminal)

El proyecto no depende de Visual Studio. Solo necesita el **SDK de .NET 10** (compruébelo con `dotnet --version`; si no existe, instálelo desde https://dotnet.microsoft.com/download/dotnet/10.0) y PostgreSQL.

**Con Visual Studio Code**

1. Instale la extensión **C# Dev Kit** y abra la carpeta del proyecto (la que contiene `VetCare.sln`).
2. Escriba la contraseña de PostgreSQL en `VetCare.Api/appsettings.Development.json`.
3. Presione **F5** y elija la configuración *VetCare.Api*. Se abre el navegador en `http://localhost:5080`.
4. Para las pruebas: **Terminal → Ejecutar tarea → ejecutar-pruebas**.

**Solo con la terminal**

```
dotnet build VetCare.sln
dotnet run --project VetCare.Api --launch-profile http
dotnet test VetCare.sln
```

La primera línea compila, la segunda levanta la API en `http://localhost:5080` (frontend) y `http://localhost:5080/swagger`, y la tercera ejecuta las pruebas. Detenga la API con **Ctrl+C**.

## Estructura

```
VetCare.sln
├── VetCare.Datos      Entidades (18 tablas), ContextoVetCare (EF Core) y repositorios
├── VetCare.Negocio    Servicios, reglas de negocio, DTOs y excepciones
├── VetCare.Api        Controladores, JWT, Swagger, middleware y frontend (wwwroot)
├── VetCare.Pruebas    Pruebas unitarias con xUnit y Moq
└── docs/diagramas     Diagrama de capas, modelo entidad-relación y flujo de autenticación
```

Regla de dependencias: `Api → Negocio → Datos`. Los controladores no conocen los repositorios y los servicios no conocen HTTP.
