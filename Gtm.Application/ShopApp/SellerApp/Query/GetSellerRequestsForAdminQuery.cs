using ErrorOr;
using Gtm.Contract.SellerContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.SellerApp.Query
{
    public record GetSellerRequestsForAdminQuery : IRequest<ErrorOr<List<SellerRequestAdminQueryModel>>>;
    public class GetSellerRequestsForAdminQueryHandler : IRequestHandler<GetSellerRequestsForAdminQuery, ErrorOr<List<SellerRequestAdminQueryModel>>>
    {
        private readonly ISellerRepository _sellerRepository;

        public GetSellerRequestsForAdminQueryHandler(ISellerRepository sellerRepository)
        {
            _sellerRepository = sellerRepository;
        }

        public async Task<ErrorOr<List<SellerRequestAdminQueryModel>>> Handle(GetSellerRequestsForAdminQuery request, CancellationToken cancellationToken)
        {
            return await _sellerRepository.GetSellerRequestsForAdmin();
        }
    }
}
