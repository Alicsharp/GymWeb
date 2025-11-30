using ErrorOr;
using Gtm.Contract.SiteContract.SliderContract.Query;
using MediatR;

using Microsoft.EntityFrameworkCore;
using Utility.Appliation;
using Utility.Appliation.FileService;


namespace Gtm.Application.SiteServiceApp.SliderApp.Query
{
    public record GetAllSliderForAdminQuery : IRequest<ErrorOr<List<SliderForAdmin>>>;

    public class GetAllSliderForAdminQueryHandler : IRequestHandler<GetAllSliderForAdminQuery, ErrorOr<List<SliderForAdmin>>>
    {
        private readonly ISliderRepository _sliderRepository;

        public GetAllSliderForAdminQueryHandler(ISliderRepository sliderRepository)
        {
            _sliderRepository = sliderRepository ?? throw new ArgumentNullException(nameof(sliderRepository));
        }

        public async Task<ErrorOr<List<SliderForAdmin>>> Handle(GetAllSliderForAdminQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query =   _sliderRepository.GetAllQueryable();

                var result = await query
                    .Select(s => new SliderForAdmin
                    {
                        Active = s.Active,
                        ImageAlt = s.ImageAlt,
                        CreationDate = s.CreateDate.ToPersianDate(), // Fixed typo in method name
                        Id = s.Id,
                        ImageName = Path.Combine(FileDirectories.SliderImageDirectory100, s.ImageName)
                    })
                    .ToListAsync(cancellationToken);

                return result;
            }
            catch (OperationCanceledException)
            {
                return Error.Conflict("Slider.OperationCancelled", "دریافت لیست اسلایدرها لغو شد");
            }
            catch (Exception ex)
            {
                return Error.Failure("Slider.FetchError", $"خطا در دریافت لیست اسلایدرها: {ex.Message}");
            }
        }
    }
}
