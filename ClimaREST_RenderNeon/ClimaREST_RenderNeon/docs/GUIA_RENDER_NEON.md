# Publicar Clima REST gratis con Render + Neon

Esta guía publica la aplicación ASP.NET Core 10 en Render y guarda los datos en PostgreSQL administrado por Neon. Se utilizan los niveles gratuitos de ambos servicios; no es necesario registrar tarjeta para crear las cuentas gratuitas.

> **Importante:** el plan gratuito de Render suspende las aplicaciones web tras 15 minutos sin tráfico y el primer acceso posterior puede tardar alrededor de un minuto. El sistema de archivos del servicio gratuito es temporal. Por eso la base de datos no se guarda en Render: se guarda en Neon.

## 1. Crear la base de datos en Neon

1. Abre <https://neon.com/> y crea una cuenta con el plan **Free**.
2. En el panel, pulsa **New project**.
3. Ponle un nombre, por ejemplo `clima-rest-db`.
4. Selecciona una región disponible. Guarda el proyecto.
5. Abre el proyecto y pulsa **Connect**.
6. Selecciona la rama `main`, la base de datos creada por Neon y el rol disponible.
7. Deja activado **Connection pooling**. La cadena de conexión debe incluir un host con `-pooler`.
8. Copia la cadena completa `postgresql://...`. Debe incluir `sslmode=require` y, si aparece, `channel_binding=require`.

La cadena es una contraseña: no la pegues en GitHub, no la incluyas en el código, no la envíes por chat y no la compartas en capturas. Guárdala temporalmente en un lugar privado. Se introducirá como variable secreta en Render.

## 2. Crear un repositorio propio en GitHub

No se debe modificar el repositorio de ejemplo del profesor. Crea uno nuevo en tu propia cuenta.

1. Abre <https://github.com/new> e inicia sesión.
2. En **Repository name**, escribe `clima-rest-neon`.
3. Selecciona **Public** para que Render pueda leer el código sin permisos adicionales.
4. No marques la creación de README, `.gitignore` ni licencia: el proyecto ya contiene esos archivos.
5. Pulsa **Create repository**.

### Subir los archivos desde Windows

1. Instala Git for Windows desde <https://git-scm.com/download/win> si todavía no está instalado.
2. Abre la carpeta `RestFulWebService` descomprimida (la carpeta donde están `Dockerfile`, `render.yaml` y `RestFulWebService.csproj`).
3. Haz clic en la barra de direcciones del Explorador, escribe `powershell` y presiona Enter.
4. Comprueba que Git está instalado:

   ```powershell
   git --version
   ```

5. Ejecuta estos comandos, uno por uno. Sustituye `TU_USUARIO` por tu usuario real de GitHub:

   ```powershell
   git init
   git add .
   git commit -m "Preparar Clima REST para Render y Neon"
   git branch -M main
   git remote add origin https://github.com/TU_USUARIO/clima-rest-neon.git
   git push -u origin main
   ```

6. Si Git pide configurar tu identidad, utiliza el nombre y correo que quieras asociar a tus commits:

   ```powershell
   git config --global user.name "Tu nombre"
   git config --global user.email "tu-correo"
   ```

   Después vuelve a ejecutar `git commit` y `git push`.

7. Si aparece una ventana de Git Credential Manager, inicia sesión en GitHub y autoriza la operación.
8. Recarga el repositorio en GitHub y comprueba que se ven `Dockerfile`, `render.yaml`, `Program.cs`, `Controllers`, `Models` y `wwwroot` en la raíz.

El archivo `.gitignore` excluye bases de datos locales y archivos `.env`. Nunca subas contraseñas ni la cadena `DATABASE_URL` a GitHub.

## 3. Crear el servicio en Render con el Blueprint

