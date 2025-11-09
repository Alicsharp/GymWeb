using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.WishListContrct
{
    public class WishListForUserPanelQueryModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Amount { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ImageAddress { get; set; }
        public string ImageAlt { get; set; }
    }
}
