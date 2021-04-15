using NUnit.Framework;
using Trustly.Api.Client;
using Trustly.Api.Domain.Requests;

namespace Trustly.Api.Domain.UnitTests
{
    public class SerializerTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestSerializingDeposit()
        {
            /*
            var serializer = new Serializer();

            var jsonRpc = JsonRpcRequestFactory.Create(new DepositRequestData
            {
                Username = "merchant_username",
                Password = "merchant_password",
                NotificationURL = "URL_to_your_notification_service",
                EndUserID = "12345",
                MessageID = "your_unique_deposit_id",
                Attributes = new DepositRequestDataAttributes
                {
                    Locale = "sv_SE",
                    Currency = "SEK",
                    IP = "123.123.123.123",
                    MobilePhone = "+46709876543",
                    Firstname = "John",
                    Lastname = "Doe",
                    NationalIdentificationNumber = "790131-1234"
                }
            });

            var serialized = serializer.SerializeData(jsonRpc.Params.Data);
            var expected = "AttributesCurrencySEKFirstnameJohnIP123.123.123.123LastnameDoeLocalesv_SEMobilePhone+46709876543NationalIdentificationNumber790131-1234EndUserID12345MessageIDyour_unique_deposit_idNotificationURLURL_to_your_notification_servicePasswordmerchant_passwordUsernamemerchant_username";

            Assert.AreEqual(expected, serialized);
            */
        }
    }
}