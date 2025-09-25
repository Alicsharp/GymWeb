using ErrorOr;
using Gtm.Contract.TransactionContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.TransactionServiceApp.Command
{
    public record GetTransactionForCheckPayment(long id):IRequest<ErrorOr<TransactionViewModel>>;
    public class GetTransactionForCheckPaymentHandelr : IRequestHandler<GetTransactionForCheckPayment, ErrorOr<TransactionViewModel>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionValidator _transactionValidator;

        public GetTransactionForCheckPaymentHandelr(ITransactionRepository transactionRepository, ITransactionValidator transactionValidator)
        {
            _transactionRepository = transactionRepository;
            _transactionValidator = transactionValidator;
        }

        public async Task<ErrorOr<TransactionViewModel>> Handle(GetTransactionForCheckPayment request, CancellationToken cancellationToken)
        {
            var validationresult = await _transactionValidator.ValidateIdAsync(request.id);
            if(validationresult.IsError)
            {
                return validationresult.Errors;
            }
            var t = await _transactionRepository.GetByIdAsync(request.id);
            return new TransactionViewModel(t.Id, t.UserId, t.Price, t.RefId, t.Portal, t.Status, t.TransactionFor, t.OwnerId);
        }
    }
}
