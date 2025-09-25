using ErrorOr;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.SellerApp.Command
{
    public  record IsSellerForUserCommand(int id, int userId):IRequest<ErrorOr<Success>>;
    public class IsSellerForUserCommandHandler : IRequestHandler<IsSellerForUserCommand, ErrorOr<Success>>
    {
        private  ISellerRepository _sellerRepository;

        public IsSellerForUserCommandHandler(ISellerRepository sellerRepository)
        {
            _sellerRepository = sellerRepository;
        }

        public async Task<ErrorOr<Success>> Handle(IsSellerForUserCommand request, CancellationToken cancellationToken)
        {
            var seller = await _sellerRepository.GetByIdAsync(request.id);
             if( seller.UserId == request.userId)
                return Result.Success;
            return Error.Failure("");
        }
    }
}
