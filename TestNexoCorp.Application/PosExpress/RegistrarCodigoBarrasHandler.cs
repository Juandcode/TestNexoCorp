using MediatR;
using TestNexoCorp.Domain;

namespace TestNexoCorp.Application.PosExpress
{
    public class RegistrarCodigoBarrasHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<RegistrarCodigoBarrasCommand, CodigosBarras>
    {
        public async Task<CodigosBarras> Handle(
            RegistrarCodigoBarrasCommand request,
            CancellationToken cancellationToken)
        {
            await unitOfWork.BeginTransactionAsync();
            CodigosBarras codigosBarras = new CodigosBarras()
            {
                IdProducto = request.IdProducto,
                Activo = true,
            };

            await unitOfWork.CodigosBarras.AddAsync(codigosBarras);

            await unitOfWork.CommitTransactionAsync();
            
            return codigosBarras;
        }
    }
}