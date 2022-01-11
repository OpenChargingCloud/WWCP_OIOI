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

using org.GraphDefined.Vanaheimr.Illias;

using Org.BouncyCastle.Crypto.Parameters;
using org.GraphDefined.WWCP.OIOIv4_x.CPO;
using org.GraphDefined.WWCP.OIOIv4_x;

#endregion

namespace org.GraphDefined.WWCP
{

    /// <summary>
    /// Extensions methods for the WWCP wrapper for OIOI roaming clients for charging station operators.
    /// </summary>
    public static class CPOExtensions
    {

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
        public static WWCPCPOAdapter

            CreateOIOIv4_x_CPORoamingProvider(this RoamingNetwork                                          RoamingNetwork,
                                              EMPRoamingProvider_Id                                        Id,
                                              I18NString                                                   Name,
                                              I18NString                                                   Description,
                                              CPORoaming                                                   CPORoaming,

                                              OIOIv4_x.CPO.ChargingStation2StationDelegate                 ChargingStation2Station                         = null,
                                              OIOIv4_x.CPO.EVSEStatusUpdate2ConnectorStatusUpdateDelegate  EVSEStatusUpdate2ConnectorStatusUpdate          = null,
                                              OIOIv4_x.CPO.ChargeDetailRecord2SessionDelegate              ChargeDetailRecord2Session                      = null,
                                              OIOIv4_x.CPO.Station2JSONDelegate                            Station2JSON                                    = null,
                                              OIOIv4_x.CPO.ConnectorStatus2JSONDelegate                    ConnectorStatus2JSON                            = null,
                                              OIOIv4_x.CPO.Session2JSONDelegate                            Session2JSON                                    = null,

                                              IncludeEVSEIdDelegate                                        IncludeEVSEIds                                  = null,
                                              IncludeEVSEDelegate                                          IncludeEVSEs                                    = null,
                                              IncludeChargingStationIdDelegate                             IncludeChargingStationIds                       = null,
                                              IncludeChargingStationDelegate                               IncludeChargingStations                         = null,
                                              ChargeDetailRecordFilterDelegate                             ChargeDetailRecordFilter                        = null,
                                              OIOIv4_x.CPO.CustomOperatorIdMapperDelegate                  CustomOperatorIdMapper                          = null,
                                              OIOIv4_x.CPO.CustomEVSEIdMapperDelegate                      CustomEVSEIdMapper                              = null,
                                              OIOIv4_x.CPO.CustomConnectorIdMapperDelegate                 CustomConnectorIdMapper                         = null,

                                              TimeSpan?                                                    ServiceCheckEvery                               = null,
                                              TimeSpan?                                                    StatusCheckEvery                                = null,
                                              TimeSpan?                                                    CDRCheckEvery                                   = null,

                                              Boolean                                                      DisablePushData                                 = false,
                                              Boolean                                                      DisablePushStatus                               = false,
                                              Boolean                                                      DisableAuthentication                           = false,
                                              Boolean                                                      DisableSendChargeDetailRecords                  = false,

                                              Action<WWCPCPOAdapter>                                       OIOIConfigurator                                = null,
                                              Action<IEMPRoamingProvider>                                  Configurator                                    = null,

                                              String                                                       EllipticCurve                                   = "P-256",
                                              ECPrivateKeyParameters                                       PrivateKey                                      = null,
                                              PublicKeyCertificates                                        PublicKeyCertificates                           = null)

        {

            #region Initial checks

            if (RoamingNetwork == null)
                throw new ArgumentNullException(nameof(RoamingNetwork),  "The given roaming network must not be null!");

            if (Id == null)
                throw new ArgumentNullException(nameof(Id),              "The given unique roaming provider identification must not be null!");

            if (Name.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Name),            "The given roaming provider name must not be null or empty!");

            if (CPORoaming is null)
                throw new ArgumentNullException(nameof(CPORoaming),      "The given CPO roaming must not be null!");

            #endregion

            var NewRoamingProvider = new WWCPCPOAdapter(Id,
                                                        Name,
                                                        Description,
                                                        RoamingNetwork,
                                                        CPORoaming,

                                                        ChargingStation2Station,
                                                        EVSEStatusUpdate2ConnectorStatusUpdate,
                                                        ChargeDetailRecord2Session,
                                                        Station2JSON,
                                                        ConnectorStatus2JSON,
                                                        Session2JSON,

                                                        IncludeEVSEIds,
                                                        IncludeEVSEs,
                                                        IncludeChargingStationIds,
                                                        IncludeChargingStations,
                                                        ChargeDetailRecordFilter,
                                                        CustomOperatorIdMapper,
                                                        CustomEVSEIdMapper,
                                                        CustomConnectorIdMapper,

                                                        ServiceCheckEvery,
                                                        StatusCheckEvery,
                                                        CDRCheckEvery,

                                                        DisablePushData,
                                                        DisablePushStatus,
                                                        DisableAuthentication,
                                                        DisableSendChargeDetailRecords,

                                                        EllipticCurve,
                                                        PrivateKey,
                                                        PublicKeyCertificates);

            OIOIConfigurator?.Invoke(NewRoamingProvider);

            return RoamingNetwork.
                       CreateNewRoamingProvider(NewRoamingProvider,
                                                Configurator) as WWCPCPOAdapter;

        }

    }

}
