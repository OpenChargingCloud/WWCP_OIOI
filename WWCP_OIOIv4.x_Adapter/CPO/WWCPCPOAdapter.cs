/*
 * Copyright (c) 2016-2019 GraphDefined GmbH
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
using System.Security.Authentication;
using System.Text.RegularExpressions;

using Org.BouncyCastle.Bcpg.OpenPgp;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.Sockets.TCP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

    /// <summary>
    /// A WWCP wrapper for the OIOI CPO Roaming client which maps
    /// WWCP data structures onto OIOI data structures and vice versa.
    /// </summary>
    public class WWCPCPOAdapter : AWWCPCSOAdapter,
                                  ICSORoamingProvider,
                                  IEquatable <WWCPCPOAdapter>,
                                  IComparable<WWCPCPOAdapter>,
                                  IComparable
    {

        #region Data

        //private        readonly  CustomOperatorIdMapperDelegate                   _CustomOperatorIdMapper;

        //private        readonly  CustomEVSEIdMapperDelegate                      _CustomEVSEIdMapper;

        //private        readonly  ChargingStation2StationDelegate                  _ChargingStation2Station;

        private        readonly  EVSEStatusUpdate2ConnectorStatusUpdateDelegate   _EVSEStatusUpdate2ConnectorStatusUpdateDelegate;

        private        readonly  ChargeDetailRecord2SessionDelegate               _ChargeDetailRecord2Session;

        private        readonly  Station2JSONDelegate                             _Station2JSON;

        private        readonly  ConnectorStatus2JSONDelegate                     _ConnectorStatus2JSON;

        private        readonly  Session2JSONDelegate                             _Session2JSON;

        private static readonly  Regex                                            pattern                      = new Regex(@"\s=\s");


        private readonly        HashSet<ChargingStation>                          StationsToAddQueue;
        private readonly        HashSet<ChargingStation>                          StationsToUpdateQueue;
        private readonly        HashSet<ChargingStation>                          StationsToRemoveQueue;
        private readonly        List<EVSEStatusUpdate>                            EVSEStatusUpdatesQueue;
        private readonly        List<EVSEStatusUpdate>                            EVSEStatusUpdatesDelayedQueue;
        private readonly        List<ChargeDetailRecord>                          ChargeDetailRecordsQueue;

        //private                 UInt64                                            _ServiceRunId;
        //private                 UInt64                                            _StatusRunId;
        private readonly        IncludeChargingStationDelegate                    _IncludeChargingStations;

        public readonly static  TimeSpan                                          DefaultRequestTimeout  = TimeSpan.FromSeconds(30);
        public readonly static  eMobilityProvider_Id                              DefaultProviderId      = eMobilityProvider_Id.Parse("DE*8PS");





        private readonly List<Session> OICP_ChargeDetailRecords_Queue;
        protected readonly SemaphoreSlim FlushOICPChargeDetailRecordsLock = new SemaphoreSlim(1, 1);

        #endregion

        #region Properties

        IId ISendAuthorizeStartStop.AuthId
            => Id;

        IId ISendChargeDetailRecords.Id
            => Id;

        IEnumerable<IId> ISendChargeDetailRecords.Ids
            => Ids.Cast<IId>();


        /// <summary>
        /// The wrapped CPO roaming object.
        /// </summary>
        public CPORoaming CPORoaming { get; }


        /// <summary>
        /// The CPO client.
        /// </summary>
        public CPOClient CPOClient
            => CPORoaming?.CPOClient;

        /// <summary>
        /// The CPO client logger.
        /// </summary>
        public CPOClient.CPOClientLogger ClientLogger
            => CPORoaming?.CPOClient?.Logger;


        /// <summary>
        /// The CPO server.
        /// </summary>
        public CPOServer CPOServer
            => CPORoaming?.CPOServer;

        /// <summary>
        /// The CPO server logger.
        /// </summary>
        public CPOServerLogger ServerLogger
            => CPORoaming?.CPOServerLogger;

        protected readonly CustomOperatorIdMapperDelegate   CustomOperatorIdMapper;
        protected readonly CustomEVSEIdMapperDelegate       CustomEVSEIdMapper;
        protected readonly ChargingStation2StationDelegate  ChargingStation2Station;

        private Int32 _maxDegreeOfParallelism = 4;

        /// <summary>
        /// The max degree of parallelism for uploads.
        /// </summary>
        public UInt32 maxDegreeOfParallelism
        {

            get
            {
                return (UInt32) _maxDegreeOfParallelism;
            }

            set
            {
                this._maxDegreeOfParallelism = (Int32) (value >= 1 ? Math.Min(value, UInt32.MaxValue) : 1);
            }

        }

        #endregion

        #region Events

        // Client logging...

        #region OnStationPostWWCPRequest/-Response

        /// <summary>
        /// An event fired whenever new charging station data will be send upstream.
        /// </summary>
        public event OnStationPostWWCPRequestDelegate   OnStationPostWWCPRequest;

        /// <summary>
        /// An event fired whenever new charging station data had been sent upstream.
        /// </summary>
        public event OnStationPostWWCPResponseDelegate  OnStationPostWWCPResponse;

        #endregion

        #region OnConnectorStatusPostRequest/-Response

        /// <summary>
        /// An event fired whenever new connector status will be send upstream.
        /// </summary>
        public event OnConnectorStatusPostWWCPRequestDelegate   OnConnectorStatusPostRequest;

        /// <summary>
        /// An event fired whenever new connector status had been sent upstream.
        /// </summary>
        public event OnConnectorStatusPostWWCPResponseDelegate  OnConnectorStatusPostResponse;

        #endregion


        #region OnAuthorizeStartRequest/-Response

        /// <summary>
        /// An event fired whenever an authentication token will be verified for charging.
        /// </summary>
        public event OnAuthorizeStartRequestDelegate                  OnAuthorizeStartRequest;

        /// <summary>
        /// An event fired whenever an authentication token had been verified for charging.
        /// </summary>
        public event OnAuthorizeStartResponseDelegate                 OnAuthorizeStartResponse;


        /// <summary>
        /// An event fired whenever an authentication token will be verified for charging at the given EVSE.
        /// </summary>
        public event OnAuthorizeEVSEStartRequestDelegate              OnAuthorizeEVSEStartRequest;

        /// <summary>
        /// An event fired whenever an authentication token had been verified for charging at the given EVSE.
        /// </summary>
        public event OnAuthorizeEVSEStartResponseDelegate             OnAuthorizeEVSEStartResponse;


        /// <summary>
        /// An event fired whenever an authentication token will be verified for charging at the given charging station.
        /// </summary>
        public event OnAuthorizeChargingStationStartRequestDelegate   OnAuthorizeChargingStationStartRequest;

        /// <summary>
        /// An event fired whenever an authentication token had been verified for charging at the given charging station.
        /// </summary>
        public event OnAuthorizeChargingStationStartResponseDelegate  OnAuthorizeChargingStationStartResponse;


        /// <summary>
        /// An event fired whenever an authentication token will be verified for charging at the given charging pool.
        /// </summary>
        public event OnAuthorizeChargingPoolStartRequestDelegate      OnAuthorizeChargingPoolStartRequest;

        /// <summary>
        /// An event fired whenever an authentication token had been verified for charging at the given charging pool.
        /// </summary>
        public event OnAuthorizeChargingPoolStartResponseDelegate     OnAuthorizeChargingPoolStartResponse;

        #endregion

        #region OnAuthorizeStopRequest/-Response

        /// <summary>
        /// An event fired whenever an authentication token will be verified to stop a charging process.
        /// </summary>
        public event OnAuthorizeStopRequestDelegate                  OnAuthorizeStopRequest;

        /// <summary>
        /// An event fired whenever an authentication token had been verified to stop a charging process.
        /// </summary>
        public event OnAuthorizeStopResponseDelegate                 OnAuthorizeStopResponse;


        /// <summary>
        /// An event fired whenever an authentication token will be verified to stop a charging process at the given EVSE.
        /// </summary>
        public event OnAuthorizeEVSEStopRequestDelegate              OnAuthorizeEVSEStopRequest;

        /// <summary>
        /// An event fired whenever an authentication token had been verified to stop a charging process at the given EVSE.
        /// </summary>
        public event OnAuthorizeEVSEStopResponseDelegate             OnAuthorizeEVSEStopResponse;


        /// <summary>
        /// An event fired whenever an authentication token will be verified to stop a charging process at the given charging station.
        /// </summary>
        public event OnAuthorizeChargingStationStopRequestDelegate   OnAuthorizeChargingStationStopRequest;

        /// <summary>
        /// An event fired whenever an authentication token had been verified to stop a charging process at the given charging station.
        /// </summary>
        public event OnAuthorizeChargingStationStopResponseDelegate  OnAuthorizeChargingStationStopResponse;


        /// <summary>
        /// An event fired whenever an authentication token will be verified to stop a charging process at the given charging pool.
        /// </summary>
        public event OnAuthorizeChargingPoolStopRequestDelegate      OnAuthorizeChargingPoolStopRequest;

        /// <summary>
        /// An event fired whenever an authentication token had been verified to stop a charging process at the given charging pool.
        /// </summary>
        public event OnAuthorizeChargingPoolStopResponseDelegate     OnAuthorizeChargingPoolStopResponse;

        #endregion

        #region OnSendCDRRequest/-Response

        /// <summary>
        /// An event fired whenever a charge detail record was enqueued for later sending upstream.
        /// </summary>
        public event OnSendCDRRequestDelegate   OnEnqueueSendCDRsRequest;

        /// <summary>
        /// An event fired whenever a charge detail record will be send upstream.
        /// </summary>
        public event OnSendCDRRequestDelegate   OnSendCDRsRequest;

        /// <summary>
        /// An event fired whenever a charge detail record had been sent upstream.
        /// </summary>
        public event OnSendCDRResponseDelegate  OnSendCDRsResponse;

        #endregion


        #region OnWWCPCPOAdapterException

        public delegate Task OnWWCPCPOAdapterExceptionDelegate(DateTime        Timestamp,
                                                               WWCPCPOAdapter  Sender,
                                                               Exception       Exception);

        public event OnWWCPCPOAdapterExceptionDelegate OnWWCPCPOAdapterException;

        #endregion


        public delegate void FlushServiceQueuesDelegate(WWCPCPOAdapter Sender, TimeSpan Every);

        public event FlushServiceQueuesDelegate FlushServiceQueuesEvent;

        public event FlushServiceQueuesDelegate FlushFastStatusQueuesEvent;


        #region Missing events

        event OnAuthorizeStartRequestDelegate ISendAuthorizeStartStop.OnAuthorizeStartRequest
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeStartResponseDelegate ISendAuthorizeStartStop.OnAuthorizeStartResponse
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeEVSEStartRequestDelegate ISendAuthorizeStartStop.OnAuthorizeEVSEStartRequest
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeEVSEStartResponseDelegate ISendAuthorizeStartStop.OnAuthorizeEVSEStartResponse
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeChargingStationStartRequestDelegate ISendAuthorizeStartStop.OnAuthorizeChargingStationStartRequest
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeChargingStationStartResponseDelegate ISendAuthorizeStartStop.OnAuthorizeChargingStationStartResponse
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeStopRequestDelegate ISendAuthorizeStartStop.OnAuthorizeStopRequest
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeStopResponseDelegate ISendAuthorizeStartStop.OnAuthorizeStopResponse
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeEVSEStopRequestDelegate ISendAuthorizeStartStop.OnAuthorizeEVSEStopRequest
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeEVSEStopResponseDelegate ISendAuthorizeStartStop.OnAuthorizeEVSEStopResponse
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeChargingStationStopRequestDelegate ISendAuthorizeStartStop.OnAuthorizeChargingStationStopRequest
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event OnAuthorizeChargingStationStopResponseDelegate ISendAuthorizeStartStop.OnAuthorizeChargingStationStopResponse
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #endregion

        #region Constructor(s)

        #region WWCPCPOAdapter(Id, Name, RoamingNetwork, CPORoaming, ...)

        /// <summary>
        /// Create a new WWCP wrapper for the OIOI roaming client for Charging Station Operators/CPOs.
        /// </summary>
        /// <param name="Id">The unique identification of the roaming provider.</param>
        /// <param name="Name">The offical (multi-language) name of the roaming provider.</param>
        /// <param name="Description">An optional (multi-language) description of the charging station operator roaming provider.</param>
        /// <param name="RoamingNetwork">A WWCP roaming network.</param>
        /// 
        /// <param name="ChargingStation2Station">A delegate customize the convertion from a WWCP charging station into an OIOI station.</param>
        /// <param name="EVSEStatusUpdate2ConnectorStatusUpdate">A delegate to customize the convertion from a WWCP EVSE status update into an OIOI connector status update.</param>
        /// <param name="ChargeDetailRecord2Session">A delegate to customize the convertion from a WWCP charge detail record into an OIOI session.</param>
        /// 
        /// <param name="Station2JSON">A delegate to process the JSON representation of an OIOI station, e.g. before uploading it.</param>
        /// <param name="ConnectorStatus2JSON">A delegate to process the JSON representation of an OIOI connector status, e.g. before uploading it.</param>
        /// <param name="Session2JSON">A delegate to process the JSON representation of an OIOI session, e.g. before uploading it.</param>
        /// 
        /// <param name="IncludeChargingStations">Only include the charging stations matching the given delegate.</param>
        /// 
        /// <param name="ServiceCheckEvery">The service check intervall.</param>
        /// <param name="StatusCheckEvery">The status check intervall.</param>
        /// 
        /// <param name="DisablePushData">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisablePushStatus">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisableAuthentication">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisableSendChargeDetailRecords">This service can be disabled, e.g. for debugging reasons.</param>
        public WWCPCPOAdapter(CSORoamingProvider_Id                           Id,
                              I18NString                                      Name,
                              I18NString                                      Description,
                              RoamingNetwork                                  RoamingNetwork,

                              CPORoaming                                      CPORoaming,
                              CustomOperatorIdMapperDelegate                  CustomOperatorIdMapper                   = null,
                              //CustomEVSEIdMapperDelegate                      CustomEVSEIdMapper                       = null,
                              ChargingStation2StationDelegate                 ChargingStation2Station                  = null,
                              EVSEStatusUpdate2ConnectorStatusUpdateDelegate  EVSEStatusUpdate2ConnectorStatusUpdate   = null,
                              ChargeDetailRecord2SessionDelegate              ChargeDetailRecord2Session               = null,

                              Station2JSONDelegate                            Station2JSON                             = null,
                              ConnectorStatus2JSONDelegate                    ConnectorStatus2JSON                     = null,
                              Session2JSONDelegate                            Session2JSON                             = null,

                              IncludeChargingStationDelegate                  IncludeChargingStations                  = null,

                              IncludeEVSEIdDelegate                           IncludeEVSEIds                           = null,
                              IncludeEVSEDelegate                             IncludeEVSEs                             = null,
                              CustomEVSEIdMapperDelegate                      CustomEVSEIdMapper                       = null,

                              TimeSpan?                                       ServiceCheckEvery                        = null,
                              TimeSpan?                                       StatusCheckEvery                         = null,
                              TimeSpan?                                       CDRCheckEvery                            = null,

                              Boolean                                         DisablePushData                          = false,
                              Boolean                                         DisablePushStatus                        = false,
                              Boolean                                         DisableAuthentication                    = false,
                              Boolean                                         DisableSendChargeDetailRecords           = false,

                              PgpPublicKeyRing                                PublicKeyRing                            = null,
                              PgpSecretKeyRing                                SecretKeyRing                            = null,
                              DNSClient                                       DNSClient                                = null)

            : base(Id,
                   Name,
                   Description,
                   RoamingNetwork,

                   IncludeEVSEIds,
                   IncludeEVSEs,

                   ServiceCheckEvery,
                   StatusCheckEvery,
                   CDRCheckEvery,

                   DisablePushData,
                   DisablePushStatus,
                   DisableAuthentication,
                   DisableSendChargeDetailRecords,

                   PublicKeyRing,
                   SecretKeyRing,
                   DNSClient ?? CPORoaming?.DNSClient)

        {

            #region Initial checks

            if (Name.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Name),        "The given roaming provider name must not be null or empty!");

            if (CPORoaming == null)
                throw new ArgumentNullException(nameof(CPORoaming),  "The given OIOI CPO Roaming object must not be null!");

            #endregion

            this.CPORoaming                                        = CPORoaming;
            this.CustomOperatorIdMapper                            = CustomOperatorIdMapper;
            this.CustomEVSEIdMapper                                = CustomEVSEIdMapper;
            this.ChargingStation2Station                           = ChargingStation2Station;
            this._EVSEStatusUpdate2ConnectorStatusUpdateDelegate   = EVSEStatusUpdate2ConnectorStatusUpdate;
            this._ChargeDetailRecord2Session                       = ChargeDetailRecord2Session;
            this._Station2JSON                                     = Station2JSON;
            this._ConnectorStatus2JSON                             = ConnectorStatus2JSON;
            this._Session2JSON                                     = Session2JSON;

            this._IncludeChargingStations                          = IncludeChargingStations;

            this.DisablePushData                                   = DisablePushData;
            this.DisablePushStatus                                 = DisablePushStatus;
            this.DisableAuthentication                             = DisableAuthentication;
            this.DisableSendChargeDetailRecords                    = DisableSendChargeDetailRecords;

            this.StationsToAddQueue                                = new HashSet<ChargingStation>();
            this.StationsToUpdateQueue                             = new HashSet<ChargingStation>();
            this.StationsToRemoveQueue                             = new HashSet<ChargingStation>();
            this.EVSEStatusUpdatesQueue                            = new List<EVSEStatusUpdate>();
            this.EVSEStatusUpdatesDelayedQueue                     = new List<EVSEStatusUpdate>();
            this.ChargeDetailRecordsQueue                          = new List<ChargeDetailRecord>();

            // Link events...

            #region OnRemoteStart

            this.CPORoaming.CPOServer.OnSessionStart += async (Timestamp,
                                                               Sender,
                                                               eMAId,
                                                               ConnectorId,
                                                               PaymentReference,
                                                               CancellationToken,
                                                               EventTrackingId,
                                                               RequestTimeout) => {


                var response = await RoamingNetwork.
                                         RemoteStart(EVSEId:             ConnectorId.ToWWCP(),
                                                     eMAId:              eMAId,
                                                     SessionId:          ChargingSession_Id.New,

                                                     Timestamp:          Timestamp,
                                                     CancellationToken:  CancellationToken,
                                                     EventTrackingId:    EventTrackingId,
                                                     RequestTimeout:     RequestTimeout).ConfigureAwait(false);

            //    #region Response mapping

            //    if (response != null)
            //    {
            //        switch (response.Result)
            //        {

            //            case RemoteStartEVSEResultType.Success:
            //                return Acknowledgement.Success(
            //                           response.Session.Id.ToOIOI(),
            //                           StatusCodeDescription: "Ready to charge!"
            //                       );

            //            case RemoteStartEVSEResultType.InvalidSessionId:
            //                return Acknowledgement.SessionIsInvalid(
            //                           SessionId: SessionId
            //                       );

            //            case RemoteStartEVSEResultType.InvalidCredentials:
            //                return Acknowledgement.NoValidContract();

            //            case RemoteStartEVSEResultType.Offline:
            //                return Acknowledgement.CommunicationToEVSEFailed();

            //            case RemoteStartEVSEResultType.Timeout:
            //            case RemoteStartEVSEResultType.CommunicationError:
            //                return Acknowledgement.CommunicationToEVSEFailed();

            //            case RemoteStartEVSEResultType.Reserved:
            //                return Acknowledgement.EVSEAlreadyReserved();

            //            case RemoteStartEVSEResultType.AlreadyInUse:
            //                return Acknowledgement.EVSEAlreadyInUse_WrongToken();

            //            case RemoteStartEVSEResultType.UnknownEVSE:
            //                return Acknowledgement.UnknownEVSEID();

            //            case RemoteStartEVSEResultType.OutOfService:
            //                return Acknowledgement.EVSEOutOfService();

            //        }
            //    }

            //    return Acknowledgement.ServiceNotAvailable(
            //               SessionId: SessionId
            //           );

            //    #endregion

                return response;

            };

            #endregion

            #region OnRemoteStop

            this.CPORoaming.CPOServer.OnSessionStop += async (Timestamp,
                                                              Sender,
                                                              ConnectorId,
                                                              SessionId,
                                                              eMAId,
                                                              CancellationToken,
                                                              EventTrackingId,
                                                              RequestTimeout) => {

                var response = await RoamingNetwork.RemoteStop(SessionId:            SessionId. ToWWCP(),
                                                               EVSEId:               ConnectorId.ToWWCP(),
                                                               eMAId:                eMAId,
                                                               ReservationHandling:  ReservationHandling.Close,

                                                               Timestamp:            Timestamp,
                                                               CancellationToken:    CancellationToken,
                                                               EventTrackingId:      EventTrackingId,
                                                               RequestTimeout:       RequestTimeout).ConfigureAwait(false);

            //    #region Response mapping

            //    if (response != null)
            //    {
            //        switch (response.Result)
            //        {

            //            case RemoteStopEVSEResultType.Success:
            //                return Acknowledgement.Success(
            //                           response.SessionId.ToOIOI(),
            //                           StatusCodeDescription: "Ready to stop charging!"
            //                       );

            //            case RemoteStopEVSEResultType.InvalidSessionId:
            //                return Acknowledgement.SessionIsInvalid(
            //                           SessionId: SessionId
            //                       );

            //            case RemoteStopEVSEResultType.Offline:
            //            case RemoteStopEVSEResultType.Timeout:
            //            case RemoteStopEVSEResultType.CommunicationError:
            //                return Acknowledgement.CommunicationToEVSEFailed();

            //            case RemoteStopEVSEResultType.UnknownEVSE:
            //                return Acknowledgement.UnknownEVSEID();

            //            case RemoteStopEVSEResultType.OutOfService:
            //                return Acknowledgement.EVSEOutOfService();

            //        }
            //    }

            //    return Acknowledgement.ServiceNotAvailable(
            //               SessionId: SessionId
            //           );

            //    #endregion

                return response;

            };

            #endregion

        }

        #endregion

        #region WWCPCPOAdapter(Id, Name, RoamingNetwork, CPOClient, CPOServer, ...)

        /// <summary>
        /// Create a new WWCP wrapper for the OIOI roaming client for Charging Station Operators/CPOs.
        /// </summary>
        /// <param name="Id">The unique identification of the roaming provider.</param>
        /// <param name="Name">The offical (multi-language) name of the roaming provider.</param>
        /// <param name="RoamingNetwork">A WWCP roaming network.</param>
        /// 
        /// <param name="CPOClient">An OIOI CPO client.</param>
        /// <param name="CPOServer">An OIOI CPO sever.</param>
        /// <param name="ServerLoggingContext">An optional context for logging server methods.</param>
        /// <param name="LogFileCreator">A delegate to create a log file from the given context and log file name.</param>
        /// 
        /// <param name="ChargingStation2Station">A delegate customize the convertion from a WWCP charging station into an OIOI station.</param>
        /// <param name="EVSEStatusUpdate2ConnectorStatusUpdate">A delegate to customize the convertion from a WWCP EVSE status update into an OIOI connector status update.</param>
        /// <param name="ChargeDetailRecord2Session">A delegate to customize the convertion from a WWCP charge detail record into an OIOI session.</param>
        /// 
        /// <param name="Station2JSON">A delegate to process the JSON representation of an OIOI station, e.g. before uploading it.</param>
        /// <param name="ConnectorStatus2JSON">A delegate to process the JSON representation of an OIOI connector status, e.g. before uploading it.</param>
        /// <param name="Session2JSON">A delegate to process the JSON representation of an OIOI session, e.g. before uploading it.</param>
        /// 
        /// <param name="IncludeChargingStations">Only include the charging stations matching the given delegate.</param>
        /// 
        /// <param name="ServiceCheckEvery">The service check intervall.</param>
        /// <param name="StatusCheckEvery">The status check intervall.</param>
        /// 
        /// <param name="DisablePushData">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisablePushStatus">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisableAuthentication">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisableSendChargeDetailRecords">This service can be disabled, e.g. for debugging reasons.</param>
        public WWCPCPOAdapter(CSORoamingProvider_Id                            Id,
                              I18NString                                       Name,
                              I18NString                                       Description,
                              RoamingNetwork                                   RoamingNetwork,

                              CPOClient                                        CPOClient,
                              CPOServer                                        CPOServer,
                              String                                           ServerLoggingContext                     = CPOServerLogger.DefaultContext,
                              LogfileCreatorDelegate                           LogfileCreator                           = null,

                              CustomOperatorIdMapperDelegate                   CustomOperatorIdMapper                   = null,
                              //CustomEVSEIdMapperDelegate                       CustomEVSEIdMapper                       = null,
                              ChargingStation2StationDelegate                  ChargingStation2Station                  = null,
                              EVSEStatusUpdate2ConnectorStatusUpdateDelegate   EVSEStatusUpdate2ConnectorStatusUpdate   = null,
                              ChargeDetailRecord2SessionDelegate               ChargeDetailRecord2Session               = null,

                              Station2JSONDelegate                             Station2JSON                             = null,
                              ConnectorStatus2JSONDelegate                     ConnectorStatus2JSON                     = null,
                              Session2JSONDelegate                             Session2JSON                             = null,

                              IncludeChargingStationDelegate                   IncludeChargingStations                  = null,
                              IncludeEVSEIdDelegate                            IncludeEVSEIds                           = null,
                              IncludeEVSEDelegate                              IncludeEVSEs                             = null,
                              CustomEVSEIdMapperDelegate                       CustomEVSEIdMapper                       = null,

                              TimeSpan?                                        ServiceCheckEvery                        = null,
                              TimeSpan?                                        StatusCheckEvery                         = null,
                              TimeSpan?                                        CDRCheckEvery                            = null,

                              Boolean                                          DisablePushData                          = false,
                              Boolean                                          DisablePushStatus                        = false,
                              Boolean                                          DisableAuthentication                    = false,
                              Boolean                                          DisableSendChargeDetailRecords           = false,

                              PgpPublicKeyRing                                 PublicKeyRing                            = null,
                              PgpSecretKeyRing                                 SecretKeyRing                            = null,
                              DNSClient                                        DNSClient                                = null)

            : this(Id,
                   Name,
                   Description,
                   RoamingNetwork,

                   new CPORoaming(CPOClient,
                                  CPOServer,
                                  ServerLoggingContext,
                                  LogfileCreator),

                   CustomOperatorIdMapper,
                     //CustomEVSEIdMapper,
                   ChargingStation2Station,
                   EVSEStatusUpdate2ConnectorStatusUpdate,
                   ChargeDetailRecord2Session,

                   Station2JSON,
                   ConnectorStatus2JSON,
                   Session2JSON,

                   IncludeChargingStations,

                   //DefaultOperator,
                   //DefaultOperatorIdFormat,
                   //OperatorNameSelector,

                   IncludeEVSEIds,
                   IncludeEVSEs,
                   CustomEVSEIdMapper,

                   ServiceCheckEvery,
                   StatusCheckEvery,
                   CDRCheckEvery,

                   DisablePushData,
                   DisablePushStatus,
                   DisableAuthentication,
                   DisableSendChargeDetailRecords,

                   PublicKeyRing,
                   SecretKeyRing,
                   DNSClient ?? CPOServer?.DNSClient)

        { }

        #endregion

        #region WWCPCPOAdapter(Id, Name, RoamingNetwork, RemoteHostName, ...)

        /// <summary>
        /// Create a new WWCP wrapper for the OIOI roaming client for Charging Station Operators/CPOs.
        /// </summary>
        /// <param name="Id">The unique identification of the roaming provider.</param>
        /// <param name="Name">The offical (multi-language) name of the roaming provider.</param>
        /// <param name="RoamingNetwork">A WWCP roaming network.</param>
        /// 
        /// <param name="RemoteHostname">The hostname of the remote OIOI service.</param>
        /// <param name="RemoteTCPPort">An optional TCP port of the remote OIOI service.</param>
        /// <param name="RemoteCertificateValidator">A delegate to verify the remote TLS certificate.</param>
        /// <param name="ClientCertificateSelector">A delegate to select a TLS client certificate.</param>
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
        /// <param name="ChargingStation2Station">A delegate customize the convertion from a WWCP charging station into an OIOI station.</param>
        /// <param name="EVSEStatusUpdate2ConnectorStatusUpdate">A delegate to customize the convertion from a WWCP EVSE status update into an OIOI connector status update.</param>
        /// <param name="ChargeDetailRecord2Session">A delegate to customize the convertion from a WWCP charge detail record into an OIOI session.</param>
        /// 
        /// <param name="Station2JSON">A delegate to process the JSON representation of an OIOI station, e.g. before uploading it.</param>
        /// <param name="ConnectorStatus2JSON">A delegate to process the JSON representation of an OIOI connector status, e.g. before uploading it.</param>
        /// <param name="Session2JSON">A delegate to process the JSON representation of an OIOI session, e.g. before uploading it.</param>
        /// 
        /// <param name="IncludeChargingStations">Only include the charging stations matching the given delegate.</param>
        /// 
        /// <param name="ServiceCheckEvery">The service check intervall.</param>
        /// <param name="StatusCheckEvery">The status check intervall.</param>
        /// 
        /// <param name="DisablePushData">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisablePushStatus">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisableAuthentication">This service can be disabled, e.g. for debugging reasons.</param>
        /// <param name="DisableSendChargeDetailRecords">This service can be disabled, e.g. for debugging reasons.</param>
        /// 
        /// <param name="DNSClient">An optional DNS client to use.</param>
        public WWCPCPOAdapter(CSORoamingProvider_Id                           Id,
                              I18NString                                      Name,
                              I18NString                                      Description,
                              RoamingNetwork                                  RoamingNetwork,

                              HTTPHostname                                    RemoteHostname,
                              APIKey                                          APIKey,
                              PartnerIdForStationDelegate                     StationPartnerIdSelector,
                              PartnerIdForConnectorStatusDelegate             ConnectorStatusPartnerIdSelector,
                              IPPort?                                         RemoteTCPPort                            = null,
                              RemoteCertificateValidationCallback             RemoteCertificateValidator               = null,
                              LocalCertificateSelectionCallback               ClientCertificateSelector                = null,
                              HTTPHostname?                                   RemoteHTTPVirtualHost                    = null,
                              HTTPPath?                                        URIPrefix                                = null,
                              String                                          HTTPUserAgent                            = CPOClient.DefaultHTTPUserAgent,

                              IncludeStationDelegate                          IncludeStation                           = null,
                              IncludeStationIdDelegate                        IncludeStationId                         = null,
                              IncludeConnectorIdDelegate                      IncludeConnectorId                       = null,
                              IncludeConnectorStatusTypesDelegate             IncludeConnectorStatusType               = null,
                              IncludeConnectorStatusDelegate                  IncludeConnectorStatus                   = null,
                              TimeSpan?                                       RequestTimeout                           = null,
                              Byte?                                           MaxNumberOfRetries                       = CPOClient.DefaultMaxNumberOfRetries,

                              String                                          ServerName                               = CPOServer.DefaultHTTPServerName,
                              HTTPHostname?                                   HTTPHostname                             = null,
                              IPPort?                                         ServerTCPPort                            = null,
                              ServerCertificateSelectorDelegate               ServerCertificateSelector                = null,
                              RemoteCertificateValidationCallback             RemoteClientCertificateValidator         = null,
                              LocalCertificateSelectionCallback               RemoteClientCertificateSelector          = null,
                              SslProtocols                                    ServerAllowedTLSProtocols                = SslProtocols.Tls12,
                              HTTPPath?                                        ServerURIPrefix                          = null,
                              ServerAPIKeyValidatorDelegate                   ServerAPIKeyValidator                    = null,
                              HTTPContentType                                 ServerContentType                        = null,
                              Boolean                                         ServerRegisterHTTPRootService            = true,
                              Boolean                                         ServerAutoStart                          = false,

                              String                                          ClientLoggingContext                     = CPOClient.CPOClientLogger.DefaultContext,
                              String                                          ServerLoggingContext                     = CPOServerLogger.DefaultContext,
                              LogfileCreatorDelegate                          LogfileCreator                           = null,

                              CustomOperatorIdMapperDelegate                  CustomOperatorIdMapper                   = null,
                              //CustomEVSEIdMapperDelegate                      CustomEVSEIdMapper                       = null,
                              ChargingStation2StationDelegate                 ChargingStation2Station                  = null,
                              EVSEStatusUpdate2ConnectorStatusUpdateDelegate  EVSEStatusUpdate2ConnectorStatusUpdate   = null,
                              ChargeDetailRecord2SessionDelegate              ChargeDetailRecord2Session               = null,

                              Station2JSONDelegate                            Station2JSON                             = null,
                              ConnectorStatus2JSONDelegate                    ConnectorStatus2JSON                     = null,
                              Session2JSONDelegate                            Session2JSON                             = null,

                              IncludeChargingStationDelegate                  IncludeChargingStations                  = null,
                              IncludeEVSEIdDelegate                           IncludeEVSEIds                           = null,
                              IncludeEVSEDelegate                             IncludeEVSEs                             = null,
                              CustomEVSEIdMapperDelegate                      CustomEVSEIdMapper                       = null,

                              TimeSpan?                                       ServiceCheckEvery                        = null,
                              TimeSpan?                                       StatusCheckEvery                         = null,
                              TimeSpan?                                       CDRCheckEvery                            = null,

                              Boolean                                         DisablePushData                          = false,
                              Boolean                                         DisablePushStatus                        = false,
                              Boolean                                         DisableAuthentication                    = false,
                              Boolean                                         DisableSendChargeDetailRecords           = false,

                              PgpPublicKeyRing                                PublicKeyRing                            = null,
                              PgpSecretKeyRing                                SecretKeyRing                            = null,
                              DNSClient                                       DNSClient                                = null)

            : this(Id,
                   Name,
                   Description,
                   RoamingNetwork,

                   new CPORoaming(Id.ToString(),
                                  RemoteHostname,
                                  APIKey,
                                  StationPartnerIdSelector,
                                  ConnectorStatusPartnerIdSelector,
                                  RemoteTCPPort,
                                  RemoteCertificateValidator,
                                  ClientCertificateSelector,
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
                                  ServerCertificateSelector,
                                  RemoteClientCertificateValidator,
                                  RemoteClientCertificateSelector,
                                  ServerAllowedTLSProtocols,
                                  ServerURIPrefix,
                                  ServerAPIKeyValidator,
                                  ServerContentType,
                                  ServerRegisterHTTPRootService,
                                  ServerAutoStart,

                                  ////RemoteCertificateValidator,
                                  ////ClientCertificateSelector,

                                  ClientLoggingContext,
                                  ServerLoggingContext,
                                  LogfileCreator,

                                  DNSClient),

                   CustomOperatorIdMapper,
                   ChargingStation2Station,
                   EVSEStatusUpdate2ConnectorStatusUpdate,
                   ChargeDetailRecord2Session,

                   Station2JSON,
                   ConnectorStatus2JSON,
                   Session2JSON,

                   IncludeChargingStations,
                   IncludeEVSEIds,
                   IncludeEVSEs,
                   CustomEVSEIdMapper,

                   ServiceCheckEvery,
                   StatusCheckEvery,
                   CDRCheckEvery,

                   DisablePushData,
                   DisablePushStatus,
                   DisableAuthentication,
                   DisableSendChargeDetailRecords,

                   PublicKeyRing,
                   SecretKeyRing,
                   DNSClient)

        {

            if (ServerAutoStart)
                CPOServer.Start();

        }

        #endregion

        #endregion


        // RN -> External service requests...

        #region StationPost/-Status directly...

        #region (private) StationsPost        (ChargingStations, ...)

        /// <summary>
        /// Upload the EVSE data of the given enumeration of ChargingStations.
        /// </summary>
        /// <param name="ChargingStations">An enumeration of ChargingStations.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        private async Task<PushChargingStationDataResult>

            StationsPost(IEnumerable<ChargingStation>  ChargingStations,

                         DateTime?                     Timestamp          = null,
                         CancellationToken?            CancellationToken  = null,
                         EventTracking_Id              EventTrackingId    = null,
                         TimeSpan?                     RequestTimeout     = null)

        {

            #region Initial checks

            if (ChargingStations == null)
                throw new ArgumentNullException(nameof(ChargingStations), "The given enumeration of charging stations must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;

            #endregion

            #region Get effective number of stations to upload

            var Warnings = new List<Warning>();

            var _Stations = ChargingStations.Where (station => station != null && _IncludeChargingStations(station)).
                                             Select(station => {

                                                 try
                                                 {

                                                     return new Tuple<ChargingStation, Station>(station,
                                                                                                station.ToOIOI(CustomOperatorIdMapper,
                                                                                                               CustomEVSEIdMapper,
                                                                                                               ChargingStation2Station));

                                                 }
                                                 catch (Exception e)
                                                 {
                                                     DebugX.  Log(e.Message);
                                                     Warnings.Add(Warning.Create(e.Message, station));
                                                 }

                                                 return null;

                                             }).
                                             Where(station => station != null).
                                             ToArray();

            #endregion

            #region Send OnStationPostWWCPRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnStationPostWWCPRequest?.Invoke(StartTime,
                                                 Timestamp.Value,
                                                 this,
                                                 Id,
                                                 EventTrackingId,
                                                 RoamingNetwork.Id,
                                                 _Stations.ULongCount(),
                                                 _Stations,
                                                 Warnings.Where(warning => warning.IsNotNullOrEmpty()),
                                                 RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnStationPostWWCPRequest));
            }

            #endregion


            DateTime                       Endtime;
            TimeSpan                       Runtime;
            PushChargingStationDataResult  result;

            if (DisablePushData)
            {

                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = PushChargingStationDataResult.AdminDown(Id,
                                                                   this,
                                                                   Warnings: Warnings);

            }

            else if (_Stations.Length == 0)
            {

                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = PushChargingStationDataResult.NoOperation(Id,
                                                                     this,
                                                                     Warnings: Warnings);

            }

            else
            {

                var semaphore  = new SemaphoreSlim(_maxDegreeOfParallelism, _maxDegreeOfParallelism);
                var tasks      = new List<Task<HTTPResponse<StationPostResponse>>>();

                for (int i = 0; i < _Stations.Length; i++)
                {
                    var item = i;
                    var task = Task.Run(async () => {

                        try
                        {

                            await semaphore.WaitAsync();

                            return await CPORoaming.StationPost(_Stations[i].Item2,
                                                                CPOClient.StationPartnerIdSelector(_Stations[i].Item2),

                                                                Timestamp,
                                                                CancellationToken,
                                                                EventTrackingId,
                                                                RequestTimeout).
                                                    ConfigureAwait(false);
                        }
                        finally
                        {
                            semaphore.Release();
                        }

                    });

                    tasks.Add(task);

                }

                await Task.WhenAll(tasks);



                //var responses  = await Task.WhenAll(_Stations.
                //                                        Select(station => CPORoaming.StationPost(station.Item2,
                //                                                                                 CPOClient.StationPartnerIdSelector(station.Item2),

                //                                                                                 Timestamp,
                //                                                                                 CancellationToken,
                //                                                                                 EventTrackingId,
                //                                                                                 RequestTimeout)).
                //                                        ToArray()).
                //                                        ConfigureAwait(false);

                var results = tasks.Select       ( task         => task.Result).
                                    SelectCounted((response, i) => {

                                        if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                                            response.Content        != null)
                                        {

                                            if (response.Content.Code == ResponseCodes.Success)
                                                return new PushSingleChargingStationDataResult(_Stations[i-1].Item1,
                                                                                               PushSingleDataResultTypes.Success,
                                                                                               new Warning[] { Warning.Create(response.Content.Message) });

                                            else
                                                return new PushSingleChargingStationDataResult(_Stations[i-1].Item1,
                                                                                               PushSingleDataResultTypes.Error,
                                                                                               new Warning[] { Warning.Create(response.Content.Message) });

                                        }
                                        else
                                            return new PushSingleChargingStationDataResult(_Stations[i-1].Item1,
                                                                                           PushSingleDataResultTypes.Error,
                                                                                           new Warning[] {
                                                                                               Warning.Create(response.HTTPStatusCode.ToString())
                                                                                           }.Concat(
                                                                                               response.HTTPBody != null
                                                                                                   ? Warnings.AddAndReturnList(response.HTTPBody.ToUTF8String())
                                                                                                   : Warnings.AddAndReturnList("No HTTP body received!")
                                                                                           ));

                });

                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                result   = results.All(_ => _.Result == PushSingleDataResultTypes.Success)

                               ? PushChargingStationDataResult.Success(Id,
                                                                       this,
                                                                       null,
                                                                       Warnings,
                                                                       Runtime)

                               : PushChargingStationDataResult.Error  (Id,
                                                                       this,
                                                                       results.Where(_ => _.Result != PushSingleDataResultTypes.Success),
                                                                       null,
                                                                       Warnings,
                                                                       Runtime);


            }


            #region Send OnStationPostResponse event

            try
            {

                OnStationPostWWCPResponse?.Invoke(Endtime,
                                                  Timestamp.Value,
                                                  this,
                                                  Id,
                                                  EventTrackingId,
                                                  RoamingNetwork.Id,
                                                  _Stations.ULongCount(),
                                                  _Stations,
                                                  RequestTimeout,
                                                  result,
                                                  Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnStationPostWWCPResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region (private) ConnectorsPostStatus(EVSEStatusUpdates, ...)

        /// <summary>
        /// Upload the EVSE status of the given lookup of EVSE status types grouped by their Charging Station Operator.
        /// </summary>
        /// <param name="EVSEStatusUpdates">An enumeration of EVSE status updates.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<PushEVSEStatusResult>

            ConnectorsPostStatus(IEnumerable<EVSEStatusUpdate>  EVSEStatusUpdates,

                                 DateTime?                      Timestamp           = null,
                                 CancellationToken?             CancellationToken   = null,
                                 EventTracking_Id               EventTrackingId     = null,
                                 TimeSpan?                      RequestTimeout      = null)

        {

            #region Initial checks

            if (EVSEStatusUpdates == null)
                throw new ArgumentNullException(nameof(EVSEStatusUpdates), "The given enumeration of EVSE status updates must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;

            #endregion

            #region Get effective number of connector status to upload

            var Warnings = new List<Warning>();

            var _ConnectorStatus = EVSEStatusUpdates.
                                       Where       (evsestatusupdate => _IncludeChargingStations(evsestatusupdate.EVSE.ChargingStation)).
                                       ToLookup    (evsestatusupdate => evsestatusupdate.EVSE.Id,
                                                    evsestatusupdate => evsestatusupdate).
                                       ToDictionary(group            => group.Key,
                                                    group            => group.AsEnumerable().OrderByDescending(item => item.NewStatus.Timestamp)).
                                       Select      (evsestatusupdate => {

                                           try
                                           {

                                               // Only push the current status of the latest status update!
                                               return new Tuple<EVSEStatusUpdate, ConnectorStatus>(
                                                          evsestatusupdate.Value.First(),
                                                          new ConnectorStatus(
                                                              CustomEVSEIdMapper != null
                                                                  ? CustomEVSEIdMapper(evsestatusupdate.Key)
                                                                  : evsestatusupdate.Key.ToOIOI(),
                                                              evsestatusupdate.Value.First().NewStatus.Value.ToOIOI()
                                                          )
                                                      );

                                           }
                                           catch (Exception e)
                                           {
                                               DebugX.  Log(e.Message);
                                               Warnings.Add(Warning.Create(e.Message, evsestatusupdate));
                                           }

                                           return null;

                                       }).
                                       Where(connectorstatus => connectorstatus != null).
                                       ToArray();

            #endregion

            #region Send OnEVSEStatusPush event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnConnectorStatusPostRequest?.Invoke(StartTime,
                                                    Timestamp.Value,
                                                    this,
                                                    Id,
                                                    EventTrackingId,
                                                    RoamingNetwork.Id,
                                                    _ConnectorStatus.ULongCount(),
                                                    _ConnectorStatus,
                                                    Warnings.Where(warning => warning.IsNotNullOrEmpty()),
                                                    RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnConnectorStatusPostRequest));
            }

            #endregion


            DateTime              Endtime;
            TimeSpan              Runtime;
            PushEVSEStatusResult  result;

            if (DisablePushStatus)
            {
                Endtime = DateTime.UtcNow;
                Runtime = Endtime - StartTime;
                result  = PushEVSEStatusResult.NoOperation(Id, this);  //AuthStartChargingStationResult.OutOfService(Id, SessionId, Runtime);
            }

            else
            {

                var semaphore  = new SemaphoreSlim(_maxDegreeOfParallelism, _maxDegreeOfParallelism);
                var tasks      = new List<Task<HTTPResponse<ConnectorPostStatusResponse>>>();

                for (int i = 0; i < _ConnectorStatus.Length; i++)
                {
                    var item = i;
                    var task = Task.Run(async () => {

                        try
                        {

                            await semaphore.WaitAsync();

                            return await CPORoaming.ConnectorPostStatus(_ConnectorStatus[i].Item2,
                                                                        CPOClient.ConnectorStatusPartnerIdSelector(_ConnectorStatus[i].Item2),

                                                                        Timestamp,
                                                                        CancellationToken,
                                                                        EventTrackingId,
                                                                        RequestTimeout).
                                                    ConfigureAwait(false);
                        }
                        finally
                        {
                            semaphore.Release();
                        }

                    });

                    tasks.Add(task);

                }

                await Task.WhenAll(tasks);

                //var responses = await Task.WhenAll(_ConnectorStatus.
                //                                       Select(status => CPORoaming.ConnectorPostStatus(status.Item2,
                //                                                                                       CPOClient.ConnectorStatusPartnerIdSelector(status.Item2),

                //                                                                                       Timestamp,
                //                                                                                       CancellationToken,
                //                                                                                       EventTrackingId,
                //                                                                                       RequestTimeout)).
                //                                       ToArray()).
                //                                       ConfigureAwait(false);

                Endtime = DateTime.UtcNow;
                Runtime = Endtime - StartTime;

                result = PushEVSEStatusResult.NoOperation(Id, this);

                //if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                //    response.Content        != null)
                //{

                //    if (response.Content.Result)
                //        result = new Acknowledgement(ResultType.True,
                //                                          response.Content.StatusCode.Description,
                //                                          response.Content.StatusCode.AdditionalInfo.IsNotNullOrEmpty()
                //                                              ? Warnings.AddAndReturnList(response.Content.StatusCode.AdditionalInfo)
                //                                              : Warnings,
                //                                          Runtime);

                //    else
                //        result = new Acknowledgement(ResultType.False,
                //                                          response.Content.StatusCode.Description,
                //                                          response.Content.StatusCode.AdditionalInfo.IsNotNullOrEmpty()
                //                                              ? Warnings.AddAndReturnList(response.Content.StatusCode.AdditionalInfo)
                //                                              : Warnings,
                //                                          Runtime);

                //}
                //else
                //    result = new Acknowledgement(ResultType.False,
                //                                      response.HTTPStatusCode.ToString(),
                //                                      response.HTTPBody != null
                //                                          ? Warnings.AddAndReturnList(response.HTTPBody.ToUTF8String())
                //                                          : Warnings.AddAndReturnList("No HTTP body received!"),
                //                                      Runtime);

            }


            #region Send OnPushEVSEStatusResponse event

            try
            {

                OnConnectorStatusPostResponse?.Invoke(Endtime,
                                                     Timestamp.Value,
                                                     this,
                                                     Id,
                                                     EventTrackingId,
                                                     RoamingNetwork.Id,
                                                     _ConnectorStatus.ULongCount(),
                                                     _ConnectorStatus,
                                                     RequestTimeout,
                                                     result,
                                                     Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnConnectorStatusPostResponse));
            }

            #endregion

            return result;

        }

        #endregion


        #region (Set/Add/Update/Delete) EVSE(s)...

        #region SetStaticData   (EVSE, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Set the given EVSE as new static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="EVSE">An EVSE to upload.</param>
        /// <param name="TransmissionType">Whether to send the EVSE directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.SetStaticData(EVSE                EVSE,
                                    TransmissionTypes   TransmissionType,

                                    DateTime?           Timestamp,
                                    CancellationToken?  CancellationToken,
                                    EventTracking_Id    EventTrackingId,
                                    TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (EVSE == null)
                throw new ArgumentNullException(nameof(EVSE), "The given EVSE must not be null!");

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                //                                    Timestamp.Value,
                //                                    this,
                //                                    EventTrackingId,
                //                                    RoamingNetwork.Id,
                //                                    ChargeDetailRecord,
                //                                    RequestTimeout);

                //}
                //catch (Exception e)
                //{
                //    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (_IncludeChargingStations == null ||
                       (_IncludeChargingStations != null && _IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToAddQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this);

            }

            #endregion


            return (await StationsPost(new ChargingStation[] { EVSE.ChargingStation },

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region AddStaticData   (EVSE, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Add the given EVSE to the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="EVSE">An EVSE to upload.</param>
        /// <param name="TransmissionType">Whether to send the EVSE directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.AddStaticData(EVSE                EVSE,
                                    TransmissionTypes   TransmissionType,

                                    DateTime?           Timestamp,
                                    CancellationToken?  CancellationToken,
                                    EventTracking_Id    EventTrackingId,
                                    TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (EVSE == null)
                throw new ArgumentNullException(nameof(EVSE), "The given EVSE must not be null!");

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                //                                    Timestamp.Value,
                //                                    this,
                //                                    EventTrackingId,
                //                                    RoamingNetwork.Id,
                //                                    ChargeDetailRecord,
                //                                    RequestTimeout);

                //}
                //catch (Exception e)
                //{
                //    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (_IncludeChargingStations == null ||
                       (_IncludeChargingStations != null && _IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToAddQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this);

            }

            #endregion


            return (await StationsPost(new ChargingStation[] { EVSE.ChargingStation },

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region UpdateStaticData(EVSE, PropertyName = null, OldValue = null, NewValue = null, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the static data of the given EVSE.
        /// The EVSE can be uploaded as a whole, or just a single property of the EVSE.
        /// </summary>
        /// <param name="EVSE">An EVSE to update.</param>
        /// <param name="PropertyName">The name of the EVSE property to update.</param>
        /// <param name="OldValue">The old value of the EVSE property to update.</param>
        /// <param name="NewValue">The new value of the EVSE property to update.</param>
        /// <param name="TransmissionType">Whether to send the EVSE update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.UpdateStaticData(EVSE                EVSE,
                                       String              PropertyName,
                                       Object              OldValue,
                                       Object              NewValue,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken?  CancellationToken,
                                       EventTracking_Id    EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (EVSE == null)
                throw new ArgumentNullException(nameof(EVSE), "The given EVSE must not be null!");

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                //                                    Timestamp.Value,
                //                                    this,
                //                                    EventTrackingId,
                //                                    RoamingNetwork.Id,
                //                                    ChargeDetailRecord,
                //                                    RequestTimeout);

                //}
                //catch (Exception e)
                //{
                //    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (_IncludeChargingStations == null ||
                       (_IncludeChargingStations != null && _IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToUpdateQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this);

            }

            #endregion


            return (await StationsPost(new ChargingStation[] { EVSE.ChargingStation },

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region DeleteStaticData(EVSE, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Delete the static data of the given EVSE.
        /// </summary>
        /// <param name="EVSE">An EVSE to delete.</param>
        /// <param name="TransmissionType">Whether to send the EVSE deletion directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.DeleteStaticData(EVSE                EVSE,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken?  CancellationToken,
                                       EventTracking_Id    EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (EVSE == null)
                throw new ArgumentNullException(nameof(EVSE), "The given EVSE must not be null!");

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                //                                    Timestamp.Value,
                //                                    this,
                //                                    EventTrackingId,
                //                                    RoamingNetwork.Id,
                //                                    ChargeDetailRecord,
                //                                    RequestTimeout);

                //}
                //catch (Exception e)
                //{
                //    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (_IncludeChargingStations == null ||
                       (_IncludeChargingStations != null && _IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToUpdateQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this);

            }

            #endregion


            return (await StationsPost(new ChargingStation[] { EVSE.ChargingStation },

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion


        #region SetStaticData   (EVSEs, ...)

        /// <summary>
        /// Set the given enumeration of EVSEs as new static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="EVSEs">An enumeration of EVSEs.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.SetStaticData(IEnumerable<EVSE>   EVSEs,
                                    TransmissionTypes   TransmissionType,

                                    DateTime?           Timestamp,
                                    CancellationToken?  CancellationToken,
                                    EventTracking_Id    EventTrackingId,
                                    TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (EVSEs == null)
                throw new ArgumentNullException(nameof(EVSEs), "The given enumeration of EVSEs must not be null!");

            #endregion

            return (await StationsPost(EVSEs.Select(evse => evse.ChargingStation).Distinct(),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region AddStaticData   (EVSEs, ...)

        /// <summary>
        /// Add the given enumeration of EVSEs to the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="EVSEs">An enumeration of EVSEs.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.AddStaticData(IEnumerable<EVSE>   EVSEs,
                                    TransmissionTypes   TransmissionType,

                                    DateTime?           Timestamp,
                                    CancellationToken?  CancellationToken,
                                    EventTracking_Id    EventTrackingId,
                                    TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (EVSEs == null)
                throw new ArgumentNullException(nameof(EVSEs), "The given enumeration of EVSEs must not be null!");

            #endregion

            return (await StationsPost(EVSEs.Select(evse => evse.ChargingStation).Distinct(),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region UpdateStaticData(EVSEs, ...)

        /// <summary>
        /// Update the given enumeration of EVSEs within the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="EVSEs">An enumeration of EVSEs.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.UpdateStaticData(IEnumerable<EVSE>   EVSEs,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken?  CancellationToken,
                                       EventTracking_Id    EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (EVSEs == null)
                throw new ArgumentNullException(nameof(EVSEs), "The given enumeration of EVSEs must not be null!");

            #endregion

            return (await StationsPost(EVSEs.Select(evse => evse.ChargingStation).Distinct(),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region DeleteStaticData(EVSEs, ...)

        /// <summary>
        /// Delete the given enumeration of EVSEs from the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="EVSEs">An enumeration of EVSEs.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.DeleteStaticData(IEnumerable<EVSE>   EVSEs,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken?  CancellationToken,
                                       EventTracking_Id    EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (EVSEs == null)
                throw new ArgumentNullException(nameof(EVSEs), "The given enumeration of EVSEs must not be null!");

            #endregion

            return (await StationsPost(EVSEs.Select(evse => evse.ChargingStation).Distinct(),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion


        #region UpdateAdminStatus(AdminStatusUpdates, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of EVSE admin status updates.
        /// </summary>
        /// <param name="AdminStatusUpdates">An enumeration of EVSE admin status updates.</param>
        /// <param name="TransmissionType">Whether to send the EVSE admin status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        Task<PushEVSEAdminStatusResult>

            ISendAdminStatus.UpdateAdminStatus(IEnumerable<EVSEAdminStatusUpdate>  AdminStatusUpdates,
                                               TransmissionTypes                   TransmissionType,

                                               DateTime?                           Timestamp,
                                               CancellationToken?                  CancellationToken,
                                               EventTracking_Id                    EventTrackingId,
                                               TimeSpan?                           RequestTimeout)


                => Task.FromResult(PushEVSEAdminStatusResult.NoOperation(Id, this));

        #endregion

        #region UpdateStatus     (StatusUpdates,      TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of EVSE status updates.
        /// </summary>
        /// <param name="StatusUpdates">An enumeration of EVSE status updates.</param>
        /// <param name="TransmissionType">Whether to send the EVSE status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEStatusResult>

            ISendStatus.UpdateStatus(IEnumerable<EVSEStatusUpdate>  StatusUpdates,
                                     TransmissionTypes              TransmissionType,

                                     DateTime?                      Timestamp,
                                     CancellationToken?             CancellationToken,
                                     EventTracking_Id               EventTrackingId,
                                     TimeSpan?                      RequestTimeout)

        {

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                //#region Send OnEnqueueSendCDRRequest event

                ////try
                ////{

                ////    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                ////                                    Timestamp.Value,
                ////                                    this,
                ////                                    EventTrackingId,
                ////                                    RoamingNetwork.Id,
                ////                                    ChargeDetailRecord,
                ////                                    RequestTimeout);

                ////}
                ////catch (Exception e)
                ////{
                ////    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                ////}

                //#endregion


                //if (Monitor.TryEnter(StatusCheckLock,
                //                     TimeSpan.FromMinutes(5)))
                //{

                //    try
                //    {

                //        //if (_IncludeChargingStations == null ||
                //        //   (_IncludeChargingStations != null && _IncludeChargingStations(EVSE)))
                //        //{

                //        EVSEStatusUpdatesQueue.AddRange(StatusUpdates);
                //        StatusCheckTimer.Change(_StatusCheckEvery, TimeSpan.FromMilliseconds(-1));

                //        //}

                //    }
                //    finally
                //    {
                //        Monitor.Exit(StatusCheckLock);
                //    }

                //    return PushEVSEStatusResult.Enqueued(Id, this);

                //}

                //return PushEVSEStatusResult.LockTimeout(Id, this);

            }

            #endregion


            return await ConnectorsPostStatus(StatusUpdates,

                                             Timestamp,
                                             CancellationToken,
                                             EventTrackingId,
                                             RequestTimeout).

                                             ConfigureAwait(false);

        }

        #endregion

        #endregion

        #region (Set/Add/Update/Delete) Charging station(s)...

        #region SetStaticData   (ChargingStation, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Set the EVSE data of the given charging station as new static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStation">A charging station.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.SetStaticData(ChargingStation     ChargingStation,
                                    TransmissionTypes   TransmissionType,

                                    DateTime?           Timestamp,
                                    CancellationToken?  CancellationToken,
                                    EventTracking_Id    EventTrackingId,
                                    TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (ChargingStation == null)
                throw new ArgumentNullException(nameof(ChargingStation), "The given charging station must not be null!");

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                //                                    Timestamp.Value,
                //                                    this,
                //                                    EventTrackingId,
                //                                    RoamingNetwork.Id,
                //                                    ChargeDetailRecord,
                //                                    RequestTimeout);

                //}
                //catch (Exception e)
                //{
                //    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (_IncludeChargingStations == null ||
                       (_IncludeChargingStations != null && _IncludeChargingStations(ChargingStation)))
                    {

                        StationsToAddQueue.Add(ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this);

            }

            #endregion


            return (await StationsPost(new ChargingStation[] { ChargingStation },

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region AddStaticData   (ChargingStation, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Add the EVSE data of the given charging station to the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStation">A charging station.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.AddStaticData(ChargingStation     ChargingStation,
                                    TransmissionTypes   TransmissionType,

                                    DateTime?           Timestamp,
                                    CancellationToken?  CancellationToken,
                                    EventTracking_Id    EventTrackingId,
                                    TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (ChargingStation == null)
                throw new ArgumentNullException(nameof(ChargingStation), "The given charging station must not be null!");

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                //                                    Timestamp.Value,
                //                                    this,
                //                                    EventTrackingId,
                //                                    RoamingNetwork.Id,
                //                                    ChargeDetailRecord,
                //                                    RequestTimeout);

                //}
                //catch (Exception e)
                //{
                //    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (_IncludeChargingStations == null ||
                       (_IncludeChargingStations != null && _IncludeChargingStations(ChargingStation)))
                    {

                        StationsToAddQueue.Add(ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this);

            }

            #endregion

            return (await StationsPost(new ChargingStation[] { ChargingStation },

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region UpdateStaticData(ChargingStation, PropertyName = null, OldValue = null, NewValue = null, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the EVSE data of the given charging station within the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStation">A charging station.</param>
        /// <param name="PropertyName">The name of the charging station property to update.</param>
        /// <param name="OldValue">The old value of the charging station property to update.</param>
        /// <param name="NewValue">The new value of the charging station property to update.</param>
        /// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.UpdateStaticData(ChargingStation     ChargingStation,
                                       String              PropertyName,
                                       Object              OldValue,
                                       Object              NewValue,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken?  CancellationToken,
                                       EventTracking_Id    EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (ChargingStation == null)
                throw new ArgumentNullException(nameof(ChargingStation), "The given charging station must not be null!");

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                //                                    Timestamp.Value,
                //                                    this,
                //                                    EventTrackingId,
                //                                    RoamingNetwork.Id,
                //                                    ChargeDetailRecord,
                //                                    RequestTimeout);

                //}
                //catch (Exception e)
                //{
                //    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (_IncludeChargingStations == null ||
                       (_IncludeChargingStations != null && _IncludeChargingStations(ChargingStation)))
                    {

                        StationsToUpdateQueue.Add(ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this);

            }

            #endregion

            return (await StationsPost(new ChargingStation[] { ChargingStation },

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region DeleteStaticData(ChargingStation, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Delete the EVSE data of the given charging station from the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStation">A charging station.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.DeleteStaticData(ChargingStation     ChargingStation,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken?  CancellationToken,
                                       EventTracking_Id    EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (ChargingStation == null)
                throw new ArgumentNullException(nameof(ChargingStation), "The given charging station must not be null!");

            #endregion

            return (await StationsPost(new ChargingStation[] { ChargingStation },

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion


        #region SetStaticData   (ChargingStations, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Set the EVSE data of the given enumeration of charging stations as new static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingStations">An enumeration of charging stations.</param>
        /// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.SetStaticData(IEnumerable<ChargingStation>  ChargingStations,
                                    TransmissionTypes             TransmissionType,

                                    DateTime?                     Timestamp,
                                    CancellationToken?            CancellationToken,
                                    EventTracking_Id              EventTrackingId,
                                    TimeSpan?                     RequestTimeout)

        {

            #region Initial checks

            if (ChargingStations == null)
                throw new ArgumentNullException(nameof(ChargingStations), "The given enumeration of charging stations must not be null!");

            #endregion

            return (await StationsPost(ChargingStations,

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region AddStaticData   (ChargingStations, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Add the EVSE data of the given enumeration of charging stations to the static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingStations">An enumeration of charging stations.</param>
        /// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.AddStaticData(IEnumerable<ChargingStation>  ChargingStations,
                                    TransmissionTypes             TransmissionType,


                                    DateTime?                     Timestamp,
                                    CancellationToken?            CancellationToken,
                                    EventTracking_Id              EventTrackingId,
                                    TimeSpan?                     RequestTimeout)

        {

            #region Initial checks

            if (ChargingStations == null)
                throw new ArgumentNullException(nameof(ChargingStations), "The given enumeration of charging stations must not be null!");

            #endregion

            return (await StationsPost(ChargingStations,

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region UpdateStaticData(ChargingStations, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the EVSE data of the given enumeration of charging stations within the static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingStations">An enumeration of charging stations.</param>
        /// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.UpdateStaticData(IEnumerable<ChargingStation>  ChargingStations,
                                       TransmissionTypes             TransmissionType,

                                       DateTime?                     Timestamp,
                                       CancellationToken?            CancellationToken,
                                       EventTracking_Id              EventTrackingId,
                                       TimeSpan?                     RequestTimeout)

        {

            #region Initial checks

            if (ChargingStations == null)
                throw new ArgumentNullException(nameof(ChargingStations), "The given enumeration of charging stations must not be null!");

            #endregion

            return (await StationsPost(ChargingStations,

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region DeleteStaticData(ChargingStations, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Delete the EVSE data of the given enumeration of charging stations from the static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingStations">An enumeration of charging stations.</param>
        /// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.DeleteStaticData(IEnumerable<ChargingStation>  ChargingStations,
                                       TransmissionTypes             TransmissionType,

                                       DateTime?                     Timestamp,
                                       CancellationToken?            CancellationToken,
                                       EventTracking_Id              EventTrackingId,
                                       TimeSpan?                     RequestTimeout)

        {

            #region Initial checks

            if (ChargingStations == null)
                throw new ArgumentNullException(nameof(ChargingStations), "The given enumeration of charging stations must not be null!");

            #endregion

            return (await StationsPost(ChargingStations, //ToDo: Mark them as deleted!

                                       Timestamp,
                                       CancellationToken,
                                       EventTrackingId,
                                       RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion


        #region UpdateAdminStatus(AdminStatusUpdates, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of charging station admin status updates.
        /// </summary>
        /// <param name="AdminStatusUpdates">An enumeration of charging station admin status updates.</param>
        /// <param name="TransmissionType">Whether to send the charging station admin status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        Task<PushChargingStationAdminStatusResult>

            ISendAdminStatus.UpdateAdminStatus(IEnumerable<ChargingStationAdminStatusUpdate>  AdminStatusUpdates,
                                               TransmissionTypes                              TransmissionType,

                                               DateTime?                                      Timestamp,
                                               CancellationToken?                             CancellationToken,
                                               EventTracking_Id                               EventTrackingId,
                                               TimeSpan?                                      RequestTimeout)


                => Task.FromResult(PushChargingStationAdminStatusResult.NoOperation(Id, this));

        #endregion

        #region UpdateStatus     (StatusUpdates,      TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of charging station status updates.
        /// </summary>
        /// <param name="StatusUpdates">An enumeration of charging station status updates.</param>
        /// <param name="TransmissionType">Whether to send the charging station status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        Task<PushChargingStationStatusResult>

            ISendStatus.UpdateStatus(IEnumerable<ChargingStationStatusUpdate>  StatusUpdates,
                                     TransmissionTypes                         TransmissionType,

                                     DateTime?                                 Timestamp,
                                     CancellationToken?                        CancellationToken,
                                     EventTracking_Id                          EventTrackingId,
                                     TimeSpan?                                 RequestTimeout)


                => Task.FromResult(PushChargingStationStatusResult.NoOperation(Id, this));

        #endregion

        #endregion

        #region (Set/Add/Update/Delete) Charging pool(s)...

        #region SetStaticData   (ChargingPool, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Set the EVSE data of the given charging pool as new static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingPool">A charging pool.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.SetStaticData(ChargingPool        ChargingPool,
                                    TransmissionTypes   TransmissionType,

                                    DateTime?           Timestamp,
                                    CancellationToken?  CancellationToken,
                                    EventTracking_Id    EventTrackingId,
                                    TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (ChargingPool == null)
                throw new ArgumentNullException(nameof(ChargingPool), "The given charging pool must not be null!");

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                //                                    Timestamp.Value,
                //                                    this,
                //                                    EventTrackingId,
                //                                    RoamingNetwork.Id,
                //                                    ChargeDetailRecord,
                //                                    RequestTimeout);

                //}
                //catch (Exception e)
                //{
                //    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    foreach (var station in ChargingPool)
                    {

                        if (_IncludeChargingStations == null ||
                           (_IncludeChargingStations != null && _IncludeChargingStations(station)))
                        {

                            StationsToAddQueue.Add(station);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                        }

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this);

            }

            #endregion


            return (await StationsPost(ChargingPool.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region AddStaticData   (ChargingPool, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Add the EVSE data of the given charging pool to the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingPool">A charging pool.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.AddStaticData(ChargingPool        ChargingPool,
                                    TransmissionTypes   TransmissionType,

                                    DateTime?           Timestamp,
                                    CancellationToken?  CancellationToken,
                                    EventTracking_Id    EventTrackingId,
                                    TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (ChargingPool == null)
                throw new ArgumentNullException(nameof(ChargingPool), "The given charging pool must not be null!");

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                //                                    Timestamp.Value,
                //                                    this,
                //                                    EventTrackingId,
                //                                    RoamingNetwork.Id,
                //                                    ChargeDetailRecord,
                //                                    RequestTimeout);

                //}
                //catch (Exception e)
                //{
                //    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    foreach (var station in ChargingPool)
                    {

                        if (_IncludeChargingStations == null ||
                           (_IncludeChargingStations != null && _IncludeChargingStations(station)))
                        {

                            StationsToAddQueue.Add(station);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                        }

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this);

            }

            #endregion


            return (await StationsPost(ChargingPool.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region UpdateStaticData(ChargingPool, PropertyName = null, OldValue = null, NewValue = null, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the EVSE data of the given charging pool within the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingPool">A charging pool.</param>
        /// <param name="PropertyName">The name of the charging pool property to update.</param>
        /// <param name="OldValue">The old value of the charging pool property to update.</param>
        /// <param name="NewValue">The new value of the charging pool property to update.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.UpdateStaticData(ChargingPool        ChargingPool,
                                       String              PropertyName,
                                       Object              OldValue,
                                       Object              NewValue,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken?  CancellationToken,
                                       EventTracking_Id    EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (ChargingPool == null)
                throw new ArgumentNullException(nameof(ChargingPool), "The given charging pool must not be null!");

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
                //                                    Timestamp.Value,
                //                                    this,
                //                                    EventTrackingId,
                //                                    RoamingNetwork.Id,
                //                                    ChargeDetailRecord,
                //                                    RequestTimeout);

                //}
                //catch (Exception e)
                //{
                //    e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    foreach (var station in ChargingPool)
                    {

                        if (_IncludeChargingStations == null ||
                           (_IncludeChargingStations != null && _IncludeChargingStations(station)))
                        {

                            StationsToUpdateQueue.Add(station);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                        }

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this);

            }

            #endregion


            return (await StationsPost(ChargingPool.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region DeleteStaticData(ChargingPool, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Delete the EVSE data of the given charging pool from the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingPool">A charging pool.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.DeleteStaticData(ChargingPool        ChargingPool,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken?  CancellationToken,
                                       EventTracking_Id    EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (ChargingPool == null)
                throw new ArgumentNullException(nameof(ChargingPool), "The given charging pool must not be null!");

            #endregion


            return (await StationsPost(ChargingPool.ChargingStations, // Mark as deleted?

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion


        #region SetStaticData   (ChargingPools, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Set the EVSE data of the given enumeration of charging pools as new static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingPools">An enumeration of charging pools.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.SetStaticData(IEnumerable<ChargingPool>  ChargingPools,
                                    TransmissionTypes          TransmissionType,

                                    DateTime?                  Timestamp,
                                    CancellationToken?         CancellationToken,
                                    EventTracking_Id           EventTrackingId,
                                    TimeSpan?                  RequestTimeout)

        {

            #region Initial checks

            if (ChargingPools == null)
                throw new ArgumentNullException(nameof(ChargingPools), "The given enumeration of charging pools must not be null!");

            #endregion


            return (await StationsPost(ChargingPools.SafeSelectMany(pool => pool.ChargingStations),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region AddStaticData   (ChargingPools, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Add the EVSE data of the given enumeration of charging pools to the static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingPools">An enumeration of charging pools.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.AddStaticData(IEnumerable<ChargingPool>  ChargingPools,
                                    TransmissionTypes          TransmissionType,

                                    DateTime?                  Timestamp,
                                    CancellationToken?         CancellationToken,
                                    EventTracking_Id           EventTrackingId,
                                    TimeSpan?                  RequestTimeout)

        {

            #region Initial checks

            if (ChargingPools == null)
                throw new ArgumentNullException(nameof(ChargingPools), "The given enumeration of charging pools must not be null!");

            #endregion

            return (await StationsPost(ChargingPools.SafeSelectMany(pool => pool.ChargingStations),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region UpdateStaticData(ChargingPools, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the EVSE data of the given enumeration of charging pools within the static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingPools">An enumeration of charging pools.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.UpdateStaticData(IEnumerable<ChargingPool>  ChargingPools,
                                       TransmissionTypes          TransmissionType,

                                       DateTime?                  Timestamp,
                                       CancellationToken?         CancellationToken,
                                       EventTracking_Id           EventTrackingId,
                                       TimeSpan?                  RequestTimeout)

        {

            #region Initial checks

            if (ChargingPools == null)
                throw new ArgumentNullException(nameof(ChargingPools), "The given enumeration of charging pools must not be null!");

            #endregion


            return (await StationsPost(ChargingPools.SafeSelectMany(pool => pool.ChargingStations),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region DeleteStaticData(ChargingPools, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Delete the EVSE data of the given enumeration of charging pools from the static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingPools">An enumeration of charging pools.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.DeleteStaticData(IEnumerable<ChargingPool>  ChargingPools,
                                       TransmissionTypes          TransmissionType,

                                       DateTime?                  Timestamp,
                                       CancellationToken?         CancellationToken,
                                       EventTracking_Id           EventTrackingId,
                                       TimeSpan?                  RequestTimeout)

        {

            #region Initial checks

            if (ChargingPools == null)
                throw new ArgumentNullException(nameof(ChargingPools), "The given enumeration of charging pools must not be null!");

            #endregion


            return (await StationsPost(ChargingPools.SafeSelectMany(pool => pool.ChargingStations),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion


        #region UpdateAdminStatus(AdminStatusUpdates, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of charging pool admin status updates.
        /// </summary>
        /// <param name="AdminStatusUpdates">An enumeration of charging pool admin status updates.</param>
        /// <param name="TransmissionType">Whether to send the charging pool admin status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        Task<PushChargingPoolAdminStatusResult>

            ISendAdminStatus.UpdateAdminStatus(IEnumerable<ChargingPoolAdminStatusUpdate>  AdminStatusUpdates,
                                               TransmissionTypes                           TransmissionType,

                                               DateTime?                                   Timestamp,
                                               CancellationToken?                          CancellationToken,
                                               EventTracking_Id                            EventTrackingId,
                                               TimeSpan?                                   RequestTimeout)


                => Task.FromResult(PushChargingPoolAdminStatusResult.NoOperation(Id, this));

        #endregion

        #region UpdateStatus     (StatusUpdates,      TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of charging pool status updates.
        /// </summary>
        /// <param name="StatusUpdates">An enumeration of charging pool status updates.</param>
        /// <param name="TransmissionType">Whether to send the charging pool status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        Task<PushChargingPoolStatusResult>

            ISendStatus.UpdateStatus(IEnumerable<ChargingPoolStatusUpdate>  StatusUpdates,
                                     TransmissionTypes                      TransmissionType,

                                     DateTime?                              Timestamp,
                                     CancellationToken?                     CancellationToken,
                                     EventTracking_Id                       EventTrackingId,
                                     TimeSpan?                              RequestTimeout)


                => Task.FromResult(PushChargingPoolStatusResult.NoOperation(Id, this));

        #endregion

        #endregion

        #region (Set/Add/Update/Delete) Charging station operator(s)...

        #region SetStaticData   (ChargingStationOperator, ...)

        /// <summary>
        /// Set the EVSE data of the given charging station operator as new static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStationOperator">A charging station operator.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.SetStaticData(ChargingStationOperator  ChargingStationOperator,

                                          DateTime?                Timestamp,
                                          CancellationToken?       CancellationToken,
                                          EventTracking_Id         EventTrackingId,
                                          TimeSpan?                RequestTimeout)

        {

            #region Initial checks

            if (ChargingStationOperator == null)
                throw new ArgumentNullException(nameof(ChargingStationOperator), "The given charging station operator must not be null!");

            #endregion


            return (await StationsPost(ChargingStationOperator.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region AddStaticData   (ChargingStationOperator, ...)

        /// <summary>
        /// Add the EVSE data of the given charging station operator to the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStationOperator">A charging station operator.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.AddStaticData(ChargingStationOperator  ChargingStationOperator,

                                          DateTime?                Timestamp,
                                          CancellationToken?       CancellationToken,
                                          EventTracking_Id         EventTrackingId,
                                          TimeSpan?                RequestTimeout)

        {

            #region Initial checks

            if (ChargingStationOperator == null)
                throw new ArgumentNullException(nameof(ChargingStationOperator), "The given charging station operator must not be null!");

            #endregion


            return (await StationsPost(ChargingStationOperator.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region UpdateStaticData(ChargingStationOperator, ...)

        /// <summary>
        /// Update the EVSE data of the given charging station operator within the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStationOperator">A charging station operator.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.UpdateStaticData(ChargingStationOperator  ChargingStationOperator,

                                             DateTime?                Timestamp,
                                             CancellationToken?       CancellationToken,
                                             EventTracking_Id         EventTrackingId,
                                             TimeSpan?                RequestTimeout)

        {

            #region Initial checks

            if (ChargingStationOperator == null)
                throw new ArgumentNullException(nameof(ChargingStationOperator), "The given charging station operator must not be null!");

            #endregion


            return (await StationsPost(ChargingStationOperator.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region DeleteStaticData(ChargingStationOperator, ...)

        /// <summary>
        /// Delete the EVSE data of the given charging station operator from the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStationOperator">A charging station operator.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.DeleteStaticData(ChargingStationOperator  ChargingStationOperator,

                                             DateTime?                Timestamp,
                                             CancellationToken?       CancellationToken,
                                             EventTracking_Id         EventTrackingId,
                                             TimeSpan?                RequestTimeout)

        {

            #region Initial checks

            if (ChargingStationOperator == null)
                throw new ArgumentNullException(nameof(ChargingStationOperator), "The given charging station operator must not be null!");

            #endregion


            return (await StationsPost(ChargingStationOperator.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion


        #region SetStaticData   (ChargingStationOperators, ...)

        /// <summary>
        /// Set the EVSE data of the given enumeration of charging station operators as new static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStationOperators">An enumeration of charging station operators.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.SetStaticData(IEnumerable<ChargingStationOperator>  ChargingStationOperators,

                                          DateTime?                             Timestamp,
                                          CancellationToken?                    CancellationToken,
                                          EventTracking_Id                      EventTrackingId,
                                          TimeSpan?                             RequestTimeout)

        {

            #region Initial checks

            if (ChargingStationOperators == null)
                throw new ArgumentNullException(nameof(ChargingStationOperators), "The given enumeration of charging station operators must not be null!");

            #endregion


            return (await StationsPost(ChargingStationOperators.SafeSelectMany(stationoperator => stationoperator.ChargingStations),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region AddStaticData   (ChargingStationOperators, ...)

        /// <summary>
        /// Add the EVSE data of the given enumeration of charging station operators to the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStationOperators">An enumeration of charging station operators.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.AddStaticData(IEnumerable<ChargingStationOperator>  ChargingStationOperators,

                                          DateTime?                             Timestamp,
                                          CancellationToken?                    CancellationToken,
                                          EventTracking_Id                      EventTrackingId,
                                          TimeSpan?                             RequestTimeout)

        {

            #region Initial checks

            if (ChargingStationOperators == null)
                throw new ArgumentNullException(nameof(ChargingStationOperators), "The given enumeration of charging station operators must not be null!");

            #endregion

            return (await StationsPost(ChargingStationOperators.SafeSelectMany(stationoperator => stationoperator.ChargingStations),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region UpdateStaticData(ChargingStationOperators, ...)

        /// <summary>
        /// Update the EVSE data of the given enumeration of charging station operators within the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStationOperators">An enumeration of charging station operators.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.UpdateStaticData(IEnumerable<ChargingStationOperator>  ChargingStationOperators,

                                             DateTime?                             Timestamp,
                                             CancellationToken?                    CancellationToken,
                                             EventTracking_Id                      EventTrackingId,
                                             TimeSpan?                             RequestTimeout)

        {

            #region Initial checks

            if (ChargingStationOperators == null)
                throw new ArgumentNullException(nameof(ChargingStationOperators), "The given enumeration of charging station operators must not be null!");

            #endregion

            return (await StationsPost(ChargingStationOperators.SafeSelectMany(stationoperator => stationoperator.ChargingStations),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region DeleteStaticData(ChargingStationOperators, ...)

        /// <summary>
        /// Delete the EVSE data of the given enumeration of charging station operators from the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStationOperators">An enumeration of charging station operators.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.DeleteStaticData(IEnumerable<ChargingStationOperator>  ChargingStationOperators,

                                             DateTime?                             Timestamp,
                                             CancellationToken?                    CancellationToken,
                                             EventTracking_Id                      EventTrackingId,
                                             TimeSpan?                             RequestTimeout)

        {

            #region Initial checks

            if (ChargingStationOperators == null)
                throw new ArgumentNullException(nameof(ChargingStationOperators), "The given enumeration of charging station operators must not be null!");

            #endregion

            return (await StationsPost(ChargingStationOperators.SafeSelectMany(stationoperator => stationoperator.ChargingStations),

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion


        #region UpdateAdminStatus(AdminStatusUpdates, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of charging station operator admin status updates.
        /// </summary>
        /// <param name="AdminStatusUpdates">An enumeration of charging station operator admin status updates.</param>
        /// <param name="TransmissionType">Whether to send the charging station operator admin status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        Task<PushChargingStationOperatorAdminStatusResult>

            ISendAdminStatus.UpdateAdminStatus(IEnumerable<ChargingStationOperatorAdminStatusUpdate>  AdminStatusUpdates,
                                               TransmissionTypes                                      TransmissionType,

                                               DateTime?                                              Timestamp,
                                               CancellationToken?                                     CancellationToken,
                                               EventTracking_Id                                       EventTrackingId,
                                               TimeSpan?                                              RequestTimeout)


                => Task.FromResult(PushChargingStationOperatorAdminStatusResult.NoOperation(Id, this));

        #endregion

        #region UpdateStatus     (StatusUpdates,      TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of charging station operator status updates.
        /// </summary>
        /// <param name="StatusUpdates">An enumeration of charging station operator status updates.</param>
        /// <param name="TransmissionType">Whether to send the charging station operator status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        Task<PushChargingStationOperatorStatusResult>

            ISendStatus.UpdateStatus(IEnumerable<ChargingStationOperatorStatusUpdate>  StatusUpdates,
                                     TransmissionTypes                                 TransmissionType,

                                     DateTime?                                         Timestamp,
                                     CancellationToken?                                CancellationToken,
                                     EventTracking_Id                                  EventTrackingId,
                                     TimeSpan?                                         RequestTimeout)


                => Task.FromResult(PushChargingStationOperatorStatusResult.NoOperation(Id, this));

        #endregion

        #endregion

        #region (Set/Add/Update/Delete) Roaming network...

        #region SetStaticData   (RoamingNetwork, ...)

        /// <summary>
        /// Set the EVSE data of the given roaming network as new static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="RoamingNetwork">A roaming network.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.SetStaticData(RoamingNetwork      RoamingNetwork,

                                          DateTime?           Timestamp,
                                          CancellationToken?  CancellationToken,
                                          EventTracking_Id    EventTrackingId,
                                          TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (RoamingNetwork == null)
                throw new ArgumentNullException(nameof(RoamingNetwork), "The given roaming network must not be null!");

            #endregion

            return (await StationsPost(RoamingNetwork.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region AddStaticData   (RoamingNetwork, ...)

        /// <summary>
        /// Add the EVSE data of the given roaming network to the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="RoamingNetwork">A roaming network.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.AddStaticData(RoamingNetwork      RoamingNetwork,

                                          DateTime?           Timestamp,
                                          CancellationToken?  CancellationToken,
                                          EventTracking_Id    EventTrackingId,
                                          TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (RoamingNetwork == null)
                throw new ArgumentNullException(nameof(RoamingNetwork), "The given roaming network must not be null!");

            #endregion

            return (await StationsPost(RoamingNetwork.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region UpdateStaticData(RoamingNetwork, ...)

        /// <summary>
        /// Update the EVSE data of the given roaming network within the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="RoamingNetwork">A roaming network.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.UpdateStaticData(RoamingNetwork      RoamingNetwork,

                                             DateTime?           Timestamp,
                                             CancellationToken?  CancellationToken,
                                             EventTracking_Id    EventTrackingId,
                                             TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (RoamingNetwork == null)
                throw new ArgumentNullException(nameof(RoamingNetwork), "The given roaming network must not be null!");

            #endregion

            return (await StationsPost(RoamingNetwork.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion

        #region DeleteStaticData(RoamingNetwork, ...)

        /// <summary>
        /// Delete the EVSE data of the given roaming network from the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="RoamingNetwork">A roaming network to upload.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        async Task<PushEVSEDataResult>

            ISendData.DeleteStaticData(RoamingNetwork      RoamingNetwork,

                                             DateTime?           Timestamp,
                                             CancellationToken?  CancellationToken,
                                             EventTracking_Id    EventTrackingId,
                                             TimeSpan?           RequestTimeout)

        {

            #region Initial checks

            if (RoamingNetwork == null)
                throw new ArgumentNullException(nameof(RoamingNetwork), "The given roaming network must not be null!");

            #endregion

            return (await StationsPost(RoamingNetwork.ChargingStations,

                                      Timestamp,
                                      CancellationToken,
                                      EventTrackingId,
                                      RequestTimeout).

                          ConfigureAwait(false)).

                          ToPushEVSEDataResult();

        }

        #endregion


        #region UpdateAdminStatus(AdminStatusUpdates, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of roaming network admin status updates.
        /// </summary>
        /// <param name="AdminStatusUpdates">An enumeration of roaming network admin status updates.</param>
        /// <param name="TransmissionType">Whether to send the roaming network admin status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        Task<PushRoamingNetworkAdminStatusResult>

            ISendAdminStatus.UpdateAdminStatus(IEnumerable<RoamingNetworkAdminStatusUpdate>  AdminStatusUpdates,
                                               TransmissionTypes                             TransmissionType,

                                               DateTime?                                     Timestamp,
                                               CancellationToken?                            CancellationToken,
                                               EventTracking_Id                              EventTrackingId,
                                               TimeSpan?                                     RequestTimeout)


                => Task.FromResult(PushRoamingNetworkAdminStatusResult.NoOperation(Id, this));

        #endregion

        #region UpdateStatus     (StatusUpdates,      TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of roaming network status updates.
        /// </summary>
        /// <param name="StatusUpdates">An enumeration of roaming network status updates.</param>
        /// <param name="TransmissionType">Whether to send the roaming network status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        Task<PushRoamingNetworkStatusResult>

            ISendStatus.UpdateStatus(IEnumerable<RoamingNetworkStatusUpdate>  StatusUpdates,
                                     TransmissionTypes                        TransmissionType,

                                     DateTime?                                Timestamp,
                                     CancellationToken?                       CancellationToken,
                                     EventTracking_Id                         EventTrackingId,
                                     TimeSpan?                                RequestTimeout)


                => Task.FromResult(PushRoamingNetworkStatusResult.NoOperation(Id, this));

        #endregion

        #endregion

        #endregion

        #region AuthorizeStart/-Stop  directly...

        #region AuthorizeStart(AuthIdentification,                    ChargingProduct = null, SessionId = null, OperatorId = null, ...)

        /// <summary>
        /// Create an authorize start request.
        /// </summary>
        /// <param name="AuthIdentification">An user identification.</param>
        /// <param name="ChargingProduct">An optional charging product.</param>
        /// <param name="SessionId">An optional session identification.</param>
        /// <param name="OperatorId">An optional charging station operator identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<AuthStartResult>

            AuthorizeStart(AuthIdentification           AuthIdentification,
                           ChargingProduct              ChargingProduct     = null,
                           ChargingSession_Id?          SessionId           = null,
                           ChargingStationOperator_Id?  OperatorId          = null,

                           DateTime?                    Timestamp           = null,
                           CancellationToken?           CancellationToken   = null,
                           EventTracking_Id             EventTrackingId     = null,
                           TimeSpan?                    RequestTimeout      = null)
        {

            #region Initial checks

            if (AuthIdentification == null)
                throw new ArgumentNullException(nameof(AuthIdentification),   "The given authentication token must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;

            #endregion

            #region Send OnAuthorizeStartRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnAuthorizeStartRequest?.Invoke(StartTime,
                                                Timestamp.Value,
                                                this,
                                                Id.ToString(),
                                                EventTrackingId,
                                                RoamingNetwork.Id,
                                                OperatorId,
                                                AuthIdentification,
                                                ChargingProduct,
                                                SessionId,
                                                RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeStartRequest));
            }

            #endregion


            DateTime         Endtime;
            TimeSpan         Runtime;
            AuthStartResult  result;

            if (DisableAuthentication)
            {
                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = AuthStartResult.AdminDown(Id,
                                                     this,
                                                     SessionId,
                                                     Runtime);
            }

            else
            {

                var response = await CPORoaming.
                                         RFIDVerify(AuthIdentification.AuthToken.ToOIOI(),

                                                    Timestamp,
                                                    CancellationToken,
                                                    EventTrackingId,
                                                    RequestTimeout).ConfigureAwait(false);


                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                    response.Content        != null              &&
                    response.Content.Code   == ResponseCodes.Success)
                {

                    result = AuthStartResult.Authorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

                }

                else
                    result = AuthStartResult.NotAuthorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

            }


            #region Send OnAuthorizeStartResponse event

            try
            {

                OnAuthorizeStartResponse?.Invoke(Endtime,
                                                 Timestamp.Value,
                                                 this,
                                                 Id.ToString(),
                                                 EventTrackingId,
                                                 RoamingNetwork.Id,
                                                 OperatorId,
                                                 AuthIdentification,
                                                 ChargingProduct,
                                                 SessionId,
                                                 RequestTimeout,
                                                 result,
                                                 Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeStartResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region AuthorizeStart(AuthIdentification, EVSEId,            ChargingProduct = null, SessionId = null, OperatorId = null, ...)

        /// <summary>
        /// Create an authorize start request at the given EVSE.
        /// </summary>
        /// <param name="AuthIdentification">An user identification.</param>
        /// <param name="EVSEId">The unique identification of an EVSE.</param>
        /// <param name="ChargingProduct">An optional charging product.</param>
        /// <param name="SessionId">An optional session identification.</param>
        /// <param name="OperatorId">An optional charging station operator identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<AuthStartEVSEResult>

            AuthorizeStart(AuthIdentification           AuthIdentification,
                           EVSE_Id                      EVSEId,
                           ChargingProduct              ChargingProduct     = null,   // [maxlength: 100]
                           ChargingSession_Id?          SessionId           = null,
                           ChargingStationOperator_Id?  OperatorId          = null,

                           DateTime?                    Timestamp           = null,
                           CancellationToken?           CancellationToken   = null,
                           EventTracking_Id             EventTrackingId     = null,
                           TimeSpan?                    RequestTimeout      = null)

        {

            #region Initial checks

            if (AuthIdentification == null)
                throw new ArgumentNullException(nameof(AuthIdentification),  "The given authentication token must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;

            #endregion

            #region Send OnAuthorizeEVSEStartRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnAuthorizeEVSEStartRequest?.Invoke(StartTime,
                                                    Timestamp.Value,
                                                    this,
                                                    Id.ToString(),
                                                    EventTrackingId,
                                                    RoamingNetwork.Id,
                                                    OperatorId,
                                                    AuthIdentification,
                                                    EVSEId,
                                                    ChargingProduct,
                                                    SessionId,
                                                    new ISendAuthorizeStartStop[0],
                                                    RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeEVSEStartRequest));
            }

            #endregion


            DateTime             Endtime;
            TimeSpan             Runtime;
            AuthStartEVSEResult  result;

            if (DisableAuthentication)
            {

                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = AuthStartEVSEResult.AdminDown(Id,
                                                         this,
                                                         SessionId,
                                                         Runtime);

            }

            else
            {

                var response  = await CPORoaming.
                                          RFIDVerify(AuthIdentification.AuthToken.ToOIOI(),

                                                     Timestamp,
                                                     CancellationToken,
                                                     EventTrackingId,
                                                     RequestTimeout);


                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                    response.Content        != null              &&
                    response.Content.Code   == ResponseCodes.Success)
                {

                    result = AuthStartEVSEResult.Authorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

                }

                else
                    result = AuthStartEVSEResult.NotAuthorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

            }


            #region Send OnAuthorizeEVSEStartResponse event

            try
            {

                OnAuthorizeEVSEStartResponse?.Invoke(Endtime,
                                                     Timestamp.Value,
                                                     this,
                                                     Id.ToString(),
                                                     EventTrackingId,
                                                     RoamingNetwork.Id,
                                                     OperatorId,
                                                     AuthIdentification,
                                                     EVSEId,
                                                     ChargingProduct,
                                                     SessionId,
                                                     new ISendAuthorizeStartStop[0],
                                                     RequestTimeout,
                                                     result,
                                                     Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeEVSEStartResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region AuthorizeStart(AuthIdentification, ChargingStationId, ChargingProduct = null, SessionId = null, OperatorId = null, ...)

        /// <summary>
        /// Create an authorize start request at the given charging station.
        /// </summary>
        /// <param name="AuthIdentification">An user identification.</param>
        /// <param name="ChargingStationId">The unique identification charging station.</param>
        /// <param name="ChargingProduct">An optional charging product.</param>
        /// <param name="SessionId">An optional session identification.</param>
        /// <param name="OperatorId">An optional charging station operator identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<AuthStartChargingStationResult>

            AuthorizeStart(AuthIdentification           AuthIdentification,
                           ChargingStation_Id           ChargingStationId,
                           ChargingProduct              ChargingProduct     = null,   // [maxlength: 100]
                           ChargingSession_Id?          SessionId           = null,
                           ChargingStationOperator_Id?  OperatorId          = null,

                           DateTime?                    Timestamp           = null,
                           CancellationToken?           CancellationToken   = null,
                           EventTracking_Id             EventTrackingId     = null,
                           TimeSpan?                    RequestTimeout      = null)

        {

            #region Initial checks

            if (AuthIdentification == null)
                throw new ArgumentNullException(nameof(AuthIdentification), "The given authentication token must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;

            #endregion

            #region Send OnAuthorizeChargingStationStartRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnAuthorizeChargingStationStartRequest?.Invoke(StartTime,
                                                               Timestamp.Value,
                                                               this,
                                                               Id.ToString(),
                                                               EventTrackingId,
                                                               RoamingNetwork.Id,
                                                               OperatorId,
                                                               AuthIdentification,
                                                               ChargingStationId,
                                                               ChargingProduct,
                                                               SessionId,
                                                               RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeChargingStationStartRequest));
            }

            #endregion


            DateTime                        Endtime;
            TimeSpan                        Runtime;
            AuthStartChargingStationResult  result;

            if (DisableAuthentication)
            {
                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = AuthStartChargingStationResult.AdminDown(Id,
                                                                    this,
                                                                    SessionId,
                                                                    Runtime);
            }

            else
            {

                var response  = await CPORoaming.
                                          RFIDVerify(AuthIdentification.AuthToken.ToOIOI(),

                                                     Timestamp,
                                                     CancellationToken,
                                                     EventTrackingId,
                                                     RequestTimeout).ConfigureAwait(false);


                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                    response.Content        != null              &&
                    response.Content.Code   == ResponseCodes.Success)
                {

                    result = AuthStartChargingStationResult.Authorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

                }

                else
                    result = AuthStartChargingStationResult.NotAuthorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

            }


            #region Send OnAuthorizeChargingStationStartResponse event

            try
            {

                OnAuthorizeChargingStationStartResponse?.Invoke(Endtime,
                                                                Timestamp.Value,
                                                                this,
                                                                Id.ToString(),
                                                                EventTrackingId,
                                                                RoamingNetwork.Id,
                                                                OperatorId,
                                                                AuthIdentification,
                                                                ChargingStationId,
                                                                ChargingProduct,
                                                                SessionId,
                                                                RequestTimeout,
                                                                result,
                                                                Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeChargingStationStartResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region AuthorizeStart(AuthIdentification, ChargingPoolId,    ChargingProduct = null, SessionId = null, OperatorId = null, ...)

        /// <summary>
        /// Create an authorize start request at the given charging pool.
        /// </summary>
        /// <param name="AuthIdentification">A user identification.</param>
        /// <param name="ChargingPoolId">The unique identification charging pool.</param>
        /// <param name="ChargingProduct">An optional charging product.</param>
        /// <param name="SessionId">An optional session identification.</param>
        /// <param name="OperatorId">An optional charging station operator identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<AuthStartChargingPoolResult>

            AuthorizeStart(AuthIdentification           AuthIdentification,
                           ChargingPool_Id              ChargingPoolId,
                           ChargingProduct              ChargingProduct     = null,   // [maxlength: 100]
                           ChargingSession_Id?          SessionId           = null,
                           ChargingStationOperator_Id?  OperatorId          = null,

                           DateTime?                    Timestamp           = null,
                           CancellationToken?           CancellationToken   = null,
                           EventTracking_Id             EventTrackingId     = null,
                           TimeSpan?                    RequestTimeout      = null)

        {

            #region Initial checks

            if (AuthIdentification == null)
                throw new ArgumentNullException(nameof(AuthIdentification), "The given authentication token must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;

            #endregion

            #region Send OnAuthorizeChargingPoolStartRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnAuthorizeChargingPoolStartRequest?.Invoke(StartTime,
                                                            Timestamp.Value,
                                                            this,
                                                            Id.ToString(),
                                                            EventTrackingId,
                                                            RoamingNetwork.Id,
                                                            OperatorId,
                                                            AuthIdentification,
                                                            ChargingPoolId,
                                                            ChargingProduct,
                                                            SessionId,
                                                            RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeChargingPoolStartRequest));
            }

            #endregion


            DateTime                     Endtime;
            TimeSpan                     Runtime;
            AuthStartChargingPoolResult  result;

            if (DisableAuthentication)
            {
                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = AuthStartChargingPoolResult.AdminDown(Id,
                                                                 this,
                                                                 SessionId,
                                                                 Runtime);
            }

            else
            {

                var response  = await CPORoaming.
                                          RFIDVerify(AuthIdentification.AuthToken.ToOIOI(),

                                                     Timestamp,
                                                     CancellationToken,
                                                     EventTrackingId,
                                                     RequestTimeout).ConfigureAwait(false);


                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                    response.Content        != null              &&
                    response.Content.Code   == ResponseCodes.Success)
                {

                    result = AuthStartChargingPoolResult.Authorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

                }

                else
                    result = AuthStartChargingPoolResult.NotAuthorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

            }


            #region Send OnAuthorizeChargingPoolStartResponse event

            try
            {

                OnAuthorizeChargingPoolStartResponse?.Invoke(Endtime,
                                                             Timestamp.Value,
                                                             this,
                                                             Id.ToString(),
                                                             EventTrackingId,
                                                             RoamingNetwork.Id,
                                                             OperatorId,
                                                             AuthIdentification,
                                                             ChargingPoolId,
                                                             ChargingProduct,
                                                             SessionId,
                                                             RequestTimeout,
                                                             result,
                                                             Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeChargingPoolStartResponse));
            }

            #endregion

            return result;

        }

        #endregion


        // UID => Not everybody can stop any session, but maybe another
        //        UID than the UID which started the session!
        //        (e.g. car sharing)

        #region AuthorizeStop(SessionId, AuthIdentification,                    OperatorId = null, ...)

        /// <summary>
        /// Create an authorize stop request.
        /// </summary>
        /// <param name="SessionId">The session identification from the AuthorizeStart request.</param>
        /// <param name="AuthIdentification">An user identification.</param>
        /// <param name="OperatorId">An optional charging station operator identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<AuthStopResult>

            AuthorizeStop(ChargingSession_Id           SessionId,
                          AuthIdentification           AuthIdentification,
                          ChargingStationOperator_Id?  OperatorId          = null,

                          DateTime?                    Timestamp           = null,
                          CancellationToken?           CancellationToken   = null,
                          EventTracking_Id             EventTrackingId     = null,
                          TimeSpan?                    RequestTimeout      = null)
        {

            #region Initial checks

            if (AuthIdentification == null)
                throw new ArgumentNullException(nameof(AuthIdentification),  "The given authentication token must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;

            #endregion

            #region Send OnAuthorizeStopRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnAuthorizeStopRequest?.Invoke(StartTime,
                                               Timestamp.Value,
                                               this,
                                               Id.ToString(),
                                               EventTrackingId,
                                               RoamingNetwork.Id,
                                               OperatorId,
                                               SessionId,
                                               AuthIdentification,
                                               RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeStopRequest));
            }

            #endregion


            DateTime        Endtime;
            TimeSpan        Runtime;
            AuthStopResult  result;

            if (DisableAuthentication)
            {
                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = AuthStopResult.AdminDown(Id,
                                                    this,
                                                    SessionId,
                                                    Runtime);
            }

            else
            {

                var response = await CPORoaming.RFIDVerify(AuthIdentification.AuthToken.ToOIOI(),

                                                           Timestamp,
                                                           CancellationToken,
                                                           EventTrackingId,
                                                           RequestTimeout).ConfigureAwait(false);


                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                    response.Content        != null              &&
                    response.Content.Code   == ResponseCodes.Success)
                {

                    result = AuthStopResult.Authorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

                }
                else
                    result = AuthStopResult.NotAuthorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

            }


            #region Send OnAuthorizeStopResponse event

            try
            {

                OnAuthorizeStopResponse?.Invoke(Endtime,
                                                Timestamp.Value,
                                                this,
                                                Id.ToString(),
                                                EventTrackingId,
                                                RoamingNetwork.Id,
                                                OperatorId,
                                                SessionId,
                                                AuthIdentification,
                                                RequestTimeout,
                                                result,
                                                Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeStopResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region AuthorizeStop(SessionId, AuthIdentification, EVSEId,            OperatorId = null, ...)

        /// <summary>
        /// Create an authorize stop request at the given EVSE.
        /// </summary>
        /// <param name="SessionId">The session identification from the AuthorizeStart request.</param>
        /// <param name="AuthIdentification">A user identification.</param>
        /// <param name="EVSEId">The unique identification of an EVSE.</param>
        /// <param name="OperatorId">An optional charging station operator identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<AuthStopEVSEResult>

            AuthorizeStop(ChargingSession_Id           SessionId,
                          AuthIdentification           AuthIdentification,
                          WWCP.EVSE_Id                 EVSEId,
                          ChargingStationOperator_Id?  OperatorId          = null,

                          DateTime?                    Timestamp           = null,
                          CancellationToken?           CancellationToken   = null,
                          EventTracking_Id             EventTrackingId     = null,
                          TimeSpan?                    RequestTimeout      = null)
        {

            #region Initial checks

            if (AuthIdentification  == null)
                throw new ArgumentNullException(nameof(AuthIdentification), "The given authentication token must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;

            #endregion

            #region Send OnAuthorizeEVSEStopRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnAuthorizeEVSEStopRequest?.Invoke(StartTime,
                                                   Timestamp.Value,
                                                   this,
                                                   Id.ToString(),
                                                   EventTrackingId,
                                                   RoamingNetwork.Id,
                                                   OperatorId,
                                                   EVSEId,
                                                   SessionId,
                                                   AuthIdentification,
                                                   RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeEVSEStopRequest));
            }

            #endregion


            DateTime            Endtime;
            TimeSpan            Runtime;
            AuthStopEVSEResult  result;

            if (DisableAuthentication)
            {
                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = AuthStopEVSEResult.AdminDown(Id,
                                                        this,
                                                        SessionId,
                                                        Runtime);
            }

            else
            {

                var response  = await CPORoaming.RFIDVerify(AuthIdentification.AuthToken.ToOIOI(),

                                                            Timestamp,
                                                            CancellationToken,
                                                            EventTrackingId,
                                                            RequestTimeout).ConfigureAwait(false);


                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                    response.Content        != null              &&
                    response.Content.Code   == ResponseCodes.Success)
                {

                    result = AuthStopEVSEResult.Authorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

                }
                else
                    result = AuthStopEVSEResult.NotAuthorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

            }


            #region Send OnAuthorizeEVSEStopResponse event

            try
            {

                OnAuthorizeEVSEStopResponse?.Invoke(Endtime,
                                                    Timestamp.Value,
                                                    this,
                                                    Id.ToString(),
                                                    EventTrackingId,
                                                    RoamingNetwork.Id,
                                                    OperatorId,
                                                    EVSEId,
                                                    SessionId,
                                                    AuthIdentification,
                                                    RequestTimeout,
                                                    result,
                                                    Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeEVSEStopResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region AuthorizeStop(SessionId, AuthIdentification, ChargingStationId, OperatorId = null, ...)

        /// <summary>
        /// Create an authorize stop request at the given charging station.
        /// </summary>
        /// <param name="SessionId">The session identification from the AuthorizeStart request.</param>
        /// <param name="AuthIdentification">An user identification.</param>
        /// <param name="ChargingStationId">The unique identification of a charging station.</param>
        /// <param name="OperatorId">An optional charging station operator identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<AuthStopChargingStationResult>

            AuthorizeStop(ChargingSession_Id           SessionId,
                          AuthIdentification           AuthIdentification,
                          ChargingStation_Id           ChargingStationId,
                          ChargingStationOperator_Id?  OperatorId          = null,

                          DateTime?                    Timestamp           = null,
                          CancellationToken?           CancellationToken   = null,
                          EventTracking_Id             EventTrackingId     = null,
                          TimeSpan?                    RequestTimeout      = null)

        {

            #region Initial checks

            if (AuthIdentification == null)
                throw new ArgumentNullException(nameof(AuthIdentification), "The given authentication token must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;

            #endregion

            #region Send OnAuthorizeChargingStationStopRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnAuthorizeChargingStationStopRequest?.Invoke(StartTime,
                                                              Timestamp.Value,
                                                              this,
                                                              Id.ToString(),
                                                              EventTrackingId,
                                                              RoamingNetwork.Id,
                                                              OperatorId,
                                                              ChargingStationId,
                                                              SessionId,
                                                              AuthIdentification,
                                                              RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeChargingStationStopRequest));
            }

            #endregion


            DateTime                       Endtime;
            TimeSpan                       Runtime;
            AuthStopChargingStationResult  result;

            if (DisableAuthentication)
            {
                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = AuthStopChargingStationResult.AdminDown(Id,
                                                                   this,
                                                                   SessionId,
                                                                   Runtime);
            }

            else
            {

                var response  = await CPORoaming.RFIDVerify(AuthIdentification.AuthToken.ToOIOI(),

                                                            Timestamp,
                                                            CancellationToken,
                                                            EventTrackingId,
                                                            RequestTimeout).ConfigureAwait(false);


                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                    response.Content        != null              &&
                    response.Content.Code   == ResponseCodes.Success)
                {

                    result = AuthStopChargingStationResult.Authorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

                }
                else
                    result = AuthStopChargingStationResult.NotAuthorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

            }


            #region Send OnAuthorizeChargingStationStopResponse event

            try
            {

                OnAuthorizeChargingStationStopResponse?.Invoke(Endtime,
                                                               Timestamp.Value,
                                                               this,
                                                               Id.ToString(),
                                                               EventTrackingId,
                                                               RoamingNetwork.Id,
                                                               OperatorId,
                                                               ChargingStationId,
                                                               SessionId,
                                                               AuthIdentification,
                                                               RequestTimeout,
                                                               result,
                                                               Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeChargingStationStopResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region AuthorizeStop(SessionId, AuthIdentification, ChargingPoolId,    OperatorId = null, ...)

        /// <summary>
        /// Create an authorize stop request at the given charging pool.
        /// </summary>
        /// <param name="SessionId">The session identification from the AuthorizeStart request.</param>
        /// <param name="AuthIdentification">An user identification.</param>
        /// <param name="ChargingPoolId">The unique identification of a charging pool.</param>
        /// <param name="OperatorId">An optional charging station operator identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<AuthStopChargingPoolResult>

            AuthorizeStop(ChargingSession_Id           SessionId,
                          AuthIdentification           AuthIdentification,
                          ChargingPool_Id              ChargingPoolId,
                          ChargingStationOperator_Id?  OperatorId          = null,

                          DateTime?                    Timestamp           = null,
                          CancellationToken?           CancellationToken   = null,
                          EventTracking_Id             EventTrackingId     = null,
                          TimeSpan?                    RequestTimeout      = null)

        {

            #region Initial checks

            if (AuthIdentification == null)
                throw new ArgumentNullException(nameof(AuthIdentification), "The given authentication token must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;

            #endregion

            #region Send OnAuthorizeChargingPoolStopRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnAuthorizeChargingPoolStopRequest?.Invoke(StartTime,
                                                           Timestamp.Value,
                                                           this,
                                                           Id.ToString(),
                                                           EventTrackingId,
                                                           RoamingNetwork.Id,
                                                           OperatorId,
                                                           ChargingPoolId,
                                                           SessionId,
                                                           AuthIdentification,
                                                           RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeChargingPoolStopRequest));
            }

            #endregion


            DateTime                    Endtime;
            TimeSpan                    Runtime;
            AuthStopChargingPoolResult  result;

            if (DisableAuthentication)
            {
                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = AuthStopChargingPoolResult.AdminDown(Id,
                                                                this,
                                                                SessionId,
                                                                Runtime);
            }

            else
            {

                var response  = await CPORoaming.RFIDVerify(AuthIdentification.AuthToken.ToOIOI(),

                                                            Timestamp,
                                                            CancellationToken,
                                                            EventTrackingId,
                                                            RequestTimeout).ConfigureAwait(false);


                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                    response.Content        != null              &&
                    response.Content.Code   == ResponseCodes.Success)
                {

                    result = AuthStopChargingPoolResult.Authorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

                }
                else
                    result = AuthStopChargingPoolResult.NotAuthorized(
                                 Id,
                                 this,
                                 SessionId,
                                 ProviderId: DefaultProviderId,
                                 Runtime:    Runtime
                             );

            }


            #region Send OnAuthorizeChargingPoolStopResponse event

            try
            {

                OnAuthorizeChargingPoolStopResponse?.Invoke(Endtime,
                                                            Timestamp.Value,
                                                            this,
                                                            Id.ToString(),
                                                            EventTrackingId,
                                                            RoamingNetwork.Id,
                                                            OperatorId,
                                                            ChargingPoolId,
                                                            SessionId,
                                                            AuthIdentification,
                                                            RequestTimeout,
                                                            result,
                                                            Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeChargingPoolStopResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #endregion

        #region SendChargeDetailRecords(ChargeDetailRecords, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Send charge detail records to an OIOI server.
        /// </summary>
        /// <param name="ChargeDetailRecords">An enumeration of charge detail records.</param>
        /// <param name="TransmissionType">Whether to send the CDR directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public async Task<SendCDRsResult>

            SendChargeDetailRecords(IEnumerable<ChargeDetailRecord>  ChargeDetailRecords,
                                    TransmissionTypes                TransmissionType    = TransmissionTypes.Enqueue,

                                    DateTime?                        Timestamp           = null,
                                    CancellationToken?               CancellationToken   = null,
                                    EventTracking_Id                 EventTrackingId     = null,
                                    TimeSpan?                        RequestTimeout      = null)

        {

            #region Initial checks

            if (ChargeDetailRecords == null)
                throw new ArgumentNullException(nameof(ChargeDetailRecords),  "The given enumeration of charge detail records must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (!CancellationToken.HasValue)
                CancellationToken = new CancellationTokenSource().Token;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPOClient?.RequestTimeout;


            DateTime        Endtime;
            TimeSpan        Runtime;
            SendCDRsResult  result;

            #endregion

            #region Send OnSendCDRsRequest event

            var StartTime = DateTime.UtcNow;

            try
            {

                OnSendCDRsRequest?.Invoke(StartTime,
                                          Timestamp.Value,
                                          this,
                                          Id.ToString(),
                                          EventTrackingId,
                                          RoamingNetwork.Id,
                                          new ChargeDetailRecord[0],
                                          ChargeDetailRecords,
                                          RequestTimeout);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRsRequest));
            }

            #endregion


            #region if disabled => 'AdminDown'...

            if (DisableSendChargeDetailRecords)
            {

                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = SendCDRsResult.AdminDown(Id,
                                                    this,
                                                    ChargeDetailRecords,
                                                    Runtime: Runtime);

            }

            #endregion

            else
            {

                var LockTaken = await FlushOICPChargeDetailRecordsLock.WaitAsync(TimeSpan.FromSeconds(60));

                try
                {

                    if (LockTaken)
                    {

                        var SendCDRsResults = new List<SendCDRResult>();

                        #region if enqueuing is requested...

                        if (TransmissionType == TransmissionTypes.Enqueue)
                        {

                            #region Send OnEnqueueSendCDRRequest event

                            try
                            {

                                OnEnqueueSendCDRsRequest?.Invoke(DateTime.UtcNow,
                                                                 Timestamp.Value,
                                                                 this,
                                                                 Id.ToString(),
                                                                 EventTrackingId,
                                                                 RoamingNetwork.Id,
                                                                 new ChargeDetailRecord[0],
                                                                 ChargeDetailRecords,
                                                                 RequestTimeout);

                            }
                            catch (Exception e)
                            {
                                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRsRequest));
                            }

                            #endregion

                            foreach (var ChargeDetailRecord in ChargeDetailRecords)
                            {

                                try
                                {

                                    OICP_ChargeDetailRecords_Queue.Add(ChargeDetailRecord.ToOIOI());// _WWCPChargeDetailRecord2OICPChargeDetailRecord));
                                    SendCDRsResults.Add(new SendCDRResult(ChargeDetailRecord,
                                                                          SendCDRResultTypes.Enqueued));

                                }
                                catch (Exception e)
                                {
                                    SendCDRsResults.Add(new SendCDRResult(ChargeDetailRecord,
                                                                          SendCDRResultTypes.CouldNotConvertCDRFormat,
                                                                          e.Message));
                                }

                            }

                            Endtime  = DateTime.UtcNow;
                            Runtime  = Endtime - StartTime;
                            result   = SendCDRsResult.Enqueued(Id,
                                                               this,
                                                               ChargeDetailRecords,
                                                               "Enqueued for at least " + FlushChargeDetailRecordsEvery.TotalSeconds + " seconds!",
                                                               //SendCDRsResults.SafeWhere(cdrresult => cdrresult.Result != SendCDRResultTypes.Enqueued),
                                                               Runtime: Runtime);

                            FlushChargeDetailRecordsTimer.Change(FlushChargeDetailRecordsEvery, TimeSpan.FromMilliseconds(-1));

                        }

                        #endregion

                        #region ...or send at once!

                        else
                        {

                            HTTPResponse<SessionPostResponse> response;

                            foreach (var ChargeDetailRecord in ChargeDetailRecords)
                            {

                                try
                                {

                                    response = await CPORoaming.SessionPost(ChargeDetailRecord.ToOIOI(), //_WWCPChargeDetailRecord2OICPChargeDetailRecord),

                                                                                       Timestamp,
                                                                                       CancellationToken,
                                                                                       EventTrackingId,
                                                                                       RequestTimeout);

                                    if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                                        response.Content        != null              &&
                                        response.Content.Code == ResponseCodes.Success)
                                    {
                                        SendCDRsResults.Add(new SendCDRResult(ChargeDetailRecord,
                                                                              SendCDRResultTypes.Success));
                                    }

                                    else
                                        SendCDRsResults.Add(new SendCDRResult(ChargeDetailRecord,
                                                                              SendCDRResultTypes.Error,
                                                                              response.HTTPBodyAsUTF8String));

                                }
                                catch (Exception e)
                                {
                                    SendCDRsResults.Add(new SendCDRResult(ChargeDetailRecord,
                                                                          SendCDRResultTypes.CouldNotConvertCDRFormat,
                                                                          e.Message));
                                }

                            }

                            Endtime  = DateTime.UtcNow;
                            Runtime  = Endtime - StartTime;

                            if      (SendCDRsResults.All(cdrresult => cdrresult.Result == SendCDRResultTypes.Success))
                                result = SendCDRsResult.Success(Id,
                                                                this,
                                                                ChargeDetailRecords,
                                                                Runtime: Runtime);

                            else
                                result = SendCDRsResult.Error(Id,
                                                              this,
                                                              SendCDRsResults.
                                                                  Where (cdrresult => cdrresult.Result != SendCDRResultTypes.Success).
                                                                  Select(cdrresult => cdrresult.ChargeDetailRecord),
                                                              Runtime: Runtime);


                        }

                        #endregion

                    }

                    #region Could not get the lock for toooo long!

                    else
                    {

                        Endtime  = DateTime.UtcNow;
                        Runtime  = Endtime - StartTime;
                        result   = SendCDRsResult.Timeout(Id,
                                                          this,
                                                          ChargeDetailRecords,
                                                          "Could not " + (TransmissionType == TransmissionTypes.Enqueue ? "enqueue" : "send") + " charge detail records!",
                                                          //ChargeDetailRecords.SafeSelect(cdr => new SendCDRResult(cdr, SendCDRResultTypes.Timeout)),
                                                          Runtime: Runtime);

                    }

                    #endregion

                }
                finally
                {
                    if (LockTaken)
                        FlushOICPChargeDetailRecordsLock.Release();
                }

            }


            #region Send OnSendCDRsResponse event

            try
            {

                OnSendCDRsResponse?.Invoke(Endtime,
                                           Timestamp.Value,
                                           this,
                                           Id.ToString(),
                                           EventTrackingId,
                                           RoamingNetwork.Id,
                                           new ChargeDetailRecord[0],
                                           ChargeDetailRecords,
                                           RequestTimeout,
                                           result,
                                           Runtime);

            }
            catch (Exception e)
            {
                e.Log(nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRsResponse));
            }

            #endregion

            return result;

        }

        #endregion


        // -----------------------------------------------------------------------------------------------------


        #region (timer) ServiceCheck(State)

        //private void ServiceCheck(Object State)
        //{

        //    try
        //    {

        //        FlushServiceQueues().Wait();

        //    }
        //    catch (Exception e)
        //    {

        //        while (e.InnerException != null)
        //            e = e.InnerException;

        //        DebugX.LogT("ServiceCheckTimer => " + e.Message);

        //        OnWWCPCPOAdapterException?.Invoke(DateTime.UtcNow,
        //                                          this,
        //                                          e);

        //    }

        //}

        #endregion

        #region FlushServiceQueues()

        //public async Task FlushServiceQueues()
        //{

        //    FlushServiceQueuesEvent?.Invoke(this, TimeSpan.FromMilliseconds(_ServiceCheckEvery));

        //    #region Make a thread local copy of all data

        //    //ToDo: AsyncLocal is currently not implemented in Mono!
        //    //var EVSEDataQueueCopy   = new AsyncLocal<HashSet<EVSE>>();
        //    //var EVSEStatusQueueCopy = new AsyncLocal<List<EVSEStatusChange>>();

        //    var StationsToAddQueueCopy     = new ThreadLocal<HashSet<ChargingStation>>();
        //    var StationsToUpdateQueueCopy  = new ThreadLocal<HashSet<ChargingStation>>();
        //    var ChargingStationsToRemoveQueueCopy  = new ThreadLocal<HashSet<ChargingStation>>();
        //    var EVSEStatusUpdatesDelayedQueueCopy  = new ThreadLocal<List<EVSEStatusUpdate>>();
        //    //var ChargeDetailRecordsQueueCopy       = new ThreadLocal<List<ChargeDetailRecord>>();

        //    if (Monitor.TryEnter(ServiceCheckLock))
        //    {

        //        try
        //        {

        //            if (StationsToAddQueue.   Count == 0 &&
        //                StationsToUpdateQueue.Count == 0 &&
        //                ChargingStationsToRemoveQueue.Count == 0 &&
        //                EVSEStatusUpdatesDelayedQueue.Count == 0 &&
        //                ChargeDetailRecordsQueue.     Count == 0)
        //            {
        //                return;
        //            }

        //            _ServiceRunId++;

        //            // Copy 'charging stations to add', remove originals...
        //            StationsToAddQueueCopy.Value     = new HashSet<ChargingStation>(StationsToAddQueue);
        //            StationsToAddQueue.Clear();

        //            // Copy 'charging stations to update', remove originals...
        //            StationsToUpdateQueueCopy.Value  = new HashSet<ChargingStation>(StationsToUpdateQueue);
        //            StationsToUpdateQueue.Clear();

        //            // Copy 'charging stations to remove', remove originals...
        //            ChargingStationsToRemoveQueueCopy.Value  = new HashSet<ChargingStation>(ChargingStationsToRemoveQueue);
        //            ChargingStationsToRemoveQueue.Clear();

        //            //// Copy 'EVSE status updates', remove originals...
        //            EVSEStatusUpdatesDelayedQueueCopy.Value  = new List<EVSEStatusUpdate>  (EVSEStatusUpdatesDelayedQueue);
        //            EVSEStatusUpdatesDelayedQueue.Clear();

        //            // Copy 'charge detail records', remove originals...
        //            //ChargeDetailRecordsQueueCopy.Value        = new List<ChargeDetailRecord>(ChargeDetailRecordsQueue);
        //            //ChargeDetailRecordsQueue.Clear();

        //            // Stop the timer. Will be rescheduled by next data/status change...
        //            ServiceCheckTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

        //        }
        //        catch (Exception e)
        //        {

        //            while (e.InnerException != null)
        //                e = e.InnerException;

        //            DebugX.LogT(nameof(WWCPCPOAdapter) + " '" + Id + "' led to an exception: " + e.Message + Environment.NewLine + e.StackTrace);

        //        }

        //        finally
        //        {
        //            Monitor.Exit(ServiceCheckLock);
        //        }

        //    }

        //    else
        //    {

        //        Console.WriteLine("ServiceCheckLock missed!");
        //        ServiceCheckTimer.Change(_ServiceCheckEvery, TimeSpan.FromMilliseconds(-1));

        //    }

        //    #endregion

        //    // Upload status changes...
        //    if (StationsToAddQueueCopy.   Value != null ||
        //        StationsToUpdateQueueCopy.Value != null ||
        //        ChargingStationsToRemoveQueueCopy.Value != null ||
        //        EVSEStatusUpdatesDelayedQueueCopy.Value != null //||
        //        //ChargeDetailRecordsQueueCopy.     Value != null
        //        )
        //    {

        //        // Use the events to evaluate if something went wrong!

        //        var EventTrackingId = EventTracking_Id.New;

        //        #region Send new charging stations

        //        if (StationsToAddQueueCopy.Value.Count > 0)
        //        {

        //            var AddOrSetStaticDataTask = _ServiceRunId == 1
        //                                             ? (this as ISendData).SetStaticData(StationsToAddQueueCopy.Value, EventTrackingId: EventTrackingId)
        //                                             : (this as ISendData).AddStaticData(StationsToAddQueueCopy.Value, EventTrackingId: EventTrackingId);

        //            AddOrSetStaticDataTask.Wait();

        //        }

        //        #endregion

        //        #region Send updated charging stations

        //        if (StationsToUpdateQueueCopy.Value.Count > 0)
        //        {

        //            // Surpress EVSE data updates for all newly added EVSEs
        //            var EVSEsWithoutNewEVSEs = StationsToUpdateQueueCopy.Value.
        //                                           Where(evse => !StationsToAddQueueCopy.Value.Contains(evse)).
        //                                           ToArray();


        //            if (EVSEsWithoutNewEVSEs.Length > 0)
        //            {

        //                var UpdateStaticDataTask = (this as ISendData).UpdateStaticData(EVSEsWithoutNewEVSEs, EventTrackingId: EventTrackingId);

        //                UpdateStaticDataTask.Wait();

        //            }

        //        }

        //        #endregion

        //        #region Send EVSE status updates

        //        if (EVSEStatusUpdatesDelayedQueueCopy.Value.Count > 0)
        //        {

        //            var ConnectorPostStatusTask = ConnectorPostStatus(EVSEStatusUpdatesDelayedQueueCopy.Value,
        //                                                              //_ServiceRunId == 1
        //                                                              //    ? ActionTypes.fullLoad
        //                                                              //    : ActionTypes.update,
        //                                                              EventTrackingId: EventTrackingId);

        //            ConnectorPostStatusTask.Wait();

        //        }

        //        #endregion

        //        #region Send removed charging stations

        //        if (ChargingStationsToRemoveQueueCopy.Value.Count > 0)
        //        {

        //            var ChargingStationsToRemove = ChargingStationsToRemoveQueueCopy.Value.ToArray();

        //            if (ChargingStationsToRemove.Length > 0)
        //            {

        //                var ChargingStationsToRemoveTask = (this as ISendData).DeleteStaticData(ChargingStationsToRemove, EventTrackingId: EventTrackingId);

        //                ChargingStationsToRemoveTask.Wait();

        //            }

        //        }

        //        #endregion

        //        #region Send charge detail records

        //        //if (ChargeDetailRecordsQueueCopy.Value.Count > 0)
        //        //{

        //        //    var SendCDRResults = await SendChargeDetailRecords(ChargeDetailRecordsQueueCopy.Value,
        //        //                                                       TransmissionTypes.Direct,
        //        //                                                       DateTime.UtcNow,
        //        //                                                       new CancellationTokenSource().Token,
        //        //                                                       EventTrackingId,
        //        //                                                       DefaultRequestTimeout).ConfigureAwait(false);

        //        //    //ToDo: Send results events...
        //        //    //ToDo: Read to queue if it could not be sent...

        //        //}

        //        #endregion

        //    }

        //    return;

        //}

        #endregion

        #region (timer) StatusCheck(State)

        //private void StatusCheck(Object State)
        //{

        //    try
        //    {

        //        FlushFastStatusQueues().Wait();

        //    }
        //    catch (Exception e)
        //    {

        //        while (e.InnerException != null)
        //            e = e.InnerException;

        //        DebugX.LogT("StatusCheckTimer => " + e.Message);

        //        OnWWCPCPOAdapterException?.Invoke(DateTime.UtcNow,
        //                                          this,
        //                                          e);

        //    }

        //}

        #endregion

        #region FlushFastStatusQueues()

        //public async Task FlushFastStatusQueues()
        //{

        //    FlushFastStatusQueuesEvent?.Invoke(this, TimeSpan.FromMilliseconds(_StatusCheckEvery));

        //    //ToDo: AsyncLocal is currently not implemented in Mono!
        //    //var EVSEStatusQueueCopy = new AsyncLocal<List<EVSEStatusChange>>();

        //    var EVSEStatusChangesFastQueue = new ThreadLocal<List<EVSEStatusUpdate>>();

        //    #region Forward EVSE status changes to "fast" or "delayed" queue...

        //    if (Monitor.TryEnter(StatusCheckLock,
        //                         TimeSpan.FromMinutes(5)))
        //    {

        //        try
        //        {

        //            if (EVSEStatusUpdatesQueue.Count == 0)
        //                return;

        //            _StatusRunId++;

        //            #region Copy all "EVSEstatus changes" of existing EVSEs into the "fast"    queue...

        //            EVSEStatusChangesFastQueue.Value = new List<EVSEStatusUpdate>(
        //                                                         EVSEStatusUpdatesQueue.
        //                                                             Where(evsestatuschange => !StationsToAddQueue.Any(station => station.EVSEs.Any(evse => evse == evsestatuschange.EVSE)))
        //                                                     );

        //            #endregion

        //            #region Copy all "EVSEstatus changes" of __new___ EVSEs into the "delayed" queue...

        //            var _EVSEStatusChangesDelayed = EVSEStatusUpdatesQueue.Where(evsestatuschange => StationsToAddQueue.Any(station => station.EVSEs.Any(evse => evse == evsestatuschange.EVSE))).ToArray();

        //            if (_EVSEStatusChangesDelayed.Length > 0)
        //                EVSEStatusUpdatesDelayedQueue.AddRange(_EVSEStatusChangesDelayed);

        //            EVSEStatusUpdatesQueue.Clear();

        //            #endregion

        //            // Stop the status check timer.
        //            // It will be rescheduled by next EVSE status change...
        //            StatusCheckTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

        //        }
        //        catch (Exception e)
        //        {

        //            while (e.InnerException != null)
        //                e = e.InnerException;

        //            DebugX.LogT(nameof(WWCPCPOAdapter) + " '" + Id + "' led to an exception: " + e.Message + Environment.NewLine + e.StackTrace);

        //        }

        //        finally
        //        {
        //            Monitor.Exit(StatusCheckLock);
        //        }

        //    }

        //    #region Reschedule, if lock was missed!

        //    else
        //    {

        //        DebugX.LogT("StatusCheckLock missed!");
        //        StatusCheckTimer.Change(_StatusCheckEvery, TimeSpan.FromMilliseconds(-1));

        //    }

        //    #endregion

        //    #endregion

        //    #region Upload _fast_ status changes now...

        //    if (EVSEStatusChangesFastQueue.IsValueCreated)
        //    {

        //        var EventTrackingId = EventTracking_Id.New;

        //        // Use the events to evaluate if something went wrong!
        //        if (EVSEStatusChangesFastQueue.Value.Count > 0)
        //        {

        //            var PushEVSEStatusTask = ConnectorPostStatus(EVSEStatusChangesFastQueue.Value,
        //                                                         //_StatusRunId == 1
        //                                                         //    ? ActionTypes.fullLoad
        //                                                         //    : ActionTypes.update,
        //                                                         EventTrackingId: EventTrackingId);

        //            PushEVSEStatusTask.Wait();

        //        }

        //    }

        //    #endregion

        //    return;

        //}

        #endregion



        #region (timer) FlushEVSEDataAndStatus()

        protected override Boolean SkipFlushEVSEDataAndStatusQueues()
            => StationsToAddQueue.           Count == 0 &&
               StationsToUpdateQueue.        Count == 0 &&
               EVSEStatusChangesDelayedQueue.Count == 0 &&
               StationsToRemoveQueue.        Count == 0;

        protected override async Task FlushEVSEDataAndStatusQueues()
        {

            #region Get a copy of all current EVSE data and delayed status

            var StationsToAddQueueCopy                 = new HashSet<ChargingStation>();
            var StationsToUpdateQueueCopy              = new HashSet<ChargingStation>();
            var StationsStatusChangesDelayedQueueCopy  = new List<EVSEStatusUpdate>();
            var StationsToRemoveQueueCopy              = new HashSet<ChargingStation>();
            var StationsUpdateLogCopy                  = new Dictionary<Station,         PropertyUpdateInfos[]>();
            var ChargingStationsUpdateLogCopy          = new Dictionary<ChargingStation, PropertyUpdateInfos[]>();
            var ChargingPoolsUpdateLogCopy             = new Dictionary<ChargingPool,    PropertyUpdateInfos[]>();

            await DataAndStatusLock.WaitAsync();

            try
            {

                // Copy 'EVSEs to add', remove originals...
                StationsToAddQueueCopy                      = new HashSet<ChargingStation>      (StationsToAddQueue);
                StationsToAddQueue.Clear();

                // Copy 'EVSEs to update', remove originals...
                StationsToUpdateQueueCopy                   = new HashSet<ChargingStation>      (StationsToUpdateQueue);
                StationsToUpdateQueue.Clear();

                // Copy 'EVSE status changes', remove originals...
                StationsStatusChangesDelayedQueueCopy        = new List<EVSEStatusUpdate>       (EVSEStatusChangesDelayedQueue);
                //////StationsStatusChangesDelayedQueueCopy.AddRange(StationsToAddQueueCopy.SelectMany(stations => stations.Connectors).Select(connector => new EVSEStatusUpdate(stations, stations.Status, stations.Status)));
                EVSEStatusChangesDelayedQueue.Clear();

                // Copy 'EVSEs to remove', remove originals...
                StationsToRemoveQueueCopy                   = new HashSet<ChargingStation>      (StationsToRemoveQueue);
                StationsToRemoveQueue.Clear();

                // Copy EVSE property updates
                //////EVSEsUpdateLog.           ForEach(_ => StationsUpdateLogCopy.           Add(_.Key, _.Value.ToArray()));
                EVSEsUpdateLog.Clear();

                // Copy charging station property updates
                ChargingStationsUpdateLog.ForEach(_ => ChargingStationsUpdateLogCopy.Add(_.Key, _.Value.ToArray()));
                ChargingStationsUpdateLog.Clear();

                // Copy charging pool property updates
                ChargingPoolsUpdateLog.   ForEach(_ => ChargingPoolsUpdateLogCopy.   Add(_.Key, _.Value.ToArray()));
                ChargingPoolsUpdateLog.Clear();


                // Stop the timer. Will be rescheduled by next EVSE data/status change...
                FlushEVSEDataAndStatusTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

            }
            finally
            {
                DataAndStatusLock.Release();
            }

            #endregion

            // Use events to check if something went wrong!
            var EventTrackingId = EventTracking_Id.New;

            //Thread.Sleep(30000);

            #region Send new EVSE data

            if (StationsToAddQueueCopy.Count > 0)
            {

                var responses = await StationsPost(StationsToAddQueueCopy,
                                                   //Timestamp,
                                                   //CancellationToken,
                                                   EventTrackingId: EventTrackingId).
                                                   //RequestTimeout)
                                      ConfigureAwait(false);

                //var responses = await Task.WhenAll(StationsToAddQueueCopy.
                //                                       Select(station => CPORoaming.StationPost(station,
                //                                                                                CPOClient.StationPartnerIdSelector(station),

                //                                                                                //Timestamp,
                //                                                                                //CancellationToken,
                //                                                                                EventTrackingId: EventTrackingId)).
                //                                                                                //RequestTimeout)).
                //                                       ToArray()).
                //                                       ConfigureAwait(false);


                //foreach (var response in responses)
                //{

                    //response.Content.

                    //if (EVSEsToAddTask.Warnings.Any())
                    //{

                    //    SendOnWarnings(DateTime.UtcNow,
                    //                   nameof(WWCPCPOAdapter) + Id,
                    //                   "EVSEsToAddTask",
                    //                   EVSEsToAddTask.Warnings);

                    //}

                //}

            }

            #endregion

            #region Send changed EVSE data

            if (StationsToUpdateQueueCopy.Count > 0)
            {

                // Surpress EVSE data updates for all newly added EVSEs
                var EVSEsWithoutNewEVSEs = StationsToUpdateQueueCopy.
                                               Where(evse => !StationsToAddQueueCopy.Contains(evse)).
                                               ToArray();


                if (EVSEsWithoutNewEVSEs.Length > 0)
                {

                    var responses = await StationsPost(EVSEsWithoutNewEVSEs,
                                                       //Timestamp,
                                                       //CancellationToken,
                                                       EventTrackingId: EventTrackingId).
                                                       //RequestTimeout)
                                          ConfigureAwait(false);

                    //var responses = await Task.WhenAll(EVSEsWithoutNewEVSEs.
                    //                                   Select(station => CPORoaming.StationPost(station,
                    //                                                                            CPOClient.StationPartnerIdSelector(station),

                    //                                                                            //Timestamp,
                    //                                                                            //CancellationToken,
                    //                                                                            EventTrackingId: EventTrackingId)).
                    //                                   //RequestTimeout)).
                    //                                   ToArray()).
                    //                                   ConfigureAwait(false);


                    //foreach (var response in responses)
                    //{

                        //response.Content.

                        //if (EVSEsToAddTask.Warnings.Any())
                        //{

                        //    SendOnWarnings(DateTime.UtcNow,
                        //                   nameof(WWCPCPOAdapter) + Id,
                        //                   "EVSEsToAddTask",
                        //                   EVSEsToAddTask.Warnings);

                        //}

                    //}

                }

            }

            #endregion

            #region Send changed EVSE status

            if (!DisablePushStatus &&
                StationsStatusChangesDelayedQueueCopy.Count > 0)
            {

                var responses = await ConnectorsPostStatus(StationsStatusChangesDelayedQueueCopy,
                                                           //Timestamp,
                                                           //CancellationToken,
                                                           EventTrackingId: EventTrackingId).
                                                           //RequestTimeout)
                                      ConfigureAwait(false);

                //var responses = await Task.WhenAll(StationsStatusChangesDelayedQueueCopy.
                //                                       Select(status => CPORoaming.ConnectorPostStatus(status.EVSE.     Id.   ToOIOI(),
                //                                                                                       status.NewStatus.Value.ToOIOI(),

                //                                                                                       //Timestamp,
                //                                                                                       //CancellationToken,
                //                                                                                       EventTrackingId: EventTrackingId)).
                //                                                                                       //RequestTimeout)).
                //                                       ToArray()).
                //                                       ConfigureAwait(false);


                //var PushEVSEStatusTask = PushEVSEStatus(EVSEStatusChangesDelayedQueueCopy,
                //                                        _FlushEVSEDataRunId == 1
                //                                            ? ActionTypes.fullLoad
                //                                            : ActionTypes.update,
                //                                        EventTrackingId: EventTrackingId);

                //PushEVSEStatusTask.Wait();

                //if (PushEVSEStatusTask.Result.Warnings.Any())
                //{

                //    SendOnWarnings(DateTime.UtcNow,
                //                   nameof(WWCPCPOAdapter) + Id,
                //                   "PushEVSEStatusTask",
                //                   PushEVSEStatusTask.Result.Warnings);

                //}

            }

            #endregion

            #region Send removed charging stations

            //if (EVSEsToRemoveQueueCopy.Count > 0)
            //{

            //    var EVSEsToRemove = EVSEsToRemoveQueueCopy.ToArray();

            //    if (EVSEsToRemove.Length > 0)
            //    {

            //        var EVSEsToRemoveTask = PushEVSEData(EVSEsToRemove,
            //                                             ActionTypes.delete,
            //                                             EventTrackingId: EventTrackingId);

            //        EVSEsToRemoveTask.Wait();

            //        if (EVSEsToRemoveTask.Result.Warnings.Any())
            //        {

            //            SendOnWarnings(DateTime.UtcNow,
            //                           nameof(WWCPCPOAdapter) + Id,
            //                           "EVSEsToRemoveTask",
            //                           EVSEsToRemoveTask.Result.Warnings);

            //        }

            //    }

            //}

            #endregion

        }

        #endregion

        #region (timer) FlushEVSEFastStatus()

        protected override Boolean SkipFlushEVSEFastStatusQueues()
            => EVSEStatusChangesFastQueue.Count == 0;

        protected override async Task FlushEVSEFastStatusQueues()
        {

            #region Get a copy of all current EVSE data and delayed status

            var EVSEStatusFastQueueCopy = new List<EVSEStatusUpdate>();

            await DataAndStatusLock.WaitAsync();

            try
            {

                // Copy 'EVSE status changes', remove originals...
                EVSEStatusFastQueueCopy = new List<EVSEStatusUpdate>(EVSEStatusChangesFastQueue.Where(evsestatuschange => !StationsToAddQueue.Any(station => station.EVSEs.Any(evse => evse.Id == evsestatuschange.EVSE.Id))));

                // Add all evse status changes of EVSE *NOT YET UPLOADED* into the delayed queue...
                var EVSEStatusChangesDelayed = EVSEStatusChangesFastQueue.Where(evsestatuschange => StationsToAddQueue.Any(station => station.EVSEs.Any(evse => evse.Id == evsestatuschange.EVSE.Id))).ToArray();

                if (EVSEStatusChangesDelayed.Length > 0)
                    EVSEStatusChangesDelayedQueue.AddRange(EVSEStatusChangesDelayed);

                EVSEStatusChangesFastQueue.Clear();

                // Stop the timer. Will be rescheduled by next EVSE status change...
                FlushEVSEFastStatusTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

            }
            finally
            {
                DataAndStatusLock.Release();
            }

            #endregion

            // Use events to check if something went wrong!
            var EventTrackingId = EventTracking_Id.New;

            #region Send changed EVSE status

            if (EVSEStatusFastQueueCopy.Count > 0)
            {

                var _PushEVSEStatus = await ConnectorsPostStatus(EVSEStatusFastQueueCopy,
                                                                EventTrackingId: EventTrackingId);

                if (_PushEVSEStatus.Warnings.Any())
                {

                    SendOnWarnings(DateTime.UtcNow,
                                   nameof(WWCPCPOAdapter) + Id,
                                   "PushEVSEFastStatus",
                                   _PushEVSEStatus.Warnings);

                }

            }

            #endregion

        }

        #endregion


        #region (timer) FlushChargeDetailRecords()

        protected override Boolean SkipFlushChargeDetailRecordsQueues()
            => OICP_ChargeDetailRecords_Queue.Count == 0;

        protected override async Task FlushChargeDetailRecordsQueues()
        {

            #region Make a thread local copy of all data

            var LockTaken                    = await FlushOICPChargeDetailRecordsLock.WaitAsync(TimeSpan.FromSeconds(30));
            var ChargeDetailRecordQueueCopy  = new List<Session>();

            try
            {

                if (LockTaken)
                {

                    // Copy CDRs, empty original queue...
                    ChargeDetailRecordQueueCopy.AddRange(OICP_ChargeDetailRecords_Queue);
                    OICP_ChargeDetailRecords_Queue.Clear();

                    //// Stop the timer. Will be rescheduled by the next CDR...
                    //FlushChargeDetailRecordsTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

                }

            }
            catch (Exception e)
            {

                while (e.InnerException != null)
                    e = e.InnerException;

                DebugX.LogT(nameof(WWCPCPOAdapter) + " '" + Id + "' led to an exception: " + e.Message + Environment.NewLine + e.StackTrace);

            }

            finally
            {
                if (LockTaken)
                    FlushOICPChargeDetailRecordsLock.Release();
            }

            #endregion

            // Use the events to evaluate if something went wrong!

            #region Send charge detail records

            if (ChargeDetailRecordQueueCopy.Count > 0)
            {

                var EventTrackingId  = EventTracking_Id.New;
                var results          = new List<HTTPResponse<SessionPostResponse>>();

                foreach (var chargedetailrecord in ChargeDetailRecordQueueCopy)
                    results.Add(await CPORoaming.SessionPost(chargedetailrecord,
                                                             DateTime.UtcNow,
                                                             new CancellationTokenSource().Token,
                                                             EventTrackingId,
                                                             DefaultRequestTimeout));

                //var Warnings         = results.Where(result => result.Content

            }

            #endregion

            //ToDo: Send FlushChargeDetailRecordsQueues result event...
            //ToDo: Re-add to queue if it could not be send...

        }

        #endregion


        // -----------------------------------------------------------------------------------------------------


        #region Operator overloading

        #region Operator == (WWCPCPOAdapter1, WWCPCPOAdapter2)

        /// <summary>
        /// Compares two WWCPCPOAdapters for equality.
        /// </summary>
        /// <param name="WWCPCPOAdapter1">A WWCPCPOAdapter.</param>
        /// <param name="WWCPCPOAdapter2">Another WWCPCPOAdapter.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (WWCPCPOAdapter WWCPCPOAdapter1, WWCPCPOAdapter WWCPCPOAdapter2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(WWCPCPOAdapter1, WWCPCPOAdapter2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) WWCPCPOAdapter1 == null) || ((Object) WWCPCPOAdapter2 == null))
                return false;

            return WWCPCPOAdapter1.Equals(WWCPCPOAdapter2);

        }

        #endregion

        #region Operator != (WWCPCPOAdapter1, WWCPCPOAdapter2)

        /// <summary>
        /// Compares two WWCPCPOAdapters for inequality.
        /// </summary>
        /// <param name="WWCPCPOAdapter1">A WWCPCPOAdapter.</param>
        /// <param name="WWCPCPOAdapter2">Another WWCPCPOAdapter.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (WWCPCPOAdapter WWCPCPOAdapter1, WWCPCPOAdapter WWCPCPOAdapter2)
            => !(WWCPCPOAdapter1 == WWCPCPOAdapter2);

        #endregion

        #region Operator <  (WWCPCPOAdapter1, WWCPCPOAdapter2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="WWCPCPOAdapter1">A WWCPCPOAdapter.</param>
        /// <param name="WWCPCPOAdapter2">Another WWCPCPOAdapter.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (WWCPCPOAdapter  WWCPCPOAdapter1, WWCPCPOAdapter  WWCPCPOAdapter2)
        {

            if (WWCPCPOAdapter1 is null)
                throw new ArgumentNullException(nameof(WWCPCPOAdapter1),  "The given WWCPCPOAdapter must not be null!");

            return WWCPCPOAdapter1.CompareTo(WWCPCPOAdapter2) < 0;

        }

        #endregion

        #region Operator <= (WWCPCPOAdapter1, WWCPCPOAdapter2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="WWCPCPOAdapter1">A WWCPCPOAdapter.</param>
        /// <param name="WWCPCPOAdapter2">Another WWCPCPOAdapter.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (WWCPCPOAdapter WWCPCPOAdapter1, WWCPCPOAdapter WWCPCPOAdapter2)
            => !(WWCPCPOAdapter1 > WWCPCPOAdapter2);

        #endregion

        #region Operator >  (WWCPCPOAdapter1, WWCPCPOAdapter2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="WWCPCPOAdapter1">A WWCPCPOAdapter.</param>
        /// <param name="WWCPCPOAdapter2">Another WWCPCPOAdapter.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (WWCPCPOAdapter WWCPCPOAdapter1, WWCPCPOAdapter WWCPCPOAdapter2)
        {

            if (WWCPCPOAdapter1 is null)
                throw new ArgumentNullException(nameof(WWCPCPOAdapter1),  "The given WWCPCPOAdapter must not be null!");

            return WWCPCPOAdapter1.CompareTo(WWCPCPOAdapter2) > 0;

        }

        #endregion

        #region Operator >= (WWCPCPOAdapter1, WWCPCPOAdapter2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="WWCPCPOAdapter1">A WWCPCPOAdapter.</param>
        /// <param name="WWCPCPOAdapter2">Another WWCPCPOAdapter.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (WWCPCPOAdapter WWCPCPOAdapter1, WWCPCPOAdapter WWCPCPOAdapter2)
            => !(WWCPCPOAdapter1 < WWCPCPOAdapter2);

        #endregion

        #endregion

        #region IComparable<WWCPCPOAdapter> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            if (!(Object is WWCPCPOAdapter WWCPCPOAdapter))
                throw new ArgumentException("The given object is not an WWCPCPOAdapter!", nameof(Object));

            return CompareTo(WWCPCPOAdapter);

        }

        #endregion

        #region CompareTo(WWCPCPOAdapter)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="WWCPCPOAdapter">An WWCPCPOAdapter object to compare with.</param>
        public Int32 CompareTo(WWCPCPOAdapter WWCPCPOAdapter)
        {

            if (WWCPCPOAdapter is null)
                throw new ArgumentNullException(nameof(WWCPCPOAdapter), "The given WWCPCPOAdapter must not be null!");

            return Id.CompareTo(WWCPCPOAdapter.Id);

        }

        #endregion

        #endregion

        #region IEquatable<WWCPCPOAdapter> Members

        #region Equals(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        /// <returns>true|false</returns>
        public override Boolean Equals(Object Object)
        {

            if (Object == null)
                return false;

            if (!(Object is WWCPCPOAdapter WWCPCPOAdapter))
                return false;

            return Equals(WWCPCPOAdapter);

        }

        #endregion

        #region Equals(WWCPCPOAdapter)

        /// <summary>
        /// Compares two WWCPCPOAdapter for equality.
        /// </summary>
        /// <param name="WWCPCPOAdapter">An WWCPCPOAdapter to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(WWCPCPOAdapter WWCPCPOAdapter)
        {

            if (WWCPCPOAdapter is null)
                return false;

            return Id.Equals(WWCPCPOAdapter.Id);

        }

        #endregion

        #endregion

        #region GetHashCode()

        /// <summary>
        /// Get the hashcode of this object.
        /// </summary>
        public override Int32 GetHashCode()
            => Id.GetHashCode();

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()
            => "OIOI " + Version.Number + " CPO Adapter " + Id;

        #endregion


    }

}
