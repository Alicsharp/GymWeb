using Gtm.Application.ArticleApp;
using Gtm.Application.ArticleCategoryApp;
using Gtm.Contract.ArticleContract.Command;
using Gtm.Domain.BlogDomain.BlogDm;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.ArticleRepo
{
 
    public class ArticleRepository : Repository<Article, int>, IArticleRepo
    {
        private readonly GtmDbContext _dbContext;

        public ArticleRepository(GtmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

       

        public async Task<int> DeleteByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            var articlesToDelete = await _dbContext.Articles
                .Where(a => a.CategoryId == categoryId || a.SubCategoryId == categoryId)
                .ToListAsync(cancellationToken);

            _dbContext.Articles.RemoveRange(articlesToDelete);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<UpdateArticleDto> GetArticleForEditById(int articleId)
        {
            return await _dbContext.Articles
                .Select(c => new UpdateArticleDto
                {
                    ImageAlt = c.ImageAlt,
                    Id = c.Id,
                    ImageFile = null,
                    ImageName = c.ImageName,
                    Slug = c.Slug,
                    Title = c.Title,
                    CategoryId = c.CategoryId,
                    ShortDescription = c.ShortDescription,
                    Text = c.Text,
                    SubCategoryId = c.SubCategoryId,
                    UserId = c.UserId,
                    Writer = c.Writer
                })
                .FirstAsync(c => c.Id == articleId);
        }


        // Optional: Additional methods can be added here


        public async Task<Article> GetBySlugAsync(string slug)
        {
            return await _dbContext.Articles.FirstOrDefaultAsync(c => c.Slug == slug);
        }
    }
}
