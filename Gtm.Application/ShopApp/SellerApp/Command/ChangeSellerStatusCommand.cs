using ErrorOr;
using Gtm.Application.ShopApp.SellerApp;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.ShopApp.SellerApp.Command
{
    public record ChangeSellerStatusCommand(int id, SellerStatus status) : IRequest<ErrorOr<bool>>;
    public class ChangeSellerStatusCommandHandler : IRequestHandler<ChangeSellerStatusCommand, ErrorOr<bool>>
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly ISellerValidator _sellerValidator;

        public ChangeSellerStatusCommandHandler(ISellerRepository sellerRepository, ISellerValidator sellerValidator)
        {
            _sellerRepository = sellerRepository;
            _sellerValidator = sellerValidator;
        }

        public async Task<ErrorOr<bool>> Handle(ChangeSellerStatusCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _sellerValidator.ValidateId(request.id);
            if (validationResult.IsError)
                return validationResult.Errors;

            var seller = await _sellerRepository.GetByIdAsync(request.id);
            seller.ChangeStatus(request.status);
            return await _sellerRepository.SaveChangesAsync();
        }
    }
}
