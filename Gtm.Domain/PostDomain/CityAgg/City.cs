using Gtm.Domain.PostDomain.StateAgg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.Domain;
using Utility.Domain.Enums;

namespace Gtm.Domain.PostDomain.CityAgg
{
    public class City : BaseEntityCreate<int>
    {
        public int StateId { get; private set; }//ایدی استان مربوطه 
        public string Title { get; private set; } // نام شهری
        public CityStatus Status { get; private set; } //نوع شهر
        public State State { get; private set; }//ارتباط یک به چند
        public City(int stateId, string title, CityStatus status)
        {
            StateId = stateId;
            Title = title;
            Status = status;
        }
        public void Edit(string title, CityStatus status)
        {
            Title = title;
            Status = status;
        }
        public void IsTehran()
        {
            Status = CityStatus.تهران;
        }
        public void IsCenter()
        {
            Status = CityStatus.مرکز_استان;
        }
        public void INotCenterOrTehran()
        {
            Status = CityStatus.شهرستان_معمولی;
        }
        public void ChangeStatus(CityStatus status)
        {
            Status = status;
        }
    }
}
