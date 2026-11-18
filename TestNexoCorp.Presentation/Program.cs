using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestNexoCorp.Application;
using TestNexoCorp.Application.EstrategiaGanaMax;
using TestNexoCorp.Application.PosExpress;
using TestNexoCorp.Domain;
using TestNexoCorp.Infrastructure;

var builder = Host.CreateApplicationBuilder(
    new HostApplicationBuilderSettings
    {
        ContentRootPath = AppContext.BaseDirectory,
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

using var host = builder.Build();

var mediator = host.Services.GetRequiredService<IMediator>();

bool negocioBase = false;
Console.WriteLine(
    $"Por los siguientes 3 meses se usara la estrategia Ganamax, desde {DateTime.Today:dd/MM/yyyy} hasta {DateTime.Today.AddMonths(3):dd/MM/yyyy}," +
    $"Si desea volver a las reglas del negocio base escriba 'si':");

string? respuesta = Console.ReadLine();
if (!string.IsNullOrEmpty(respuesta) && respuesta == "si") negocioBase = true;

ErpProductos erpProducto;
//Pregunta 1: Registro de nuevo producto ERP
if (negocioBase)
{
    //SISTEMA POS EXPRESS: precio = costo / 2, stock recibido tal como viene
    erpProducto = await mediator.Send(
        new RegistrarNuevoErpProductoCommand
        {
            Nombre = "Producto de prueba 4",
            Costo = 2,
            Stock = 100,
        });
}
else
{
    //ESTRATEGIA GANAMAX: precio = costo * 0.80 (80% margen), stock fijado en 20 unidades
    erpProducto = await mediator.Send(
        new RegistrarNuevoErpProductoGanamaxCommand()
        {
            Nombre = "Producto de prueba 4",
            Costo = 2,
            Stock = 100,
        });
}

Console.WriteLine(
    $"ErpProducto registrado: IdProducto={erpProducto.IdProducto}, Stock={erpProducto.Stock}, Costo={erpProducto.Costo}");

//Pregunta 2: Registro de código de barras
var codigosBarras = await mediator.Send(
    new RegistrarCodigoBarrasCommand
    {
        IdProducto = erpProducto.IdProducto,
    });

Console.WriteLine(
    $"Codigo Barra registrado: IdProducto={codigosBarras.IdCodigoBarra}, Unique={erpProducto.UniqueCodigo}");


//Pregunta 3: Registro de categoría + vinculación de producto
var categoriaProducto = await mediator.Send(
    new RegistrarCategoriasProductosCommand
    {
        DescripcionCategoria = "Categoria de prueba 4",
        IdErpProducto = erpProducto.IdProducto,
    });

Console.WriteLine(
    $"Categoria registrada: Categoria descripcion={categoriaProducto.Categorias.Descripcion}, " +
    $"Nombre producto={categoriaProducto.ExpProductos.Nombre}, Id Producto={categoriaProducto.IdProducto}");

//Pregunta 4: Registro de venta
VentasExpress venta;
if (negocioBase)
{
    //REGLA NEGOCIO BASE: 30% de descuento si producto tiene exactamente 1 categoría,
    //stock mínimo verificado: stock - cantidad > 0
    venta = await mediator.Send(
        new RegistrarVentasCommand
        {
            IdProducto = erpProducto.IdProducto,
            Cantidad = 5
        });
}
else
{
    //ESTRATEGIA GANAMAX: 10% de descuento si producto tiene exactamente 1 categoría,
    //regla 4: stock debe quedar > 10 después de la venta
    venta = await mediator.Send(
        new RegistrarVentasGanamaxCommand
        {
            IdProducto = erpProducto.IdProducto,
            Cantidad = 5
        });
}