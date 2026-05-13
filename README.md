# Guía completa: CRUD con ASP.NET Core 10 + EF Core + SQL Server

Esta guía explica cómo construir desde cero una API REST con ASP.NET Core, Entity Framework Core y SQL Server, incluyendo un frontend estático con HTML, Bootstrap y JavaScript. Al terminar tendrás un sistema funcional para gestionar **Categorías**, **Productos** y **Tipos de Documentos**.

---

## ¿Qué vamos a construir?

Un proyecto con dos capas:

- **Backend:** API REST en ASP.NET Core que expone endpoints para crear, leer, actualizar y eliminar registros en SQL Server.
- **Frontend:** Páginas HTML simples que consumen esa API usando `fetch` y muestran los datos con Bootstrap.

```
Navegador (HTML + JS)
      ↕ fetch HTTP
API REST (ASP.NET Core)
      ↕ EF Core
SQL Server (TiendaDB)
```

---

## Requisitos previos

Antes de empezar instala:

| Herramienta | Para qué se usa | Descarga |
|---|---|---|
| .NET 10 SDK | Compilar y ejecutar el proyecto | https://dotnet.microsoft.com/download/dotnet/10.0 |
| SQL Server Express | Base de datos local | https://www.microsoft.com/sql-server/sql-server-downloads |
| SSMS (opcional) | Gestionar la base de datos visualmente | https://aka.ms/ssms |

Verifica que .NET esté instalado:

```bash
dotnet --version
# debe mostrar 10.x.x
```

---

## Paso 1 — Crear el proyecto

Abre una terminal y ejecuta:

```bash
dotnet new webapi -n WebApplication1
cd WebApplication1
```

Esto genera un proyecto Web API básico. Puedes borrar los archivos de ejemplo que crea la plantilla (`WeatherForecast.cs` y `Controllers/WeatherForecastController.cs`) porque no los usaremos.

---

## Paso 2 — Instalar los paquetes NuGet

Los paquetes son librerías externas que agregan funcionalidad al proyecto. Necesitamos cuatro:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Swashbuckle.AspNetCore
```

**¿Para qué sirve cada uno?**

| Paquete | Por qué lo necesitamos |
|---|---|
| `EntityFrameworkCore.SqlServer` | Permite que EF Core se conecte y hable con SQL Server. Sin este paquete EF Core no sabe cómo traducir sus consultas al dialecto de SQL Server. |
| `EntityFrameworkCore.Tools` | Agrega comandos a la CLI de .NET para trabajar con EF Core (como `dotnet ef`). Necesario aunque no usemos migraciones. |
| `Microsoft.AspNetCore.OpenApi` | Genera automáticamente la documentación de los endpoints en formato OpenAPI (el estándar para describir APIs). |
| `Swashbuckle.AspNetCore` | Lee esa documentación OpenAPI y la convierte en la interfaz web de Swagger, donde puedes probar los endpoints desde el navegador. |

---

## Paso 3 — Crear la estructura de carpetas

Crea estas carpetas manualmente o desde la terminal:

```
WebApplication1/
├── Controllers/    ← recibe las peticiones HTTP y devuelve respuestas
├── Data/           ← contiene el DbContext (la conexión a la BD)
├── Models/         ← las clases que representan las tablas de la BD
└── wwwroot/        ← archivos estáticos (HTML, JS, CSS)
    └── js/
