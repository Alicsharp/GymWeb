using Gtm.Contract.SellerContract.Query;
using Gtm.Domain.PostDomain.StateAgg;
using Gtm.Domain.ShopDomain.SellerDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ShopApp.SellerApp
{
    public interface ISellerRepository : IRepository<Seller, int>
    {
        Task<List<SellerRequestAdminQueryModel>> GetSellerRequestsForAdmin();
        Task<SellerRequestDetailAdminQueryModel> GetSellerRequestDetailForAdmin(int id);
        Task<Seller?> GetSellerForUserPanelAsync(int id, int userId);
        Task<Seller?> GetSellerByIdAsync(int id);
    }
}
