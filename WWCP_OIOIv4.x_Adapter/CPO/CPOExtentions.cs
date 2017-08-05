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
using System.Security.Cryptography.X509Certificates;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

#endregion

namespace org.GraphDefined.WWCP
{

    /// <summary>
    /// Extentions methods for the WWCP wrapper for OIOI roaming clients for charging station operators.
    /// </summary>
    public static class CPOExtentions
    {

        #region CreateOIOIv4_x_CPORoamingProvider(this RoamingNetwork, Id, Name, RemoteHostname, ... , Action = null)

        /// <summary>
        /// Create and register a new electric vehicle roaming provider
        /// using the OIOI protocol and having the given unique electric
        /// vehicle roaming provider identification.
        /// </summary>
        /// 
        /// <param name="RoamingNetwork">A WWCP roaming network.</param>
        /// <param name="Id">The unique identification of the roaming provider.</param>
        /// <param name="Name">The offical (multi-language) name of the roaming provider.</param>
        /// 
        /// <param name="RemoteHostname">The hostname of the remote OIOI service.</param>
        /// <param name="RemoteTCPPort">An optional TCP port of the remote OIOI service.</param>
        /// <param name="RemoteCertificateValidator">A delegate to verify the remote TLS certificate.</param>
        /// <param name="ClientCert">The TLS client certificate to use.</param>
        /// <param name="RemoteHTTPVirtualHost">An optional HTTP virtual hostname of the remote OIOI service.</param>
        /// <param name="HTTPUserAgent">An optional HTTP user agent identification string for this HTTP client.</param>
        /// <param name="RequestTimeout">An optional timeout for upstream queries.</param>
        /// <param name="MaxNumberOfRetries">The default number of maximum transmission retries.</param>
        /// 
        /// <param name="ServerName"> An optional identification string for the HTTP server.</param>
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
        /// <param name="EVSE2Station">A delegate to process an EVSE data record, e.g. before pushing it to the roaming provider.</param>
        /// <param name="Station2JSON">A delegate to process the XML representation of an EVSE data record, e.g. before pushing it to the roaming provider.</param>
        /// 
        /// <param name="DefaultOperator">An optional Charging Station Operator, which will be copied into the main OperatorID-section of the OIOI SOAP request.</param>
        /// <param name="OperatorNameSelector">An optional delegate to select an Charging Station Operator name, which will be copied into the OperatorName-section of the OIOI SOAP request.</param>
        /// <param name="IncludeChargingStations">Only include the EVSEs matching the given delegate.</param>
        /// <param name="ServiceCheckEvery">The service check intervall.</param>
        /// <param name="StatusCheckEvery">The status check intervall.</param>
        /// 
        /// <param name="DisablePushData">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisablePushStatus">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisableAuthentication">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisableSendChargeDetailRecords">This service can be disabled, e.g. for debugging reasons.</param>
        /// 
        /// <param name="OIOIConfigurator">An optional delegate to configure the new OIOI roaming provider after its creation.</param>
        /// <param name="Configurator">An optional delegate to configure the new roaming provider after its creation.</param>
        /// <param name="DNSClient">An optional DNS client to use.</param>
        public static OIOIv4_x.CPO.WWCPCPOAdapter

