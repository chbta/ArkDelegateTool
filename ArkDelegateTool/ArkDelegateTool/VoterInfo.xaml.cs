using ArkDelegateToolLib;
using System.Linq;
using System.Windows.Controls;

namespace ArkDelegateTool
{
    /// <summary>
    /// Interaction logic for VoterInfo.xaml
    /// </summary>
    public partial class VoterInfo : UserControl
    {
        public VoterInfo()
        {
            InitializeComponent();
        }

        private void loadVoterInfo(string address)
        {
            var voteDate = Transactions.getTransactionsByParam("senderId", address).Where(t => t.type == 3).Max(t => t.timestamp);
            var votedatedt = Helper.convertTime(voteDate);

            var dInfo = new VoterInfoObject()
            {
                Address = address,
                VoteDate = votedatedt.ToString(),
                Balance = Accounts.getAccount(address).Balance
            };
            this.DataContext = dInfo;
        }

        public void setCurrentVoterAddress(string address)
        {
            loadVoterInfo(address);
        }
    }

    class VoterInfoObject
    {
        public string Address { get; set; }
        public string VoteDate { get; set; }
        public double Balance { get; set; }
    }
}
