using System;
using System.Collections.Generic;
using System.Linq;

namespace ArkDelegateToolLib
{
    /// <summary>
    /// The BalanceTracker is an aggregate of accountBalanceTrackers and can be used to calculate voter's percentages.
    /// </summary>
    public class BalanceTracker
    {
        public string delegateAddress { get; }
        public readonly List<AccountBalanceTracker> accountBalanceTrackers;

        public BalanceTracker(string delegateAddress)
        {
            this.delegateAddress = delegateAddress;
            accountBalanceTrackers = new List<AccountBalanceTracker>();
        }

        /// <summary>
        /// Calculates the percentages each voter should receive of the payout.
        /// Returns a dictonary with addresses and percentages.
        /// 
        /// Percentages are calculated by dividing the time since the given startTime in intervales based on changes in the balances of voters.
        /// For each interval each voter's voting weight percentage is calculated, these percentages are weighted by the interval's length.
        /// Based on these scores a final percentage is calculated per voter.
        /// </summary>
        /// <param name="startTime">timestamp of start of the payout interval</param>
        /// <returns>Dictonary where keys are addresses and doubles are percentages.</returns>
        public Dictionary<string, double> getPayoutPercentages(DateTime startTime, DateTime endTime)
        {
            var transactionPoints = getBalanceChangePoints(startTime, endTime);

            var balanceOverTime = new Dictionary<string, double>();
            accountBalanceTrackers.ForEach(abt => balanceOverTime[abt.Address] = 0);

            for (int i = 0; i < transactionPoints.Count - 1; i++)
            {
                double totalBalanceAtPoint = getTotalBalanceAtTime(transactionPoints[i]);
                int timeDelta = transactionPoints[i] + transactionPoints[i + 1];
                foreach (AccountBalanceTracker abt in accountBalanceTrackers)
                {
                    double balanceOverTimePeriod = (abt.getBalanceAt(transactionPoints[i]) * timeDelta)/totalBalanceAtPoint;
                    balanceOverTime[abt.Address] += balanceOverTimePeriod;
                }
            }

            var percentages = new Dictionary<string, double>();
            double total = balanceOverTime.Values.Sum();
            foreach (var entry in balanceOverTime)
                percentages[entry.Key] = entry.Value / total;
            return percentages;
        }

        /// <summary>
        /// Calculates the total sum of votes by all voters at the specified times.
        /// </summary>
        public double getTotalBalanceAtTime(int time)
        {
            double total = 0;
            accountBalanceTrackers.ForEach(abt => total += abt.getBalanceAt(time));
            return total;
        }

        public List<int> getBalanceChangePoints(DateTime startTime, DateTime endTime)
        {
            int start = Helper.convertTime(startTime);
            int end = Helper.convertTime(endTime);

            var transactionPointsSet = new SortedSet<int>();
            accountBalanceTrackers.ForEach(abt => abt.getBalanceChangePoints().ForEach(bp => transactionPointsSet.Add(bp)));
            var transactionPoints = transactionPointsSet.Where(t => t > start && t <= end).ToList();
            transactionPoints.Insert(0, start);
            return transactionPoints;
        }
        
        public void addAccount(string address, double balance, List<Transaction> transactions)
        {
            var abt = new AccountBalanceTracker(address);
            abt.processTransactions(balance, transactions);
            this.accountBalanceTrackers.Add(abt);
        }
    }

    /// <summary>
    /// A balance tracker for a specified account.
    /// Once properly constructed it can be used to obtain the account's balance at a specific point in time.
    /// </summary>
    public class AccountBalanceTracker
    {
        public string Address { get; }
        private List<BalancePoint> balanceOverTime;


        /// <summary>
        /// Basic initialization an AccountBalanceTracker, balancePoints need to be set separatly.
        /// </summary>
        /// <param name="address">The account's address</param>
        public AccountBalanceTracker(string address)
        {
            this.Address = address;
            balanceOverTime = new List<BalancePoint>();
        }

