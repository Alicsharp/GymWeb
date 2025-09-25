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
    public record GetForUbsertCommand : IRequest<ErrorOr<UbsertPostSetting>>;
    public class GetForUbsertCommandHandler : IRequestHandler<GetForUbsertCommand, ErrorOr<UbsertPostSetting>>
    {
        private readonly IPostSettingRepo _postSettingRepository;
        private readonly IPostSettingValidation _validation;
     

        public GetForUbsertCommandHandler(IPostSettingRepo postSettingRepository,IPostSettingValidation validation)
        {
            _postSettingRepository = postSettingRepository;
            _validation = validation;
        }

        public async Task<ErrorOr<UbsertPostSetting>> Handle(
            GetForUbsertCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var setting = await _postSettingRepository.GetForUbsertAsync();

                var validationResult =await _validation.ValidateUbsertPostSettingAsync(setting);
                if (validationResult.IsError)
                {
                   
                    return validationResult.Errors;
                }

                return setting;
            }
            catch (Exception ex)
            {
 
                return Error.Unexpected(
                    code: "PostSetting.UnexpectedError",
                    description: $"خطای غیرمنتظره در دریافت تنظیمات پست: {ex.Message}");
            }
        }
    }
}
