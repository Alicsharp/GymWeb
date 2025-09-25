using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Contract;

namespace Utility.Appliation.Auth
{
    public interface IAuthService
    {
        bool Login(AuthModel command);
        void Logout();
        int GetLoginUserId();
        string GetLoginUserAvatar();
        string GetLoginUserEmail();
        string GetLoginUserFullName();
        bool IsUserLogin();
    }
}
