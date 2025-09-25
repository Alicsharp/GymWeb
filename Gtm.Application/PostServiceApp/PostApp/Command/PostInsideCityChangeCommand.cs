using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostApp.Command
{
    public record PostInsideCityChangeCommand(int Id) : IRequest<ErrorOr<Success>>;

    public class PostInsideCityChangeCommandHandler: IRequestHandler<PostInsideCityChangeCommand, ErrorOr<Success>>
    {
        private readonly IPostRepo _postRepository;
        private readonly IPostValidation _postValidation;

        public PostInsideCityChangeCommandHandler(IPostRepo postRepository,IPostValidation postValidation)
        {
            _postRepository = postRepository;
            _postValidation = postValidation;
        }

        public async Task<ErrorOr<Success>> Handle(
            PostInsideCityChangeCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _postValidation.ValidateInsideCityChange(request.Id);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت پست
                var post = await _postRepository.GetByIdAsync(request.Id);

                // تغییر وضعیت
                post.InsideCityChange();

                // ذخیره تغییرات
                var result = await _postRepository.SaveChangesAsync(cancellationToken);

                return result
                    ? Result.Success
                    : Error.Failure(
                        code: "Post.UpdateFailed",
                        description: "خطا در تغییر وضعیت درون شهری پست");
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Post.UpdateError",
                    description: $"خطای غیرمنتظره در تغییر وضعیت: {ex.Message}");
            }
        }
    }
}
