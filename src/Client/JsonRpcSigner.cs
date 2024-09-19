using System;
using System.Text;
using Org.BouncyCastle.Security;

namespace Trustly.Api.Client
{
    public class JsonRpcSigner
    {
        private const string SHA1_WITH_RSA = "SHA1withRSA";

        private readonly Serializer _serializer;
        private readonly TrustlyApiClientSettings _settings;

        public JsonRpcSigner(Serializer serializer, TrustlyApiClientSettings settings)
        {
            this._serializer = serializer;
            this._settings = settings;
        }

        public string CreatePlaintext(string serializedData, string method, string uuid)
        {
            return string.Format("{0}{1}{2}", method, uuid, serializedData);
        }

        public string CreateSignature(string method, string uuid, object data)
        {
            var serializedData = this._serializer.SerializeData(data);
            var plainText = this.CreatePlaintext(serializedData, method, uuid);

            var signer = SignerUtilities.GetSigner(SHA1_WITH_RSA);
            signer.Init(true, this._settings.ClientPrivateKey);

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            signer.BlockUpdate(plainBytes, 0, plainBytes.Length);

            var signedBytes = signer.GenerateSignature();

            return Convert.ToBase64String(signedBytes);
        }

        public bool Verify<TData>(string method, string uuid, TData data, string expectedSignature)
        {
            var serializedResponseData = this._serializer.SerializeData(data);
            var responsePlainText = this.CreatePlaintext(serializedResponseData, method, uuid);

            var responseBytes = Encoding.UTF8.GetBytes(responsePlainText);

            var expectedSignatureBytes = Convert.FromBase64String(expectedSignature);

            var signer = SignerUtilities.GetSigner(SHA1_WITH_RSA);
            signer.Init(false, this._settings.TrustlyPublicKey);
            signer.BlockUpdate(responseBytes, 0, responseBytes.Length);

            return signer.VerifySignature(expectedSignatureBytes);
        }
    }
}
