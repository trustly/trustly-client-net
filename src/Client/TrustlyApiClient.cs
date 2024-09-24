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
using Trustly.Api.Domain;
using Trustly.Api.Domain.Exceptions;

namespace Trustly.Api.Client
{
    public class TrustlyApiClient
    {
        public static JsonSerializerSettings DEFAULT_SERIALIZER_SETTINGS = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public TrustlyApiClientSettings Settings { get; }

        private readonly JsonRpcFactory _objectFactory = new JsonRpcFactory();
        private readonly Serializer _serializer;
        private JsonRpcValidator Validator { get; } = new JsonRpcValidator();

        public Func<string, WebRequest> RequestCreator { get; set; }
        public JsonRpcSigner Signer { get; }
        private JsonSerializerSettings SerializerSettings { get; }

        public event EventHandler<NotificationArgs<AccountDefaultNotificationData, AckData>> OnAccount;
        public event EventHandler<NotificationArgs<CancelDefaultNotificationData, AckData>> OnCancel;
        public event EventHandler<NotificationArgs<CreditDefaultNotificationData, AckData>> OnCredit;
        public event EventHandler<NotificationArgs<DebitDefaultNotificationData, DebitNotificationResponseData>> OnDebit;
        public event EventHandler<NotificationArgs<PayoutConfirmationNotificationData, AckData>> OnPayoutConfirmation;
        public event EventHandler<NotificationArgs<PayoutFailedNotificationData, AckData>> OnPayoutFailed;
        public event EventHandler<NotificationArgs<PendingDefaultNotificationData, AckData>> OnPending;
        public event EventHandler<NotificationArgs<KYCNotificationData, KYCNotificationResponseData>> OnKYC;
        public event EventHandler<NotificationArgs<Any, AckData>> OnUnknownNotification;

        public TrustlyApiClient(TrustlyApiClientSettings settings)
        {
            this.SerializerSettings = TrustlyApiClient.DEFAULT_SERIALIZER_SETTINGS;
            this._serializer = new Serializer();
            this.Settings = settings;
            this.Signer = new JsonRpcSigner(_serializer, this.Settings);
        }

