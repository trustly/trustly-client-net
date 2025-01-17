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
    [Route("GetAccountTransactions")]
    public class GetAccountTransactionsController : AbstractBaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            var request = new Api.Domain.Requests.GetAccountTransactionsRequestData()
            {
                NotificationURL = "https://localhost:52714/api/Notification",
                EndUserID = "user@email.com",
                MessageID = Guid.NewGuid().ToString(),
                Locale = "en_GB",
                SuccessURL = "https://localhost:52714/Success",
                Country = "DE",
                FailURL = "https://localhost:52714/Failed",
                Attributes = new Api.Domain.Requests.GetAccountTransactionsRequestAttributes()
                {
                    Firstname = "John",
                    Lastname = "Smith",
                    Email = "user@email.com",
                    MobilePhone = "070-1234567",
                    NationalIdentificationNumber = "010101-1234",
                    IP = "123.123.123.123",
                    DateOfBirth = "1979-01-31"
                }
            };

            var response = this.Client.GetAccountTransactions(request);

            return View(new GetAccountTransactionsViewModel()
            {
                OrderID = response.OrderID,
                URL = response.URL
            });
        }
    }

    public class GetAccountTransactionsViewModel
    {
        public string URL { get; set; }
        public string OrderID { get; set; }
    }
}