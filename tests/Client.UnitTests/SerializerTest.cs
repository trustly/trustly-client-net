using NUnit.Framework;
using Trustly.Api.Client;
using Trustly.Api.Domain.Exceptions;
using Trustly.Api.Domain.Requests;

namespace Trustly.Api.Domain.UnitTests
{
    public class SerializerTest
    {
        [Test]
        public void TestSerializingDepositWithoutValidation()
        {
            var serializer = new Serializer();
            var factory = new JsonRpcFactory();

            var jsonRpc = factory.Create(new DepositRequestData
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
            }, "Deposit");

            var serialized = serializer.SerializeData(jsonRpc.Params.Data);
            var expected = "AttributesCurrencySEKFirstnameJohnIP123.123.123.123LastnameDoeLocalesv_SEMobilePhone+46709876543NationalIdentificationNumber790131-1234EndUserID12345MessageIDyour_unique_deposit_idNotificationURLURL_to_your_notification_servicePasswordmerchant_passwordUsernamemerchant_username";

            Assert.AreEqual(expected, serialized);
        }

        [Test]
        public void TestMissingDepositShopperStatement()
        {
            var serializer = new Serializer();
            var factory = new JsonRpcFactory();
            var validator = new JsonRpcValidator();

            var jsonRpc = factory.Create(new DepositRequestData
            {
                Username = "merchant_username",
                Password = "merchant_password",
                NotificationURL = "URL_to_your_notification_service",
                EndUserID = "12345",
                MessageID = "your_unique_deposit_id",
                Attributes = new DepositRequestDataAttributes
                {
                    Country = "SE",
                    Locale = "sv_SE",
                    Currency = "SEK",
                    IP = "123.123.123.123",
                    MobilePhone = "+46709876543",
                    Firstname = "John",
                    Lastname = "Doe",
                    NationalIdentificationNumber = "790131-1234"
                }
            }, "Deposit");

            Assert.Throws<TrustlyDataException>(() =>
            {
                validator.Validate(jsonRpc);
            });

            jsonRpc.Params.Data.Attributes.ShopperStatement = "A Statement";

            validator.Validate(jsonRpc);
        }
    }
}