using ErrorOr;
using Gtm.Application.PostServiceApp.PostPriceApp;
using Gtm.Contract.PostContract.PostContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.PostServiceApp.PostApp.Query
{
    public record GetAllPostsForAdminQuery : IRequest<ErrorOr<List<PostAdminQueryModel>>>;
    public class GetAllPostsForAdminQueryHandler: IRequestHandler<GetAllPostsForAdminQuery, ErrorOr<List<PostAdminQueryModel>>>
    {
        private readonly IPostRepo _postRepository;

        public GetAllPostsForAdminQueryHandler(IPostRepo postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<ErrorOr<List<PostAdminQueryModel>>> Handle(GetAllPostsForAdminQuery request,CancellationToken cancellationToken)
        {
            try
            { 

                // دریافت داده‌ها
                var query =   _postRepository.GetAllQueryable( );
                var posts = await query.ToListAsync(cancellationToken);

                // اگر داده‌ای وجود نداشت، لیست خالی برگردانده می‌شود
                if (!posts.Any())
                {
                    return new List<PostAdminQueryModel>();
                }

                // تبدیل به مدل نمایشی
                var result = posts.Select(p => new PostAdminQueryModel
                {
                    Active = p.Active,
                    CreationDate = p.CreateDate.ToPersianDate(),  
                    Id = p.Id,
                    InsideCity = p.InsideCity,
                    OutsideCity = p.OutSideCity,
                    Title = p.Title
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                return Error.Failure(
                    code: "Post.FetchError",
                    description: $"خطا در دریافت لیست پست‌ها: {ex.Message}");
            }
        }
    }
}
