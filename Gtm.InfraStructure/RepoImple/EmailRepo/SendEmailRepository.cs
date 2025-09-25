using Gtm.Application.EmailServiceApp.SensEmailApp;
using Gtm.Domain.EmailDomain.SendEmailAgg;
using Gtm.InfraStructure;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.EmailRepo
{
    internal class SendEmailRepository : Repository<SendEmail, int>, ISendEmailRepository
    {
        private readonly GtmDbContext _context;

        public SendEmailRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
