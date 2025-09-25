using Gtm.Domain.BlogDomain.BlogCategoryDm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ArticleCategoryApp
{
    public interface IArticleCategoryRepo : IRepository<ArticleCategory, int>
    {
        Task<ArticleCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

        //    // دریافت تمام دسته‌بندی‌های والد (سطح اول)
        //    Task<List<ArticleCategory>> GetParentCategoriesAsync(CancellationToken cancellationToken = default);

        //    // دریافت زیردسته‌های یک دسته‌بندی
        //    Task<List<ArticleCategory>> GetChildCategoriesAsync(int parentId, CancellationToken cancellationToken = default);

        //    // بررسی وجود اسلاگ تکراری
        //    Task<bool> IsSlugExistAsync(string slug, int? ignoreId = null, CancellationToken cancellationToken = default);

        //    // دریافت دسته‌بندی‌ها به همراه تعداد مقالات هر دسته
        //    //Task<List<ArticleCategoryWithCountDto>> GetCategoriesWithArticleCountAsync(CancellationToken cancellationToken = default);

        //    // دریافت دسته‌بندی‌های فعال برای نمایش در UI
        //    Task<List<ArticleCategory>> GetActiveCategoriesForUiAsync(CancellationToken cancellationToken = default);

        //    // دریافت مسیر دسته‌بندی (برای breadcrumb)
        //    Task<List<ArticleCategory>> GetCategoryPathAsync(int categoryId, CancellationToken cancellationToken = default);
        //}
    }
}
