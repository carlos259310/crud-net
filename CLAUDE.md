# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the project
dotnet run --project WebApplication1/WebApplication1.csproj

# Build only
dotnet build WebApplication1/WebApplication1.csproj

# Restore packages
dotnet restore WebApplication1/WebApplication1.csproj
```

There are no automated tests in this project.

## Architecture

ASP.NET Core (.NET 10) Web API with a vanilla JS + Bootstrap frontend served as static files.

**Data flow:** `wwwroot/js/*.js` → HTTP fetch → `Controllers/*Controller.cs` → `Data/AppDbContext.cs.cs` → SQL Server

### Key constraints

- **No EF Core migrations.** Tables must be created manually in SQL Server before running. The database is `TiendaDB` on `localhost\SQLEXPRESS` with login `developer / 123456`. See README for the setup SQL.
- **JS API URLs are hardcoded** to `http://localhost:5117` in all three files under `wwwroot/js/`. If the port changes (via `Properties/launchSettings.json`), all three JS files must be updated.

### Models and business rules

- `Categoria` — basic entity (Id, Nombre).
- `Producto` — references `CategoriaId`; POST and PUT validate that the category exists before saving.
- `TipoDocumento` — mapped to table `TiposDocumentos`; `Codigo` must be unique (enforced in controller logic, not DB constraint).

### Controllers pattern

All three resource controllers follow the same structure: constructor-injected `AppDbContext`, async EF Core queries with `AsNoTracking()` on reads, and manual property mapping on updates (no AutoMapper). `TestDbController` (`GET /api/testdb`) returns `{ "connected": true/false }` to verify DB connectivity.

### Static frontend

Three standalone HTML pages in `wwwroot/` (`categorias.html`, `productos.html`, `tiposdocumentos.html`), each backed by a matching JS file. No build step — plain Bootstrap 5 from CDN and vanilla `fetch`.
