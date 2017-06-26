using ArkDelegateToolLib;
using System.Windows.Controls;

namespace ArkDelegateTool
{
    /// <summary>
    /// Interaction logic for DelegateInfo.xaml
    /// </summary>
    public partial class DelegateInfo : UserControl
    {
        public DelegateInfo()
        {
            InitializeComponent();
            loadDelegateInfo();
        }

        private void loadDelegateInfo()
        {
            var arkDel = Delegates.getDelegate("biz");
            var arkDelAcc = Accounts.getAccount(arkDel.Address);

            var dInfo = new DelegateInfoObject()
            {
                Name = arkDel.Username,
                Address = arkDel.Address,
                Balance = arkDelAcc.Balance
            };
            this.DataContext = dInfo;
        }
    }


    class DelegateInfoObject
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public double Balance { get; set; }
    }
}