```

```bash
mkdir Controllers Data Models wwwroot wwwroot/js
```

---

## Paso 4 — Crear los modelos

Los **modelos** son clases de C# que representan las tablas de la base de datos. Cada propiedad de la clase se convierte en una columna de la tabla.

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

**¿Qué significan los atributos?**
- `[Required]` — el campo no puede estar vacío. ASP.NET Core lo valida automáticamente antes de guardar.
- `[MaxLength(100)]` — limita el texto a 100 caracteres, lo que se traduce a `NVARCHAR(100)` en SQL Server.
- La propiedad `Id` es reconocida automáticamente por EF Core como clave primaria por su nombre.

---

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

**Puntos clave:**
- `[MaxLength]` sin número = texto sin límite (`NVARCHAR(MAX)` en SQL Server).
- `string?` con el signo `?` indica que el campo puede ser nulo (opcional).
- `[Column(TypeName = "decimal(18,2)")]` especifica que en la BD se guarda como `DECIMAL(18,2)` para manejar centavos correctamente. Sin esto, C# usaría un tipo menos preciso.
- `CategoriaId` es la referencia a qué categoría pertenece el producto (relación lógica, sin FK de base de datos).

---

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

**Punto clave:**
- `[Table("TiposDocumentos")]` le dice a EF Core que esta clase usa la tabla `TiposDocumentos` en la BD. Sin este atributo, EF Core buscaría una tabla llamada `TipoDocumento` (el nombre de la clase), que no existe.

---

## Paso 5 — Crear el DbContext

El **DbContext** es la clase central de EF Core. Representa la sesión con la base de datos y es el punto de entrada para hacer consultas. Piénsalo como el "intermediario" entre tu código C# y SQL Server.

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

**¿Qué es un `DbSet<T>`?**

Cada `DbSet` representa una tabla. Con `_context.Categorias` puedes hacer consultas como `ToListAsync()`, `FindAsync(id)`, `Add()`, `Remove()`, etc. EF Core traduce esas llamadas al SQL correspondiente.

---

## Paso 6 — Configurar la base de datos

Este proyecto **no usa migraciones** de EF Core. Las tablas se crean manualmente con SQL. Abre SSMS y ejecuta:

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

-- 3. Crear las tablas (deben coincidir exactamente con los modelos)
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

**¿Por qué los tipos coinciden con los modelos?**

EF Core no crea las tablas, pero sí las lee. Si los tipos no coinciden (por ejemplo, poner `VARCHAR` en vez de `NVARCHAR`) pueden aparecer errores al guardar o leer datos.

---

## Paso 7 — Configurar la cadena de conexión

La cadena de conexión le dice a la aplicación **dónde está** la base de datos, **cómo conectarse** y **con qué credenciales**.

Edita `appsettings.json`:

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

**¿Qué significa cada parte de la cadena?**

| Parte | Qué indica |
|---|---|
| `Server=localhost\\SQLEXPRESS` | Instancia de SQL Server en tu máquina local |
| `Database=TiendaDB` | Nombre de la base de datos |
| `User Id=developer` | Usuario SQL que creamos en el paso anterior |
| `Password=123456` | Contraseña del usuario |
| `TrustServerCertificate=True` | Acepta el certificado SSL local sin validarlo (necesario en desarrollo local) |

Si tu SQL Server no es Express, cambia `Server`:

| Instalación | Valor |
|---|---|
| SQL Server Express | `localhost\\SQLEXPRESS` |
| SQL Server Developer / Standard | `localhost` |
| Instancia con nombre | `localhost\\NOMBRE_INSTANCIA` |

---

## Paso 8 — Configurar Program.cs

`Program.cs` es el punto de entrada de la aplicación. Aquí se registran todos los servicios y se configura el pipeline de peticiones HTTP.

```csharp
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

var builder = WebApplication.CreateBuilder(args);

// Registrar los controladores (lee las clases en la carpeta Controllers/)
builder.Services.AddControllers();

// Registrar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar EF Core con SQL Server y la cadena de conexión de appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Redirigir HTTP a HTTPS
app.UseHttpsRedirection();

// Servir archivos estáticos desde wwwroot/ (index.html, categorias.html, etc.)
app.UseDefaultFiles();
app.UseStaticFiles();

// Activar Swagger en todos los entornos
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

// Mapear las rutas automáticamente desde los atributos de los controladores
app.MapControllers();

