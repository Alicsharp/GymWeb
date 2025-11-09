using Gtm.Application.ShopApp.SellerApp;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.CityApp.Query
{
    public record GetCityOfSellerQuery(int SellerId) : IRequest<int>;
    public class GetCityOfSellerQueryHandler : IRequestHandler<GetCityOfSellerQuery, int>
    {
        private readonly ICityRepo _cityRepo;

        public GetCityOfSellerQueryHandler(ICityRepo cityRepo)
        {
            _cityRepo = cityRepo;
        }

        public async Task<int> Handle(GetCityOfSellerQuery request, CancellationToken cancellationToken)
        {
            // تمام منطق اکنون به ریپازیتوری سپرده شده است
            return await _cityRepo.GetCityOfSeller(request.SellerId);
        }
    }
}
