using ErrorOr;
using Gtm.Application.ArticleApp;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleCategoryApp.Command
{
    public record DeleteArticleCategoryCommand(int Id, bool ForceDelete = false): IRequest<ErrorOr<Deleted>>;

    public class DeleteArticleCategoryCommandHandler: IRequestHandler<DeleteArticleCategoryCommand, ErrorOr<Deleted>>
    {
        private readonly IArticleCategoryRepo _categoryRepo;
        private readonly IArticleRepo _articleRepo;

        public DeleteArticleCategoryCommandHandler(IArticleCategoryRepo categoryRepo,IArticleRepo articleRepo)
        {
            _categoryRepo = categoryRepo;
            _articleRepo = articleRepo;
        }

        public async Task<ErrorOr<Deleted>> Handle(
            DeleteArticleCategoryCommand request,
            CancellationToken cancellationToken)
        {
            // 1. پیدا کردن دسته‌بندی
            var category = await _categoryRepo.GetByIdAsync(request.Id);
            if (category is null)
            {
                return Error.NotFound(
                    code: "Category.NotFound",
                    description: "دسته‌بندی مورد نظر یافت نشد");
            }

            // 2. بررسی مقالات مرتبط
            //var hasArticles = await _articleRepo.AnyAsync(a => a.CategoryId == request.Id);

            //if (hasArticles && !request.ForceDelete)
            //{
            //    return Error.Conflict(
            //        code: "Category.HasArticles",
            //        description: "دسته‌بندی دارای مقالات است. برای حذف از ForceDelete استفاده کنید");
            //}
            var hasArticles = await _articleRepo.ExistsAsync(a => a.CategoryId == request.Id);

            if (hasArticles && !request.ForceDelete)
            {
                return Error.Conflict(
                    code: "Category.HasArticles",
                    description: "دسته‌بندی دارای مقالات است. برای حذف از ForceDelete استفاده کنید");
            }

            try
            {
                // 3. شروع تراکنش (در سطح ریپازیتوری)
                await _categoryRepo.BeginTransactionAsync();

                // 4. حذف مقالات اگر ForceDelete=true
                if (request.ForceDelete)
                {
                    await _articleRepo.DeleteByCategoryIdAsync(request.Id);
                }

                // 5. حذف دسته‌بندی
                await _categoryRepo.RemoveByIdAsync(request.Id);

                // 6. کامیت تراکنش
                await _categoryRepo.CommitTransactionAsync();

                return Result.Deleted;
            }
            catch (Exception ex)
            {
                // 7. در صورت خطا، رولبک انجام شود
                await _categoryRepo.RollbackTransactionAsync();

                return Error.Failure(
                    code: "Database.DeleteError",
                    description: "خطا در حذف دسته‌بندی: " + ex.Message);
            }
        }
    }
}
