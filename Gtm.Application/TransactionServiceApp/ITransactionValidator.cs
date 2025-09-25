using ErrorOr;
using Gtm.Application.UserApp;
using Gtm.Contract.TransactionContract.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.TransactionServiceApp
{
    public interface ITransactionValidator
    {
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateTransaction dto);
        Task<ErrorOr<Success>> ValidateIdAsync(long id);
        Task<ErrorOr<Success>> ValidatePaymentAsync(  long transactionId, string refId);
        Task<ErrorOr<Success>> ValidateAuthorityAsync(string authority);
        Task<ErrorOr<Success>> ValidateUserAccessAsync(int userId);
    }

    public class TransactionValidator : ITransactionValidator
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepo _userRepo;

        public TransactionValidator(ITransactionRepository transactionRepository, IUserRepo userRepo)
        {
            _transactionRepository = transactionRepository;
            _userRepo = userRepo;
        }

        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateTransaction dto)
        {
            var errors = new List<Error>();

            // اعتبارسنجی مبلغ
            if (dto.Price < 1000)
            {
                errors.Add(Error.Validation(
                    code: "Transaction.InvalidPrice",
                    description: "مبلغ تراکنش باید بیشتر از 1000 باشد"));
            }

            // اعتبارسنجی Authority
            //if (string.IsNullOrEmpty(dto.Authority))
            //{
            //    errors.Add(Error.Validation(
            //        code: "Transaction.MissingAuthority",
            //        description: "Authority نمی‌تواند خالی باشد"));
            //}
            //else if (await _transactionRepository.ExistsAsync(t => t.Authority == dto.Authority))
            //{
            //    errors.Add(Error.Conflict(
            //        code: "Transaction.DuplicateAuthority",
            //        description: "Authority تکراری است"));
            //}

            // اعتبارسنجی UserId
            if (dto.UserId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Transaction.InvalidUserId",
                    description: "شناسه کاربر نامعتبر است"));
            }

            //// اعتبارسنجی OwnerId
            //if (dto.OwnerId <= 0)
            //{
            //    errors.Add(Error.Validation(
            //        code: "Transaction.InvalidOwnerId",
            //        description: "شناسه مالک نامعتبر است"));
            //}

            return errors.Count > 0 ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateIdAsync(long id)
        {
            if (id <= 0)
            {
                return Error.Validation(
                    code: "Transaction.InvalidId",
                    description: "شناسه تراکنش نامعتبر است");
            }

            if (!await _transactionRepository.ExistsAsync(x=>x.Id==id))
            {
                return Error.NotFound(
                    code: "Transaction.NotFound",
                    description: "تراکنش یافت نشد");
            }

            return Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidatePaymentAsync(long transactionId, string refId)
        {
            var errors = new List<Error>();
         
            // اعتبارسنجی RefId
            if (string.IsNullOrWhiteSpace(refId))
            {
                errors.Add(Error.Validation(
                    code: "Payment.MissingRefId",
                    description: "شناسه مرجع پرداخت نمی‌تواند خالی باشد"));
            }

            // اعتبارسنجی وضعیت تراکنش
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction?.Status != TransactionStatus.نا_موفق)
            {
                errors.Add(Error.Conflict(
                    code: "Payment.AlreadyProcessed",
                    description: "تراکنش قبلاً پردازش شده است"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateAuthorityAsync(string authority)
        {
            if (string.IsNullOrWhiteSpace(authority))
            {
                return Error.Validation(
                    code: "Payment.MissingAuthority",
                    description: "Authority نمی‌تواند خالی باشد");
            }

            // می‌توانید اعتبارسنجی‌های اضافی مانند فرمت Authority را اینجا اضافه کنید
            // if (!IsValidAuthorityFormat(authority)) { ... }

            return Result.Success;
        }

     
        

            public async Task<ErrorOr<Success>> ValidateUserAccessAsync(int userId)
            {
                var errors = new List<Error>();

                if (userId <= 0)
                {
                    errors.Add(Error.Validation("User.InvalidId", "شناسه کاربر نامعتبر است"));
                }
                else if (!await _userRepo.ExistsAsync(u => u.Id == userId))
                {
                    errors.Add(Error.Validation("User.NotFound", "کاربر یافت نشد"));
                }

                return errors.Any() ? errors : Result.Success;
            }
        }
    
}
