using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.EmailContract.MessageUserContract.Query;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Utility.Appliation;
using Utility.Domain.Enums;

namespace Gtm.Application.EmailServiceApp.MessageUserApp.Query
{
    public record GetMessagesForAdminQuery(int pageId, int take, string filter, MessageStatus status) : IRequest<ErrorOr<MessageUserAdminPaging>>;
    public class GetMessagesForAdminQueryHandler : IRequestHandler<GetMessagesForAdminQuery, ErrorOr<MessageUserAdminPaging>>
    {
        private readonly IMessageUserRepository _messageUserRepository;
        private readonly IUserRepo _userRepository;

        public GetMessagesForAdminQueryHandler(IMessageUserRepository messageUserRepository,IUserRepo userRepository)
        {
            _messageUserRepository = messageUserRepository;
            _userRepository = userRepository;
        }

        public async Task<ErrorOr<MessageUserAdminPaging>> Handle(GetMessagesForAdminQuery request,CancellationToken cancellationToken)
        {
            // ایجاد کوئری پایه
            var query = _messageUserRepository.GetAllQueryable();

            // اعمال فیلتر وضعیت
            if (request.status != MessageStatus.همه)
            {
                query = query.Where(r => r.Status == request.status);
            }

            // اعمال فیلتر جستجو
            if (!string.IsNullOrEmpty(request.filter))
            {
                var filter = request.filter.ToLower();
                query = query.Where(r =>
                    r.Email.ToLower().Contains(filter) ||
                    r.Message.ToLower().Contains(filter) ||
                    r.PhoneNumber.Contains(request.filter) ||
                    r.FullName.ToLower().Contains(filter));
            }

            // مرتب سازی نهایی
            var orderedQuery = query.OrderByDescending(m => m.Id);

            // ایجاد مدل صفحه‌بندی
            var model = new MessageUserAdminPaging
            {
                Status = request.status,
                Filter = request.filter,
                Messages = new List<MessageUserAdminQueryModel>()
            };

            // محاسبه پارامترهای صفحه‌بندی
            model.GetData(orderedQuery, request.pageId, request.take, 5);

            // دریافت نتایج صفحه‌بندی شده
            if (model.DataCount > 0)
            {
                var messages = orderedQuery
                    .Skip(model.Skip)
                    .Take(model.Take)
                    .ToList();

                // تبدیل به مدل‌های نمایش
                var messageModels = messages
                    .Select(m => new MessageUserAdminQueryModel
                    {
                        CreationDate = m.CreateDate.ToPersianDate(),
                        Email = m.Email,
                        FullName = m.FullName,
                        Id = m.Id,
                        PhoneNumber = m.PhoneNumber,
                        Status = m.Status,
                        Subject = m.Subject,
                        UseName = "",
                        UserId = m.UserId
                    })
                    .OrderByDescending(m => m.Id)
                    .ToList();

                // به‌روزرسانی نام کاربران
                var userIds = messageModels.Where(x => x.UserId > 0).Select(x => x.UserId).Distinct().ToList();
                if (userIds.Any())
                {
                    var users = await _userRepository.GetAllByQueryAsync(u => userIds.Contains(u.Id), cancellationToken);
                    var userDict = users.ToDictionary(u => u.Id);

                    foreach (var message in messageModels.Where(x => x.UserId > 0))
                    {
                        if (userDict.TryGetValue(message.UserId, out var user))
                        {
                            message.FullName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
                        }
                    }
                }

                model.Messages = messageModels;
            }

            return model;
        }
    }
}
