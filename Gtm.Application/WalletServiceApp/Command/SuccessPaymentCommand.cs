using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.WalletServiceApp.Command
{
    public record SuccessPaymentCommand(int Id) : IRequest<ErrorOr<Success>>;

    public class SuccessPaymentCommandHandler : IRequestHandler<SuccessPaymentCommand, ErrorOr<Success>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletValidator _validator;

        public SuccessPaymentCommandHandler(IWalletRepository walletRepository,IWalletValidator validator)
        {
            _walletRepository = walletRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(
            SuccessPaymentCommand request,
            CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidatePaymentSuccessAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // دریافت کیف پول
            var wallet = await _walletRepository.GetByIdAsync(request.Id);

            // تغییر وضعیت
            wallet.PaymentSuccess();

            // ذخیره تغییرات
                _walletRepository.Update(wallet);
            var result=await _walletRepository.SaveChangesAsync(cancellationToken); 

            return result ?
                Result.Success :
                Error.Failure("Wallet.PaymentFailed", "تغییر وضعیت پرداخت با خطا مواجه شد.");
        }
    }
}
