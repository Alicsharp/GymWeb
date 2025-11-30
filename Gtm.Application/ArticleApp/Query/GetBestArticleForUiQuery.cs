using ErrorOr;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Contract.ArticleContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.ArticleApp.Query
{
    public record GetBestArticleForUiQuery : IRequest<ErrorOr<List<BestArticleQueryModel>>>;
    public class GetBestArtilceForUiQueryHandler : IRequestHandler<GetBestArticleForUiQuery, ErrorOr<List<BestArticleQueryModel>>>
    {
        private readonly  IArticleRepo  _articleRepo;
        private readonly IArticleCategoryRepo  _articleCategoryRepo;

        public GetBestArtilceForUiQueryHandler(IArticleRepo articleRepo, IArticleCategoryRepo articleCategoryRepo)
        {
            _articleRepo = articleRepo;
            _articleCategoryRepo = articleCategoryRepo;
        }
        public async Task<ErrorOr<List<BestArticleQueryModel>>> Handle(GetBestArticleForUiQuery request, CancellationToken cancellationToken)
        {
            // 1. دریافت مقالات (بدون تبدیل تاریخ و بدون اسم دسته‌بندی)
            // فرض بر این است که QueryBy خروجی IQueryable می‌دهد
            var query = _articleRepo.QueryBy(b => b.IsActive)
                .OrderByDescending(b => b.VisitCount)
                .Take(4);

            // واکشی داده‌های مورد نیاز (Projection)
            var rawArticles = await query.Select(b => new
            {
                b.Id,
                b.Title,
                b.Slug,
                b.CreateDate, // تاریخ میلادی
                b.Writer,
                b.VisitCount,
                b.ImageName,
                b.ImageAlt,
                b.CategoryId,
                b.SubCategoryId,
                // منطق انتخاب آیدی درست برای دسته‌بندی
                DisplayCategoryId = b.SubCategoryId > 0 ? b.SubCategoryId : b.CategoryId
            }).ToListAsync(cancellationToken); // پاس دادن توکن

            if (!rawArticles.Any())
                return new List<BestArticleQueryModel>();

            // 2. دریافت اطلاعات دسته‌بندی‌ها به صورت یکجا (Bulk Load)
            // تمام آیدی‌های دسته‌بندی مورد نیاز را جمع می‌کنیم
            var categoryIds = rawArticles.Select(r => r.DisplayCategoryId).Distinct().ToList();

            // با یک کوئری همه نام‌ها را می‌گیریم
            // فرض: متدی مثل GetAllByQueryAsync در ریپازیتوری دسته‌بندی دارید
            var categories = await _articleCategoryRepo.GetAllByQueryAsync(c => categoryIds.Contains(c.Id), cancellationToken);

            // تبدیل به دیکشنری برای جستجوی سریع در حافظه
            var categoryDict = categories.ToDictionary(c => c.Id, c => new { c.Title, c.Slug });

            // 3. مپ کردن نهایی (In-Memory Mapping)
            var result = rawArticles.Select(b => new BestArticleQueryModel
            {
                Title = b.Title,
                Slug = b.Slug,
                Writer = b.Writer,
                // تبدیل تاریخ اینجا امن است چون در حافظه رم هستیم
                CreationDate = b.CreateDate.ToPersianDate(),
                Visit = b.VisitCount,
                Category = b.CategoryId,
                SubCategory = b.SubCategoryId,
                ImageName = FileDirectories.ArticleImageDirectory + b.ImageName,
                ImageName400 = FileDirectories.ArticleImageDirectory400 + b.ImageName,
                ImageAlt = b.ImageAlt,

                // پر کردن اطلاعات دسته از دیکشنری
                CategoryTitle = categoryDict.ContainsKey(b.DisplayCategoryId) ? categoryDict[b.DisplayCategoryId].Title : "",
                CategorySlug = categoryDict.ContainsKey(b.DisplayCategoryId) ? categoryDict[b.DisplayCategoryId].Slug : ""
            }).ToList();

            return result;
        }
    }

}
