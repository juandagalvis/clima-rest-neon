# Clima REST — Aplicación web con acceso a datos

Proyecto académico construido a partir de la plantilla `RestFulWebService` compartida por el profesor. La plantilla original exponía un endpoint de ejemplo que generaba pronósticos aleatorios. Esta versión la convierte en una aplicación web CRUD conectada a una base de datos.

## ¿Qué hace?

- Consultar pronósticos guardados.
- Crear un pronóstico indicando fecha, condición y temperatura.
- Editar un pronóstico existente.
- Eliminar un pronóstico.
- Ver estadísticas sencillas de temperatura.
- Consultar las mismas operaciones desde una API REST.

La interfaz está en español y se adapta a pantallas de computador y celular.

## Tecnologías

| Tecnología | Uso |
|---|---|
| C# / ASP.NET Core 10 | Aplicación web y API REST |
| Entity Framework Core 10 | Acceso a datos |
| SQLite | Base de datos local para desarrollo |
| PostgreSQL en Neon | Base de datos persistente en la nube |
| HTML, CSS y JavaScript | Interfaz web |
| Docker | Empaquetado y despliegue en Render |

## Requisitos para ejecutarlo en Windows

1. Windows 10/11.
2. Instalar [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
3. Para el despliegue, crear cuentas gratuitas en [GitHub](https://github.com/), [Neon](https://neon.com/) y [Render](https://render.com/).

## Instalación y ejecución local

1. Descomprime el proyecto.
2. Abre la carpeta `RestFulWebService`.
3. Haz doble clic en `run-local.bat`, o ejecuta en PowerShell dentro de la carpeta:

   ```powershell
   dotnet restore
   dotnet run --urls http://localhost:5160
   ```

4. Abre <http://localhost:5160>.
5. La especificación OpenAPI se encuentra en <http://localhost:5160/openapi/v1.json>.

Si `DATABASE_URL` no está configurada, el entorno local utiliza SQLite y crea `App_Data/weather.db`. La primera ejecución crea las tablas e inserta cinco registros de demostración.

## API REST

La API se encuentra bajo `/api/weatherforecast`.

| Método | Ruta | Acción |
|---|---|---|
| GET | `/api/weatherforecast` | Listar todos los pronósticos |
| GET | `/api/weatherforecast/{id}` | Consultar un pronóstico |
| POST | `/api/weatherforecast` | Crear un pronóstico |
| PUT | `/api/weatherforecast/{id}` | Actualizar un pronóstico |
| DELETE | `/api/weatherforecast/{id}` | Eliminar un pronóstico |

Ejemplo de cuerpo JSON para crear o actualizar:

```json
{
  "date": "2026-11-25",
  "temperatureC": 23,
  "summary": "Parcialmente nublado"
}
```

La temperatura permitida está entre -80 °C y 60 °C. La API calcula y devuelve también el equivalente Fahrenheit.

## Despliegue en la nube: Render + Neon

El archivo `render.yaml` define un servicio Docker gratuito en Render. En producción, la aplicación utiliza PostgreSQL de Neon mediante la variable secreta `DATABASE_URL`. No se necesita un disco persistente en Render.

Sigue la guía completa [`docs/GUIA_RENDER_NEON.md`](docs/GUIA_RENDER_NEON.md). La guía explica cómo crear la base PostgreSQL, publicar el código en un repositorio GitHub propio, conectar Render y comprobar la URL pública.

**Seguridad:** nunca guardes la contraseña de Neon en `Program.cs`, `appsettings.json`, `.env` que se suba al repositorio, ni en capturas públicas. Configura `DATABASE_URL` únicamente en las variables de entorno de Render.

## Comprobaciones antes de entregar

- [ ] Ejecutar `run-local.bat`.
- [ ] Verificar que cargan los cinco pronósticos iniciales.
- [ ] Crear un registro y comprobar que aparece en la tabla.
- [ ] Editar y eliminar un registro.
- [ ] Abrir `/openapi/v1.json` y revisar los endpoints.
- [ ] Crear la base de datos PostgreSQL en Neon.
- [ ] Desplegar el servicio en Render con plan Free y variable `DATABASE_URL`.
- [ ] Guardar la URL pública de Render.
- [ ] Probar `/health` y las operaciones CRUD en la URL pública.
