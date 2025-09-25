using ErrorOr;
using Gtm.Contract.PostContract.PostCalculateContract.Query;
using Gtm.Contract.PostContract.PostContract.Command;
using Gtm.Contract.PostContract.PostContract.Query;
using Gtm.Domain.PostDomain.Postgg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.PostServiceApp.PostApp
{
    public interface IPostRepo : IRepository<Post,int>
    {
        Task<List<PostModel>> GetAllPosts();
        Task<EditPost> GetForEditAsync(int id);
        List<PostAdminQueryModel> GetAllPostsForAdmin();
        PostAdminDetailQueryModel GetPostDetails(int id);
        Task<List<PostPriceResponseModel>> CalculatePostAsync(PostPriceRequestModel command);

    }
  
}
