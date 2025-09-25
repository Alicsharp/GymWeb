
using Gtm.Contract.PostContract.UserPostContract.Query;
using Gtm.Domain.PostDomain.UserPostAgg;
using Utility.Appliation.RepoInterface;


namespace Gtm.Application.PostServiceApp.UserPostApp
{
    public interface IPostOrderRepo : IRepository<PostOrder, int>
    {
        Task<PostOrder> GetPostOrderNotPaymentForUserAsync(int userId);
        Task<PostOrderUserPanelModel> GetPostOrderNotPaymentForUser(int userId);
    }
}
