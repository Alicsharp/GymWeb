using ErrorOr;
using Gtm.Contract.PostContract.PostPriceContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostPriceApp.Query
{
    public record GetAllForPostQuery(int postId) : IRequest<ErrorOr<List<PostPriceModel>>>;
    public partial class GetAllForPostQueryHandler : IRequestHandler<GetAllForPostQuery, ErrorOr<List<PostPriceModel>>>
    {
        private readonly IPostPriceRepo _postPriceRepository;
        private readonly IPostPriceValidation _validation;

        public GetAllForPostQueryHandler(IPostPriceRepo postPriceRepository,IPostPriceValidation validation)
        {
            _postPriceRepository = postPriceRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<List<PostPriceModel>>> Handle(
            GetAllForPostQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = _validation.ValidateGetAllForPost(request.postId);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت لیست قیمت‌ها
                var prices = await _postPriceRepository.GetAllForPostAsync(request.postId);

                // برگرداندن لیست خالی در صورت عدم وجود داده (بدون خطا)
                return prices ?? new List<PostPriceModel>();
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "PostPrice.FetchError",
                    description: $"خطا در دریافت لیست قیمت‌های پست: {ex.Message}");
            }
        }
    }
}
