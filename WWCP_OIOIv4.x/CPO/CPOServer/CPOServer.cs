/*
 * Copyright (c) 2016-2020 GraphDefined GmbH
 * This file is part of WWCP OIOI <https://github.com/OpenChargingCloud/WWCP_OIOI>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Authentication;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.Sockets;
using org.GraphDefined.Vanaheimr.Hermod.Sockets.TCP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

    /// <summary>
    /// An OIOI CPO Server.
    /// </summary>
    public class CPOServer
    {

        #region Data

        /// <summary>
        /// The default HTTP server name.
        /// </summary>
        public new const           String    DefaultHTTPServerName  = "GraphDefined OIOI " + Version.Number + " HTTP CPO Server API";

        /// <summary>
        /// The default HTTP server TCP port.
        /// </summary>
        public new static readonly IPPort    DefaultHTTPServerPort  = IPPort.Parse(4567);

        /// <summary>
        /// The default HTTP server URI prefix.
        /// </summary>
        public     static readonly HTTPPath   DefaultURLPrefix       = HTTPPath.Parse("/api/v4/request");

        /// <summary>
        /// The default query timeout.
        /// </summary>
        public new static readonly TimeSpan  DefaultQueryTimeout    = TimeSpan.FromMinutes(1);


        private readonly ServerAPIKeyValidatorDelegate  APIKeyValidator;

        #endregion

        #region Properties

        /// <summary>
        /// The HTTP server of this API.
        /// </summary>
        public HTTPServer                                   HTTPServer          { get; }

        /// <summary>
        /// The HTTP hostname of this API.
        /// </summary>
        public HTTPHostname                                 HTTPHostname        { get; }

        /// <summary>
        /// The common URI prefix of the HTTP server of this API for all incoming requests.
        /// </summary>
        public HTTPPath                                      URLPrefix           { get; }

        /// <summary>
        /// The HTTP content type used by this service.
        /// </summary>
        public HTTPContentType                              ServerContentType   { get;  }

        /// <summary>
        /// The DNS client used by this API.
        /// </summary>
        public DNSClient                                    DNSClient           { get; }

        #endregion

        #region Events

        #region OnAnyHTTPRequest/-Response

        /// <summary>
        /// An event sent whenever a HTTP request was received.
        /// </summary>
        public event HTTPRequestLogHandler           OnAnyHTTPRequest;

        /// <summary>
        /// An event sent whenever a HTTP response was sent.
        /// </summary>
        public event HTTPResponseLogHandler          OnAnyHTTPResponse;

        #endregion

        #region OnSessionStart(HTTP)Request/-Response

        /// <summary>
        /// An event sent whenever a session start HTTP request was received.
        /// </summary>
        public event RequestLogHandler               OnSessionStartHTTPRequest;

        /// <summary>
        /// An event sent whenever a session start request was received.
        /// </summary>
        public event OnSessionStartRequestDelegate   OnSessionStartRequest;

        /// <summary>
        /// An event sent whenever an EVSE should start charging.
        /// </summary>
        public event OnSessionStartDelegate          OnSessionStart;

        /// <summary>
        /// An event sent whenever a HTTP response to a session start request was sent.
        /// </summary>
        public event AccessLogHandler                OnSessionStartHTTPResponse;

        /// <summary>
        /// An event sent whenever a response to a session start request was sent.
        /// </summary>
        public event OnSessionStartResponseDelegate  OnSessionStartResponse;

        #endregion

        #region OnSessionStop (HTTP)Request/-Response

        /// <summary>
        /// An event sent whenever a session stop HTTP request was received.
        /// </summary>
        public event RequestLogHandler              OnSessionStopHTTPRequest;

        /// <summary>
        /// An event sent whenever a session stop request was received.
        /// </summary>
        public event OnSessionStopRequestDelegate   OnSessionStopRequest;

        /// <summary>
        /// An event sent whenever an EVSE should stop charging.
        /// </summary>
        public event OnSessionStopDelegate          OnSessionStop;

        /// <summary>
        /// An event sent whenever a HTTP response to a session stop request was sent.
        /// </summary>
        public event AccessLogHandler               OnSessionStopHTTPResponse;

        /// <summary>
        /// An event sent whenever a response to a session stop request was sent.
        /// </summary>
        public event OnSessionStopResponseDelegate  OnSessionStopResponse;

        #endregion


        #region Generic HTTP/SOAP server logging

        /// <summary>
        /// An event called whenever a HTTP request came in.
        /// </summary>
        public HTTPRequestLogEvent   RequestLog    = new HTTPRequestLogEvent();

        /// <summary>
        /// An event called whenever a HTTP request could successfully be processed.
        /// </summary>
        public HTTPResponseLogEvent  ResponseLog   = new HTTPResponseLogEvent();

        /// <summary>
        /// An event called whenever a HTTP request resulted in an error.
        /// </summary>
        public HTTPErrorLogEvent     ErrorLog      = new HTTPErrorLogEvent();

        #endregion

        #endregion

        #region Constructor(s)

        #region CPOServer(TCPPort = null, ...)

        /// <summary>
        /// Create a new OIOI CPO Server using the given parameters.
        /// </summary>
        /// <param name="DefaultServerName">The default HTTP servername, used whenever no HTTP Host-header had been given.</param>
        /// <param name="HTTPHostname">An optional HTTP hostname.</param>
        /// <param name="HTTPPort">An IP port to listen on.</param>
        /// <param name="ServerCertificateSelector">An optional delegate to select a SSL/TLS server certificate.</param>
        /// <param name="ClientCertificateValidator">An optional delegate to verify the SSL/TLS client certificate used for authentication.</param>
        /// <param name="ClientCertificateSelector">An optional delegate to select the SSL/TLS client certificate used for authentication.</param>
        /// <param name="AllowedTLSProtocols">The SSL/TLS protocol(s) allowed for this connection.</param>
        /// <param name="URLPrefix">The URI prefix for all incoming HTTP requests.</param>
        /// 
        /// <param name="ServerThreadName">The optional name of the TCP server thread.</param>
        /// <param name="ServerThreadPriority">The optional priority of the TCP server thread.</param>
        /// <param name="ServerThreadIsBackground">Whether the TCP server thread is a background thread or not.</param>
        /// <param name="ConnectionIdBuilder">An optional delegate to build a connection identification based on IP socket information.</param>
        /// <param name="ConnectionThreadsNameBuilder">An optional delegate to set the name of the TCP connection threads.</param>
        /// <param name="ConnectionThreadsPriorityBuilder">An optional delegate to set the priority of the TCP connection threads.</param>
        /// <param name="ConnectionThreadsAreBackground">Whether the TCP connection threads are background threads or not (default: yes).</param>
        /// <param name="ConnectionTimeout">The TCP client timeout for all incoming client connections in seconds (default: 30 sec).</param>
        /// <param name="MaxClientConnections">The maximum number of concurrent TCP client connections (default: 4096).</param>
        /// 
        /// <param name="DNSClient">The DNS client to use.</param>
        /// <param name="Autostart">Start the HTTP server thread immediately (default: no).</param>
        public CPOServer(String                               DefaultServerName                  = DefaultHTTPServerName,
                         HTTPHostname?                        HTTPHostname                       = null,
                         IPPort?                              HTTPPort                           = null,
                         ServerCertificateSelectorDelegate    ServerCertificateSelector          = null,
                         RemoteCertificateValidationCallback  ClientCertificateValidator         = null,
                         LocalCertificateSelectionCallback    ClientCertificateSelector          = null,
                         SslProtocols                         AllowedTLSProtocols                = SslProtocols.Tls12,
                         HTTPPath?                             URLPrefix                          = null,
                         ServerAPIKeyValidatorDelegate        APIKeyValidator                    = null,
                         HTTPContentType                      ServerContentType                  = null,
                         Boolean                              ServerRegisterHTTPRootService      = true,

                         String                               ServerThreadName                   = null,
                         ThreadPriority                       ServerThreadPriority               = ThreadPriority.AboveNormal,
                         Boolean                              ServerThreadIsBackground           = true,
                         ConnectionIdBuilder                  ConnectionIdBuilder                = null,
                         ConnectionThreadsNameBuilder         ConnectionThreadsNameBuilder       = null,
                         ConnectionThreadsPriorityBuilder     ConnectionThreadsPriorityBuilder   = null,
                         Boolean                              ConnectionThreadsAreBackground     = true,
                         TimeSpan?                            ConnectionTimeout                  = null,
                         UInt32                               MaxClientConnections               = TCPServer.__DefaultMaxClientConnections,

                         DNSClient                            DNSClient                          = null,
                         Boolean                              Autostart                          = false)

            : this(new HTTPServer(HTTPPort ?? DefaultHTTPServerPort,
                                  DefaultServerName.IsNotNullOrEmpty() ? DefaultServerName : DefaultHTTPServerName,
                                  ServerCertificateSelector,
                                  ClientCertificateValidator,
                                  ClientCertificateSelector,
                                  AllowedTLSProtocols,
                                  ServerThreadName,
                                  ServerThreadPriority,
                                  ServerThreadIsBackground,
                                  ConnectionIdBuilder,
                                  ConnectionThreadsNameBuilder,
                                  ConnectionThreadsPriorityBuilder,
                                  ConnectionThreadsAreBackground,
                                  ConnectionTimeout,
                                  MaxClientConnections,
                                  DNSClient ?? new DNSClient(),
                                  false),
                   HTTPHostname,
                   URLPrefix ?? DefaultURLPrefix,
                   APIKeyValidator,
                   ServerContentType,
                   ServerRegisterHTTPRootService)

        {

            #region / (HTTPRoot)

            if (ServerRegisterHTTPRootService &&
                URLPrefix.ToString() != "/")

                HTTPServer.AddMethodCallback(HTTPHostname ?? Vanaheimr.Hermod.HTTP.HTTPHostname.Any,
                                             HTTPMethod.GET,
                                             HTTPPath.Parse("/"),
                                             HTTPContentType.TEXT_UTF8,
                                             HTTPDelegate: Request => {

                                                 return Task.FromResult(
                                                     new HTTPResponse.Builder(Request) {

                                                         HTTPStatusCode  = HTTPStatusCode.BadGateway,
                                                         ContentType     = HTTPContentType.TEXT_UTF8,
                                                         Content         = String.Concat("Welcome at ", DefaultHTTPServerName, Environment.NewLine,
                                                                                         "This is an OIOI v", Version.Number, " endpoint!", Environment.NewLine, Environment.NewLine,
                                                                                         "Default endpoint: ", URLPrefix, Environment.NewLine, Environment.NewLine).
                                                                                  ToUTF8Bytes(),
                                                         Connection      = "close"

                                                     }.AsImmutable);

                                             },
                                             AllowReplacement: URIReplacement.Allow);

            #endregion

            //RegisterURITemplates();

            if (Autostart)
                HTTPServer.Start();

        }

        #endregion

        #region CPOServer(HTTPServer, HTTPHostname = null, URLPrefix = DefaultURLPrefix, APIKeyValidator = null, ...)

        /// <summary>
        /// Create a new OIOI CPO Server using the given parameters.
        /// </summary>
        /// <param name="HTTPServer">An existing HTTP server.</param>
        /// <param name="HTTPHostname">An optional HTTP hostname.</param>
        /// <param name="URLPrefix">The URI prefix for all incoming HTTP requests.</param>
        public CPOServer(HTTPServer                                   HTTPServer,
                         HTTPHostname?                                HTTPHostname                    = null,
                         HTTPPath?                                     URLPrefix                       = null,
                         ServerAPIKeyValidatorDelegate                APIKeyValidator                 = null,
                         HTTPContentType                              ServerContentType               = null,
                         Boolean                                      ServerRegisterHTTPRootService   = true)
        {

            this.HTTPServer         = HTTPServer   ?? throw new ArgumentNullException(nameof(HTTPServer), "The given HTTP server must not be null!");
            this.HTTPHostname       = HTTPHostname ?? Vanaheimr.Hermod.HTTP.HTTPHostname.Any;
            this.URLPrefix          = URLPrefix    ?? DefaultURLPrefix;
            this.APIKeyValidator    = APIKeyValidator;
            this.DNSClient          = HTTPServer.DNSClient;
            this.ServerContentType  = ServerContentType ?? HTTPContentType.JSON_UTF8;

            // Link HTTP events...
            HTTPServer.RequestLog   += (HTTPProcessor, ServerTimestamp, Request)                                 => RequestLog. WhenAll(HTTPProcessor, ServerTimestamp, Request);
            HTTPServer.ResponseLog  += (HTTPProcessor, ServerTimestamp, Request, Response)                       => ResponseLog.WhenAll(HTTPProcessor, ServerTimestamp, Request, Response);
            HTTPServer.ErrorLog     += (HTTPProcessor, ServerTimestamp, Request, Response, Error, LastException) => ErrorLog.   WhenAll(HTTPProcessor, ServerTimestamp, Request, Response, Error, LastException);

            RegisterURITemplates();

        }

        #endregion

        #endregion


        #region (private) RegisterURITemplates()

        private void RegisterURITemplates()
        {

            HTTPServer.AddMethodCallback(HTTPHostname.Any,
                                         HTTPMethod.POST,
                                         URLPrefix,
                                         HTTPContentType.JSON_UTF8,
                                         HTTPRequestLogger:  OnAnyHTTPRequest,
                                         HTTPResponseLogger: OnAnyHTTPResponse,
                                         HTTPDelegate:       async request => {

                                             HTTPResponse _HTTPResponse = null;

                                             if (!request.TryParseJObjectRequestBody(out JObject JSONBody, out HTTPResponse HTTPResponse))
                                                 return CreateResponse(request,
                                                                       HTTPStatusCode.BadRequest,
                                                                       Result.Error(140, "Invalid HTTP body!"));

                                             #region Parse Session Start [optional]

                                             if (JSONBody.ParseOptional("session-start",
                                                                        "session-start",
                                                                        HTTPServer.DefaultServerName,
                                                                        out JObject JSONObj,
                                                                        request,
                                                                        out HTTPResponse))
                                             {

                                                 // ---------------------------------------------------------
                                                 // curl -v -H "Accept: application/json" \
                                                 //         -H "Content-Type: application/json" \
                                                 //         -d "{ \"session-start\": { \
                                                 //                   \"user\": { \
                                                 //                       \"identifier-type\": \"evco-id\", \
                                                 //                       \"identifier\":      \"DE-GDF-123456-7\" \
                                                 //                   }, \
                                                 //                   \"connector-id\":       \"DE*GEF*E12345678\", \
                                                 //                   \"payment-reference\":  \"bitcoins\" } }" \
                                                 //         http://127.0.0.1:4567/api/v4/request
                                                 // ---------------------------------------------------------

                                                 // ---------------------------------------------------------
                                                 // curl -v -H "Accept: application/json" \
                                                 //         -H "Content-Type: application/json" \
                                                 //         -d "{ \"session-start\": { \
                                                 //                   \"user\": { \
                                                 //                       \"identifier-type\": \"evco-id\", \
                                                 //                       \"identifier\":      \"DE-BDP-123456-7\" \
                                                 //                   }, \
                                                 //                   \"connector-id\":       \"DE*BDO*EVSE*CI*TESTS*A*1\", \
                                                 //                   \"payment-reference\":  \"bitcoins\" } }" \
                                                 //         http://127.0.0.1:3004/RNs/Prod/IO/PlugSurfing
                                                 // ---------------------------------------------------------

                                                 #region Data

                                                 Result               SessionStartResult   = null;
                                                 eMobilityAccount_Id  eMobilityAccountId;
                                                 String               Token                = null;

                                                 #endregion

                                                 #region Send OnSessionStartHTTPRequest event

                                                 try
                                                 {

                                                     if (OnSessionStartHTTPRequest != null)
                                                         await Task.WhenAll(OnSessionStartHTTPRequest.GetInvocationList().
                                                                            Cast<RequestLogHandler>().
                                                                            Select(e => e(DateTime.UtcNow,
                                                                                          HTTPServer,
                                                                                          request))).
                                                                    ConfigureAwait(false);

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     e.Log(nameof(CPOServer) + "." + nameof(OnSessionStartHTTPRequest));
                                                 }

                                                 #endregion


                                                 #region Parse 'user'...

                                                 if (!JSONObj.ParseMandatory("user",
                                                                             "user",
                                                                             out JObject  UserJSON,
                                                                             out String   ErrorResponse))
                                                 {

                                                     return SendSessionStartResponse(request,
                                                                                     HTTPStatusCode.BadRequest,
                                                                                     Result.UserTokenNotValid);

                                                 }

                                                 #region Parse 'user/identifier-type'

                                                 if (!UserJSON.MapMandatory("identifier-type",
                                                                            "identifier-type",
                                                                            IdentifierTypesExtentions.AsIdentifierType,
                                                                            out IdentifierTypes  IdentifierType,
                                                                            out                  ErrorResponse))
                                                 {

                                                     return SendSessionStartResponse(request,
                                                                                     HTTPStatusCode.BadRequest,
                                                                                     Result.Error(145, "JSON property 'user/identifier-type' missing or invalid!"));

                                                 }

                                                 #endregion

                                                 #region Parse 'user/identifier'

                                                 switch (IdentifierType)
                                                 {

                                                     case IdentifierTypes.EVCOId:
                                                         if (!UserJSON.ParseMandatory("identifier",
                                                                                      "identifier",
                                                                                      eMobilityAccount_Id.TryParse,
                                                                                      out eMobilityAccountId,
                                                                                      out ErrorResponse))
                                                         {

                                                             return SendSessionStartResponse(request,
                                                                                             HTTPStatusCode.BadRequest,
                                                                                             Result.Error(145, "JSON property 'user/identifier' missing or invalid!"));

                                                         }
                                                         break;

                                                     default:
                                                         return SendSessionStartResponse(request,
                                                                                         HTTPStatusCode.BadRequest,
                                                                                         Result.Error(145, "JSON property 'user/identifier' missing or invalid!"));

                                                 }

                                                 #endregion

                                                 #region Parse 'user/token'           [optional]

                                                 switch (IdentifierType)
                                                 {

                                                     case IdentifierTypes.Username:

                                                         if (!UserJSON.ParseOptional("token",
                                                                                     "user token",
                                                                                     out Token,
                                                                                     out ErrorResponse))
                                                         {

                                                             return SendSessionStartResponse(request,
                                                                                             HTTPStatusCode.BadRequest,
                                                                                             Result.Error(145, "JSON property 'user/token' invalid!"));

                                                         }
                                                         break;

                                                 }

                                                 #endregion

                                                 #endregion

                                                 #region Parse 'connector-id'

                                                 if (!JSONObj.ParseMandatory("connector-id",
                                                                             "connector-id",
                                                                             Connector_Id.TryParse,
                                                                             out Connector_Id  ConnectorId,
                                                                             out               ErrorResponse))
                                                 {

                                                     return SendSessionStartResponse(request,
                                                                                     HTTPStatusCode.BadRequest,
                                                                                     Result.Error(310, "JSON property 'connector-id' missing or invalid!"));

                                                 }

                                                 #endregion

                                                 #region Parse 'payment-reference'    [optional]

                                                 if (JSONObj.ParseOptional("payment-reference",
                                                                           "payment reference",
                                                                           OIOIv4_x.PaymentReference.TryParse,
                                                                           out PaymentReference? PaymentReference,
                                                                           out ErrorResponse))
                                                 {

                                                     if (ErrorResponse != null)
                                                         return SendSessionStartResponse(request,
                                                                                         HTTPStatusCode.BadRequest,
                                                                                         Result.Error(310, "JSON property 'payment-reference' missing or invalid!"));

                                                 }

                                                 #endregion


                                                 #region Send OnSessionStartRequest event

                                                 try
                                                 {

                                                     if (OnSessionStartRequest != null)
                                                         await Task.WhenAll(OnSessionStartRequest.GetInvocationList().
                                                                            Cast<OnSessionStartRequestDelegate>().
                                                                            Select(e => e(DateTime.UtcNow,
                                                                                          request.Timestamp,
                                                                                          this,
                                                                                          EventTracking_Id.New,
                                                                                          new User(eMobilityAccountId.ToString(),
                                                                                                   IdentifierType,
                                                                                                   Token),
                                                                                          ConnectorId,
                                                                                          PaymentReference))).
                                                                    ConfigureAwait(false);

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     e.Log(nameof(CPOServer) + "." + nameof(OnSessionStartRequest));
                                                 }

                                                 #endregion


                                                 try
                                                 {

                                                     var OnSessionStartLocal = OnSessionStart;
                                                     if (OnSessionStartLocal != null)
                                                     {

                                                         SessionStartResult = (await Task.WhenAll(OnSessionStartLocal.
                                                                                                      GetInvocationList().
                                                                                                      Select(subscriber => (subscriber as OnSessionStartDelegate)
                                                                                                          (DateTime.UtcNow,
                                                                                                           this,
                                                                                                           eMobilityAccountId,
                                                                                                           ConnectorId,
                                                                                                           PaymentReference,
                                                                                                           new CancellationTokenSource().Token,
                                                                                                           EventTracking_Id.New,
                                                                                                           TimeSpan.FromSeconds(45))))).

                                                                                     FirstOrDefault();

                                                     }

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     e.Log(nameof(CPOServer) + "." + nameof(OnSessionStart));
                                                 }

                                                 if (SessionStartResult == null)
                                                     SessionStartResult = Result.Error(310);

                                                 #region Map code to HTTP Status Codes

                                                 var statusCode = HTTPStatusCode.BadRequest;

                                                 switch (SessionStartResult?.Code)
                                                 {

                                                     case 000: // Success
                                                     case 011: // Successfully started a charging session. The customer is charging at the EVSE.
                                                     case 012: // Successfully authorized a charging session. The customer must now plug in the cable to start.
                                                     case 310: // EVSE error
                                                     case 312: // EVSE timeout
                                                     case 320: // EVSE already in use
                                                     case 321: // No EV connected to EVSE
                                                     case 323: // EVSE already reserved
                                                         statusCode = HTTPStatusCode.OK;
                                                         break;

                                                     case 140: // Authentication failed: No positive authentication response
                                                     case 145: // Authentication failed: User token not valid
                                                         statusCode = HTTPStatusCode.Unauthorized;
                                                         break;

                                                     case 181: // EVSE not found
                                                     case 300: // Unknown charging station operator
                                                         statusCode = HTTPStatusCode.NotFound;
                                                         break;

                                                     default:
                                                         statusCode = HTTPStatusCode.BadRequest;
                                                         break;

                                                 }

                                                 #endregion


                                                 _HTTPResponse = new HTTPResponse.Builder(request) {
                                                                     HTTPStatusCode  = statusCode,
                                                                     Server          = HTTPServer.DefaultServerName,
                                                                     Date            = DateTime.UtcNow,
                                                                     ContentType     = HTTPContentType.JSON_UTF8,
                                                                     Content         = SessionStartResult.ToUTF8Bytes(),
                                                                     Connection      = "close"
                                                                 };


                                                 #region Send OnSessionStartResponse events

                                                 try
                                                 {

                                                     if (OnSessionStartResponse != null)
                                                         await Task.WhenAll(OnSessionStartResponse.GetInvocationList().
                                                                            Cast<OnSessionStartResponseDelegate>().
                                                                            Select(e => e(DateTime.UtcNow,
                                                                                          request.Timestamp,
                                                                                          this,
                                                                                          EventTracking_Id.New,
                                                                                          new User(eMobilityAccountId.ToString(),
                                                                                                   IdentifierType,
                                                                                                   Token),
                                                                                          ConnectorId,
                                                                                          PaymentReference,
                                                                                          SessionStartResult,
                                                                                          request.Timestamp - DateTime.UtcNow))).
                                                                    ConfigureAwait(false);

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     e.Log(nameof(CPOServer) + "." + nameof(OnSessionStartResponse));
                                                 }

                                                 #endregion

                                                 #region Send OnSessionStartHTTPResponse events

                                                 try
                                                 {

                                                     if (OnSessionStartHTTPResponse != null)
                                                         await Task.WhenAll(OnSessionStartHTTPResponse.GetInvocationList().
                                                                            Cast<AccessLogHandler>().
                                                                            Select(e => e(DateTime.UtcNow,
                                                                                          HTTPServer,
                                                                                          request,
                                                                                          _HTTPResponse))).
                                                                    ConfigureAwait(false);

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     e.Log(nameof(CPOServer) + "." + nameof(OnSessionStartHTTPRequest));
                                                 }

                                                 #endregion

                                             }

                                             #endregion

                                             #region Parse Session Stop  [optional]

                                             else if (JSONBody.ParseOptional("session-stop",
                                                                             "session-stop",
                                                                             HTTPServer.DefaultServerName,
                                                                             out JSONObj,
                                                                             request,
                                                                             out HTTPResponse))
                                             {

                                                 // ---------------------------------------------------------
                                                 // curl -v -H "Accept: application/json" \
                                                 //         -H "Content-Type: application/json" \
                                                 //         -d "{ \"session-stop\": { \
                                                 //                   \"user\": { \
                                                 //                       \"identifier-type\": \"evco-id\", \
                                                 //                       \"identifier\":      \"DE-GDF-123456-7\" }, \
                                                 //                   \"connector-id\":  \"DE*GEF*E12345678\", \
                                                 //                   \"session-id\":    \"a35823fb-f40a-42a3-b234-df243cb06c89\" } }" \
                                                 //         http://127.0.0.1:4567/api/v4/request
                                                 // ---------------------------------------------------------

                                                 // ---------------------------------------------------------
                                                 // curl -v -H "Accept: application/json" \
                                                 //         -H "Content-Type: application/json" \
                                                 //         -d "{ \"session-stop\": { \
                                                 //                   \"user\": { \
                                                 //                       \"identifier-type\": \"evco-id\", \
                                                 //                       \"identifier\":      \"DE-BDP-123456-7\" }, \
                                                 //                   \"connector-id\":  \"DE*GEF*E12345678\", \
                                                 //                   \"session-id\":    \"a35823fb-f40a-42a3-b234-df243cb06c89\" } }" \
                                                 //         http://127.0.0.1:3004/RNs/Prod/IO/PlugSurfing
                                                 // ---------------------------------------------------------

                                                 #region Data

                                                 Result               SessionStopResult    = null;
                                                 eMobilityAccount_Id  eMobilityAccountId;
                                                 String               Token                = null;

                                                 #endregion

                                                 #region Send OnSessionStopHTTPRequest event

                                                 try
                                                 {

                                                     if (OnSessionStopHTTPRequest != null)
                                                         await Task.WhenAll(OnSessionStopHTTPRequest.GetInvocationList().
                                                                            Cast<RequestLogHandler>().
                                                                            Select(e => e(DateTime.UtcNow,
                                                                                          HTTPServer,
                                                                                          request))).
                                                                    ConfigureAwait(false);

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     e.Log(nameof(CPOServer) + "." + nameof(OnSessionStopHTTPRequest));
                                                 }

                                                 #endregion


                                                 #region Parse 'user'...

                                                 if (!JSONObj.ParseMandatory("user",
                                                                             "user",
                                                                             out JObject  UserJSON,
                                                                             out String   ErrorResponse))
                                                 {

                                                     return SendSessionStopResponse(request,
                                                                                    HTTPStatusCode.BadRequest,
                                                                                    Result.UserTokenNotValid);

                                                 }

                                                 #region Parse 'user/identifier-type'

                                                 if (!UserJSON.MapMandatory("identifier-type",
                                                                            "identifier-type",
                                                                            IdentifierTypesExtentions.AsIdentifierType,
                                                                            out IdentifierTypes  IdentifierType,
                                                                            out                  ErrorResponse))
                                                 {

                                                     return SendSessionStopResponse(request,
                                                                                    HTTPStatusCode.BadRequest,
                                                                                    Result.Error(145, "JSON property 'user/identifier-type' missing or invalid!"));

                                                 }

                                                 #endregion

                                                 #region Parse 'user/identifier'

                                                 switch (IdentifierType)
                                                 {

                                                     case IdentifierTypes.EVCOId:
                                                         if (!UserJSON.ParseMandatory("identifier",
                                                                                      "identifier",
                                                                                      eMobilityAccount_Id.TryParse,
                                                                                      out eMobilityAccountId,
                                                                                      out ErrorResponse))
                                                         {

                                                             return SendSessionStopResponse(request,
                                                                                            HTTPStatusCode.BadRequest,
                                                                                            Result.Error(145, "JSON property 'user/identifier' missing or invalid!"));

                                                         }
                                                         break;

                                                     default:
                                                         return SendSessionStopResponse(request,
                                                                                        HTTPStatusCode.BadRequest,
                                                                                        Result.Error(145, "JSON property 'user/identifier' missing or invalid!"));

                                                 }

                                                 #endregion

                                                 #region Parse 'user/token'           [optional]

                                                 switch (IdentifierType)
                                                 {

                                                     case IdentifierTypes.Username:

                                                         if (!UserJSON.ParseOptional("token",
                                                                                     "user token",
                                                                                     out Token,
                                                                                     out ErrorResponse))
                                                         {

                                                             return SendSessionStopResponse(request,
                                                                                            HTTPStatusCode.BadRequest,
                                                                                            Result.Error(145, "JSON property 'user/token' invalid!"));

                                                         }
                                                         break;

                                                 }

                                                 #endregion

                                                 #endregion

                                                 #region Parse 'connector-id'

                                                 if (!JSONObj.ParseMandatory("connector-id",
                                                                             "connector-id",
                                                                             Connector_Id.TryParse,
                                                                             out Connector_Id  ConnectorId,
                                                                             out               ErrorResponse))
                                                 {

                                                     return SendSessionStopResponse(request,
                                                                                    HTTPStatusCode.BadRequest,
                                                                                    Result.Error(310, "JSON property 'connector-id' missing or invalid!"));

                                                 }

                                                 #endregion

                                                 #region Parse 'session-id'

                                                 if (!JSONObj.MapMandatory("session-id",
                                                                           "session-id",
                                                                           Session_Id.Parse,
                                                                           out Session_Id  SessionId,
                                                                           out             ErrorResponse))
                                                 {

                                                     return SendSessionStopResponse(request,
                                                                                    HTTPStatusCode.BadRequest,
                                                                                    Result.Error(310, "JSON property 'session-id' missing or invalid!"));

                                                 }

                                                 #endregion


                                                 #region Send OnSessionStopRequest event

                                                 try
                                                 {

                                                     if (OnSessionStopRequest != null)
                                                         await Task.WhenAll(OnSessionStopRequest.GetInvocationList().
                                                                            Cast<OnSessionStopRequestDelegate>().
                                                                            Select(e => e(DateTime.UtcNow,
                                                                                          request.Timestamp,
                                                                                          this,
                                                                                          EventTracking_Id.New,
                                                                                          new User(eMobilityAccountId.ToString(),
                                                                                                   IdentifierType,
                                                                                                   Token),
                                                                                          ConnectorId,
                                                                                          SessionId))).
                                                                    ConfigureAwait(false);

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     e.Log(nameof(CPOServer) + "." + nameof(OnSessionStopRequest));
                                                 }

                                                 #endregion


                                                 try
                                                 {

                                                     var OnSessionStopLocal = OnSessionStop;
                                                     if (OnSessionStopLocal != null)
                                                     {

                                                         SessionStopResult = (await Task.WhenAll(OnSessionStopLocal.
                                                                                                     GetInvocationList().
                                                                                                     Select(subscriber => (subscriber as OnSessionStopDelegate)
                                                                                                         (DateTime.UtcNow,
                                                                                                          this,
                                                                                                          ConnectorId,
                                                                                                          SessionId,
                                                                                                          eMobilityAccountId,
                                                                                                          new CancellationTokenSource().Token,
                                                                                                          EventTracking_Id.New,
                                                                                                          TimeSpan.FromSeconds(45))))).

                                                                                  FirstOrDefault();

                                                     }

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     e.Log(nameof(CPOServer) + "." + nameof(SessionStopResult));
                                                 }

                                                 if (SessionStopResult == null)
                                                     SessionStopResult = Result.Error(310);


                                                 #region Map code to HTTP Status Codes

                                                 var statusCode = HTTPStatusCode.BadRequest;

                                                 switch (SessionStopResult?.Code)
                                                 {

                                                     case 0:
                                                         statusCode = HTTPStatusCode.OK;
                                                         break;

                                                     case 140:
                                                     case 144:
                                                     case 145:
                                                         statusCode = HTTPStatusCode.Unauthorized;
                                                         break;

                                                     case 181:
                                                         statusCode = HTTPStatusCode.NotFound;
                                                         break;

                                                     default:
                                                         statusCode = HTTPStatusCode.BadRequest;
                                                         break;

                                                 }

                                                 #endregion


                                                 _HTTPResponse = new HTTPResponse.Builder(request) {
                                                                     HTTPStatusCode  = statusCode,
                                                                     Server          = HTTPServer.DefaultServerName,
                                                                     Date            = DateTime.UtcNow,
                                                                     ContentType     = HTTPContentType.JSON_UTF8,
                                                                     Content         = SessionStopResult.ToUTF8Bytes(),
                                                                     Connection      = "close"
                                                                 };


                                                 #region Send OnSessionStopResponse events

                                                 try
                                                 {

                                                     if (OnSessionStopResponse != null)
                                                         await Task.WhenAll(OnSessionStopResponse.GetInvocationList().
                                                                            Cast<OnSessionStopResponseDelegate>().
                                                                            Select(e => e(DateTime.UtcNow,
                                                                                          request.Timestamp,
                                                                                          this,
                                                                                          EventTracking_Id.New,
                                                                                          new User(eMobilityAccountId.ToString(),
                                                                                                   IdentifierType,
                                                                                                   Token),
                                                                                          ConnectorId,
                                                                                          SessionId,
                                                                                          SessionStopResult,
                                                                                          request.Timestamp - DateTime.UtcNow))).
                                                                    ConfigureAwait(false);

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     e.Log(nameof(CPOServer) + "." + nameof(OnSessionStopResponse));
                                                 }

                                                 #endregion

                                                 #region Send OnSessionStopHTTPResponse events

                                                 try
                                                 {

                                                     if (OnSessionStopHTTPResponse != null)
                                                         await Task.WhenAll(OnSessionStopHTTPResponse.GetInvocationList().
                                                                            Cast<AccessLogHandler>().
                                                                            Select(e => e(DateTime.UtcNow,
                                                                                          HTTPServer,
                                                                                          request,
                                                                                          _HTTPResponse))).
                                                                    ConfigureAwait(false);

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     e.Log(nameof(CPOServer) + "." + nameof(OnSessionStopHTTPResponse));
                                                 }

                                                 #endregion

                                             }

                                             #endregion


                                             if (_HTTPResponse == null)
                                                 _HTTPResponse = CreateResponse(request,
                                                                                HTTPStatusCode.BadRequest,
                                                                                Result.Error(140, "Unknown JSON in HTTP body!"));

                                             return _HTTPResponse;

                                         });

        }

        #endregion


        #region (static) AttachToHTTPAPI(HTTPServer, HTTPHostname = null, URLPrefix = DefaultURLPrefix, ...)

        /// <summary>
        /// Create and attach a OIOI CPO Server to the given HTTP API.
        /// </summary>
        /// <param name="HTTPServer">An existing HTTP server.</param>
        /// <param name="HTTPHostname">An optional HTTP hostname.</param>
        /// <param name="URLPrefix">The URI prefix for all incoming HTTP requests.</param>
        public static CPOServer

            AttachToHTTPAPI(HTTPServer     HTTPServer,
                            HTTPHostname?  HTTPHostname   = null,
                            HTTPPath?       URLPrefix      = null)

            => new CPOServer(HTTPServer,
                             HTTPHostname,
                             URLPrefix ?? DefaultURLPrefix);

        #endregion


        #region (private, static) CreateResponse  (HTTPRequest, HTTPStatusCode, OIOIResult)

        /// <summary>
        /// Create a new HTTP Response.
        /// </summary>
        /// <param name="HTTPRequest">The HTTP request.</param>
        /// <param name="HTTPStatusCode">The HTTP status code.</param>
        /// <param name="OIOIResult">The OIOI result.</param>
        private static HTTPResponse CreateResponse(HTTPRequest     HTTPRequest,
                                                   HTTPStatusCode  HTTPStatusCode,
                                                   Result          OIOIResult)

            => new HTTPResponse.Builder(HTTPRequest) {
                   HTTPStatusCode  = HTTPStatusCode,
                   Server          = HTTPRequest.HTTPServer.DefaultServerName,
                   Date            = DateTime.UtcNow,
                   ContentType     = HTTPContentType.JSON_UTF8,
                   Content         = OIOIResult.ToUTF8Bytes(),
                   Connection      = "close"
               };

        #endregion

        #region (private) SendSessionStartResponse(HTTPRequest, HTTPStatusCode, OIOIResult)

        private HTTPResponse SendSessionStartResponse(HTTPRequest     HTTPRequest,
                                                      HTTPStatusCode  HTTPStatusCode,
                                                      Result          OIOIResult)
        {

            var _HTTPResponse = CreateResponse(HTTPRequest,
                                               HTTPStatusCode,
                                               OIOIResult);

            OnSessionStartHTTPResponse?.Invoke(DateTime.UtcNow,
                                               HTTPServer,
                                               HTTPRequest,
                                               _HTTPResponse);

            return _HTTPResponse;

        }

        #endregion

        #region (private) SendSessionStopResponse (HTTPRequest, HTTPStatusCode, OIOIResult)

        private HTTPResponse SendSessionStopResponse(HTTPRequest     HTTPRequest,
                                                     HTTPStatusCode  HTTPStatusCode,
                                                     Result          OIOIResult)
        {

            var _HTTPResponse = CreateResponse(HTTPRequest,
                                               HTTPStatusCode,
                                               OIOIResult);

            OnSessionStopHTTPResponse?.Invoke(DateTime.UtcNow,
                                              HTTPServer,
                                              HTTPRequest,
                                              _HTTPResponse);

            return _HTTPResponse;

        }

        #endregion



        // Start/stop the HTTP server(s)...

        #region Start()

        public void Start()
        {
            HTTPServer.Start();
        }

        #endregion

        #region Start(Delay, InBackground = true)

        public void Start(TimeSpan Delay, Boolean InBackground = true)
        {
            HTTPServer.Start(Delay, InBackground);
        }

        #endregion

        #region Shutdown(Message = null, Wait = true)

        public void Shutdown(String Message = null, Boolean Wait = true)
        {
            HTTPServer.Shutdown(Message, Wait);
        }

        #endregion


        #region Dispose()

        public void Dispose()
        {
            HTTPServer.Dispose();
        }

        #endregion


    }

}
