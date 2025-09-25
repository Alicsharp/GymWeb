using ErrorOr;
using Gtm.Domain.TransactionDomian;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.TransactionServiceApp.Command
{

    public record AddTransactionWalletIdCommand(long transactionId, int walletId) : IRequest<ErrorOr<bool>>;
    public class AddTransactionWalletIdCommandHandler : IRequestHandler<AddTransactionWalletIdCommand, ErrorOr<bool>>
    {
        private readonly ITransactionRepository _transactionRepository;

        public AddTransactionWalletIdCommandHandler(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<ErrorOr<bool>> Handle(AddTransactionWalletIdCommand request, CancellationToken cancellationToken)
        {
            Transaction transaction = await _transactionRepository.GetByIdAsync(request.transactionId);
            transaction.AddWalletId(request.walletId);
            return await _transactionRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
