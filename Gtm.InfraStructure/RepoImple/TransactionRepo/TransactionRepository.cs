using Gtm.Application.TransactionServiceApp;
using Gtm.Domain.TransactionDomian;
using Gtm.InfraStructure.RepoImple.CommentRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.TransactionRepo
{
    internal class TransactionRepository : Repository<Transaction, long>, ITransactionRepository
    {
        private readonly PersianCalendar _persianCalendar;
        private readonly GtmDbContext _context;
        public TransactionRepository(GtmDbContext context) : base(context)
        {
            _context = context;
          _persianCalendar = new PersianCalendar(); ;
        }

        public async Task<long> CreateAsyncReturnKey(Transaction transaction)
        {
            try
            {
                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync(); 
                return transaction.Id;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }
            catch
            {
                return 0; // یا مقدار پیش‌فرض دیگری در صورت خطا
            }
            //if (await AddAsync(transaction))
            //    return transaction.Id;
            //return 0;
        }

        public Task<Transaction> GetByAuthorityAsync(string authority) =>
            _context.Transactions.SingleOrDefaultAsync(s => s.Authority.ToLower().Trim() == authority.ToLower().Trim());
 
        public async Task<int> GetSuccessfulTransactionSumAsync(CancellationToken cancellationToken = default)
        {
            // 3. از متد Query() (که در IRepository شما بود) استفاده می‌کنیم
            return await Query()
                .Where(t => t.Status == TransactionStatus.موفق) // فیلتر کردن
                .SumAsync(t => t.Price, cancellationToken); // سپس جمع بستن
        }
        /// <summary>
        /// پیاده‌سازی متد واکشی بازه زمانی
        /// </summary>
        public async Task<(DateTime? MinDate, DateTime? MaxDate)> GetTransactionDateRangeAsync(CancellationToken cancellationToken = default)
        {
            var query = Query().Where(t => t.Status ==  TransactionStatus.موفق);

            // اجرای موازی برای بهینه‌سازی
            var minDateTask = query.MinAsync(t => (DateTime?)t.CreateDate, cancellationToken);
            var maxDateTask = query.MaxAsync(t => (DateTime?)t.CreateDate, cancellationToken);

            await Task.WhenAll(minDateTask, maxDateTask);

            return (minDateTask.Result, maxDateTask.Result);
        }
        public async Task<List<int>> GetMonthlyTransactionSumsForYearAsync(int year, CancellationToken cancellationToken = default)
        {
            var transactions = await Query()
                .Where(t => t.Status ==  TransactionStatus.موفق &&
                            t.CreateDate.Year == year)
                .ToListAsync(cancellationToken);

            // گروه‌بندی در حافظه (چون تعداد تراکنش‌های یک سال معمولاً زیاد نیست)
            var pricesByMonth = transactions
                .GroupBy(t => t.CreateDate.Month) // گروه‌بندی بر اساس ماه میلادی (1 تا 12)
                .Select(g => new { Month = g.Key, Sum = g.Sum(t => t.Price) })
                .ToDictionary(g => g.Month, g => g.Sum);

            // ساخت لیست ۱۲ تایی (که ماه‌های خالی، صفر باشند)
            List<int> monthlyPrices = new List<int>();
            for (int i = 1; i <= 12; i++)
            {
                monthlyPrices.Add(pricesByMonth.GetValueOrDefault(i, 0));
            }

            return monthlyPrices;
        }

        // --- پیاده‌سازی متد دوم (مجموع درآمد ماهانه شمسی) ---
        public async Task<List<int>> GetPersianMonthlyTransactionSumsAsync(string persianYear, CancellationToken cancellationToken = default)
        {
            int pYear;
            try { pYear = Convert.ToInt32(persianYear); }
            catch { return new List<int>(new int[12]); } // لیست ۱۲ تایی صفر

            // 1. محاسبه شروع و پایان سال شمسی به میلادی
            // (این منطق جایگزین ToEnglishDateTime().Split() شما می‌شود)
            DateTime startDate = _persianCalendar.ToDateTime(pYear, 1, 1, 0, 0, 0, 0);

            bool isLeap = _persianCalendar.IsLeapYear(pYear);
            int lastDay = isLeap ? 30 : 29;
            DateTime endDate = _persianCalendar.ToDateTime(pYear, 12, lastDay, 23, 59, 59, 999);

            // 2. واکشی *تمام* تراکنش‌های آن سال در *یک* کوئری (حل مشکل 12 کوئری)
            var transactions = await Query()
                .Where(t => t.Status == TransactionStatus.موفق &&
                            t.CreateDate >= startDate &&
                            t.CreateDate <= endDate)
                .ToListAsync(cancellationToken);

            // 3. گروه‌بندی در حافظه بر اساس ماه شمسی (بسیار سریع)
            var pricesByMonth = transactions
                .GroupBy(t => _persianCalendar.GetMonth(t.CreateDate)) // گروه‌بندی بر اساس ماه شمسی (1 تا 12)
                .Select(g => new { Month = g.Key, Sum = g.Sum(t => t.Price) })
                .ToDictionary(g => g.Month, g => g.Sum);

            // 4. ساخت لیست ۱۲ تایی (که ماه‌های خالی، صفر باشند)
            List<int> monthlyPrices = new List<int>();
            for (int i = 1; i <= 12; i++)
            {
                monthlyPrices.Add(pricesByMonth.GetValueOrDefault(i, 0));
            }

            return monthlyPrices;
        }
    }
}
