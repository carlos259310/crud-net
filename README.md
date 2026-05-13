# TiendaDB — CRUD con ASP.NET Core + SQL Server

API REST en ASP.NET Core (.NET 10) con frontend estático (HTML + Bootstrap + JavaScript) para gestionar categorías, productos y tipos de documentos. Sin migraciones: las tablas se crean con script SQL manual.

---

## Tabla de contenidos

1. [Requisitos previos](#1-requisitos-previos)
2. [Crear el proyecto desde cero](#2-crear-el-proyecto-desde-cero)
3. [Instalar los paquetes NuGet](#3-instalar-los-paquetes-nuget)
4. [Estructura del proyecto](#4-estructura-del-proyecto)
5. [Configurar la base de datos](#5-configurar-la-base-de-datos)
6. [Configurar la cadena de conexión](#6-configurar-la-cadena-de-conexión)
7. [Crear los archivos del proyecto](#7-crear-los-archivos-del-proyecto)
8. [Ejecutar el proyecto](#8-ejecutar-el-proyecto)
9. [URLs disponibles](#9-urls-disponibles)
10. [Endpoints de la API](#10-endpoints-de-la-api)
11. [Solución de problemas](#11-solución-de-problemas)

---

## 1. Requisitos previos

| Herramienta | Versión mínima | Descarga |
|---|---|---|
| .NET SDK | 10.0 | https://dotnet.microsoft.com/download/dotnet/10.0 |
| SQL Server Express | cualquiera | https://www.microsoft.com/sql-server/sql-server-downloads |
| SQL Server Management Studio (SSMS) | cualquiera | opcional, para gestionar la BD |

Verifica que .NET esté instalado:

```bash
dotnet --version
# debe mostrar 10.x.x
```

---

## 2. Crear el proyecto desde cero

```bash
# Crear la solución y el proyecto Web API
dotnet new webapi -n WebApplication1 --no-openapi
cd WebApplication1
```

> Si prefieres clonar el repositorio ya completo, ejecuta:
> ```bash
> git clone https://github.com/carlos259310/crud-net.git
> cd crud-net
> ```
> Y salta directo al [Paso 5](#5-configurar-la-base-de-datos).

---

## 3. Instalar los paquetes NuGet

Estos paquetes son necesarios para que el proyecto funcione:

| Paquete | Para qué sirve |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | Conectar EF Core con SQL Server |
| `Microsoft.EntityFrameworkCore.Tools` | Herramientas EF Core para la CLI |
| `Microsoft.AspNetCore.OpenApi` | Soporte OpenAPI (metadatos Swagger) |
| `Swashbuckle.AspNetCore` | Genera la UI interactiva de Swagger |

Instálalos con:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Swashbuckle.AspNetCore
```

El archivo `.csproj` quedará así:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.7" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.7" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.7">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
  <PackageReference Include="Swashbuckle.AspNetCore" Version="10.1.7" />
</ItemGroup>
```

---

## 4. Estructura del proyecto

```
WebApplication1/
├── Controllers/
│   ├── CategoriasController.cs       # CRUD de categorías
│   ├── ProductosController.cs        # CRUD de productos
│   ├── TiposDocumentosController.cs  # CRUD de tipos de documentos
│   └── TestController.cs             # Verifica conexión a la BD
├── Data/
│   └── AppDbContext.cs.cs            # DbContext con los tres DbSets
├── Models/
│   ├── Categoria.cs                  # Entidad Categoria
│   ├── Producto.cs                   # Entidad Producto
│   └── TipoDocumento.cs              # Entidad TipoDocumento
├── Properties/
│   └── launchSettings.json           # Puerto y entorno de ejecución
├── wwwroot/
│   ├── categorias.html               # Página frontend Categorías
│   ├── productos.html                # Página frontend Productos
│   ├── tiposdocumentos.html          # Página frontend Tipos de Documentos
│   └── js/
│       ├── categorias.js             # Lógica fetch para categorías
│       ├── productos.js              # Lógica fetch para productos
│       └── tiposdocumentos.js        # Lógica fetch para tipos de documentos
├── appsettings.json                  # Cadena de conexión y logging
└── Program.cs                        # Configuración de servicios y middleware
```

### Flujo de datos

```
wwwroot/js/*.js  →  fetch HTTP  →  Controllers  →  AppDbContext  →  SQL Server
```

- Los controladores reciben peticiones REST y llaman a `AppDbContext`.
- `AppDbContext` hereda de `DbContext` (EF Core) y expone tres `DbSet<T>`.
- Los modelos en `Models/` mapean directamente a tablas SQL (sin migraciones).
- El frontend en `wwwroot/` consume la API con `fetch` vanilla y muestra los datos con Bootstrap 5.

---

## 5. Configurar la base de datos

Abre SSMS o cualquier cliente SQL y ejecuta este script completo:

```sql
-- 1. Crear la base de datos
CREATE DATABASE TiendaDB;
GO

-- 2. Crear el usuario de la aplicación
CREATE LOGIN developer WITH PASSWORD = '123456';
USE TiendaDB;
GO
CREATE USER developer FOR LOGIN developer;
ALTER ROLE db_owner ADD MEMBER developer;
GO

-- 3. Crear las tablas
CREATE TABLE Categorias (
    Id     INT           IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL
);

CREATE TABLE Productos (
    Id          INT           IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(MAX) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Precio      DECIMAL(18,2) NOT NULL,
    Stock       INT           NOT NULL,
    CategoriaId INT           NOT NULL,
    Activo      BIT           NOT NULL
);

CREATE TABLE TiposDocumentos (
    Id     INT          IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(10) NOT NULL,
    Nombre NVARCHAR(50) NOT NULL
);
```

> **Nota:** `CategoriaId` en `Productos` es una referencia lógica a `Categorias.Id`. La validación la hace el controlador, no una FK a nivel de BD.

---

## 6. Configurar la cadena de conexión

Edita `appsettings.json` con los datos de tu instancia SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TiendaDB;User Id=developer;Password=123456;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Si tu instancia SQL Server **no** es `SQLEXPRESS`, cambia `Server`:

| Instalación | Valor de Server |
|---|---|
| SQL Server Express | `localhost\\SQLEXPRESS` |
| SQL Server Developer / Standard | `localhost` |
| Instancia con nombre personalizado | `localhost\\NOMBRE_INSTANCIA` |

---

## 7. Crear los archivos del proyecto

### `Data/AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<TipoDocumento> TiposDocumentos => Set<TipoDocumento>();
    }
}
```

### `Models/Categoria.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;
    }
}
```

### `Models/Producto.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public int Stock { get; set; }
        public int CategoriaId { get; set; }
        public bool Activo { get; set; }
    }
}
```

### `Models/TipoDocumento.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("TiposDocumentos")]
    public class TipoDocumento
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;
    }
}
```

### `Program.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 8. Ejecutar el proyecto

```bash
dotnet run --project WebApplication1/WebApplication1.csproj
```

El servidor arranca en `http://localhost:5117`.

Para verificar que la base de datos conecta correctamente:

```
GET http://localhost:5117/api/testdb
```

Respuesta esperada:

```json
{ "connected": true }
```

---

## 9. URLs disponibles

| Recurso | URL |
|---|---|
| Swagger (explorador de API) | `http://localhost:5117/swagger` |
| Frontend — Categorías | `http://localhost:5117/categorias.html` |
| Frontend — Productos | `http://localhost:5117/productos.html` |
| Frontend — Tipos de Documentos | `http://localhost:5117/tiposdocumentos.html` |
| Test conexión BD | `http://localhost:5117/api/testdb` |

---

## 10. Endpoints de la API

### Categorías `/api/categorias`

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/categorias` | Listar todas |
| GET | `/api/categorias/{id}` | Obtener por ID |
| POST | `/api/categorias` | Crear |
| PUT | `/api/categorias/{id}` | Actualizar |
| DELETE | `/api/categorias/{id}` | Eliminar |

**Body POST / PUT:**
```json
{ "nombre": "Electrónica" }
```

### Productos `/api/productos`

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/productos` | Listar todos |
| GET | `/api/productos/{id}` | Obtener por ID |
| POST | `/api/productos` | Crear (requiere `categoriaId` existente) |
| PUT | `/api/productos/{id}` | Actualizar |
| DELETE | `/api/productos/{id}` | Eliminar |

**Body POST / PUT:**
```json
{
  "nombre": "Laptop",
  "descripcion": "Laptop 15 pulgadas",
  "precio": 999.99,
  "stock": 10,
  "categoriaId": 1,
  "activo": true
}
```

### Tipos de Documentos `/api/tiposdocumentos`

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/tiposdocumentos` | Listar todos |
| GET | `/api/tiposdocumentos/{id}` | Obtener por ID |
| POST | `/api/tiposdocumentos` | Crear (`codigo` debe ser único) |
| PUT | `/api/tiposdocumentos/{id}` | Actualizar |
| DELETE | `/api/tiposdocumentos/{id}` | Eliminar |

**Body POST / PUT:**
```json
{ "codigo": "DNI", "nombre": "Documento Nacional de Identidad" }
```

---

## 11. Solución de problemas

**Error de conexión a la base de datos**
- Verifica que SQL Server esté corriendo: `services.msc` → busca `SQL Server (SQLEXPRESS)`.
- Confirma que el usuario `developer` existe y tiene acceso a `TiendaDB`.
- Prueba la conexión en: `GET http://localhost:5117/api/testdb`.

**Las tablas no existen**
- Ejecuta el script SQL completo del [Paso 5](#5-configurar-la-base-de-datos).

**El puerto 5117 ya está en uso**
- Cambia el puerto en `Properties/launchSettings.json`.
- Actualiza también la URL base en los tres archivos `wwwroot/js/*.js` (están hardcodeadas a `http://localhost:5117`).

**`dotnet run` no encuentra el proyecto**
- Asegúrate de estar en la raíz del repositorio y ejecutar:
  ```bash
  dotnet run --project WebApplication1/WebApplication1.csproj
  ```
