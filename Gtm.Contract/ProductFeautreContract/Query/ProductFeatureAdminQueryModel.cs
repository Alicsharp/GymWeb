using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gtm.Contract.ProductFeautreContract.Query
{
    public class ProductFeatureAdminQueryModel
    {
        public ProductFeatureAdminQueryModel(int id, string title, string value)
        {
            Id = id;
            Title = title;
            Value = value;
        }

        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Value { get; private set; }
    } 
}
