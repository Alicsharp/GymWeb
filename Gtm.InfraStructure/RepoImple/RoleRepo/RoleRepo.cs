using Gtm.Application.RoleApp;
using Gtm.Contract.RoleContract.Command;
using Gtm.Contract.RoleContract.Query;
using Gtm.Domain.UserDomain.UserDm;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.RoleRepo
{
    public class RoleRepo : Repository<Role, int>, IRoleRepo
    {
        private readonly GtmDbContext _dbContext;

        public RoleRepo(GtmDbContext context) : base(context)
        {
            _dbContext=context;
        }

        public async Task<bool> CheckPermissionAsync(int userId, UserPermission permission)
        {
            var userRoles = await _dbContext.UserRoles
                  .Include(u => u.Role)
                  .ThenInclude(r => r.Permissions)
                  .Where(u => u.UserId == userId)
                  .ToListAsync();

            return userRoles
                .Any(ur => ur.Role.Permissions.Any(p => p.UserPermission == permission));
        }

        public async Task<int?> CreateRoleAndReturnIdAsync(Role role)
        {
            await _dbContext.Roles.AddAsync(role);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0 ? role.Id : null;
        }
        public async Task AddPermissionsToRoleAsync(int roleId, List<UserPermission> permissions)
        {
            var newPermissions = permissions.Select(p => new Permission(roleId, p));
            await _dbContext.Permissions.AddRangeAsync(newPermissions);
        }
        public async Task<bool> CreateRoleAsync(CreateRole command, List<UserPermission> permissions)
        {
            if (command == null || string.IsNullOrWhiteSpace(command.Title)) return false;

            if (await ExistsAsync(r => r.Title.Trim() == command.Title.Trim()))
                return false;

            if (permissions is null || !permissions.Any())
                return false;

            var role = new Role(command.Title.Trim());

                await AddAsync(role);
 

            _dbContext.Permissions.AddRange(
                permissions.Select(p => new Permission(role.Id, p))
            );
                return true;
        }
        public async Task<bool> EditRoleAsync(EditRole command, List<UserPermission> permissions)
        {
            if (await ExistsAsync(r => r.Title.Trim() == command.Title.Trim() && r.Id != command.Id))
                return false;

            if (permissions is null || permissions.Count == 0)
                return false;

            var role = await _dbContext.Roles.Where(i => i.Id == command.Id).FirstOrDefaultAsync();
            if (role is null) return false;

            role.Edit(command.Title.Trim());

            var oldPermissions = await _dbContext.Permissions
                .Where(p => p.RoleId == role.Id)
                .ToListAsync();

            _dbContext.Permissions.RemoveRange(oldPermissions);

            _dbContext.Permissions.AddRange(
                permissions.Select(p => new Permission(role.Id, p))
            );

            return true;
        }

        public async Task<List<RoleQueryModel>> GetAllRolesAsync()
        {
            return await _dbContext.Roles
                .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)

                .Select(r => new RoleQueryModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    UserRoles = r.UserRoles.Select(ur => new UserRoleQueryModel
                    {
                        Id = ur.Id,
                        UserId = ur.UserId,
                        UserAvatar = ur.User.Avatar,
                        UserName = ur.User.FullName ?? ur.User.Mobile
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<EditRole?> GetForEditAsync(int id)
        {
            return await _dbContext.Roles
                 .Where(r => r.Id == id)
                 .Select(r => new EditRole
                 {
                     Id = r.Id,
                     Title = r.Title
                 })
                 .SingleOrDefaultAsync();
        }

        public async Task<List<RolePermissionQueryModel>> GetPermissionsForRoleAsync(int id)
        {
            return await _dbContext.Permissions
           .Where(p => p.RoleId == id)
           .Select(p => new RolePermissionQueryModel
           {
               Id = p.Id,
               RoleId = p.RoleId,
               UserPermission = p.UserPermission
           })
           .ToListAsync();
        }

        public async Task<bool> IsUserAdmin(int userId)
        {
            return await _dbContext.UserRoles.AnyAsync(r => r.UserId == userId);
        }
         
    }
}
  