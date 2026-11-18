using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestNexoCorp.Application;
using TestNexoCorp.Application.PosExpress;
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

//SISTEMA POS EXPRESS
//Pregunta 1
var erpProducto = await mediator.Send(
    new RegistrarNuevoErpProductoCommand
    {
        Nombre = "Producto de prueba2",
        Costo = 100,
        Stock = 10,
    });

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