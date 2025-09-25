using Gtm.Domain.EmailDomain.EmailUserAgg;
using Utility.Appliation.RepoInterface;


namespace Gtm.Application.EmailServiceApp.EmailUserApp
{
    public interface IEmailUserRepository : IRepository<EmailUser,int>
    {
        Task<bool> CreateListAsync(List<EmailUser> emailUsers);

        //Task<List<EmailUserViewModel>> GetAllForAdminAsync();

        Task<EmailUser?> GetByEmailAsync(string email);
    }
}
