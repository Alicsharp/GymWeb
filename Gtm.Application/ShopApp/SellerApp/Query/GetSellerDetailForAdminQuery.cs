using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.PostServiceApp.StateApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Application.UserApp;
using Gtm.Contract.SellerContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.ShopApp.SellerApp.Query
{
    public record GetSellerDetailForAdminQuery(int id) : IRequest<ErrorOr<SellerDetailAdminQueryModel>>;
    public class GetSellerDetailForAdminQueryHandler : IRequestHandler<GetSellerDetailForAdminQuery, ErrorOr<SellerDetailAdminQueryModel>>
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IUserRepo _userRepository;
        private readonly IStateRepo _stateRepository;
        private readonly ICityRepo _cityRepository;
        private readonly ISellerValidator _sellerValidator;

        public GetSellerDetailForAdminQueryHandler(ISellerRepository sellerRepository, IUserRepo userRepository, IStateRepo stateRepository, ICityRepo cityRepository, ISellerValidator sellerValidator)
        {
            _sellerRepository = sellerRepository;
            _userRepository = userRepository;
            _stateRepository = stateRepository;
            _cityRepository = cityRepository;
            _sellerValidator = sellerValidator;
        }

        public async Task<ErrorOr<SellerDetailAdminQueryModel>> Handle(GetSellerDetailForAdminQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _sellerValidator.ValidateId(request.id);
            if (validationResult.IsError)
                return validationResult.Errors;
            // دریافت اطلاعات فروشنده
            var seller = await _sellerRepository.GetByIdAsync(request.id);
            if (seller == null)
                return Error.NotFound("seller", "فروشنده پیدا نشد");

            // دریافت اطلاعات کاربر
            var user = await _userRepository.GetByIdAsync(seller.UserId);
            if (user == null)
                return Error.NotFound("User", "کاربر مورد نظر پیدا نشد");

            // دریافت اطلاعات استان و شهر
            var state = await _stateRepository.GetByIdAsync(seller.StateId);
            var city = await _cityRepository.GetByIdAsync(seller.CityId);

            if (state == null || city == null)
                return Error.NotFound("state or city", " شهر یا استان پیدا نشد ");

            // ایجاد مدل و پر کردن اطلاعات
            var model = new SellerDetailAdminQueryModel
            {
                Address = seller.Address,
                ImageAccept = seller.ImageAccept,
                ImageAlt = seller.ImageAlt,
                WhatsApp = seller.WhatsApp,
                CityId = seller.CityId,
                CityName = $"{state.Title} {city.Title}",
                CreateDate = seller.CreateDate.ToPersianDate(),
                Email = seller.Email,
                GoogleMapUrl = seller.GoogleMapUrl,
                Id = request.id,
                ImageName = seller.ImageName,
                Instagram = seller.Instagram,
                Phone1 = seller.Phone1,
                Phone2 = seller.Phone2,
                StateId = seller.StateId,
                Status = seller.Status,
                Telegram = seller.Telegram,
                Title = seller.Title,
                UpdateDate = seller.UpdateDate.ToPersianDate(),
                UserId = seller.UserId,
                UserName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName
            };

            return model;
        }
    }
}
