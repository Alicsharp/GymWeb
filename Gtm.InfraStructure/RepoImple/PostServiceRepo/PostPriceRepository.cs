using Gtm.Application.PostServiceApp.PostPriceApp;
using Gtm.Contract.PostContract.PostPriceContract.Command;
using Gtm.Contract.PostContract.PostPriceContract.Query;
using Gtm.Domain.PostDomain.PostPriceAgg;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.PostServiceRepo
{
    internal class PostPriceRepository : Repository<PostPrice,int>, IPostPriceRepo
    {
        private readonly GtmDbContext _context;
        public PostPriceRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<PostPriceModel>> GetAllForPostAsync(int postId)
        {
            return await QueryBy(p => p.PostId == postId)
                .Select(p => new PostPriceModel
                {
                    CityPrice = p.CityPrice,
                    End = p.End,
                    Id = p.Id,
                    InsideStatePrice = p.InsideStatePrice,
                    Start = p.Start,
                    StateCenterPrice = p.StateCenterPrice,
                    StateClosePrice = p.StateClosePrice,
                    StateNonClosePrice = p.StateNonClosePrice,
                    TehranPrice = p.TehranPrice
                })
                .ToListAsync(); // نیاز به `Microsoft.EntityFrameworkCore`
        }

        public async Task<EditPostPrice> GetForEditAsync(int id)
        {
            return await _context.PostPrices.Select(p => new EditPostPrice
            {
                CityPrice = p.CityPrice,
                End = p.End,
                Id = p.Id,
                InsideStatePrice = p.InsideStatePrice,
                Start = p.Start,
                StateCenterPrice = p.StateCenterPrice,
                StateClosePrice = p.StateClosePrice,
                StateNonClosePrice = p.StateNonClosePrice,
                TehranPrice = p.TehranPrice
            }).SingleOrDefaultAsync(p => p.Id == id);
        }
    }
}
