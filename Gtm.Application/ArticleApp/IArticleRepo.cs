using Gtm.Contract.ArticleContract.Command;
using Gtm.Domain.BlogDomain.BlogDm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Appliation.RepoInterface;

namespace Gtm.Application.ArticleApp
{
    public interface IArticleRepo:IRepository<Article,int>
    {
     Task<int> DeleteByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);
 
     Task<UpdateArticleDto> GetArticleForEditById(int ArticleId);
     Task<Article> GetBySlugAsync(string slug); 
        
 
    }
}
