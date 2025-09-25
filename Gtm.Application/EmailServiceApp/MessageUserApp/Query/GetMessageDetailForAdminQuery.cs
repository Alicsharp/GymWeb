using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.EmailContract.MessageUserContract.Query;
using MediatR;
using Utility.Appliation;

namespace Gtm.Application.EmailServiceApp.MessageUserApp.Query
{
    public record GetMessageDetailForAdminQuery(int id) : IRequest<ErrorOr<MessageUserDetailAdminQueryModel>>;
    public class GetMessageDetailForAdminQueryHandler : IRequestHandler<GetMessageDetailForAdminQuery, ErrorOr<MessageUserDetailAdminQueryModel>>
    {
        private readonly IMessageUserRepository _messageUserRepository;
        private readonly IUserRepo _userRepository;

        public GetMessageDetailForAdminQueryHandler(IMessageUserRepository messageUserRepository, IUserRepo userRepository)
        {
            _messageUserRepository = messageUserRepository;
            _userRepository = userRepository;
        }
        public async Task<ErrorOr<MessageUserDetailAdminQueryModel>> Handle(GetMessageDetailForAdminQuery request, CancellationToken cancellationToken)
        {
            var m = await _messageUserRepository.GetByIdAsync(request.id);
            MessageUserDetailAdminQueryModel model = new()
            {
                AnswerEmail = m.AnswerEmail,
                AnswerSms = m.AnswerSms,
                CreationDate = m.CreateDate.ToPersainDate(),
                Email = m.Email,
                FullName = m.FullName,
                Id = request.id,
                Message = m.Message,
                PhoneNumber = m.PhoneNumber,
                Status = m.Status,
                Subject = m.Subject,
                UseName = "",
                UserId = m.UserId
            };
            if (model.UserId > 0)
            {
                var user = await _userRepository.GetByIdAsync(request.id);
                model.UseName = string.IsNullOrEmpty(user.FullName) ? user.Mobile : user.FullName;
            }
            return model;
        }
    }
}
