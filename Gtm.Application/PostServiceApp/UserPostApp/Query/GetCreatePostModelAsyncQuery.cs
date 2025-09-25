using ErrorOr;
using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Contract.PostContract.UserPostContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.UserPostApp.Query
{

    public record GetCreatePostModelAsyncQuery(int userId, int packageId) : IRequest<ErrorOr<CreatePostOrder>>;
    public class GetCreatePostModelAsyncQueryHandler: IRequestHandler<GetCreatePostModelAsyncQuery, ErrorOr<CreatePostOrder>>
    {
        private readonly IPackageRepo _packageRepository;
        private readonly IPostOrderValidation  _validationService;

        public GetCreatePostModelAsyncQueryHandler(IPackageRepo packageRepository,IPostOrderValidation validationService)
        {
            _packageRepository = packageRepository;
            _validationService = validationService;
        }

        public async Task<ErrorOr<CreatePostOrder>> Handle(GetCreatePostModelAsyncQuery request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی اولیه (غیرهمزمان)
                var validationResult = await _validationService.ValidateUserAndPackageAsync(
                    request.userId,
                    request.packageId);

                if (validationResult.IsError)
                    return validationResult.Errors;

                // دریافت مدل (غیرهمزمان)
                var model = await _packageRepository.GetCreatePostModelAsync(
                    request.userId,
                    request.packageId
                     );

                // اعتبارسنجی مدل (غیرهمزمان)
                var modelValidation = await _validationService.ValidateCreatePostModelAsync(model);
                if (modelValidation.IsError)
                    return modelValidation.Errors;

                return model;
            }
           
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "CreatePostModel.UnexpectedError",
                    description: $"خطای غیرمنتظره در دریافت مدل ایجاد پست: {ex.Message}");
            }
        }
    }
}
