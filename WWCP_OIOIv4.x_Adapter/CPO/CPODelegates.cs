﻿/*
 * Copyright (c) 2016-2023 GraphDefined GmbH
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

using org.GraphDefined.Vanaheimr.Illias;

using cloud.charging.open.protocols.WWCP;

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x.CPO
{

    /// <summary>
    /// A delegate which allows you to customize the mapping between WWCP charging station operator identifications
    /// and OIOI charging station operator identifications.
    /// </summary>
    /// <param name="ChargingStationOperatorId">A WWCP charging station operator identification.</param>
    public delegate ChargingStationOperator_Id  CustomOperatorIdMapperDelegate                (ChargingStationOperator_Id  ChargingStationOperatorId);

    /// <summary>
    /// A delegate which allows you to customize the mapping between WWCP EVSE identifications
    /// and OIOI connector identifications.
    /// </summary>
    /// <param name="EVSEId">A WWCP EVSE identification.</param>
    public delegate Connector_Id?               CustomEVSEIdMapperDelegate                    (EVSE_Id                     EVSEId);

    /// <summary>
    /// A delegate which allows you to customize the mapping between OIOI connector identifications
    /// and WWCP EVSE identifications.
    /// </summary>
    /// <param name="ConnectorId">An OIOI connector identification.</param>
    public delegate EVSE_Id?                    CustomConnectorIdMapperDelegate               (Connector_Id                ConnectorId);

    /// <summary>
    /// A delegate which allows you to modify charging stations before sending them upstream.
    /// </summary>
    /// <param name="ChargingStation">A WWCP charging station.</param>
    /// <param name="Station">A charging station.</param>
    public delegate Station                     ChargingStation2StationDelegate               (IChargingStation            ChargingStation,
                                                                                               Station                     Station);

    /// <summary>
    /// A delegate which allows you to modify connector status records before sending them upstream.
    /// </summary>
    /// <param name="EVSEStatusUpdate">A WWCP EVSE status update.</param>
    /// <param name="ConnectorStatusUpdate">A connector status record.</param>
    public delegate ConnectorStatusUpdate       EVSEStatusUpdate2ConnectorStatusUpdateDelegate(EVSEStatusUpdate            EVSEStatusUpdate,
                                                                                               ConnectorStatusUpdate       ConnectorStatusUpdate);

    /// <summary>
    /// A delegate which allows you to modify charging sessions before sending them upstream.
    /// </summary>
    /// <param name="ChargeDetailRecord">A WWCP charge detail record.</param>
    /// <param name="Session">A charging session.</param>
    public delegate Session                     ChargeDetailRecord2SessionDelegate            (ChargeDetailRecord          ChargeDetailRecord,
                                                                                               Session                     Session);

    /// <summary>
    /// A delegate which allows you to modify charging sessions after receiving them from upstream.
    /// </summary>
    /// <param name="Session">A charging session.</param>
    /// <param name="ChargeDetailRecord">A WWCP charge detail record.</param>
    public delegate ChargeDetailRecord          Session2ChargeDetailRecordDelegate            (Session                     Session,
                                                                                               ChargeDetailRecord          ChargeDetailRecord);


    /// <summary>
    /// A delegate called whenever a new station will be send upstream.
    /// </summary>
    public delegate void OnStationPostWWCPRequestDelegate (DateTime                                        LogTimestamp,
                                                           DateTime                                        RequestTimestamp,
                                                           Object                                          Sender,
                                                           CSORoamingProvider_Id                           SenderId,
                                                           EventTracking_Id                                EventTrackingId,
                                                           RoamingNetwork_Id                               RoamingNetworkId,
                                                           UInt64                                          NumberOfStations,
                                                           IEnumerable<Tuple<IChargingStation, Station>>   Stations,
                                                           IEnumerable<Warning>                            Warnings,
                                                           TimeSpan?                                       RequestTimeout);


    /// <summary>
    /// A delegate called whenever a new station had been send upstream.
    /// </summary>
    public delegate void OnStationPostWWCPResponseDelegate(DateTime                                        LogTimestamp,
                                                           DateTime                                        RequestTimestamp,
                                                           Object                                          Sender,
                                                           CSORoamingProvider_Id                           SenderId,
                                                           EventTracking_Id                                EventTrackingId,
                                                           RoamingNetwork_Id                               RoamingNetworkId,
                                                           UInt64                                          NumberOfStations,
                                                           IEnumerable<Tuple<IChargingStation, Station>>   Stations,
                                                           TimeSpan?                                       RequestTimeout,
                                                           AddOrUpdateChargingStationsResult               Result,
                                                           TimeSpan                                        Runtime);


    /// <summary>
    /// A delegate called whenever new connector status will be send upstream.
    /// </summary>
    public delegate void OnConnectorStatusPostWWCPRequestDelegate (DateTime                                                LogTimestamp,
                                                                   DateTime                                                RequestTimestamp,
                                                                   Object                                                  Sender,
                                                                   CSORoamingProvider_Id                                   SenderId,
                                                                   EventTracking_Id                                        EventTrackingId,
                                                                   RoamingNetwork_Id                                       RoamingNetworkId,
                                                                   UInt64                                                  NumberOfConnectors,
                                                                   IEnumerable<Tuple<EVSEStatusUpdate, ConnectorStatus>>   ConnectorStatus,
                                                                   IEnumerable<Warning>                                    Warnings,
                                                                   TimeSpan?                                               RequestTimeout);


    /// <summary>
    /// A delegate called whenever new connector status had been send upstream.
    /// </summary>
    public delegate void OnConnectorStatusPostWWCPResponseDelegate(DateTime                                                LogTimestamp,
                                                                   DateTime                                                RequestTimestamp,
                                                                   Object                                                  Sender,
                                                                   CSORoamingProvider_Id                                   SenderId,
                                                                   EventTracking_Id                                        EventTrackingId,
                                                                   RoamingNetwork_Id                                       RoamingNetworkId,
                                                                   UInt64                                                  NumberOfConnectors,
                                                                   IEnumerable<Tuple<EVSEStatusUpdate, ConnectorStatus>>   ConnectorStatus,
                                                                   TimeSpan?                                               RequestTimeout,
                                                                   PushEVSEStatusResult                                    Result,
                                                                   TimeSpan                                                Runtime);

}
