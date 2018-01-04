/*
 * Copyright (c) 2016-2018 GraphDefined GmbH
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

namespace org.GraphDefined.WWCP.OIOIv4_x.EMP
{

    /// <summary>
    /// An OIOI EMP Client.
    /// </summary>
    public partial class EMPClient : AJSONClient,
                                     IEMPClient
    {

        #region Data

        /// <summary>
        /// The default HTTP user agent string.
        /// </summary>
        public new const           String  DefaultHTTPUserAgent  = "GraphDefined OIOI " + Version.Number + " EMP Client";

        /// <summary>
        /// The default remote TCP port to connect to.
        /// </summary>
        public new static readonly IPPort  DefaultRemotePort     = IPPort.Parse(443);

        /// <summary>
        /// The default HTTP client URI prefix.
        /// </summary>
        public const               String  DefaultURIPrefix      = "/api/v4/request";

        #endregion

        #region Properties

        /// <summary>
        /// The URI prefix for all HTTP requests.
        /// </summary>
        public String           URIPrefix          { get; }

        /// <summary>
        /// The API key for all requests.
        /// </summary>
        public String           APIKey             { get; }

        /// <summary>
        /// The default communication partner identification for all requests.
        /// </summary>
        public Partner_Id       DefaultPartnerId   { get; }

        /// <summary>
        /// The attached OIOI CPO client (HTTP/JSON client) logger.
        /// </summary>
        public EMPClientLogger  Logger             { get; }

        #endregion

        #region Events

        #region OnStationGetSurfaceRequest/-Response

        /// <summary>
        /// An event fired whenever a charging station search request will be send.
        /// </summary>
        public event OnStationGetSurfaceRequestDelegate   OnStationGetSurfaceRequest;

        /// <summary>
        /// An event fired whenever a HTTP request searching for charging stations will be send.
        /// </summary>
        public event ClientRequestLogHandler              OnStationGetSurfaceHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging station search request had been received.
        /// </summary>
        public event ClientResponseLogHandler             OnStationGetSurfaceHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging station search request had been received.
        /// </summary>
        public event OnStationGetSurfaceResponseDelegate  OnStationGetSurfaceResponse;

        #endregion

        #region OnSessionStartRequest/-Response

        /// <summary>
        /// An event fired whenever a charging session start request will be send.
        /// </summary>
        public event OnSessionStartRequestDelegate   OnSessionStartRequest;

        /// <summary>
        /// An event fired whenever a HTTP request starting a charging session will be send.
        /// </summary>
        public event ClientRequestLogHandler         OnSessionStartHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging session start request had been received.
        /// </summary>
        public event ClientResponseLogHandler        OnSessionStartHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging session start request had been received.
        /// </summary>
        public event OnSessionStartResponseDelegate  OnSessionStartResponse;

        #endregion

        #region OnSessionStopRequest/-Response

        /// <summary>
        /// An event fired whenever a charging session stop request will be send.
        /// </summary>
        public event OnSessionStopRequestDelegate   OnSessionStopRequest;

        /// <summary>
        /// An event fired whenever a HTTP request stopping a charging session will be send.
        /// </summary>
        public event ClientRequestLogHandler        OnSessionStopHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging session stop request had been received.
        /// </summary>
        public event ClientResponseLogHandler       OnSessionStopHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging session stop request had been received.
        /// </summary>
        public event OnSessionStopResponseDelegate  OnSessionStopResponse;

        #endregion

        #endregion

        #region Custom request mappers

        #region CustomStationGetSurfaceRequestMapper

        #region CustomStationGetSurfaceRequestMapper

        private Func<StationGetSurfaceRequest, StationGetSurfaceRequest> _CustomStationGetSurfaceRequestMapper = _ => _;

        public Func<StationGetSurfaceRequest, StationGetSurfaceRequest> CustomStationGetSurfaceRequestMapper
        {

            get
            {
                return _CustomStationGetSurfaceRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomStationGetSurfaceRequestMapper = value;
            }

        }

        #endregion

        #region CustomStationGetSurfaceJSONRequestMapper

        private Func<StationGetSurfaceRequest, JObject, JObject> _CustomStationGetSurfaceJSONRequestMapper = (request, xml) => xml;

        public Func<StationGetSurfaceRequest, JObject, JObject> CustomStationGetSurfaceJSONRequestMapper
        {

            get
            {
                return _CustomStationGetSurfaceJSONRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomStationGetSurfaceJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<StationGetSurfaceResponse, StationGetSurfaceResponse.Builder> CustomStationGetSurfaceResponseMapper { get; set; }

        #endregion

        #region CustomSessionStartRequestMapper

        #region CustomSessionStartRequestMapper

        private Func<SessionStartRequest, SessionStartRequest> _CustomSessionStartRequestMapper = _ => _;

        public Func<SessionStartRequest, SessionStartRequest> CustomSessionStartRequestMapper
        {

            get
            {
                return _CustomSessionStartRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomSessionStartRequestMapper = value;
            }

        }

        #endregion

        #region CustomSessionStartJSONRequestMapper

        private Func<SessionStartRequest, JObject, JObject> _CustomSessionStartJSONRequestMapper = (request, xml) => xml;

        public Func<SessionStartRequest, JObject, JObject> CustomSessionStartJSONRequestMapper
        {

            get
            {
                return _CustomSessionStartJSONRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomSessionStartJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<SessionStartResponse, SessionStartResponse.Builder> CustomSessionStartResponseMapper { get; set; }

        #endregion

        #region CustomSessionStopRequestMapper

        #region CustomSessionStopRequestMapper

        private Func<SessionStopRequest, SessionStopRequest> _CustomSessionStopRequestMapper = _ => _;

        public Func<SessionStopRequest, SessionStopRequest> CustomSessionStopRequestMapper
        {

            get
            {
                return _CustomSessionStopRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomSessionStopRequestMapper = value;
            }

        }

        #endregion

        #region CustomSessionStopJSONRequestMapper

        private Func<SessionStopRequest, JObject, JObject> _CustomSessionStopJSONRequestMapper = (request, xml) => xml;

        public Func<SessionStopRequest, JObject, JObject> CustomSessionStopJSONRequestMapper
        {

            get
            {
                return _CustomSessionStopJSONRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomSessionStopJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<SessionStopResponse, SessionStopResponse.Builder> CustomSessionStopResponseMapper { get; set; }

        #endregion

        #endregion

        #region Constructor(s)

        #region EMPClient(ClientId, Hostname, APIKey, ..., LoggingContext = EMPClientLogger.DefaultContext, ...)

        /// <summary>
        /// Create a new OIOI EMP Client using the given parameters.
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
        /// <param name="MaxNumberOfRetries">The default number of maximum transmission retries.</param>
        /// <param name="DNSClient">An optional DNS client to use.</param>
        /// <param name="LoggingContext">An optional context for logging client methods.</param>
        /// <param name="LogFileCreator">A delegate to create a log file from the given context and log file name.</param>
        public EMPClient(String                               ClientId,
                         String                               Hostname,
                         String                               APIKey,
                         IPPort                               RemotePort                   = null,
                         RemoteCertificateValidationCallback  RemoteCertificateValidator   = null,
                         LocalCertificateSelectionCallback    LocalCertificateSelector     = null,
                         X509Certificate                      ClientCert                   = null,
                         String                               HTTPVirtualHost              = null,
                         String                               URIPrefix                    = null,
                         String                               HTTPUserAgent                = DefaultHTTPUserAgent,
                         Partner_Id?                          DefaultPartnerId             = null,
                         TimeSpan?                            RequestTimeout               = null,
                         Byte?                                MaxNumberOfRetries           = DefaultMaxNumberOfRetries,
                         DNSClient                            DNSClient                    = null,
                         String                               LoggingContext               = EMPClientLogger.DefaultContext,
                         LogfileCreatorDelegate               LogFileCreator               = null)

            : base(ClientId,
                   Hostname,
                   RemotePort ?? DefaultRemotePort,
                   RemoteCertificateValidator,
                   LocalCertificateSelector,
                   ClientCert,
                   HTTPVirtualHost,
                   HTTPUserAgent,
                   RequestTimeout,
                   MaxNumberOfRetries,
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

            this.APIKey            = APIKey;
            this.URIPrefix         = URIPrefix.IsNotNullOrEmpty() ? URIPrefix : DefaultURIPrefix;
            this.DefaultPartnerId  = DefaultPartnerId.HasValue ? DefaultPartnerId.Value : Partner_Id.Parse("1");

            this.Logger            = new EMPClientLogger(this,
                                                         LoggingContext,
                                                         LogFileCreator);

        }

        #endregion

        #region EMPClient(ClientId, Logger, Hostname, APIKey, ...)

        /// <summary>
        /// Create a new OIOI EMP Client.
        /// </summary>
        /// <param name="ClientId">A unqiue identification of this client.</param>
        /// <param name="Logger">A CPO client logger.</param>
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
        /// <param name="MaxNumberOfRetries">The default number of maximum transmission retries.</param>
        /// <param name="DNSClient">An optional DNS client to use.</param>
        public EMPClient(String                               ClientId,
                         EMPClientLogger                      Logger,
                         String                               Hostname,
                         String                               APIKey,
                         IPPort                               RemotePort                   = null,
                         String                               URIPrefix                    = null,
                         RemoteCertificateValidationCallback  RemoteCertificateValidator   = null,
                         LocalCertificateSelectionCallback    LocalCertificateSelector     = null,
                         X509Certificate                      ClientCert                   = null,
                         String                               HTTPVirtualHost              = null,
                         String                               HTTPUserAgent                = DefaultHTTPUserAgent,
                         Partner_Id?                          DefaultPartnerId             = null,
                         TimeSpan?                            RequestTimeout               = null,
                         Byte?                                MaxNumberOfRetries           = DefaultMaxNumberOfRetries,
                         DNSClient                            DNSClient                    = null)

            : base(ClientId,
                   Hostname,
                   RemotePort ?? DefaultRemotePort,
                   RemoteCertificateValidator,
                   LocalCertificateSelector,
                   ClientCert,
                   HTTPVirtualHost,
                   HTTPUserAgent,
                   RequestTimeout,
                   MaxNumberOfRetries,
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

            this.Logger            = Logger;
            this.APIKey            = APIKey;
            this.URIPrefix         = URIPrefix.IsNotNullOrEmpty() ? URIPrefix : DefaultURIPrefix;
            this.DefaultPartnerId  = DefaultPartnerId.HasValue ? DefaultPartnerId.Value : Partner_Id.Parse("1");

        }

        #endregion

        #endregion


        #region StationGetSurface(Request, ...)

        /// <summary>
        /// Upload a charging station onto the OIOI server.
        /// </summary>
        /// <param name="Request">A StationGetSurface request.</param>
        public async Task<HTTPResponse<StationGetSurfaceResponse>>

            StationGetSurface(StationGetSurfaceRequest  Request)

        {

            #region Initial checks

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The given StationGetSurface request must not be null!");

            Request = _CustomStationGetSurfaceRequestMapper(Request);

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The mapped StationGetSurface request must not be null!");


            HTTPResponse<StationGetSurfaceResponse> result = null;

            #endregion

            #region Send OnStationGetSurfaceRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnStationGetSurfaceRequest?.Invoke(StartTime,
                                                   Request.Timestamp.Value,
                                                   this,
                                                   ClientId,
                                                   Request.EventTrackingId,
                                                   Request.MinLat,
                                                   Request.MaxLat,
                                                   Request.MinLong,
                                                   Request.MaxLong,
                                                   Request.IncludeConnectorTypes,
                                                   Request.RequestTimeout.HasValue ? Request.RequestTimeout : RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(EMPClient) + "." + nameof(OnStationGetSurfaceRequest));
            }

            #endregion


            using (var _JSONClient = new JSONClient(Hostname,
                                                    RemotePort,
                                                    HTTPVirtualHost,
                                                    URIPrefix,
                                                    RemoteCertificateValidator,
                                                    ClientCertificateSelector,
                                                    ClientCert,
                                                    UserAgent,
                                                    RequestTimeout,
                                                    DNSClient))
            {

                result = await _JSONClient.Query(_CustomStationGetSurfaceJSONRequestMapper(Request,
                                                                                           Request.ToJSON()),
                                                 HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                 RequestLogDelegate:   OnStationGetSurfaceHTTPRequest,
                                                 ResponseLogDelegate:  OnStationGetSurfaceHTTPResponse,
                                                 CancellationToken:    Request.CancellationToken,
                                                 EventTrackingId:      Request.EventTrackingId,
                                                 RequestTimeout:       Request.RequestTimeout.HasValue ? Request.RequestTimeout : RequestTimeout,

                                                 #region OnSuccess

                                                 OnSuccess: JSONResponse => JSONResponse.ConvertContent(Request,
                                                                                                        (request, json, onexception) =>
                                                                                                            StationGetSurfaceResponse.Parse(request,
                                                                                                                                            json,
                                                                                                                                            CustomStationGetSurfaceResponseMapper,
                                                                                                                                            onexception)),

                                                 #endregion

                                                 #region OnJSONFault

                                                 OnJSONFault: (timestamp, jsonclient, httpresponse) => {

                                                     SendJSONError(timestamp, this, httpresponse.Content);

                                                     return new HTTPResponse<StationGetSurfaceResponse>(httpresponse,
                                                                                                        new StationGetSurfaceResponse(Request,
                                                                                                                                      ResponseCodes.SystemError,
                                                                                                                                      "Invalid JSON response!",
                                                                                                                                      new Station[0]),
                                                                                                        IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnHTTPError

                                                 OnHTTPError: (timestamp, soapclient, httpresponse) => {

                                                     SendHTTPError(timestamp, this, httpresponse);

                                                     return new HTTPResponse<StationGetSurfaceResponse>(httpresponse,
                                                                                                        new StationGetSurfaceResponse(Request,
                                                                                                                                      ResponseCodes.SystemError,
                                                                                                                                      "Invalid HTTP response!",
                                                                                                                                      new Station[0]),
                                                                                                        IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnException

                                                 OnException: (timestamp, sender, exception) => {

                                                     SendException(timestamp, sender, exception);

                                                     return HTTPResponse<StationGetSurfaceResponse>.ExceptionThrown(new StationGetSurfaceResponse(Request,
                                                                                                                                                  ResponseCodes.SystemError,
                                                                                                                                                  "Exception occured!",
                                                                                                                                                  new Station[0]),
                                                                                                                    Exception:  exception);

                                                 }

                                                 #endregion

                                                );

            }


            if (result == null)
                result = HTTPResponse<StationGetSurfaceResponse>.OK(new StationGetSurfaceResponse(Request,
                                                                                                  ResponseCodes.SystemError,
                                                                                                  "Invalid response!",
                                                                                                  new Station[0]));

            #region Send OnStationGetSurfaceResponse event

            var Endtime = DateTime.UtcNow;

            try
            {

                OnStationGetSurfaceResponse?.Invoke(Endtime,
                                                    Request.Timestamp.Value,
                                                    this,
                                                    ClientId,
                                                    Request.EventTrackingId,
                                                    Request.MinLat,
                                                    Request.MaxLat,
                                                    Request.MinLong,
                                                    Request.MaxLong,
                                                    Request.IncludeConnectorTypes,
                                                    Request.RequestTimeout.HasValue ? Request.RequestTimeout : RequestTimeout,
                                                    result.Content,
                                                    Endtime - StartTime);

            }
            catch (Exception e)
            {
                e.Log(nameof(EMPClient) + "." + nameof(OnStationGetSurfaceResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region SessionStart(Request, ...)

        /// <summary>
        /// Upload a charging station onto the OIOI server.
        /// </summary>
        /// <param name="Request">A SessionStart request.</param>
        public async Task<HTTPResponse<SessionStartResponse>>

            SessionStart(SessionStartRequest  Request)

        {

            #region Initial checks

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The given SessionStart request must not be null!");

            Request = _CustomSessionStartRequestMapper(Request);

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The mapped SessionStart request must not be null!");


            HTTPResponse<SessionStartResponse> result = null;

            #endregion

            #region Send OnSessionStartRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnSessionStartRequest?.Invoke(StartTime,
                                              Request.Timestamp.Value,
                                              this,
                                              ClientId,
                                              Request.EventTrackingId,
                                              Request.User,
                                              Request.ConnectorId,
                                              Request.PaymentReference,
                                              Request.RequestTimeout.HasValue ? Request.RequestTimeout : RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(EMPClient) + "." + nameof(OnSessionStartRequest));
            }

            #endregion


            using (var _JSONClient = new JSONClient(Hostname,
                                                    RemotePort,
                                                    HTTPVirtualHost,
                                                    URIPrefix,
                                                    RemoteCertificateValidator,
                                                    ClientCertificateSelector,
                                                    ClientCert,
                                                    UserAgent,
                                                    RequestTimeout,
                                                    DNSClient))
            {

                result = await _JSONClient.Query(_CustomSessionStartJSONRequestMapper(Request,
                                                                                      Request.ToJSON()),
                                                 HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                 RequestLogDelegate:   OnSessionStartHTTPRequest,
                                                 ResponseLogDelegate:  OnSessionStartHTTPResponse,
                                                 CancellationToken:    Request.CancellationToken,
                                                 EventTrackingId:      Request.EventTrackingId,
                                                 RequestTimeout:       Request.RequestTimeout.HasValue ? Request.RequestTimeout : RequestTimeout,

                                                 #region OnSuccess

                                                 OnSuccess: JSONResponse => JSONResponse.ConvertContent(Request,
                                                                                                        (request, json, onexception) =>
                                                                                                            SessionStartResponse.Parse(request,
                                                                                                                                       json,
                                                                                                                                       CustomSessionStartResponseMapper,
                                                                                                                                       onexception)),

                                                 #endregion

                                                 #region OnJSONFault

                                                 OnJSONFault: (timestamp, jsonclient, httpresponse) => {

                                                     SendJSONError(timestamp, this, httpresponse.Content);

                                                     return new HTTPResponse<SessionStartResponse>(httpresponse,
                                                                                                   new SessionStartResponse(Request,
                                                                                                                            ResponseCodes.SystemError,
                                                                                                                            "Invalid JSON response!"),
                                                                                                   IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnHTTPError

                                                 OnHTTPError: (timestamp, soapclient, httpresponse) => {

                                                     SendHTTPError(timestamp, this, httpresponse);

                                                     return new HTTPResponse<SessionStartResponse>(httpresponse,
                                                                                                   new SessionStartResponse(Request,
                                                                                                                            ResponseCodes.SystemError,
                                                                                                                            "Invalid HTTP response!"),
                                                                                                   IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnException

                                                 OnException: (timestamp, sender, exception) => {

                                                     SendException(timestamp, sender, exception);

                                                     return HTTPResponse<SessionStartResponse>.ExceptionThrown(new SessionStartResponse(Request,
                                                                                                                                        ResponseCodes.SystemError,
                                                                                                                                        "Exception occured!"),
                                                                                                               Exception:  exception);

                                                 }

                                                 #endregion

                                                );

            }


            if (result == null)
                result = HTTPResponse<SessionStartResponse>.OK(new SessionStartResponse(Request,
                                                                                        ResponseCodes.SystemError,
                                                                                        "Invalid response!"));

            #region Send OnSessionStartResponse event

            var Endtime = DateTime.UtcNow;

            try
            {

                OnSessionStartResponse?.Invoke(Endtime,
                                               Request.Timestamp.Value,
                                               this,
                                               ClientId,
                                               Request.EventTrackingId,
                                               Request.User,
                                               Request.ConnectorId,
                                               Request.PaymentReference,
                                               Request.RequestTimeout.HasValue ? Request.RequestTimeout : RequestTimeout,
                                               result.Content,
                                               Endtime - StartTime);

            }
            catch (Exception e)
            {
                e.Log(nameof(EMPClient) + "." + nameof(OnSessionStartResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region SessionStop(Request, ...)

        /// <summary>
        /// Upload a charging station onto the OIOI server.
        /// </summary>
        /// <param name="Request">A SessionStop request.</param>
        public async Task<HTTPResponse<SessionStopResponse>>

            SessionStop(SessionStopRequest  Request)

        {

            #region Initial checks

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The given SessionStop request must not be null!");

            Request = _CustomSessionStopRequestMapper(Request);

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The mapped SessionStop request must not be null!");


            HTTPResponse<SessionStopResponse> result = null;

            #endregion

            #region Send OnSessionStopRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnSessionStopRequest?.Invoke(StartTime,
                                             Request.Timestamp.Value,
                                             this,
                                             ClientId,
                                             Request.EventTrackingId,
                                             Request.User,
                                             Request.ConnectorId,
                                             Request.SessionId,
                                             Request.RequestTimeout.HasValue ? Request.RequestTimeout : RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(EMPClient) + "." + nameof(OnSessionStopRequest));
            }

            #endregion


            using (var _JSONClient = new JSONClient(Hostname,
                                                    RemotePort,
                                                    HTTPVirtualHost,
                                                    URIPrefix,
                                                    RemoteCertificateValidator,
                                                    ClientCertificateSelector,
                                                    ClientCert,
                                                    UserAgent,
                                                    RequestTimeout,
                                                    DNSClient))
            {

                result = await _JSONClient.Query(_CustomSessionStopJSONRequestMapper(Request,
                                                                                     Request.ToJSON()),
                                                 HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                 RequestLogDelegate:   OnSessionStopHTTPRequest,
                                                 ResponseLogDelegate:  OnSessionStopHTTPResponse,
                                                 CancellationToken:    Request.CancellationToken,
                                                 EventTrackingId:      Request.EventTrackingId,
                                                 RequestTimeout:       Request.RequestTimeout.HasValue ? Request.RequestTimeout : RequestTimeout,

                                                 #region OnSuccess

                                                 OnSuccess: JSONResponse => JSONResponse.ConvertContent(Request,
                                                                                                        (request, json, onexception) =>
                                                                                                            SessionStopResponse.Parse(request,
                                                                                                                                      json,
                                                                                                                                      CustomSessionStopResponseMapper,
                                                                                                                                      onexception)),

                                                 #endregion

                                                 #region OnJSONFault

                                                 OnJSONFault: (timestamp, jsonclient, httpresponse) => {

                                                     SendJSONError(timestamp, this, httpresponse.Content);

                                                     return new HTTPResponse<SessionStopResponse>(httpresponse,
                                                                                                  new SessionStopResponse(Request,
                                                                                                                          ResponseCodes.SystemError,
                                                                                                                          "Invalid JSON response!"),
                                                                                                  IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnHTTPError

                                                 OnHTTPError: (timestamp, soapclient, httpresponse) => {

                                                     SendHTTPError(timestamp, this, httpresponse);

                                                     return new HTTPResponse<SessionStopResponse>(httpresponse,
                                                                                                  new SessionStopResponse(Request,
                                                                                                                          ResponseCodes.SystemError,
                                                                                                                          "Invalid HTTP response!"),
                                                                                                  IsFault: true);

                                                 },

                                                 #endregion

                                                 #region OnException

                                                 OnException: (timestamp, sender, exception) => {

                                                     SendException(timestamp, sender, exception);

                                                     return HTTPResponse<SessionStopResponse>.ExceptionThrown(new SessionStopResponse(Request,
                                                                                                                                      ResponseCodes.SystemError,
                                                                                                                                      "Exception occured!"),
                                                                                                              Exception:  exception);

                                                 }

                                                 #endregion

                                                );

            }


            if (result == null)
                result = HTTPResponse<SessionStopResponse>.OK(new SessionStopResponse(Request,
                                                                                      ResponseCodes.SystemError,
                                                                                      "Invalid response!"));

            #region Send OnSessionStopResponse event

            var Endtime = DateTime.UtcNow;

            try
            {

                OnSessionStopResponse?.Invoke(Endtime,
                                              Request.Timestamp.Value,
                                              this,
                                              ClientId,
                                              Request.EventTrackingId,
                                              Request.User,
                                              Request.ConnectorId,
                                              Request.SessionId,
                                              Request.RequestTimeout.HasValue ? Request.RequestTimeout : RequestTimeout,
                                              result.Content,
                                              Endtime - StartTime);

            }
            catch (Exception e)
            {
                e.Log(nameof(EMPClient) + "." + nameof(OnSessionStopResponse));
            }

            #endregion

            return result;

        }

        #endregion


    }

}
