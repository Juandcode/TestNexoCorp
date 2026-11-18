using MediatR;
using TestNexoCorp.Domain;

namespace TestNexoCorp.Application.PosExpress
{
    public class RegistrarVentasHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<RegistrarVentasCommand, VentasExpress>
    {
        public async Task<VentasExpress> Handle(
            RegistrarVentasCommand request,
            CancellationToken cancellationToken)
        {
            await unitOfWork.BeginTransactionAsync();

            // ErpProductos? producto = await unitOfWork.ErpProductos.GetByIdAsync(request.IdProducto);
            // if (producto == null) throw new Exception("Producto no encontrado.");

            ExpProductos? expProducto = await unitOfWork.ExpProductos.GetByIdAsync(request.IdProducto);

            if (expProducto == null || expProducto.ErpProductos == null) throw new Exception("Producto no encontrado.");


            //regla de negocio 2
            var uniqueProducto = expProducto.ErpProductos.UniqueCodigo;

            //regla de negocio 3
            var allCategoriasProducto =
                await unitOfWork.ProductosCategorias.GetAllAsync(catProd => catProd.IdProducto == request.IdProducto);
            //var allCategoriasProducto = (await unitOfWork.ProductosCategorias.GetAllAsync()).ToList();
            //var categoriasProducto = allCategoriasProducto.Where(catProd => catProd.IdProducto == expProducto.IdProducto);

            decimal descuento = 0;
            if (allCategoriasProducto.Count() == 1)
            {
                descuento = expProducto.Precio * 0.3m;
            }
            
            //int stockaVender = 3;
            //regla de negocio 4
            if (expProducto.ErpProductos.Stock <= request.Cantidad) throw new Exception("No hay stock suficiente");


            var total = (expProducto.Precio * request.Cantidad) - descuento;

            VentasExpress ventasExpress = new VentasExpress()
            {
                IdProducto = expProducto.IdProducto,
                UniqueProducto = uniqueProducto,
                Precio = expProducto.Precio,
                Cantidad = request.Cantidad,
                Cliente = "Diego",
                Descuento = descuento,
                Fecha = DateTime.Now,
                Producto = expProducto.Nombre,
                Total = total,
            };
            await unitOfWork.VentasExpress.AddAsync(ventasExpress);

            expProducto.ErpProductos.Stock -= request.Cantidad;

            unitOfWork.ExpProductos.Update(expProducto);

            await unitOfWork.CommitTransactionAsync();
            return ventasExpress;
        }
    }
}