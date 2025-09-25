 
using Gtm.Domain.EmailDomain.MessageUserAgg;
 
using Utility.Appliation.RepoInterface;
 

namespace Gtm.Application.EmailServiceApp.MessageUserApp
{
    public interface IMessageUserRepository : IRepository<MessageUser, int>
    {
    }
}
