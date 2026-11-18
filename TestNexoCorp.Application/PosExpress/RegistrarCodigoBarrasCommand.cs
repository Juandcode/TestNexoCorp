using MediatR;
using TestNexoCorp.Domain;

namespace TestNexoCorp.Application.PosExpress
{
    public record RegistrarCodigoBarrasCommand : IRequest<CodigosBarras>
    {
        public Guid IdProducto { get; init; }
    }
}