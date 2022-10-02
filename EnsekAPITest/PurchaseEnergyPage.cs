using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsekAPITest
{
    public class PurchaseEnergyPage
    {
        public IRestResponse initialiseClientGet()
        {
            var clientUri = new RestClient("https://ensekapicandidatetest.azurewebsites.net");
            var energyGetRequest = new RestRequest("/energy", Method.GET);
            var energyGetResponse = clientUri.Execute(energyGetRequest);
            return energyGetResponse;
        }


        public IRestResponse initialiseClientPut(string energyTypeId, string quantity)
        {
            var clientUri = new RestClient("https://ensekapicandidatetest.azurewebsites.net");
            var BuyEnergyPutRequest = new RestRequest("/buy/{id}/{quantity}", Method.PUT);
            BuyEnergyPutRequest.AddUrlSegment("id", energyTypeId);
            BuyEnergyPutRequest.AddUrlSegment("quantity", quantity);
            var BuyElecEnergyPutResponse = clientUri.Execute(BuyEnergyPutRequest);
            return BuyElecEnergyPutResponse;
        }

    }
}
