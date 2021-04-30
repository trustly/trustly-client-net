using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Exceptions;
using Trustly.Api.Domain.Notifications;
using Trustly.Api.Domain.Requests;

namespace Trustly.Api.Client
{
    public class TrustlyApiClient : IDisposable
    {
        private static readonly List<TrustlyApiClient> _staticRegisteredClients = new List<TrustlyApiClient>();

        private readonly TrustlyApiClientSettings _settings;

        private readonly JsonRpcFactory _objectFactory = new();
        private readonly Serializer serializer = new();
        private readonly JsonRpcSigner _signer;
        private readonly JsonRpcValidator _validator = new();

        public event EventHandler<NotificationArgs<AccountNotificationData>> OnAccount;
        public event EventHandler<NotificationArgs<CancelNotificationData>> OnCancel;
        public event EventHandler<NotificationArgs<CreditNotificationData>> OnCredit;
        public event EventHandler<NotificationArgs<DebitNotificationData>> OnDebit;
        public event EventHandler<NotificationArgs<PayoutConfirmationNotificationData>> OnPayoutConfirmation;
        public event EventHandler<NotificationArgs<PendingNotificationData>> OnPending;
        public event EventHandler<NotificationArgs<UnknownNotificationData>> OnUnknownNotification;

        private readonly Dictionary<string, Action<string, Action, Action>> _methodToNotificationMapper = new();

        public TrustlyApiClient(TrustlyApiClientSettings settings)
        {
            this._settings = settings;
            this._signer = new JsonRpcSigner(serializer, this._settings);

            this._methodToNotificationMapper.Add("account", (json, ok, error) => this.HandleNotificationFromString(json, this.OnAccount, ok, error));
            this._methodToNotificationMapper.Add("cancel", (json, ok, error) => this.HandleNotificationFromString(json, this.OnCancel, ok, error));
            this._methodToNotificationMapper.Add("credit", (json, ok, error) => this.HandleNotificationFromString(json, this.OnCredit, ok, error));
            this._methodToNotificationMapper.Add("debit", (json, ok, error) => this.HandleNotificationFromString(json, this.OnDebit, ok, error));
            this._methodToNotificationMapper.Add("payoutconfirmation", (json, ok, error) => this.HandleNotificationFromString(json, this.OnPayoutConfirmation, ok, error));
            this._methodToNotificationMapper.Add("pending", (json, ok, error) => this.HandleNotificationFromString(json, this.OnPending, ok, error));

            this._methodToNotificationMapper.Add(string.Empty, (json, ok, error) => this.HandleNotificationFromString(json, this.OnUnknownNotification, ok, error));

            TrustlyApiClient._staticRegisteredClients.Add(this);
        }

        ~TrustlyApiClient()
        {
            TrustlyApiClient._staticRegisteredClients.Remove(this);
        }

        public void Dispose()
        {
            TrustlyApiClient._staticRegisteredClients.Remove(this);
        }

        public static IEnumerable<TrustlyApiClient> GetRegisteredClients()
        {
            return _staticRegisteredClients;
        }

        public AccountLedgerResponseData AccountLedger(AccountLedgerRequestData request, string uuid = null)
        {
            return this.SendRequest<AccountLedgerRequestData, AccountLedgerResponseData>(request, "AccountLedger");
        }

        public AccountPayoutResponseData AccountPayout(AccountPayoutRequestData request, string uuid = null)
        {
            return this.SendRequest<AccountPayoutRequestData, AccountPayoutResponseData>(request, "AccountPayout");
        }

        public ApproveWithdrawalResponseData ApproveWithdrawal(ApproveWithdrawalRequestData request, string uuid = null)
        {
            return this.SendRequest<ApproveWithdrawalRequestData, ApproveWithdrawalResponseData>(request, "ApproveWithdrawal");
        }

        public BalanceResponseData Balance(BalanceRequestData request, string uuid = null)
        {
            return this.SendRequest<BalanceRequestData, BalanceResponseData>(request, "Balance");
        }

        public CancelChargeResponseData CancelCharge(CancelChargeRequestData request, string uuid = null)
        {
            return this.SendRequest<CancelChargeRequestData, CancelChargeResponseData>(request, "CancelCharge");
        }

        public ChargeResponseData Charge(ChargeRequestData request, string uuid = null)
        {
            return this.SendRequest<ChargeRequestData, ChargeResponseData>(request, "Charge");
        }

        public DenyWithdrawalResponseData DenyWithdrawal(DenyWithdrawalRequestData request, string uuid = null)
        {
            return this.SendRequest<DenyWithdrawalRequestData, DenyWithdrawalResponseData>(request, "DenyWithdrawal");
        }

        public DepositResponseData Deposit(DepositRequestData request, string uuid = null)
        {
            return this.SendRequest<DepositRequestData, DepositResponseData>(request, "Deposit");
        }

        public GetWithdrawalsResponseData GetWithdrawals(GetWithdrawalsRequestData request, string uuid = null)
        {
            return this.SendRequest<GetWithdrawalsRequestData, GetWithdrawalsResponseData>(request, "GetWithdrawals");
        }

        public RefundResponseData Refund(RefundRequestData request, string uuid = null)
        {
            return this.SendRequest<RefundRequestData, RefundResponseData>(request, "Refund");
        }

        public SettlementReportResponseData SettlementReport(SettlementReportRequestData request, string uuid = null)
        {
            var response = this.SendRequest<SettlementReportRequestData, SettlementReportResponseData>(request, "ViewAutomaticSettlementDetailsCSV");

            var parser = new SettlementReportParser();
            var entries = parser.Parse(response.CsvContent);
            response.Entries = entries;

            return response;
        }

