using System;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Trustly.Api.Domain.Base;

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

        public void Sign<TData>(JsonRpcRequest<TData> request)
            where TData : IRequestParamsData
        {
            var serializedData = this._serializer.SerializeData(request.Params.Data);
            var plainText = this.CreatePlaintext(serializedData, request.Method, request.Params.UUID);

            var signer = SignerUtilities.GetSigner(SHA1_WITH_RSA);
            signer.Init(true, this._settings.ClientPrivateKey);

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            signer.BlockUpdate(plainBytes, 0, plainBytes.Length);

            var signedBytes = signer.GenerateSignature();

            var signedString = Convert.ToBase64String(signedBytes);
            request.Params.Signature = signedString;
        }

        public bool Verify<TData>(JsonRpcRequest<TData> request)
            where TData : IRequestParamsData
        {
            return this.Verify(request.Method, request.Params.UUID, request.Params.Signature, request.Params.Data);
        }

        public bool Verify<TData>(JsonRpcResponse<TData> response)
            where TData : IResponseResultData
        {
            return this.Verify(response.GetMethod(), response.GetUUID(), response.GetSignature(), response.GetData());
        }

        private bool Verify(string method, string uuid, string expectedSignature, IData data)
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
