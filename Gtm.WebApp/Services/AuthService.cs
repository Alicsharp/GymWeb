using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

using Utility.Contract;
using Utility.Appliation.Auth;

namespace Gtm.WebApp.Services
{
 
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        // متد جدید برای دریافت ایمیل کاربر
        public string GetLoginUserEmail()
        {
            if (IsUserLogin())
            {
                // بررسی وجود claim ایمیل
                var emailClaim = _contextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.Email || x.Type == "Email");

                return emailClaim?.Value ?? string.Empty;
            }
            return string.Empty;
        }

        // سایر متدهای موجود...
        public string GetLoginUserAvatar()
        {
            if (IsUserLogin())
                return _contextAccessor.HttpContext.User.Claims
                    .Single(x => x.Type == "Avatar").Value;

            return "";
        }

        public string GetLoginUserFullName()
        {
            if (IsUserLogin())
                return _contextAccessor.HttpContext.User.Claims
                    .Single(x => x.Type == "FullName").Value;

            return "";
        }

        public int GetLoginUserId()
        {
            if (IsUserLogin())
                return int.Parse(_contextAccessor.HttpContext.User.Claims
                    .Single(x => x.Type == "AuthorUserId").Value);

            return 0;
        }

        public bool IsUserLogin() =>
            _contextAccessor.HttpContext.User.Claims.Any();

        public bool Login(AuthModel command)
        {
            try
            {
                var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, command.Mobile ?? string.Empty),
                new Claim("Avatar", command.Avatar ?? string.Empty),
                new Claim("AuthorUserId", command.UserId.ToString()),
                new Claim("FullName", command.FullName ?? string.Empty),
                new Claim(ClaimTypes.Email, command.Email ?? string.Empty) // اضافه کردن ایمیل به claims
            };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authenticationProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                };

                var httpContext = _contextAccessor.HttpContext;
                if (httpContext == null)
                    return false;

                httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authenticationProperties
                );
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Logout()
        {
            _contextAccessor.HttpContext?.SignOutAsync();
        }
    }
}
