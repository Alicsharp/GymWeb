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
    public record DepositByAdminCommand(CreateWallet command) : IRequest<ErrorOr<Success>>;

    public class DepositByAdminCommandHandler : IRequestHandler<DepositByAdminCommand, ErrorOr<Success>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletValidator _validator;

        public DepositByAdminCommandHandler(IWalletRepository walletRepository,IWalletValidator validator)
        {
            _walletRepository = walletRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(DepositByAdminCommand request,CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateDepositByAdminAsync(request.command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // ایجاد موجودیت Wallet
            Wallet wallet = Wallet.DepositByAdmin(
                request.command.UserId,
                request.command.Price,
                request.command.Description);

            // ذخیره در دیتابیس
             await _walletRepository.AddAsync(wallet);
            var result= await _walletRepository.SaveChangesAsync(cancellationToken);

            return result ?
                Result.Success :
                Error.Failure("Wallet.DepositFailed", "عملیات واریز با خطا مواجه شد.");
        }
    }
}
