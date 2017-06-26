using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ArkDelegateToolLib;

namespace ArkDelegateTool
{
    /// <summary>
    /// Interaction logic for VoterInfoTable.xaml
    /// </summary>
    public partial class VoterInfoTable : UserControl
    {

        private MainWindow main;

        List<votersListEntry> votersListInfo;
        private string previousSort;

        public VoterInfoTable()
        {
            InitializeComponent();
            votersListInfo = new List<votersListEntry>();
        }

        public void setMain(MainWindow main) { this.main = main;  }

        public void loadVoterInfo(BalanceTracker balTracker)
        {

            balTracker.accountBalanceTrackers.ForEach(abt => votersListInfo.Add(new votersListEntry()
            {
                Address = abt.Address,
                Balance = abt.getCurrentBalance()
            }));

            votersList.ItemsSource = votersListInfo;
        }

        public void calculateShares(BalanceTracker balTracker, DateTime start, DateTime end)
        {
            var shares = balTracker.getPayoutPercentages(start, end);
            votersListInfo.ForEach(v => v.Share = shares[v.Address]);
            votersList.ItemsSource = votersListInfo;
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = (sender as GridViewColumnHeader);
            var tag = columnHeader.Tag.ToString();
            switch (tag)
            {
                case "Address":
                    votersListInfo = votersListInfo.OrderBy(t => t.Address).ToList();
                    break;
                case "Balance":
                    votersListInfo = votersListInfo.OrderBy(t => t.Balance).ToList();
                    break;
                case "Share":
                    votersListInfo = votersListInfo.OrderBy(t => t.Share).ToList();
                    break;
            }
            if (previousSort != null && previousSort.Equals(tag))
            {
                votersListInfo.Reverse();
                previousSort = null;
            }
            else
                previousSort = tag;
            votersList.ItemsSource = votersListInfo;
        }

        private void votersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = votersList.SelectedItem;
            if (selected != null)
            {
                var entry = selected as votersListEntry;
                main.setVoterInfoAddress(entry.Address);
            }
        }

    }


    public class votersListEntry
    {
        public string Address { get; set; }
        public double Balance { get; set; }
        public double Share { get; set; }
    }
}
