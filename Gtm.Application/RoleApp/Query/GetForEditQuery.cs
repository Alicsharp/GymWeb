using ErrorOr;
using Gtm.Contract.RoleContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.RoleApp.Query
{
    public record GetForEditQuery(int Id) : IRequest<ErrorOr<EditRole>>;

    public class GetForEditQueryHandler : IRequestHandler<GetForEditQuery, ErrorOr<EditRole>>
    {
        private readonly IRoleRepo _roleRepository;
        private readonly IRoleValidatin _roleValidatin;

        public GetForEditQueryHandler(IRoleRepo roleRepository, IRoleValidatin roleValidatin)
        {
            _roleRepository = roleRepository;
            _roleValidatin = roleValidatin;
        }

        public async Task<ErrorOr<EditRole>> Handle(GetForEditQuery request, CancellationToken cancellationToken)
        {
            var validationResult = await _roleValidatin.GetForEditRoleValidation(request.Id);
            if (validationResult.IsError) return validationResult.Errors;

            var role = await _roleRepository.GetForEditAsync(request.Id);

            if (role is null)
                return Error.NotFound("Role.NotFound", "نقشی با این شناسه یافت نشد.");

            return role;
        }
    }


}
