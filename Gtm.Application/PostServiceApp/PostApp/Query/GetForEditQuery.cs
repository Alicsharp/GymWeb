using ErrorOr;
using Gtm.Contract.PostContract.PostContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostApp.Query
{
  
     public record GetForEditQuery(int Id) : IRequest<ErrorOr<EditPost>>;
    public class GetForEditQueryHandler : IRequestHandler<GetForEditQuery, ErrorOr<EditPost>>
    {
        private readonly IPostRepo _postRepository;
        private readonly IPostValidation _postValidation;

        public GetForEditQueryHandler( IPostRepo postRepository, IPostValidation postValidation)
        {
            _postRepository = postRepository;
            _postValidation = postValidation;
        }

        public async Task<ErrorOr<EditPost>> Handle(GetForEditQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _postValidation.ValidateGetForEdit(request.Id);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت پست برای ویرایش
                var post = await _postRepository.GetForEditAsync(request.Id);

                // روش صحیح بررسی null
                if (post == null)
                {
                    return Error.NotFound(
                        code: "Post.NotFound",
                        description: "پست مورد نظر یافت نشد");
                }

                return post;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "Post.FetchError",
                    description: $"خطا در دریافت اطلاعات پست: {ex.Message}");
            }
        }
    }
}
