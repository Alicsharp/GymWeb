using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.PostDomain.UserPostAgg
{
    public class Package : BaseEntityCreateUpdateActive<int>
    {
        public Package()
        {
            PostOrders = new();
        }
        public Package(string title, string description, int count, int price, string imageName, string imageAlt)
        {
            Title = title;
            Description = description;
            Count = count;
            Price = price;
            ImageName = imageName;
            ImageAlt = imageAlt;
        }
        public void Edit(string title, string description, int count, int price, string imageName, string imageAlt)
        {
            Title = title;
            Description = description;
            Count = count;
            Price = price;
            ImageName = imageName;
            ImageAlt = imageAlt;
            UpdateEntity();
        }

        public string Title { get; private set; } //عنوان بسته
        public string Description { get; private set; }//توضیحات بسته
        public string ImageName { get; private set; } // نام عکس
        public string ImageAlt { get; private set; } // اطلاعات تصویر
        public int Count { get; private set; } // تعداد موجود
        public int Price { get; private set; } // قیمت بسته
        public List<PostOrder> PostOrders { get; private set; } // لیست سفاررشات مربوط به این بسته
    }
}
