using Gtm.Contract.PostContract.StateContract.Command;
using Gtm.Contract.PostContract.StateContract.Query;
using Gtm.Domain.PostDomain.StateAgg;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.PostServiceApp.StateApp
{
    public interface IStateRepo : IRepository< State, int>
    {
        Task<List<StateViewModel>> GetAllStateViewModelAsync();
        Task<EditStateModel?> GetStateForEditAsync(int id);
        Task<List<StateQueryModel>> GetStatesWithCityAsync();
        Task<List<StateAdminQueryModel>> GetStatesForAdminAsync();
        Task<List<CityForChooseQueryModel>> GetCitiesForChooseAsync(int stateId);
        Task<List<StateForChooseQueryModel>> GetStatesForChooseAsync();
    }

}
