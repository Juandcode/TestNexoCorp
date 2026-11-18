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
//Pregunta 1
if (negocioBase)
{
//SISTEMA POS EXPRESS
    erpProducto = await mediator.Send(
        new RegistrarNuevoErpProductoCommand
        {
            Nombre = "Producto de prueba3",
            Costo = 2,
            Stock = 100,
        });
}
else
{
    //ESTRATEGIA GANAMAX
    erpProducto = await mediator.Send(
        new RegistrarNuevoErpProductoGanamaxCommand()
        {
            Nombre = "Producto de prueba3",
            Costo = 2,
            Stock = 100,
        });
}

Console.WriteLine(
    $"ErpProducto registrado: IdProducto={erpProducto.IdProducto}, Stock={erpProducto.Stock}, Costo={erpProducto.Costo}");

//Pregunta 2
var codigosBarras = await mediator.Send(
    new RegistrarCodigoBarrasCommand
    {
        IdProducto = erpProducto.IdProducto,
    });

Console.WriteLine(
    $"Codigo Barra registrado: IdProducto={codigosBarras.IdCodigoBarra}, Unique={erpProducto.UniqueCodigo}");


//Pregunta 3
var categoriaProducto = await mediator.Send(
    new RegistrarCategoriasProductosCommand
    {
        DescripcionCategoria = "Categoria de prueba",
        IdErpProducto = erpProducto.IdProducto,
    });

Console.WriteLine(
    $"Categoria registrada: Categoria descripcion={categoriaProducto.Categorias.Descripcion}, " +
    $"Nombre producto={categoriaProducto.ExpProductos.Nombre}, Id Producto={categoriaProducto.IdProducto}");

//Pregunta 4 

var venta = await mediator.Send(
    new RegistrarVentasCommand
    {
        IdProducto = erpProducto.IdProducto,
        Cantidad = 5
    });