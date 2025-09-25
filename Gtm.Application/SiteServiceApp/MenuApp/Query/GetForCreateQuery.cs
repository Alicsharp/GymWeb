using ErrorOr;
 
using Gtm.Contract.SiteContract.MenuContract.Command;
using MediatR;
 
namespace Gtm.Application.SiteServiceApp.MenuApp.Query
{
    public record GetForCreateQuery(int parentId) : IRequest<ErrorOr<CreateSubMenu>>;
    public class GetForCreateCommandQuery : IRequestHandler<GetForCreateQuery, ErrorOr<CreateSubMenu>>
    {
        private readonly IMenuRepository _menuRepository;

        public GetForCreateCommandQuery(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<ErrorOr<CreateSubMenu>> Handle(GetForCreateQuery request, CancellationToken cancellationToken)
        {
            if (request.parentId == null)
            {
                return Error.NotFound("Menu.Id", "یافت نشد");
            }
            var parent = await _menuRepository.GetByIdAsync(request.parentId);
            return new CreateSubMenu()
            {
                ImageAlt = "",
                ImageFile = null,
                ParentId = parent.Id,
                ParentStatus = parent.Status,
                ParentTitle = $"افزودن زیر منو برای {parent.Title}"

            };
        }
    }
}
