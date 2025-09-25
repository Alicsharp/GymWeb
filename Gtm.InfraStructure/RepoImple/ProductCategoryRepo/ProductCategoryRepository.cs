using Gtm.Application.ShopApp.ProductCategoryApp;
using Gtm.Contract.ArticleContract.Query;
using Gtm.Contract.ProductCategoryContract.Query;
using Gtm.Contract.ProductContract.Query;
using Gtm.Domain.ShopDomain.ProductCategoryDomain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.ProductCategoryRepo
{
    internal class ProductCategoryRepository : Repository<ProductCategory,int>, IProductCategoryRepository
    {
        private readonly GtmDbContext _context;
        public ProductCategoryRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<ProductCategory?> GetCategoryBySlugAsync(string slug)
        {
            return await _context.ProductCategories.SingleOrDefaultAsync(c => c.Slug == slug);
        }


        public async Task<bool> CheckProductCategoriesExist(List<int> categoryids)
        {
            foreach (int id in categoryids)
            {
                var ok = await ExistsAsync(c => c.Id == id);
                if (ok == false) return false;
            }
            return true;
        }
        public async Task<ProductCategory> GetParentCategoryByIdAsync(int parentCategoryId)
        {
            return await _context.ProductCategories
                .FindAsync(parentCategoryId);
        }
        public async Task<ProductCategory> FindProductCategoryByIdAsync(int categoryId)
        {
            return await _context.ProductCategories
             .SingleOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<List<BreadCrumbQueryModel>> GenerateBreadcrumbsAsync(int? categoryId)
        {
            var model = new List<BreadCrumbQueryModel>();

            if (!categoryId.HasValue || categoryId.Value <= 0)
                return model;

            var category = await _context.ProductCategories.FindAsync(categoryId.Value);

            if (category == null)
                return model;

            int order = 3; // مقدار پیش‌فرض شروع

            if (category.Parent > 0)
            {
                var parent = await _context.ProductCategories.FindAsync(category.Parent);
                if (parent != null)
                {
                    model.Add(new BreadCrumbQueryModel
                    {
                        Number = order++,
                        Title = parent.Title,
                        Url = $"/Shop?slug={parent.Slug}"
                    });
                }
            }

            model.Add(new BreadCrumbQueryModel
            {
                Number = order,
                Title = category.Title,
                Url = ""
            });

            return model.OrderBy(x => x.Number).ToList();
        }

      
        public async Task<List<ProductCategoryUiQueryModel>> GetActiveCategoriesWithHierarchyAsync()
        {
            var activeCategories = _context.ProductCategories
            .Where(c => c.IsActive)
            .AsQueryable();

            var result = activeCategories
                .Where(c => c.Parent == 0)
                .Select(c => new ProductCategoryUiQueryModel
                {
                    Title = c.Title,
                    Slug = c.Slug,
                    Childs = activeCategories
                        .Where(d => d.Parent == c.Id)
                        .Select(d => new ProductCategoryUiQueryModel
                        {
                            Title = d.Title,
                            Slug = d.Slug,
                            Childs = null // اگر زیردسته‌های سطح سوم هم دارید، اینجا پر کنید
                        })
                        .ToList()
                })
                .ToList();

            return result;
        }

        public IQueryable<ProductCategory?> GetActiveCategoriesAsync()
        {
            var cat = _context.ProductCategories.Where(c => c.IsActive);
            return cat;
        }
    }
}
