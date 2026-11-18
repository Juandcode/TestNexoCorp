using MediatR;
using TestNexoCorp.Domain;

namespace TestNexoCorp.Application.EstrategiaGanaMax
{
    public record RegistrarNuevoErpProductoGanamaxCommand : IRequest<ErpProductos>
    {
        public string Nombre { get; init; }
        public string Observaciones { get; init; } = string.Empty;
        public decimal Costo { get; init; }
        public int Stock { get; init; }
    }
}