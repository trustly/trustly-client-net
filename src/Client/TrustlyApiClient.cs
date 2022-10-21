using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

        public TrustlyApiClientSettings Settings { get; }

        private readonly JsonRpcFactory _objectFactory = new JsonRpcFactory();
        private readonly Serializer serializer = new Serializer();
        private readonly JsonRpcSigner _signer;
        private readonly JsonRpcValidator _validator = new JsonRpcValidator();

        public event EventHandler<NotificationArgs<AccountNotificationData>> OnAccount;
        public event EventHandler<NotificationArgs<CancelNotificationData>> OnCancel;
        public event EventHandler<NotificationArgs<CreditNotificationData>> OnCredit;
        public event EventHandler<NotificationArgs<DebitNotificationData>> OnDebit;
        public event EventHandler<NotificationArgs<PayoutConfirmationNotificationData>> OnPayoutConfirmation;
        public event EventHandler<NotificationArgs<PendingNotificationData>> OnPending;
        public event EventHandler<NotificationArgs<UnknownNotificationData>> OnUnknownNotification;

        private readonly Dictionary<string, Func<string, NotificationResponseDelegate, NotificationFailResponseDelegate, int>> _methodToNotificationMapper
            = new Dictionary<string, Func<string, NotificationResponseDelegate, NotificationFailResponseDelegate, int>>();

        public TrustlyApiClient(TrustlyApiClientSettings settings)
        {
            this.Settings = settings;
            this._signer = new JsonRpcSigner(serializer, this.Settings);

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
            return this.SendRequest<AccountLedgerRequestData, AccountLedgerResponseData>(request, "AccountLedger", uuid);
        }

        public AccountPayoutResponseData AccountPayout(AccountPayoutRequestData request, string uuid = null)
        {
            return this.SendRequest<AccountPayoutRequestData, AccountPayoutResponseData>(request, "AccountPayout", uuid);
        }

        public ApproveWithdrawalResponseData ApproveWithdrawal(ApproveWithdrawalRequestData request, string uuid = null)
        {
            return this.SendRequest<ApproveWithdrawalRequestData, ApproveWithdrawalResponseData>(request, "ApproveWithdrawal", uuid);
        }

        public BalanceResponseData Balance(BalanceRequestData request, string uuid = null)
        {
            return this.SendRequest<BalanceRequestData, BalanceResponseData>(request, "Balance", uuid);
        }

        public CancelChargeResponseData CancelCharge(CancelChargeRequestData request, string uuid = null)
        {
            return this.SendRequest<CancelChargeRequestData, CancelChargeResponseData>(request, "CancelCharge", uuid);
        }

        public ChargeResponseData Charge(ChargeRequestData request, string uuid = null)
        {
            return this.SendRequest<ChargeRequestData, ChargeResponseData>(request, "Charge", uuid);
        }

        public DenyWithdrawalResponseData DenyWithdrawal(DenyWithdrawalRequestData request, string uuid = null)
        {
            return this.SendRequest<DenyWithdrawalRequestData, DenyWithdrawalResponseData>(request, "DenyWithdrawal", uuid);
        }

        public DepositResponseData Deposit(DepositRequestData request, string uuid = null)
        {
            return this.SendRequest<DepositRequestData, DepositResponseData>(request, "Deposit", uuid);
        }

        public GetWithdrawalsResponseData GetWithdrawals(GetWithdrawalsRequestData request, string uuid = null)
        {
            return this.SendRequest<GetWithdrawalsRequestData, GetWithdrawalsResponseData>(request, "GetWithdrawals", uuid);
        }

        public RefundResponseData Refund(RefundRequestData request, string uuid = null)
        {
            return this.SendRequest<RefundRequestData, RefundResponseData>(request, "Refund", uuid);
        }

        public CreateAccountResponseData CreateAccount(CreateAccountRequestData request, string uuid = null)
        {
            return this.SendRequest<CreateAccountRequestData, CreateAccountResponseData>(request, "CreateAccount", uuid);
        }

        public SelectAccountResponseData SelectAccount(SelectAccountRequestData request, string uuid = null)
        {
            return this.SendRequest<SelectAccountRequestData, SelectAccountResponseData>(request, "SelectAccount", uuid);
        }

        public RegisterAccountResponseData RegisterAccount(RegisterAccountRequestData request, string uuid = null)
        {
            return this.SendRequest<RegisterAccountRequestData, RegisterAccountResponseData>(request, "RegisterAccount", uuid);
        }

        public SettlementReportResponseData SettlementReport(SettlementReportRequestData request, string uuid = null)
        {
            var response = this.SendRequest<SettlementReportRequestData, SettlementReportResponseData>(request, "ViewAutomaticSettlementDetailsCSV", uuid);

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
        public JsonRpcRequest<TReqData> CreateRequestPackage<TReqData>(TReqData requestData, string method, string uuid = null)
            where TReqData : IRequestParamsData
        {
            var rpcRequest = this._objectFactory.Create(requestData, method, uuid);

            this._signer.Sign(rpcRequest);
            this._validator.Validate(rpcRequest);

            return rpcRequest;
        }

        /// <summary>
        /// Used internally to create a response package.
        /// </summary>
        /// <typeparam name="TResData"></typeparam>
        /// <returns>A signed and validated JsonRpc response package</returns>
        public JsonRpcResponse<TResData> CreateResponsePackage<TResData>(string method, string requestUuid, TResData responseData)
            where TResData : IResponseResultData
        {
            var rpcResponse = new JsonRpcResponse<TResData>
            {
                Result = new ResponseResult<TResData>
                {
                    Method = method,
                    UUID = requestUuid,
                    Data = responseData
                },
                Version = "1.1"
            };

            this._signer.Sign(rpcResponse);
            this._validator.Validate(rpcResponse);

            return rpcResponse;
        }

        /// <summary>
        /// Sends given request to Trustly.
        /// </summary>
        /// <param name="requestData">Request to send to Trustly API</param>
        /// <param name="method">The RPC method name of the request</param>
        /// <param name="uuid">Optional UUID for the request. If not specified, a Guid will be generated</param>
        /// <returns>Response generated from the request</returns>
        public TRespData SendRequest<TReqData, TRespData>(TReqData requestData, string method, string uuid = null)
            where TReqData : IToTrustlyRequestParamsData
            where TRespData : IResponseResultData
        {
            requestData.Username = this.Settings.Username;
            requestData.Password = this.Settings.Password;

            var rpcRequest = this.CreateRequestPackage(requestData, method, uuid);

            var requestString = JsonConvert.SerializeObject(rpcRequest, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            var responseString = NewHttpPost(requestString);
            var rpcResponse = JsonConvert.DeserializeObject<JsonRpcResponse<TRespData>>(responseString);

            if (!rpcResponse.IsSuccessfulResult())
            {
                var message = rpcResponse.Error?.Message ?? rpcResponse.Error?.Name ?? ("" + rpcResponse.Error?.Code);
                throw new TrustlyDataException("Received an error response from the Trustly API: " + message)
                {
                    ResponseError = rpcResponse.Error
                };
            }

            if (rpcResponse.Result.Data is IWithRejectionResult rejectionResult)
            {
                if (!rejectionResult.Result)
                {
                    var message = rejectionResult.Rejected ?? "The request was rejected for an unknown reason";
                    throw new TrustlyRejectionException("Received a rejection response from the Trustly API: " + message)
                    {
                        Reason = rejectionResult.Rejected
                    };
                }
            }

            if (!this._signer.Verify(rpcResponse))
            {
                throw new TrustlySignatureException("Incoming data signature is not valid");
            }

            if (string.IsNullOrEmpty(rpcResponse.GetUUID()) || !rpcResponse.GetUUID().Equals(rpcRequest.Params.UUID))
            {
                throw new TrustlyDataException("Incoming UUID is not valid");
            }

            return rpcResponse.Result.Data;
        }

        public async Task<int> HandleNotificationFromRequestAsync(HttpRequest request, NotificationResponseDelegate onOK = null, NotificationFailResponseDelegate onFailed = null)
        {
            if (string.Equals(request.Method, "post", StringComparison.InvariantCultureIgnoreCase))
            {
                using (var sr = new StreamReader(request.Body))
                {
                    var requestStringBody = await sr.ReadToEndAsync();
                    return this.HandleNotificationFromString(requestStringBody, onOK, onFailed);
                }
            }
            else
            {
                throw new TrustlyNotificationException("Notifications are only allowed to be received as a HTTP Post");
            }
        }

        public int HandleNotificationFromString(string jsonString, NotificationResponseDelegate onOK = null, NotificationFailResponseDelegate onFailed = null)
        {
            var jsonToken = JToken.Parse(jsonString);

            var methodToken = jsonToken.SelectToken("$.method");
            var methodValue = methodToken.Value<string>().ToLower(CultureInfo.InvariantCulture);

            var mapper = this._methodToNotificationMapper.ContainsKey(methodValue)
                ? this._methodToNotificationMapper[methodValue]
                : this._methodToNotificationMapper[string.Empty];

            // This will then call generic method HandleNotificationFromString below.
            // We do it this way to keep the dynamic generic parameters intact.
            return mapper(jsonString, onOK, onFailed);
        }

        private int HandleNotificationFromString<TReqData>(
                string jsonString,
                EventHandler<NotificationArgs<TReqData>> eventHandler,
                NotificationResponseDelegate onOK,
                NotificationFailResponseDelegate onFailed
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

            try
            {
                eventHandler(this, new NotificationArgs<TReqData>(rpcRequest.Params.Data, rpcRequest.Method, rpcRequest.Params.UUID, onOK, onFailed));
            }
            catch (Exception ex)
            {
                var message = this.Settings.IncludeExceptionMessageInNotificationResponse ? ex.Message : null;
                onFailed(rpcRequest.Method, rpcRequest.Params.UUID, message);
            }

            return eventHandler.GetInvocationList().Length;
        }

        /// <summary>
        /// Sends an HTTP POST to Trustly server.
        /// </summary>
        /// <param name="request">String representation of a request</param>
        /// <returns>String representation of a response</returns>
        protected string NewHttpPost(string request)
        {
            var requestBytes = Encoding.UTF8.GetBytes(request);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.Settings.URL);

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
