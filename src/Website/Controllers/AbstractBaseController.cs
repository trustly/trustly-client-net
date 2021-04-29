using System;
using Microsoft.AspNetCore.Mvc;
using Trustly.Api.Client;

namespace Trustly.Website.Controllers
{
    public abstract class AbstractBaseController : Controller
    {
        protected TrustlyApiClient Client { get; private set; }

        public AbstractBaseController()
        {
            this.Client = new TrustlyApiClient(new TrustlyApiClientSettings(test: true));
        }
    }
}
