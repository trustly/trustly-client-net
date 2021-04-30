﻿using System;
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
    public class TrustlyApiClient
    {
        private static readonly List<TrustlyApiClient> _staticRegisteredClients = new List<TrustlyApiClient>();

        private readonly TrustlyApiClientSettings _settings;

        private readonly JsonRpcFactory _objectFactory = new();
        private readonly Serializer serializer = new();
        private readonly JsonRpcSigner _signer;
        private readonly JsonRpcValidator _validator = new();

        public event EventHandler<AccountNotificationData> OnAccount;
        public event EventHandler<CancelNotificationData> OnCancel;
        public event EventHandler<CreditNotificationData> OnCredit;
        public event EventHandler<DebitNotificationData> OnDebit;
        public event EventHandler<PayoutConfirmationNotificationData> OnPayoutConfirmation;
        public event EventHandler<PendingNotificationData> OnPending;
        public event EventHandler<UnknownNotificationData> OnUnknownNotification;

        private readonly Dictionary<string, Action<string>> _methodToNotificationMapper = new();

        public TrustlyApiClient(TrustlyApiClientSettings settings)
        {
            this._settings = settings;
            this._signer = new JsonRpcSigner(serializer, this._settings);

            this._methodToNotificationMapper.Add("account", (json) => this.HandleNotificationFromString(json, this.OnAccount));
            this._methodToNotificationMapper.Add("cancel", (json) => this.HandleNotificationFromString(json, this.OnCancel));
            this._methodToNotificationMapper.Add("credit", (json) => this.HandleNotificationFromString(json, this.OnCredit));
            this._methodToNotificationMapper.Add("debit", (json) => this.HandleNotificationFromString(json, this.OnDebit));
            this._methodToNotificationMapper.Add("payoutconfirmation", (json) => this.HandleNotificationFromString(json, this.OnPayoutConfirmation));
            this._methodToNotificationMapper.Add("pending", (json) => this.HandleNotificationFromString(json, this.OnPending));

            this._methodToNotificationMapper.Add(string.Empty, (json) => this.HandleNotificationFromString(json, this.OnUnknownNotification));
        }

        public void NotificationRegistration()
        {
            TrustlyApiClient._staticRegisteredClients.Add(this);
        }

        public void NotificationDeregistration()
        {
            TrustlyApiClient._staticRegisteredClients.Remove(this);
        }

        public static IEnumerable<TrustlyApiClient> GetRegisteredClients()
        {
            return _staticRegisteredClients;
        }

        public AccountLedgerResponseData AccountLedger(AccountLedgerRequestData request)
        {
            return this.SendRequest<AccountLedgerRequestData, AccountLedgerResponseData>(request, "AccountLedger");
        }

        public AccountPayoutResponseData AccountPayout(AccountPayoutRequestData request)
        {
            return this.SendRequest<AccountPayoutRequestData, AccountPayoutResponseData>(request, "AccountPayout");
        }

        public ApproveWithdrawalResponseData ApproveWithdrawal(ApproveWithdrawalRequestData request)
        {
            return this.SendRequest<ApproveWithdrawalRequestData, ApproveWithdrawalResponseData>(request, "ApproveWithdrawal");
        }

        public BalanceResponseData Balance(BalanceRequestData request)
        {
            return this.SendRequest<BalanceRequestData, BalanceResponseData>(request, "Balance");
        }

        public CancelChargeResponseData CancelCharge(CancelChargeRequestData request)
        {
            return this.SendRequest<CancelChargeRequestData, CancelChargeResponseData>(request, "CancelCharge");
        }

        public ChargeResponseData Charge(ChargeRequestData request)
        {
            return this.SendRequest<ChargeRequestData, ChargeResponseData>(request, "Charge");
        }

        public DenyWithdrawalResponseData DenyWithdrawal(DenyWithdrawalRequestData request)
        {
            return this.SendRequest<DenyWithdrawalRequestData, DenyWithdrawalResponseData>(request, "DenyWithdrawal");
        }

        public DepositResponseData Deposit(DepositRequestData request)
        {
            return this.SendRequest<DepositRequestData, DepositResponseData>(request, "Deposit");
        }

        public GetWithdrawalsResponseData GetWithdrawals(GetWithdrawalsRequestData request)
        {
            return this.SendRequest<GetWithdrawalsRequestData, GetWithdrawalsResponseData>(request, "GetWithdrawals");
        }

        public RefundResponseData Refund(RefundRequestData request)
        {
            return this.SendRequest<RefundRequestData, RefundResponseData>(request, "Refund");
        }

        public SettlementReportResponseData SettlementReport(SettlementReportRequestData request)
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

        public void HandleNotificationFromRequest(HttpRequest request)
        {
            if (string.Equals(request.Method, "post", StringComparison.InvariantCultureIgnoreCase))
            {
                using (var sr = new StreamReader(request.Body))
                {
                    var requestStringBody = sr.ReadToEnd();
                    this.HandleNotificationFromString(requestStringBody);
                }
            }
            else
            {
                throw new TrustlyNotificationException("Notifications are only allowed to be received as a HTTP Post");
            }
        }

        public void HandleNotificationFromString(string jsonString)
        {
            var jsonToken = JToken.Parse(jsonString);

            var methodToken = jsonToken.SelectToken("$.method");
            var methodValue = methodToken.Value<string>().ToLower(CultureInfo.InvariantCulture);

            var mapper = this._methodToNotificationMapper.ContainsKey(methodValue)
                ? this._methodToNotificationMapper[methodValue]
                : this._methodToNotificationMapper[string.Empty];

            // This will then call generic method HandleNotificationFromString below.
            // We do it this way to keep the dynamic generic parameters intact.
            mapper(jsonString);
        }

        private void HandleNotificationFromString<TReqData>(string jsonString, EventHandler<TReqData> eventHandler)
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

            eventHandler(this, rpcRequest.Params.Data);
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
