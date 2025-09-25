using ErrorOr;
using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Contract.ArticleContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ShopApp.ProductApp.Query
{
    public record GetProductBreadCrumbQuery(string? categorySlug, string? productSlug) : IRequest<ErrorOr<List<BreadCrumbQueryModel>>>;
    public class GetProductBreadCrumbQueryHandler
    : IRequestHandler<GetProductBreadCrumbQuery, ErrorOr<List<BreadCrumbQueryModel>>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly IProductRepository _productRepository;

        public GetProductBreadCrumbQueryHandler(IProductCategoryRepository productCategoryRepository,IProductRepository productRepository)
        {
            _productCategoryRepository = productCategoryRepository;
            _productRepository = productRepository;
        }

        public async Task<ErrorOr<List<BreadCrumbQueryModel>>> Handle(GetProductBreadCrumbQuery request,CancellationToken cancellationToken)
        {
            List<BreadCrumbQueryModel> model = new()
        {
            new BreadCrumbQueryModel { Number = 1, Title = "خانه", Url = "/" },
            new BreadCrumbQueryModel { Number = 2, Title = "محصولات", Url = "/Shop" }
        };

            // -------------------------------
            // حالت categorySlug
            // -------------------------------
            if (!string.IsNullOrWhiteSpace(request.categorySlug))
            {
                var category = await _productCategoryRepository
                    .GetCategoryBySlugAsync(request.categorySlug.ToLower().Trim());

                if (category != null)
                {
                    int i = 3;

                    if (category.Parent > 0)
                    {
                        var parent = await _productCategoryRepository.GetByIdAsync(category.Parent);
                        if (parent != null)
                        {
                            model.Add(new BreadCrumbQueryModel
                            {
                                Number = i,
                                Title = parent.Title,
                                Url = $"/Shop?slug={parent.Slug}"
                            });
                            i++;
                        }
                    }

                    model.Add(new BreadCrumbQueryModel
                    {
                        Number = i,
                        Title = category.Title,
                        Url = ""
                    });
                }

                return model;
            }

            // -------------------------------
            // حالت productSlug
            // -------------------------------
            else if (!string.IsNullOrWhiteSpace(request.productSlug))
            {
                var product = await _productRepository
                    .GetBySlugWithCategoriesAsync(request.productSlug.ToLower().Trim());

                if (product == null)
                    return model; // فقط خانه و محصولات

                int i = 3;

                if (product.ProductCategoryRelations.Any())
                {
                    var categoryId = product.ProductCategoryRelations.First().ProductCategoryId;
                    var category = await _productCategoryRepository.GetByIdAsync(categoryId);

                    if (category != null)
                    {
                        if (category.Parent > 0)
                        {
                            var parent = await _productCategoryRepository.GetByIdAsync(category.Parent);
                            if (parent != null)
                            {
                                model.Add(new BreadCrumbQueryModel
                                {
                                    Number = i,
                                    Title = parent.Title,
                                    Url = $"/Shop?slug={parent.Slug}"
                                });
                                i++;
                            }
                        }

                        model.Add(new BreadCrumbQueryModel
                        {
                            Number = i,
                            Title = category.Title,
                            Url = $"/Shop?slug={category.Slug}"
                        });
                        i++;
                    }
                }

                model.Add(new BreadCrumbQueryModel
                {
                    Number = i,
                    Title = product.Title,
                    Url = ""
                });

                return model;
            }

            // -------------------------------
            // حالت پیش‌فرض
            // -------------------------------
            else
            {
                return model;
            }
        }
    }

}
