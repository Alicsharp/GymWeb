using ErrorOr;
using Gtm.Contract.PostContract.PostSettingContract.Command;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.PostSettingApp.Command
{
    public record UbsertCommand(UbsertPostSetting Command) : IRequest<ErrorOr<Success>>;

    public class UbsertCommandHandler : IRequestHandler<UbsertCommand, ErrorOr<Success>>
    {
        private readonly IPostSettingRepo _postSettingRepository;
        private readonly IPostSettingValidation _validation;

        public UbsertCommandHandler(IPostSettingRepo postSettingRepository, IPostSettingValidation validation)
        {
            _postSettingRepository = postSettingRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<Success>> Handle(UbsertCommand request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی اولیه
                var validationResult = _validation.ValidateUbsertCommand(request.Command);
                if (validationResult.IsError)
                {
              
                    return validationResult.Errors;
                }

                // بررسی وجود تنظیمات
                var existenceCheck = await _validation.ValidatePostSettingExists();
                if (existenceCheck.IsError)
                {
                    return existenceCheck.Errors;
                }

                // دریافت و ویرایش تنظیمات
                var setting = await _postSettingRepository.GetSingleAsync();
                setting.Edit(
                    request.Command.PackageTitle,
                    request.Command.PackageDescription,
                    request.Command.ApiDescription);

                // ذخیره تغییرات
                var saveResult = await _postSettingRepository.SaveChangesAsync(cancellationToken);

                return saveResult
                    ? Result.Success
                    : Error.Failure(
                        code: "PostSetting.SaveFailed",
                        description: "خطا در ذخیره تنظیمات پست");
            }
            catch (Exception ex)
            {
        
                return Error.Unexpected(
                    code: "PostSetting.UnexpectedError",
                    description: $"خطای غیرمنتظره در بروزرسانی تنظیمات: {ex.Message}");
            }
        }
    }
}
