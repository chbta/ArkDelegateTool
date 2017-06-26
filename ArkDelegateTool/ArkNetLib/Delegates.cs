using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkDelegateToolLib
{
    public class Delegates
    {
        public static Delegate getDelegate(string username)
        {
            var restClient = new RestClient("https://node1.arknet.cloud/api"); //https://github.com/restsharp/RestSharp
            var request = new RestRequest("delegates/get", Method.GET);
            request.AddParameter("username",username);
            var response = restClient.Execute(request);
            var res = JsonConvert.DeserializeObject<RestResD>(response.Content);
            return res.Delegate.convertToDelegate();
        }

        public static List<string> getVoters(string publicKey)
        {
            var restClient = new RestClient("https://node1.arknet.cloud/api"); //https://github.com/restsharp/RestSharp
            var request = new RestRequest("delegates/voters", Method.GET);
            request.AddParameter("publicKey", publicKey);
            var response = restClient.Execute(request);
            var res = JsonConvert.DeserializeObject<RestResDV>(response.Content);
            return res.Accounts.Select(a => a.Address).ToList();
        }
    }

    public class Delegate
    {
        public string Username;
        public string Address;
        public string PublicKey;
        public double Vote;
        public int Producedblocks;
        public int Missedblocks;
        public int Rate;
        public double Approval;
        public double Productivity;
    }

    class Del
    {
        public string Username;
        public string Address;
        public string PublicKey;
        public string Vote;
        public int Producedblocks;
        public int Missedblocks;
        public int Rate;
        public double Approval;
        public double Productivity;

        public Delegate convertToDelegate()
        {
            return new Delegate()
            {
                Username = Username,
                Address = Address,
                PublicKey = PublicKey,
                Vote = Helper.convertBalance(Vote),
                Producedblocks = Producedblocks,
                Missedblocks = Missedblocks,
                Rate = Rate,
                Approval = Approval,
                Productivity = Productivity
            };
        }
    }

    class RestResD
    {
        public bool Success;
        public Del Delegate;
    }

    class RestResDV
    {
        public bool Success;
        public List<Voter> Accounts;
    }

    class Voter
    {
        public string Address;
    }
}
