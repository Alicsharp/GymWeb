namespace Gtm.Contract.DiscountsContract.OrderDiscountContract.Command
{
    /// <summary>
    /// رکوردی که در صورت موفقیت‌آمیز بودن عملیات بازگردانده می‌شود
    /// </summary>


    public class OperationResultOrderDiscount
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
        public int Id { get; set; } // بر اساس تعریف شما، این فیلد int است
        public int Percent { get; set; }

        public OperationResultOrderDiscount(bool success, string? message = "", string? title = "", int id = 0, int percent = 0)
        {
            Success = success;
            Message = message;
            Id = id;
            Percent = percent;
            Title = title;
        }
    }
}
