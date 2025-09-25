using ErrorOr;
using Gtm.Application.SeoApp;
using Gtm.Application.UserApp;
using Gtm.Contract.WalletContract.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.WalletServiceApp
{
    public interface IWalletValidator
    {
        Task<ErrorOr<Success>> ValidateCreateAsync(CreateWalletWithWhy command);
        Task<ErrorOr<Success>> ValidateDepositByAdminAsync(CreateWallet command);
        Task<ErrorOr<Success>> ValidatePaymentSuccessAsync(int walletId);
        Task<ErrorOr<Success>> ValidateWithdrawalAsync(CreateWalletWithWhy command);
        Task<ErrorOr<Success>> ValidateGetWalletAmountAsync(int userId);
    }
    public class WalletValidator : IWalletValidator
    {
        private readonly IUserRepo _userRepository;
        private readonly IWalletRepository _walletRepository;

        public WalletValidator(IUserRepo userRepository, IWalletRepository walletRepository)
        {
            _userRepository = userRepository;
            _walletRepository = walletRepository;
        }

        public async Task<ErrorOr<Success>> ValidateCreateAsync(CreateWalletWithWhy command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی UserId
            if (command.UserId <= 0)
            {
                errors.Add(Error.Validation("Wallet.InvalidUserId", "شناسه کاربر نامعتبر است."));
            }
            else if (!await _userRepository.ExistsAsync(u => u.Id == command.UserId))
            {
                errors.Add(Error.Validation("Wallet.UserNotFound", "کاربر یافت نشد."));
            }

            // اعتبارسنجی Price
            if (command.Price <= 0)
            {
                errors.Add(Error.Validation("Wallet.InvalidAmount", "مبلغ واریز باید بزرگتر از صفر باشد."));
            }

            // اعتبارسنجی Description
            if (string.IsNullOrWhiteSpace(command.Description))
            {
                errors.Add(Error.Validation("Wallet.DescriptionRequired", "توضیحات الزامی است."));
            }
            else if (command.Description.Length > 500)
            {
                errors.Add(Error.Validation("Wallet.DescriptionTooLong", "توضیحات نباید بیشتر از 500 کاراکتر باشد."));
            }

            // اعتبارسنجی WalletWhy
            if (!Enum.IsDefined(typeof(WalletWhy), command.WalletWhy))
            {
                errors.Add(Error.Validation("Wallet.InvalidWhy", "علت واریز نامعتبر است."));
            }

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateDepositByAdminAsync(CreateWallet command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی UserId
            if (command.UserId <= 0)
            {
                errors.Add(Error.Validation("Wallet.InvalidUserId", "شناسه کاربر نامعتبر است."));
            }
            else if (!await _userRepository.ExistsAsync(u => u.Id == command.UserId))
            {
                errors.Add(Error.Validation("Wallet.UserNotFound", "کاربر یافت نشد."));
            }

            // اعتبارسنجی Price
            if (command.Price <= 0)
            {
                errors.Add(Error.Validation("Wallet.InvalidAmount", "مبلغ واریز باید بزرگتر از صفر باشد."));
            }
            else if (command.Price > 100000000) // مثال برای حداکثر مقدار
            {
                errors.Add(Error.Validation("Wallet.AmountTooLarge", "مبلغ واریز بیش از حد مجاز است."));
            }

            // اعتبارسنجی Description
            if (string.IsNullOrWhiteSpace(command.Description))
            {
                errors.Add(Error.Validation("Wallet.DescriptionRequired", "توضیحات الزامی است."));
            }
            else if (command.Description.Length > 500)
            {
                errors.Add(Error.Validation("Wallet.DescriptionTooLong", "توضیحات نباید بیشتر از 500 کاراکتر باشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidatePaymentSuccessAsync(int walletId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی ID
            if (walletId <= 0)
            {
                errors.Add(Error.Validation("Wallet.InvalidId", "شناسه کیف پول نامعتبر است."));
            }

            // بررسی وجود کیف پول
            var walletExists = await _walletRepository.ExistsAsync(w => w.Id == walletId);
            if (!walletExists)
            {
                errors.Add(Error.Validation("Wallet.NotFound", "کیف پول یافت نشد."));
            }

            // می‌توانید اعتبارسنجی‌های بیشتری اضافه کنید مثلاً:
            // - آیا کیف پول قبلاً پرداخت شده است؟
            // - آیا وضعیت کیف پول مناسب برای پرداخت است؟

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateWithdrawalAsync(CreateWalletWithWhy command)
        {
            var errors = new List<Error>();

            // اعتبارسنجی UserId
            if (command.UserId <= 0)
            {
                errors.Add(Error.Validation("Wallet.InvalidUserId", "شناسه کاربر نامعتبر است."));
            }
            else if (!await _userRepository.ExistsAsync(u => u.Id == command.UserId))
            {
                errors.Add(Error.Validation("Wallet.UserNotFound", "کاربر یافت نشد."));
            }

            // اعتبارسنجی مبلغ برداشت
            if (command.Price <= 0)
            {
                errors.Add(Error.Validation("Wallet.InvalidAmount", "مبلغ برداشت باید بزرگتر از صفر باشد."));
            }
            else
            {
                //// بررسی موجودی کافی کاربر
                //var balance = await _walletRepository.GetUserBalanceAsync(command.UserId);
                //if (balance < command.Price)
                //{
                //    errors.Add(Error.Validation("Wallet.InsufficientBalance", "موجودی حساب کافی نیست."));
                //}
            }

            // اعتبارسنجی توضیحات
            if (string.IsNullOrWhiteSpace(command.Description))
            {
                errors.Add(Error.Validation("Wallet.DescriptionRequired", "توضیحات الزامی است."));
            }
            else if (command.Description.Length > 500)
            {
                errors.Add(Error.Validation("Wallet.DescriptionTooLong", "توضیحات نباید بیشتر از 500 کاراکتر باشد."));
            }

            // اعتبارسنجی علت برداشت
            if (!Enum.IsDefined(typeof(WalletWhy), command.WalletWhy))
            {
                errors.Add(Error.Validation("Wallet.InvalidWhy", "علت برداشت نامعتبر است."));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetWalletAmountAsync(int userId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی UserId
            if (userId <= 0)
            {
                errors.Add(Error.Validation("Wallet.InvalidUserId", "شناسه کاربر نامعتبر است."));
            }
            else if (!await _userRepository.ExistsAsync(u => u.Id == userId))
            {
                errors.Add(Error.Validation("Wallet.UserNotFound", "کاربر یافت نشد."));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}
