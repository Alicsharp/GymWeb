using Gtm.Application.ArticleCategoryApp;
using Gtm.Domain.BlogDomain.BlogCategoryDm;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Infrastuctuer.Repo;

namespace Gtm.InfraStructure.RepoImple.ArticleCategoryRepo
{
    public class ArticleCategoryRepo : Repository<ArticleCategory, int>, IArticleCategoryRepo
    {
        private readonly GtmDbContext _dbContext;
        public ArticleCategoryRepo(GtmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ArticleCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ArticleCategories.FirstOrDefaultAsync(category => category.Slug == slug, cancellationToken);
        }
    }
}
