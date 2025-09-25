using ErrorOr;
using Gtm.Application.PostServiceApp.UserPostApp;
using Gtm.Contract.PostContract.PostCalculateContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostCalculateApp.Command
{
    public record PostCalculateApiCommand(PostPriceRequestApiModel postPriceRequestApiModel) : IRequest<ErrorOr<PostPriceResponseApiModel>>;
    public class PostCalculateApiCommandHandler : IRequestHandler<PostCalculateApiCommand, ErrorOr<PostPriceResponseApiModel>>
    {
        private readonly IUserPostRepo _userPostRepository;
        private readonly IMediator _mediator;
        private readonly IUserPostValidation _validation;

        public PostCalculateApiCommandHandler(IUserPostRepo userPostRepository,IMediator mediator,IUserPostValidation validation)
        {
            _userPostRepository = userPostRepository;
            _mediator = mediator;
            _validation = validation;
        }

        public async Task<ErrorOr<PostPriceResponseApiModel>> Handle(PostCalculateApiCommand request,CancellationToken cancellationToken)
        {
            try
            {
                var userPost = await _userPostRepository.GetByApiCodeAsync(request.postPriceRequestApiModel.ApiCode);

                // اعتبارسنجی
                var validationResult = _validation.ValidateApiRequest(userPost, request.postPriceRequestApiModel);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // محاسبه قیمت
                var prices = await _mediator.Send(new PostCalculateCommand(
                    new PostPriceRequestModel
                    {
                        DestinationCityId = request.postPriceRequestApiModel.DestinationCityId,
                        SourceCityId = request.postPriceRequestApiModel.SourceCityId,
                        Weight = request.postPriceRequestApiModel.Weight
                    }),
                    cancellationToken);

                if (!prices.IsError && !prices.Value.Any())
                {
                    return Error.Conflict(
                        code: "Post.CalculationError",
                        description: "خطا در محاسبه قیمت. لطفا با پشتیبانی تماس بگیرید");
                }

                if (prices.IsError)
                {
                    return prices.Errors;
                }

                // کاهش تعداد درخواست‌های باقیمانده
                userPost.UseApi();
                await _userPostRepository.SaveChangesAsync(cancellationToken);

                return new PostPriceResponseApiModel(
                    prices.Value,
                    "عملیات با موفقیت انجام شد",
                    true);
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Post.ApiError",
                    description: $"خطای غیرمنتظره در پردازش درخواست: {ex.Message}");
            }
        }
    }
}
