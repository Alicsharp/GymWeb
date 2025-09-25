using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;
using Utility.Domain;

namespace Gtm.Domain.SiteDomain.BannerAgg
{
    public class Baner : BaseEntityCreateActive<int>
    {
        public Baner(string imageName, string imageAlt, string url, BanerState state)
        {
            ImageName = imageName;
            ImageAlt = imageAlt;
            Url = url;
            State = state;
        }
        public void Edit(string imageName, string imageAlt, string url)
        {
            ImageName = imageName;
            ImageAlt = imageAlt;
            Url = url;
        }
        public string ImageName { get; private set; }
        public string ImageAlt { get; private set; }
        public string Url { get; private set; }
        public BanerState State { get; private set; }
    }
}
