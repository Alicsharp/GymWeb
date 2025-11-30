using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.PostServiceApp.StateApp;
using Gtm.Contract.SellerContract.Query;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.ShopApp.SellerApp.Query
{
    public record GetSellerDetailForSellerQuery(int id, int userId) : IRequest<ErrorOr<SellerDetailForUserPanelQueryModel>>;
    public class GetSellerDetailForSellerQueryHandler : IRequestHandler<GetSellerDetailForSellerQuery, ErrorOr<SellerDetailForUserPanelQueryModel>>
    {
        private readonly ISellerRepository _sellerRepository;   
        private readonly IStateRepo _stateRepository;
        private readonly ICityRepo _cityRepository;

        public GetSellerDetailForSellerQueryHandler(ISellerRepository sellerRepository, IStateRepo stateRepository, ICityRepo cityRepository)
        {
            _sellerRepository = sellerRepository;
            _stateRepository = stateRepository;
            _cityRepository = cityRepository;
        }

        public async Task<ErrorOr<SellerDetailForUserPanelQueryModel>> Handle(GetSellerDetailForSellerQuery request, CancellationToken cancellationToken)
        {
            var seller = await _sellerRepository.GetSellerForUserPanelAsync(request.id,request.userId);
            if (seller == null) return Error.NotFound() ;
            SellerDetailForUserPanelQueryModel model = new()
            {
                Address = seller.Address,
                ImageAccept = seller.ImageAccept,
                WhatsApp = seller.WhatsApp,
                ImageAlt = seller.ImageAlt,
                CityId = seller.CityId,
                CreationDate = seller.CreateDate.ToPersianDate(),
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
                CityName = ""
            };
            var state =  await _stateRepository.GetByIdAsync(seller.StateId);
            var city = await _cityRepository.GetByIdAsync(seller.CityId);
            model.CityName = $"{state.Title} - {city.Title}";
            return model;
        }
    }
}
