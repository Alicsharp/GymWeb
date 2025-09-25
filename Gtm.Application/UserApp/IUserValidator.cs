using ErrorOr;
using Gtm.Contract.UserContract.Command;
using Gtm.Contract.UserContract.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation;

namespace Gtm.Application.UserApp
{
    public interface IUserValidator
    {
        Task<ErrorOr<Success>> ValidateIdAsync(int id);
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateUserDto dto);
        Task<ErrorOr<Success>> ValidateUpdateByAdminAsync(EditUserByAdmin dto);
        Task<ErrorOr<Success>> ValidateGetUserInfoForPanelAsync(int userId);
        Task<ErrorOr<Success>> ValidateGetForEditByUserAsync(int userId);
        Task<ErrorOr<Success>> ValidateEditByUserAsync(EditUserByUser editUser);
    }
    public class UserValidator : IUserValidator
    {
        public async Task<ErrorOr<Success>> ValidateIdAsync(int id)
        {
            var errors = new List<Error>();
            if (id < 0 && id == null)
            {
                return Error.Validation("Category.Id.Invalid", "شناسه دسته‌بندی معتبر نیست.");
            }
            return errors.Count > 0 ? errors : Result.Success;
        }
        private readonly IUserRepo _userRepo;

        public UserValidator(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateUserDto dto)
        {
            var errors = new List<Error>();

            // اعتبارسنجی نام کامل
            if (string.IsNullOrWhiteSpace(dto.FullName))
                errors.Add(Error.Validation("User.FullNameRequired", "نام کامل الزامی است."));

            // اعتبارسنجی موبایل
            if (string.IsNullOrWhiteSpace(dto.Mobile))
                errors.Add(Error.Validation("User.MobileRequired", "شماره موبایل الزامی است."));
            else if (await _userRepo.ExistsAsync(u => u.Mobile.Trim() == dto.Mobile.Trim()))
                errors.Add(Error.Conflict("User.MobileExists", "شماره موبایل وارد شده قبلاً ثبت شده است."));

            // اعتبارسنجی ایمیل (اختیاری)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                if (await _userRepo.ExistsAsync(u => u.Email.ToLower().Trim() == dto.Email.ToLower().Trim()))
                    errors.Add(Error.Conflict("User.EmailExists", "ایمیل وارد شده قبلاً ثبت شده است."));
            }

            // اعتبارسنجی رمز عبور
            if (string.IsNullOrWhiteSpace(dto.Password))
                errors.Add(Error.Validation("User.PasswordRequired", "رمز عبور الزامی است."));
            else if (dto.Password.Length <=5)
                errors.Add(Error.Validation("User.PasswordTooShort", "رمز عبور باید حداقل 5 کاراکتر باشد."));

            // اعتبارسنجی تصویر آواتار (در صورت وجود)
            if (dto.AvatarFile is not null && !dto.AvatarFile.IsImage())
                errors.Add(Error.Validation("User.AvatarInvalid", "تصویر آپلودی معتبر نیست."));

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateUpdateByAdminAsync(EditUserByAdmin dto)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه کاربر
            var idValidation = await ValidateIdAsync(dto.Id);
            if (idValidation.IsError)
                errors.AddRange(idValidation.Errors);

            // اعتبارسنجی نام کامل
            if (string.IsNullOrWhiteSpace(dto.FullName))
                errors.Add(Error.Validation("User.FullNameRequired", "نام کامل الزامی است."));

            // اعتبارسنجی موبایل
            if (string.IsNullOrWhiteSpace(dto.Mobile))
                errors.Add(Error.Validation("User.MobileRequired", "شماره موبایل الزامی است."));
            else if (await _userRepo.ExistsAsync(u => u.Mobile.Trim() == dto.Mobile.Trim() && u.Id != dto.Id))
                errors.Add(Error.Conflict("User.MobileExists", "شماره موبایل وارد شده قبلاً ثبت شده است."));

            // اعتبارسنجی ایمیل (اختیاری)
            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                await _userRepo.ExistsAsync(u => u.Email.ToLower().Trim() == dto.Email.ToLower().Trim() && u.Id != dto.Id))
                errors.Add(Error.Conflict("User.EmailExists", "ایمیل وارد شده قبلاً ثبت شده است."));

            // اعتبارسنجی تصویر آواتار (در صورت وجود)
            if (dto.AvatarFile is not null && !dto.AvatarFile.IsImage())
                errors.Add(Error.Validation("User.AvatarInvalid", "تصویر آپلودی معتبر نیست."));

            // اعتبارسنجی رمز عبور جدید (در صورت وجود)
            if (!string.IsNullOrWhiteSpace(dto.Password) && dto.Password.Length < 6)
                errors.Add(Error.Validation("User.PasswordTooShort", "رمز عبور باید حداقل 6 کاراکتر باشد."));

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetUserInfoForPanelAsync(int userId)
        {
            var errors = new List<Error>();

            if (userId <= 0)
                errors.Add(Error.Validation("User.InvalidId", "شناسه کاربر نامعتبر است."));

            if (!await _userRepo.ExistsAsync(u => u.Id == userId))
                errors.Add(Error.NotFound("User.NotFound", "کاربر مورد نظر یافت نشد."));

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEditByUserAsync(int userId)
        {
            var errors = new List<Error>();

            if (userId <= 0)
                errors.Add(Error.Validation("User.InvalidId", "شناسه کاربر نامعتبر است."));

            if (!await _userRepo.ExistsAsync(u => u.Id == userId))
                errors.Add(Error.NotFound("User.NotFound", "کاربر مورد نظر یافت نشد."));

            // می‌توانید شرایط اضافی مانند بررسی فعال بودن کاربر را اینجا اضافه کنید
            // var user = await _userRepository.GetByIdAsync(userId);
            // if(user.IsActive == false) {...}

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateEditByUserAsync(EditUserByUser editUser)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه کاربر
            if (editUser.Id <= 0)
                errors.Add(Error.Validation("User.InvalidId", "شناسه کاربر نامعتبر است."));

            // اعتبارسنجی نام کامل
            if (string.IsNullOrWhiteSpace(editUser.FullName))
                errors.Add(Error.Validation("User.FullName.Required", "نام کامل الزامی است."));
            else if (editUser.FullName.Length > 100)
                errors.Add(Error.Validation("User.FullName.TooLong", "نام کامل نمی‌تواند بیشتر از 100 کاراکتر باشد."));

            // اعتبارسنجی موبایل
            if (string.IsNullOrWhiteSpace(editUser.Mobile))
                errors.Add(Error.Validation("User.Mobile.Required", "شماره موبایل الزامی است."));
            else if (!IsValidMobile(editUser.Mobile))
                errors.Add(Error.Validation("User.Mobile.Invalid", "فرمت شماره موبایل معتبر نیست."));

            // اعتبارسنجی ایمیل (در صورتی که وجود دارد)
            if (!string.IsNullOrWhiteSpace(editUser.Email) && !IsValidEmail(editUser.Email))
                errors.Add(Error.Validation("User.Email.Invalid", "فرمت ایمیل معتبر نیست."));

            // بررسی وجود کاربر
            var userExists = await _userRepo.ExistsAsync(u => u.Id == editUser.Id);
            if (!userExists)
                errors.Add(Error.NotFound("User.NotFound", "کاربر مورد نظر یافت نشد."));

            // بررسی تکراری نبودن موبایل
            var mobileExists = await _userRepo.ExistsAsync(u =>
                u.Mobile.Trim() == editUser.Mobile.Trim() &&
                u.Id != editUser.Id);
            if (mobileExists)
                errors.Add(Error.Conflict("User.Mobile.Exists", "شماره موبایل قبلاً ثبت شده است."));

            // بررسی تکراری نبودن ایمیل (در صورتی که وجود دارد)
            if (!string.IsNullOrWhiteSpace(editUser.Email))
            {
                var emailExists = await _userRepo.ExistsAsync(u =>
                    u.Email.ToLower().Trim() == editUser.Email.ToLower().Trim() &&
                    u.Id != editUser.Id);
                if (emailExists)
                    errors.Add(Error.Conflict("User.Email.Exists", "ایمیل قبلاً ثبت شده است."));
            }

            return errors.Any() ? errors : Result.Success;
        }

        private bool IsValidMobile(string mobile)
        {
            // پیاده‌سازی منطق اعتبارسنجی موبایل
            return !string.IsNullOrWhiteSpace(mobile) && mobile.Length == 11 && mobile.StartsWith("09");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
