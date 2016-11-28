/*
 * Copyright (c) 2016 GraphDefined GmbH
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
using System.Net.Security;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.JSON;
using org.GraphDefined.Vanaheimr.Hermod.SOAP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    /// <summary>
    /// An OIOI CPO Client.
    /// </summary>
    public partial class CPOClient : AJSONClient
    {

        #region Data

        /// <summary>
        /// The default HTTP user agent string.
        /// </summary>
        public new const           String  DefaultHTTPUserAgent  = "GraphDefined OIOI " + Version.Number + " CPO Client";

        /// <summary>
        /// The default remote TCP port to connect to.
        /// </summary>
        public new static readonly IPPort  DefaultRemotePort     = IPPort.Parse(443);

        /// <summary>
        /// The default HTTP client URI prefix.
        /// </summary>
        public const               String  DefaultURIPrefix      = "/api/v3/request";

        #endregion

        #region Properties

        /// <summary>
        /// The URI prefix for all HTTP requests.
        /// </summary>
        public String                                       URIPrefix                    { get; }

        /// <summary>
        /// The API key for all requests.
        /// </summary>
        public String                                       APIKey                       { get; }

        /// <summary>
        /// The default communication partner identification for all requests.
        /// </summary>
        public Partner_Id                                   DefaultPartnerId             { get; }

        /// <summary>
        /// The attached OIOI CPO client (HTTP/JSON client) logger.
        /// </summary>
        public CPOClientLogger                              Logger                       { get; }

        public RoamingNetwork                               RoamingNetwork               { get; }

        public ChargingStationOperatorNameSelectorDelegate  DefaultOperatorNameSelector  { get; }

        #endregion

        #region Events

        #region OnStationPostRequest/-Response

        /// <summary>
        /// An event fired whenever a request posting a charging station will be send.
        /// </summary>
        public event OnStationPostRequestDelegate   OnStationPostRequest;

        /// <summary>
        /// An event fired whenever a HTTP request posting a charging station will be send.
        /// </summary>
        public event ClientRequestLogHandler        OnStationPostHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging station post request had been received.
        /// </summary>
        public event ClientResponseLogHandler       OnStationPostHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging station post request had been received.
        /// </summary>
        public event OnStationPostResponseDelegate  OnStationPostResponse;

        #endregion

        #region OnConnectorPostStatustRequest/-Response

        /// <summary>
        /// An event fired whenever a request posting a charging connector status will be send.
        /// </summary>
        public event OnConnectorPostStatusRequestDelegate   OnConnectorPostStatusRequest;

        /// <summary>
        /// An event fired whenever a HTTP request posting a charging connector status will be send.
        /// </summary>
        public event ClientRequestLogHandler                OnConnectorPostStatusHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging connector status post HTTP request had been received.
        /// </summary>
        public event ClientResponseLogHandler               OnConnectorPostStatusHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging connector status post HTTP request had been received.
        /// </summary>
        public event OnConnectorPostStatusResponseDelegate  OnConnectorPostStatusResponse;

        #endregion

        #region OnRFIDVerifyRequest/-Response

        /// <summary>
        /// An event fired whenever a request verifying a RFID identification will be send.
        /// </summary>
        public event OnRFIDVerifyRequestDelegate   OnRFIDVerifyRequest;

        /// <summary>
        /// An event fired whenever a HTTP request verifying a RFID identification will be send.
        /// </summary>
        public event ClientRequestLogHandler       OnRFIDVerifyHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a RFID identification verification request had been received.
        /// </summary>
        public event ClientResponseLogHandler      OnRFIDVerifyHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a RFID identification verification request had been received.
        /// </summary>
        public event OnRFIDVerifyResponseDelegate  OnRFIDVerifyResponse;

        #endregion

        #endregion

        #region Constructor(s)

        #region CPOClient(ClientId, Hostname, APIKey, ..., LoggingContext = CPOClientLogger.DefaultContext, ...)

        /// <summary>
        /// Create a new OIOI CPO Client using the given parameters.
        /// </summary>
        /// <param name="ClientId">A unqiue identification of this client.</param>
        /// <param name="Hostname">The hostname of the remote OIOI service.</param>
        /// <param name="APIKey">The PlugSurfing API key.</param>
        /// <param name="RemotePort">An optional TCP port of the remote OIOI service.</param>
        /// <param name="RemoteCertificateValidator">A delegate to verify the remote TLS certificate.</param>
        /// <param name="ClientCert">The TLS client certificate to use.</param>
        /// <param name="HTTPVirtualHost">An optional HTTP virtual hostname of the remote OIOI service.</param>
        /// <param name="HTTPUserAgent">An optional HTTP user agent identification string for this HTTP client.</param>
        /// <param name="URIPrefix">The default URI prefix.</param>
        /// <param name="DefaultPartnerId">The default communication partner identification.</param>
        /// <param name="RequestTimeout">An optional timeout for upstream queries.</param>
        /// <param name="DNSClient">An optional DNS client to use.</param>
        /// <param name="LoggingContext">An optional context for logging client methods.</param>
        /// <param name="LogFileCreator">A delegate to create a log file from the given context and log file name.</param>
        public CPOClient(String                               ClientId,
                         String                               Hostname,
                         String                               APIKey,
                         IPPort                               RemotePort                  = null,
                         RemoteCertificateValidationCallback  RemoteCertificateValidator  = null,
                         X509Certificate                      ClientCert                  = null,
                         String                               HTTPVirtualHost             = null,
                         String                               HTTPUserAgent               = DefaultHTTPUserAgent,
                         String                               URIPrefix                   = null,
                         Partner_Id                           DefaultPartnerId            = null,
                         TimeSpan?                            RequestTimeout              = null,
                         DNSClient                            DNSClient                   = null,
                         String                               LoggingContext              = CPOClientLogger.DefaultContext,
                         Func<String, String, String>         LogFileCreator              = null)

            : base(ClientId,
                   Hostname,
                   RemotePort ?? DefaultRemotePort,
                   RemoteCertificateValidator,
                   ClientCert,
                   HTTPVirtualHost,
                   HTTPUserAgent,
                   RequestTimeout,
                   DNSClient)

        {

            #region Initial checks

            if (ClientId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Logger),    "The given client identification must not be null or empty!");

            if (Hostname.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Hostname),  "The given hostname must not be null or empty!");

            if (APIKey.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(APIKey),    "The given API key must not be null or empty!");

            #endregion

            this.APIKey                       = APIKey;
            this.URIPrefix                    = URIPrefix.IsNotNullOrEmpty() ? URIPrefix : DefaultURIPrefix;
            this.DefaultPartnerId             = DefaultPartnerId;

            this.Logger                       = new CPOClientLogger(this,
                                                                    LoggingContext,
                                                                    LogFileCreator);

            this.DefaultOperatorNameSelector  = I18N => I18N.FirstText;

        }

        #endregion

        #region CPOClient(ClientId, Logger, Hostname, APIKey, ...)

        /// <summary>
        /// Create a new OIOI CPO Client.
        /// </summary>
        /// <param name="ClientId">A unqiue identification of this client.</param>
        /// <param name="Hostname">The hostname of the remote OIOI service.</param>
        /// <param name="APIKey">The PlugSurfing API key.</param>
        /// <param name="RemotePort">An optional TCP port of the remote OIOI service.</param>
        /// <param name="URIPrefix">The default URI prefix.</param>
        /// <param name="RemoteCertificateValidator">A delegate to verify the remote TLS certificate.</param>
        /// <param name="ClientCert">The TLS client certificate to use.</param>
        /// <param name="HTTPVirtualHost">An optional HTTP virtual hostname of the remote OIOI service.</param>
        /// <param name="HTTPUserAgent">An optional HTTP user agent identification string for this HTTP client.</param>
        /// <param name="DefaultPartnerId">The default communication partner identification.</param>
        /// <param name="RequestTimeout">An optional timeout for upstream queries.</param>
        /// <param name="DNSClient">An optional DNS client to use.</param>
        public CPOClient(String                               ClientId,
                         CPOClientLogger                      Logger,
                         String                               Hostname,
                         String                               APIKey,
                         IPPort                               RemotePort                  = null,
                         String                               URIPrefix                   = null,
                         RemoteCertificateValidationCallback  RemoteCertificateValidator  = null,
                         X509Certificate                      ClientCert                  = null,
                         String                               HTTPVirtualHost             = null,
                         String                               HTTPUserAgent               = DefaultHTTPUserAgent,
                         Partner_Id                           DefaultPartnerId            = null,
                         TimeSpan?                            RequestTimeout              = null,
                         DNSClient                            DNSClient                   = null)

            : base(ClientId,
                   Hostname,
                   RemotePort ?? DefaultRemotePort,
                   RemoteCertificateValidator,
                   ClientCert,
                   HTTPVirtualHost,
                   HTTPUserAgent,
                   RequestTimeout,
                   DNSClient)

        {

            #region Initial checks

            if (ClientId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Logger),    "The given client identification must not be null or empty!");

            if (Logger == null)
                throw new ArgumentNullException(nameof(Logger),    "The given mobile client logger must not be null!");

            if (Hostname.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Hostname),  "The given hostname must not be null or empty!");

            if (APIKey.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(APIKey),    "The given API key must not be null or empty!");

            #endregion

            this.Logger                       = Logger;
            this.APIKey                       = APIKey;
            this.URIPrefix                    = URIPrefix.IsNotNullOrEmpty() ? URIPrefix : "/api/v3/request";
            this.DefaultPartnerId             = DefaultPartnerId;

            this.DefaultOperatorNameSelector  = I18N => I18N.FirstText;

        }

        #endregion

        #endregion


        #region StationPost(Station, PartnerId, ...)

        /// <summary>
        /// Create a new task posting a charging station onto the OIOI server.
        /// </summary>
        /// <param name="Station">A charging station.</param>
        /// <param name="PartnerId">An optional communication partner identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<HTTPResponse<Result>>

            StationPost(Station             Station,
                        Partner_Id          PartnerId          = null,

                        DateTime?           Timestamp          = null,
                        CancellationToken?  CancellationToken  = null,
                        EventTracking_Id    EventTrackingId    = null,
                        TimeSpan?           RequestTimeout     = null)

        {

            #region Initial checks

            if (Station == null)
                throw new ArgumentNullException(nameof(Station),    "The given charging station must not be null!");

            if (PartnerId == null)
                PartnerId = DefaultPartnerId;

            if (PartnerId == null)
                throw new ArgumentNullException(nameof(PartnerId),  "The given communication partner identification must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.Now;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = this.RequestTimeout;

            HTTPResponse<Result> result = null;

            #endregion

            #region Send OnStationPostRequest event

            try
            {

                OnStationPostRequest?.Invoke(DateTime.Now,
                                             Timestamp.Value,
                                             this,
                                             ClientId,
                                             EventTrackingId,
                                             Station,
                                             PartnerId,
                                             RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(CPOClient) + "." + nameof(OnStationPostRequest));
            }

            #endregion


            using (var _JSONClient = new JSONClient(Hostname,
                                                    RemotePort,
                                                    HTTPVirtualHost,
                                                    URIPrefix,
                                                    RemoteCertificateValidator,
                                                    ClientCert,
                                                    UserAgent,
                                                    DNSClient))
            {

                result = await _JSONClient.Query(new JObject(
                                                     new JProperty("station-post", new JObject(
                                                         new JProperty("station",             Station.  ToJSON()),
                                                         new JProperty("partner-identifier",  PartnerId.ToString())
                                                     ))
                                                 ),
                                                 HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                 RequestLogDelegate:   OnStationPostHTTPRequest,
                                                 ResponseLogDelegate:  OnStationPostHTTPResponse,
                                                 CancellationToken:    CancellationToken,
                                                 EventTrackingId:      EventTrackingId,
                                                 RequestTimeout:       RequestTimeout,

                                                 #region OnSuccess

                                                 OnSuccess: JSONResponse => JSONResponse.ConvertContent(jsonobject => Result.Parse(jsonobject)),

                                                 #endregion

                                                 #region OnJSONFault

                                                 OnJSONFault: (timestamp, jsonclient, httpresponse) => {

                                                     SendJSONError(timestamp, this, httpresponse.Content);

                                                     return new HTTPResponse<Result>(httpresponse,
                                                                                     Result.Error(500, "JSON Fault!"),
                                                                                     IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnHTTPError

                                                 OnHTTPError: (timestamp, soapclient, httpresponse) => {

                                                     SendHTTPError(timestamp, this, httpresponse);

                                                     return new HTTPResponse<Result>(httpresponse,
                                                                                     Result.Error(httpresponse.HTTPStatusCode.Code,
                                                                                                  httpresponse.HTTPBody.      ToUTF8String()),
                                                                                     IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnException

                                                 OnException: (timestamp, sender, exception) => {

                                                     SendException(timestamp, sender, exception);

                                                     return HTTPResponse<Result>.ExceptionThrown(Result.Error(500,
                                                                                                            exception.Message),
                                                                                                 Exception:  exception);

                                                 }

                                                 #endregion

                                                );

            }


            if (result == null)
                result = HTTPResponse<Result>.OK(Result.Error(500, "result == null!"));

            #region Send OnStationPostResponse event

            try
            {

                OnStationPostResponse?.Invoke(DateTime.Now,
                                              Timestamp.Value,
                                              this,
                                              ClientId,
                                              EventTrackingId,
                                              Station,
                                              PartnerId,
                                              RequestTimeout,
                                              result.Content,
                                              DateTime.Now - Timestamp.Value);

            }
            catch (Exception e)
            {
                e.Log(nameof(CPOClient) + "." + nameof(OnStationPostResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region ConnectorPostStatus(ConnectorId, Status, PartnerId, ...)

        /// <summary>
        /// Create a new task posting the status of a charging connector onto the OIOI server.
        /// </summary>
        /// <param name="ConnectorId">A charging connector identification.</param>
        /// <param name="Status">The current status of the connector.</param>
        /// <param name="PartnerId">An optional communication partner identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<HTTPResponse<Result>>

            ConnectorPostStatus(EVSE_Id              ConnectorId,
                                ConnectorStatusType  Status,
                                Partner_Id           PartnerId          = null,

                                DateTime?            Timestamp          = null,
                                CancellationToken?   CancellationToken  = null,
                                EventTracking_Id     EventTrackingId    = null,
                                TimeSpan?            RequestTimeout     = null)

        {

            #region Initial checks

            if (ConnectorId == null)
                throw new ArgumentNullException(nameof(ConnectorId),  "The given charging station must not be null!");

            if (PartnerId == null)
                PartnerId = DefaultPartnerId;

            if (PartnerId == null)
                throw new ArgumentNullException(nameof(PartnerId),    "The given communication partner identification must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.Now;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = this.RequestTimeout;

            HTTPResponse<Result> result = null;

            #endregion

            #region Send OnConnectorPostStatusRequest event

            try
            {

                OnConnectorPostStatusRequest?.Invoke(DateTime.Now,
                                                     Timestamp.Value,
                                                     this,
                                                     ClientId,
                                                     EventTrackingId,
                                                     ConnectorId,
                                                     Status,
                                                     PartnerId,
                                                     RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(CPOClient) + "." + nameof(OnConnectorPostStatusRequest));
            }

            #endregion


            using (var _JSONClient = new JSONClient(Hostname,
                                                    RemotePort,
                                                    HTTPVirtualHost,
                                                    URIPrefix,
                                                    RemoteCertificateValidator,
                                                    ClientCert,
                                                    UserAgent,
                                                    DNSClient))
            {

                result = await _JSONClient.Query(new JObject(
                                                     new JProperty("connector-post-status", new JObject(
                                                         new JProperty("connector-id",        ConnectorId.ToString()),
                                                         new JProperty("partner-identifier",  PartnerId.ToString()),
                                                         new JProperty("status",              Status.ToString())
                                                     ))
                                                 ),
                                                 HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                 RequestLogDelegate:   OnStationPostHTTPRequest,
                                                 ResponseLogDelegate:  OnStationPostHTTPResponse,
                                                 CancellationToken:    CancellationToken,
                                                 EventTrackingId:      EventTrackingId,
                                                 RequestTimeout:       RequestTimeout,

                                                 #region OnSuccess

                                                 OnSuccess: JSONResponse => JSONResponse.ConvertContent(jsonobject => Result.Parse(jsonobject)),

                                                 #endregion

                                                 #region OnJSONFault

                                                 OnJSONFault: (timestamp, jsonclient, httpresponse) => {

                                                     SendJSONError(timestamp, this, httpresponse.Content);

                                                     return new HTTPResponse<Result>(httpresponse,
                                                                                     Result.Error(500, "JSON Fault!"),
                                                                                     IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnHTTPError

                                                 OnHTTPError: (timestamp, soapclient, httpresponse) => {

                                                     SendHTTPError(timestamp, this, httpresponse);

                                                     return new HTTPResponse<Result>(httpresponse,
                                                                                     Result.Error(httpresponse.HTTPStatusCode.Code,
                                                                                                  httpresponse.HTTPBody.      ToUTF8String()),
                                                                                     IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnException

                                                 OnException: (timestamp, sender, exception) => {

                                                     SendException(timestamp, sender, exception);

                                                     return HTTPResponse<Result>.ExceptionThrown(Result.Error(500,
                                                                                                            exception.Message),
                                                                                                 Exception:  exception);

                                                 }

                                                 #endregion

                                                );

            }


            if (result == null)
                result = HTTPResponse<Result>.OK(Result.Error(500, "result == null!"));

            #region Send OnConnectorPostStatusResponse event

            try
            {

                OnConnectorPostStatusResponse?.Invoke(DateTime.Now,
                                                      Timestamp.Value,
                                                      this,
                                                      ClientId,
                                                      EventTrackingId,
                                                      ConnectorId,
                                                      Status,
                                                      PartnerId,
                                                      RequestTimeout,
                                                      result.Content,
                                                      DateTime.Now - Timestamp.Value);

            }
            catch (Exception e)
            {
                e.Log(nameof(CPOClient) + "." + nameof(OnConnectorPostStatusResponse));
            }

            #endregion

            return result;

        }

        #endregion


        #region RFIDVerify(RFIDId, ...)

        /// <summary>
        /// Create a new task verifying a RFID identification via the OIOI server.
        /// </summary>
        /// <param name="RFIDId">A RFID identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<HTTPResponse<Result>>

            RFIDVerify(Auth_Token          RFIDId,

                       DateTime?           Timestamp          = null,
                       CancellationToken?  CancellationToken  = null,
                       EventTracking_Id    EventTrackingId    = null,
                       TimeSpan?           RequestTimeout     = null)

        {

            #region Initial checks

            if (RFIDId == null)
                throw new ArgumentNullException(nameof(RFIDId),  "The given RFID identification must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.Now;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = this.RequestTimeout;

            HTTPResponse<Result> result = null;

            #endregion

            #region Send OnRFIDVerifyRequest event

            try
            {

                OnRFIDVerifyRequest?.Invoke(DateTime.Now,
                                            Timestamp.Value,
                                            this,
                                            ClientId,
                                            EventTrackingId,
                                            RFIDId,
                                            RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(CPOClient) + "." + nameof(OnRFIDVerifyRequest));
            }

            #endregion


            using (var _JSONClient = new JSONClient(Hostname,
                                                    RemotePort,
                                                    HTTPVirtualHost,
                                                    URIPrefix,
                                                    RemoteCertificateValidator,
                                                    ClientCert,
                                                    UserAgent,
                                                    DNSClient))
            {

                result = await _JSONClient.Query(new JObject(
                                                     new JProperty("rfid-verify", new JObject(
                                                         new JProperty("rfid", RFIDId.ToString())
                                                     ))
                                                 ),
                                                 HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                 RequestLogDelegate:   OnRFIDVerifyHTTPRequest,
                                                 ResponseLogDelegate:  OnRFIDVerifyHTTPResponse,
                                                 CancellationToken:    CancellationToken,
                                                 EventTrackingId:      EventTrackingId,
                                                 RequestTimeout:       RequestTimeout,

                                                 #region OnSuccess

                                                 OnSuccess: JSONResponse => JSONResponse.ConvertContent(jsonobject => Result.Parse(jsonobject)),

                                                 #endregion

                                                 #region OnJSONFault

                                                 OnJSONFault: (timestamp, jsonclient, httpresponse) => {

                                                     SendJSONError(timestamp, this, httpresponse.Content);

                                                     return new HTTPResponse<Result>(httpresponse,
                                                                                     Result.Error(500, "JSON Fault!"),
                                                                                     IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnHTTPError

                                                 OnHTTPError: (timestamp, soapclient, httpresponse) => {

                                                     SendHTTPError(timestamp, this, httpresponse);

                                                     return new HTTPResponse<Result>(httpresponse,
                                                                                     Result.Error(httpresponse.HTTPStatusCode.Code,
                                                                                                  httpresponse.HTTPBody.      ToUTF8String()),
                                                                                     IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnException

                                                 OnException: (timestamp, sender, exception) => {

                                                     SendException(timestamp, sender, exception);

                                                     return HTTPResponse<Result>.ExceptionThrown(Result.Error(500,
                                                                                                            exception.Message),
                                                                                                 Exception:  exception);

                                                 }

                                                 #endregion

                                                );

            }


            if (result == null)
                result = HTTPResponse<Result>.OK(Result.Error(500, "result == null!"));

            #region Send OnRFIDVerifyResponse event

            try
            {

                OnRFIDVerifyResponse?.Invoke(DateTime.Now,
                                                      Timestamp.Value,
                                                      this,
                                                      ClientId,
                                                      EventTrackingId,
                                                      RFIDId,
                                                      RequestTimeout,
                                                      result.Content,
                                                      DateTime.Now - Timestamp.Value);

            }
            catch (Exception e)
            {
                e.Log(nameof(CPOClient) + "." + nameof(OnRFIDVerifyResponse));
            }

            #endregion

            return result;

        }

        #endregion


        // SessionPost

    }

}
