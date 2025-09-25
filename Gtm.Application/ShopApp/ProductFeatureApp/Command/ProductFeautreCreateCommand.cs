using ErrorOr;
using Gtm.Contract.ProductFeautreContract.Command;
using Gtm.Domain.ShopDomain.ProductFeaureDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.ShopApp.ProductFeatureApp.Command
{
    public record ProductFeautreCreateCommand(CreateProductFeautre command) : IRequest<ErrorOr<Success>>;
    public class ProductFeautreCreateCommandHnadler : IRequestHandler<ProductFeautreCreateCommand, ErrorOr<Success>>
    {
        private readonly IProductFeatureRepository _featureRepository;

        public ProductFeautreCreateCommandHnadler(IProductFeatureRepository featureRepository)
        {
            _featureRepository = featureRepository;
        }

        public async Task<ErrorOr<Success>> Handle(ProductFeautreCreateCommand request, CancellationToken cancellationToken)
        {
            ProductFeature productFeature = new(request.command.ProductId, request.command.Title, request.command.Value);
            await _featureRepository.AddAsync(productFeature);
            if (await _featureRepository.SaveChangesAsync(cancellationToken)) return Result.Success;
            return Error.Failure("systemError", ValidationMessages.SystemErrorMessage);
        }
    }
}
