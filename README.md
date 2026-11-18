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

## Estrategia Ganamax

Durante **3 meses** (desde la fecha actual hasta 3 meses después), la aplicación opera bajo la estrategia **Ganamax** en lugar de las reglas del negocio base. Al iniciar, el usuario se le pregunta si desea volver a las reglas base; si no, todas las operaciones usan los parámetros Ganamax.

### Diferencias clave: Negocio Base vs. Estrategia Ganamax

| Aspecto | Negocio Base | Estrategia Ganamax |
|---|---|---|
| **Precio de venta** | `Precio = Costo / 2` (50% margen) | `Precio = Costo × 0.80` (20% margen, precio más alto al cliente) |
| **Stock inicial** | Valor ingresado en el comando | **Fijado en 20 unidades** (siempre) |
| **Descuento por categoría** | 30% si el producto tiene exactamente 1 categoría | 10% si el producto tiene exactamente 1 categoría |
| **Regla de stock (Negocio 4)** | `stock - cantidad > 0` (solo stock suficiente) | `stock - cantidad > 10` (mínimo 11 unidades deben quedar) |
| **Comandos usados** | `RegistrarNuevoErpProductoCommand`, `RegistrarVentasCommand` | `RegistrarNuevoErpProductoGanamaxCommand`, `RegistrarVentasGanamaxCommand` |

---

## Comandos y Reglas de Negocio

Cada comando encapsula los datos necesarios para su operación. A continuación, los valores y su significado:

### 1. `RegistrarNuevoErpProductoCommand` / `RegistrarNuevoErpProductoGanamaxCommand`

| Propiedad | Tipo | Negocio Base | Estrategia Ganamax | Significado |
|---|---|---|---|---|
| `Nombre` | `string` | Ingresado por usuario | Ingresado por usuario | Nombre descriptivo del producto |
| `Observaciones` | `string` | `string.Empty` por defecto | `string.Empty` por defecto | Comentarios adicionales |
| `Costo` | `decimal` | Costo real del producto | Costo real del producto | Precio de adquisición del producto |
| `Stock` | `int` | Valor ingresado (ej. 100) | Valor ingresado, pero **handler fija en 20** | Inventario inicial disponible |

**En el handler Ganamax:** El `Stock` enviado en el comando es recibido pero **sobrescrito a 20** unidades siempre, independientemente del valor enviado. Esto garantiza un stock de seguridad inicial bajo la estrategia Ganamax.

---

### 2. `RegistrarCodigoBarrasCommand`

| Propiedad | Tipo | Significado |
|---|---|---|
| `IdProducto` | `Guid` | Identificador del producto al que se asignará el código de barras |

*No tiene propiedades adicionales; la lógica de generación del `UniqueCodigo` y la regla de activación (`Activo = true`) está en el handler.*

---

### 3. `RegistrarCategoriasProductosCommand`

| Propiedad | Tipo | Significado |
|---|---|---|
| `IdErpProducto` | `Guid` | ID del producto ERP al que se asignará la categoría |
| `DescripcionCategoria` | `string` | Nombre/descripción de la nueva categoría |

*El handler crea la categoría si no existe y registra el vínculo producto-categoría.*

---

### 4. `RegistrarVentasCommand` / `RegistrarVentasGanamaxCommand`

| Propiedad | Tipo | Negocio Base | Estrategia Ganamax | Significado |
|---|---|---|---|---|
| `IdProducto` | `Guid` | ID del producto a vender | ID del producto a vender | Producto sobre el cual se registra la venta |
| `Cantidad` | `int` | `5` (fijo en demo) | `5` (fijo en demo) | Cantidad de unidades que se vende |

**Reglas de negocio aplicadas en ambos handlers:**

| Regla | Descripción | Negocio Base | Ganamax |
|---|---|---|---|
| **Regla 1** | Venta de un solo producto por transacción | ✅ | ✅ |
| **Regla 2** | Snapshot de `UniqueProducto` del ERP | ✅ | ✅ |
| **Regla 3** | Descuento si tiene exactamente 1 categoría | 30% sobre el total | 10% sobre el total |
| **Regla 4** | Stock mínimo después de la venta | `stock - cantidad > 0` | `stock - cantidad > 10` (mínimo 11 unidades) |

**Fórmula del total:**
```
Total = (Precio × Cantidad) - Descuento
```

Donde:
- `Precio` viene del producto (costo/2 o costo×0.80 según estrategia)
- `Descuento` = `Precio × Cantidad × 0.30` (base) o `Precio × Cantidad × 0.10` (Ganamax) si hay exactamente 1 categoría
- Si no hay exactamente 1 categoría, `Descuento = 0`

---

## Flujo de Ejecución (Presentation)

Al correr la aplicación (`dotnet run --project TestNexoCorp.Presentation`):

1. Se crea el host y se pregunta al usuario si desea volver a las reglas del **negocio base** o usar la estrategia **Ganamax** por 3 meses.
2. Si **no** escribe "si", se activa la estrategia Ganamax por 3 meses.
3. **Pregunta 1:** Registra un nuevo producto ERP usando el comando correspondiente a la estrategia activa.
4. **Pregunta 2:** Genera un código de barras para el producto registrado.
5. **Pregunta 3:** Crea una categoría y vincula el producto a ella.
6. **Pregunta 4:** Registra una venta de 5 unidades con las reglas de la estrategia activa (descuento y validación de stock).

---

## Migraciones y Base de Datos

- SQL Server (por defecto `localhost\SQLEXPRESS`, base `Nexocorp1`, Windows Authentication).
- Cadena de conexión en `TestNexoCorp.Presentation/appsettings.json` bajo `ConnectionStrings:DefaultConnection`.
- Migración inicial: `20261117204108_Inicial`.

```bash
dotnet ef migrations add <Nombre> --project TestNexoCorp.Infrastructure
dotnet ef database update --project TestNexoCorp.Infrastructure
```