using MediatR;
using TestNexoCorp.Domain;

namespace TestNexoCorp.Application.PosExpress
{
    public class RegistrarNuevoErpProductoHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<RegistrarNuevoErpProductoCommand, ErpProductos>
    {
        public async Task<ErpProductos> Handle(
            RegistrarNuevoErpProductoCommand request,
            CancellationToken cancellationToken)
        {
            await unitOfWork.BeginTransactionAsync();

            decimal costo = request.Costo;
            decimal precio = costo / 2;
            ExpProductos producto = new ExpProductos()
            {
                Activo = true,
                Nombre = request.Nombre,
                Observaciones = request.Observaciones,
                Precio = precio,
                FechaVencimiento = DateTime.Now.AddMonths(10),
            };

            await unitOfWork.ExpProductos.AddAsync(producto);

            await unitOfWork.CommitAsync();

            ErpProductos erpProductos = new ErpProductos()
            {
                IdProducto = producto.IdProducto,
                Costo = costo,
                Stock = request.Stock,
                FechaRegistro = DateTime.Now,
            };

            await unitOfWork.ErpProductos.AddAsync(erpProductos);

            await unitOfWork.CommitAsync();

            await unitOfWork.CommitTransactionAsync();

            return erpProductos;
        }
    }
}