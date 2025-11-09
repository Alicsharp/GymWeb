using ErrorOr;
using Gtm.Application.ShopApp.ProductApp;
using Gtm.Application.ShopApp.SellerApp;
using Gtm.Contract.ProductSellContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductSellApp
{
    public interface IProductSellValidation 
    {
        Task<ErrorOr<Success>> ValidateAsync(int id);
        Task<ErrorOr<Success>>ValidationProductEditSellerAmout(List<EditProdoctSellAmount> sels  );
        Task<ErrorOr<Success>> ValidateCreateProductSellAsync(CreateProductSell createProductSell);
    }
    public class ProductSellValidation : IProductSellValidation
    {
        private readonly IProductSellRepository _productSellRepository;
        private readonly IProductRepository _productRepository;
        private readonly ISellerRepository  _sellerRepository;

        public ProductSellValidation(IProductSellRepository productSellRepository, IProductRepository productRepository, ISellerRepository sellerRepository)
        {
            _productSellRepository = productSellRepository;
            _productRepository = productRepository;
            _sellerRepository = sellerRepository;
        }

        public async Task<ErrorOr<Success>> ValidateAsync(int id)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه
            if (id <= 0)
            {
                errors.Add(Error.Validation(
                    code: "ActivationChange.Id.Invalid",
                    description: "شناسه فروش محصول باید بزرگتر از صفر باشد."
                ));
                return errors;
            }

            // بررسی وجود فروش محصول
            var productSellExists = await _productSellRepository.ExistsAsync(x=>x.Id==id);
            if (productSellExists==false)
            {
                errors.Add(Error.NotFound(
                    code: "ActivationChange.NotFound",
                    description: "فروش محصول مورد نظر یافت نشد."
                ));
            }

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateCreateProductSellAsync(CreateProductSell createProductSell)
        {
            var errors = new List<Error>();

            var ProductExist = await _productRepository.ExistsAsync(c => c.Id == createProductSell.ProductId);
            if (!ProductExist) 
            {
                errors.Add(Error.NotFound("ProductNotFound", "محصولی پیدا نشد"));
            }
            var sellerExist= await _sellerRepository.ExistsAsync(c=>c.Id==createProductSell.SellerId);
            if (!sellerExist)
                errors.Add(Error.NotFound("SellerNotFound", "فروشنده پیدا نشد"));
            if(createProductSell.Weight<=0)
            {
                errors.Add(Error.Validation("Weight", "وزن نمی تواند صفر یا منفی باشد"));

            }
            return errors.Any() ? errors : Result.Success;

        }
        public async Task<ErrorOr<Success>> ValidationProductEditSellerAmout(List<EditProdoctSellAmount> sels)
        {
            var errors = new List<Error>();
            foreach(var  sel in sels)
            {
                if (sel.SellId > 0)
                {
                    //var exist = await _productSellRepository.ExistsAsync(c=> c.Id == sel.SellId);
                    //if (!exist) { }
                    //errors.Add(Error.NotFound(code: "SellerIdNotCorrect", "ایدی فروشنده معتبر نیست"));
                }
                if (sel.count < 0)
                    errors.Add(Error.Validation("ProductCount", "تعدا محصول نمی تواند صفر باشد"));
            }


            return errors.Any() ? errors : Result.Success;
        }
    }
}
