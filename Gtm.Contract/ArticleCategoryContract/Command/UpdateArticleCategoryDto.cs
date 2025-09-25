using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.ArticleCategoryContract.Command
{
    public class UpdateArticleCategoryDto:CreateArticleCategory
    {
        public int Id { get; set; }
        public string ImageName { get; set; }
    }
}
