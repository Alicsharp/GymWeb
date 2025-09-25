using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.PostContract.PackageContract.Query
{
  
    public class PackageUiQueryModel
    {
        public PackageUiQueryModel(int id, int count, int price, string title, string description, string imageAlt, string imageName)
        {
            Id = id;
            Count = count;
            Price = price;
            Title = title;
            Description = description;
            ImageName = imageName;
            ImageAlt = imageAlt;
        }

        public int Id { get; private set; }
        public int Count { get; private set; }
        public int Price { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string ImageName { get; private set; }
        public string ImageAlt { get; private set; }
    }
}
