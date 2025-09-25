using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.UserAddressContract.Command;
using Gtm.Domain.UserDomain.UserDm;
 
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.UserAddressApp.Command
{
    public record CreateUserAddressCommand(CreateAddress Command) : IRequest<ErrorOr<Success>>;

    public class CreateUserAddressCommandHandler : IRequestHandler<CreateUserAddressCommand, ErrorOr<Success>>
    {
        private readonly IUserAddressRepo _userAdressRepository;
        private readonly IUserAddressValidator _userAddressValidator;

        public CreateUserAddressCommandHandler(IUserAddressRepo userAdressRepository, IUserAddressValidator userAddressValidator)
        {
            _userAdressRepository = userAdressRepository;
            _userAddressValidator = userAddressValidator;
        }

        public async Task<ErrorOr<Success>> Handle(CreateUserAddressCommand request, CancellationToken cancellationToken)
        {
            var model = request.Command;

            var validationResult = await _userAddressValidator.ValidateCreateAsync(model);
            if(validationResult.IsError) return validationResult.Errors;

            // ✳️ ساخت و ثبت
            var address = new UserAddress(
                model.StateId,
                model.CityId,
                model.AddressDetail,
                model.PostalCode,
                model.Phone,
                model.FullName,
                model.IranCode,
                model.UserId
            );

             await _userAdressRepository.AddAsync(address);
            var created=await _userAdressRepository.SaveChangesAsync(cancellationToken);    
            if (!created)
                return Error.Failure("Address.CreateFailed", "ذخیره آدرس با خطا مواجه شد.");

            return Result.Success;
        }
    }
}
