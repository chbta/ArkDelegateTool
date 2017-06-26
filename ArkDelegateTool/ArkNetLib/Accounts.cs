using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkDelegateToolLib
{
    public class Accounts
    {
        public static Account getAccount(string address)
        {
            var restClient = new RestClient("https://node1.arknet.cloud/api"); //https://github.com/restsharp/RestSharp
            var request = new RestRequest("accounts", Method.GET);
            request.AddParameter("address", address);
            var response = restClient.Execute(request);
            var res = JsonConvert.DeserializeObject<RestResA>(response.Content);
            //if (res.Success)
                return res.Account.convertToAccount();
            /*else
            {
                Console.WriteLine("Error retrieving account: " + address);
                return null;
            }*/
        }
    }

    public class Account
    {
        public string Address;
        public double UnconfirmedBalance;
        public double Balance;
        public string PublicKey;
        public bool UnconfirmedSignature;
        public bool SecondSignature;
        public string SecondPublicKey;
    }

    class Acc
    {
        public string Address;
        public string UnconfirmedBalance;
        public string Balance;
        public string PublicKey;
        public bool UnconfirmedSignature;
        public bool SecondSignature;
        public string SecondPublicKey;

        public Account convertToAccount()
        {
            return new Account()
            {
                Address = Address,
                UnconfirmedBalance = Helper.convertBalance(UnconfirmedBalance),
                Balance = Helper.convertBalance(Balance),
                PublicKey = PublicKey,
                UnconfirmedSignature = UnconfirmedSignature,
                SecondSignature = SecondSignature,
                SecondPublicKey = SecondPublicKey
            };
        }
    }

    class RestResA
    {
        public bool Success;
        public Acc Account;
    }
}
