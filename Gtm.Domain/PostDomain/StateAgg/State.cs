using Gtm.Domain.PostDomain.CityAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;

namespace Gtm.Domain.PostDomain.StateAgg
{
    public class State : BaseEntityCreate<int>
    {
        public string Title { get; private set; } // نام استان  
        public string CloseStates { get; private set; } // استان های هم جوار
        public List<City> Cities { get; private set; } // رابطه چند به یک
        public State(string title)
        {
            Title = title;
            CloseStates = "";
            Cities = new();
        }
        public void Edit(string title)
        {
            Title = title;
        }
        public void ChangeCloseStates(List<int> States)
        {
            CloseStates = string.Join('-', States);
        }
    }

}