            CreateOIOIv4_x_CPORoamingProvider(this RoamingNetwork                                                 RoamingNetwork,
                                              CSORoamingProvider_Id                                               Id,
                                              I18NString                                                          Name,

                                              String                                                              RemoteHostname,
                                              OIOIv4_x.APIKey                                                     APIKey,
                                              OIOIv4_x.Partner_Id                                                 DefaultPartnerId,
                                              IPPort                                                              RemoteTCPPort                                   = null,
                                              String                                                              RemoteHTTPVirtualHost                           = null,
                                              RemoteCertificateValidationCallback                                 RemoteCertificateValidator                      = null,
                                              LocalCertificateSelectionCallback                                   LocalCertificateSelector                        = null,
                                              X509Certificate                                                     ClientCert                                      = null,
                                              String                                                              URIPrefix                                       = OIOIv4_x.CPO.CPOClient.DefaultURIPrefix,
                                              String                                                              HTTPUserAgent                                   = OIOIv4_x.CPO.CPOClient.DefaultHTTPUserAgent,
                                              OIOIv4_x.IncludeStationDelegate                                     IncludeStation                                  = null,
                                              OIOIv4_x.IncludeStationIdDelegate                                   IncludeStationId                                = null,
                                              OIOIv4_x.IncludeConnectorIdDelegate                                 IncludeConnectorId                              = null,
                                              OIOIv4_x.IncludeConnectorStatusTypesDelegate                        IncludeConnectorStatusType                      = null,
                                              OIOIv4_x.IncludeConnectorStatusDelegate                             IncludeConnectorStatus                          = null,
                                              TimeSpan?                                                           RequestTimeout                                  = null,
                                              Byte?                                                               MaxNumberOfRetries                              = OIOIv4_x.CPO.CPOClient.DefaultMaxNumberOfRetries,

                                              String                                                              ServerName                                      = OIOIv4_x.CPO.CPOServer.DefaultHTTPServerName,
                                              HTTPHostname                                                        HTTPHostname                                    = null,
                                              IPPort                                                              ServerTCPPort                                   = null,
                                              X509Certificate2                                                    X509Certificate                                 = null,
                                              String                                                              ServerURIPrefix                                 = OIOIv4_x.CPO.CPOServer.DefaultURIPrefix,
                                              OIOIv4_x.CPO.ServerAPIKeyValidatorDelegate                          ServerAPIKeyValidator                           = null,
                                              HTTPContentType                                                     ServerContentType                               = null,
                                              Boolean                                                             ServerRegisterHTTPRootService                   = true,
                                              Boolean                                                             ServerAutoStart                                 = false,

                                              String                                                              ClientLoggingContext                            = OIOIv4_x.CPO.CPOClient.CPOClientLogger.DefaultContext,
                                              String                                                              ServerLoggingContext                            = OIOIv4_x.CPO.CPOServerLogger.DefaultContext,
                                              LogfileCreatorDelegate                                              LogFileCreator                                  = null,

                                              OIOIv4_x.CPO.CustomOperatorIdMapperDelegate                         CustomOperatorIdMapper                          = null,
                                              OIOIv4_x.CPO.CustomEVSEIdMapperDelegate                             CustomEVSEIdMapper                              = null,
                                              OIOIv4_x.CPO.ChargingStation2StationDelegate                        EVSE2Station                                    = null,
                                              OIOIv4_x.CPO.EVSEStatusUpdate2ConnectorStatusUpdateDelegate         EVSEStatusUpdate2ConnectorStatusUpdate          = null,
                                              OIOIv4_x.CPO.ChargeDetailRecord2SessionDelegate                     WWCPChargeDetailRecord2OIOIChargeDetailRecord   = null,
                                              OIOIv4_x.CPO.Station2JSONDelegate                                   Station2JSON                                    = null,
                                              OIOIv4_x.CPO.ConnectorStatus2JSONDelegate                           ConnectorStatus2JSON                            = null,
                                              OIOIv4_x.CPO.Session2JSONDelegate                                   ChargeDetailRecord2JSON                         = null,

                                              ChargingStationOperator                                             DefaultOperator                                 = null,
                                              ChargingStationOperatorNameSelectorDelegate                         OperatorNameSelector                            = null,
                                              IncludeChargingStationDelegate                                      IncludeChargingStations                         = null,
                                              TimeSpan?                                                           ServiceCheckEvery                               = null,
                                              TimeSpan?                                                           StatusCheckEvery                                = null,

                                              Boolean                                                             DisablePushData                                 = false,
                                              Boolean                                                             DisablePushStatus                               = false,
                                              Boolean                                                             DisableAuthentication                           = false,
                                              Boolean                                                             DisableSendChargeDetailRecords                  = false,

                                              Action<OIOIv4_x.CPO.WWCPCPOAdapter>                                 OIOIConfigurator                                = null,
                                              Action<ICSORoamingProvider>                                         Configurator                                    = null,
                                              DNSClient                                                           DNSClient                                       = null)

