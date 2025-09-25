using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Contract;

namespace Gtm.Contract.ArticleCategoryContract.Command
{
    public class CreateArticleCategory : Title_Slug_Image_ImageAlt
    {
        public int? Parent { get; set; }
    }
}
