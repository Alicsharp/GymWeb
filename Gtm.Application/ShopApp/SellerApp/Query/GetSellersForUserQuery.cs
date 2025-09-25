using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.PostServiceApp.StateApp;
using Gtm.Contract.SellerContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.ShopApp.SellerApp.Query
{
    public record GetSellersForUserQuery(int userId) : IRequest<ErrorOr<List<SellersUserPanelQueryModel>>>;
    public class GetSellersForUserQueryHandler : IRequestHandler<GetSellersForUserQuery, ErrorOr<List<SellersUserPanelQueryModel>>>
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IStateRepo _stateRepository;
        private readonly ICityRepo _cityRepository;
        private readonly ISellerValidator _sellerValidator;

        public GetSellersForUserQueryHandler(ISellerRepository sellerRepository, IStateRepo stateRepository, ICityRepo cityRepository, ISellerValidator sellerValidator)
        {
            _sellerRepository = sellerRepository;
            _stateRepository = stateRepository;
            _cityRepository = cityRepository;
            _sellerValidator = sellerValidator;
        }

        public async Task<ErrorOr<List<SellersUserPanelQueryModel>>> Handle(GetSellersForUserQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _sellerValidator.ValidateGetSellersForUserQueryAsync(request.userId);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }
            var anyRecordExists = await _sellerRepository.CountAsync();
             
          
            // دریافت لیست فروشندگان
            var sellers = await _sellerRepository
                .QueryBy(c => c.UserId == request.userId)
                .Select(r => new SellersUserPanelQueryModel
                {
                    ImageAccept = r.ImageAccept,
                    CityId = r.CityId,
                    CityName = "", // مقدار موقت
                    CreationDate = r.CreateDate.ToPersainDate(),
                    Id = r.Id,
                    ImageName = r.ImageName,
                    Phone1 = r.Phone1,
                    StateId = r.StateId,
                    Status = r.Status,
                    Title = r.Title
                })
                .ToListAsync(cancellationToken);

            // دریافت یکجا تمام شهرها و استان‌های مورد نیاز
            var cityIds = sellers.Select(s => s.CityId).Distinct().ToList();
            var stateIds = sellers.Select(s => s.StateId).Distinct().ToList();

            var cities = await _cityRepository
                .QueryBy(c => cityIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Title);

            var states = await _stateRepository
                .QueryBy(s => stateIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Title);

            // پر کردن نام شهرها و استان‌ها
            foreach (var seller in sellers)
            {
                seller.CityName = $"{states.GetValueOrDefault(seller.StateId, "نامعلوم")} - " +
                                 $"{cities.GetValueOrDefault(seller.CityId, "نامعلوم")}";
            }

            return sellers;
        }
    }
}
