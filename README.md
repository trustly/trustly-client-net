# Trustly .NET API Client

## Create Client

You can easily create any instance of the client by instantiating it, given a settings object with different levels of granular options.

```C#
var client = new TrustlyApiClient(TrustlyApiClientSettings.ForDefaultTest());
```

This is a shorthand to the more elaborate:

```C#
var client = new TrustlyApiClient(TrustlyApiClientSettings
                .ForTest()
                .WithCredentialsFromUserHome()
                .WithCertificatesFromUserHome()
                .AndTrustlyCertificate());
```

Which will load client credentials and certificates from the user computer's home directory.
The default file names are:

* `trustly_client_username.txt
* `trustly_client_password.txt`
* `trustly_client_public.pem`
* `trustly_client_private.pem`

Which can of course be overridden and customized.

## Make a request

A Request is done as simply as:

```C#
var response = client.Deposit(new Trustly.Api.Domain.Requests.DepositRequestData
{
    NotificationURL = "https://fake.test.notification.trustly.com",
    MessageID = Guid.NewGuid().ToString(),
    EndUserID = "user@email.com",
    Attributes = new Trustly.Api.Domain.Requests.DepositRequestDataAttributes
    {
        Amount = "100.00",
        Firstname = "John",
        Lastname = "Doe",
        Email = "user@email.com",
        Currency = "EUR",
        Country = "SE",
        Locale = "sv_SE",
        ShopperStatement = "Trustly Test Deposit"
    }
});

var redirectOrIFrameUrl = response.URL;
```

Where the request and reponse types are typesafe and easy to handle. If there ever are properties which are not represented in the model, they will be placed under the `ExtensionData` dictionary properties on the respective object graph levels.

## Handle notifications

There are three ways to insert the notifications into the client.
All these will end up calling on events available on the client, these are:

* `OnAccount`
* `OnCancel`
* `OnCredit`
* `OnDebit`
* `OnPayoutConfirmation`
* `OnPending`
* `OnUnknown` (All properties will be placed in `ExtensionData` dictionary property)

You register to these as usual with C# events:

```C#
client.OnDebit += (sender, args) =>
{
    System.Console.WriteLine($"{args.Data.Amount} was debited");
    args.RespondWithOK();
};
```

---

1. Automatically, by registering on the MVC Application Builder

```C#
public class Startup
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // ...

        // Register Trustly Notifications Middleware
        app.UseTrustlyNotifications();

        // ...
    }
}
```

This will register an MVC Core Middleware that listens to HTTP POSTs on context path `/trustly/notifications`.

It will automatically find all instantiated Trustly Api clients and call all of them, if there are multiple ones, until one of them has reported the notification as done by calling `RespondWithOK()` or `RespondWithError()` on the event args. If no client has reported, we will automatically respond with *OK* unless an exception has been thrown (and then Report with *Error*).

For this to work you *MUST* keep a non-garbage-collected instantiation of the API Client in memory somewhere in your code.
You cannot create an API Client, do a request, and then dispose of the client. It must be kept in memory to be able to receive the middleware request's notification sent asynchronously from Trustly's server.

---

2. Manually, by calling on `client.HandleNotificationFromRequest(HttpRequest request)`.

---

3. Manually, by calling on `client.HandleNotificationFromString(String json)`.

