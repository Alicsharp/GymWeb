using ErrorOr;
using Gtm.Contract.PostContract.PostSettingContract.Command;

namespace Gtm.Application.PostServiceApp.PostSettingApp
{
    public interface IPostSettingValidation
    {
        ErrorOr<Success> ValidateUbsertCommand(UbsertPostSetting command);
        Task<ErrorOr<Success>> ValidatePostSettingExists();
        Task<ErrorOr<Success>> ValidateUbsertPostSettingAsync(UbsertPostSetting model);
    }
    public class PostSettingValidation : IPostSettingValidation
    {
        private readonly IPostSettingRepo _postSettingRepository;

        public PostSettingValidation(IPostSettingRepo postSettingRepository)
        {
            _postSettingRepository = postSettingRepository;
        }

        public ErrorOr<Success> ValidateUbsertCommand(UbsertPostSetting command)
        {
            var errors = new List<Error>();

            if (command == null)
            {
                errors.Add(Error.Validation(
                    code: "Command.Null",
                    description: "اطلاعات ارسالی نمی‌تواند خالی باشد"));
                return errors;
            }

            if (string.IsNullOrWhiteSpace(command.PackageTitle))
            {
                errors.Add(Error.Validation(
                    code: "PackageTitle.Empty",
                    description: "عنوان پکیج نمی‌تواند خالی باشد"));
            }

            if (string.IsNullOrWhiteSpace(command.PackageDescription))
            {
                errors.Add(Error.Validation(
                    code: "PackageDescription.Empty",
                    description: "توضیحات پکیج نمی‌تواند خالی باشد"));
            }

            if (string.IsNullOrWhiteSpace(command.ApiDescription))
            {
                errors.Add(Error.Validation(
                    code: "ApiDescription.Empty",
                    description: "توضیحات API نمی‌تواند خالی باشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidatePostSettingExists()
        {
            var setting = await _postSettingRepository.GetSingleAsync();
            if (setting == null)
            {
                return Error.NotFound(
                    code: "PostSetting.NotFound",
                    description: "تنظیمات پست یافت نشد");
            }
            return Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateUbsertPostSettingAsync(UbsertPostSetting model)
        {
            // اعتبارسنجی اولیه مدل
            if (model == null)
                return Error.Validation("PostSetting.NullModel", "مدل داده‌ای نمی‌تواند خالی باشد");

            var errors = new List<Error>();

            // اعتبارسنجی عنوان پکیج
            if (string.IsNullOrWhiteSpace(model.PackageTitle))
            {
                errors.Add(Error.Validation(
                    code: "PostSetting.PackageTitleRequired",
                    description: "عنوان صفحه پکیج‌ها الزامی است"));
            }
            else if (model.PackageTitle.Length > 255)
            {
                errors.Add(Error.Validation(
                    code: "PostSetting.PackageTitleTooLong",
                    description: "عنوان صفحه پکیج‌ها نمی‌تواند بیش از 255 کاراکتر باشد"));
            }

            // اعتبارسنجی توضیحات پکیج
            if (!string.IsNullOrWhiteSpace(model.PackageDescription))
            {
                if (model.PackageDescription.Length > 2000)
                {
                    errors.Add(Error.Validation(
                        code: "PostSetting.PackageDescriptionTooLong",
                        description: "توضیحات صفحه پکیج‌ها نمی‌تواند بیش از 2000 کاراکتر باشد"));
                }
            }

            // اعتبارسنجی توضیحات API
            if (!string.IsNullOrWhiteSpace(model.ApiDescription))
            {
                if (model.ApiDescription.Length > 2000)
                {
                    errors.Add(Error.Validation(
                        code: "PostSetting.ApiDescriptionTooLong",
                        description: "توضیحات API نمی‌تواند بیش از 2000 کاراکتر باشد"));
                }
            }

            // اعتبارسنجی یکتایی عنوان (در صورت نیاز)
            //var isTitleUnique = await _postSettingRepository.IsPackageTitleUniqueAsync(model.PackageTitle);
            //if (!isTitleUnique)
            //{
            //    errors.Add(Error.Conflict(
            //        code: "PostSetting.DuplicatePackageTitle",
            //        description: "این عنوان قبلاً استفاده شده است"));
            //}

            return errors.Any() ? errors : Result.Success;
        }
        }
    }
 
 