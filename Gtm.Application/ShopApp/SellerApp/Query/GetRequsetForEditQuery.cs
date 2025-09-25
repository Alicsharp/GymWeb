using ErrorOr;
using Gtm.Contract.SellerContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.ShopApp.SellerApp.Query
{
    public record GetRequsetFoeEditQuery(int id, int userId) : IRequest<ErrorOr<EditSellerRequest>>;
    public class GetRequsetFoeEditQueryHandler : IRequestHandler<GetRequsetFoeEditQuery, ErrorOr<EditSellerRequest>>
    {
        private readonly ISellerRepository _sellerRepository;

        public GetRequsetFoeEditQueryHandler(ISellerRepository sellerRepository)
        {
            _sellerRepository = sellerRepository;
        }

        public async Task<ErrorOr<EditSellerRequest>> Handle(GetRequsetFoeEditQuery request, CancellationToken cancellationToken)
        {

            var seller = await _sellerRepository.GetByIdAsync(request.id);
            if (seller.UserId != request.userId || seller.Status != SellerStatus.درخواست_تایید_نشده) return Error.NotFound("notfound", " مقداری پیدا نشد");
            return new EditSellerRequest()
            {
                Address = seller.Address,
                ImageAccept = null,
                ImageAcceptName = seller.ImageAccept,
                ImageAlt = seller.ImageAlt,
                WhatsApp = seller.WhatsApp,
                CityId = seller.CityId,
                Email = seller.Email,
                GoogleMapUrl = seller.GoogleMapUrl,
                Id = seller.Id,
                ImageFile = null,
                ImageName = seller.ImageName,
                Instagram = seller.Instagram,
                Phone1 = seller.Phone1,
                Phone2 = seller.Phone2,
                StateId = seller.StateId,
                Telegram = seller.Telegram,
                Title = seller.Title
            };
        }
    }
}
