using ArkDelegateToolLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ArkDelegateTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BalanceTracker balanceTracker;

        public MainWindow()
        {
            InitializeComponent();
            voterInfoTable.setMain(this);

            datetimeStart.Value = DateTime.Now.AddDays(-7);
            datetimeEnd.Value = DateTime.Now;
        }

        private BalanceTracker getBalanceTracker()
        {
            if (balanceTracker == null)//Lazy BalanceTracker
                balanceTracker = buildBalanceTracker("biz");
            return balanceTracker;
        }
        
        /// <summary>
        /// Sets up a BalanceTracker for the given delegate.
        /// Gets all voters and their transactions and puts them in the BalanceTracker.
        /// </summary>
        private BalanceTracker buildBalanceTracker(string delegateName)
        {
            ArkDelegateToolLib.Delegate del = Delegates.getDelegate(delegateName);
            var balanceTracker = new BalanceTracker(del.Address);


            var voters = Delegates.getVoters(del.PublicKey);

            Parallel.ForEach(voters, (voter) =>
            {
                if (voter != del.Address)
                {
                    var trans = Transactions.getTransactionsByParams(new Dictionary<string, string>  {
                        { "senderId", voter },
                        { "recipientId", voter }
                    });
                    balanceTracker.addAccount(voter, Accounts.getAccount(voter).Balance, trans);
                }
            });

            return balanceTracker; ; ;
        }


        private void buttonLoadVoterInfoTable_Click(object sender, RoutedEventArgs e)
        {
            voterInfoTable.loadVoterInfo(getBalanceTracker());
            voteChart.loadTracker(getBalanceTracker());
        }

        public void setVoterInfoAddress(string address)
        {
            voterInfo.setCurrentVoterAddress(address);
            voteChart.setCurrentAddress(address);
        }

        private void buttonCalculateShares_Click(object sender, RoutedEventArgs e)
        {
            if (datetimeStart.Value.HasValue && datetimeEnd.Value.HasValue)
                voterInfoTable.calculateShares(getBalanceTracker(), datetimeStart.Value.Value, datetimeEnd.Value.Value);
        }
    }

}
