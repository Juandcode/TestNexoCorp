using MediatR;
using TestNexoCorp.Domain;

namespace TestNexoCorp.Application.PosExpress
{
    public class RegistrarCategoriasProductosHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<RegistrarCategoriasProductosCommand, ProductosCategorias>
    {
        public async Task<ProductosCategorias> Handle(
            RegistrarCategoriasProductosCommand request,
            CancellationToken cancellationToken)
        {
            await unitOfWork.BeginTransactionAsync();

            Categorias categoria = new Categorias()
            {
                Descripcion = request.DescripcionCategoria,
            };
            await unitOfWork.Categorias.AddAsync(categoria);

            await unitOfWork.CommitAsync();

            ProductosCategorias productosCategorias = new ProductosCategorias()
            {
                IdProducto = request.IdErpProducto,
                IdCategoria = categoria.IdCategoria,
            };
            await unitOfWork.ProductosCategorias.AddAsync(productosCategorias);

            await unitOfWork.CommitAsync();

            await unitOfWork.CommitTransactionAsync();
            
            return productosCategorias;
        }
    }
}