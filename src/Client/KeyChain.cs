using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace Trustly.Api.Client
{
    public class KeyChain
    {
        public static Stream GetTrustlyTestKeyStream()
        {
            var clientAssembly = typeof(TrustlyApiClient).Assembly;
            return clientAssembly.GetManifestResourceStream("Trustly.Api.Client.Keys.trustly_test_key.cer");
        }

        public static Stream GetTrustlyLiveKeyStream()
        {
            var clientAssembly = typeof(TrustlyApiClient).Assembly;
            return clientAssembly.GetManifestResourceStream("Trustly.Api.Client.Keys.trustly_live_key.cer");
        }

        public AsymmetricKeyParameter ClientPublicKey { get; private set; }
        public AsymmetricKeyParameter ClientPrivateKey { get; private set; }

        public AsymmetricKeyParameter TrustlyPublicKey { get; private set; }

        public KeyChain(string publicKeyFilePath, string privateKeyFilePath, bool production)
        {
            using (var publicFileStream = new FileStream(publicKeyFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var privateFileStream = new FileStream(privateKeyFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var trustleyKeyStream = (production ? GetTrustlyLiveKeyStream() : GetTrustlyTestKeyStream()))
                    {
                        this.Load(publicFileStream, privateFileStream, trustleyKeyStream);
                    }
                }
            }
        }

        /// <summary>
        /// Create a new <see cref="KeyChain"/> from readers.
        /// Will not Dispose()/Close() the streams, you must do that from your own client code.
        /// </summary>
        /// <param name="clientPublicKeyStream"></param>
        /// <param name="clientPrivateKeyStream"></param>
        /// <param name="trustlyPublicKeyStream"></param>
        public KeyChain(Stream clientPublicKeyStream, Stream clientPrivateKeyStream, Stream trustlyPublicKeyStream)
        {
            this.Load(clientPublicKeyStream, clientPrivateKeyStream, trustlyPublicKeyStream);
        }

        private void Load(Stream clientPublicKeyStream, Stream clientPrivateKeyStream, Stream trustlyPublicKeyStream)
        {
            using (var reader = new StreamReader(clientPrivateKeyStream))
            {
                var pemReader = new PemReader(reader);
                var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();

                this.ClientPrivateKey = keyPair.Private;
                if (this.ClientPrivateKey == null)
                {
                    throw new ArgumentException($"Failed to load private client key stream");
                }
            }

            using (var reader = new StreamReader(clientPublicKeyStream))
            {
                var pemReader = new PemReader(reader);
                var keyPair = (AsymmetricKeyParameter)pemReader.ReadObject();

                this.ClientPublicKey = keyPair;
                if (this.ClientPublicKey == null)
                {
                    throw new ArgumentException($"Failed to load public client key from stream");
                }
            }

            using (var reader = new StreamReader(trustlyPublicKeyStream))
            {
                var pemReader = new PemReader(reader);
                var keyPair = (AsymmetricKeyParameter)pemReader.ReadObject();

                this.TrustlyPublicKey = keyPair;
                if (this.TrustlyPublicKey == null)
                {
                    throw new ArgumentException($"Failed to load Trustly's public key from stream");
                }
            }
        }
    }
}
