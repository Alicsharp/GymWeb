using Gtm.Contract.PostContract.CityContract.Command;
using Gtm.Contract.PostContract.CityContract.Query;
using Gtm.Domain.PostDomain.CityAgg;
using Gtm.Domain.ShopDomain.OrderDomain.OrderAddressDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;
using Utility.Domain.Enums;

namespace Gtm.Application.PostServiceApp.CityApp
{
    public interface ICityRepo:IRepository<City,int>
    {
        Task<List<CityViewModel>> GetAllForStateAsync(int stateId);
        Task<EditCityModel?> GetCityForEditAsync(int id);
        Task<bool> ChangeStatusAsync(int id, CityStatus status); 
        Task<City?> GetCityWithStateAsync(Expression<Func<City, bool>> predicate);
        Task<City?> GetCityWithStateByIdAsync(int Id);
        Task<int> GetCityOfSeller(int sellerId);
        Task<List<City>> GetCitiesWithStateAsync(IEnumerable<int> cityIds);
        Task<City> GetCityWithStateAsync(int CityId, int StateId);


    }
}
