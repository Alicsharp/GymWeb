using ErrorOr;
using Gtm.Domain.ShopDomain.SellerDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.SellerApp.Query
{
    public record GetSellerUserIdByIdQuery(int SellerId):IRequest<ErrorOr<int>>;
    public class GetSellerUserIdByIdQueryHandler : IRequestHandler<GetSellerUserIdByIdQuery, ErrorOr<int>>
    {
        private readonly ISellerRepository _sellerRepository;

        public GetSellerUserIdByIdQueryHandler(ISellerRepository sellerRepository)
        {
            _sellerRepository = sellerRepository;
        }

        public async Task<ErrorOr<int>> Handle(GetSellerUserIdByIdQuery request, CancellationToken cancellationToken)
        {
            var seller= await _sellerRepository.GetByIdAsync(request.SellerId);
            return seller.UserId;
        }
    }
}