        {

            #region Initial checks

            if (RoamingNetwork    == null)
                throw new ArgumentNullException(nameof(RoamingNetwork),  "The given roaming network must not be null!");

            if (Id == null)
                throw new ArgumentNullException(nameof(Id),              "The given unique roaming provider identification must not be null!");

            if (Name.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Name),            "The given roaming provider name must not be null or empty!");

            if (RemoteHostname    == null)
                throw new ArgumentNullException(nameof(RemoteHostname),  "The given remote hostname must not be null!");

            #endregion

            var NewRoamingProvider = new OIOIv4_x.CPO.WWCPCPOAdapter(Id,
                                                                     Name,
                                                                     RoamingNetwork,

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

                                                                     ServerName,
                                                                     HTTPHostname,
                                                                     ServerTCPPort,
                                                                     X509Certificate,
                                                                     ServerURIPrefix,
                                                                     ServerAPIKeyValidator,
                                                                     ServerContentType,
                                                                     ServerRegisterHTTPRootService,
                                                                     ServerAutoStart,

                                                                     ClientLoggingContext,
                                                                     ServerLoggingContext,
                                                                     LogFileCreator,

                                                                     CustomOperatorIdMapper,
                                                                     CustomEVSEIdMapper,
                                                                     EVSE2Station,
                                                                     EVSEStatusUpdate2ConnectorStatusUpdate,
                                                                     WWCPChargeDetailRecord2OIOIChargeDetailRecord,
                                                                     Station2JSON,
                                                                     ConnectorStatus2JSON,
                                                                     ChargeDetailRecord2JSON,

                                                                     IncludeChargingStations,

                                                                     ServiceCheckEvery,
                                                                     StatusCheckEvery,

                                                                     DisablePushData,
                                                                     DisablePushStatus,
                                                                     DisableAuthentication,
                                                                     DisableSendChargeDetailRecords,

                                                                     DNSClient);


            OIOIConfigurator?.Invoke(NewRoamingProvider);

            return RoamingNetwork.
                       CreateNewRoamingProvider(NewRoamingProvider,
                                                Configurator) as OIOIv4_x.CPO.WWCPCPOAdapter;

        }

        #endregion

        #region CreateOIOIv4_x_CPORoamingProvider(this RoamingNetwork, Id, Name, HTTPServer, RemoteHostname, ...)

        /// <summary>
        /// Create and register a new electric vehicle roaming provider
        /// using the OIOI protocol and having the given unique electric
        /// vehicle roaming provider identification.
        /// </summary>
        /// 
        /// <param name="RoamingNetwork">A WWCP roaming network.</param>
        /// <param name="Id">The unique identification of the roaming provider.</param>
        /// <param name="Name">The offical (multi-language) name of the roaming provider.</param>
        /// <param name="HTTPServer">An optional identification string for the HTTP server.</param>
        /// <param name="ServerURIPrefix">An optional prefix for the HTTP URIs.</param>
        /// 
        /// <param name="RemoteHostname">The hostname of the remote OIOI service.</param>
        /// <param name="RemoteTCPPort">An optional TCP port of the remote OIOI service.</param>
        /// <param name="RemoteCertificateValidator">A delegate to verify the remote TLS certificate.</param>
        /// <param name="ClientCert">The TLS client certificate to use.</param>
        /// <param name="RemoteHTTPVirtualHost">An optional HTTP virtual hostname of the remote OIOI service.</param>
        /// <param name="HTTPUserAgent">An optional HTTP user agent identification string for this HTTP client.</param>
        /// <param name="RequestTimeout">An optional timeout for upstream queries.</param>
        /// <param name="MaxNumberOfRetries">The default number of maximum transmission retries.</param>
        /// 
        /// <param name="ClientLoggingContext">An optional context for logging client methods.</param>
        /// <param name="ServerLoggingContext">An optional context for logging server methods.</param>
        /// <param name="LogFileCreator">A delegate to create a log file from the given context and log file name.</param>
        /// 
        /// <param name="EVSE2EVSEDataRecord">A delegate to process an EVSE data record, e.g. before pushing it to the roaming provider.</param>
        /// <param name="EVSEDataRecord2XML">A delegate to process the XML representation of an EVSE data record, e.g. before pushing it to the roaming provider.</param>
        /// 
        /// <param name="DefaultOperator">An optional Charging Station Operator, which will be copied into the main OperatorID-section of the OIOI SOAP request.</param>
        /// <param name="OperatorNameSelector">An optional delegate to select an Charging Station Operator name, which will be copied into the OperatorName-section of the OIOI SOAP request.</param>
        /// <param name="IncludeChargingStations">Only include the EVSEs matching the given delegate.</param>
        /// <param name="ServiceCheckEvery">The service check intervall.</param>
        /// <param name="StatusCheckEvery">The status check intervall.</param>
        /// 
        /// <param name="DisablePushData">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisablePushStatus">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisableAuthentication">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisableSendChargeDetailRecords">This service can be disabled, e.g. for debugging reasons.</param>
        /// 
        /// <param name="OIOIConfigurator">An optional delegate to configure the new OIOI roaming provider after its creation.</param>
        /// <param name="Configurator">An optional delegate to configure the new roaming provider after its creation.</param>
        /// <param name="DNSClient">An optional DNS client to use.</param>
        public static OIOIv4_x.CPO.WWCPCPOAdapter

