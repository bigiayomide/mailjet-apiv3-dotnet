using System;
using System.Net;
using System.Text.Json.Nodes;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RichardSzalay.MockHttp;
using Sms = Mailjet.Client.Resources.SMS;

namespace Mailjet.Tests
{

    [TestClass]
    public class MailjetClientTests
    {
        private const string JsonMediaType = "application/json";
        private const string ApiKeyTest = "apikeytest";
        private const string ApiSecretTest = "apisecrettest";
        private const string TotalKey = "Total";
        private const string CountKey = "Count";
        private const string DataKey = "Data";
        private const string Status = "Status";
        private const string Code = "Code";
        private const string Name = "Name";
        private const string Description = "Description";

        private string API_TOKEN;

        [TestInitialize]
        public void TestInit()
        {
            API_TOKEN = "ApiToken";
        }

        [TestMethod]
        public void TestGetAsync()
        {
            int expectedTotal = 1;
            int expectedCount = 1;

            var expectedData = new JsonArray
            {
                new JsonObject
                {
                    { Apikey.APIKey, "ApiKeyTest" },
                },
            };

            var mockHttp = new MockHttpMessageHandler();

            string jsonResponse = GenerateJsonResponse(expectedTotal, expectedCount, expectedData);

            // Setup a respond for the user api (including a wildcard in the URL)
            mockHttp.When("https://api.mailjet.com/v3/*")
                    .Respond(JsonMediaType, jsonResponse); // Respond with JSON

            // Inject the handler into your application code
            IMailjetClient client = new MailjetClient(ApiKeyTest, ApiSecretTest, mockHttp);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Apikey.Resource,
            };

            MailjetResponse response = client.GetAsync(request).Result;
            
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(expectedTotal, response.GetTotal());
            Assert.AreEqual(expectedCount, response.GetCount());
            Assert.IsTrue(JsonValue.DeepEquals(expectedData, response.GetData()));
        }

        [TestMethod]
        public void TestTooManyRequestsStatus()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://api.mailjet.com/v3/*")
                   .Respond(((HttpStatusCode) 429));

            // Inject the handler into your application code
            IMailjetClient client = new MailjetClient(ApiKeyTest, ApiSecretTest, mockHttp);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Apikey.Resource,
            };

            MailjetResponse response = client.GetAsync(request).Result;

            Assert.AreEqual(429, response.StatusCode);
            Assert.AreEqual("Too many requests", response.GetErrorInfo());
        }


        [TestMethod]
        public void TestInternalServerErrorStatus()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://api.mailjet.com/v3/*")
                   .Respond(HttpStatusCode.InternalServerError);


            // Inject the handler into your application code
            IMailjetClient client = new MailjetClient(ApiKeyTest, ApiSecretTest, mockHttp);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Apikey.Resource,
            };

            MailjetResponse response = client.GetAsync(request).Result;

            Assert.AreEqual(500, response.StatusCode);
            Assert.AreEqual("Internal Server Error", response.GetErrorInfo());
        }

        [TestMethod]
        public void TestSmsCountAsync()
        {
            int expectedTotal = 1;
            int expectedCount = 1;
            var expectedData = new JsonArray();

            var mockHttp = new MockHttpMessageHandler();

            var jsonResponse = GenerateJsonResponse(expectedTotal, expectedCount, expectedData);

            // Setup a respond for the user api (including a wildcard in the URL)
            mockHttp.When("https://api.mailjet.com/v4/*")
                    .Respond(JsonMediaType, jsonResponse); // Respond with JSON

            IMailjetClient client = new MailjetClient(API_TOKEN, mockHttp);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Sms.Count.Resource
            }
            .Filter(Sms.Count.FromTS, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())
            .Filter(Sms.Count.ToTS, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());


            MailjetResponse response = client.GetAsync(request).Result;

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(expectedTotal, response.GetTotal());
            Assert.AreEqual(expectedCount, response.GetCount());
            Assert.IsTrue(JsonArray.DeepEquals(expectedData, response.GetData()));
        }

        [TestMethod]
        public void TestSmsExportAsync()
        {
            int expectedCode = 1;
            string expectedName = "PENDING";
            string expectedDescription = "The request is accepted.";

            var status = new JsonObject
            {
                { Code, expectedCode},
                { Name, expectedName},
                { Description, expectedDescription}
            };

            var smsExportResponse = new JsonObject
            {
                { Status, status }
            };

            var mockHttp = new MockHttpMessageHandler();
            // Setup a respond for the user api (including a wildcard in the URL)
            mockHttp.When("https://api.mailjet.com/v4/*")
                    .Respond(JsonMediaType, GenerateJsonResponse(smsExportResponse)); // Respond with JSON

            // timsestamp range offset
            int offset = 1000;

            IMailjetClient client = new MailjetClient(API_TOKEN, mockHttp);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Sms.Export.Resource
            }
            .Property(Sms.Export.FromTS, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            .Property(Sms.Export.ToTS, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + offset);


            MailjetResponse response = client.PostAsync(request).Result;

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(expectedCode, response.GetData()[0][Status][Code].GetValue<int>());
            Assert.AreEqual(expectedName, response.GetData()[0][Status][Name].GetValue<string>());
            Assert.AreEqual(expectedDescription, response.GetData()[0][Status][Description].GetValue<string>());
        }

        [TestMethod]
        public void TestSmsStatisticsAsync()
        {
            var expectedData = new JsonArray();
            var mockHttp = new MockHttpMessageHandler();
            var jsonResponse = GenerateJsonResponse(1, 1, expectedData);

            // Setup a respond for the user api (including a wildcard in the URL)
            mockHttp.When("https://api.mailjet.com/v4/*")
                    .Respond(JsonMediaType, jsonResponse); // Respond with JSON

            IMailjetClient client = new MailjetClient(API_TOKEN, mockHttp);

            MailjetRequest request = new MailjetRequest
            {
                Resource = Sms.SMS.Resource
            }
            .Filter(Sms.SMS.FromTS, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())
            .Filter(Sms.SMS.ToTS, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

            MailjetResponse response = client.GetAsync(request).Result;

            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.IsTrue(JsonArray.DeepEquals(expectedData, response.GetData()));
        }

        private string GenerateJsonResponse(int total, int count, JsonArray data)
        {
            var jObject = new JsonObject()
            {
                { TotalKey, total },
                { CountKey, count },
                { DataKey, data },
            };

            return GenerateJsonResponse(jObject);
        }

        private string GenerateJsonResponse(JsonObject jObject)
        {
            return jObject.ToString();
        }
    }
}
