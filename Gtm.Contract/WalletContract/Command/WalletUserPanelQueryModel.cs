using Utility.Domain.Enums;

namespace Gtm.Contract.WalletContract.Command
{
    public class WalletUserPanelQueryModel
    {
        public int Id { get; set; }
        public int Price { get; set; }
        public WalletType Type { get; set; }
        public string Description { get; set; }
        public string CreationDate { get; set; }
    }
}
