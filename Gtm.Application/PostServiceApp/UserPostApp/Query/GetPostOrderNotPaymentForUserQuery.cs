using ErrorOr;
using Gtm.Contract.PostContract.UserPostContract.Query;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.UserPostApp.Query
{
    public record GetPostOrderNotPaymentForUserQuery(int userId) : IRequest<ErrorOr<PostOrderUserPanelModel>>;
    public class GetPostOrderNotPaymentForUserQueryHandler : IRequestHandler<GetPostOrderNotPaymentForUserQuery, ErrorOr<PostOrderUserPanelModel>>
    {
        private readonly IPostOrderRepo _repository;
        private readonly IPostOrderValidation _validator;

        public GetPostOrderNotPaymentForUserQueryHandler(IPostOrderRepo repository,IPostOrderValidation validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<ErrorOr<PostOrderUserPanelModel>> Handle(GetPostOrderNotPaymentForUserQuery request,CancellationToken cancellationToken)
        {
            // اعتبارسنجی
            var validationResult = await _validator.ValidateGetPostOrderNotPaymentAsync(request.userId);
            if (validationResult.IsError)
                return validationResult.Errors;

            // دریافت داده‌ها
            var model = await _repository.GetPostOrderNotPaymentForUser(request.userId);

            if (model == null)
                return Error.NotFound("PostOrder.NotFound", "سفارش پستی یافت نشد");

            return model;
        }
    }
}
