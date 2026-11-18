using MediatR;
using TestNexoCorp.Domain;

namespace TestNexoCorp.Application.PosExpress
{
    public record RegistrarCategoriasProductosCommand: IRequest<ProductosCategorias>
    {
        public Guid IdErpProducto { get; init; }
        public string DescripcionCategoria { get; init; }
    }
}