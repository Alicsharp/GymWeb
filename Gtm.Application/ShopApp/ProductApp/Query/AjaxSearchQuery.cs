using ErrorOr;
using Gtm.Contract.ProductContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.FileService;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    public record AjaxSearchQuery(string Filter)
      : IRequest<ErrorOr<List<AjaxSearchModel>>>;
    public class AjaxSearchQueryHandler
    : IRequestHandler<AjaxSearchQuery, ErrorOr<List<AjaxSearchModel>>>
    {
        private readonly IProductRepository _productRepository;
        // (فرض می‌کنیم FileDirectories یک کلاس static است)

        public AjaxSearchQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ErrorOr<List<AjaxSearchModel>>> Handle(
            AjaxSearchQuery request, CancellationToken cancellationToken)
        {
            // 1. دریافت IQueryable پایه از ریپازیتوری
            var query = _productRepository.SearchProductsByTitleQuery(request.Filter);

            // 2. اجرای مپینگ (Select) و اجرای نهایی (ToListAsync) در هندلر
            var model =   query
                .Select(p => new AjaxSearchModel
                {
                    ImageAddress = FileDirectories.ProductImageDirectory100 + p.ImageName,
                    id = p.Id,
                    Slug = p.Slug,
                    Title = p.Title
                })
                .ToList( ); // <-- اجرای Async

            // 3. بازگرداندن نتیجه (لیست خالی یک خطا نیست)
            return model;
        }
    }
}
