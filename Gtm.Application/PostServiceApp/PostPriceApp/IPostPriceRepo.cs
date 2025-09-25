using Gtm.Contract.PostContract.PostPriceContract.Command;
using Gtm.Contract.PostContract.PostPriceContract.Query;
using Gtm.Domain.PostDomain.PostPriceAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.PostServiceApp.PostPriceApp
{
    public interface IPostPriceRepo : IRepository<PostPrice,int>
    {
        Task<List<PostPriceModel>> GetAllForPostAsync(int postId);
        Task<EditPostPrice> GetForEditAsync(int id);
    }
}
