using Gtm.Application.EmailServiceApp.MessageUserApp;
using Gtm.Domain.EmailDomain.MessageUserAgg;
using Gtm.InfraStructure;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.EmailRepo
{
    internal class MessageUserRepository : Repository<MessageUser, int>, IMessageUserRepository
    {
        private readonly GtmDbContext _context;

        public MessageUserRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
