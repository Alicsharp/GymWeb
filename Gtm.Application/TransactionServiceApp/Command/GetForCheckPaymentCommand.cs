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
    public record GetForCheckPaymentCommand(string Authority) : IRequest<ErrorOr<TransactionQueryModel>>;

    public class GetForCheckPaymentCommandHandler: IRequestHandler<GetForCheckPaymentCommand, ErrorOr<TransactionQueryModel>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionValidator _validator;

        public GetForCheckPaymentCommandHandler(ITransactionRepository transactionRepository,ITransactionValidator validator)
        {
            _transactionRepository = transactionRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<TransactionQueryModel>> Handle(GetForCheckPaymentCommand request,CancellationToken cancellationToken)
        {
            var validatonresult= await _validator.ValidateAuthorityAsync(request.Authority);    
            if(validatonresult.IsError) 
            {
               return validatonresult.Errors;
            }
            // اعتبارسنجی Authority
            if (string.IsNullOrWhiteSpace(request.Authority))
            {
                return Error.Validation(
                    code: "Payment.MissingAuthority",
                    description: "Authority نمی‌تواند خالی باشد");
            }

            // دریافت تراکنش از دیتابیس
            var transaction = await _transactionRepository.GetByAuthorityAsync(request.Authority);

            if (transaction is null)
            {
                return Error.NotFound(
                    code: "Transaction.NotFound",
                    description: "تراکنش با Authority مشخص شده یافت نشد");
            }

            // مپ کردن به مدل نمایش
            return new TransactionQueryModel(
                transaction.Id,
                transaction.UserId,
                transaction.Price,
                transaction.RefId,
                transaction.Portal,
                transaction.Status,
                transaction.TransactionFor,
                transaction.OwnerId);
        }
    }
}
