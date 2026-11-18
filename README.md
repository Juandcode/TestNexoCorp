# TestNexoCorp

Solución en **.NET 10** que implementa un dominio de tipo **POS Express** (punto de venta): productos, categorías, códigos de barras y ventas. Está estructurada siguiendo el patrón de **Arquitectura Limpia (Clean Architecture)** con **CQRS vía MediatR**, **Entity Framework Core 10** y **SQL Server** como base de datos.

## Arquitectura

```
┌─────────────────────────────┐
│      Presentation           │  App de consola (composition root)
│  TestNexoCorp.Presentation  │  Host genérico + DI + MediatR
└──────────┬──────────────────┘
           │
┌──────────▼──────────────────┐      ┌──────────────────────────────┐
│      Application            │◄─────┤       Infrastructure         │
│  TestNexoCorp.Application   │ impl │  TestNexoCorp.Infrastructure │
│  Comandos + Handlers (CQRS) │      │  EF Core / Repos / UoW       │
└──────────┬──────────────────┘      └──────────────────────────────┘
           │
┌──────────▼──────────────────┐
│         Domain              │  Entidades puras, sin dependencias
│    TestNexoCorp.Domain      │
└─────────────────────────────┘
```

| Proyecto | Rol | Dependencias |
|---|---|---|
| **Domain** | Entidades del dominio (productos, categorías, ventas, códigos de barras). No referencia nada. | Ninguna |
| **Application** | Casos de uso con MediatR (comandos + handlers), abstracciones `IGenericRepository<T>` e `IUnitOfWork`. | Domain |
| **Infrastructure** | `ApplicationDbContext` (EF Core + SQL Server), repositorios genéricos, `UnitOfWork`, migraciones y factory de design-time para `dotnet ef`. | Application |
| **Presentation** | Punto de entrada (consola). Configura el host, registra DI y ejecuta los escenarios de demostración enviando comandos con `IMediator`. | Application, Infrastructure |

## Tecnologías

- **.NET 10** (todas las capas en `net10.0`)
- **MediatR 12.5** — patrón CQRS (Command / Handler)
- **Entity Framework Core 10** con proveedor **SQL Server**
- **Microsoft.Extensions.Hosting** — Generic Host (app de consola, no web)
- Nullable reference types e implicit usings habilitados en toda la solución

## Modelo de dominio

Entidades (namespace `TestNexoCorp.Domain`, PKs `Guid`):

| Entidad | Descripción |
|---|---|
| `ExpProductos` | Producto del POS: nombre, precio, activo, fecha de vencimiento, observaciones. FK a `TiposProductos`. |
| `ErpProductos` | Información del lado ERP: costo, stock, fecha de registro. Comparte PK (1:1) con `ExpProductos`. |
| `TiposProductos` | Tipos de producto (descripción). |
| `Categorias` | Categorías con auto-referencia (`IdCategoriaPadre`) para jerarquías. |
| `ProductosCategorias` | Vínculo producto ↔ categoría con índice único compuesto. |
| `CodigosBarras` | Códigos de barras asociados a un producto (1:1, índice único). |
| `VentasExpress` | Venta: cliente, producto, cantidad, precio, descuento y total. |

## Casos de uso (CQRS)

Los comandos y handlers viven en `TestNexoCorp.Application/PosExpress`:

| Comando | Handler | Lógica |
|---|---|---|
| `RegistrarNuevoErpProductoCommand` | `RegistrarNuevoErpProductoHandler` | Crea el producto EXP (precio = costo / 2, vencimiento +10 meses) y su registro ERP con costo/stock. |
| `RegistrarCodigoBarrasCommand` | `RegistrarCodigoBarrasHandler` | Genera un código de barras activo para un producto dentro de una transacción. |
| `RegistrarCategoriasProductosCommand` | `RegistrarCategoriasProductosHandler` | Crea la categoría y vincula el producto a la categoría. |
| `RegistrarVentasCommand` | `RegistrarVentasHandler` | Registra una venta con reglas de negocio: snapshot de `UniqueCodigo`, **descuento del 30%** si el producto pertenece a exactamente una categoría, y validación de **stock suficiente**. |

Reglas de negocio implementadas en las ventas:
1. La venta es de un solo producto por transacción.
2. Se guarda el snapshot `UniqueProducto` del lado ERP.
3. 30% de descuento si el producto tiene exactamente una categoría.
4. Se rechaza la venta si no hay stock suficiente.

## Configuración de infraestructura

- `ApplicationDbContext` con los 7 `DbSet` y configuración de relaciones inline en `OnModelCreating` (delete behaviors `Restrict`/`Cascade`, índices únicos).
- `GenericRepository<T>` + `UnitOfWork` con soporte de transacciones explícitas (`BeginTransactionAsync` / `CommitTransactionAsync` / `RollbackTransactionAsync`).
- `DesignTimeDbContextFactory` que busca `appsettings.json` del proyecto Presentation para que `dotnet ef migrations` funcione desde Infrastructure.
- `AddApplication()` y `AddInfrastructure(IConfiguration)` extienden `IServiceCollection` para el registro de DI.

## Base de datos

SQL Server (por defecto una instancia local `localhost\SQLEXPRESS`, base de datos `Nexocorp1`, conexión por Windows/Trusted Connection). La cadena se define en `TestNexoCorp.Presentation/appsettings.json` bajo `ConnectionStrings:DefaultConnection`.

Migración inicial: `20261117204108_Inicial`.

```bash
# Crear una nueva migración
dotnet ef migrations add <Nombre> --project TestNexoCorp.Infrastructure

# Aplicar migraciones
dotnet ef database update --project TestNexoCorp.Infrastructure
```

## Cómo ejecutar

```bash
# Restaurar y compilar
dotnet build TestNexoCorp.sln

# Ejecutar la demo (Presentation es el proyecto de inicio)
dotnet run --project TestNexoCorp.Presentation
```

La app de consola ejecuta tres escenarios de demostración mediante MediatR: registrar un producto ERP, generar su código de barras y asignarle una categoría.

## Estructura del repositorio

```
TestNexoCorp.sln
├── TestNexoCorp.Domain/            # Entidades del dominio
├── TestNexoCorp.Application/       # Comandos, handlers, abstracciones (CQRS)
│   └── PosExpress/
├── TestNexoCorp.Infrastructure/    # EF Core, repos, UnitOfWork, migraciones
│   └── Migrations/
└── TestNexoCorp.Presentation/      # Consola + DI + appsettings.json
```
