using ErrorOr;
using Gtm.Contract.SiteContract.MenuContract.Command;
using MediatR;

namespace Gtm.Application.SiteServiceApp.MenuApp.Command
{
    public record GetForCreateSubMenuCommand(int ParentId) : IRequest<ErrorOr<CreateSubMenu>>;

    public class GetForCreateSubMenuCommandHandler : IRequestHandler<GetForCreateSubMenuCommand, ErrorOr<CreateSubMenu>>
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IMenuValidator _validator;

        public GetForCreateSubMenuCommandHandler(IMenuRepository menuRepository,IMenuValidator validator)
        {
            _menuRepository = menuRepository;
            _validator = validator;
        }

        public async Task<ErrorOr<CreateSubMenu>> Handle(
            GetForCreateSubMenuCommand request,
            CancellationToken cancellationToken)
        {
            // Validate request
            var validationResult = await _validator.ValidateGetForCreateSubMenuAsync(request.ParentId);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            try
            {
                // Get parent menu
                var parent = await _menuRepository.GetByIdAsync(request.ParentId);

                return new CreateSubMenu
                {
                    ImageAlt = "",
                    ImageFile = null,
                    ParentId = parent.Id,
                    ParentStatus = parent.Status,
                    ParentTitle = $"افزودن زیر منو برای {parent.Title}"
                };
            }
            catch (Exception ex)
            {
                return Error.Failure("Menu.QueryFailed", $"خطا در دریافت اطلاعات والد: {ex.Message}");
            }
        }
    }
}
