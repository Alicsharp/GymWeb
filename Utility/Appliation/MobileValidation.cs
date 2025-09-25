using System.ComponentModel.DataAnnotations;

namespace Utility.Appliation
{
    public class MobileValidation : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            string val = value as string;
            if (string.IsNullOrEmpty(val)) return false;
            if (val.Length != 11) return false;
            try
            {
                long number = long.Parse(val);
            }
            catch
            {
                return false;
            }
            if (!val.StartsWith("09")) return false;
            return true;
        }
    }
}
