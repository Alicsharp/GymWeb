using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.RoleApp.Command
{
    public record CheckPermissionCommand(int UserId, UserPermission Permission) : IRequest<ErrorOr<Success>>;

    public class CheckPermissionCommandHandler : IRequestHandler<CheckPermissionCommand, ErrorOr<Success>>
    {
        private readonly IRoleRepo _roleRepository;
        private readonly IRoleValidatin _roleValidatin;

        public CheckPermissionCommandHandler(IRoleRepo roleRepository, IRoleValidatin roleValidatin)
        {
            _roleRepository = roleRepository;
            _roleValidatin = roleValidatin;
        }

        public async Task<ErrorOr<Success>> Handle(CheckPermissionCommand request, CancellationToken cancellationToken)
        {
            var validationresult = await _roleValidatin.CheckPermissionValidation(request.UserId, request.Permission);
            if(validationresult.IsError) return validationresult.Errors;

            var hasPermission = await _roleRepository.CheckPermissionAsync(request.UserId, request.Permission);
            if (!hasPermission) return Error.Failure("CheckPermisson", "عملیات بررسی مجوز شکست خورد");
             
           
            return Result.Success;

        }
    }

}
