using MediatR;
using TestNexoCorp.Application.PosExpress;
using TestNexoCorp.Domain;

namespace TestNexoCorp.Application.EstrategiaGanaMax
{
    public class RegistrarNuevoErpProductoGanamaxHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<RegistrarNuevoErpProductoGanamaxCommand, ErpProductos>
    {
        public async Task<ErpProductos> Handle(
            RegistrarNuevoErpProductoGanamaxCommand request,
            CancellationToken cancellationToken)
        {
            await unitOfWork.BeginTransactionAsync();

            decimal costo = request.Costo;
            decimal precio = costo * 0.80m;
            ExpProductos exProducto = new ExpProductos()
            {
                Activo = true,
                Nombre = request.Nombre,
                Observaciones = request.Observaciones,
                Precio = precio,
                FechaVencimiento = DateTime.Now.AddMonths(10),
            };

            await unitOfWork.ExpProductos.AddAsync(exProducto);

            await unitOfWork.CommitAsync();

            ErpProductos erpProductos = new ErpProductos()
            {
                IdProducto = exProducto.IdProducto,
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