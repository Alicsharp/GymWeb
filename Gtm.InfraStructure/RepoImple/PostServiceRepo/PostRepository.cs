using Gtm.Application.PostServiceApp.PostApp;
using Gtm.Contract.PostContract.PostCalculateContract.Query;
using Gtm.Contract.PostContract.PostContract.Command;
using Gtm.Contract.PostContract.PostContract.Query;
using Gtm.Domain.PostDomain.Postgg;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.PostServiceRepo
{
    internal class PostRepository : Repository<Post,int>, IPostRepo
    {
        private readonly GtmDbContext _context;
        public PostRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<PostModel>> GetAllPosts()
        {
            return await _context.Posts
                .Select(p => new PostModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Status = p.Status,
                    Active = p.Active,
                    CreationDate = p.CreateDate.ToPersainDate(), // استفاده از متد توسعه‌ای برای تاریخ فارسی
                    CityPricePlus = p.CityPricePlus,
                    InsideStatePricePlus = p.InsideStatePricePlus,
                    StateCenterPricePlus = p.StateCenterPricePlus,
                    StateClosePricePlus = p.StateClosePricePlus,
                    StateNonClosePricePlus = p.StateNonClosePricePlus,
                    TehranPricePlus = p.TehranPricePlus,
                    InsideCity = p.InsideCity,
                    OutsideCity = p.OutSideCity
                })
                .AsNoTracking() // بهبود عملکرد برای کوئری‌های فقط خواندنی
                .ToListAsync();
        }

        public List<PostAdminQueryModel> GetAllPostsForAdmin()
        {
            throw new NotImplementedException();
        }

        public async Task<EditPost> GetForEditAsync(int id)
        {
            return await _context.Posts.Select(p => new EditPost
            {
                CityPricePlus = p.CityPricePlus,
                Id = p.Id,
                InsideStatePricePlus = p.InsideStatePricePlus,
                StateCenterPricePlus = p.StateCenterPricePlus,
                StateClosePricePlus = p.StateClosePricePlus,
                StateNonClosePricePlus = p.StateNonClosePricePlus,
                Status = p.Status,
                TehranPricePlus = p.TehranPricePlus,
                Title = p.Title,
                Description = p.Description
            }).SingleOrDefaultAsync(p => p.Id == id);
        }

        public PostAdminDetailQueryModel GetPostDetails(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<PostPriceResponseModel>> CalculatePostAsync(PostPriceRequestModel command)
        {
            List<PostPriceResponseModel> model = new();
            IQueryable<Post> posts = _context.Posts.Include(p => p.PostPrices);
            List<Post> posts1 = new();
            CalculatePost calculatePost = await GetCalculatePostAsync(command);
            switch (calculatePost)
            {
                case CalculatePost.درون_شهری:
                    posts1 = await posts.Where(p => p.InsideCity).ToListAsync();
                    break;
                case CalculatePost.تهران:
                    posts1 = await posts.Where(p => p.InsideCity).ToListAsync();
                    break;
                case CalculatePost.مرکز_استان:
                    posts1 = await posts.Where(p => p.InsideCity).ToListAsync();
                    break;
                case CalculatePost.درون_استانی:
                    posts1 = await posts.Where(p => p.OutSideCity).ToListAsync();
                    break;
                case CalculatePost.هم_جوار:
                    posts1 = await posts.Where(p => p.OutSideCity).ToListAsync();
                    break;
                case CalculatePost.غیر_هم_جوار:
                    posts1 = await posts.Where(p => p.OutSideCity).ToListAsync();
                    break;
                case CalculatePost.هیچکدام:
                    break;
                default:
                    break;
            }
            if (posts1.Count() > 0)
            {
                foreach (var item in posts1)
                {
                    int price = item.Calculate(calculatePost, command.Weight);
                    PostPriceResponseModel postPrice = new(item.Title, item.Status, price);
                    model.Add(postPrice);
                }
            }



            return model;
        }

        private async Task<CalculatePost> GetCalculatePostAsync(PostPriceRequestModel command)
        {

            var sourceCity = await _context.Cities.Include(c => c.State).SingleOrDefaultAsync(c => c.Id == command.SourceCityId);
            var destinationCity = await _context.Cities.Include(c => c.State).SingleOrDefaultAsync(c => c.Id == command.DestinationCityId);
            if (sourceCity == null || destinationCity == null) return CalculatePost.هیچکدام;
            if (command.SourceCityId == command.DestinationCityId)
            {
                switch (sourceCity.Status)
                {
                    case CityStatus.تهران: return CalculatePost.تهران;
                    case CityStatus.مرکز_استان: return CalculatePost.مرکز_استان;
                    case CityStatus.شهرستان_معمولی: return CalculatePost.درون_شهری;
                    default: return CalculatePost.هیچکدام;
                }
            }
            else
            {
                if (sourceCity.StateId == destinationCity.StateId)
                    return CalculatePost.درون_استانی;
                else
                {
                    if (sourceCity.State.CloseStates.StartsWith($"{destinationCity.StateId}-") ||
                        sourceCity.State.CloseStates.Contains($"-{destinationCity.StateId}-") ||
                        sourceCity.State.CloseStates.EndsWith($"_{destinationCity.StateId}"))
                        return CalculatePost.هم_جوار;
                    else return CalculatePost.غیر_هم_جوار;
                }
            }
        }
    }
}
