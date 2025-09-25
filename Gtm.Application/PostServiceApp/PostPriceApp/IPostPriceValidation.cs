using ErrorOr;
using Gtm.Application.PostServiceApp.PostApp;
using Gtm.Contract.PostContract.PostPriceContract.Command;
using Gtm.Domain.PostDomain.PostPriceAgg;

namespace Gtm.Application.PostServiceApp.PostPriceApp
{
    public interface IPostPriceValidation
    {
        ErrorOr<Success> ValidateCreatePostPrice(CreatePostPrice command);
        Task<ErrorOr<Success>> ValidateEditPostPrice(EditPostPrice command, PostPrice existingPostPrice);
        ErrorOr<Success> ValidateGetAllForPost(int postId);

        Task<ErrorOr<Success>> ValidateGetForEdit(int priceId);
    }
    public class PostPriceValidation : IPostPriceValidation
    {
        private readonly IPostRepo _postRepository;
        private readonly IPostPriceRepo _postPriceRepository;

        public PostPriceValidation(IPostRepo postRepository,IPostPriceRepo postPriceRepository)
        {
            _postRepository = postRepository;
            _postPriceRepository = postPriceRepository;
        }

        public ErrorOr<Success> ValidateCreatePostPrice(CreatePostPrice command)
        {
            var errors = new List<Error>();

            if (command == null)
            {
                errors.Add(Error.Validation(
                    code: "PostPrice.InvalidInput",
                    description: "اطلاعات قیمت پست نامعتبر است"));
                return errors;
            }

            // اعتبارسنجی شناسه پست
            if (command.PostId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "PostPrice.InvalidPostId",
                    description: "شناسه پست نامعتبر است"));
            }

            // اعتبارسنجی محدوده تاریخ
            if (command.Start >= command.End)
            {
                errors.Add(Error.Validation(
                    code: "PostPrice.InvalidDateRange",
                    description: "تاریخ شروع باید قبل از تاریخ پایان باشد"));
            }

            // اعتبارسنجی قیمت‌ها
            ValidatePriceValues(command, errors);

            // اعتبارسنجی تداخل تاریخ
            if (errors.Count == 0)
            {
                ValidateDateOverlap(command, errors);
            }

            return errors.Any() ? errors : Result.Success;
        }

        private void ValidatePriceValues(CreatePostPrice command, List<Error> errors)
        {
            var priceProperties = new[]
            {
            (command.TehranPrice, "TehranPrice"),
            (command.StateCenterPrice, "StateCenterPrice"),
            (command.CityPrice, "CityPrice"),
            (command.InsideStatePrice, "InsideStatePrice"),
            (command.StateClosePrice, "StateClosePrice"),
            (command.StateNonClosePrice, "StateNonClosePrice")
        };

            foreach (var (price, name) in priceProperties)
            {
                if (price < 0)
                {
                    errors.Add(Error.Validation(
                        code: $"PostPrice.Invalid{name}",
                        description: $"مقدار {name} نمی‌تواند منفی باشد"));
                }
            }
        }
        public async Task<ErrorOr<Success>> ValidateEditPostPrice(EditPostPrice command, PostPrice existingPostPrice)
        {
            var errors = new List<Error>();

            // اعتبارسنجی مدل پایه
            if (command == null)
            {
                errors.Add(Error.Validation(
                    code: "PostPrice.InvalidInput",
                    description: "اطلاعات قیمت پست نامعتبر است"));
                return errors;
            }

            if (command.Id <= 0)
            {
                errors.Add(Error.Validation(
                    code: "PostPrice.InvalidId",
                    description: "شناسه قیمت پست نامعتبر است"));
            }

            // اعتبارسنجی محدوده تاریخ
            if (command.Start >= command.End)
            {
                errors.Add(Error.Validation(
                    code: "PostPrice.InvalidDateRange",
                    description: "تاریخ شروع باید قبل از تاریخ پایان باشد"));
            }

            // اعتبارسنجی مقادیر قیمت
            ValidatePriceValues(command, errors);

            // اعتبارسنجی وجود رکورد
            if (existingPostPrice == null)
            {
                errors.Add(Error.NotFound(
                    code: "PostPrice.NotFound",
                    description: "قیمت پست مورد نظر یافت نشد"));
            }

            // اعتبارسنجی تداخل تاریخ (به جز رکورد فعلی)
            if (existingPostPrice != null && errors.Count == 0)
            {
                await ValidateDateOverlap(command, existingPostPrice, errors);
            }

            return errors.Any() ? errors : Result.Success;
        }

        private void ValidatePriceValues(EditPostPrice command, List<Error> errors)
        {
            var priceProperties = new[]
            {
            (command.TehranPrice, "TehranPrice"),
            (command.StateCenterPrice, "StateCenterPrice"),
            (command.CityPrice, "CityPrice"),
            (command.InsideStatePrice, "InsideStatePrice"),
            (command.StateClosePrice, "StateClosePrice"),
            (command.StateNonClosePrice, "StateNonClosePrice")
        };

            foreach (var (price, name) in priceProperties)
            {
                if (price < 0)
                {
                    errors.Add(Error.Validation(
                        code: $"PostPrice.Invalid{name}",
                        description: $"مقدار {name} نمی‌تواند منفی باشد"));
                }
            }
        }

        private async Task ValidateDateOverlap(EditPostPrice command, PostPrice existingPostPrice, List<Error> errors)
        {
            var overlapExists = await _postPriceRepository.ExistsAsync(p =>
                p.Id != existingPostPrice.Id && // به جز رکورد فعلی
                p.PostId == existingPostPrice.PostId &&
                p.Start <= command.End &&
                p.End >= command.Start);

            if (overlapExists)
            {
                errors.Add(Error.Conflict(
                    code: "PostPrice.DateOverlap",
                    description: "برای این بازه زمانی قبلا قیمت ثبت شده است"));
            }
        }

        private async Task ValidateDateOverlap(CreatePostPrice command, List<Error> errors)
        {
            // استفاده از QueryBy برای ایجاد کوئری پایه
            var baseQuery = _postPriceRepository.QueryBy(
                p => p.PostId == command.PostId &&
                     p.Start <= command.End &&
                     p.End >= command.Start);

            // روش ۱: استفاده از ExistsAsync برای بررسی وجود رکوردهای تداخلی
            var exists = await _postPriceRepository.ExistsAsync(
                p => p.PostId == command.PostId &&
                     p.Start <= command.End &&
                     p.End >= command.Start);

            // یا روش ۲: استفاده از CountAsync برای دریافت تعداد تداخل‌ها
            // var count = await _postPriceRepository.CountAsync(
            //     p => p.PostId == command.PostId &&
            //          p.Start <= command.End && 
            //          p.End >= command.Start);
            // var exists = count > 0;

            if (exists)
            {
                errors.Add(Error.Conflict(
                    code: "PostPrice.DateOverlap",
                    description: "برای این بازه زمانی قبلا قیمت ثبت شده است"));
            }
        }
        public ErrorOr<Success> ValidateGetAllForPost(int postId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه پست
            if (postId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidId",
                    description: "شناسه پست نامعتبر است"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEdit(int priceId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه قیمت
            if (priceId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "PostPrice.InvalidId",
                    description: "شناسه قیمت پست نامعتبر است"));
                return errors;
            }

            // بررسی وجود قیمت پست
            var priceExists = await _postPriceRepository.ExistsAsync(p => p.Id == priceId);
            if (!priceExists)
            {
                errors.Add(Error.NotFound(
                    code: "PostPrice.NotFound",
                    description: "قیمت پست مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}
