using ErrorOr;
using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Contract.PostContract.UserPostContract.Command;
using Gtm.Domain.PostDomain.UserPostAgg;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Application.PostServiceApp.UserPostApp.Command
{
    public record CreatePostOrderCommand(CreatePostOrder Command) : IRequest<ErrorOr<Success>>;
    public class CreatePostOrderCommandHandler : IRequestHandler<CreatePostOrderCommand, ErrorOr<Success>>
    {
        private readonly IPostOrderRepo _postOrderRepository;
        private readonly IPackageRepo _packageRepository;
        private readonly IUserPostRepo _userPostRepository;
        private readonly IPostOrderValidation _validationService;

        public CreatePostOrderCommandHandler(IPostOrderRepo postOrderRepository,IPackageRepo packageRepository,IUserPostRepo userPostRepository,IPostOrderValidation validationService)
        {
            _postOrderRepository = postOrderRepository;
            _packageRepository = packageRepository;
            _userPostRepository = userPostRepository;
            _validationService = validationService;
        }

        public async Task<ErrorOr<Success>> Handle(CreatePostOrderCommand request,CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی اولیه
                var validationResult = await _validationService.ValidateCreatePostModelAsync(request.Command);
                if (validationResult.IsError)
                    return validationResult.Errors;

                // بررسی سفارش پرداخت نشده موجود
                var existingOrder = await _postOrderRepository.GetPostOrderNotPaymentForUserAsync(request.Command.UserId);

                if (existingOrder == null)
                {
                    // ایجاد سفارش جدید
                    var newOrder = new PostOrder(
                        request.Command.PackageId,
                        request.Command.UserId,
                        request.Command.Price);

                    await _postOrderRepository.AddAsync(newOrder);
                    var createResult=await _postOrderRepository.SaveChangesAsync(cancellationToken);
                    if (!createResult)
                        return Error.Failure("Order.CreateFailed", "عملیات ایجاد سفارش با شکست مواجه شد");

                    return Result.Success;
                }
                else
                {
                    // به‌روزرسانی سفارش موجود در صورت نیاز
                    if (request.Command.PackageId != existingOrder.PackageId ||
                        request.Command.Price != existingOrder.Price)
                    {
                        existingOrder.Edit(request.Command.PackageId, request.Command.Price);
                        var updateResult = await _postOrderRepository.SaveChangesAsync();
                        if (!updateResult)
                            return Error.Failure("Order.UpdateFailed", "عملیات به‌روزرسانی سفارش با شکست مواجه شد");
                    }

                    return Result.Success;
                }
            }
            catch (Exception ex)
            {
                return Error.Unexpected(
                    code: "Order.ProcessingError",
                    description: $"خطای غیرمنتظره در پردازش سفارش: {ex.Message}");
            }
        }
    }
}