app.Run();
```

**¿Qué es la inyección de dependencias (`AddDbContext`, `AddControllers`)?**

ASP.NET Core usa un sistema de inyección de dependencias. Al llamar `builder.Services.AddDbContext<AppDbContext>(...)`, le dices al framework: "cuando alguien pida un `AppDbContext`, créalo con esta configuración". Los controladores reciben el contexto automáticamente en su constructor sin necesidad de crearlo manualmente.

---

## Paso 9 — Crear los controladores

Los **controladores** reciben las peticiones HTTP, ejecutan la lógica necesaria y devuelven una respuesta. Todos siguen el mismo patrón.

### Patrón base de un controlador

```csharp
[ApiController]                    // indica que es un controlador de API
[Route("api/[controller]")]        // la ruta se toma del nombre de la clase (sin "Controller")
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;

    // ASP.NET Core inyecta el DbContext automáticamente aquí
    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }
}
```

Con `[Route("api/[controller]")]` y el nombre de clase `CategoriasController`, la ruta base será `/api/categorias`.

---

### `Controllers/CategoriasController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/categorias — devuelve todas las categorías
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categorias = await _context.Categorias
                .AsNoTracking()   // más eficiente para consultas de solo lectura
                .ToListAsync();

            return Ok(categorias);
        }

        // GET api/categorias/1 — devuelve una categoría por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            var categoria = await _context.Categorias
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (categoria == null)
                return NotFound();

            return Ok(categoria);
        }

        // POST api/categorias — crea una nueva categoría
        [HttpPost]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT api/categorias/1 — actualiza una categoría existente
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Categoria model)
        {
            if (id <= 0) return BadRequest("Id inválido");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound();

            categoria.Nombre = model.Nombre;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE api/categorias/1 — elimina una categoría
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Id inválido");

            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
                return NotFound();

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
```

**Conceptos importantes:**

- `async / await` — Las operaciones de base de datos son lentas. Con `async` el hilo del servidor queda libre para atender otras peticiones mientras espera a SQL Server.
- `AsNoTracking()` — Por defecto EF Core "rastrea" los objetos que carga para detectar cambios. En consultas de solo lectura esto es innecesario y consume memoria, `AsNoTracking()` lo desactiva.
- `FindAsync(id)` — Busca por clave primaria directamente, más eficiente que un `FirstOrDefaultAsync`.
- `SaveChangesAsync()` — Envía todos los cambios pendientes a la base de datos en una sola transacción.
- `CreatedAtAction` — Devuelve HTTP 201 con la URL del nuevo recurso en el header `Location`.
- `NoContent()` — Devuelve HTTP 204, estándar para operaciones PUT y DELETE exitosas.

---

### `Controllers/ProductosController.cs`

El controlador de productos tiene una validación extra: antes de crear o actualizar, verifica que la categoría indicada exista.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productos = await _context.Productos.AsNoTracking().ToListAsync();
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Id inválido");

            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (producto == null) return NotFound();
            return Ok(producto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Validar que la categoría exista antes de guardar
                var categoriaExiste = await _context.Categorias
                    .AnyAsync(x => x.Id == producto.CategoriaId);

                if (!categoriaExiste)
                    return BadRequest("La categoría no existe");

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Producto model)
        {
            if (id <= 0) return BadRequest("Id inválido");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            var categoriaExiste = await _context.Categorias
                .AnyAsync(x => x.Id == model.CategoriaId);

            if (!categoriaExiste)
                return BadRequest("La categoría no existe");

            producto.Nombre = model.Nombre;
            producto.Descripcion = model.Descripcion;
            producto.Precio = model.Precio;
            producto.Stock = model.Stock;
            producto.CategoriaId = model.CategoriaId;
            producto.Activo = model.Activo;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Id inválido");

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
```

---

### `Controllers/TiposDocumentosController.cs`

Este controlador valida que el campo `Codigo` sea único antes de crear o actualizar.

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposDocumentosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TiposDocumentosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tipos = await _context.TiposDocumentos.AsNoTracking().ToListAsync();
            return Ok(tipos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Id inválido");

            var tipo = await _context.TiposDocumentos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tipo == null) return NotFound();
            return Ok(tipo);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TipoDocumento tipodocumento)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Validar que el código no esté duplicado
                var existeCodigo = await _context.TiposDocumentos
                    .AnyAsync(x => x.Codigo == tipodocumento.Codigo);

                if (existeCodigo)
                    return BadRequest("El código ya existe");

                _context.TiposDocumentos.Add(tipodocumento);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = tipodocumento.Id }, tipodocumento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TipoDocumento model)
        {
            if (id <= 0) return BadRequest("Id inválido");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tipo = await _context.TiposDocumentos.FindAsync(id);
            if (tipo == null) return NotFound();

            // Al actualizar, excluir el registro actual de la validación de código único
            var existeCodigo = await _context.TiposDocumentos
                .AnyAsync(x => x.Codigo == model.Codigo && x.Id != id);

            if (existeCodigo)
                return BadRequest("El código ya existe");

            tipo.Codigo = model.Codigo;
            tipo.Nombre = model.Nombre;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("Id inválido");

            var tipo = await _context.TiposDocumentos.FindAsync(id);
            if (tipo == null) return NotFound();

            _context.TiposDocumentos.Remove(tipo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
```

---

### `Controllers/TestController.cs`

Controlador auxiliar para verificar que la conexión a la base de datos funciona.

```csharp
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestDbController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestDbController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var canConnect = await _context.Database.CanConnectAsync();
            return Ok(new { connected = canConnect });
        }
    }
}
```

---

## Paso 10 — Crear el frontend

El frontend son páginas HTML planas ubicadas en `wwwroot/`. ASP.NET Core las sirve automáticamente como archivos estáticos gracias a `app.UseStaticFiles()` en `Program.cs`.

Cada página tiene:
- Un formulario para crear y editar registros.
- Una tabla para mostrar los registros existentes.
- Un archivo JS que hace todas las llamadas a la API con `fetch`.

### `wwwroot/categorias.html`

```html
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <title>Categorías</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet">
</head>
<body class="bg-light">
    <div class="container mt-5">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white">
                <h3 class="mb-0">Gestión de Categorías</h3>
            </div>
            <div class="card-body">

                <form id="categoriaForm">
                    <input type="hidden" id="categoriaId">
                    <div class="row">
                        <div class="col-md-10">
                            <input type="text" id="nombre" class="form-control"
                                   placeholder="Nombre categoría" required>
                        </div>
                        <div class="col-md-2 d-grid">
                            <button class="btn btn-success" type="submit">Guardar</button>
                        </div>
                    </div>
                </form>

                <hr>

                <table class="table table-striped table-hover">
                    <thead class="table-dark">
                        <tr>
                            <th>ID</th>
                            <th>Nombre</th>
                            <th width="180">Acciones</th>
                        </tr>
                    </thead>
                    <tbody id="categoriasTable"></tbody>
                </table>

            </div>
        </div>
    </div>
    <script src="/js/categorias.js"></script>
