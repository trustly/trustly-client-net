// The MIT License (MIT)
//
// Copyright (c) 2017 Trustly Group AB
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Microsoft.AspNetCore.Mvc;
using System;

namespace Trustly.Website.Controllers
{
    [Route("Deposit")]
    public class DepositController : AbstractBaseController
    {
        [HttpGet]
        public ActionResult Index(
            [FromQuery(Name = "amount")] string amount,
            [FromQuery(Name = "currency")] string currency
            )
        {
            var request = new Api.Domain.Requests.DepositRequestData
            {
                EndUserID = "user@email.com",
                MessageID = Guid.NewGuid().ToString(),
                NotificationURL = "https://localhost:52714/api/Notification",
                Attributes = new Api.Domain.Requests.DepositRequestDataAttributes
                {
                    Locale = "en_GB",
                    Firstname = "John",
                    Lastname = "Smith",
                    Email = "user@email.com",
                    MobilePhone = "070-1234567",
                    NationalIdentificationNumber = "010101-1234",
                    SuccessURL = "https://localhost:52714/Success",
                    Amount = amount ?? "10.00",
                    Currency = currency ?? "EUR",
                    Country = "SE",
                    ShopperStatement = "A Statement"
                }
            };

            var response = this.Client.Deposit(request);

            return View(new DepositViewModel
            {
                URL = response.URL,
                Amount = request.Attributes.Amount,
                Currency = request.Attributes.Currency
            });
        }
    }

    public class DepositViewModel
    {
        public string URL { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
    }
}
