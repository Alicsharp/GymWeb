using ErrorOr;
 
using Gtm.Contract.SiteContract.MenuContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.SiteServiceApp.MenuApp.Query
{
    public record GetForFooterQuery : IRequest<ErrorOr<List<MenuForUi>>>;
    public class GetForFooterQueryHandler : IRequestHandler<GetForFooterQuery, ErrorOr<List<MenuForUi>>>
    {
        private readonly IMenuRepository _menuRepository;

        public GetForFooterQueryHandler(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<ErrorOr<List<MenuForUi>>> Handle(GetForFooterQuery request, CancellationToken cancellationToken)
        {
            List<MenuForUi> model = new();
            var menus = await _menuRepository.GetAllByQueryAsync(b => b.Active &&
            b.Status == MenuStatus.تیتر_منوی_فوتر);
            foreach (var item in menus)
            {
                MenuForUi menu = new()
                {
                    Number = item.Number,
                    Title = item.Title,
                    Url = item.Url,
                    Status = item.Status,
                    Childs = new()
                };
                if (await _menuRepository.ExistsAsync(m => m.ParentId == item.Id && m.Active))
                    menu.Childs = (await _menuRepository.GetAllByQueryAsync(m => m.Active && m.ParentId == item.Id))
                  .Select(m => new MenuForUi
                  {
                      Number = m.Number,
                      Title = m.Title,
                      Url = m.Url,
                      Status = m.Status
                  }).ToList(); //اینجا

                model.Add(menu);
            }
            return model;
        }
    }
}
