using MediatR;
using TestNexoCorp.Domain;

namespace TestNexoCorp.Application.EstrategiaGanaMax
{
    public record RegistrarVentasGanamaxCommand: IRequest<VentasExpress>
    {
        //reglas de negocio 1: solo un producto a vender
        public Guid IdProducto { get; init; }
        
        public int Cantidad { get; init; }
    }
}