        public IList<AccountLedgerResponseDataEntry> AccountLedger(AccountLedgerRequestData request, string uuid = null)
        => this.SendRequest<AnyAttributes, AccountLedgerRequestData, IList<AccountLedgerResponseDataEntry>>(request, "AccountLedger", uuid);
        public AccountPayoutResponseData AccountPayout(AccountPayoutRequestData request, string uuid = null)
        => this.SendRequest<AccountPayoutRequestDataAttributes, AccountPayoutRequestData, AccountPayoutResponseData>(request, "AccountPayout", uuid);
        public ApproveWithdrawalResponseData ApproveWithdrawal(ApproveWithdrawalRequestData request, string uuid = null)
        => this.SendRequest<AnyAttributes, ApproveWithdrawalRequestData, ApproveWithdrawalResponseData>(request, "ApproveWithdrawal", uuid);
        public IList<BalanceResponseDataEntry> Balance(BalanceRequestData request, string uuid = null)
        => this.SendRequest<AnyAttributes, BalanceRequestData, IList<BalanceResponseDataEntry>>(request, "Balance", uuid);
        public CancelChargeResponseData CancelCharge(CancelChargeRequestData request, string uuid = null)
        => this.SendRequest<AnyAttributes, CancelChargeRequestData, CancelChargeResponseData>(request, "CancelCharge", uuid);
        public DenyWithdrawalResponseData DenyWithdrawal(DenyWithdrawalRequestData request, string uuid = null)
        => this.SendRequest<AnyAttributes, DenyWithdrawalRequestData, DenyWithdrawalResponseData>(request, "DenyWithdrawal", uuid);
        public DepositResponseData Deposit(DepositRequestData request, string uuid = null)
        => this.SendRequest<DepositRequestDataAttributes, DepositRequestData, DepositResponseData>(request, "Deposit", uuid);
        public GetWithdrawalsResponseDataEntry[] GetWithdrawals(GetWithdrawalsRequestData request, string uuid = null)
        => this.SendRequest<AnyAttributes, GetWithdrawalsRequestData, GetWithdrawalsResponseDataEntry[]>(request, "GetWithdrawals", uuid);
        public RefundResponseData Refund(RefundRequestData request, string uuid = null)
        => this.SendRequest<RefundRequestDataAttributes, RefundRequestData, RefundResponseData>(request, "Refund", uuid);
        public CreateAccountResponseData CreateAccount(CreateAccountRequestData request, string uuid = null)
        => this.SendRequest<CreateAccountRequestDataAttributes, CreateAccountRequestData, CreateAccountResponseData>(request, "CreateAccount", uuid);
        public SelectAccountResponseData SelectAccount(SelectAccountRequestData request, string uuid = null)
        => this.SendRequest<SelectAccountRequestDataAttributes, SelectAccountRequestData, SelectAccountResponseData>(request, "SelectAccount", uuid);
        public RegisterAccountResponseData RegisterAccount(RegisterAccountRequestData request, string uuid = null)
        => this.SendRequest<RegisterAccountRequestDataAttributes, RegisterAccountRequestData, RegisterAccountResponseData>(request, "RegisterAccount", uuid);
        public RegisterAccountPayoutResponseData RegisterAccountPayout(RegisterAccountPayoutRequestData request, string uuid = null)
        => this.SendRequest<RegisterAccountPayoutRequestDataAttributes, RegisterAccountPayoutRequestData, RegisterAccountPayoutResponseData>(request, "RegisterAccountPayout", uuid);
        public WithdrawResponseData Withdraw(WithdrawRequestData request)
        => this.SendRequest<WithdrawRequestDataAttributes, WithdrawRequestData, WithdrawResponseData>(request, "Withdraw");
        public MerchantSettlementResponseData MerchantSettlement(MerchantSettlementRequestData request)
        => this.SendRequest<AnyAttributes, MerchantSettlementRequestData, MerchantSettlementResponseData>(request, "MerchantSettlement");
        public SwishResponseData Swish(SwishRequestData request)
        => this.SendRequest<SwishRequestDataAttributes, SwishRequestData, SwishResponseData>(request, "Swish");
        public DirectDebitMandateResponseData DirectDebitMandate(DirectDebitMandateRequestData request)
        => this.SendRequest<DirectDebitMandateRequestDataAttributes, DirectDebitMandateRequestData, DirectDebitMandateResponseData>(request, "DirectDebitMandate");
        public CancelDirectDebitMandateResponseData CancelDirectDebitMandate(CancelDirectDebitRequestData request)
        => this.SendRequest<AnyAttributes, CancelDirectDebitRequestData, CancelDirectDebitMandateResponseData>(request, "CancelDirectDebitMandate");
        public ImportDirectDebitMandateResponseData ImportDirectDebitMandate(ImportDirectDebitMandateRequestData request)
        => this.SendRequest<ImportDirectDebitMandateRequestDataAttributes, ImportDirectDebitMandateRequestData, ImportDirectDebitMandateResponseData>(request, "ImportDirectDebitMandate");
        public DirectDebitResponseData DirectDebit(DirectDebitRequestData request)
        => this.SendRequest<DirectDebitRequestDataAttributes, DirectDebitRequestData, DirectDebitResponseData>(request, "DirectDebit");
        public CancelDirectDebitResponseData CancelDirectDebit(CancelDirectDebitRequestData request)
        => this.SendRequest<AnyAttributes, CancelDirectDebitRequestData, CancelDirectDebitResponseData>(request, "CancelDirectDebit");
        public DirectCreditResponseData DirectCredit(DirectCreditRequestData request)
        => this.SendRequest<DirectDebitRequestDataAttributes, DirectCreditRequestData, DirectCreditResponseData>(request, "DirectCredit");
        public RefundDirectDebitResponseData RefundDirectDebit(RefundDirectDebitRequestData request)
        => this.SendRequest<AnyAttributes, RefundDirectDebitRequestData, RefundDirectDebitResponseData>(request, "RefundDirectDebit");
        public DirectPaymentBatchResponseData DirectPaymentBatch(DirectPaymentBatchRequestData request)
        => this.SendRequest<DirectPaymentBatchRequestDataAttributes, DirectPaymentBatchRequestData, DirectPaymentBatchResponseData>(request, "DirectPaymentBatch");

