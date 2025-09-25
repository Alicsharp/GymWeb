using ErrorOr;

using Gtm.Contract.EmailContract.SensEmailContract.Query;
using MediatR;
using Utility.Appliation;


namespace Gtm.Application.EmailServiceApp.SensEmailApp.Query
{
    public record GetSendEmailDetailForAdminQuery(int Id) : IRequest<ErrorOr<SendEmailDetailQueryModel>>;
    public class GetSendEmailDetailForAdminQueryHandler : IRequestHandler<GetSendEmailDetailForAdminQuery, ErrorOr<SendEmailDetailQueryModel>>
    {
        private readonly ISendEmailRepository _sendEmailRepository;
        public GetSendEmailDetailForAdminQueryHandler(ISendEmailRepository sendEmailRepository)
        {
            _sendEmailRepository = sendEmailRepository;
        }
        public async Task<ErrorOr<SendEmailDetailQueryModel>> Handle(GetSendEmailDetailForAdminQuery request, CancellationToken cancellationToken)
        {
            var email = await _sendEmailRepository.GetByIdAsync(request.Id);
            return new SendEmailDetailQueryModel()
            {
                CreationDate = email.CreateDate.ToPersainDate(),
                Id = email.Id,
                Text = email.Text,
                Title = email.Title
            };
        }
    }
}
