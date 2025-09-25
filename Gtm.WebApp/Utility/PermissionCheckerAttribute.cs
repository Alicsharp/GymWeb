using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Utility.Domain.Enums;
using MediatR;
using Gtm.Application.RoleApp.Command;

namespace Gtm.WebApp.Utility
{
    internal class PermissionCheckerAttribute : AuthorizeAttribute, IAuthorizationFilter
    { 
        private readonly IMediator _mediator;
        private readonly UserPermission _permissionId;

        public PermissionCheckerAttribute(IMediator mediator, UserPermission permissionId)
        {
            _mediator = mediator;
            _permissionId = permissionId;
        }

        public async void OnAuthorization(AuthorizationFilterContext context)
        {
            //_roleQuery = (IRoleQuery)context.HttpContext.RequestServices.
            //   GetService(typeof(IRoleQuery));
            if (context.HttpContext.User.Claims.Count() > 0)
            {
                string userId = context.HttpContext.User.Claims.
                FirstOrDefault(x => x.Type == "UserId").Value;
                var res = await _mediator.Send(new CheckPermissionCommand(int.Parse(userId), _permissionId));
                if (res.IsError)
                    context.Result = new RedirectResult("/Auth/AccessDenied");
            }
            else
                context.Result = new RedirectResult("/Auth/Login");
        }
    }
}
