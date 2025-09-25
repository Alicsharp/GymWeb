using ErrorOr;
using Gtm.Contract.TransactionContract.Command;
using Gtm.Domain.TransactionDomian;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utility.Appliation;

namespace Gtm.Application.TransactionServiceApp.Command
{
    public record CreateTransactionCommand(CreateTransaction Command) : IRequest<ErrorOr<long>>;

    public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, ErrorOr<long>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionValidator _validator;

        public CreateTransactionCommandHandler(ITransactionRepository transactionRepository,ITransactionValidator validator)
        {
            _transactionRepository = transactionRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<long>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateCreateAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }
            request.Command.Authority = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
            // ایجاد تراکنش
            var transaction = new Transaction(
                request.Command.UserId,
                request.Command.Price,
                request.Command.Portal,
                request.Command.TransactionFor,
                request.Command.OwnerId,
                request.Command.Authority);

            // ذخیره تراکنش
            var transactionId = await _transactionRepository.CreateAsyncReturnKey(transaction);

            if (transactionId > 0)
            {
                return transactionId;
            }

            return Error.Failure(
                code: "Transaction.CreateFailed",
                description: "خطا در ایجاد تراکنش");
        }
    }
}
