using Gtm.Contract.RoleContract.Command;
using Gtm.Contract.RoleContract.Query;
using Gtm.Domain.UserDomain.UserDm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;
using Utility.Domain.Enums;

namespace Gtm.Application.RoleApp
{

    public interface IRoleRepo : IRepository<Role, int>
    {

        //OperationResult EditUserRole(int userId, List<int> roles);
        Task<bool> CheckPermissionAsync(int userId, UserPermission permission);

        Task<bool> CreateRoleAsync(CreateRole command, List<UserPermission> permissions);

        Task<bool> EditRoleAsync(EditRole command, List<UserPermission> permissions);

        Task<List<RoleQueryModel>> GetAllRolesAsync();

        Task<EditRole?> GetForEditAsync(int id);

        Task<List<RolePermissionQueryModel>> GetPermissionsForRoleAsync(int id);
        Task<bool> IsUserAdmin(int userId);

        Task<int?> CreateRoleAndReturnIdAsync( Role role);
        Task AddPermissionsToRoleAsync(int roleId, List<UserPermission> permissions);

    }
}
