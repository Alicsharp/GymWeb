using ErrorOr;
using Gtm.Application.PostServiceApp.CityApp.Command;
using Gtm.Contract.PostContract.CityContract.Command;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Application.PostServiceApp.CityApp
{
    public interface ICityValidation
    {
        Task<ErrorOr<Success>> ChangeStatusValidation(int id, CityStatus status);
        Task<ErrorOr<Success>> CreateCityValidation(CreateCityModel command);
        Task<ErrorOr<Success>> EditCityValidation(EditCityModel command);
        Task<ErrorOr<Success>> IsCityCorrectValidation(int stateId, int cityId );
        Task<ErrorOr<Success>> ExistTitleForCreateCityValidation(string Title, int StateId);
        Task<ErrorOr<Success>> ExistTitleForEditValidation(string title, int id, int stateId);
        Task<ErrorOr<Success>> ValidateStateIdForGetAll(int stateId);
        Task<ErrorOr<Success>> ValidateStateForCitySelection(int stateId);
        Task<ErrorOr<Success>> ValidateCityForEdit(int cityId);
    }
    public class CityValidation : ICityValidation
    {
        private readonly ICityRepo _cityRepo;

        public CityValidation(ICityRepo cityRepo)
        {
            _cityRepo = cityRepo;
        }

        public async Task<ErrorOr<Success>> ChangeStatusValidation(int id, CityStatus status)
        {
            var errors = new List<Error>();  
            // 1. بررسی وجود ID
            if (id <= 0)
                errors.Add(Error.Validation(code: "City.Id", description: "شناسه شهر معتبر نیست"));

            // 2. بررسی وجود شهر در دیتابیس
            var cityExists = await _cityRepo.ExistsAsync(C=>C.Id==id);
            if (!cityExists)
                errors.Add(Error.NotFound(code: "City.NotFound", description: "شهر مورد نظر یافت نشد"));

            // 3. بررسی منطق کسب و کار (مثلاً تغییر وضعیت مجاز باشد)
            // ... اعتبارسنجی‌های خاص کسب و کار

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> CreateCityValidation(CreateCityModel command)
        {
            var errors= new List<Error>();     
            if(command==null )
              errors.Add(Error.Validation(code: "City.InvalidInput", description: "   اطلاعات ارسال نشد یا اطلاعات شهر نا معبتر است"));
            if (command.StateId <= 0)
                errors.Add(Error.Validation(code: "InvalidStateId", description: "شناسه استان نا معبتر است"));

            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> EditCityValidation(EditCityModel command)
        {
            var errors= new List<Error>();

            if ( command == null)
            {
                errors.Add(Error.Validation(
                    code: "City.InvalidInput",
                    description: "اطلاعات شهر نامعتبر است"));
            }
            if ( command.Id <= 0)
            {
                errors.Add(Error.Validation(
                    code: "City.InvalidId",
                    description: "شناسه شهر نامعتبر است"));
            }
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                errors.Add(Error.Validation(
                    code: "City.InvalidTitle",
                    description: "عنوان شهر نمی‌تواند خالی باشد"));
            }
            var city = await _cityRepo.GetByIdAsync( command.Id);

            if (city == null)
            {
                errors.Add(Error.NotFound(
                    code: "City.NotFound",
                    description: "شهر مورد نظر یافت نشد"));
            }
            return errors.Any() ? errors : Result.Success;
        }

        public async Task<ErrorOr<Success>> ExistTitleForCreateCityValidation(string Title, int StateId)
        {
            if (Title == null || StateId <= 0)
                return Error.Failure("InformationNotTrue", "اطلاعات وارد شده نادرست است");
            return Result.Success;
        }

        public async Task<ErrorOr<Success>> IsCityCorrectValidation(int stateId, int cityId)
        {
            var errors = new List<Error>();

            if (stateId == null || cityId==null || stateId<=0 || cityId <=0)
            {
                errors.Add(Error.Validation(
                    code: "City.InvalidInput",
                    description: "اطلاعات شهر نامعتبر است"));
            }
        
           
         
            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ExistTitleForEditValidation(string title, int id, int stateId)
        {
            var errors = new List<Error>();

            // 1. Validate title
            if (string.IsNullOrWhiteSpace(title))
                errors.Add(Error.Validation(code: "City.Title", description: "عنوان شهر نمی‌تواند خالی باشد"));

            // 2. Validate ID
            if (id <= 0)
                errors.Add(Error.Validation(code: "City.Id", description: "شناسه شهر معتبر نیست"));

            // 3. Validate state ID
            if (stateId <= 0)
                errors.Add(Error.Validation(code: "City.StateId", description: "شناسه استان معتبر نیست"));

            // 4. Check if city exists
            var cityExists = await _cityRepo.ExistsAsync(c => c.Id == id);
            if (!cityExists)
                errors.Add(Error.NotFound(code: "City.NotFound", description: "شهر مورد نظر یافت نشد"));

            // 5. Check for duplicate title in the same state (excluding current city)
            var duplicateTitleExists = await _cityRepo.ExistsAsync(c =>
                c.Title == title &&
                c.StateId == stateId &&
                c.Id != id);

            if (duplicateTitleExists)
                errors.Add(Error.Conflict(code: "City.DuplicateTitle", description: "شهری با این عنوان در این استان قبلا ثبت شده است"));

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateStateIdForGetAll(int stateId)
        {
            var errors = new List<Error>();

            // 1. Validate StateId
            if (stateId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "City.InvalidStateId",
                    description: "شناسه استان نامعتبر است"));
            }
 
            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateStateForCitySelection(int stateId)
        {
            var errors = new List<Error>();

            // 1. اعتبارسنجی StateId
            if (stateId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "State.InvalidId",
                    description: "شناسه استان معتبر نیست"));
            }

 
       

            return errors.Any() ? errors : Result.Success;
        }
        public async Task<ErrorOr<Success>> ValidateCityForEdit(int cityId)
        {
            var errors = new List<Error>();

            // 1. اعتبارسنجی شناسه شهر
            if (cityId <= 0)
            {
                errors.Add(Error.Validation(
                    code: "City.InvalidId",
                    description: "شناسه شهر نامعتبر است"));
            }

            // 2. بررسی وجود شهر در دیتابیس
            var cityExists = await _cityRepo.ExistsAsync(c => c.Id == cityId);
            if (!cityExists)
            {
                errors.Add(Error.NotFound(
                    code: "City.NotFound",
                    description: "شهر مورد نظر یافت نشد"));
            }

            return errors.Any() ? errors : Result.Success;
        }
    }
 }

 