using System.Text.RegularExpressions;

namespace Utility.Appliation
{
    public static class EmailValidator
    {
        // الگوی اصلی RFC 5322 برای ایمیل
        private const string EmailPattern =
            @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

        private static readonly Regex EmailRegex = new Regex(
            EmailPattern,
            RegexOptions.Compiled | RegexOptions.IgnoreCase,
            TimeSpan.FromMilliseconds(250));

        /// <summary>
        /// بررسی می‌کند که آیا رشته ورودی یک ایمیل معتبر است یا نه
        /// </summary>
        /// <param name="email">آدرس ایمیل برای بررسی</param>
        /// <returns>true اگر ایمیل معتبر باشد، در غیر این صورت false</returns>
        public static bool IsValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                return EmailRegex.IsMatch(email);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// بررسی می‌کند که آیا دامنه ایمیل از دامنه‌های معتبر است (اختیاری)
        /// </summary>
        public static bool HasValidDomain(string email)
        {
            if (!IsValid(email))
                return false;

            try
            {
                var domain = email.Split('@')[1];
                var validDomains = new[] { "gmail.com", "yahoo.com", "outlook.com" }; // لیست دامنه‌های مجاز
                return validDomains.Any(d => domain.EndsWith(d, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }
    }
}
