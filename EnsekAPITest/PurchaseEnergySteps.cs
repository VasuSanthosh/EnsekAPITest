using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using ThirdParty.Json.LitJson;

namespace EnsekAPITest
{
    [Binding]
    public class PurchaseEnergySteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly PurchaseEnergyPage _purchaseEnergyPage;
        public PurchaseEnergySteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _purchaseEnergyPage = new PurchaseEnergyPage();
        }


        [Given(@"the user checks the status of the (.*)")]
        public void GivenTheUserChecksTheStatusOfThe(string energyType)
        {
           
            var energyGetResponse =_purchaseEnergyPage.initialiseClientGet();
            
            JObject ListEnergyDeserialise = JObject.Parse(energyGetResponse.Content);
            var energyTypeKind = ListEnergyDeserialise[energyType];
            string energyID = energyTypeKind.Value<string>("energy_id").ToString();
            string energyPPU = energyTypeKind.Value<string>("price_per_unit").ToString();
            int energyQtyRem = energyTypeKind.Value<int>("quantity_of_units");
            string energyUnitType = energyTypeKind.Value<string>("unit_type").ToString();

            _scenarioContext.Add("energyID", energyID);
            _scenarioContext.Add("energyPPU", energyPPU);
            _scenarioContext.Add("energyQtyRem", energyQtyRem);
            _scenarioContext.Add("energyUnitType", energyUnitType);
        }

        [When(@"the user try to purchase the (.*) and (.*)")]
        public void WhenTheUserTryToPurchaseTheAnd(string energyTypeId, string quantity)
        {
            var BuyElecEnergyPutResponse = _purchaseEnergyPage.initialiseClientPut(energyTypeId, quantity);

            JObject purchaseEnergy = JObject.Parse(BuyElecEnergyPutResponse.Content);
            var BuyEnergyPutStatuscode = (int)(BuyElecEnergyPutResponse).StatusCode;
            var purchaseEnergyMessage = purchaseEnergy["message"].ToString();
            Console.WriteLine("The PUT status codes is:" + BuyEnergyPutStatuscode);
            Console.WriteLine("The purchase message is:" + purchaseEnergyMessage);
            _scenarioContext.Add("StatusCode", BuyEnergyPutStatuscode);
            _scenarioContext.Add("ActualMessage", purchaseEnergyMessage);

            string orderId = purchaseEnergyMessage.Substring(purchaseEnergyMessage.IndexOf("is ") + 2).TrimEnd('.');
            _scenarioContext.Add("orderId", orderId);

        }


        [Then(@"the response should contain a valid Message (.*) (.*) (.*) (.*)  and units remaining")]
        public void ThenTheResponseShouldContainAValidMessageMAndUnitsRemaining(int quantity, string unitType, decimal CostPerUnit, string statuscode)
        {
            int qtyPurchased = quantity;
            decimal CostPUnit = CostPerUnit;
            decimal totalCost = qtyPurchased * CostPUnit;
            int qtyInStock = (int)_scenarioContext["energyQtyRem"];

            var qtyRemaining = qtyInStock - qtyPurchased;

            string orderId = _scenarioContext["orderId"].ToString();

            string actualMessage = (string)_scenarioContext["ActualMessage"];
            string expectedMessage = "You have purchased " + quantity + " " + unitType + " at a cost of " + totalCost + " there are " + qtyRemaining + " units remaining. Your order id is" + orderId + ".";

            int statusCode = (int)_scenarioContext["StatusCode"];

            if (qtyInStock > 0)
            {
                Assert.AreEqual(expectedMessage, actualMessage);
                Assert.AreEqual(200, statusCode);
            }
            else
            {
                Console.WriteLine(actualMessage);
            }

        }
        [Then(@"the order details (.*),(.*) are saved to the system")]
        public void ThenTheOrderDetailsGasAreSavedToTheSystem(string EnergyType, string Quantity)
        {
            var clientUri = new RestClient("https://ensekapicandidatetest.azurewebsites.net");
            var orderGetRequest = new RestRequest("/orders", Method.GET);
            var orderGetResponse = clientUri.Execute(orderGetRequest);

            JArray jArray = JArray.Parse(orderGetResponse.Content);
            int count = jArray.Count();
            string orderId = _scenarioContext["orderId"].ToString().TrimStart();


            foreach (JObject item in jArray)
            {
                string fuel = item.GetValue("fuel").ToString();
                string id = item.GetValue("id").ToString();
                string quantity = item.GetValue("quantity").ToString();


                if (id == orderId)
                {
                    Assert.AreEqual(EnergyType, fuel, "Energy type mismatch please check");
                    Assert.AreEqual(Quantity, quantity, "Quantity mismatch please check");
                    Assert.AreEqual(orderId, id, "OrderId mismatch please check");
                    Console.WriteLine("Assertion sucessful, Order details are:" + EnergyType + "," + Quantity + "," + orderId);
                    break;
                }
            }

        }

        [Then(@"the feb orders are verified")]
        public void ThenTheFebOrdersAreVerified()
        {
            var clientUri = new RestClient("https://ensekapicandidatetest.azurewebsites.net");
            var orderGetRequest = new RestRequest("/orders", Method.GET);
            var orderGetResponse = clientUri.Execute(orderGetRequest);

            JArray jArray = JArray.Parse(orderGetResponse.Content);
            int count = jArray.Count();

            List<string> febCount = new List<string>();

            foreach (JObject item in jArray)
            {
                string time = item.GetValue("time").ToString();
                DateTime datevalue = (Convert.ToDateTime(time.ToString()));
                string Month = datevalue.Month.ToString();

                if (Month == "2")
                {
                    febCount.Add(time);
                }
            }

            int febOrderCount = febCount.Count();
            Assert.IsTrue(febOrderCount == 2);
            Console.WriteLine("The Feb order count is verified and it is not more than 2");
        }



    }
}
