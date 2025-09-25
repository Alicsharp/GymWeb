using ErrorOr;
using Gtm.Contract.RoleContract.Command;
using Gtm.Domain.UserDomain.UserDm;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Gtm.Application.RoleApp.Command
{
    public record CreateRoleCommand(CreateRole CreateRole, List<UserPermission> Permissions):IRequest<ErrorOr<Success>>;
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ErrorOr<Success>>
    {
        private readonly IRoleRepo _roleRepository;
        private readonly IRoleValidatin _validatin;

        public CreateRoleCommandHandler(IRoleRepo roleRepository, IRoleValidatin validatin)
        {
            _roleRepository = roleRepository;
            _validatin = validatin;
        }

        public async Task<ErrorOr<Success>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validatin.CreateRoleValidation(request.CreateRole.Title, request.Permissions);
            if (validationResult.IsError)
                return validationResult.Errors;

            var role = new Role(request.CreateRole.Title.Trim());

            var roleId = await _roleRepository.CreateRoleAndReturnIdAsync(role);
            if (roleId == null)
                return Error.Failure("RoleCreate", "ایجاد نقش شکست خورد");

            await _roleRepository.AddPermissionsToRoleAsync(roleId.Value, request.Permissions);

            var save = await _roleRepository.SaveChangesAsync(cancellationToken);
            if (!save)
                return Error.Failure("RoleCreate", "ذخیره نهایی نقش و دسترسی‌ها شکست خورد");

            return Result.Success;
        }
    }
}
