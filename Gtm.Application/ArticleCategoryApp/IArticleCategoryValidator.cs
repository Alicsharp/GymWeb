using ErrorOr;
using Gtm.Contract.ArticleCategoryContract.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.Slug;

namespace Gtm.Application.ArticleCategoryApp
{
    public interface IArticleCategoryValidator
    {
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateArticleCategory dto);
        Task<ErrorOr<Success>> ValidateUpdateAsync(UpdateArticleCategoryDto dto);
        Task<ErrorOr<Success>> ValidateIdAsync(int id);
    }

    public class ArticleCategoryValidator : IArticleCategoryValidator
    {
        private readonly IArticleCategoryRepo _categoryRepo;

        public ArticleCategoryValidator(IArticleCategoryRepo categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateArticleCategory dto)
        {
            var errors = new List<Error>();

            // اعتبارسنجی عنوان
            ValidateTitle(dto.Title, errors);

            // اعتبارسنجی ParentId
            await ValidateParentId(dto.Parent, errors);

            // اعتبارسنجی اسلاگ (اختیاری)
            if (!string.IsNullOrEmpty(dto.Title))
            {
                var slug = SlugUtility.GenerateSlug(dto.Title);
                if (await _categoryRepo.ExistsAsync(c => c.Slug == slug))
                {
                    errors.Add(Error.Conflict(
                        code: "Category.SlugExists",
                        description: "اسلاگ تکراری است"));
                }
            }

            return errors.Count > 0 ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateUpdateAsync(UpdateArticleCategoryDto dto)
        {
            var errors = new List<Error>();

            // اعتبارسنجی عنوان
            ValidateTitle(dto.Title, errors);

            // اعتبارسنجی ParentId
            await ValidateIdAsync(dto.Id);

            // اعتبارسنجی عدم تغییر ParentId به خودش
            if (dto.Parent == dto.Id)
            {
                errors.Add(Error.Validation(
                    code: "Category.SelfParent",
                    description: "دسته‌بندی نمی‌تواند والد خود باشد"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }

        private void ValidateTitle(string title, List<Error> errors)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                errors.Add(Error.Validation(
                    code: "Category.TitleRequired",
                    description: "عنوان دسته‌بندی الزامی است"));
            }
            else if (title.Length > 100)
            {
                errors.Add(Error.Validation(
                    code: "Category.TitleTooLong",
                    description: "عنوان دسته‌بندی نمی‌تواند بیش از 100 کاراکتر باشد"));
            }
        }

        private async Task ValidateParentId(int? parentId, List<Error> errors, int? excludeId = null)
        {
            // اگر parentId null یا 0 باشد، یعنی دسته‌بندی والد ندارد که معتبر است
            if (!parentId.HasValue || parentId.Value == 0)
            {
                return;
            }

            // بررسی وجود دسته‌بندی والد
            var parentExists = await _categoryRepo.ExistsAsync(c => c.Id == parentId.Value);

            if (!parentExists)
            {
                errors.Add(Error.Validation(
                    code: "Category.ParentNotFound",
                    description: "دسته‌بندی والد وجود ندارد"));
            }
        }

        public async Task<ErrorOr<Success>> ValidateIdAsync(int id)
        {
            var errors = new List<Error>();
            if (id<0 && id ==null)
            {
                return Error.Validation("Category.Id.Invalid", "شناسه دسته‌بندی معتبر نیست.");
            }
            return errors.Count > 0 ? errors : Result.Success;
        }
    }
}