        /// <summary>
        /// Full initialization of an AccountBalanceTracker.
        /// </summary>
        /// <param name="address">The account's address</param>
        /// <param name="transactions">All transactions involving the account</param>
        /// <param name="balance">Current balance</param>
        public AccountBalanceTracker(string address, double balance, List<Transaction> transactions) : this(address)
        {
            processTransactions(balance, transactions);
        }

        /// <summary>
        /// Given a list of all transactions involving the account, and the account's current balance, the balanceOverTime list is filled with the account's balance history.
        /// </summary>
        /// <param name="transactions">All transactions involving the account</param>
        /// <param name="balance">Current balance</param>
        public void processTransactions(double balance, List<Transaction> transactions)
        {
            var lastVoteTime = transactions.Where(t => t.type == 3).Select(t => t.timestamp).Max();
            var transactionsSinceVote = transactions.Where(t => t.timestamp >= lastVoteTime).OrderBy(t => t.timestamp).ToList();

            if (transactionsSinceVote.Count == 0)
            {
                throw new Exception();//The vote transaction should always be in the list.
            }
            
            double curBal = balance;
            this.addBalancePoint(transactionsSinceVote.Last().timestamp, curBal);
            for (int i = transactionsSinceVote.Count() - 1; i > 0; i--)
            {
                int time = transactionsSinceVote[i-1].timestamp;
                var tr = transactionsSinceVote[i];
                double balChange = tr.amount;
                if (tr.senderId.Equals(Address))
                    balChange = -tr.amount - tr.fee;

                curBal -= balChange;
                this.addBalancePoint(time, curBal);
            }
        }

        /// <summary>
        /// Returns a list of timestamps indicating moments when the account's balance has changed.
        /// </summary>
        public List<int> getBalanceChangePoints()
        {
            return balanceOverTime.Select(bp => bp.time).ToList();
        }

        /// <summary>
        /// Gets the account's balance at the specified time.
        /// </summary>
        public double getBalanceAt(int time)
        {
            if (balanceOverTime.Count == 0)
                return 0;
            if (balanceOverTime[0].time > time)
                return 0;//If the first entry is later than the requested time point, the account wasn't relevant yet.
            int i = getLastIndexBeforeTime(time);
            return balanceOverTime[i].balance;
        }

        /// <summary>
        /// Returns the account's current balance.
        /// </summary>
        public double getCurrentBalance()
        {
            return balanceOverTime.Last().balance;
        }

        /// <summary>
        /// Add a balance point indicating the account had a specified balance since the given time (until the next balance point).
        /// </summary>
        public void addBalancePoint(int time, double balance)
        {
            var bp = new BalancePoint(time, balance);
            if (balanceOverTime.Count == 0)
                balanceOverTime.Add(bp);
            else if (balanceOverTime[balanceOverTime.Count - 1].time <= time)//Time comes after last timepoint, add to the end.
                balanceOverTime.Add(bp);
            else
            {
                int i = getFirstIndexAfterTime(time);
                balanceOverTime.Insert(i, bp);
            }
        }

        /// <summary>
        /// Returns the index of the element in the balanceOverTime List with the smallest timestamp greater than the specified time.
        /// </summary>
        private int getFirstIndexAfterTime(int time)
        {
            if (balanceOverTime.Count == 0)
                return -1;
            if (balanceOverTime[balanceOverTime.Count - 1].time <= time) //Last timepoint earlier than requested time.
                return -1;

            int i = 0;
            while (i < balanceOverTime.Count && balanceOverTime[i].time <= time)
                i++;

            return i;
        }

        /// <summary>
        /// Returns the index of the element in the balanceOverTime List with the largest timestamp lower than the specified time.
        /// </summary>
        private int getLastIndexBeforeTime(int time)
        {
            if (balanceOverTime.Count == 0)
                return -1;
            if (balanceOverTime[0].time > time) //First timepoint later than requested time.
                return -1;

            int i = 0;
            while (i + 1 < balanceOverTime.Count && balanceOverTime[i + 1].time <= time)
                i++;

            return i;
        }
    }
    
    class BalancePoint
    {
        public int time;
        public double balance;

        public BalancePoint(int time, double balance)
        {
            this.time = time;
            this.balance = balance;
        }
    }
}
