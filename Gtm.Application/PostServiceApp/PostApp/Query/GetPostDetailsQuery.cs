using ErrorOr;
using Gtm.Application.PostServiceApp.PostPriceApp;
using Gtm.Contract.PostContract.PostContract.Query;
using Gtm.Contract.PostContract.PostPriceContract.Query;
using Gtm.Domain.PostDomain.Postgg;
using Gtm.Domain.PostDomain.PostPriceAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.PostServiceApp.PostApp.Query
{
    public record GetPostDetailsQuery(int id) : IRequest<ErrorOr<PostAdminDetailQueryModel>>;
    public class GetPostDetailsQueryHandler : IRequestHandler<GetPostDetailsQuery, ErrorOr<PostAdminDetailQueryModel>>
    {
        private readonly IPostRepo _postRepository;
        private readonly IPostPriceRepo _postPriceRepository;
        private readonly IPostValidation _postValidation;

        public GetPostDetailsQueryHandler(IPostRepo postRepository,IPostPriceRepo postPriceRepository,IPostValidation postValidation)
        {
            _postRepository = postRepository;
            _postPriceRepository = postPriceRepository;
            _postValidation = postValidation;
        }

        public async Task<ErrorOr<PostAdminDetailQueryModel>> Handle(GetPostDetailsQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی
                var validationResult = await _postValidation.ValidateGetPostDetails(request.id);
                if (validationResult.IsError)
                {
                    return validationResult.Errors;
                }

                // دریافت داده‌ها
                var post = await _postRepository.GetByIdAsync(request.id);
                var prices = await _postPriceRepository.GetAllByQueryAsync(p => p.PostId == post.Id);

                // تبدیل به مدل نمایشی
                return MapToViewModel(post, prices);
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "Post.FetchError",
                    description: $"خطا در دریافت جزئیات پست: {ex.Message}");
            }
        }

        private PostAdminDetailQueryModel MapToViewModel(Post post, IEnumerable<PostPrice> prices)
        {
            return new PostAdminDetailQueryModel
            {
                Active = post.Active,
                CityPricePlus = post.CityPricePlus,
                CreationDate = post.CreateDate.ToPersainDate(), // اصلاح نام متد
                Description = post.Description,
                Id = post.Id,
                InsideCity = post.InsideCity,
                InsideStatePricePlus = post.InsideStatePricePlus,
                OutsideCity = post.OutSideCity,
                PostPrices = prices.Select(p => new PostPriceAdminQueryModel
                {
                    CityPrice = p.CityPrice,
                    End = p.End,
                    Id = p.Id,
                    InsideStatePrice = p.InsideStatePrice,
                    Start = p.Start,
                    StateCenterPrice = p.StateCenterPrice,
                    StateClosePrice = p.StateClosePrice,
                    StateNonClosePrice = p.StateNonClosePrice,
                    TehranPrice = p.TehranPrice
                }).ToList(),
                StateCenterPricePlus = post.StateCenterPricePlus,
                StateClosePricePlus = post.StateClosePricePlus,
                StateNonClosePricePlus = post.StateNonClosePricePlus,
                Status = post.Status,
                TehranPricePlus = post.TehranPricePlus,
                Title = post.Title
            };
        }
    }
}
