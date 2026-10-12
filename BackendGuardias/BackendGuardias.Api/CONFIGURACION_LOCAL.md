# Configuración local de BackendGuardias.Api

## 1. Configurar MySQL

Desde la carpeta del proyecto:

```powershell
cd BackendGuardias\BackendGuardias.Api
Copy-Item .env.example .env
notepad .env
```

Edite `GUARDIAS_CONNECTION_STRING` y reemplace `USUARIO_LOCAL` y `CONTRASENA_LOCAL` por las credenciales de MySQL que tengan acceso al esquema `guardia`.

Ejemplo de formato (no use estos valores como credenciales):

```dotenv
GUARDIAS_CONNECTION_STRING="Server=192.168.4.33;Port=3306;Database=guardia;User ID=mi_usuario;Password=mi_contrasena;"
```

El archivo `.env` es local y está excluido de Git. No suba contraseñas al repositorio.

También puede configurar la cadena directamente en `appsettings.Development.json` bajo `ConnectionStrings:GuardiasConnection`, o definir la variable de entorno `ConnectionStrings__GuardiasConnection`. Evite guardar secretos en archivos versionados.

## 2. Ejecutar la API

```powershell
dotnet restore
dotnet build
dotnet run
```

La API escucha en `http://localhost:5206`; Swagger está en `http://localhost:5206/swagger`.

## 3. Si la conexión falla

- Verifique que el servidor MySQL esté accesible desde su equipo y que el puerto sea correcto.
- Confirme que el usuario tenga permisos sobre la base `guardia` y las vistas que consulta la API.
- No publique el mensaje de error si contiene contraseñas ni comparta el archivo `.env`.
