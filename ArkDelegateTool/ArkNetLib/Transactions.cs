using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace ArkDelegateToolLib
{
    public class Transactions
    {

        /// <summary>
        /// Queries the Ark API for a list of transactions based on the given parameters.
        /// Possible filters are: blockId, senderId, recipientId, type.
        /// Due to the way the API works, all transactions corresponding to one of the filters will be returned (instead of those only corresponding to all filters).
        /// 
        /// Automatically uses offset to return the entire list.
        /// </summary>
        /// <param name="parameters">Api query parameters</param>
        /// <returns>List of transactions</returns>
        public static List<Transaction> getTransactionsByParams(Dictionary<string, string> parameters)
        {
            var result = new List<Transaction>();

            var restClient = new RestClient("https://node1.arknet.cloud/api"); //https://github.com/restsharp/RestSharp

            while (true)
            {
                var request = new RestRequest("transactions", Method.GET);
                foreach (KeyValuePair<string, string> kv in parameters)
                    request.AddParameter(kv.Key, kv.Value);
                request.AddParameter("offset", result.Count);
                var response = restClient.Execute(request);
                var res = JsonConvert.DeserializeObject<RestResT>(response.Content);
                res.Transactions.ForEach(t => result.Add(t.convertToTransaction()));

                if (res.Count <= result.Count)
                    break;
            }

            return result;
        }

        public static List<Transaction> getTransactionsByParam(string param, string value)
        {
            return getTransactionsByParams(new Dictionary<string, string>()
            {
                {param, value }
            });
        }
    }

    public class Transaction
    {
        public string id;
        public string blockid;
        public int type;
        public int timestamp;
        public double amount;
        public double fee;
        public string senderId;
        public string recipientId;
        public string senderPublicKey;
        public string signature;
        public int confirmations;

        public bool equals(object o)
        {
            if (o is Transaction)
                return id.Equals(((Transaction)o).id);
            return false;
        }
    }

    class RestResT
    {
        public string Success;
        public List<TransString> Transactions;
        public int Count;
    }



    class TransString
    {
        public string id;
        public string blockid;
        public string type;
        public string timestamp;
        public string amount;
        public string fee;
        public string senderId;
        public string recipientId;
        public string senderPublicKey;
        public string signature;
        //public string asset;
        public string confirmations;

        public Transaction convertToTransaction()
        {
            return new Transaction()
            {
                id = this.id,
                blockid = this.blockid,
                type = int.Parse(this.type),
                timestamp = int.Parse(this.timestamp),
                amount = Helper.convertBalance(this.amount),
                fee = Helper.convertBalance(this.fee),
                senderId = this.senderId,
                recipientId = this.recipientId,
                senderPublicKey = this.senderPublicKey,
                signature = this.signature,
                confirmations = int.Parse(this.confirmations)
            };
        }
    }
}
