using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.TransactionServiceApp.Command
{
    public record TransactionPaymentCommand(TransactionStatus status, long id, string refId) : IRequest<ErrorOr<bool>>;
    public class TransactionPaymentCommandHandler : IRequestHandler<TransactionPaymentCommand, ErrorOr<bool>>
    {
        public readonly ITransactionRepository _transactionRepository;

        public TransactionPaymentCommandHandler(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<ErrorOr<bool>> Handle(TransactionPaymentCommand request, CancellationToken cancellationToken)
        {
            var transaction = await _transactionRepository.GetByIdAsync(request.id);
            transaction.Payment(request.status, request.refId);
            return await _transactionRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
