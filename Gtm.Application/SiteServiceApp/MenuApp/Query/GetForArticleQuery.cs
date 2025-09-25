using ErrorOr;
using Gtm.Contract.SiteContract.MenuContract.Query;
using MediatR;
using Utility.Appliation.FileService;
using Utility.Domain.Enums;


namespace Gtm.Application.SiteServiceApp.MenuApp.Query
{
    public record GetForArticleQuery : IRequest<ErrorOr<List<MenuForUi>>>;
    public class GetForArticleQueryHandler : IRequestHandler<GetForArticleQuery, ErrorOr<List<MenuForUi>>>
    {
        private readonly IMenuRepository _menuRepository;

        public GetForArticleQueryHandler(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public async Task<ErrorOr<List<MenuForUi>>> Handle(GetForArticleQuery request, CancellationToken cancellationToken)
        {
            List<MenuForUi> model = new();

            // دریافت منوها به صورت غیرهمزمان
            var menus = await _menuRepository.GetAllByQueryAsync(b => b.Active &&
                (b.Status == MenuStatus.منوی_وبلاگ_لینک ||
                 b.Status == MenuStatus.منوی_وبلاگ_با_زیرمنوی_بدون_عکس ||
                 b.Status == MenuStatus.منوی_وبلاگ_با_زیر_منوی_عکس_دار));

            // برای هر منو، زیرمنوهای آن را به صورت جداگانه بارگذاری می‌کنیم
            foreach (var item in menus)
            {
                MenuForUi menu = new()
                {
                    Number = item.Number,
                    Title = item.Title,
                    Url = item.Url,
                    Status = item.Status,
                    Childs = new List<MenuForUi>()
                };

                // بررسی اینکه زیرمنوها وجود دارند یا نه
                var subMenus = await _menuRepository.GetAllByQueryAsync(m => m.Active && m.ParentId == item.Id);
                menu.Childs = subMenus.Select(m => new MenuForUi
                {
                    ImageAlt = m.ImageAlt,
                    Childs = new List<MenuForUi>(), // می‌توانید در صورت نیاز این را تغییر دهید
                    ImageName = FileDirectories.MenuImageDirectory + m.ImageName,
                    Number = m.Number,
                    Title = m.Title,
                    Url = m.Url,
                    Status = m.Status
                }).ToList();

                model.Add(menu);
            }

            return model;
        }
    }

}