1. Abre <https://render.com/> y crea una cuenta con el plan gratuito.
2. Conecta Render con GitHub cuando Render lo solicite. Autoriza el acceso al repositorio `clima-rest-neon`.
3. En Render Dashboard, pulsa **New +** y selecciona **Blueprint**.
4. Selecciona el repositorio `TU_USUARIO/clima-rest-neon` y la rama `main`.
5. Render detectará el archivo `render.yaml` y mostrará el servicio `clima-rest-neon`.
6. Verifica que el plan del servicio sea **Free**.
7. En la variable `DATABASE_URL`, pega la cadena completa copiada de Neon. Esta variable se marca `sync: false` para que sea un valor secreto y no se guarde en el repositorio.
8. Confirma la creación del Blueprint y espera a que termine el primer deploy.

Si Render no solicita la variable durante la creación, abre el servicio → **Environment** → **Add Environment Variable**, crea `DATABASE_URL`, pega la URL de Neon y guarda los cambios para iniciar un nuevo deploy.

## 4. Comprobar el despliegue

Render asignará una URL parecida a `https://clima-rest-neon.onrender.com`. Usa la URL exacta que aparece en el panel.

Prueba estas rutas:

- `/` — interfaz web.
- `/health` — debe responder JSON con `status: healthy` y `database: PostgreSQL (Neon)`.
- `/api/weatherforecast` — lista de pronósticos.
- `/openapi/v1.json` — especificación OpenAPI.

En la interfaz crea un pronóstico, edítalo y elimínalo. Para comprobar persistencia, espera a que el servicio se suspenda o realiza otro deploy y luego revisa que los registros que no eliminaste sigan en Neon.

## 5. Cómo funciona la conexión

- En local, si `DATABASE_URL` no existe, la aplicación utiliza SQLite en `App_Data/weather.db`.
- En Render, `ASPNETCORE_ENVIRONMENT=Production` y `DATABASE_URL` conecta Entity Framework Core a PostgreSQL de Neon.
- La URL PostgreSQL que entrega Neon se convierte dentro de la aplicación a una cadena que entiende Npgsql, exigiendo SSL y channel binding.
- La aplicación crea las tablas automáticamente la primera vez y registra cinco pronósticos iniciales si la tabla está vacía.
- La cadena de conexión se mantiene en las variables secretas de Render, no en el código.

## 6. Límites de los planes gratuitos

- Render Free suspende el servicio web después de 15 minutos sin actividad; la primera petición posterior puede tardar cerca de un minuto.
- Render Free no ofrece disco persistente; por eso no debe usarse SQLite para datos de producción en este despliegue.
- Neon Free tiene cuotas de cómputo, almacenamiento y transferencia. El cómputo puede apagarse cuando está inactivo y activarse con la siguiente conexión.
- Estos planes son apropiados para prácticas, demostraciones y proyectos académicos pequeños, no para aplicaciones con compromisos de disponibilidad.

## 7. Solución de problemas

### Render muestra `Falta configurar DATABASE_URL`

La variable `DATABASE_URL` no se creó o no se guardó. Añádela en **Environment** con la URL completa de Neon y vuelve a desplegar.

### Error de autenticación o SSL al conectar con Neon

Abre Neon → **Connect**, vuelve a copiar la URL completa del proyecto correcto y verifica que contiene `sslmode=require`. No modifiques el host ni el usuario a mano.

### Error al compilar Docker

Abre Render → servicio → **Events** y **Logs**. Comprueba que el repositorio tenga `Dockerfile`, `RestFulWebService.csproj` y las carpetas `Controllers`, `Data`, `Models` y `wwwroot` en la raíz del repositorio.

### Los datos no aparecen tras volver a desplegar

Asegúrate de que Render usa `DATABASE_URL` de Neon. No se debe definir `WEATHER_DB_PATH` ni configurar un disco SQLite para este plan gratuito.

## Documentación oficial

- Render: <https://render.com/docs/your-first-deploy>
- Render Free: <https://render.com/docs/free>
- Render Blueprints: <https://render.com/docs/blueprint-spec>
- Neon Free plan: <https://neon.com/docs/introduction/plans>
- Neon connection strings: <https://neon.com/docs/get-started/connect-neon>
- Npgsql EF Core: <https://www.npgsql.org/efcore/>
