using ErrorOr;
using Gtm.Application.TransactionServiceApp;
using Gtm.Contract.AdminDashboard;
using MediatR;
using System.Globalization;

namespace Gtm.Application.AdminDashbord.Query
{
    /// <summary>
    /// کوئری برای دریافت داده‌های نمودار تراکنش‌ها
    /// </summary>
    public record GetTransactionChartDataQuery(string Year)
        : IRequest<ErrorOr<TransactionChartQueryModel>>;

    public class GetTransactionChartDataQueryHandler
    : IRequestHandler<GetTransactionChartDataQuery, ErrorOr<TransactionChartQueryModel>>
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly PersianCalendar _persianCalendar; // (تقویم فارسی)

        public GetTransactionChartDataQueryHandler(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
            _persianCalendar = new PersianCalendar();
        }

        public async Task<ErrorOr<TransactionChartQueryModel>> Handle(
            GetTransactionChartDataQuery request, CancellationToken cancellationToken)
        {
            TransactionChartQueryModel model = new()
            {
                Years = new List<string>(),
                Prices = new List<int>()
            };

            // 1. فراخوانی متد اول ریپازیتوری
            var (minDate, maxDate) = await _transactionRepository.GetTransactionDateRangeAsync(cancellationToken);

            DateTime startDate = minDate ?? DateTime.Now;
            DateTime endDate = maxDate ?? DateTime.Now;

            // 2. منطق بیزینس: ساخت لیست سال‌ها (از کد اصلی شما)
            // (استفاده از GetYear به جای ToPersianDate().Split() برای ایمنی)
            var startYear = _persianCalendar.GetYear(startDate);
            var endYear = _persianCalendar.GetYear(endDate);

            model.Years.Add(startYear.ToString());
            if (endYear > startYear)
            {
                int year = startYear + 1;
                while (year <= endYear)
                {
                    model.Years.Add(year.ToString());
                    year++;
                }
            }

            // 3. منطق بیزینس: تعیین سال انتخاب شده (از کد اصلی شما)
            string selectedPersianYear = request.Year == "0" ? endYear.ToString() : request.Year;

            // 4. فراخوانی متد دوم ریپازیتوری (متد بهینه شده)
            // (دیگر نیازی به GetMonthTransactionForYear نیست)
            var prices = await _transactionRepository.GetPersianMonthlyTransactionSumsAsync(
                selectedPersianYear,
                cancellationToken
            );

            model.Prices = prices;
            model.Year = selectedPersianYear;
            return model;
        }
    }
}