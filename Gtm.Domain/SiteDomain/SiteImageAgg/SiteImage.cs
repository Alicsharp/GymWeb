using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.SiteDomain.SiteImageAgg
{
    public class SiteImage : BaseEntityCreate<int>
    {
        private void SetValues(string imageName, string title)
        {
            ImageName = imageName;
            Title = title;
        }
        public SiteImage(string imageName, string title)
        {
            SetValues(imageName, title);
        }
        public string ImageName { get; private set; }
        public string Title { get; private set; }
    }
}
