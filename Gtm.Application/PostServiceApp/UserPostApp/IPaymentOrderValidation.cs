using ErrorOr;
using Gtm.Application.PostServiceApp.PackageApp;
using Gtm.Contract.PostContract.UserPostContract.Query;
using Gtm.Domain.PostDomain.UserPostAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.PostServiceApp.UserPostApp
{
    public interface IPaymentOrderValidation
    {
        Task<ErrorOr<Success>> ValidatePaymentCommandAsync(PaymentPostModel command);
        Task<ErrorOr<PostOrder>> ValidatePostOrderAsync(PaymentPostModel command, PostOrder postOrder);
        Task<ErrorOr<Package>> ValidatePackageAsync(int packageId);
        Task<ErrorOr<UserPost>> ValidateUserPostAsync(int userId);
    }
    public class PaymentOrderValidationService : IPaymentOrderValidation
    {
        private readonly IPostOrderRepo _postOrderRepository;
        private readonly IPackageRepo _packageRepository;
        private readonly IUserPostRepo _userPostRepository;

        public PaymentOrderValidationService(IPostOrderRepo postOrderRepository,IPackageRepo packageRepository,IUserPostRepo userPostRepository)
        {
            _postOrderRepository = postOrderRepository;
            _packageRepository = packageRepository;
            _userPostRepository = userPostRepository;
        }
 

        public async Task<ErrorOr<Success>> ValidatePaymentCommandAsync(PaymentPostModel command)
        {
            var errors = new List<Error>();

            if (command == null)
            {
                errors.Add(Error.Validation(
                    code: "Payment.CommandNull",
                    description: "دستور پرداخت نمی‌تواند خالی باشد"));
                return errors;
            }

            // اعتبارسنجی UserId (عدد مثبت)
            if (command.UserId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Payment.InvalidUserId",
                    description: "شناسه کاربر باید عددی مثبت باشد"));
            }

            // اعتبارسنجی TransactionId (عدد مثبت)
            if (command.TransactionId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Payment.InvalidTransactionId",
                    description: "شناسه تراکنش باید عددی مثبت باشد"));
            }

            // اعتبارسنجی Price (عدد مثبت)
            if (command.Price <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Payment.InvalidPrice",
                    description: "مبلغ پرداخت باید بزرگتر از صفر باشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Package>> ValidatePackageAsync(int packageId)
        {
            var package = await _packageRepository.GetByIdAsync(packageId);
            return package == null
                ? Error.NotFound("Payment.PackageNotFound", "پکیج مرتبط یافت نشد")
                : package;
        }

        public async Task<ErrorOr<UserPost>> ValidateUserPostAsync(int userId)
        {
            var userPost = await _userPostRepository.GetForUserAsync(userId);
            return userPost == null
                ? Error.NotFound("Payment.UserPostNotFound", "رکورد پست کاربر یافت نشد")
                : userPost;
        }

        public async Task<ErrorOr<PostOrder>> ValidatePostOrderAsync(PaymentPostModel command, PostOrder postOrder)
        {
            // بررسی null بودن شیء سفارش
            if (postOrder == null)
            {
                return Error.NotFound(
                    code: "PostOrder.NotFound",
                    description: "سفارش مورد نظر یافت نشد");
            }

            // تطابق کاربر سفارش با کاربر پرداخت کننده
            if (postOrder.UserId != command.UserId)
            {
                return Error.Conflict(
                    code: "PostOrder.UserMismatch",
                    description: "سفارش متعلق به کاربر جاری نیست");
            }

            // تطابق مبلغ سفارش با مبلغ پرداختی
            if (postOrder.Price != command.Price)
            {
                return Error.Conflict(
                    code: "PostOrder.PriceMismatch",
                    description: $"مبلغ پرداختی ({command.Price}) با مبلغ سفارش ({postOrder.Price}) مطابقت ندارد");
            }

            // بررسی وضعیت پرداخت سفارش
            if (postOrder.Status == PostOrderStatus.پرداخت_شده)
            {
                return Error.Conflict(
                    code: "PostOrder.AlreadyPaid",
                    description: $"سفارش شماره {postOrder.Id} قبلاً پرداخت شده است");
            }

            //// بررسی تاریخ انقضای سفارش (در صورت وجود)
            //if (postOrder.ExpireDate.HasValue && postOrder.ExpireDate < DateTime.Now)
            //{
            //    return Error.Validation(
            //        code: "PostOrder.Expired",
            //        description: $"مهلت پرداخت سفارش در تاریخ {postOrder.ExpireDate.Value.ToPersianDate()} به پایان رسیده است");
            //}

            // در صورت گذر از تمام اعتبارسنجی‌ها
            return postOrder;
        }
    }
}
