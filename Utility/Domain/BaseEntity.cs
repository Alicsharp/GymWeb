using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Domain
{
    public class BaseEntity<TKey>
    {
        public TKey Id { get;   set; }
    }
    public class BaseEntityCreate<Tkey> : BaseEntity<Tkey>
    {
        public DateTime CreateDate { get; private set; }
        public BaseEntityCreate()
        {
            CreateDate = DateTime.Now;
        }
    }
    public class BaseEntityCreateUpdate<Tkey> : BaseEntityCreate<Tkey>
    {
        public DateTime UpdateDate { get; private set; }
        public BaseEntityCreateUpdate()
        {
            UpdateDate = DateTime.Now;
        }
        public void UpdateEntity()
        {
            UpdateDate = DateTime.Now;
        }
    }
    public class BaseEntityCreateUpdateActive<Tkey> : BaseEntityCreateUpdate<Tkey>
    {
        public bool IsActive { get;   set; }
        public BaseEntityCreateUpdateActive()
        {
            IsActive = true;
        }
        public void ActivationChange()
        {
            if (IsActive) IsActive = false;
            else IsActive = true;
        }
        public void SetActivation(bool active)
        {
            IsActive = active;
        }
    }
    public class BaseEntityCreateActive<Tkey> : BaseEntityCreate<Tkey>
    {
        public bool Active { get; private set; }
        public BaseEntityCreateActive()
        {
            Active = true;
        }
        public void ActivationChange()
        {
            if (Active) Active = false;
            else Active = true;
        }
    }
    public class BaseEntityActive<Tkey> : BaseEntity<Tkey>
    {
        public bool Active { get; private set; }
        public BaseEntityActive()
        {
            Active = true;
        }
        public void ActivationChange()
        {
            if (Active) Active = false;
            else Active = true;
        }
    }
}
