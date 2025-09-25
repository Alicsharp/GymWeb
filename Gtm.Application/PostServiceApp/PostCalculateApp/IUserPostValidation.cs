using ErrorOr;
using Gtm.Contract.PostContract.PostCalculateContract.Query;
using Gtm.Domain.PostDomain.UserPostAgg;

namespace Gtm.Application.PostServiceApp.PostCalculateApp
{
    public interface IUserPostValidation
    {
        ErrorOr<Success> ValidateApiRequest(UserPost userPost, PostPriceRequestApiModel request);
    }
    public class UserPostValidation : IUserPostValidation
    {
        public ErrorOr<Success> ValidateApiRequest(UserPost userPost, PostPriceRequestApiModel request)
        {
            var errors = new List<Error>();

            if (userPost == null)
            {
                errors.Add(Error.NotFound(
                    code: "UserPost.NotFound",
                    description: "کاربری با این کد API یافت نشد"));
            }
            else if (userPost.Count < 1)
            {
                errors.Add(Error.Conflict(
                    code: "UserPost.LimitExceeded",
                    description: "محدودیت درخواست‌های API شما به پایان رسیده است"));
            }

            if (request.DestinationCityId <= 0 || request.SourceCityId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidCityId",
                    description: "شناسه شهر مبدا/مقصد نامعتبر است"));
            }

            if (request.Weight <= 0)
            {
                errors.Add(Error.Validation(
                    code: "Post.InvalidWeight",
                    description: "وزن بسته باید بیشتر از صفر باشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
}
