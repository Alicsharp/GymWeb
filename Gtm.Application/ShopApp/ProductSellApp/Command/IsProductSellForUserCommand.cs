using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductSellApp.Command
{
    public record IsProductSellForUserCommand(int userId, int id):IRequest<ErrorOr<Success>>;
    public class IsProductSellForUserCommandHandler : IRequestHandler<IsProductSellForUserCommand, ErrorOr<Success>>
    {
        private readonly IProductSellRepository _productSellRepository;

        public IsProductSellForUserCommandHandler(IProductSellRepository productSellRepository)
        {
            _productSellRepository = productSellRepository;
        }

        public async Task<ErrorOr<Success>> Handle(IsProductSellForUserCommand request, CancellationToken cancellationToken)
        {
            return await _productSellRepository.IsProductSellForUserAsync(request.userId, request.id);
        }
    }
}
