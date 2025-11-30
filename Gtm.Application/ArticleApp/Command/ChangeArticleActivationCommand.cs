using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.ArticleApp.Command
{
    public record ChangeArticleActivationCommand(int Id):IRequest<ErrorOr<Success>>;
    public class ChangeArticleActivationCommandHandler : IRequestHandler<ChangeArticleActivationCommand, ErrorOr<Success>>
    { 
        private readonly IArticleRepo _articleRepo;
        private readonly IArticleValidator _articleValidator;

        public ChangeArticleActivationCommandHandler(IArticleRepo articleRepo, IArticleValidator articleValidator)
        {
            _articleRepo = articleRepo;
            _articleValidator = articleValidator;
        }

        public async Task<ErrorOr<Success>> Handle(ChangeArticleActivationCommand request, CancellationToken cancellationToken)
        {
            // 1. اعتبارسنجی اولیه (آیا عدد مثبت است؟)
            var validationResults = await _articleValidator.ValidateIdAsync(request.Id);
            if (validationResults.IsError)
            {
                return validationResults.Errors;
            }

            // 2. دریافت از دیتابیس
            var entity = await _articleRepo.GetByIdAsync(request.Id);

            // 3. 🚨 گارد کلاز حیاتی: آیا اصلا پیدا شد؟
            if (entity == null)
            {
                return Error.NotFound("Article.NotFound", "مقاله‌ای با این شناسه یافت نشد");
            }

            // 4. تغییر وضعیت
            entity.ActivationChange();

            // 5. ذخیره
            var saved = await _articleRepo.SaveChangesAsync(cancellationToken);
            if (!saved)
            {
                return Error.Failure("NotSaved", "عملیات شکست خورد");
            }

            return Result.Success;
        }
    }

}
