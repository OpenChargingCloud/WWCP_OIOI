WWCP OIOI v4.x
==============

This software will allow the communication between World Wide Charging
Protocol (WWCP) entities and entities implementing the _OIOI Protocol_,
which is defined and used by PlugSurfing GmbH. The focus of this protocol
are the communication aspects of an e-mobility provider. For more details
on this protocol please visit https://www.plugsurfing.com and
http://docs.plugsurfing.com (this is no longer freely accessible :(
You have to ask PlugSurfing for a login now).


## Usage

For every api call you will need an api-key. You can obtain yours via an e-mail to service@plugsurfing.com.


### Charge Point Operators

First create a CPO client and choose a appropriate level of paranoia for validating the SSL/TLS certificate.

```csharp

var OIOI = new CPOClient("OIOIDevClient",
                         "dev-api.plugsurfing.com",
                         "<your api-key>",
                         RemoteCertificateValidator: (sender, cert, chain, policyerrors) => true);

```

Now we can do our first api call and verify a given RFID identification.    
As every method call is async you have to wait explicitly, use async/await or continuations.

```csharp

var result = await OIOI.RFIDVerify(Auth_Token.Parse("CAFEBABE"));

```


## Your participation

This software is Open Source under the Apache 2.0 license. We appreciate
your participation in this ongoing project, and your help to improve it
and the e-mobility ICT in general. If you find bugs, want to request a
feature or send us a pull request, feel free to use the normal GitHub
features to do so. For this please read the Contributor License Agreement
carefully and send us a signed copy or use a similar free and open license.
