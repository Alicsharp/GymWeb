using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductCategoryApp.Command
{
    public record CheckProductCategoryHaveParentCommand(int id) : IRequest<ErrorOr<Success>>;
    public class CheckCategoryHaveParentCommandHandler : IRequestHandler<CheckProductCategoryHaveParentCommand, ErrorOr<Success>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public CheckCategoryHaveParentCommandHandler(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        public async Task<ErrorOr<Success>> Handle(CheckProductCategoryHaveParentCommand request, CancellationToken cancellationToken)
        {
            var category = await _productCategoryRepository.GetByIdAsync(request.id);
            if (category != null && category.Parent > 0)
            {
                return Error.Validation("ProductCategoryNotParent", "این کتکوری دارای والد است");
            }
            return Result.Success;
            //return category != null && category.Parent > 0;
        }
    }
}
