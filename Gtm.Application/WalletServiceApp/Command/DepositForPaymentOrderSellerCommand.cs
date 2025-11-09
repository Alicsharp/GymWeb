using ErrorOr;
using Gtm.Contract.WalletContract.Command;
using Gtm.Domain.UserDomain.WalletAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.WalletServiceApp.Command
{
    /// <summary>
    /// کامند برای واریز وجه به حساب فروشنده پس از پرداخت سفارش
    /// </summary>
    public record DepositForPaymentOrderSellerCommand(CreateWallet Model)
        : IRequest<ErrorOr<Success>>;
    public class DepositForPaymentOrderSellerCommandHandler
    : IRequestHandler<DepositForPaymentOrderSellerCommand, ErrorOr<Success>>
    {
        private readonly IWalletRepository _walletRepository;
        // (فرض می‌کنیم ValidationMessages یک کلاس static با پیام‌های خطا است)

        public DepositForPaymentOrderSellerCommandHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<ErrorOr<Success>> Handle(DepositForPaymentOrderSellerCommand request, CancellationToken cancellationToken)
        {
            var command = request.Model;

            // 1. ایجاد انتیتی با استفاده از Factory Method
            Wallet wallet = Wallet.DepositForPaymentOrderSeller(
                command.UserId,
                command.Price,
                command.Description
            );

            // 2. ایجاد و ذخیره در دیتابیس
            await _walletRepository.AddAsync(wallet);
            if(await _walletRepository.SaveChangesAsync())
            {
                // به جای: return new(true);
                return Result.Success;
            }

            // 3. مدیریت خطای ذخیره‌سازی
            // به جای: return new(false, ValidationMessages.SystemErrorMessage);
            return Error.Failure(description: ValidationMessages.SystemErrorMessage);
        }
    }
}
