using ErrorOr;
 
using Utility.Domain.Enums;

namespace Gtm.Application.RoleApp
{
    public interface IRoleValidatin
    {
     Task<ErrorOr<Success>> CreateRoleValidation(string roleName, List<UserPermission> userPermissions);
     Task<ErrorOr<Success>> EditRoleValidation(int roleId, string roleName, List<UserPermission> userPermissions);
     Task<ErrorOr<Success>> CheckPermissionValidation(int UserId, UserPermission Permission);
        Task<ErrorOr<Success>> GetForEditRoleValidation(int Id);


    }
      public class RoleValidation : IRoleValidatin
      {
        private readonly IRoleRepo _roleRepository;

        public RoleValidation(IRoleRepo roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ErrorOr<Success>> CheckPermissionValidation(int UserId, UserPermission Permission)
        {
            var errors = new List<Error>();
               
            if(UserId<=0) errors.Add(Error.NotFound("UserNotFound", " کاربر معتبر نمی باشید"));
            if(Permission == null) errors.Add(Error.NotFound("PermmisonNUll", " مجوز خالی است"));


            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> CreateRoleValidation(string roleName, List<UserPermission> userPermissions)
        {
            var errors = new List<Error>();

            if (string.IsNullOrWhiteSpace(roleName))
                errors.Add(Error.Validation("RoleNameIsRequird", "عنوان مقاله الزامی است."));
            if (userPermissions.Count <= 0) errors.Add(Error.Validation("SelectPermission","لطفا یک مقدار برای مجوز انتخاب کنید"));
            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> EditRoleValidation(int roleId, string roleName, List<UserPermission> userPermissions)
        {
            var errors = new List<Error>();
            if( !await _roleRepository.ExistsAsync(c=>c.Id==roleId))
                errors.Add(Error.NotFound("RoleNotFound","نقش مورد نظر یافت نشد"));
            if (string.IsNullOrWhiteSpace(roleName))
                errors.Add(Error.Validation("RoleNameIsRequird", "عنوان مقاله الزامی است."));
            if (userPermissions.Count <= 0) errors.Add(Error.Validation("SelectPermission", "لطفا یک مقدار برای مجوز انتخاب کنید"));

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> GetForEditRoleValidation(int Id)
        {
            var errors = new List<Error>();
            if (Id <= 0 && !await _roleRepository.ExistsAsync(c => c.Id == Id))
                errors.Add(Error.NotFound("IdNotValid", "ایدی معتبر نمی باشید یا نقشی با این ایدی وجود ندارد"));
            return errors.Any() ? errors : Result.Success;
        }
    }
}
