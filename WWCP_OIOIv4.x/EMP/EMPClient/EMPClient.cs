/*
 * Copyright (c) 2016-2022 GraphDefined GmbH
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
    /// The OIOI EMP client.
    /// </summary>
    public partial class EMPClient : AJSONClient,
                                     IEMPClient
    {

        #region Data

        /// <summary>
        /// The default HTTP user agent string.
        /// </summary>
        public new const       String      DefaultHTTPUserAgent     = "GraphDefined OIOI " + Version.Number + " EMP Client";

        /// <summary>
        /// The default HTTP client remote URL.
        /// </summary>
        public static readonly URL         DefaultRemoteURL         = URL.Parse("https://api.plugsurfing.com/api/v4/request");


        public static readonly Partner_Id  DefaultDefaultPartnerId  = Partner_Id.Parse("1");

        #endregion

        #region Properties

        /// <summary>
        /// The API key for all requests.
        /// </summary>
        public APIKey      APIKey              { get; }

        /// <summary>
        /// The default communication partner identification for all requests.
        /// </summary>
        public Partner_Id  DefaultPartnerId    { get; }

        /// <summary>
        /// The attached HTTP client logger.
        /// </summary>
        public new Logger HTTPLogger
        {
            get
            {
                return base.HTTPLogger as Logger;
            }
            set
            {
                base.HTTPLogger = value;
            }
        }

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

        /// <summary>
        /// Create a new EMP client.
        /// </summary>
        /// <param name="RemoteURL">The remote URL of the HTTP endpoint to connect to.</param>
        /// <param name="APIKey">The PlugSurfing API key.</param>
        /// <param name="VirtualHostname">An optional HTTP virtual hostname.</param>
        /// <param name="Description">An optional description of this EMP client.</param>
        /// <param name="RemoteCertificateValidator">The remote SSL/TLS certificate validator.</param>
        /// <param name="ClientCertificateSelector">A delegate to select a TLS client certificate.</param>
        /// <param name="ClientCert">The SSL/TLS client certificate to use of HTTP authentication.</param>
        /// <param name="HTTPUserAgent">The HTTP user agent identification.</param>
        /// <param name="RequestTimeout">An optional request timeout.</param>
        /// <param name="TransmissionRetryDelay">The delay between transmission retries.</param>
        /// <param name="MaxNumberOfRetries">The maximum number of transmission retries for HTTP request.</param>
        /// <param name="DisableLogging">Disable all logging.</param>
        /// <param name="LoggingContext">An optional context for logging.</param>
        /// <param name="LogfileCreator">A delegate to create a log file from the given context and log file name.</param>
        /// <param name="DNSClient">The DNS client to use.</param>
        public EMPClient(APIKey                               APIKey,
                         URL?                                 RemoteURL                    = null,
                         HTTPHostname?                        VirtualHostname              = null,
                         String                               Description                  = null,
                         RemoteCertificateValidationCallback  RemoteCertificateValidator   = null,
                         LocalCertificateSelectionCallback    ClientCertificateSelector    = null,
                         X509Certificate                      ClientCert                   = null,
                         String                               HTTPUserAgent                = DefaultHTTPUserAgent,
                         TimeSpan?                            RequestTimeout               = null,
                         TransmissionRetryDelayDelegate       TransmissionRetryDelay       = null,
                         UInt16?                              MaxNumberOfRetries           = DefaultMaxNumberOfRetries,
                         Boolean                              DisableLogging               = false,
                         String                               LoggingPath                  = null,
                         String                               LoggingContext               = null,
                         LogfileCreatorDelegate               LogfileCreator               = null,
                         DNSClient                            DNSClient                    = null)


            : base(RemoteURL                  ?? DefaultRemoteURL,
                   VirtualHostname,
                   Description,
                   RemoteCertificateValidator ?? ((sender, certificate, chain, sslPolicyErrors) => true),
                   ClientCertificateSelector,
                   ClientCert,
                   null,
                   null,
                   HTTPUserAgent,
                   RequestTimeout,
                   TransmissionRetryDelay,
                   MaxNumberOfRetries,
                   DNSClient)

        {

            #region Initial checks

            if (APIKey.IsNullOrEmpty)
                throw new ArgumentNullException(nameof(APIKey),  "The given API key must not be null or empty!");

            #endregion

            this.APIKey      = APIKey;

            base.HTTPLogger  = DisableLogging == false
                                   ? new Logger(this,
                                                LoggingPath,
                                                LoggingContext,
                                                LogfileCreator)
                                   : null;

        }

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
                                                   Description,
                                                   Request.EventTrackingId,
                                                   Request.MinLat,
                                                   Request.MaxLat,
                                                   Request.MinLong,
                                                   Request.MaxLong,
                                                   Request.IncludeConnectorTypes,
                                                   Request.RequestTimeout ?? RequestTimeout);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(EMPClient) + "." + nameof(OnStationGetSurfaceRequest));
            }

            #endregion


            using (var _JSONClient = new JSONClient(RemoteURL,
                                                    VirtualHostname,
                                                    Description,
                                                    RemoteCertificateValidator,
                                                    ClientCertificateSelector,
                                                    ClientCert,
                                                    null,
                                                    null,
                                                    HTTPUserAgent,
                                                    //URLPathPrefix,
                                                    RequestTimeout,
                                                    TransmissionRetryDelay,
                                                    MaxNumberOfRetries,
                                                    DNSClient))
            {

                result = await _JSONClient.Query(_CustomStationGetSurfaceJSONRequestMapper(Request,
                                                                                           Request.ToJSON()),
                                                 HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                 RequestLogDelegate:   OnStationGetSurfaceHTTPRequest,
                                                 ResponseLogDelegate:  OnStationGetSurfaceHTTPResponse,
                                                 CancellationToken:    Request.CancellationToken,
                                                 EventTrackingId:      Request.EventTrackingId,
                                                 RequestTimeout:       Request.RequestTimeout ?? RequestTimeout,

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
                                                    Description,
                                                    Request.EventTrackingId,
                                                    Request.MinLat,
                                                    Request.MaxLat,
                                                    Request.MinLong,
                                                    Request.MaxLong,
                                                    Request.IncludeConnectorTypes,
                                                    Request.RequestTimeout ?? RequestTimeout,
                                                    result.Content,
                                                    Endtime - StartTime);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(EMPClient) + "." + nameof(OnStationGetSurfaceResponse));
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
                                              Description,
                                              Request.EventTrackingId,
                                              Request.User,
                                              Request.ConnectorId,
                                              Request.PaymentReference,
                                              Request.RequestTimeout ?? RequestTimeout);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(EMPClient) + "." + nameof(OnSessionStartRequest));
            }

            #endregion


            using (var _JSONClient = new JSONClient(RemoteURL,
                                                    VirtualHostname,
                                                    Description,
                                                    RemoteCertificateValidator,
                                                    ClientCertificateSelector,
                                                    ClientCert,
                                                    null,
                                                    null,
                                                    HTTPUserAgent,
                                                    //URLPathPrefix,
                                                    RequestTimeout,
                                                    TransmissionRetryDelay,
                                                    MaxNumberOfRetries,
                                                    DNSClient))
            {

                result = await _JSONClient.Query(_CustomSessionStartJSONRequestMapper(Request,
                                                                                      Request.ToJSON()),
                                                 HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                 RequestLogDelegate:   OnSessionStartHTTPRequest,
                                                 ResponseLogDelegate:  OnSessionStartHTTPResponse,
                                                 CancellationToken:    Request.CancellationToken,
                                                 EventTrackingId:      Request.EventTrackingId,
                                                 RequestTimeout:       Request.RequestTimeout ?? RequestTimeout,

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
                                               Description,
                                               Request.EventTrackingId,
                                               Request.User,
                                               Request.ConnectorId,
                                               Request.PaymentReference,
                                               Request.RequestTimeout ?? RequestTimeout,
                                               result.Content,
                                               Endtime - StartTime);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(EMPClient) + "." + nameof(OnSessionStartResponse));
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
                                             Description,
                                             Request.EventTrackingId,
                                             Request.User,
                                             Request.ConnectorId,
                                             Request.SessionId,
                                             Request.RequestTimeout ?? RequestTimeout);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(EMPClient) + "." + nameof(OnSessionStopRequest));
            }

            #endregion


            using (var _JSONClient = new JSONClient(RemoteURL,
                                                    VirtualHostname,
                                                    Description,
                                                    RemoteCertificateValidator,
                                                    ClientCertificateSelector,
                                                    ClientCert,
                                                    null,
                                                    null,
                                                    HTTPUserAgent,
                                                    //URLPathPrefix,
                                                    RequestTimeout,
                                                    TransmissionRetryDelay,
                                                    MaxNumberOfRetries,
                                                    DNSClient))
            {

                result = await _JSONClient.Query(_CustomSessionStopJSONRequestMapper(Request,
                                                                                     Request.ToJSON()),
                                                 HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                 RequestLogDelegate:   OnSessionStopHTTPRequest,
                                                 ResponseLogDelegate:  OnSessionStopHTTPResponse,
                                                 CancellationToken:    Request.CancellationToken,
                                                 EventTrackingId:      Request.EventTrackingId,
                                                 RequestTimeout:       Request.RequestTimeout ?? RequestTimeout,

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
                                              Description,
                                              Request.EventTrackingId,
                                              Request.User,
                                              Request.ConnectorId,
                                              Request.SessionId,
                                              Request.RequestTimeout ?? RequestTimeout,
                                              result.Content,
                                              Endtime - StartTime);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(EMPClient) + "." + nameof(OnSessionStopResponse));
            }

            #endregion

            return result;

        }

        #endregion


    }

}
