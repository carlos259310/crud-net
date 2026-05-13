# TiendaDB — API REST + Frontend

API REST en ASP.NET Core (.NET 10) con frontend estático para gestión de categorías, productos y tipos de documentos.

---

## Requisitos previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server Express instalado y corriendo en `localhost\SQLEXPRESS`
- Las tablas `Categorias`, `Productos` y `TiposDocumentos` deben existir en la base de datos

---

## Paso a paso para ejecutar el proyecto

### 1. Clonar el repositorio

```bash
git clone https://github.com/carlos259310/crud-net.git
cd crud-net
```

### 2. Configurar la base de datos

Abre SQL Server Management Studio (SSMS) o cualquier cliente SQL y ejecuta:

```sql
-- Crear la base de datos
CREATE DATABASE TiendaDB;

-- Crear el usuario
CREATE LOGIN developer WITH PASSWORD = '123456';
USE TiendaDB;
CREATE USER developer FOR LOGIN developer;
ALTER ROLE db_owner ADD MEMBER developer;
```

> Las tablas deben existir previamente. Solicita el script de creación al desarrollador si es una instalación nueva.

### 3. Verificar la cadena de conexión

El archivo `WebApplication1/appsettings.json` ya tiene configurada la conexión:

```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TiendaDB;User Id=developer;Password=123456;TrustServerCertificate=True;"
```

Si tu instancia de SQL Server tiene un nombre diferente, actualiza el valor de `Server`.

### 4. Ejecutar el proyecto

```bash
dotnet run --project WebApplication1/WebApplication1.csproj
```

El servidor arranca en `http://localhost:5117`.

---

## URLs disponibles

| Recurso | URL |
|---|---|
| Swagger (explorador de API) | `http://localhost:5117/swagger` |
| Frontend — Categorías | `http://localhost:5117/categorias.html` |
| Frontend — Productos | `http://localhost:5117/productos.html` |
| Frontend — Tipos de Documentos | `http://localhost:5117/tiposdocumentos.html` |
| Test conexión BD | `GET http://localhost:5117/api/testdb` |

---

## Endpoints de la API

### Categorías `/api/categorias`
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/categorias` | Listar todas |
| GET | `/api/categorias/{id}` | Obtener por ID |
| POST | `/api/categorias` | Crear |
| PUT | `/api/categorias/{id}` | Actualizar |
| DELETE | `/api/categorias/{id}` | Eliminar |

### Productos `/api/productos`
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/productos` | Listar todos |
| GET | `/api/productos/{id}` | Obtener por ID |
| POST | `/api/productos` | Crear (requiere `categoriaId` existente) |
| PUT | `/api/productos/{id}` | Actualizar |
| DELETE | `/api/productos/{id}` | Eliminar |

### Tipos de Documentos `/api/tiposdocumentos`
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/tiposdocumentos` | Listar todos |
| GET | `/api/tiposdocumentos/{id}` | Obtener por ID |
| POST | `/api/tiposdocumentos` | Crear (`codigo` debe ser único) |
| PUT | `/api/tiposdocumentos/{id}` | Actualizar |
| DELETE | `/api/tiposdocumentos/{id}` | Eliminar |

---

## Solución de problemas comunes

**Error de conexión a la base de datos**
- Verifica que SQL Server Express esté corriendo: `services.msc` → busca `SQL Server (SQLEXPRESS)`.
- Si tu instancia tiene otro nombre (ej. `MSSQLSERVER`), cambia `Server=localhost\\SQLEXPRESS` por `Server=localhost` en `appsettings.json`.
- Confirma que el usuario `developer` existe y tiene acceso a `TiendaDB`.
- Verifica la conexión en: `GET http://localhost:5117/api/testdb` — debe responder `{ "connected": true }`.

**El puerto 5117 ya está en uso**
- Cambia el puerto en `Properties/launchSettings.json`.
- Luego actualiza la URL en los tres archivos `wwwroot/js/*.js` (están hardcodeadas a `http://localhost:5117`).

**Las tablas no existen en la base de datos**
- Las tablas deben crearse manualmente. Solicita el script SQL al desarrollador original.
