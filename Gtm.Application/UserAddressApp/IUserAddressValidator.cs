using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.UserAddressContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.UserAddressApp
{
    public interface IUserAddressValidator
    {
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateAddress dto);
        //Task<ErrorOr<Success>> ValidateUpdateAsync(UpdateUserAddressDto dto);
        Task<ErrorOr<Success>> ValidateDeleteAsync(int id, int userId);
        Task<ErrorOr<Success>> ValidateGetAddressesForUserAsync(int userId);
    }
    public class UserAddressValidator : IUserAddressValidator
    { 
        private readonly IUserAddressRepo _userAddressRepo;
        private readonly IUserRepo _userRepo;

        public UserAddressValidator(IUserAddressRepo userAddressRepo, IUserRepo userRepo)
        {
            _userAddressRepo = userAddressRepo;
            _userRepo = userRepo;
        }

        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateAddress dto)
        {
            var errors = new List<Error>();
            // ✳️ اعتبارسنجی اولیه
            if (dto.UserId <= 0)
                  errors.Add(Error.Validation("Address.User.Invalid", "کاربر معتبر نیست."));
            if (dto.CityId <= 0)
                errors.Add(Error.Validation("Address.CityId.Invalid", "شهر معتبر نیست."));   
            if (dto.StateId <= 0)
                errors.Add(Error.Validation("Address.User.stateId", "استان معتبر نیست."));
            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.AddressDetail))
                errors.Add(Error.Validation("Address.Data.Invalid", "نام و آدرس نباید خالی باشد."));
            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateDeleteAsync(int id, int userId)
        {
            var errors = new List<Error>();
           
            if (id <= 0)
                errors.Add(Error.Validation("Address.Id.Invalid", "کاربر معتبر نیست."));
            if ( userId <= 0)
                errors.Add(Error.Validation("Address.User.Invalid", "کاربر معتبر نیست."));
            var address = await _userAddressRepo.GetByIdAsync(id);
            if (address.UserId != userId)
                errors.Add(Error.NotFound("Address", "شما نمی توانید این ادرس را حذف کنید"));
            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetAddressesForUserAsync(int userId)
        {
            var errors = new List<Error>();

            if (userId <= 0)
                errors.Add(Error.Validation("Address.InvalidUserId", "شناسه کاربر نامعتبر است"));
 
            if (!await _userRepo.ExistsAsync(u => u.Id == userId))
                errors.Add(Error.NotFound("User.NotFound", "کاربر یافت نشد"));

            return errors.Any() ? errors : Result.Success;
        }
    }
}