            CreateOIOIv4_x_CPORoamingProvider(this RoamingNetwork                                          RoamingNetwork,
                                              CSORoamingProvider_Id                                        Id,
                                              I18NString                                                   Name,
                                              HTTPServer<RoamingNetworks, RoamingNetwork>                  HTTPServer,

                                              String                                                       RemoteHostname,
                                              OIOIv4_x.APIKey                                              APIKey,
                                              OIOIv4_x.Partner_Id                                          DefaultPartnerId,

                                              IPPort                                                       RemoteTCPPort                                   = null,
                                              RemoteCertificateValidationCallback                          RemoteCertificateValidator                      = null,
                                              LocalCertificateSelectionCallback                            LocalCertificateSelector                        = null,
                                              X509Certificate                                              ClientCert                                      = null,
                                              String                                                       RemoteHTTPVirtualHost                           = null,
                                              String                                                       URIPrefix                                       = OIOIv4_x.CPO.CPOClient.DefaultURIPrefix,
                                              String                                                       HTTPUserAgent                                   = OIOIv4_x.CPO.CPOClient.DefaultHTTPUserAgent,
                                              OIOIv4_x.IncludeStationDelegate                              IncludeStation                                  = null,
                                              OIOIv4_x.IncludeStationIdDelegate                            IncludeStationId                                = null,
                                              OIOIv4_x.IncludeConnectorIdDelegate                          IncludeConnectorId                              = null,
                                              OIOIv4_x.IncludeConnectorStatusTypesDelegate                 IncludeConnectorStatusType                      = null,
                                              OIOIv4_x.IncludeConnectorStatusDelegate                      IncludeConnectorStatus                          = null,
                                              TimeSpan?                                                    RequestTimeout                                  = null,
                                              Byte?                                                        MaxNumberOfRetries                              = OIOIv4_x.CPO.CPOClient.DefaultMaxNumberOfRetries,

                                              HTTPHostname                                                 HTTPHostname                                    = null,
                                              String                                                       ServerURIPrefix                                 = null,
                                              OIOIv4_x.CPO.ServerAPIKeyValidatorDelegate                   ServerAPIKeyValidator                           = null,
                                              HTTPContentType                                              ServerContentType                               = null,
                                              Boolean                                                      ServerRegisterHTTPRootService                   = true,

                                              String                                                       ClientLoggingContext                            = OIOIv4_x.CPO.CPOClient.CPOClientLogger.DefaultContext,
                                              String                                                       ServerLoggingContext                            = OIOIv4_x.CPO.CPOServerLogger.DefaultContext,
                                              LogfileCreatorDelegate                                       LogFileCreator                                  = null,

                                              OIOIv4_x.CPO.CustomOperatorIdMapperDelegate                  CustomOperatorIdMapper                          = null,
                                              OIOIv4_x.CPO.CustomEVSEIdMapperDelegate                      CustomEVSEIdMapper                              = null,
                                              OIOIv4_x.CPO.ChargingStation2StationDelegate                 EVSE2Station                                    = null,
                                              OIOIv4_x.CPO.EVSEStatusUpdate2ConnectorStatusUpdateDelegate  EVSEStatusUpdate2ConnectorStatusUpdate          = null,
                                              OIOIv4_x.CPO.ChargeDetailRecord2SessionDelegate              WWCPChargeDetailRecord2OIOIChargeDetailRecord   = null,
                                              OIOIv4_x.CPO.Station2JSONDelegate                            Station2JSON                                    = null,
                                              OIOIv4_x.CPO.ConnectorStatus2JSONDelegate                    ConnectorStatus2JSON                            = null,
                                              OIOIv4_x.CPO.Session2JSONDelegate                            ChargeDetailRecord2JSON                         = null,

                                              IncludeChargingStationDelegate                               IncludeChargingStations                         = null,

                                              TimeSpan?                                                    ServiceCheckEvery                               = null,
                                              TimeSpan?                                                    StatusCheckEvery                                = null,

                                              Boolean                                                      DisablePushData                                 = false,
                                              Boolean                                                      DisablePushStatus                               = false,
                                              Boolean                                                      DisableAuthentication                           = false,
                                              Boolean                                                      DisableSendChargeDetailRecords                  = false,

                                              Action<OIOIv4_x.CPO.WWCPCPOAdapter>                          OIOIConfigurator                                = null,
                                              Action<ICSORoamingProvider>                                  Configurator                                    = null,
                                              DNSClient                                                    DNSClient                                       = null)

