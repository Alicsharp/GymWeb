using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.WalletServiceApp.Query
{
    public record GetWalletForCheckPaymentQuery(int id) : IRequest<ErrorOr<WalletForCheckPayemntQueryModel>>;
    public class GetWalletForCheckPaymentQueryHandler : IRequestHandler<GetWalletForCheckPaymentQuery, ErrorOr<WalletForCheckPayemntQueryModel>>
    {
        private readonly IWalletRepository _walletRepository;

        public GetWalletForCheckPaymentQueryHandler(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<ErrorOr<WalletForCheckPayemntQueryModel>> Handle(GetWalletForCheckPaymentQuery request, CancellationToken cancellationToken)
        {
            var wallet = await _walletRepository.GetByIdAsync(request.id);
            var walletquerymodel = new WalletForCheckPayemntQueryModel(wallet.Id, wallet.Type, wallet.IsPay, wallet.Description);
            return walletquerymodel;
        }
    }
}
