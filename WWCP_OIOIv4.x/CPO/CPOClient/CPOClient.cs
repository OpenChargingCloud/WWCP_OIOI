﻿/*
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
using System.Net.Security;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.JSON;
using org.GraphDefined.Vanaheimr.Hermod.SOAP;
using System.Linq;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

    public class OIOI_Exception : ApplicationException
    {

        public String  MethodName   { get; }

        public OIOI_Exception(String     MethodName,
                              String     Message,
                              Exception  Exception)

            : base(Message,
                   Exception)

        {

            this.MethodName  = MethodName;

        }

    }

    public class OIOI_CPOClientException : OIOI_Exception
    {

        public CPOClient  Client   { get; }

        public OIOI_CPOClientException(CPOClient  Client,
                                       String     MethodName,
                                       String     Message,
                                       Exception  Exception)

            : base(MethodName,
                   Message,
                   Exception)

        {

            this.Client = Client;

        }

    }


    /// <summary>
    /// An OIOI CPO Client.
    /// </summary>
    public partial class CPOClient : AJSONClient,
                                     ICPOClient
    {

        #region Data

        /// <summary>
        /// The default HTTP user agent string.
        /// </summary>
        public new const           String   DefaultHTTPUserAgent  = "GraphDefined OIOI " + Version.Number + " CPO Client";

        /// <summary>
        /// The default remote TCP port to connect to.
        /// </summary>
        public new static readonly IPPort   DefaultRemotePort     = IPPort.Parse(443);

        /// <summary>
        /// The default HTTP client URI prefix.
        /// </summary>
        public     static readonly HTTPURI  DefaultURIPrefix      = HTTPURI.Parse("/api/v4/request");

        #endregion

        #region Properties

        /// <summary>
        /// The URI prefix for all HTTP requests.
        /// </summary>
        public HTTPURI                              URIPrefix                     { get; }

        /// <summary>
        /// The API key for all requests.
        /// </summary>
        public APIKey                               APIKey                        { get; }

        /// <summary>
        /// A delegate to select a partner identification based on the given charging station.
        /// </summary>
        public PartnerIdForStationDelegate          StationPartnerIdSelector      { get; }

        /// <summary>
        /// A delegate to select a partner identification based on the given charging connector status.
        /// </summary>
        public PartnerIdForConnectorStatusDelegate        ConnectorStatusPartnerIdSelector    { get; }

        /// <summary>
        /// The attached OIOI CPO client (HTTP/JSON client) logger.
        /// </summary>
        public CPOClientLogger                      Logger                        { get; }

        #endregion

        public IncludeStationDelegate               IncludeStation                { get; }
        public IncludeStationIdDelegate             IncludeStationId              { get; }

        public IncludeConnectorIdDelegate           IncludeConnectorId            { get; }
        public IncludeConnectorStatusTypesDelegate  IncludeConnectorStatusType    { get; }
        public IncludeConnectorStatusDelegate       IncludeConnectorStatus        { get; }

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

        #region OnConnectorPostStatusRequest/-Response

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

        #region OnSessionPostRequest/-Response

        /// <summary>
        /// An event fired whenever a charging session post request will be send.
        /// </summary>
        public event OnSessionPostRequestDelegate   OnSessionPostRequest;

        /// <summary>
        /// An event fired whenever a HTTP request posting charging session will be send.
        /// </summary>
        public event ClientRequestLogHandler        OnSessionPostHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging session post request had been received.
        /// </summary>
        public event ClientResponseLogHandler       OnSessionPostHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging session post request had been received.
        /// </summary>
        public event OnSessionPostResponseDelegate  OnSessionPostResponse;

        #endregion

        #endregion

        #region Custom request mappers

        #region CustomStationPostRequestMapper

        #region CustomStationPostRequestMapper

        private Func<StationPostRequest, StationPostRequest> _CustomStationPostRequestMapper = _ => _;

        public Func<StationPostRequest, StationPostRequest> CustomStationPostRequestMapper
        {

            get
            {
                return _CustomStationPostRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomStationPostRequestMapper = value;
            }

        }

        #endregion

        #region CustomStationPostJSONRequestMapper

        private Func<StationPostRequest, JObject, JObject> _CustomStationPostJSONRequestMapper = (request, xml) => xml;

        public Func<StationPostRequest, JObject, JObject> CustomStationPostJSONRequestMapper
        {

            get
            {
                return _CustomStationPostJSONRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomStationPostJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<StationPostResponse, StationPostResponse.Builder> CustomStationPostResponseMapper { get; set; }

        #endregion

        #region CustomConnectorPostStatusRequestMapper

        #region CustomConnectorPostStatusRequestMapper

        private Func<ConnectorPostStatusRequest, ConnectorPostStatusRequest> _CustomConnectorPostStatusRequestMapper = _ => _;

        public Func<ConnectorPostStatusRequest, ConnectorPostStatusRequest> CustomConnectorPostStatusRequestMapper
        {

            get
            {
                return _CustomConnectorPostStatusRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomConnectorPostStatusRequestMapper = value;
            }

        }

        #endregion

        #region CustomConnectorPostStatusJSONRequestMapper

        private Func<ConnectorPostStatusRequest, JObject, JObject> _CustomConnectorPostStatusJSONRequestMapper = (request, xml) => xml;

        public Func<ConnectorPostStatusRequest, JObject, JObject> CustomConnectorPostStatusJSONRequestMapper
        {

            get
            {
                return _CustomConnectorPostStatusJSONRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomConnectorPostStatusJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<ConnectorPostStatusResponse, ConnectorPostStatusResponse.Builder> CustomConnectorPostStatusResponseMapper { get; set; }

        #endregion


        #region CustomRFIDVerifyRequestMapper

        #region CustomRFIDVerifyRequestMapper

        private Func<RFIDVerifyRequest, RFIDVerifyRequest> _CustomRFIDVerifyRequestMapper = _ => _;

        public Func<RFIDVerifyRequest, RFIDVerifyRequest> CustomRFIDVerifyRequestMapper
        {

            get
            {
                return _CustomRFIDVerifyRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomRFIDVerifyRequestMapper = value;
            }

        }

        #endregion

        #region CustomRFIDVerifyJSONRequestMapper

        private Func<RFIDVerifyRequest, JObject, JObject> _CustomRFIDVerifyJSONRequestMapper = (request, xml) => xml;

        public Func<RFIDVerifyRequest, JObject, JObject> CustomRFIDVerifyJSONRequestMapper
        {

            get
            {
                return _CustomRFIDVerifyJSONRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomRFIDVerifyJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<RFIDVerifyResponse, RFIDVerifyResponse.Builder> CustomRFIDVerifyResponseMapper { get; set; }

        #endregion

        #region CustomSessionPostRequestMapper

        #region CustomSessionPostRequestMapper

        private Func<SessionPostRequest, SessionPostRequest> _CustomSessionPostRequestMapper = _ => _;

        public Func<SessionPostRequest, SessionPostRequest> CustomSessionPostRequestMapper
        {

            get
            {
                return _CustomSessionPostRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomSessionPostRequestMapper = value;
            }

        }

        #endregion

        #region CustomSessionPostJSONRequestMapper

        private Func<SessionPostRequest, JObject, JObject> _CustomSessionPostJSONRequestMapper = (request, xml) => xml;

        public Func<SessionPostRequest, JObject, JObject> CustomSessionPostJSONRequestMapper
        {

            get
            {
                return _CustomSessionPostJSONRequestMapper;
            }

            set
            {
                if (value != null)
                    _CustomSessionPostJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<SessionPostResponse, SessionPostResponse.Builder> CustomSessionPostResponseMapper { get; set; }

        #endregion


        public CustomJSONSerializerDelegate<StationPostRequest> CustomStationPostRequestSerializer   { get; set; }
        public CustomJSONSerializerDelegate<Station>            CustomStationSerializer              { get; set; }
        public CustomJSONSerializerDelegate<Address>            CustomAddressSerializer              { get; set; }
        public CustomJSONSerializerDelegate<Connector>          CustomConnectorSerializer            { get; set; }

        public CustomJSONSerializerDelegate<SessionPostRequest> CustomSessionPostRequestSerializer   { get; set; }
        public CustomJSONSerializerDelegate<Session>            CustomSessionSerializer              { get; set; }

        #endregion

        #region Constructor(s)

        #region CPOClient(ClientId, Hostname, APIKey, ..., LoggingContext = CPOClientLogger.DefaultContext, ...)

        /// <summary>
        /// Create a new OIOI CPO Client using the given parameters.
        /// </summary>
        /// <param name="ClientId">A unqiue identification of this client.</param>
        /// <param name="Hostname">The hostname of the remote OIOI service.</param>
        /// <param name="APIKey">The PlugSurfing API key.</param>
        /// <param name="StationPartnerIdSelector">A delegate to select a partner identification based on the given charging station.</param>
        /// <param name="ConnectorStatusPartnerIdSelector">A delegate to select a partner identification based on the given charging connector status.</param>
        /// <param name="RemotePort">An optional TCP port of the remote OIOI service.</param>
        /// <param name="RemoteCertificateValidator">A delegate to verify the remote TLS certificate.</param>
        /// <param name="ClientCertificateSelector">A delegate to select a TLS client certificate.</param>
        /// <param name="HTTPVirtualHost">An optional HTTP virtual hostname of the remote OIOI service.</param>
        /// <param name="HTTPUserAgent">An optional HTTP user agent identification string for this HTTP client.</param>
        /// <param name="URIPrefix">The default URI prefix.</param>
        /// <param name="RequestTimeout">An optional timeout for upstream queries.</param>
        /// <param name="MaxNumberOfRetries">The default number of maximum transmission retries.</param>
        /// <param name="DNSClient">An optional DNS client to use.</param>
        /// <param name="LoggingContext">An optional context for logging client methods.</param>
        /// <param name="LogFileCreator">A delegate to create a log file from the given context and log file name.</param>
        public CPOClient(String                               ClientId,
                         String                               Hostname,
                         APIKey                               APIKey,
                         PartnerIdForStationDelegate          StationPartnerIdSelector,
                         PartnerIdForConnectorStatusDelegate  ConnectorStatusPartnerIdSelector,
                         IPPort?                              RemotePort                   = null,
                         RemoteCertificateValidationCallback  RemoteCertificateValidator   = null,
                         LocalCertificateSelectionCallback    ClientCertificateSelector    = null,
                         String                               HTTPVirtualHost              = null,
                         HTTPURI?                             URIPrefix                    = null,
                         String                               HTTPUserAgent                = DefaultHTTPUserAgent,
                         IncludeStationDelegate               IncludeStation               = null,
                         IncludeStationIdDelegate             IncludeStationId             = null,
                         IncludeConnectorIdDelegate           IncludeConnectorId           = null,
                         IncludeConnectorStatusTypesDelegate  IncludeConnectorStatusType   = null,
                         IncludeConnectorStatusDelegate       IncludeConnectorStatus       = null,
                         TimeSpan?                            RequestTimeout               = null,
                         Byte?                                MaxNumberOfRetries           = DefaultMaxNumberOfRetries,
                         DNSClient                            DNSClient                    = null,
                         String                               LoggingContext               = CPOClientLogger.DefaultContext,
                         LogfileCreatorDelegate               LogFileCreator               = null)

            : base(ClientId,
                   Hostname,
                   RemotePort ?? DefaultRemotePort,
                   RemoteCertificateValidator,
                   ClientCertificateSelector,
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

            if (StationPartnerIdSelector == null)
                throw new ArgumentNullException(nameof(Hostname), "The given partner identification selector must not be null!");

            if (ConnectorStatusPartnerIdSelector == null)
                throw new ArgumentNullException(nameof(Hostname), "The given partner identification selector must not be null!");

            #endregion

            this.APIKey                      = APIKey;
            this.URIPrefix                   = URIPrefix ?? DefaultURIPrefix;
            this.StationPartnerIdSelector    = StationPartnerIdSelector;
            this.ConnectorStatusPartnerIdSelector  = ConnectorStatusPartnerIdSelector;

            this.IncludeStation              = IncludeStation             ?? (station             => true);
            this.IncludeStationId            = IncludeStationId           ?? (stationid           => true);
            this.IncludeConnectorId          = IncludeConnectorId         ?? (connectorid         => true);
            this.IncludeConnectorStatusType  = IncludeConnectorStatusType ?? (connectorstatustype => true);
            this.IncludeConnectorStatus      = IncludeConnectorStatus     ?? (connectorstatus     => true);

            this.Logger                      = new CPOClientLogger(this,
                                                                   LoggingContext,
                                                                   LogFileCreator);

        }

        #endregion

        #region CPOClient(ClientId, Logger, Hostname, APIKey, ...)

        /// <summary>
        /// Create a new OIOI CPO Client.
        /// </summary>
        /// <param name="ClientId">A unqiue identification of this client.</param>
        /// <param name="Logger">A CPO client logger.</param>
        /// <param name="Hostname">The hostname of the remote OIOI service.</param>
        /// <param name="APIKey">The PlugSurfing API key.</param>
        /// <param name="StationPartnerIdSelector">A delegate to select a partner identification based on the given charging station.</param>
        /// <param name="ConnectorStatusPartnerIdSelector">A delegate to select a partner identification based on the given charging connector status.</param>
        /// <param name="RemotePort">An optional TCP port of the remote OIOI service.</param>
        /// <param name="URIPrefix">The default URI prefix.</param>
        /// <param name="RemoteCertificateValidator">A delegate to verify the remote TLS certificate.</param>
        /// <param name="ClientCertificateSelector">A delegate to select a TLS client certificate.</param>
        /// <param name="HTTPVirtualHost">An optional HTTP virtual hostname of the remote OIOI service.</param>
        /// <param name="HTTPUserAgent">An optional HTTP user agent identification string for this HTTP client.</param>
        /// <param name="RequestTimeout">An optional timeout for upstream queries.</param>
        /// <param name="MaxNumberOfRetries">The default number of maximum transmission retries.</param>
        /// <param name="DNSClient">An optional DNS client to use.</param>
        public CPOClient(String                               ClientId,
                         CPOClientLogger                      Logger,
                         String                               Hostname,
                         APIKey                               APIKey,
                         PartnerIdForStationDelegate          StationPartnerIdSelector,
                         PartnerIdForConnectorStatusDelegate  ConnectorStatusPartnerIdSelector,
                         IPPort?                              RemotePort                   = null,
                         HTTPURI?                             URIPrefix                    = null,
                         RemoteCertificateValidationCallback  RemoteCertificateValidator   = null,
                         LocalCertificateSelectionCallback    ClientCertificateSelector    = null,
                         String                               HTTPVirtualHost              = null,
                         String                               HTTPUserAgent                = DefaultHTTPUserAgent,
                         IncludeStationDelegate               IncludeStation               = null,
                         IncludeStationIdDelegate             IncludeStationId             = null,
                         IncludeConnectorIdDelegate           IncludeConnectorId           = null,
                         IncludeConnectorStatusTypesDelegate  IncludeConnectorStatusType   = null,
                         IncludeConnectorStatusDelegate       IncludeConnectorStatus       = null,
                         TimeSpan?                            RequestTimeout               = null,
                         Byte?                                MaxNumberOfRetries           = DefaultMaxNumberOfRetries,
                         DNSClient                            DNSClient                    = null)

            : base(ClientId,
                   Hostname,
                   RemotePort ?? DefaultRemotePort,
                   RemoteCertificateValidator,
                   ClientCertificateSelector,
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

            if (StationPartnerIdSelector == null)
                throw new ArgumentNullException(nameof(Hostname),  "The given partner identification selector must not be null!");

            if (ConnectorStatusPartnerIdSelector == null)
                throw new ArgumentNullException(nameof(Hostname),  "The given partner identification selector must not be null!");

            #endregion

            this.URIPrefix                   = URIPrefix ?? DefaultURIPrefix;
            this.APIKey                      = APIKey;
            this.StationPartnerIdSelector    = StationPartnerIdSelector;
            this.ConnectorStatusPartnerIdSelector  = ConnectorStatusPartnerIdSelector;

            this.IncludeStation              = IncludeStation             ?? (station             => true);
            this.IncludeStationId            = IncludeStationId           ?? (stationid           => true);
            this.IncludeConnectorId          = IncludeConnectorId         ?? (connectorid         => true);
            this.IncludeConnectorStatusType  = IncludeConnectorStatusType ?? (connectorstatustype => true);
            this.IncludeConnectorStatus      = IncludeConnectorStatus     ?? (connectorstatus     => true);

            this.Logger                      = Logger;

        }

        #endregion

        #endregion


        #region StationPost        (Request, ...)

        /// <summary>
        /// Upload a charging station onto the OIOI server.
        /// </summary>
        /// <param name="Request">A StationPost request.</param>
        public async Task<HTTPResponse<StationPostResponse>>

            StationPost(StationPostRequest  Request)

        {

            #region Initial checks

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The given StationPost request must not be null!");

            Request = _CustomStationPostRequestMapper(Request);

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The mapped StationPost request must not be null!");


            Byte                              TransmissionRetry  = 0;
            HTTPResponse<StationPostResponse> result             = null;

            #endregion

            #region Send OnStationPostRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                if (OnStationPostRequest != null)
                    await Task.WhenAll(OnStationPostRequest.GetInvocationList().
                                       Cast<OnStationPostRequestDelegate>().
                                       Select(e => e(StartTime,
                                                     Request.Timestamp.Value,
                                                     this,
                                                     ClientId,
                                                     Request.EventTrackingId,
                                                     Request.Station,
                                                     Request.PartnerIdentifier,
                                                     Request.RequestTimeout ?? RequestTimeout.Value))).
                                       ConfigureAwait(false);

            }
            catch (Exception e)
            {

                SendException(DateTime.UtcNow,
                              this,
                              new OIOI_CPOClientException(this,
                                                          nameof(OnStationPostRequest),
                                                          "Sending " + nameof(OnStationPostRequest) + " events failed!",
                                                          e));

            }

            #endregion


            // Notes: It is not allowed to change the connectors of an existing station.
            //        The station’s connectors’ IDs may not be changed once it is created.

            if (IncludeStation  (Request.Station) &&
                IncludeStationId(Request.Station.Id))
            {

                do
                {

                    using (var _JSONClient = new JSONClient(Hostname,
                                                            HTTPVirtualHost,
                                                            URIPrefix,
                                                            RemotePort,
                                                            RemoteCertificateValidator,
                                                            ClientCertificateSelector,
                                                            UserAgent,
                                                            RequestTimeout,
                                                            DNSClient))
                    {

                        result = await _JSONClient.Query(_CustomStationPostJSONRequestMapper(Request,
                                                                                             Request.ToJSON(CustomStationPostRequestSerializer,
                                                                                                            CustomStationSerializer,
                                                                                                            CustomAddressSerializer,
                                                                                                            CustomConnectorSerializer)),
                                                         HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                         RequestLogDelegate:   OnStationPostHTTPRequest,
                                                         ResponseLogDelegate:  OnStationPostHTTPResponse,
                                                         CancellationToken:    Request.CancellationToken,
                                                         EventTrackingId:      Request.EventTrackingId,
                                                         RequestTimeout:       Request.RequestTimeout ?? RequestTimeout,
                                                         NumberOfRetry:        TransmissionRetry,

                                                         #region OnSuccess

                                                         OnSuccess: JSONResponse => JSONResponse.ConvertContent(Request,
                                                                                                                (request, json, onexception) =>
                                                                                                                    StationPostResponse.Parse(request,
                                                                                                                                              json,
                                                                                                                                              CustomStationPostResponseMapper,
                                                                                                                                              onexception)),

                                                         #endregion

                                                         #region OnJSONFault

                                                         OnJSONFault: (timestamp, jsonclient, httpresponse) => {

                                                             SendJSONError(timestamp, this, httpresponse.Content);

                                                             return new HTTPResponse<StationPostResponse>(
                                                                        httpresponse,
                                                                        StationPostResponse.InvalidResponseFormat(Request,
                                                                                                                  httpresponse),
                                                                        IsFault: true);

                                                         },

                                                         #endregion

                                                         #region OnHTTPError

                                                         OnHTTPError: (timestamp, soapclient, httpresponse) => {

                                                             SendHTTPError(timestamp, this, httpresponse);

                                                             return new HTTPResponse<StationPostResponse>(
                                                                        httpresponse,
                                                                        StationPostResponse.InvalidResponseFormat(Request,
                                                                                                                  httpresponse),
                                                                        IsFault: true);

                                                         },

                                                         #endregion

                                                         #region OnException

                                                         OnException: (timestamp, sender, exception) => {

                                                             SendException(timestamp, sender, exception);

                                                             return HTTPResponse<StationPostResponse>.ExceptionThrown(

                                                                 StationPostResponse.ClientRequestError(Request,
                                                                                                        "Exception occured!"),

                                                                 Exception: exception);

                                                         }

                                                         #endregion

                                                        );

                    }

                    if (result == null)
                        result = HTTPResponse<StationPostResponse>.ClientError(
                                     StationPostResponse.InvalidResponseFormat(
                                         Request
                                     )
                                 );

                }
                while (result.HTTPStatusCode == HTTPStatusCode.RequestTimeout &&
                       TransmissionRetry++    < MaxNumberOfRetries);

            }

            #region ...or no station data to push?

            else

                result = HTTPResponse<StationPostResponse>.OK(
                             StationPostResponse.Success(Request,
                                                         "No station data to push")
                         );

            #endregion


            #region Send OnStationPostResponse event

            var Endtime = DateTime.UtcNow;

            try
            {

                if (OnStationPostResponse != null)
                    await Task.WhenAll(OnStationPostResponse.GetInvocationList().
                                       Cast<OnStationPostResponseDelegate>().
                                       Select(e => e(Endtime,
                                                     Request.Timestamp.Value,
                                                     this,
                                                     ClientId,
                                                     Request.EventTrackingId,
                                                     Request.Station,
                                                     Request.PartnerIdentifier,
                                                     Request.RequestTimeout ?? RequestTimeout.Value,
                                                     result.Content,
                                                     Endtime - StartTime))).
                                       ConfigureAwait(false);

            }
            catch (Exception e)
            {

                SendException(DateTime.UtcNow,
                              this,
                              new OIOI_CPOClientException(this,
                                                          nameof(OnStationPostResponse),
                                                          "Sending " + nameof(OnStationPostResponse) + " events failed!",
                                                          e));

            }

            #endregion

            return result;

        }

        #endregion

        #region ConnectorPostStatus(Request, ...)

        /// <summary>
        /// Update the status of a charging connector on the OIOI server.
        /// </summary>
        /// <param name="Request">A StationPost request.</param>
        public async Task<HTTPResponse<ConnectorPostStatusResponse>>

            ConnectorPostStatus(ConnectorPostStatusRequest Request)

        {

            #region Initial checks

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The given ConnectorPostStatus request must not be null!");

            Request = _CustomConnectorPostStatusRequestMapper(Request);

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The mapped ConnectorPostStatus request must not be null!");


            Byte                                      TransmissionRetry  = 0;
            HTTPResponse<ConnectorPostStatusResponse> result             = null;

            #endregion

            #region Send OnConnectorPostStatusRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                if (OnConnectorPostStatusRequest != null)
                    await Task.WhenAll(OnConnectorPostStatusRequest.GetInvocationList().
                                       Cast<OnConnectorPostStatusRequestDelegate>().
                                       Select(e => e(StartTime,
                                                     Request.Timestamp.Value,
                                                     this,
                                                     ClientId,
                                                     Request.EventTrackingId,
                                                     Request.ConnectorStatus,
                                                     Request.PartnerIdentifier,
                                                     Request.RequestTimeout ?? RequestTimeout.Value))).
                                       ConfigureAwait(false);

            }
            catch (Exception e)
            {

                SendException(DateTime.UtcNow,
                              this,
                              new OIOI_CPOClientException(this,
                                                          nameof(OnConnectorPostStatusRequest),
                                                          "Sending " + nameof(OnConnectorPostStatusRequest) + " events failed!",
                                                          e));

            }

            #endregion


            // Note: It is not allowed to send connector status before the corresponding
            //       charging station was acknowledged by someone at PlugSurfing.

            if (IncludeConnectorId        (Request.ConnectorStatus.Id)     &&
                IncludeConnectorStatusType(Request.ConnectorStatus.Status) &&
                IncludeConnectorStatus    (Request.ConnectorStatus))
            {

                do
                {

                    using (var _JSONClient = new JSONClient(Hostname,
                                                            HTTPVirtualHost,
                                                            URIPrefix,
                                                            RemotePort,
                                                            RemoteCertificateValidator,
                                                            ClientCertificateSelector,
                                                            UserAgent,
                                                            RequestTimeout,
                                                            DNSClient))
                    {

                        result = await _JSONClient.Query(_CustomConnectorPostStatusJSONRequestMapper(Request,
                                                                                                     Request.ToJSON()),
                                                         HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                         RequestLogDelegate:   OnConnectorPostStatusHTTPRequest,
                                                         ResponseLogDelegate:  OnConnectorPostStatusHTTPResponse,
                                                         CancellationToken:    Request.CancellationToken,
                                                         EventTrackingId:      Request.EventTrackingId,
                                                         RequestTimeout:       Request.RequestTimeout ?? RequestTimeout,
                                                         NumberOfRetry:        TransmissionRetry,

                                                         #region OnSuccess

                                                         OnSuccess: JSONResponse => JSONResponse.ConvertContent(Request,
                                                                                                                (request, json, onexception) =>
                                                                                                                    ConnectorPostStatusResponse.Parse(request,
                                                                                                                                                      json,
                                                                                                                                                      CustomConnectorPostStatusResponseMapper,
                                                                                                                                                      onexception)),

                                                         #endregion

                                                         #region OnJSONFault

                                                         OnJSONFault: (timestamp, jsonclient, httpresponse) => {

                                                             SendJSONError(timestamp, this, httpresponse.Content);

                                                             return new HTTPResponse<ConnectorPostStatusResponse>(
                                                                        httpresponse,
                                                                        ConnectorPostStatusResponse.InvalidResponseFormat(Request,
                                                                                                                          httpresponse),
                                                                        IsFault: true);

                                                         },

                                                         #endregion

                                                         #region OnHTTPError

                                                         OnHTTPError: (timestamp, soapclient, httpresponse) => {

                                                             // 404 The connector or the status were not found
                                                             // 422 The connector could not be identified uniquely

                                                             SendHTTPError(timestamp, this, httpresponse);

                                                             return new HTTPResponse<ConnectorPostStatusResponse>(
                                                                        httpresponse,
                                                                        ConnectorPostStatusResponse.InvalidResponseFormat(Request,
                                                                                                                          httpresponse),
                                                                        IsFault: true);

                                                         },

                                                         #endregion

                                                         #region OnException

                                                         OnException: (timestamp, sender, exception) => {

                                                             SendException(timestamp, sender, exception);

                                                             return HTTPResponse<ConnectorPostStatusResponse>.ExceptionThrown(

                                                                 ConnectorPostStatusResponse.ClientRequestError(Request,
                                                                                                                "Exception occured!"),

                                                                 Exception: exception);

                                                         }

                                                         #endregion

                                                        );

                    }

                    if (result == null)
                        result = HTTPResponse<ConnectorPostStatusResponse>.ClientError(
                                     ConnectorPostStatusResponse.InvalidResponseFormat(
                                         Request
                                     )
                                 );

                }
                while (result.HTTPStatusCode == HTTPStatusCode.RequestTimeout &&
                       TransmissionRetry++    < MaxNumberOfRetries);

            }

            #region ...or no station data to push?

            else

                result = HTTPResponse<ConnectorPostStatusResponse>.OK(
                             ConnectorPostStatusResponse.Success(Request,
                                                                 "No connector status to push")
                         );

            #endregion


            #region Send OnConnectorPostStatusResponse event

            var Endtime = DateTime.UtcNow;

            try
            {

                if (OnConnectorPostStatusResponse != null)
                    await Task.WhenAll(OnConnectorPostStatusResponse.GetInvocationList().
                                       Cast<OnConnectorPostStatusResponseDelegate>().
                                       Select(e => e(Endtime,
                                                     Request.Timestamp.Value,
                                                     this,
                                                     ClientId,
                                                     Request.EventTrackingId,
                                                     Request.ConnectorStatus,
                                                     Request.PartnerIdentifier,
                                                     Request.RequestTimeout ?? RequestTimeout.Value,
                                                     result.Content,
                                                     Endtime - StartTime))).
                                       ConfigureAwait(false);

            }
            catch (Exception e)
            {

                SendException(DateTime.UtcNow,
                              this,
                              new OIOI_CPOClientException(this,
                                                          nameof(OnConnectorPostStatusResponse),
                                                          "Sending " + nameof(OnConnectorPostStatusResponse) + " events failed!",
                                                          e));

            }

            #endregion

            return result;

        }

        #endregion


        #region RFIDVerify (Request, ...)

        /// <summary>
        /// Verify a RFID identification via the OIOI server.
        /// </summary>
        /// <param name="Request">A RFIDVerify request.</param>
        public async Task<HTTPResponse<RFIDVerifyResponse>>

            RFIDVerify(RFIDVerifyRequest Request)

        {

            #region Initial checks

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The given RFIDVerify request must not be null!");

            Request = _CustomRFIDVerifyRequestMapper(Request);

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The mapped RFIDVerify request must not be null!");


            Byte                             TransmissionRetry  = 0;
            HTTPResponse<RFIDVerifyResponse> result             = null;

            #endregion

            #region Send OnRFIDVerifyRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                if (OnRFIDVerifyRequest != null)
                    await Task.WhenAll(OnRFIDVerifyRequest.GetInvocationList().
                                       Cast<OnRFIDVerifyRequestDelegate>().
                                       Select(e => e(StartTime,
                                                     Request.Timestamp.Value,
                                                     this,
                                                     ClientId,
                                                     Request.EventTrackingId,
                                                     Request.RFIDId,
                                                     Request.RequestTimeout ?? RequestTimeout.Value))).
                                       ConfigureAwait(false);

            }
            catch (Exception e)
            {

                SendException(DateTime.UtcNow,
                              this,
                              new OIOI_CPOClientException(this,
                                                          nameof(OnRFIDVerifyRequest),
                                                          "Sending " + nameof(OnRFIDVerifyRequest) + " events failed!",
                                                          e));

            }

            #endregion


            do
            {

                using (var _JSONClient = new JSONClient(Hostname,
                                                        HTTPVirtualHost,
                                                        URIPrefix,
                                                        RemotePort,
                                                        RemoteCertificateValidator,
                                                        ClientCertificateSelector,
                                                        UserAgent,
                                                        RequestTimeout,
                                                        DNSClient))
                {

                    result = await _JSONClient.Query(_CustomRFIDVerifyJSONRequestMapper(Request,
                                                                                        Request.ToJSON()),
                                                     HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                     RequestLogDelegate:   OnRFIDVerifyHTTPRequest,
                                                     ResponseLogDelegate:  OnRFIDVerifyHTTPResponse,
                                                     CancellationToken:    Request.CancellationToken,
                                                     EventTrackingId:      Request.EventTrackingId,
                                                     RequestTimeout:       Request.RequestTimeout ?? RequestTimeout,
                                                     NumberOfRetry:        TransmissionRetry,

                                                     #region OnSuccess

                                                     OnSuccess: JSONResponse => JSONResponse.ConvertContent(Request,
                                                                                                            (request, json, onexception) =>
                                                                                                                RFIDVerifyResponse.Parse(request,
                                                                                                                                         json,
                                                                                                                                         CustomRFIDVerifyResponseMapper,
                                                                                                                                         onexception)),

                                                     #endregion

                                                     #region OnJSONFault

                                                     OnJSONFault: (timestamp, jsonclient, httpresponse) => {

                                                         SendJSONError(timestamp, this, httpresponse.Content);

                                                         return new HTTPResponse<RFIDVerifyResponse>(
                                                                    httpresponse,
                                                                    RFIDVerifyResponse.InvalidResponseFormat(Request,
                                                                                                             httpresponse),
                                                                    IsFault: true);

                                                     },

                                                     #endregion

                                                     #region OnHTTPError

                                                     OnHTTPError: (timestamp, soapclient, httpresponse) => {

                                                         // 404 The connector or the status were not found
                                                         // 422 The connector could not be identified uniquely

                                                         SendHTTPError(timestamp, this, httpresponse);

                                                         return new HTTPResponse<RFIDVerifyResponse>(
                                                                    httpresponse,
                                                                    RFIDVerifyResponse.InvalidResponseFormat(Request,
                                                                                                             httpresponse),
                                                                    IsFault: true);

                                                     },

                                                     #endregion

                                                     #region OnException

                                                     OnException: (timestamp, sender, exception) => {

                                                         SendException(timestamp, sender, exception);

                                                         return HTTPResponse<RFIDVerifyResponse>.ExceptionThrown(

                                                             RFIDVerifyResponse.ClientRequestError(Request,
                                                                                                   "Exception occured!"),

                                                             Exception: exception);

                                                     }

                                                     #endregion

                                                    );

                }

                if (result == null)
                    result = HTTPResponse<RFIDVerifyResponse>.ClientError(
                                 RFIDVerifyResponse.InvalidResponseFormat(
                                     Request
                                 )
                             );

            }
            while (result.HTTPStatusCode == HTTPStatusCode.RequestTimeout &&
                   TransmissionRetry++    < MaxNumberOfRetries);


            #region Send OnRFIDVerifyResponse event

            var Endtime = DateTime.UtcNow;

            try
            {

                if (OnRFIDVerifyResponse != null)
                    await Task.WhenAll(OnRFIDVerifyResponse.GetInvocationList().
                                       Cast<OnRFIDVerifyResponseDelegate>().
                                       Select(e => e(Endtime,
                                                     Request.Timestamp.Value,
                                                     this,
                                                     ClientId,
                                                     Request.EventTrackingId,
                                                     Request.RFIDId,
                                                     Request.RequestTimeout ?? RequestTimeout.Value,
                                                     result.Content,
                                                     Endtime - StartTime))).
                                       ConfigureAwait(false);

            }
            catch (Exception e)
            {

                SendException(DateTime.UtcNow,
                              this,
                              new OIOI_CPOClientException(this,
                                                          nameof(OnRFIDVerifyResponse),
                                                          "Sending " + nameof(OnRFIDVerifyResponse) + " events failed!",
                                                          e));

            }

            #endregion

            return result;

        }

        #endregion

        #region SessionPost(Request, ...)

        /// <summary>
        /// Upload a charging session to the OIOI server.
        /// </summary>
        /// <param name="Request">A SessionPost request.</param>
        public async Task<HTTPResponse<SessionPostResponse>>

            SessionPost(SessionPostRequest Request)

        {

            #region Initial checks

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The given SessionPost request must not be null!");

            Request = _CustomSessionPostRequestMapper(Request);

            if (Request == null)
                throw new ArgumentNullException(nameof(Request), "The mapped SessionPost request must not be null!");


            Byte                              TransmissionRetry  = 0;
            HTTPResponse<SessionPostResponse> result             = null;

            #endregion

            #region Send OnSessionPostRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                if (OnSessionPostRequest != null)
                    await Task.WhenAll(OnSessionPostRequest.GetInvocationList().
                                       Cast<OnSessionPostRequestDelegate>().
                                       Select(e => e(StartTime,
                                                     Request.Timestamp.Value,
                                                     this,
                                                     ClientId,
                                                     Request.EventTrackingId,
                                                     Request.Session,
                                                     Request.RequestTimeout ?? RequestTimeout.Value))).
                                       ConfigureAwait(false);

            }
            catch (Exception e)
            {

                SendException(DateTime.UtcNow,
                              this,
                              new OIOI_CPOClientException(this,
                                                          nameof(OnSessionPostRequest),
                                                          "Sending " + nameof(OnSessionPostRequest) + " events failed!",
                                                          e));

            }

            #endregion


            do
            {

                using (var _JSONClient = new JSONClient(Hostname,
                                                        HTTPVirtualHost,
                                                        URIPrefix,
                                                        RemotePort,
                                                        RemoteCertificateValidator,
                                                        ClientCertificateSelector,
                                                        UserAgent,
                                                        RequestTimeout,
                                                        DNSClient))
                {

                    result = await _JSONClient.Query(_CustomSessionPostJSONRequestMapper(Request,
                                                                                         Request.ToJSON(CustomSessionPostRequestSerializer,
                                                                                                        CustomSessionSerializer)),
                                                     HTTPRequestBuilder:   request => request.Set("Authorization", "key=" + APIKey),
                                                     RequestLogDelegate:   OnSessionPostHTTPRequest,
                                                     ResponseLogDelegate:  OnSessionPostHTTPResponse,
                                                     CancellationToken:    Request.CancellationToken,
                                                     EventTrackingId:      Request.EventTrackingId,
                                                     RequestTimeout:       Request.RequestTimeout ?? RequestTimeout,
                                                     NumberOfRetry:        TransmissionRetry,

                                                     #region OnSuccess

                                                     OnSuccess: JSONResponse => JSONResponse.ConvertContent(Request,
                                                                                                            (request, json, onexception) =>
                                                                                                            SessionPostResponse.Parse(request,
                                                                                                                                      json,
                                                                                                                                      CustomSessionPostResponseMapper,
                                                                                                                                      onexception)),

                                                     #endregion

                                                     #region OnJSONFault

                                                     OnJSONFault: (timestamp, jsonclient, httpresponse) => {

                                                         SendJSONError(timestamp, this, httpresponse.Content);

                                                         return new HTTPResponse<SessionPostResponse>(
                                                                    httpresponse,
                                                                    SessionPostResponse.InvalidResponseFormat(Request,
                                                                                                              httpresponse),
                                                                    IsFault: true);

                                                     },

                                                     #endregion

                                                     #region OnHTTPError

                                                     OnHTTPError: (timestamp, soapclient, httpresponse) => {

                                                         // 404 The connector or the status were not found
                                                         // 422 The connector could not be identified uniquely

                                                         SendHTTPError(timestamp, this, httpresponse);

                                                         return new HTTPResponse<SessionPostResponse>(
                                                                    httpresponse,
                                                                    SessionPostResponse.InvalidResponseFormat(Request,
                                                                                                              httpresponse),
                                                                    IsFault: true);

                                                     },

                                                     #endregion

                                                     #region OnException

                                                     OnException: (timestamp, sender, exception) => {

                                                         SendException(timestamp, sender, exception);

                                                         return HTTPResponse<SessionPostResponse>.ExceptionThrown(

                                                             SessionPostResponse.ClientRequestError(Request,
                                                                                                    "Exception occured!"),

                                                             Exception: exception);

                                                     }

                                                     #endregion

                                                    );

                }

                if (result == null)
                    result = HTTPResponse<SessionPostResponse>.ClientError(
                                 SessionPostResponse.InvalidResponseFormat(
                                     Request
                                 )
                             );

            }
            while (result.HTTPStatusCode == HTTPStatusCode.RequestTimeout &&
                   TransmissionRetry++    < MaxNumberOfRetries);


            #region Send OnRFIDVerifyResponse event

            var Endtime = DateTime.UtcNow;

            try
            {

                if (OnSessionPostResponse != null)
                    await Task.WhenAll(OnSessionPostResponse.GetInvocationList().
                                       Cast<OnSessionPostResponseDelegate>().
                                       Select(e => e(Endtime,
                                                     Request.Timestamp.Value,
                                                     this,
                                                     ClientId,
                                                     Request.EventTrackingId,
                                                     Request.Session,
                                                     Request.RequestTimeout ?? RequestTimeout.Value,
                                                     result.Content,
                                                     Endtime - StartTime))).
                                       ConfigureAwait(false);

            }
            catch (Exception e)
            {

                SendException(DateTime.UtcNow,
                              this,
                              new OIOI_CPOClientException(this,
                                                          nameof(OnRFIDVerifyResponse),
                                                          "Sending " + nameof(OnRFIDVerifyResponse) + " events failed!",
                                                          e));

            }

            #endregion

            return result;

        }

        #endregion


    }

}
