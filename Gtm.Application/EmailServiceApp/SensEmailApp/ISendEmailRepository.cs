using ErrorOr;
using Gtm.Contract.EmailContract.SensEmailContract.Command;
using Gtm.Domain.EmailDomain.SendEmailAgg;

using Utility.Appliation.RepoInterface;


namespace Gtm.Application.EmailServiceApp.SensEmailApp
{
    public interface ISendEmailRepository : IRepository<SendEmail,int>
    {
    }

}
