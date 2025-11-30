using Gtm.Application.EmailServiceApp.EmailUserApp;
using Gtm.Domain.EmailDomain.EmailUserAgg;
using Gtm.InfraStructure;
using Microsoft.EntityFrameworkCore;
using Utility.Infrastuctuer.Repo;


namespace Gtm.InfraStructure.RepoImple.EmailRepo
{
    internal class EmailUserRepository : Repository<EmailUser, int>, IEmailUserRepository
    {
        private readonly GtmDbContext _context;

        public EmailUserRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> CreateListAsync(List<EmailUser> emailUsers)
        {
            if (emailUsers == null || !emailUsers.Any())
                return false;

            await _context.AddRangeAsync(emailUsers);
            return true;

        }

        //public async Task<List<EmailUserViewModel>> GetAllForAdminAsync()
        //{
        //    return await _context.EmailUsers
        //        .Select(e => new EmailUserViewModel
        //        {
        //            Id = e.Id,
        //            Email = e.Email,
        //            Status = e.Status,
        //            CreateDate = e.CreateDate.ToPersianDate()
        //        })
        //        .ToListAsync();
        //}

        public async Task<EmailUser?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _context.EmailUsers.FirstOrDefaultAsync(e => e.Email.Trim().ToLower() == email.Trim().ToLower());
        }
    }

}
