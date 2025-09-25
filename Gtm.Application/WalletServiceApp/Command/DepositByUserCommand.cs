using ErrorOr;
using Gtm.Contract.WalletContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gtm.Application.WalletServiceApp.Command
{
    public record DepositByUserCommand(CreateWalletWithWhy command):IRequest<ErrorOr<int>>;
    public class DepositByUserCommandHandler : IRequestHandler<DepositByUserCommand, ErrorOr<int>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletValidator _validator;

        public DepositByUserCommandHandler(IWalletRepository walletRepository,IWalletValidator validator)
        {
            _walletRepository = walletRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<int>> Handle(DepositByUserCommand request, CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateCreateAsync(request.command);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // انجام عملیات واریز
            return await _walletRepository.DepositByUserAsync(request.command);
        }
    }
}
