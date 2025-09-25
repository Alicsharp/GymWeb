using ErrorOr;
using Gtm.Contract.ProductSellContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductSellApp.Command
{
    public record EditProdoctSellAmountCommand(List<EditProdoctSellAmount> sels) : IRequest<ErrorOr<Success>>;
    public class EditProdoctSellAmountCommandHandler : IRequestHandler<EditProdoctSellAmountCommand, ErrorOr<Success>>
    {
        private readonly IProductSellRepository _productSellRepository;
        private readonly IProductSellValidation _productSellValidation;

        public EditProdoctSellAmountCommandHandler(IProductSellRepository productSellRepository, IProductSellValidation productSellValidation)
        {
            _productSellRepository = productSellRepository;
            _productSellValidation = productSellValidation;
        }

        public async Task<ErrorOr<Success>> Handle(EditProdoctSellAmountCommand request, CancellationToken cancellationToken)
        {
            var validation = await _productSellValidation.ValidationProductEditSellerAmout(request.sels);
            if (validation.IsError)
                return Error.Validation("EditProductSellValidation", "اعتبار سنجی با شکست مواجه شد");

            foreach (var item in request.sels)
            {
                var sell = await _productSellRepository.GetByIdAsync(item.SellId);
                sell.ChangeAmount(item.count, item.Type);
            }
           if( await _productSellRepository.SaveChangesAsync(cancellationToken))
                return Result.Success;
            return Error.Failure("EditProductSell",",ویرایش با شکست مواجه شد");
        }
    }
}
