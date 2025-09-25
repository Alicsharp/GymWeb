using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostApp.Command
{
    public record PostActivationChangeCommand(int Id) : IRequest<ErrorOr<Success>>;

    public class PostActivationChangeCommandHandler : IRequestHandler<PostActivationChangeCommand, ErrorOr<Success>>
    {
        private readonly IPostRepo _postRepository;
        private readonly IPostValidation _postValidation;

        public PostActivationChangeCommandHandler(IPostRepo postRepository,IPostValidation postValidation)
        {
            _postRepository = postRepository;
            _postValidation = postValidation;
        }

        public async Task<ErrorOr<Success>> Handle(PostActivationChangeCommand request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _postValidation.ValidateActivationChange(request.Id);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت پست
                var post = await _postRepository.GetByIdAsync(request.Id);

                // تغییر وضعیت فعال‌سازی
                post.ActivationChange();

                // ذخیره تغییرات
                var result = await _postRepository.SaveChangesAsync(cancellationToken);

                return result
                    ? Result.Success
                    : Error.Failure(
                        code: "Post.ActivationFailed",
                        description: "خطا در تغییر وضعیت فعال‌سازی پست");
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Post.UnexpectedError",
                    description: $"خطای غیرمنتظره در تغییر وضعیت پست: {ex.Message}");
            }
        }
    }
}
