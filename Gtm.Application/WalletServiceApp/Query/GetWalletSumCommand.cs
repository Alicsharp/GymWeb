using ErrorOr;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.WalletServiceApp.Query
{
    public record GetWalletSumCommand(int userId):IRequest<ErrorOr<int>>;
    public class GetWalletSumCommandHandler : IRequestHandler<GetWalletSumCommand, ErrorOr<int>>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletValidator _validator;

        public GetWalletSumCommandHandler(IWalletRepository walletRepository,IWalletValidator validator)
        {
            _walletRepository = walletRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<int>> Handle(GetWalletSumCommand request,CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateGetWalletAmountAsync(request.userId);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            // دریافت موجودی
            var amount =   _walletRepository.GetWalletAmount(request.userId);

            return amount;
        }
    }
}
