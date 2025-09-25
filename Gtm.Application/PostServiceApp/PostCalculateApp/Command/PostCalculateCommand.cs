using ErrorOr;
using Gtm.Application.PostServiceApp.PostApp;
using Gtm.Contract.PostContract.PostCalculateContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostCalculateApp.Command
{
    public record PostCalculateCommand(PostPriceRequestModel command) : IRequest<ErrorOr<List<PostPriceResponseModel>>>;
    public class PostCalculateCommandHandler : IRequestHandler<PostCalculateCommand, ErrorOr<List<PostPriceResponseModel>>>
    {
        private readonly IPostRepo _postRepository;
        private readonly IPostValidation _postValidation;

        public PostCalculateCommandHandler(IPostRepo postRepository, IPostValidation postValidation)
        {
            _postRepository = postRepository;
            _postValidation = postValidation;
        }

        public async Task<ErrorOr<List<PostPriceResponseModel>>> Handle(PostCalculateCommand request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = _postValidation.ValidatePostCalculation(request.command);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // محاسبه قیمت پست
                var result = await _postRepository.CalculatePostAsync(request.command);

                if (result == null || !result.Any())
                {
                    return Error.NotFound(
                        code: "PostCalculate.NoResults",
                        description: "نتیجه‌ای برای محاسبه یافت نشد");
                }

                return result;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "PostCalculate.Error",
                    description: $"خطا در محاسبه قیمت پست: {ex.Message}");
            }
        }
    }
}
