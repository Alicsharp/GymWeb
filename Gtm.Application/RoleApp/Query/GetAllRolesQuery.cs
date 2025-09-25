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
    public record GetAllRolesQuery : IRequest<ErrorOr<List<RoleQueryModel>>>;
    public class GetAllRoleQueryHandler : IRequestHandler<GetAllRolesQuery, ErrorOr<List<RoleQueryModel>>>
    {
        private readonly IRoleRepo _roleRepository;

        public GetAllRoleQueryHandler(IRoleRepo roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ErrorOr<List<RoleQueryModel>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _roleRepository.GetAllRolesAsync();

            if (roles == null || !roles.Any())
                return Error.NotFound("Role.None", "هیچ نقشی یافت نشد.");

            return roles;
        }
    }
}
