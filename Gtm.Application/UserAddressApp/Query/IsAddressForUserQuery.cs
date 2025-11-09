using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.UserAddressApp.Query
{
    public record IsAddressForUserQuery(int AddressId, int UserId) : IRequest<bool>;
    public class IsAddressForUserQueryHandler : IRequestHandler<IsAddressForUserQuery, bool>
    {
        private readonly IUserAddressRepo _userAdressRepository;

        public IsAddressForUserQueryHandler(IUserAddressRepo userAdressRepository)
        {
            _userAdressRepository = userAdressRepository;
        }

        public async Task<bool> Handle(IsAddressForUserQuery request, CancellationToken cancellationToken)
        {
            var address = await _userAdressRepository.GetByIdAsync(request.AddressId);

            // رفع باگ: اگر آدرس وجود نداشته باشد، قطعاً متعلق به کاربر نیست
            if (address == null)
            {
                return false;
            }

            // بررسی تعلق آدرس به کاربر
            return address.UserId == request.UserId;
        }
    }
}