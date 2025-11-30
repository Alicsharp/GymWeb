using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.PostServiceApp.StateApp;
using Gtm.Application.SeoApp;
using Gtm.Application.UserApp;
using Gtm.Contract.SellerContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.ShopApp.SellerApp.Query
{
    public record GetSellersForAdminQuery(int pageId, int take, string filter) : IRequest<ErrorOr<SellerAdminPaging>>;
    public class GetSellersForAdminQueryHandler : IRequestHandler<GetSellersForAdminQuery, ErrorOr<SellerAdminPaging>>
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IUserRepo _userRepository;
        private readonly IStateRepo _stateRepository;
        private readonly ICityRepo _cityRepository;
        private readonly ISellerValidator _sellerValidator;

        public GetSellersForAdminQueryHandler(ISellerRepository sellerRepository, IUserRepo userRepository, IStateRepo stateRepository, ICityRepo cityRepository, ISellerValidator sellerValidator)
        {
            _sellerRepository = sellerRepository;
            _userRepository = userRepository;
            _stateRepository = stateRepository;
            _cityRepository = cityRepository;
            _sellerValidator = sellerValidator;
        }

        public async Task<ErrorOr<SellerAdminPaging>> Handle(GetSellersForAdminQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _sellerValidator.ValidateGetSellersForAdminQuery(request.pageId, request.take, request.filter);
            if (validationResult.IsError)
                return validationResult.Errors;
            var query = _sellerRepository.QueryBy(s => s.Status == SellerStatus.درخواست_تایید_شده);

            // اعمال فیلتر اگر وجود داشته باشد
            if (!string.IsNullOrEmpty(request.filter))
            {
                query = query.Where(r => r.Title.ToLower() == request.filter.ToLower());
            }

            // ایجاد مدل صفحه‌بندی
            SellerAdminPaging model = new();
            model.GetData(query, request.pageId, request.take, 2);
            model.Filter = request.filter;
            model.Sellers = new List<SellerAdminQueryModel>();

            // اگر داده‌ای وجود دارد
            if (await query.AnyAsync())
            {
                // دریافت اطلاعات پایه فروشندگان
                var sellers = await query
                    .Select(s => new SellerAdminQueryModel
                    {
                        CityId = s.CityId,
                        CityName = "",
                        CreateDate = s.CreateDate.ToPersianDate(),
                        Email = s.Email,
                        Id = s.Id,
                        ImageName = s.ImageName,
                        Phone1 = s.Phone1,
                        StateId = s.StateId,
                        Title = s.Title,
                        UpdateDate = s.UpdateDate.ToPersianDate(),
                        UserId = s.UserId,
                        UserName = ""
                    })
                    .ToListAsync();

                // دریافت اطلاعات کاربران، استان‌ها و شهرها به صورت بهینه
                var userIds = sellers.Select(s => s.UserId).Distinct();
                var stateIds = sellers.Select(s => s.StateId).Distinct();
                var cityIds = sellers.Select(s => s.CityId).Distinct();

                var users = await _userRepository.QueryBy(u => userIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id);

                var states = await _stateRepository.QueryBy(s => stateIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id);

                var cities = await _cityRepository.QueryBy(c => cityIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id);

                // پر کردن اطلاعات تکمیلی
                foreach (var seller in sellers)
                {
                    if (users.TryGetValue(seller.UserId, out var user))
                    {
                        seller.UserName = string.IsNullOrEmpty(user.FullName)
                            ? user.Mobile
                            : user.FullName;
                    }

                    if (states.TryGetValue(seller.StateId, out var state))
                    {
                        seller.CityName = state.Title;
                    }

                    if (cities.TryGetValue(seller.CityId, out var city))
                    {
                        seller.CityName += " " + city.Title;
                    }
                }

                model.Sellers = sellers;
            }

            return model;
        }
    }
}

