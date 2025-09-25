using Gtm.Application.PostServiceApp.StateApp;
using Gtm.Contract.PostContract.StateContract.Command;
using Gtm.Contract.PostContract.StateContract.Query;
using Gtm.Domain.PostDomain.StateAgg;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.PostServiceRepo
{
    internal class StateRepository : Repository<State,int>, IStateRepo
    {
        private readonly GtmDbContext _context;

        public StateRepository(GtmDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<StateViewModel>> GetAllStateViewModelAsync()
        {
            return await _context.States
                .Select(s => new StateViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    CreateDate = s.CreateDate.ToString("yyyy/MM/dd HH:mm") // فرمت استاندارد تاریخ
                })
                .AsNoTracking() // بهبود عملکرد برای داده‌های فقط خواندنی
                .ToListAsync();
        }

        public Task<List<CityForChooseQueryModel>> GetCitiesForChooseAsync(int stateId)
        {
            return _context.Cities.Where(c => c.StateId == stateId)
            .Select(c => new CityForChooseQueryModel()
            {
                CityCode = c.Id,
                Title = c.Title
            }).ToListAsync();

        }

        public async Task<EditStateModel?> GetStateForEditAsync(int id)
        {
            var state = await GetByIdAsync(id);
            if (state == null) return null;

            return new EditStateModel
            {
                Id = state.Id,
                Title = state.Title
            };
        }

        public async Task<List<StateAdminQueryModel>> GetStatesForAdminAsync()
        {
            return await _context.States
                .Include(s => s.Cities)
                .Select(s => new StateAdminQueryModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    CreateDate = s.CreateDate.ToPersainDate(),
                    CityCount = s.Cities.Count()
                })
                .ToListAsync();
        }

        public Task<List<StateForChooseQueryModel>> GetStatesForChooseAsync()
        {
            return _context.States.Select(s => new StateForChooseQueryModel
            {
                Id = s.Id,
                Title = s.Title
            }).ToListAsync();
        }

        public async Task<List<StateQueryModel>> GetStatesWithCityAsync()
        {
            return await _context.States
                .Include(s => s.Cities)
                .Select(s => new StateQueryModel
                {
                    Name = s.Title,
                    Cities = s.Cities.Select(c => new CityQueryModel
                    {
                        CityCode = c.Id,
                        Name = c.Title
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}
