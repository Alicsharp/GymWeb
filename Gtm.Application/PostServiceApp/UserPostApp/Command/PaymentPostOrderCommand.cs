using ErrorOr;
using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Contract.PostContract.UserPostContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.UserPostApp.Command
{
    public record PaymentPostOrderCommand(PaymentPostModel Command) : IRequest<ErrorOr<Success>>;
    public class PaymentPostOrderCommandHandler : IRequestHandler<PaymentPostOrderCommand, ErrorOr<Success>>
    {
        private readonly IPostOrderRepo _postOrderRepository;
        private readonly IPackageRepo _packageRepository;
        private readonly IUserPostRepo _userPostRepository;
        private readonly IPaymentOrderValidation  _validationService;

        public PaymentPostOrderCommandHandler(IPostOrderRepo postOrderRepository,IPackageRepo packageRepository,IUserPostRepo userPostRepository,IPaymentOrderValidation  validationService)
        {
            _postOrderRepository = postOrderRepository;
            _packageRepository = packageRepository;
            _userPostRepository = userPostRepository;
            _validationService = validationService;
        }

        public async Task<ErrorOr<Success>> Handle(
            PaymentPostOrderCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی اولیه
                var commandValidation = await _validationService.ValidatePaymentCommandAsync(request.Command);
                if (commandValidation.IsError)
                    return commandValidation.Errors;

                // دریافت سفارش
                var postOrder = await _postOrderRepository.GetPostOrderNotPaymentForUserAsync(request.Command.UserId);

                // اعتبارسنجی سفارش
                var orderValidation = await _validationService.ValidatePostOrderAsync(request.Command, postOrder);
                if (orderValidation.IsError)
                    return orderValidation.Errors;

                var validatedOrder = orderValidation.Value;

                // اعتبارسنجی پکیج
                var packageValidation = await _validationService.ValidatePackageAsync(validatedOrder.PackageId);
                if (packageValidation.IsError)
                    return packageValidation.Errors;

                var package = packageValidation.Value;

                // اعتبارسنجی پست کاربر
                var userPostValidation = await _validationService.ValidateUserPostAsync(request.Command.UserId);
                if (userPostValidation.IsError)
                    return userPostValidation.Errors;

                var userPost = userPostValidation.Value;

                // عملیات پرداخت
                validatedOrder.SuccessPayment(request.Command.TransactionId);
                userPost.CountPlus(package.Count);

                // ذخیره تغییرات
                var saveResult = await _userPostRepository.SaveChangesAsync(cancellationToken);
                if (!saveResult)
                    return Error.Failure("Payment.SaveFailed", "خطا در ذخیره‌سازی پرداخت");

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Payment.UnexpectedError",
                    description: $"خطای غیرمنتظره در پردازش پرداخت: {ex.Message}");
            }
        }
    }
}
