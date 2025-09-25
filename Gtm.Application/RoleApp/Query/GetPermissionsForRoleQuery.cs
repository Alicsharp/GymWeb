using ErrorOr;
using Gtm.Contract.RoleContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.RoleApp.Query
{
    public record GetPermissionsForRoleQuery(int Id) : IRequest<ErrorOr<List<RolePermissionQueryModel>>>;
    public class GetPermissionsForRoleQueryHandler : IRequestHandler<GetPermissionsForRoleQuery, ErrorOr<List<RolePermissionQueryModel>>>
    {
        private readonly IRoleRepo _roleRepository;

        public GetPermissionsForRoleQueryHandler(IRoleRepo roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ErrorOr<List<RolePermissionQueryModel>>> Handle(GetPermissionsForRoleQuery request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return Error.Validation("Role.Id.Invalid", "شناسه نقش معتبر نیست.");

            var permissions = await _roleRepository.GetPermissionsForRoleAsync(request.Id);

            if (permissions == null || !permissions.Any())
                return Error.NotFound("Role.Permissions.Empty", "هیچ دسترسی‌ای برای این نقش ثبت نشده است.");

            return permissions;
        }
    }
}
