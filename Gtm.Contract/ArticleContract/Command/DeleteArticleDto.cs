namespace Gtm.Contract.ArticleContract.Command
{
    public class DeleteArticleDto
    {
        public int Id { get; set; }
        public string Reason { get; set; } // دلایل حذف برای گزارشات
        public bool SoftDelete { get; set; } = true; // حذف نرم یا سخت
        
    }
}
