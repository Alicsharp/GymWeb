using Microsoft.AspNetCore.Http;

namespace Gtm.Contract.ArticleContract.Command
{
    public class UpdateArticleDto: CreateArticleDto
    {
        public int Id { get; set; }
        public string ImageName { get; set; }
    }
}
