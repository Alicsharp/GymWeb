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
    public record PaymentTransactionCommand(TransactionStatus Status, long Id, string RefId) : IRequest<ErrorOr<Success>>;

    public class PaymentTransactionCommandHandler : IRequestHandler<PaymentTransactionCommand, ErrorOr<Success>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionValidator _validator;

        public PaymentTransactionCommandHandler(ITransactionRepository transactionRepository,ITransactionValidator validator)
        {
            _transactionRepository = transactionRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(PaymentTransactionCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی شناسه تراکنش
            var idValidation = await _validator.ValidatePaymentAsync(request.Id,request.RefId);
            if (idValidation.IsError)
            {
                return idValidation.Errors;
            }

            // اعتبارسنجی RefId
            if (string.IsNullOrWhiteSpace(request.RefId))
            {
                return Error.Validation(
                    code: "Payment.MissingRefId",
                    description: "شناسه مرجع پرداخت نمی‌تواند خالی باشد");
            }

            // دریافت تراکنش از دیتابیس
            var transaction = await _transactionRepository.GetByIdAsync(request.Id);
            if (transaction is null)
            {
                return Error.NotFound(
                    code: "Transaction.NotFound",
                    description: "تراکنش یافت نشد");
            }

            // اعتبارسنجی وضعیت تراکنش فعلی
            if (transaction.Status != TransactionStatus.نا_موفق)
            {
                return Error.Conflict(
                    code: "Payment.AlreadyProcessed",
                    description: "تراکنش قبلاً پردازش شده است");
            }

            // اعمال تغییرات پرداخت
            transaction.Payment(request.Status, request.RefId);

            // ذخیره تغییرات
            var result = await _transactionRepository.SaveChangesAsync(cancellationToken);

            if (!result)
            {
                return Error.Failure(
                    code: "Payment.Failed",
                    description: "عملیات پرداخت با خطا مواجه شد");
            }

            return Result.Success;
        }
    }
}
