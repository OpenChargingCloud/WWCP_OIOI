/*
 * Copyright (c) 2016-2017 GraphDefined GmbH
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

using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

    /// <summary>
    /// An OIOI roaming client for CPOs which combines the CPO client
    /// and server and adds additional logging for both.
    /// </summary>
    public class CPORoaming : ICPOClient
    {

        #region Properties

        /// <summary>
        /// The CPO client.
        /// </summary>
        public CPOClient        CPOClient         { get; }

        public IPPort RemotePort
            => CPOClient?.RemotePort;

        public RemoteCertificateValidationCallback RemoteCertificateValidator
            => CPOClient?.RemoteCertificateValidator;

        /// <summary>
        /// The CPO server.
        /// </summary>
        public CPOServer        CPOServer         { get; }

        /// <summary>
        /// The CPO server logger.
        /// </summary>
        public CPOServerLogger  CPOServerLogger   { get; }

        /// <summary>
        /// The DNS client defines which DNS servers to use.
        /// </summary>
        public DNSClient DNSClient
            => CPOServer.DNSClient;

        /// <summary>
        /// The default request timeout for this client.
        /// </summary>
        public TimeSpan?        RequestTimeout
        {
            get
            {
                return CPOClient.RequestTimeout;
            }
        }

        /// <summary>
        /// The API key for all requests.
        /// </summary>
        public APIKey           APIKey
        {
            get
            {
                return CPOClient.APIKey;
            }
        }

        /// <summary>
        /// The default communication partner identification for all requests.
        /// </summary>
        public Partner_Id       DefaultPartnerId
        {
            get
            {
                return CPOClient.DefaultPartnerId;
            }
        }

        #endregion

        #region Events

        // CPOClient logging methods

        #region OnStationPostRequest/-Response

        /// <summary>
        /// An event fired whenever a request posting a charging station will be send.
        /// </summary>
        public event OnStationPostRequestDelegate   OnStationPostRequest
        {

            add
            {
                CPOClient.OnStationPostRequest += value;
            }

            remove
            {
                CPOClient.OnStationPostRequest -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a HTTP request posting a charging station will be send.
        /// </summary>
        public event ClientRequestLogHandler        OnStationPostHTTPRequest
        {

            add
            {
                CPOClient.OnStationPostHTTPRequest += value;
            }

            remove
            {
                CPOClient.OnStationPostHTTPRequest -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a HTTP response to a charging station post request had been received.
        /// </summary>
        public event ClientResponseLogHandler       OnStationPostHTTPResponse
        {

            add
            {
                CPOClient.OnStationPostHTTPResponse += value;
            }

            remove
            {
                CPOClient.OnStationPostHTTPResponse -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a response to a charging station post request had been received.
        /// </summary>
        public event OnStationPostResponseDelegate  OnStationPostResponse
        {

            add
            {
                CPOClient.OnStationPostResponse += value;
            }

            remove
            {
                CPOClient.OnStationPostResponse -= value;
            }

        }

        #endregion

        #region OnConnectorPostStatustRequest/-Response

        /// <summary>
        /// An event fired whenever a request posting a charging connector status will be send.
        /// </summary>
        public event OnConnectorPostStatusRequestDelegate   OnConnectorPostStatusRequest
        {

            add
            {
                CPOClient.OnConnectorPostStatusRequest += value;
            }

            remove
            {
                CPOClient.OnConnectorPostStatusRequest -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a HTTP request posting a charging connector status will be send.
        /// </summary>
        public event ClientRequestLogHandler                OnConnectorPostStatusHTTPRequest
        {

            add
            {
                CPOClient.OnConnectorPostStatusHTTPRequest += value;
            }

            remove
            {
                CPOClient.OnConnectorPostStatusHTTPRequest -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a HTTP response to a charging connector status post HTTP request had been received.
        /// </summary>
        public event ClientResponseLogHandler               OnConnectorPostStatusHTTPResponse
        {

            add
            {
                CPOClient.OnConnectorPostStatusHTTPResponse += value;
            }

            remove
            {
                CPOClient.OnConnectorPostStatusHTTPResponse -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a response to a charging connector status post HTTP request had been received.
        /// </summary>
        public event OnConnectorPostStatusResponseDelegate  OnConnectorPostStatusResponse
        {

            add
            {
                CPOClient.OnConnectorPostStatusResponse += value;
            }

            remove
            {
                CPOClient.OnConnectorPostStatusResponse -= value;
            }

        }

        #endregion

        #region OnRFIDVerifyRequest/-Response

        /// <summary>
        /// An event fired whenever a request verifying a RFID identification will be send.
        /// </summary>
        public event OnRFIDVerifyRequestDelegate   OnRFIDVerifyRequest
        {

            add
            {
                CPOClient.OnRFIDVerifyRequest += value;
            }

            remove
            {
                CPOClient.OnRFIDVerifyRequest -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a HTTP request verifying a RFID identification will be send.
        /// </summary>
        public event ClientRequestLogHandler       OnRFIDVerifyHTTPRequest
        {

            add
            {
                CPOClient.OnRFIDVerifyHTTPRequest += value;
            }

            remove
            {
                CPOClient.OnRFIDVerifyHTTPRequest -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a HTTP response to a RFID identification verification request had been received.
        /// </summary>
        public event ClientResponseLogHandler      OnRFIDVerifyHTTPResponse
        {

            add
            {
                CPOClient.OnRFIDVerifyHTTPResponse += value;
            }

            remove
            {
                CPOClient.OnRFIDVerifyHTTPResponse -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a response to a RFID identification verification request had been received.
        /// </summary>
        public event OnRFIDVerifyResponseDelegate  OnRFIDVerifyResponse
        {

            add
            {
                CPOClient.OnRFIDVerifyResponse += value;
            }

            remove
            {
                CPOClient.OnRFIDVerifyResponse -= value;
            }

        }

        #endregion

        #region OnSessionPostRequest/-Response

        /// <summary>
        /// An event fired whenever a request posting a session will be send.
        /// </summary>
        public event OnSessionPostRequestDelegate OnSessionPostRequest
        {

            add
            {
                CPOClient.OnSessionPostRequest += value;
            }

            remove
            {
                CPOClient.OnSessionPostRequest -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a HTTP request posting a session will be send.
        /// </summary>
        public event ClientRequestLogHandler OnSessionPostHTTPRequest
        {

            add
            {
                CPOClient.OnSessionPostHTTPRequest += value;
            }

            remove
            {
                CPOClient.OnSessionPostHTTPRequest -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a HTTP response to a session post request had been received.
        /// </summary>
        public event ClientResponseLogHandler OnSessionPostHTTPResponse
        {

            add
            {
                CPOClient.OnSessionPostHTTPResponse += value;
            }

            remove
            {
                CPOClient.OnSessionPostHTTPResponse -= value;
            }

        }

        /// <summary>
        /// An event fired whenever a response to a session post request had been received.
        /// </summary>
        public event OnSessionPostResponseDelegate OnSessionPostResponse
        {

            add
            {
                CPOClient.OnSessionPostResponse += value;
            }

            remove
            {
                CPOClient.OnSessionPostResponse -= value;
            }

        }

        #endregion


        // CPOServer methods

        #region OnRemoteStart/-Stop

        ///// <summary>
        ///// An event sent whenever a remote start command was received.
        ///// </summary>
        //public event OnRemoteStartDelegate OnRemoteStart
        //{

        //    add
        //    {
        //        CPOServer.OnRemoteStart += value;
        //    }

        //    remove
        //    {
        //        CPOServer.OnRemoteStart -= value;
        //    }

        //}

        ///// <summary>
        ///// An event sent whenever a remote stop command was received.
        ///// </summary>
        //public event OnRemoteStopDelegate OnRemoteStop

        //{

        //    add
        //    {
        //        CPOServer.OnRemoteStop += value;
        //    }

        //    remove
        //    {
        //        CPOServer.OnRemoteStop -= value;
        //    }

        //}

        #endregion


        // Generic HTTP/JSON server logging

        #region RequestLog

        /// <summary>
        /// An event called whenever a request came in.
        /// </summary>
        public event RequestLogHandler RequestLog
        {

            add
            {
                CPOServer.RequestLog += value;
            }

            remove
            {
                CPOServer.RequestLog -= value;
            }

        }

        #endregion

        #region AccessLog

        /// <summary>
        /// An event called whenever a request could successfully be processed.
        /// </summary>
        public event AccessLogHandler AccessLog
        {

            add
            {
                CPOServer.AccessLog += value;
            }

            remove
            {
                CPOServer.AccessLog -= value;
            }

        }

        #endregion

        #region ErrorLog

        /// <summary>
        /// An event called whenever a request resulted in an error.
        /// </summary>
        public event ErrorLogHandler ErrorLog
        {

            add
            {
                CPOServer.ErrorLog += value;
            }

            remove
            {
                CPOServer.ErrorLog -= value;
            }

        }

        #endregion

        #endregion

        #region Custom request mappers

        #region CustomStationPostRequestMapper

        #region CustomStationPostRequestMapper

        public Func<StationPostRequest, StationPostRequest> CustomStationPostRequestMapper
        {

            get
            {
                return CPOClient.CustomStationPostRequestMapper;
            }

            set
            {
                CPOClient.CustomStationPostRequestMapper = value;
            }

        }

        #endregion

        #region CustomStationPostJSONRequestMapper

        public Func<StationPostRequest, JObject, JObject> CustomStationPostJSONRequestMapper
        {

            get
            {
                return CPOClient.CustomStationPostJSONRequestMapper;
            }

            set
            {
                CPOClient.CustomStationPostJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<StationPostResponse, StationPostResponse.Builder> CustomStationPostResponseMapper
        {

            get
            {
                return CPOClient.CustomStationPostResponseMapper;
            }

            set
            {
                CPOClient.CustomStationPostResponseMapper = value;
            }

        }

        #endregion

        #region CustomConnectorPostStatusRequestMapper

        #region CustomConnectorPostStatusRequestMapper

        public Func<ConnectorPostStatusRequest, ConnectorPostStatusRequest> CustomConnectorPostStatusRequestMapper
        {

            get
            {
                return CPOClient.CustomConnectorPostStatusRequestMapper;
            }

            set
            {
                CPOClient.CustomConnectorPostStatusRequestMapper = value;
            }

        }

        #endregion

        #region CustomConnectorPostStatusJSONRequestMapper

        public Func<ConnectorPostStatusRequest, JObject, JObject> CustomConnectorPostStatusJSONRequestMapper
        {

            get
            {
                return CPOClient.CustomConnectorPostStatusJSONRequestMapper;
            }

            set
            {
                CPOClient.CustomConnectorPostStatusJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<ConnectorPostStatusResponse, ConnectorPostStatusResponse.Builder> CustomConnectorPostStatusResponseMapper
        {

            get
            {
                return CPOClient.CustomConnectorPostStatusResponseMapper;
            }

            set
            {
                CPOClient.CustomConnectorPostStatusResponseMapper = value;
            }

        }

        #endregion


        #region CustomRFIDVerifyRequestMapper

        #region CustomRFIDVerifyRequestMapper

        public Func<RFIDVerifyRequest, RFIDVerifyRequest> CustomRFIDVerifyRequestMapper
        {

            get
            {
                return CPOClient.CustomRFIDVerifyRequestMapper;
            }

            set
            {
                CPOClient.CustomRFIDVerifyRequestMapper = value;
            }

        }

        #endregion

        #region CustomRFIDVerifyJSONRequestMapper

        public Func<RFIDVerifyRequest, JObject, JObject> CustomRFIDVerifyJSONRequestMapper
        {

            get
            {
                return CPOClient.CustomRFIDVerifyJSONRequestMapper;
            }

            set
            {
                CPOClient.CustomRFIDVerifyJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<RFIDVerifyResponse, RFIDVerifyResponse.Builder> CustomRFIDVerifyResponseMapper
        {

            get
            {
                return CPOClient.CustomRFIDVerifyResponseMapper;
            }

            set
            {
                CPOClient.CustomRFIDVerifyResponseMapper = value;
            }

        }

        #endregion

        #region CustomSessionPostRequestMapper

        #region CustomSessionPostRequestMapper

        public Func<SessionPostRequest, SessionPostRequest> CustomSessionPostRequestMapper
        {

            get
            {
                return CPOClient.CustomSessionPostRequestMapper;
            }

            set
            {
                CPOClient.CustomSessionPostRequestMapper = value;
            }

        }

        #endregion

        #region CustomSessionPostJSONRequestMapper

        public Func<SessionPostRequest, JObject, JObject> CustomSessionPostJSONRequestMapper
        {

            get
            {
                return CPOClient.CustomSessionPostJSONRequestMapper;
            }

            set
            {
                CPOClient.CustomSessionPostJSONRequestMapper = value;
            }

        }

        #endregion

        public CustomMapperDelegate<SessionPostResponse, SessionPostResponse.Builder> CustomSessionPostResponseMapper
        {

            get
            {
                return CPOClient.CustomSessionPostResponseMapper;
            }

            set
            {
                CPOClient.CustomSessionPostResponseMapper = value;
            }

        }

        #endregion

        #endregion

        #region Constructor(s)

        #region CPORoaming(CPOClient, CPOServer, ServerLoggingContext = CPOServerLogger.DefaultContext, LogFileCreator = null)

        /// <summary>
        /// Create a new OIOI roaming client for CPOs.
        /// </summary>
        /// <param name="CPOClient">A CPO client.</param>
        /// <param name="CPOServer">A CPO sever.</param>
        /// <param name="ServerLoggingContext">An optional context for logging server methods.</param>
        /// <param name="LogFileCreator">A delegate to create a log file from the given context and log file name.</param>
        public CPORoaming(CPOClient               CPOClient,
                          CPOServer               CPOServer,
                          String                  ServerLoggingContext  = CPOServerLogger.DefaultContext,
                          LogfileCreatorDelegate  LogFileCreator        = null)
        {

            this.CPOClient        = CPOClient;
            this.CPOServer        = CPOServer;
            this.CPOServerLogger  = new CPOServerLogger(CPOServer, ServerLoggingContext, LogFileCreator);

        }

        #endregion

        #region CPORoaming(ClientId, RemoteHostname, RemoteTCPPort = null, RemoteHTTPVirtualHost = null, ... )

        /// <summary>
        /// Create a new OIOI roaming client for CPOs.
        /// </summary>
        /// <param name="ClientId">A unqiue identification of this client.</param>
        /// <param name="RemoteHostname">The hostname of the remote OIOI service.</param>
        /// <param name="RemoteTCPPort">An optional TCP port of the remote OIOI service.</param>
        /// <param name="RemoteCertificateValidator">A delegate to verify the remote TLS certificate.</param>
        /// <param name="ClientCert">The TLS client certificate to use.</param>
        /// <param name="RemoteHTTPVirtualHost">An optional HTTP virtual hostname of the remote OIOI service.</param>
        /// <param name="HTTPUserAgent">An optional HTTP user agent identification string for this HTTP client.</param>
        /// <param name="RequestTimeout">An optional timeout for upstream queries.</param>
        /// <param name="MaxNumberOfRetries">The default number of maximum transmission retries.</param>
        /// 
        /// <param name="ServerName">An optional identification string for the HTTP server.</param>
        /// <param name="ServerTCPPort">An optional TCP port for the HTTP server.</param>
        /// <param name="ServerURIPrefix">An optional prefix for the HTTP URIs.</param>
        /// <param name="ServerContentType">An optional HTTP content type to use.</param>
        /// <param name="ServerRegisterHTTPRootService">Register HTTP root services for sending a notice to clients connecting via HTML or plain text.</param>
        /// <param name="ServerAutoStart">Whether to start the server immediately or not.</param>
        /// 
        /// <param name="ClientLoggingContext">An optional context for logging client methods.</param>
        /// <param name="ServerLoggingContext">An optional context for logging server methods.</param>
        /// <param name="LogFileCreator">A delegate to create a log file from the given context and log file name.</param>
        /// 
        /// <param name="DNSClient">An optional DNS client to use.</param>
        public CPORoaming(String                               ClientId,
                          String                               RemoteHostname,
                          APIKey                               APIKey,
                          Partner_Id                           DefaultPartnerId,
                          IPPort                               RemoteTCPPort                   = null,
                          RemoteCertificateValidationCallback  RemoteCertificateValidator      = null,
                          LocalCertificateSelectionCallback    LocalCertificateSelector        = null,
                          X509Certificate                      ClientCert                      = null,
                          String                               RemoteHTTPVirtualHost           = null,
                          String                               URIPrefix                       = CPOClient.DefaultURIPrefix,
                          String                               HTTPUserAgent                   = CPOClient.DefaultHTTPUserAgent,
                          IncludeStationDelegate               IncludeStation                  = null,
                          IncludeStationIdDelegate             IncludeStationId                = null,
                          IncludeConnectorIdDelegate           IncludeConnectorId              = null,
                          IncludeConnectorStatusTypesDelegate  IncludeConnectorStatusType      = null,
                          IncludeConnectorStatusDelegate       IncludeConnectorStatus          = null,
                          TimeSpan?                            RequestTimeout                  = null,
                          Byte?                                MaxNumberOfRetries              = CPOClient.DefaultMaxNumberOfRetries,

                          String                               ServerName                      = CPOServer.DefaultHTTPServerName,
                          HTTPHostname                         HTTPHostname                    = null,
                          IPPort                               ServerTCPPort                   = null,
                          X509Certificate2                     X509Certificate                 = null,
                          String                               ServerURIPrefix                 = CPOServer.DefaultURIPrefix,
                          ServerAPIKeyValidatorDelegate        ServerAPIKeyValidator           = null,
                          HTTPContentType                      ServerContentType               = null,
                          Boolean                              ServerRegisterHTTPRootService   = true,
                          Boolean                              ServerAutoStart                 = false,

                          String                               ClientLoggingContext            = CPOClient.CPOClientLogger.DefaultContext,
                          String                               ServerLoggingContext            = CPOServerLogger.DefaultContext,
                          LogfileCreatorDelegate               LogFileCreator                  = null,

                          DNSClient                            DNSClient                       = null)

            : this(new CPOClient(ClientId,
                                 RemoteHostname,
                                 APIKey,
                                 DefaultPartnerId,
                                 RemoteTCPPort,
                                 RemoteCertificateValidator,
                                 LocalCertificateSelector,
                                 ClientCert,
                                 RemoteHTTPVirtualHost,
                                 URIPrefix,
                                 HTTPUserAgent,
                                 IncludeStation,
                                 IncludeStationId,
                                 IncludeConnectorId,
                                 IncludeConnectorStatusType,
                                 IncludeConnectorStatus,
                                 RequestTimeout,
                                 MaxNumberOfRetries,
                                 DNSClient,
                                 ClientLoggingContext,
                                 LogFileCreator),

                   new CPOServer(ServerName,
                                 HTTPHostname,
                                 ServerTCPPort,
                                 X509Certificate,
                                 ServerURIPrefix,
                                 ServerAPIKeyValidator,
                                 ServerContentType,
                                 ServerRegisterHTTPRootService,
                                 DNSClient: DNSClient,
                                 Autostart: false),

                   ServerLoggingContext,
                   LogFileCreator)

        {

            if (ServerAutoStart)
                Start();

        }

        #endregion

        #endregion


        #region StationPost        (Request)

        /// <summary>
        /// Upload a charging station onto the OIOI server.
        /// </summary>
        /// <param name="Request">A StationPost request.</param>
        public Task<HTTPResponse<StationPostResponse>>

            StationPost(StationPostRequest Request)

                => CPOClient.StationPost(Request);

        #endregion

        #region ConnectorPostStatus(Request)

        /// <summary>
        /// Update the status of a charging connector on the OIOI server.
        /// </summary>
        /// <param name="Request">A StationPost request.</param>
        public Task<HTTPResponse<ConnectorPostStatusResponse>>

            ConnectorPostStatus(ConnectorPostStatusRequest Request)

                => CPOClient.ConnectorPostStatus(Request);

        #endregion

        #region RFIDVerify         (Request)

        /// <summary>
        /// Verify a RFID identification via the OIOI server.
        /// </summary>
        /// <param name="Request">A RFIDVerify request.</param>
        public Task<HTTPResponse<RFIDVerifyResponse>>

            RFIDVerify(RFIDVerifyRequest Request)

                => CPOClient.RFIDVerify(Request);

        #endregion

        #region SessionPost        (Request)

        /// <summary>
        /// Upload the given charging session onto the OIOI server.
        /// </summary>
        /// <param name="Request">A SessionPost request.</param>
        public Task<HTTPResponse<SessionPostResponse>>

            SessionPost(SessionPostRequest Request)

                => CPOClient.SessionPost(Request);

        #endregion


        #region Start()

        public void Start()
        {
            CPOServer.Start();
        }

        #endregion

        #region Shutdown(Message = null, Wait = true)

        public void Shutdown(String Message = null, Boolean Wait = true)
        {
            CPOServer.Shutdown(Message, Wait);
        }

        #endregion

        public void Dispose()
        { }

    }

}
