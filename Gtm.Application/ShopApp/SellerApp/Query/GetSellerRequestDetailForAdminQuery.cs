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
    public record GetSellerRequestDetailForAdminQuery(int id) : IRequest<ErrorOr<SellerRequestDetailAdminQueryModel>>;
    public class GetSellerRequestDetailForAdminQueryHandler : IRequestHandler<GetSellerRequestDetailForAdminQuery, ErrorOr<SellerRequestDetailAdminQueryModel>>
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly ISellerValidator _validator;

        public GetSellerRequestDetailForAdminQueryHandler(ISellerRepository sellerRepository, ISellerValidator validator)
        {
            _sellerRepository = sellerRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<SellerRequestDetailAdminQueryModel>> Handle(GetSellerRequestDetailForAdminQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateId(request.id);
            if (validationResult.IsError)
                return validationResult.Errors;

            return await _sellerRepository.GetSellerRequestDetailForAdmin(request.id);
        }
    }
}
