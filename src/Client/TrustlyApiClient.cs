using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Trustly.Api.Client.Validation;
using Trustly.Api.Domain.Base;
using Trustly.Api.Domain.Exceptions;
using Trustly.Api.Domain.Requests;

namespace Trustly.Api.Client
{
    public class TrustlyApiClient
    {
        private readonly TrustlyApiClientSettings _settings;

        private readonly JsonRpcFactory _objectFactory = new();
        private readonly Serializer serializer = new();
        private readonly JsonRpcSigner _signer;
        private readonly JsonRpcValidator _validator = new();

        public TrustlyApiClient(TrustlyApiClientSettings settings)
        {
            this._settings = settings;
            this._signer = new JsonRpcSigner(serializer, this._settings);
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
        /// Sends given request to Trustly.
        /// </summary>
        /// <param name="requestData">Request to send to Trustly API</param>
        /// <returns>Response generated from the request</returns>
        protected TRespData SendRequest<TReqData, TRespData>(TReqData requestData, string method)
            where TReqData : IToTrustlyRequestParamsData
            where TRespData : IResponseResultData
        {
            requestData.Username = this._settings.Username;
            requestData.Password = this._settings.Password;

            var rpcRequest = this._objectFactory.Create(requestData, method);

            this._signer.Sign(rpcRequest);
            this._validator.Validate(rpcRequest);

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
