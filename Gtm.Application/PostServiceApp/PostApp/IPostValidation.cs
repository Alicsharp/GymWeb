using ErrorOr;
using Gtm.Contract.PostContract.PostCalculateContract.Query;
using Gtm.Contract.PostContract.PostContract.Command;

namespace Gtm.Application.PostServiceApp.PostApp
{
    public interface IPostValidation
    {
        Task<ErrorOr<Success>> ValidateCreatePost(CreatePost model);
        Task<ErrorOr<Success>> ValidateEditPost(EditPost model);
        Task<ErrorOr<Success>> ValidateActivationChange(int postId); 
        Task<ErrorOr<Success>> ValidateInsideCityChange(int postId);
        Task<ErrorOr<Success>> ValidateOutSideCityChange(int postId);
        Task<ErrorOr<Success>> ValidateGetForEdit(int postId); 
        Task<ErrorOr<Success>> ValidatePostDetails(int postId);
        ErrorOr<Success> ValidatePostCalculation(PostPriceRequestModel request);
        Task<ErrorOr<Success>> ValidateGetPostDetails(int postId);

    }
    public class PostValidation : IPostValidation
    {
        private readonly IPostRepo _postRepository;

        public PostValidation(IPostRepo postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<ErrorOr<Success>> ValidateCreatePost(CreatePost model)
        {
            var errors = new List<Error>();

            // اعتبارسنجی مدل
            if (model == null)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidInput",
                    description: "اطلاعات پست نامعتبر است"));
                return errors;
            }

            // اعتبارسنجی عنوان
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidTitle",
                    description: "عنوان پست نمی‌تواند خالی باشد"));
            }

            // اعتبارسنجی وضعیت
            //if (!Enum.IsDefined(typeof(PostStatus), model.Status))
            //{
            //    errors.Add(Error.Validation(
            //        code: "Post.InvalidStatus",
            //        description: "وضعیت پست نامعتبر است"));
            //}

            // اعتبارسنجی عنوان تکراری
            if (await _postRepository.ExistsAsync(p => p.Title == model.Title))
            {
                errors.Add(Error.Conflict(
                    code: "Post.DuplicateTitle",
                    description: "پستی با این عنوان قبلاً ثبت شده است"));
            }

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ValidateEditPost(EditPost model)
        {
            var errors = new List<Error>();

            // اعتبارسنجی مدل
            if (model == null)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidInput",
                    description: "اطلاعات پست نامعتبر است"));
                return errors;
            }

            // اعتبارسنجی شناسه
            if (model.Id <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidId",
                    description: "شناسه پست نامعتبر است"));
            }

            // اعتبارسنجی عنوان
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidTitle",
                    description: "عنوان پست نمی‌تواند خالی باشد"));
            }

            //// اعتبارسنجی وضعیت
            //if (!Enum.IsDefined(typeof(PostStatus), model.Status))
            //{
            //    errors.Add(Error.Validation(
            //        code: "Post.InvalidStatus",
            //        description: "وضعیت پست نامعتبر است"));
            //}

            // اعتبارسنجی عنوان تکراری
            if (await _postRepository.ExistsAsync(p =>
                p.Title == model.Title && p.Id != model.Id))
            {
                errors.Add(Error.Conflict(
                    code: "Post.DuplicateTitle",
                    description: "پستی با این عنوان قبلاً ثبت شده است"));
            }

            // بررسی وجود پست
            var postExists = await _postRepository.ExistsAsync(p => p.Id == model.Id);
            if (!postExists)
            {
                errors.Add(Error.NotFound(
                    code: "Post.NotFound",
                    description: "پست مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
       
        public async Task<ErrorOr<Success>> ValidateActivationChange(int postId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه پست
            if (postId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidId",
                    description: "شناسه پست نامعتبر است"));
            }

            // بررسی وجود پست
            var postExists = await _postRepository.ExistsAsync(p => p.Id == postId);
            if (!postExists)
            {
                errors.Add(Error.NotFound(
                    code: "Post.NotFound",
                    description: "پست مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateInsideCityChange(int postId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه پست
            if (postId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidId",
                    description: "شناسه پست نامعتبر است"));
            }

            // بررسی وجود پست
            var postExists = await _postRepository.ExistsAsync(p => p.Id == postId);
            if (!postExists)
            {
                errors.Add(Error.NotFound(
                    code: "Post.NotFound",
                    description: "پست مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateOutSideCityChange(int postId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه پست
            if (postId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidId",
                    description: "شناسه پست نامعتبر است"));
            }

            // بررسی وجود پست
            var postExists = await _postRepository.ExistsAsync(p => p.Id == postId);
            if (!postExists)
            {
                errors.Add(Error.NotFound(
                    code: "Post.NotFound",
                    description: "پست مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetForEdit(int postId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه پست
            if (postId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidId",
                    description: "شناسه پست نامعتبر است"));
                return errors;
            }

            // بررسی وجود پست
            var postExists = await _postRepository.ExistsAsync(p => p.Id == postId);
            if (!postExists)
            {
                errors.Add(Error.NotFound(
                    code: "Post.NotFound",
                    description: "پست مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidatePostDetails(int postId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه پست
            if (postId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidId",
                    description: "شناسه پست نامعتبر است"));
                return errors;
            }

            // بررسی وجود پست
            var postExists = await _postRepository.ExistsAsync(p => p.Id == postId);
            if (!postExists)
            {
                errors.Add(Error.NotFound(
                    code: "Post.NotFound",
                    description: "پست مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public ErrorOr<Success> ValidatePostCalculation(PostPriceRequestModel request)
        {
            var errors = new List<Error>();

            if (request == null)
            {
                errors.Add(Error.Validation(
                    code: "PostCalculate.NullRequest",
                    description: "داده‌های درخواست نمی‌تواند خالی باشد"));
                return errors;
            }

            if (request.SourceCityId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "PostCalculate.InvalidSourceCity",
                    description: "شناسه شهر مبدا نامعتبر است"));
            }

            if (request.DestinationCityId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "PostCalculate.InvalidDestinationCity",
                    description: "شناسه شهر مقصد نامعتبر است"));
            }

            if (request.Weight <= 0)
            {
                errors.Add(Error.Validation(
                    code: "PostCalculate.InvalidWeight",
                    description: "وزن بسته باید بیشتر از صفر باشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateGetPostDetails(int postId)
        {
            var errors = new List<Error>();

            // اعتبارسنجی شناسه پست
            if (postId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidId",
                    description: "شناسه پست نامعتبر است"));
            }

            // بررسی وجود پست
            if (!await _postRepository.ExistsAsync(p => p.Id == postId))
            {
                errors.Add(Error.NotFound(
                    code: "Post.NotFound",
                    description: "پست مورد نظر یافت نشد"));
            }

            return errors.Count > 0 ? errors : Result.Success;
        }
    }
}
