using ErrorOr;
using Gtm.Contract.ProductSellContract.Command;
using Gtm.Domain.ShopDomain.ProductSellDomain;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.ShopApp.ProductSellApp.Command
{
    public record CreateProductSellCommand(CreateProductSell command) : IRequest<ErrorOr<Success>>;
    public class CreateProductSellCommandHandler : IRequestHandler<CreateProductSellCommand, ErrorOr<Success>>
    {
        private readonly IProductSellRepository _productSellRepository;
        private readonly IProductSellValidation _productSellValidation;

        public CreateProductSellCommandHandler(IProductSellRepository productSellRepository, IProductSellValidation productSellValidation)
        {
            _productSellRepository = productSellRepository;
            _productSellValidation = productSellValidation;
        }

        public async Task<ErrorOr<Success>> Handle(CreateProductSellCommand request, CancellationToken cancellationToken)
        {
            var validation = await _productSellValidation.ValidateCreateProductSellAsync(request.command);
            if (validation.IsError)
                return Error.Validation("CreateProductSellValidation", "اعتبار سنجی با شکست مواجه شد");
            var sell = new  ProductSell(request.command.ProductId, request.command.SellerId, request.command.Price, request.command.Unit, request.command.Weight);
            await _productSellRepository.AddAsync(sell);
            if (await _productSellRepository.SaveChangesAsync(cancellationToken)) return Result.Success;
            return Error.Failure(nameof(request.command.Unit), ValidationMessages.SystemErrorMessage);
        }
    }
}
