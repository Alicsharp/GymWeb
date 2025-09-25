namespace Utility.Appliation
{
    public class BasePaging
    {
        public void GetData(IQueryable<object> data, int pageId, int take, int showPageCount)
        {
            int dataCount = data.Count();

            PageCount = dataCount / take;
            if (dataCount % take > 0)
                PageCount++;

            // اگر داده‌ای نیست حداقل یک صفحه داشته باشیم
            if (PageCount == 0)
                PageCount = 1;

            // اصلاح مقدار pageId در بازه درست
            if (pageId < 1)
                pageId = 1;

            if (pageId > PageCount)
                pageId = PageCount;

            PageId = pageId;

            // اگر take مقدار نامعتبری داشت، مقدار پیش‌فرض بگذار
            if (take < 1)
                take = 10;

            Take = take;
            DataCount = dataCount;

            Skip = (pageId - 1) * take;

            if (showPageCount < 1)
                showPageCount = 2;

            StartPage = pageId > showPageCount ? pageId - showPageCount : 1;
            EndPage = PageCount - pageId > showPageCount ? pageId + showPageCount : PageCount;
        }

        public int PageId { get; private set; }
        public int PageCount { get; private set; }
        public int Take { get; private set; }
        public int DataCount { get; private set; }
        public int Skip { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }
    }

}
