using ErrorOr;
using Gtm.Application.SiteServiceApp.MenuApp;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.SiteServiceApp.MenuApp.Command
{
    public record MenuActivationChangeCommand(int Id) : IRequest<ErrorOr<Success>>;

    public class MenuActivationChangeCommandHandler : IRequestHandler<MenuActivationChangeCommand, ErrorOr<Success>>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IMenuValidator _validator;

        public MenuActivationChangeCommandHandler(IMenuRepository menuRepository,IMenuValidator validator)
        {
            _menuRepository = menuRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<Success>> Handle(MenuActivationChangeCommand request, CancellationToken cancellationToken)
        {
            // Validate command
            var validationResult = await _validator.ValidateActivationChangeAsync(request.Id);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // Get menu
                var menu = await _menuRepository.GetByIdAsync(request.Id);

                // Toggle activation status
                menu.ActivationChange();

                // Save changes
                var result = await _menuRepository.SaveChangesAsync(cancellationToken);

                if (!result)
                {
                    return Error.Failure("Menu.SaveFailed", "ذخیره تغییرات وضعیت منو با خطا مواجه شد.");
                }

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Error.Failure("Menu.OperationFailed", $"خطا در تغییر وضعیت منو: {ex.Message}");
            }
        }
    }
}
