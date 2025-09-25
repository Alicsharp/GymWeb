using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain.Enums;

namespace Gtm.Contract.TransactionContract.Command
{
    public class CreateTransaction
    {
        public int UserId { get; set; }
        [Display(Name = "مبلغ (تومان)")]
        public int Price { get; set; }
        [Display(Name = "انتخاب درگاه")]
        public TransactionPortal Portal { get; set; }
        public TransactionFor TransactionFor { get; set; }
        public int OwnerId { get; set; }
        public string Authority { get; set; }
    }
}
