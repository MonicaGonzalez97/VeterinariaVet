# VetCare · API de gestión veterinaria

Proyecto final de **Arquitectura de Sistemas** · Universidad Mesoamericana Quetzaltenango (UMES).

VetCare ("Vet" de veterinaria + "Care", cuidado) es una API pública protegida con JWT, con arquitectura en capas, Entity Framework Core sobre PostgreSQL, documentación Swagger, pruebas unitarias con mocks y un frontend web funcional.

## Requisitos

- .NET SDK 10 (probado con 10.0.300)
- PostgreSQL 18
- Visual Studio 2026 (o `dotnet` en la terminal)

## Cómo levantar el proyecto

1. Abra `VetCare.slnx` con Visual Studio 2026.
2. Abra `VetCare.Api/appsettings.Development.json` y escriba la contraseña de su usuario de PostgreSQL en `Password=`. El usuario configurado es `sharoncanellon`; cámbielo si usa otro.
3. Presione **F5** (perfil `http`). La base de datos `vetcare` se crea sola al arrancar, junto con los roles, los usuarios de prueba y el catálogo de especies y razas.
4. Se abre el frontend en `http://localhost:5080`. La documentación Swagger está en `http://localhost:5080/swagger`.

Desde la terminal:

```
dotnet run --project VetCare.Api
dotnet test
```

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

## Estructura

```
VetCare.slnx
├── VetCare.Datos      Entidades (18 tablas), ContextoVetCare (EF Core) y repositorios
├── VetCare.Negocio    Servicios, reglas de negocio, DTOs y excepciones
├── VetCare.Api        Controladores, JWT, Swagger, middleware y frontend (wwwroot)
├── VetCare.Pruebas    Pruebas unitarias con xUnit y Moq
└── docs/diagramas     Diagrama de capas, modelo entidad-relación y flujo de autenticación
```

Regla de dependencias: `Api → Negocio → Datos`. Los controladores no conocen los repositorios y los servicios no conocen HTTP.
