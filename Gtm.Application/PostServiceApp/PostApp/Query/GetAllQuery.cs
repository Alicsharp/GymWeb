using ErrorOr;
using Gtm.Contract.PostContract.PostContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostApp.Query
{
    public record GetAllQuery : IRequest<ErrorOr<List<PostModel>>>;

    public class GetAllQueryHandler : IRequestHandler<GetAllQuery, ErrorOr<List<PostModel>>>
    {
        private readonly IPostRepo _postRepository;

        public GetAllQueryHandler(IPostRepo postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<ErrorOr<List<PostModel>>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var posts = await _postRepository.GetAllPosts();

                return posts is null or { Count: 0 }
                    ? new List<PostModel>() // Return empty list instead of error
                    : posts;
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
