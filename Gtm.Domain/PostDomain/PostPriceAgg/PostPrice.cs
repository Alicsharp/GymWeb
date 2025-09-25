using Gtm.Domain.PostDomain.Postgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.PostDomain.PostPriceAgg
{
    public class PostPrice : BaseEntity<int>
    {
        public int PostId { get; private set; } // کلید خارجی مشخص می کند  متعلق به کدام پست است
        public int Start { get; private set; } //شروع بازه وزنی
        public int End { get; private set; }// پایان بازه وزنی 
        public int TehranPrice { get; private set; } //هزینه حمل به تهران
        public int StateCenterPrice { get; private set; }  // هزینه حمل به مرکز استان
        public int CityPrice { get; private set; }//هزینه حمل درون شهری
        public int InsideStatePrice { get; private set; } // هزینه حمل درون استانی
        public int StateClosePrice { get; private set; } //هزینه حمل به استان همجوار
        public int StateNonClosePrice { get; private set; } //هزینه حمل به استان غیرهمجوار
        public Post Post { get; set; }
        public PostPrice(int postId, int start, int end, int tehranPrice,int stateCenterPrice, int cityPrice, int insideStatePrice,int stateClosePrice, int stateNonClosePrice)
        {
            PostId = postId;
            SetValues(start, end, tehranPrice, stateCenterPrice, cityPrice, insideStatePrice, stateClosePrice, stateNonClosePrice);
        }
        public void Edit(int start, int end, int tehranPrice,int stateCenterPrice, int cityPrice, int insideStatePrice,int stateClosePrice, int stateNonClosePrice)
        {
            SetValues(start, end, tehranPrice, stateCenterPrice, cityPrice, insideStatePrice, stateClosePrice, stateNonClosePrice);
        }
        private void SetValues(int start, int end, int tehranPrice,int stateCenterPrice, int cityPrice, int insideStatePrice,int stateClosePrice, int stateNonClosePrice)
        {
            Start = start;
            End = end;
            TehranPrice = tehranPrice;
            StateCenterPrice = stateCenterPrice;
            CityPrice = cityPrice;
            InsideStatePrice = insideStatePrice;
            StateClosePrice = stateClosePrice;
            StateNonClosePrice = stateNonClosePrice;
        }
    }
}
