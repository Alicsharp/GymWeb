using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.AdminDashboard;
using MediatR;
using Utility.Appliation;
using Utility.Appliation.FileService;

namespace Gtm.Application.AdminDashbord.Query
{
    public record GetLastUsersForAdminQuery()
    : IRequest<ErrorOr<List<LastUserAdminQueryModel>>>;
    public class GetLastUsersForAdminQueryHandler
    : IRequestHandler<GetLastUsersForAdminQuery, ErrorOr<List<LastUserAdminQueryModel>>>
    {
        private readonly IUserRepo _userRepository;
        // (فرض می‌کنیم FileDirectories یک کلاس static است)

        public GetLastUsersForAdminQueryHandler(IUserRepo userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ErrorOr<List<LastUserAdminQueryModel>>> Handle(
            GetLastUsersForAdminQuery request, CancellationToken cancellationToken)
        {
            // 1. واکشی انتیتی‌ها از ریپازیتوری
            var users = await _userRepository.GetLast8UsersAsync(cancellationToken);

            if (users == null || !users.Any())
            {
                return new List<LastUserAdminQueryModel>(); // بازگشت لیست خالی
            }

            // 2. انجام مپینگ (منطق Select کد اصلی شما) در هندلر
            var model = users.Select(u => new LastUserAdminQueryModel
            {
                FullName = string.IsNullOrEmpty(u.FullName) ? u.Mobile : u.FullName,
                ImageName = FileDirectories.UserImageDirectory100 + u.Avatar,
                RegisterDate = u.CreateDate.ToPersainDate(),
                UserId = u.Id
            }).ToList();

            // 3. بازگرداندن مدل
            return model;
        }
    }

}
