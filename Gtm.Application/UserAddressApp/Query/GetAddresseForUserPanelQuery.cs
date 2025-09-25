using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp;
using Gtm.Application.PostServiceApp.StateApp;
using Gtm.Contract.UserAddressContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.UserAddressApp.Query
{
    public record GetAddresseForUserPanelQuery(int userId) : IRequest<ErrorOr<List<UserAddressForPanelQueryModel>>>;
    public class GetAddresseForUserPanelQueryHandler : IRequestHandler<GetAddresseForUserPanelQuery, ErrorOr<List<UserAddressForPanelQueryModel>>>
    {
        private readonly IUserAddressRepo _userAdressRepository;
        private readonly IStateRepo _stateRepository;
        private readonly ICityRepo _cityRepository;
        private readonly IUserAddressValidator _userAddressValidator;

        public GetAddresseForUserPanelQueryHandler(IUserAddressRepo userAdressRepository, IStateRepo stateRepository, ICityRepo cityRepository, IUserAddressValidator userAddressValidator)
        {
            _userAdressRepository = userAdressRepository;
            _stateRepository = stateRepository;
            _cityRepository = cityRepository;
            _userAddressValidator = userAddressValidator;
        }

        public async Task<ErrorOr<List<UserAddressForPanelQueryModel>>> Handle(GetAddresseForUserPanelQuery request, CancellationToken cancellationToken)
        {
            var validationresult = await _userAddressValidator.ValidateGetAddressesForUserAsync(request.userId);

            if(validationresult.IsError)
            {
                return validationresult.Errors;
            }
            var addresses = await _userAdressRepository.QueryBy(a => a.UserId == request.userId).OrderByDescending(a => a.Id)
          .Select(a => new UserAddressForPanelQueryModel
          {
              Id = a.Id,
              AddressDetail = a.AddressDetail,
              CityId = a.CityId,
              CityName = "",
              CreationDate = a.CreateDate.ToPersainDate(),
              FullName = a.FullName,

              IranCode = a.IranCode,
              Phone = a.Phone,
              PostalCode = a.PostalCode,
              StateId = a.StateId,
              StateName = ""
          }).ToListAsync();

            foreach (var address in addresses)
            {
                var city = await _cityRepository.GetByIdAsync(address.CityId);
                var state = await _stateRepository.GetByIdAsync(address.StateId);

                address.CityName = city?.Title ?? "نامعلوم";
                address.StateName = state?.Title ?? "نامعلوم";
            }
            return addresses;

        }
    }
}
