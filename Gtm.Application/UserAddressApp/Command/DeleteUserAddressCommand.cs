using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.UserAddressApp.Command
{
    public record DeleteUserAddressCommand(int Id, int UserId) : IRequest<ErrorOr<bool>>;
    public class DeleteUserAddressCommandHandler : IRequestHandler<DeleteUserAddressCommand, ErrorOr<bool>>
    {
        private readonly IUserAddressRepo _userAdressRepository;
        private readonly IUserAddressValidator _userAddressValidator;

        public DeleteUserAddressCommandHandler(IUserAddressRepo userAdressRepository, IUserAddressValidator userAddressValidator)
        {
            _userAdressRepository = userAdressRepository;
            _userAddressValidator = userAddressValidator;
        }

        public async Task<ErrorOr<bool>> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _userAddressValidator.ValidateDeleteAsync(request.Id,request.UserId);
            if (validationResult.IsError) return validationResult.Errors;


            var address = await _userAdressRepository.GetByIdAsync(request.Id);

            if (address is null)
                return Error.NotFound("Address.NotFound", "آدرسی با این شناسه پیدا نشد.");

            _userAdressRepository.Remove(address);
            var deleted = await _userAdressRepository.SaveChangesAsync(cancellationToken);
            if (!deleted)
                return Error.Failure("Address.DeleteFailed", "حذف آدرس با خطا مواجه شد.");

            return true;
        }
    }
  }
