using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace Trustly.Api.Client
{
    public class TrustlyApiClientSettings
    {
        public static readonly string URL_TEST = "https://test.trustly.com/api/1";
        public static readonly string URL_PRODUCTION = "https://api.trustly.com/1";

        public string URL { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public AsymmetricKeyParameter ClientPublicKey { get; private set; }
        public AsymmetricKeyParameter ClientPrivateKey { get; private set; }

        public AsymmetricKeyParameter TrustlyPublicKey { get; private set; }

        public TrustlyApiClientSettings()
        {
        }

        public TrustlyApiClientSettings(bool test)
        {
            (test ? this.ForTest() : this.ForProduction())
                .WithSettingsFromDirectory(null);
        }

        public TrustlyApiClientSettings(bool test, string username, string password, Stream clientPublicKeyStream, Stream clientPrivateKeyStream)
        {
            (test ? this.ForTest() : this.ForProduction())
                .WithCredentials(username, password)
                .WithKeysFromStreams(clientPublicKeyStream, clientPrivateKeyStream);
        }

        public TrustlyApiClientSettings ForTest()
        {
            this.URL = URL_TEST;

            var clientAssembly = typeof(TrustlyApiClient).Assembly;
            return this.WithTrustlyPublicKeyFromStream(clientAssembly.GetManifestResourceStream("Trustly.Api.Client.Keys.trustly_test_key.cer"));
        }

        public TrustlyApiClientSettings ForProduction()
        {
            this.URL = URL_PRODUCTION;

            var clientAssembly = typeof(TrustlyApiClient).Assembly;
            return this.WithTrustlyPublicKeyFromStream(clientAssembly.GetManifestResourceStream("Trustly.Api.Client.Keys.trustly_live_key.cer"));
        }

        public TrustlyApiClientSettings WithCredentials(string username, string password)
        {
            this.Username = username;
            this.Password = password;
            return this;
        }

        public TrustlyApiClientSettings WithSettingsFromFiles(
                string clientPublicKeyPath,
                string clientPrivateKeyPath,
                string usernamePath,
                string passwordPath
            )
        {
            if (File.Exists(clientPublicKeyPath) == false)
                throw new ArgumentException($"Cannot create api settings since public key file {clientPublicKeyPath} is missing");
            if (File.Exists(clientPrivateKeyPath) == false)
                throw new ArgumentException($"Cannot create api settings since private key file {clientPrivateKeyPath} is missing");
            if (File.Exists(usernamePath) == false)
                throw new ArgumentException($"Cannot create api settings since username key file {usernamePath} is missing");
            if (File.Exists(passwordPath) == false)
                throw new ArgumentException($"Cannot create api settings since password key file {passwordPath} is missing");

            return
                this.WithKeysFromFiles(clientPublicKeyPath, clientPrivateKeyPath)
                .WithCredentials(
                    File.ReadAllText(usernamePath).Trim(),
                    File.ReadAllText(passwordPath).Trim()
                );
        }

        public TrustlyApiClientSettings WithSettingsFromDirectory(
                string directoryPath,
                string clientPublicKeyFileName = "trustly_client_public.pem",
                string clientPrivateKeyFileName = "trustly_client_private.pem",
                string clientUsernameFileName = "trustly_client_username.txt",
                string clientPsswordFileName = "trustly_client_password.txt"
            )
        {
            if (directoryPath == null)
            {
                directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            var publicKeyPath = Path.Combine(directoryPath, clientPublicKeyFileName);
            var privateKeyPath = Path.Combine(directoryPath, clientPrivateKeyFileName);
            var usernamePath = Path.Combine(directoryPath, clientUsernameFileName);
            var passwordPath = Path.Combine(directoryPath, clientPsswordFileName);

            return this.WithSettingsFromFiles(publicKeyPath, privateKeyPath, usernamePath, passwordPath);
        }

        public TrustlyApiClientSettings WithKeysFromFiles(string publicKeyFilePath, string privateKeyFilePath)
        {
            using (var publicFileStream = new FileStream(publicKeyFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var privateFileStream = new FileStream(privateKeyFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return this.WithKeysFromStreams(publicFileStream, privateFileStream);
                }
            }
        }

        public TrustlyApiClientSettings WithKeysFromStreams(Stream clientPublicKeyStream, Stream clientPrivateKeyStream)
        {
            this.WithClientPublicKeyFromStream(clientPublicKeyStream);
            this.WithClientPrivateKeyFromStream(clientPrivateKeyStream);

            return this;
        }

        public TrustlyApiClientSettings WithClientPrivateKeyFromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var pemReader = new PemReader(reader);
                var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();

                this.ClientPrivateKey = keyPair.Private;
                if (this.ClientPrivateKey == null)
                {
                    throw new ArgumentException($"Failed to load private client key stream");
                }
            }

            return this;
        }

        public TrustlyApiClientSettings WithClientPublicKeyFromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var pemReader = new PemReader(reader);
                var keyPair = (AsymmetricKeyParameter)pemReader.ReadObject();

                this.ClientPublicKey = keyPair;
                if (this.ClientPublicKey == null)
                {
                    throw new ArgumentException($"Failed to load public client key from stream");
                }
            }

            return this;
        }

        public TrustlyApiClientSettings WithTrustlyPublicKeyFromStream(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var pemReader = new PemReader(reader);
                var keyPair = (AsymmetricKeyParameter)pemReader.ReadObject();

                this.TrustlyPublicKey = keyPair;
                if (this.TrustlyPublicKey == null)
                {
                    throw new ArgumentException($"Failed to load Trustly's public key from stream");
                }
            }

            return this;
        }
    }
}