        {

            #region Initial checks

            if (HTTPServer == null)
                throw new ArgumentNullException(nameof(HTTPServer),      "The given HTTP server must not be null!");


            if (RoamingNetwork == null)
                throw new ArgumentNullException(nameof(RoamingNetwork),  "The given roaming network must not be null!");

            if (Id == null)
                throw new ArgumentNullException(nameof(Id),              "The given unique roaming provider identification must not be null!");

            if (Name.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Name),            "The given roaming provider name must not be null or empty!");

            if (RemoteHostname == null)
                throw new ArgumentNullException(nameof(RemoteHostname),  "The given remote hostname must not be null!");

            #endregion

            var NewRoamingProvider = new OIOIv4_x.CPO.WWCPCPOAdapter(Id,
                                                                     Name,
                                                                     RoamingNetwork,

                                                                     new OIOIv4_x.CPO.CPOClient(Id.ToString(),
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

                                                                     new OIOIv4_x.CPO.CPOServer(HTTPServer,
                                                                                                HTTPHostname,
                                                                                                ServerURIPrefix,
                                                                                                ServerAPIKeyValidator,
                                                                                                ServerContentType,
                                                                                                ServerRegisterHTTPRootService),

                                                                     ServerLoggingContext,
                                                                     LogFileCreator,

                                                                     CustomOperatorIdMapper,
                                                                     CustomEVSEIdMapper,
                                                                     EVSE2Station,
                                                                     EVSEStatusUpdate2ConnectorStatusUpdate,
                                                                     WWCPChargeDetailRecord2OIOIChargeDetailRecord,
                                                                     Station2JSON,
                                                                     ConnectorStatus2JSON,
                                                                     ChargeDetailRecord2JSON,

                                                                     IncludeChargingStations,
                                                                     ServiceCheckEvery,
                                                                     StatusCheckEvery,

                                                                     DisablePushData,
                                                                     DisablePushStatus,
                                                                     DisableAuthentication,
                                                                     DisableSendChargeDetailRecords);

            OIOIConfigurator?.Invoke(NewRoamingProvider);

            return RoamingNetwork.
                       CreateNewRoamingProvider(NewRoamingProvider,
                                                Configurator) as OIOIv4_x.CPO.WWCPCPOAdapter;

        }

        #endregion

    }

}
