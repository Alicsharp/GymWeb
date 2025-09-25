using ErrorOr;
using Gtm.Contract.WalletContract.Command;
using Gtm.Domain.UserDomain.WalletAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Utility.Appliation;

namespace Gtm.Application.WalletServiceApp.Command
{
    public record WithdrawalCommand(CreateWalletWithWhy Command) : IRequest<ErrorOr<Success>>;

    public class WithdrawalCommandHandler : IRequestHandler<WithdrawalCommand, ErrorOr<Success>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletValidator _validator;

        public WithdrawalCommandHandler(IWalletRepository walletRepository,IWalletValidator validator)
        {
            _walletRepository = walletRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(
            WithdrawalCommand request,
            CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateWithdrawalAsync(request.Command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // ایجاد تراکنش برداشت
            var wallet = Wallet.Withdrawall(
                request.Command.UserId,
                request.Command.Price,
                request.Command.Description,
                request.Command.WalletWhy);

            // ذخیره در دیتابیس
            await _walletRepository.AddAsync(wallet);
            var result = await _walletRepository.SaveChangesAsync(cancellationToken);
            return result ?
                Result.Success :
                Error.Failure("Wallet.WithdrawalFailed", "عملیات برداشت با خطا مواجه شد.");
        }
    }
}