        public ChargeResponseData Charge(ChargeRequestData request, string uuid = null)
        {
            var response = this.SendRequest<ChargeRequestDataAttributes, ChargeRequestData, ChargeResponseData>(request, "Charge", uuid);
            if (response.Result == StringBoolean.FALSE)
            {
                var message = response.Rejected ?? "The request was rejected for an unknown reason";
                throw new TrustlyRejectionException("Received a rejection response from the Trustly API: " + message)
                {
                    Reason = response.Rejected
                };
            }

            return response;
        }

        public List<SettlementReportResponseDataEntry> SettlementReport(SettlementReportRequestData request, string uuid = null)
        {
            var response = this.SendRequest<SettlementReportRequestDataAttributes, SettlementReportRequestData, SettlementReportResponseData>(request, "ViewAutomaticSettlementDetailsCSV", uuid);
            var entries = new SettlementReportParser().Parse(response.ViewAutomaticSettlementDetails);

            return entries;
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
        public JsonRpcRequest<TReqAttr, TReqData, JsonRpcRequestParams<TReqAttr, TReqData>> CreateRequestPackage<TReqAttr, TReqData>(TReqData requestData, string method, string uuid = null)
            where TReqAttr : AbstractRequestDataAttributes
            where TReqData : AbstractRequestData<TReqAttr>
        {
            var rpcRequest = this._objectFactory.Create<TReqAttr, TReqData>(requestData, method, uuid);

            rpcRequest.Params.Signature = this.Signer.CreateSignature(rpcRequest.Method, rpcRequest.Params.UUID, requestData);
            this.Validator.Validate(rpcRequest);

            return rpcRequest;
        }

        /// <summary>
        /// Used internally to create a response package.
        /// </summary>
        /// <typeparam name="TResData"></typeparam>
        /// <returns>A signed and validated JsonRpc response package</returns>
        public JsonRpcResponse<TResData, ResponseResult<TResData>> CreateResponsePackage<TResData>(TResData responseData, string method, string requestUuid)
        {
            var rpcResponse = this._objectFactory.CreateResponse(responseData, method, requestUuid);

            rpcResponse.Result.Signature = this.Signer.CreateSignature(rpcResponse.Result.Method, rpcResponse.Result.UUID, rpcResponse.Result.Data);
            this.Validator.Validate(rpcResponse);

            return rpcResponse;
        }

        /// <summary>
        /// Sends given request to Trustly.
        /// </summary>
        /// <param name="requestData">Request to send to Trustly API</param>
        /// <param name="method">The RPC method name of the request</param>
        /// <param name="uuid">Optional UUID for the request. If not specified, a Guid will be generated</param>
        /// <returns>Response data returned from the request</returns>
        public TResData SendRequest<TReqAttr, TReqData, TResData>(TReqData requestData, string method, string uuid = null)
            where TReqAttr : AbstractRequestDataAttributes
            where TReqData : AbstractRequestData<TReqAttr>
        {
            requestData.Username = this.Settings.Username;
            requestData.Password = this.Settings.Password;

            var rpcRequest = this.CreateRequestPackage<TReqAttr, TReqData>(requestData, method, uuid);
            return this.SendRequest<TReqAttr, TReqData, TResData>(rpcRequest);
        }

        /// <summary>
        /// Sends given request to Trustly.
        /// </summary>
        /// <param name="rpcRequest">The full JsonRpc request to send to the Trustly API</param>
        /// <returns>Response data returned from the request</returns>
        public TResData SendRequest<TReqAttr, TReqData, TResData>(JsonRpcRequest<TReqAttr, TReqData, JsonRpcRequestParams<TReqAttr, TReqData>> rpcRequest)
            where TReqAttr : AbstractRequestDataAttributes
            where TReqData : AbstractRequestData<TReqAttr>
        {
            var requestString = JsonConvert.SerializeObject(rpcRequest, this.SerializerSettings);

            var responseString = NewHttpPost(requestString);
            var responseNode = JObject.Parse(responseString);

            if (responseNode.ContainsKey("error") || !responseNode.ContainsKey("result"))
            {
                var rpcErrorResponse = responseNode.ToObject<JsonRpcErrorResponse>();

                var message = rpcErrorResponse.Error?.Message ?? rpcErrorResponse.Error?.Name ?? ("" + rpcErrorResponse.Error?.Code);
                throw new TrustlyDataException("Received an error response from the Trustly API: " + message)
                {
                    ResponseError = rpcErrorResponse.Error
                };
            }
            else
            {
                var rpcResponse = responseNode.ToObject<JsonRpcResponse<TResData, ResponseResult<TResData>>>();

                if (!this.Signer.Verify(rpcResponse.Result.Method, rpcResponse.Result.UUID, responseNode["result"]["data"], rpcResponse.Result.Signature))
                {
                    throw new TrustlySignatureException("Incoming data signature is not valid");
                }

                if (string.IsNullOrEmpty(rpcResponse.Result.UUID) || !rpcResponse.Result.UUID.Equals(rpcRequest.Params.UUID))
                {
                    throw new TrustlyDataException("Incoming UUID is not valid");
                }

                return rpcResponse.Result.Data;
            }
        }

        public async Task<int> HandleNotificationFromRequestAsync(HttpRequest request, NotificationRespondDelegate callback)
        {
            if (!string.Equals(request.Method, "post", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new TrustlyNotificationException("Notifications are only allowed to be received as a HTTP Post");
            }

            using (var sr = new StreamReader(request.Body))
            {
                var jsonString = await sr.ReadToEndAsync();

                var jsonToken = JToken.Parse(jsonString);
                var methodValue = jsonToken.Value<string>("method").ToLower(CultureInfo.InvariantCulture);

                switch (methodValue)
                {
                    case "account": return HandleNotification(jsonToken, this.OnAccount, callback);
                    case "cancel": return HandleNotification(jsonToken, this.OnCancel, callback);
                    case "credit": return HandleNotification(jsonToken, this.OnCredit, callback);
                    case "debit": return HandleNotification(jsonToken, this.OnDebit, callback);
                    case "payoutconfirmation": return HandleNotification(jsonToken, this.OnPayoutConfirmation, callback);
                    case "pending": return HandleNotification(jsonToken, this.OnPending, callback);
                    case "kyc": return HandleNotification(jsonToken, this.OnKYC, callback);
                    default: return HandleNotification(jsonToken, this.OnUnknownNotification, callback);
                };
            }
        }

        private int HandleNotification<TNotificationData, TAckData>(
            JToken token,
            EventHandler<NotificationArgs<TNotificationData, TAckData>> eventHandler,
            NotificationRespondDelegate callback
        )
        {
            if (eventHandler == null)
            {
                return 0;
            }

            var notification = token.ToObject<JsonRpcNotification<TNotificationData, JsonRpcNotificationParams<TNotificationData>>>();

            // Verify the notification (RpcRequest from Trustly) signature.
            if (!this.Signer.Verify(notification.Method, notification.Params.UUID, token["params"]["data"], notification.Params.Signature))
            {
                throw new TrustlySignatureException("Could not validate signature of notification from Trustly. Is the public key for Trustly the correct one, for test or production?");
            }

            // Validate the incoming request instance.
            // Most likely this will do nothing, since we are lenient on things sent from Trustly server.
            // But we do this in case anything is needed to be validated on the local domain classes in the future.
            this.Validator.Validate(notification);

            var args = new NotificationArgs<TNotificationData, TAckData>(notification.Params.Data, notification.Method, notification.Params.UUID, async (ackData) =>
            {
                var responseObject = this._objectFactory.CreateResponse(notification, ackData);

                responseObject.Result.Signature = this.Signer.CreateSignature(
                    responseObject.Result.Method,
                    responseObject.Result.UUID,
                    responseObject.Result.Data
                );

                var responseStr = JsonConvert.SerializeObject(responseObject, this.SerializerSettings);

                await callback(responseStr);
            });

            eventHandler(this, args);

            return eventHandler.GetInvocationList().Length;
        }

        protected virtual WebRequest CreateWebRequest(string url)
        {
            return WebRequest.Create(url);
        }

        /// <summary>
        /// Sends an HTTP POST to Trustly server.
        /// </summary>
        /// <param name="request">String representation of a request</param>
        /// <returns>String representation of a response</returns>
        protected string NewHttpPost(string request)
        {
            var requestBytes = Encoding.UTF8.GetBytes(request);
            var httpWebRequest = (this.RequestCreator ?? this.CreateWebRequest)(this.Settings.URL);

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

            var httpResponse = httpWebRequest.GetResponse();

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
