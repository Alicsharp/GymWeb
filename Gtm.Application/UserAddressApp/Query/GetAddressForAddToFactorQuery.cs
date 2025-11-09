using Gtm.Contract.UserAddressContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.UserAddressApp.Query
{
    public record GetAddressForAddToFactorQuery(int AddressId) : IRequest<CreateAddress?>;
    public class GetAddressForAddToFactorQueryHandler : IRequestHandler<GetAddressForAddToFactorQuery, CreateAddress?>
    {
        private readonly IUserAddressRepo _userAdressRepository;

        public GetAddressForAddToFactorQueryHandler(IUserAddressRepo userAdressRepository)
        {
            _userAdressRepository = userAdressRepository;
        }

        public async Task<CreateAddress?> Handle(GetAddressForAddToFactorQuery request, CancellationToken cancellationToken)
        {
            var address = await _userAdressRepository.GetByIdAsync(request.AddressId);

            // 1. بررسی حالت "یافت نشد" (رفع باگ کد اصلی)
            if (address == null)
            {
                return null;
            }

            // 2. نگاشت (Mapping) انتیتی به DTO
            return new CreateAddress
            {
                AddressDetail = address.AddressDetail,
                CityId = address.CityId,
                FullName = address.FullName,
                IranCode = address.IranCode,
                Phone = address.Phone,
                PostalCode = address.PostalCode,
                StateId = address.StateId
            };
        }
    }
}
