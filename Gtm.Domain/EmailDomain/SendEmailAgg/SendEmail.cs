using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.EmailDomain.SendEmailAgg
{
    public class SendEmail : BaseEntityCreate<int>
    {
        public string Title { get; private set; }
        public string Text { get; private set; }
        public SendEmail(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }
}
