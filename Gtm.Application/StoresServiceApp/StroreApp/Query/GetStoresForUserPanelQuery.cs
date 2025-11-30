using ErrorOr;
using Gtm.Contract.SellerContract.Query;
using Gtm.Contract.StoresContract.StoreContract.Query;
using Gtm.Domain.ShopDomain.SellerDomain;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.StoresServiceApp.StroreApp.Query
{
    public record GetStoresForUserPanelQuery(int userId, int sellerId, string filter, int pageId, int take):IRequest<ErrorOr<StoreUserPanelPaging>>;
    public class GetStoresForUserPanelQueryHandler: IRequestHandler<GetStoresForUserPanelQuery, ErrorOr<StoreUserPanelPaging>>
    {
        private readonly IStoreRepository _storeRepository;

        public GetStoresForUserPanelQueryHandler(IStoreRepository storeRepository)
        {
            _storeRepository = storeRepository;
        }

        public async Task<ErrorOr<StoreUserPanelPaging>> Handle(GetStoresForUserPanelQuery request, CancellationToken cancellationToken)
        {
            var res =   _storeRepository.GetStoresByUserIdAsync(request.userId, cancellationToken);

            if (request.sellerId > 0)
                res = res.Where(s => s.SellerId == request.sellerId).OrderByDescending(o => o.Id);

            if (!string.IsNullOrEmpty(request.filter))
                res = res.Where(r => r.Description.Contains(request.filter)).OrderByDescending(o => o.Id);

            StoreUserPanelPaging model = new StoreUserPanelPaging();
            model.GetData(res, request.pageId, request.take, 2);
            model.Filter = request.filter;
            model.SellerId = request.sellerId;
            model.PageTitle = "انبار داری";
            model.Stores = new();
            model.Sellers = new();

            var storeList = await res.Skip(model.Skip).Take(model.Take).ToListAsync(cancellationToken);
            if (storeList.Count > 0)
            {
                model.Stores = storeList.Select(s => new StoreUserPanelQueryModel
                {
                    CreationDate = s.CreateDate.ToPersianDate(),
                    Id = s.Id,
                    SellerId = s.SellerId,
                    SellerName = "" // بعداً ست می‌کنیم
                }).ToList();

                foreach (var store in model.Stores)
                {
                    var seller = await _storeRepository.GetSellerByIdAsync(store.SellerId );
                    store.SellerName = seller?.Title ?? "";
                }
            }

            if (model.SellerId > 0)
            {
                var seller = await _storeRepository.GetSellerByIdAsync(model.SellerId, cancellationToken);
                if (seller == null || seller.UserId != request.userId)
                    return Error.NotFound();

                model.PageTitle = $"انبار داری فروشگاه {seller.Title}";
            }

            var sellers = await _storeRepository.GetSellersByUserIdAsync(request.userId, cancellationToken);
            model.Sellers = sellers.Select(s => new SellerForStoresUserPanelQueryModel
            {
                Id = s.Id,
                SellerName = s.Title
            }).ToList();

            return model;
        }
    }

}