</body>
</html>
```

### `wwwroot/js/categorias.js`

```javascript
document.addEventListener("DOMContentLoaded", () => {

    const apiUrl = "http://localhost:5117/api/categorias";
    const form = document.getElementById("categoriaForm");
    const categoriaId = document.getElementById("categoriaId");
    const nombre = document.getElementById("nombre");
    const tabla = document.getElementById("categoriasTable");

    // Cargar y mostrar todas las categorías en la tabla
    async function cargarCategorias() {
        const response = await fetch(apiUrl);
        const categorias = await response.json();

        tabla.innerHTML = "";
        categorias.forEach(c => {
            tabla.innerHTML += `
                <tr>
                    <td>${c.id}</td>
                    <td>${c.nombre}</td>
                    <td>
                        <button class="btn btn-warning btn-sm"
                            onclick='editar(${JSON.stringify(c)})'>Editar</button>
                        <button class="btn btn-danger btn-sm"
                            onclick="eliminar(${c.id})">Eliminar</button>
                    </td>
                </tr>`;
        });
    }

    // Guardar: si hay ID hace PUT, si no hay ID hace POST
    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const datos = { nombre: nombre.value };
        const id = categoriaId.value;

        if (id) {
            await fetch(`${apiUrl}/${id}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(datos)
            });
        } else {
            await fetch(apiUrl, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(datos)
            });
        }

        form.reset();
        categoriaId.value = "";
        cargarCategorias();
    });

    // Llenar el formulario con los datos del registro a editar
    window.editar = function (categoria) {
        categoriaId.value = categoria.id;
        nombre.value = categoria.nombre;
    };

    // Confirmar y eliminar
    window.eliminar = async function (id) {
        if (!confirm("¿Desea eliminar la categoría?")) return;
        await fetch(`${apiUrl}/${id}`, { method: "DELETE" });
        cargarCategorias();
    };

    cargarCategorias();
});
```

> Los archivos `wwwroot/productos.html`, `wwwroot/tiposdocumentos.html` y sus respectivos JS siguen exactamente el mismo patrón, adaptados a los campos de cada entidad.

> **Importante:** la URL `http://localhost:5117` está escrita directamente en cada archivo JS. Si cambias el puerto en `Properties/launchSettings.json`, debes actualizarla en los tres archivos JS.

---

## Paso 11 — Ejecutar el proyecto

```bash
dotnet run --project WebApplication1/WebApplication1.csproj
```

Verifica que la base de datos conecta:

```
GET http://localhost:5117/api/testdb
→ { "connected": true }
```

---

## URLs disponibles

| Recurso | URL |
|---|---|
| Swagger | `http://localhost:5117/swagger` |
| Frontend — Categorías | `http://localhost:5117/categorias.html` |
| Frontend — Productos | `http://localhost:5117/productos.html` |
| Frontend — Tipos de Documentos | `http://localhost:5117/tiposdocumentos.html` |
| Test conexión BD | `http://localhost:5117/api/testdb` |

---

## Resumen de endpoints

### `GET /api/categorias`
Devuelve todas las categorías.

### `POST /api/categorias`
```json
{ "nombre": "Electrónica" }
```

### `PUT /api/categorias/{id}`
```json
{ "nombre": "Electrónica y Tecnología" }
```

### `DELETE /api/categorias/{id}`
No requiere body.

---

### `GET /api/productos`
Devuelve todos los productos.

### `POST /api/productos`
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
> `categoriaId` debe existir en la tabla `Categorias`.

### `PUT /api/productos/{id}`
Mismo body que POST.

---

### `POST /api/tiposdocumentos`
```json
{ "codigo": "DNI", "nombre": "Documento Nacional de Identidad" }
```
> `codigo` debe ser único.

---

## Solución de problemas

**`{ "connected": false }` en `/api/testdb`**
- Verifica que el servicio SQL Server esté corriendo: `services.msc` → `SQL Server (SQLEXPRESS)`.
- Confirma que el usuario `developer` existe y tiene acceso a `TiendaDB`.
- Revisa la cadena de conexión en `appsettings.json`.

**Las tablas no existen**
- Ejecuta el script SQL completo del [Paso 6](#paso-6--configurar-la-base-de-datos).

**El puerto 5117 ya está en uso**
- Cambia el puerto en `Properties/launchSettings.json` y actualiza la URL en los tres archivos `wwwroot/js/*.js`.

**`dotnet run` no encuentra el proyecto**
- Asegúrate de ejecutar el comando desde la raíz del repositorio:
  ```bash
  dotnet run --project WebApplication1/WebApplication1.csproj
  ```