        public WithdrawResponseData Withdraw(WithdrawRequestData request)
        {
            return this.SendRequest<WithdrawRequestData, WithdrawResponseData>(request, "Withdraw");
        }

        /// <summary>
        /// Used internally to create a request package.
        /// You usually do not need to directly call this method unless you are creating a custom
        /// request that exist in the documentation but not as a managed type in this class.
        /// </summary>
        /// <typeparam name="TReqData">The type of the request data</typeparam>
        /// <param name="requestData">The request data that will be used for the request</param>
        /// <param name="method">The method of the JsonRpc package</param>
        /// <returns>A signed and validated JsonRpc request package</returns>
        public JsonRpcRequest<TReqData> CreateRequestPackage<TReqData>(string method, TReqData requestData)
            where TReqData : IRequestParamsData
        {
            var rpcRequest = this._objectFactory.Create(requestData, method);

            this._signer.Sign(rpcRequest);
            this._validator.Validate(rpcRequest);

            return rpcRequest;
        }

        /// <summary>
        /// Sends given request to Trustly.
        /// </summary>
        /// <param name="requestData">Request to send to Trustly API</param>
        /// <returns>Response generated from the request</returns>
        public TRespData SendRequest<TReqData, TRespData>(TReqData requestData, string method)
            where TReqData : IToTrustlyRequestParamsData
            where TRespData : IResponseResultData
        {
            requestData.Username = this._settings.Username;
            requestData.Password = this._settings.Password;

            var rpcRequest = this.CreateRequestPackage(method, requestData);

            var requestString = JsonConvert.SerializeObject(rpcRequest, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var responseString = NewHttpPost(requestString);
            var rpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<TRespData>>(responseString);

            if (!this._signer.Verify(rpcResponse))
            {
                throw new TrustlySignatureException("Incoming data signature is not valid");
            }

            if (!rpcResponse.IsSuccessfulResult())
            {
                var message = rpcResponse.Error?.Message ?? rpcResponse.Error?.Name ?? ("" + rpcResponse.Error?.Code);
                throw new TrustlyDataException("Received an error response from the Trustly API: " + message)
                {
                    ResponseError = rpcResponse.Error
                };
            }

            if (string.IsNullOrEmpty(rpcResponse.GetUUID()) || !rpcResponse.GetUUID().Equals(rpcRequest.Params.UUID))
            {
                throw new TrustlyDataException("Incoming UUID is not valid");
            }

            return rpcResponse.Result.Data;
        }

        public void HandleNotificationFromRequest(HttpRequest request, Action onOK = null, Action onError = null)
        {
            if (string.Equals(request.Method, "post", StringComparison.InvariantCultureIgnoreCase))
            {
                using (var sr = new StreamReader(request.Body))
                {
                    var requestStringBody = sr.ReadToEnd();
                    this.HandleNotificationFromString(requestStringBody, onOK, onError);
                }
            }
            else
            {
                throw new TrustlyNotificationException("Notifications are only allowed to be received as a HTTP Post");
            }
        }

        public void HandleNotificationFromString(string jsonString, Action onOK = null, Action onError = null)
        {
            var jsonToken = JToken.Parse(jsonString);

            var methodToken = jsonToken.SelectToken("$.method");
            var methodValue = methodToken.Value<string>().ToLower(CultureInfo.InvariantCulture);

            var mapper = this._methodToNotificationMapper.ContainsKey(methodValue)
                ? this._methodToNotificationMapper[methodValue]
                : this._methodToNotificationMapper[string.Empty];

            // This will then call generic method HandleNotificationFromString below.
            // We do it this way to keep the dynamic generic parameters intact.
            mapper(jsonString, onOK, onError);
        }

        private void HandleNotificationFromString<TReqData>(
                string jsonString,
                EventHandler<NotificationArgs<TReqData>> eventHandler,
                Action onOK,
                Action onError
            )
            where TReqData : IRequestParamsData
        {
            var rpcRequest = JsonConvert.DeserializeObject<JsonRpcRequest<TReqData>>(jsonString);

            // Verify the notification (RpcRequest from Trustly) signature.
            if (!this._signer.Verify(rpcRequest))
            {
                throw new TrustlySignatureException("Could not validate signature of notification from Trustly. Is the public key for Trustly the correct one, for test or production?");
            }

            // Validate the incoming request instance.
            // Most likely this will do nothing, since we are lenient on things sent from Trustly server.
            // But we do this in case anything is needed to be validated on the local domain classes in the future.
            this._validator.Validate(rpcRequest);

            if (eventHandler == null || eventHandler.GetInvocationList().Length == 0)
            {
                throw new TrustlyNoNotificationListenerException($"Received an incoming '{rpcRequest.Method}' notification, but there was no event listener subscribing to it");
            }

            eventHandler(this, new NotificationArgs<TReqData>(rpcRequest.Params.Data, onOK, onError));
        }

        /// <summary>
        /// Sends an HTTP POST to Trustly server.
        /// </summary>
        /// <param name="request">String representation of a request</param>
        /// <returns>String representation of a response</returns>
        protected string NewHttpPost(string request)
        {
            var requestBytes = Encoding.UTF8.GetBytes(request);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(this._settings.URL);

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = requestBytes.Length;
            httpWebRequest.Method = "POST";

            using (var requestStream = httpWebRequest.GetRequestStream())
            {
                using (var streamWriter = new BinaryWriter(requestStream, Encoding.UTF8))
                {
                    streamWriter.Write(requestBytes);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            var responseStream = httpResponse.GetResponseStream();
            if (responseStream == null)
            {
                throw new NullReferenceException("ResponseStream from HTTP POST is null");
            }

            using (var streamReader = new StreamReader(responseStream, Encoding.UTF8))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
        }
    }
}
