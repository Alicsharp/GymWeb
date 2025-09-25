using ErrorOr;
using Gtm.Contract.RoleContract.Command;
using MediatR;
using Utility.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Gtm.Application.RoleApp.Command
{
    public record EditRoleCommand(EditRole Command, List<UserPermission> Permissions) : IRequest<ErrorOr<Success>>;
    public class EditRoleCommandHandler : IRequestHandler<EditRoleCommand, ErrorOr<Success>>
    {
        private readonly IRoleRepo _roleRepository;
        private readonly IRoleValidatin _roleValidatin;

        public EditRoleCommandHandler(IRoleRepo roleRepository, IRoleValidatin roleValidatin)
        {
            _roleRepository = roleRepository;
            _roleValidatin = roleValidatin;
        }

        public async Task<ErrorOr<Success>> Handle(EditRoleCommand request, CancellationToken cancellationToken)
        {
            var validationresult = await _roleValidatin.EditRoleValidation(request.Command.Id,request.Command.Title, request.Permissions);
            if (validationresult.IsError) return validationresult.Errors;

            var edit = await _roleRepository.EditRoleAsync(request.Command, request.Permissions);
            if (!edit) return Error.Failure("Failure", "عملیات ادیت شکست خورد");
            var result= await _roleRepository.SaveChangesAsync(cancellationToken);
            if(!result) return Error.Failure("Failure", "عملیات ادیت شکست خورد");
             return Result.Success;


        }
    }
}
