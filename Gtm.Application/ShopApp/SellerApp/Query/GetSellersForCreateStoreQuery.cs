using ErrorOr;
using Gtm.Contract.SellerContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.SellerApp.Query
{
    public record GetSellersForCreateStoreQuery(int userId) : IRequest<ErrorOr<List<SellerForStoresUserPanelQueryModel>>>;
    public class GetSellersForCreateStoreQueryHandler : IRequestHandler<GetSellersForCreateStoreQuery, ErrorOr<List<SellerForStoresUserPanelQueryModel>>>
    {
        private readonly ISellerRepository _sellerRepository;

        public GetSellersForCreateStoreQueryHandler(ISellerRepository sellerRepository)
        {
            _sellerRepository = sellerRepository;
        }

        public async Task<ErrorOr<List<SellerForStoresUserPanelQueryModel>>> Handle(GetSellersForCreateStoreQuery request, CancellationToken cancellationToken)
        {
            var query =   _sellerRepository.QueryBy(s => s.UserId == request.userId);

            return await query
                .Select(s => new SellerForStoresUserPanelQueryModel
                {
                    Id = s.Id,
                    SellerName = s.Title
                })
                .ToListAsync();
        }
    }
}
