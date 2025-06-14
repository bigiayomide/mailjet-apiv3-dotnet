﻿using System;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mailjet.Tests.Integration
{
    [TestClass]
    public class ContactsIntegrationTests
    {
        private static MailjetClient _client;
        private static string _contactName;
        private static string _contactEmail;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _client = new MailjetClient(Environment.GetEnvironmentVariable("MJ_APIKEY_PUBLIC"),
                Environment.GetEnvironmentVariable("MJ_APIKEY_PRIVATE"));

            _contactName = $"contact_name_{Guid.NewGuid()}";
            _contactEmail = $"{_contactName}@mailjet.com";
        }

        [TestMethod]
        public async Task ContactCRD_Test()
        {
            var createdContactId = await AssertCreateContact();

            await AssertGetContact(createdContactId);

            await AssertDeleteContact(createdContactId);
        }

        public async Task<long> AssertCreateContact()
        {
            // arrange
            var request = new MailjetRequest
                {
                    Resource = Contact.Resource,
                }
                .Property(Contact.IsExcludedFromCampaigns, "true")
                .Property(Contact.Name, _contactName)
                .Property(Contact.Email, _contactEmail);

            // act
            var response = await _client.PostAsync(request);

            // assert
            Assert.AreEqual(201, response.StatusCode);
            Assert.AreEqual(1, response.GetCount());
            Assert.AreEqual(1, response.GetTotal());

            var firstObject = response.GetData()[0];

            Assert.AreEqual(true, firstObject["IsExcludedFromCampaigns"].GetValue<bool>());
            Assert.AreEqual(_contactName, firstObject["Name"].GetValue<string>());
            Assert.AreEqual(_contactEmail, firstObject["Email"].GetValue<string>());

            var id = firstObject["ID"].GetValue<long>();

            return id;
        }

        private async Task AssertGetContact(long contactId)
        {
            // arrange
            MailjetRequest request = new MailjetRequest
            {
                Resource = Contact.Resource,
                ResourceId = ResourceId.Numeric(contactId)
            };

            // act
            MailjetResponse response = await _client.GetAsync(request);

            // assert
            Assert.AreEqual(200, response.StatusCode);
            Assert.AreEqual(1, response.GetCount());
            Assert.AreEqual(1, response.GetTotal());

            var firstObject = response.GetData()[0];

            Assert.AreEqual(true, firstObject["IsExcludedFromCampaigns"].GetValue<bool>());
            Assert.AreEqual(_contactName, firstObject["Name"].GetValue<string>());
            Assert.AreEqual(_contactEmail, firstObject["Email"].GetValue<string>());
            Assert.AreEqual(contactId, firstObject["ID"].GetValue<long>());
        }

        private async Task AssertDeleteContact(long contactId)
        {
            // arrange
            MailjetRequest request = new MailjetRequest
            {
                Resource = Contacts.Resource, // pay attention - GDPR DELETE contact resource is named in plural
                ResourceId = ResourceId.Numeric(contactId)
            };

            // act
            MailjetResponse response = await _client.DeleteAsync(request);

            // assert
            Assert.AreEqual(200, response.StatusCode);
        }
    }
}
