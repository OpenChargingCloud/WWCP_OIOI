/*
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
using Org.BouncyCastle.Crypto.Parameters;
using cloud.charging.open.protocols.WWCP;

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x.CPO
{

    /// <summary>
    /// A WWCP wrapper for the OIOI CPO Roaming client which maps
    /// WWCP data structures onto OIOI data structures and vice versa.
    /// </summary>
    public class WWCPCPOAdapter : AWWCPEMPAdapter<Session>,
                                  IEMPRoamingProvider,
                                  IEquatable <WWCPCPOAdapter>,
                                  IComparable<WWCPCPOAdapter>,
                                  IComparable
    {

        #region Data

        private        readonly  CustomOperatorIdMapperDelegate                   CustomOperatorIdMapper;
        private        readonly  CustomEVSEIdMapperDelegate                       CustomEVSEIdMapper;
        private        readonly  CustomConnectorIdMapperDelegate                  CustomConnectorIdMapper;
        private        readonly  ChargingStation2StationDelegate                  ChargingStation2Station;
        private        readonly  ChargeDetailRecord2SessionDelegate               CustomChargeDetailRecord2Session;
        private        readonly  EVSEStatusUpdate2ConnectorStatusUpdateDelegate   CustomEVSEStatusUpdate2ConnectorStatusUpdateDelegate;

        private        readonly  Station2JSONDelegate                             _Station2JSON;

        private        readonly  ConnectorStatus2JSONDelegate                     _ConnectorStatus2JSON;

        private        readonly  Session2JSONDelegate                             _Session2JSON;

        private static readonly  Regex                                            pattern                      = new (@"\s=\s");


        private readonly        HashSet<IChargingStation>                         StationsToAddQueue;
        private readonly        HashSet<IChargingStation>                         StationsToUpdateQueue;
        private readonly        HashSet<IChargingStation>                         StationsToRemoveQueue;
        private readonly        List<EVSEStatusUpdate>                            EVSEStatusUpdatesQueue;
        private readonly        List<EVSEStatusUpdate>                            EVSEStatusUpdatesDelayedQueue;

        public readonly static  TimeSpan                                          DefaultRequestTimeout  = TimeSpan.FromSeconds(30);
        public readonly static  WWCP.EMobilityProvider_Id                         DefaultProviderId      = WWCP.EMobilityProvider_Id.Parse("DE*8PS");


        //private readonly List<Session> OICP_ChargeDetailRecords_Queue;
        //protected readonly SemaphoreSlim FlushOICPChargeDetailRecordsLock = new SemaphoreSlim(1, 1);

        #endregion

        #region Properties

        IId IAuthorizeStartStop.AuthId
            => Id;

        IId ISendChargeDetailRecords.SendChargeDetailRecordsId
            => Id;

        /// <summary>
        /// The wrapped CPO roaming object.
        /// </summary>
        public CPORoaming  CPORoaming    { get; }

        public CPOClient CPOClient
            => CPORoaming?.CPOClient;

        public CPOServer CPOServer
            => CPORoaming?.CPOServer;



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

        public Func<ChargeDetailRecord, ChargeDetailRecordFilters>  ChargeDetailRecordFilter    { get; set; }

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
        public event OnAuthorizeStartRequestDelegate   OnAuthorizeStartRequest;

        /// <summary>
        /// An event fired whenever an authentication token had been verified for charging.
        /// </summary>
        public event OnAuthorizeStartResponseDelegate  OnAuthorizeStartResponse;

        #endregion

        #region OnAuthorizeStopRequest/-Response

        /// <summary>
        /// An event fired whenever an authentication token will be verified to stop a charging process.
        /// </summary>
        public event OnAuthorizeStopRequestDelegate   OnAuthorizeStopRequest;

        /// <summary>
        /// An event fired whenever an authentication token had been verified to stop a charging process.
        /// </summary>
        public event OnAuthorizeStopResponseDelegate  OnAuthorizeStopResponse;

        #endregion

        #region OnSendCDRRequest/-Response

        /// <summary>
        /// An event fired whenever a charge detail record was enqueued for later sending upstream.
        /// </summary>
        public event OnSendCDRsRequestDelegate   OnEnqueueSendCDRsRequest;

        /// <summary>
        /// An event fired whenever a charge detail record will be send upstream.
        /// </summary>
        public event OnSendCDRsRequestDelegate   OnSendCDRsRequest;

        /// <summary>
        /// An event fired whenever a charge detail record had been sent upstream.
        /// </summary>
        public event OnSendCDRsResponseDelegate  OnSendCDRsResponse;

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

        #endregion

        #region Constructor(s)

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
        public WWCPCPOAdapter(EMPRoamingProvider_Id                            Id,
                              I18NString                                       Name,
                              I18NString                                       Description,
                              RoamingNetwork                                   RoamingNetwork,
                              CPORoaming                                       CPORoaming,

                              ChargingStation2StationDelegate?                 ChargingStation2Station                  = null,
                              EVSEStatusUpdate2ConnectorStatusUpdateDelegate?  EVSEStatusUpdate2ConnectorStatusUpdate   = null,
                              ChargeDetailRecord2SessionDelegate?              ChargeDetailRecord2Session               = null,
                              Station2JSONDelegate?                            Station2JSON                             = null,
                              ConnectorStatus2JSONDelegate?                    ConnectorStatus2JSON                     = null,
                              Session2JSONDelegate?                            Session2JSON                             = null,

                              IncludeEVSEIdDelegate?                           IncludeEVSEIds                           = null,
                              IncludeEVSEDelegate?                             IncludeEVSEs                             = null,
                              IncludeChargingStationIdDelegate?                IncludeChargingStationIds                = null,
                              IncludeChargingStationDelegate?                  IncludeChargingStations                  = null,
                              ChargeDetailRecordFilterDelegate?                ChargeDetailRecordFilter                 = null,
                              CustomOperatorIdMapperDelegate?                  CustomOperatorIdMapper                   = null,
                              CustomEVSEIdMapperDelegate?                      CustomEVSEIdMapper                       = null,
                              CustomConnectorIdMapperDelegate?                 CustomConnectorIdMapper                  = null,

                              TimeSpan?                                        ServiceCheckEvery                        = null,
                              TimeSpan?                                        StatusCheckEvery                         = null,
                              TimeSpan?                                        CDRCheckEvery                            = null,

                              Boolean                                          DisablePushData                          = false,
                              Boolean                                          DisablePushAdminStatus                   = true,
                              Boolean                                          DisablePushStatus                        = false,
                              Boolean                                          DisableAuthentication                    = false,
                              Boolean                                          DisableSendChargeDetailRecords           = false,

                              String                                           EllipticCurve                            = "P-256",
                              ECPrivateKeyParameters?                          PrivateKey                               = null,
                              PublicKeyCertificates?                           PublicKeyCertificates                    = null)

            : base(Id,
                   RoamingNetwork,
                   Name,
                   Description,

                   IncludeEVSEIds,
                   IncludeEVSEs,
                   IncludeChargingStationIds,
                   IncludeChargingStations,
                   null,
                   null,
                   null,
                   null,
                   ChargeDetailRecordFilter,

                   ServiceCheckEvery,
                   StatusCheckEvery,
                   null,
                   CDRCheckEvery,

                   DisablePushData,
                   DisablePushAdminStatus,
                   DisablePushStatus,
                   true,
                   true,
                   DisableAuthentication,
                   DisableSendChargeDetailRecords,

                   EllipticCurve,
                   PrivateKey,
                   PublicKeyCertificates)

        {

            this.CPORoaming                                            = CPORoaming ?? throw new ArgumentNullException(nameof(CPORoaming), "The given OIOI CPO Roaming object must not be null!");
            this.CustomOperatorIdMapper                                = CustomOperatorIdMapper;
            this.CustomEVSEIdMapper                                    = CustomEVSEIdMapper;
            this.ChargingStation2Station                               = ChargingStation2Station;
            this.CustomEVSEStatusUpdate2ConnectorStatusUpdateDelegate  = EVSEStatusUpdate2ConnectorStatusUpdate;
            this.CustomChargeDetailRecord2Session                      = ChargeDetailRecord2Session;
            this._Station2JSON                                         = Station2JSON;
            this._ConnectorStatus2JSON                                 = ConnectorStatus2JSON;
            this._Session2JSON                                         = Session2JSON;

            this.DisablePushData                                       = DisablePushData;
            this.DisablePushStatus                                     = DisablePushStatus;
            this.DisableAuthentication                                 = DisableAuthentication;
            this.DisableSendChargeDetailRecords                        = DisableSendChargeDetailRecords;

            this.StationsToAddQueue                                    = new HashSet<IChargingStation>();
            this.StationsToUpdateQueue                                 = new HashSet<IChargingStation>();
            this.StationsToRemoveQueue                                 = new HashSet<IChargingStation>();
            this.EVSEStatusUpdatesQueue                                = new List<EVSEStatusUpdate>();
            this.EVSEStatusUpdatesDelayedQueue                         = new List<EVSEStatusUpdate>();

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

                RemoteStartResult response = null;

                var EVSEId   = ConnectorId.ToWWCP(CustomConnectorIdMapper);

                if (!EVSEId.HasValue)
                    response = RemoteStartResult.UnknownLocation(TimeSpan.Zero);

                else
                    response = await RoamingNetwork.
                                         RemoteStart(EMPRoamingProvider:    this,
                                                     ChargingLocation:      ChargingLocation.FromEVSEId(EVSEId),
                                                     RemoteAuthentication:  RemoteAuthentication.FromRemoteIdentification(WWCP.EMobilityAccount_Id.Parse(eMAId.ToString())),
                                                     SessionId:             ChargingSession_Id.NewRandom,
                                                     ProviderId:            WWCP.EMobilityProvider_Id.Parse(eMAId.ProviderId.ToString()),

                                                     Timestamp:             Timestamp,
                                                     CancellationToken:     CancellationToken,
                                                     EventTrackingId:       EventTrackingId,
                                                     RequestTimeout:        RequestTimeout).
                                          ConfigureAwait(false);


                Result SessionStartResult = null;

                switch (response.Result)
                {

                    #region Success

                    case RemoteStartResultTypes.Success:

                        SessionStartResult  = Result.ChargingSuccess("Successfully started a charging session. The customer is charging at the EVSE!",
                                                                     Session_Id.Parse(response.Session.Id.ToString()),
                                                                     true);

                        break;

                    #endregion

                    #region AsyncOperation

                    case RemoteStartResultTypes.AsyncOperation:

                        SessionStartResult  = Result.ChargingSuccess("Successfully started an async charging session!",
                                                                     Session_Id.Parse(response.Session.Id.ToString()),
                                                                     true);

                        break;

                    #endregion

                    #region SuccessPlugInCableToStartCharging

                    case RemoteStartResultTypes.SuccessPlugInCableToStartCharging:

                        SessionStartResult  = Result.SuccessPleasePlugIn("Successfully authorized a charging session. The customer must now plug in the cable to start!",
                                                                         Session_Id.Parse(response.Session.Id.ToString()),
                                                                         true);

                        break;

                    #endregion

                    #region UnknownLocation

                    case RemoteStartResultTypes.UnknownLocation:

                        SessionStartResult  = Result.Error(181, "EVSE not found!");

                        break;

                    #endregion

                    #region UnknownOperator

                    case RemoteStartResultTypes.UnknownOperator:

                        SessionStartResult  = Result.Error(300, "Unknown charging station operator!");

                        break;

                    #endregion

                    #region InvalidCredentials

                    case RemoteStartResultTypes.InvalidCredentials:

                        SessionStartResult  = Result.Error(145, "Authentication failed: User token not valid!");

                        break;

                    #endregion

                    #region Timeout

                    case RemoteStartResultTypes.Timeout:

                        SessionStartResult  = Result.Error(312, "EVSE timeout!");

                        break;

                    #endregion

                    #region AlreadyInUse

                    case RemoteStartResultTypes.AlreadyInUse:

                        SessionStartResult  = Result.Error(320, "EVSE already in use!");

                        break;

                    #endregion

                    #region NoEVConnectedToEVSE

                    case RemoteStartResultTypes.NoEVConnectedToEVSE:

                        SessionStartResult  = Result.Error(321, "No EV connected to EVSE!");

                        break;

                    #endregion

                    #region Reserved

                    case RemoteStartResultTypes.Reserved:

                        SessionStartResult  = Result.Error(323, "EVSE already reserved!");

                        break;

                    #endregion

                    default:

                        SessionStartResult  = Result.Error(310, "EVSE error!");

                        break;

                }

                return SessionStartResult;

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

                var response = await RoamingNetwork.RemoteStop(EMPRoamingProvider:    this,
                                                               SessionId:             SessionId. ToWWCP(),
                                                               //EVSEId:                ConnectorId.ToWWCP(),
                                                               RemoteAuthentication:  RemoteAuthentication.FromRemoteIdentification(WWCP.EMobilityAccount_Id.Parse(eMAId.ToString())),
                                                               ReservationHandling:   ReservationHandling.Close,
                                                               ProviderId:            WWCP.EMobilityProvider_Id.Parse(eMAId.ProviderId.ToString()),

                                                               Timestamp:             Timestamp,
                                                               CancellationToken:     CancellationToken,
                                                               EventTrackingId:       EventTrackingId,
                                                               RequestTimeout:        RequestTimeout).
                                                    ConfigureAwait(false);


                Result SessionStopResult = null;

                switch (response.Result)
                {

                    #region Documentation

                    // HTTP Status codes
                    //  200 OK             Request was processed successfully
                    //  401 Unauthorized   The token, username or identifier type were incorrect
                    //  404 Not found      A connector could not be found by the supplied identifier

                    // Result codes
                    //    0                Success
                    //  140                Authentication failed: No positive authentication response
                    //  144                Authentication failed: Email does not exist
                    //  145                Authentication failed: User token not valid
                    //  181                EVSE not found

                    #endregion

                    #region Success

                    case RemoteStopResultTypes.Success:

                        SessionStopResult  = Result.Success("Success!");

                        break;

                    #endregion

                    #region UnknownEVSE

                    case RemoteStopResultTypes.UnknownLocation:

                        SessionStopResult  = Result.Error(181, "EVSE not found!");

                        break;

                    #endregion

                    //#region UnknownOperator

                    //case RemoteStartResultType.UnknownOperator:

                    //    SessionStartResult  = Result.Error(300, "Unknown charging station operator!");

                    //    _HTTPResponse       = CreateResponse(Request,
                    //                                         HTTPStatusCode.OK,
                    //                                         SessionStartResult);

                    //    break;

                    //#endregion

                    //#region Timeout

                    //case RemoteStartResultType.Timeout:

                    //    SessionStartResult  = Result.Error(312, "EVSE timeout!");

                    //    _HTTPResponse       = CreateResponse(Request,
                    //                                         HTTPStatusCode.OK,
                    //                                         SessionStartResult);

                    //    break;

                    //#endregion

                    default:

                        SessionStopResult  = Result.Error(310, "EVSE error!");

                        break;

                }

                return SessionStopResult;

            };

            #endregion

        }

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

            StationsPost(IEnumerable<IChargingStation>  ChargingStations,

                         DateTime?                      Timestamp           = null,
                         CancellationToken              CancellationToken   = default,
                         EventTracking_Id?              EventTrackingId     = null,
                         TimeSpan?                      RequestTimeout      = null)

        {

            #region Initial checks

            if (ChargingStations == null)
                throw new ArgumentNullException(nameof(ChargingStations), "The given enumeration of charging stations must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPORoaming.CPOClient.RequestTimeout;

            #endregion

            #region Get effective number of stations to upload

            var warnings      = new List<Warning>();

            var wwcpStations  = new Dictionary<Station_Id, IChargingStation>();

            var stations      = ChargingStations.Where (station => station != null && IncludeChargingStations(station)).
                                                 Select(station => {

                                                     try
                                                     {

                                                         wwcpStations.Add(station.Id.ToOIOI(), station);

                                                         return new Tuple<IChargingStation, Station>(station,
                                                                                                     station.ToOIOI(CustomOperatorIdMapper,
                                                                                                                    CustomEVSEIdMapper,
                                                                                                                    ChargingStation2Station));

                                                     }
                                                     catch (Exception e)
                                                     {
                                                         DebugX.  Log(e.Message);
                                                         warnings.Add(Warning.Create(I18NString.Create(Languages.en, e.Message), station));
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
                                                 stations.ULongCount(),
                                                 stations,
                                                 warnings.Where(warning => warning.IsNeitherNullNorEmpty()),
                                                 RequestTimeout);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnStationPostWWCPRequest));
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
                                                                   ChargingStations,
                                                                   Warnings: warnings);

            }

            else if (stations.Length == 0)
            {

                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                result   = PushChargingStationDataResult.NoOperation(Id,
                                                                     this,
                                                                     ChargingStations,
                                                                     Warnings: warnings);

            }

            else
            {

                var semaphore  = new SemaphoreSlim(_maxDegreeOfParallelism,
                                                   _maxDegreeOfParallelism);

                var tasks      = stations.Select(async station => {

                    await semaphore.WaitAsync().ConfigureAwait(false);

                    try
                    {

                        return await CPORoaming.StationPost(station.Item2,
                                                            CPORoaming.CPOClient.StationPartnerIdSelector(station.Item2),

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

                await Task.WhenAll(tasks);


                var results = tasks.Select((task) => {

                                        if (task.Result.HTTPStatusCode == HTTPStatusCode.OK &&
                                            task.Result.Content        != null)
                                        {

                                            if (task.Result.Content.Code == ResponseCodes.Success)
                                                return new PushSingleChargingStationDataResult(wwcpStations[task.Result.Content.Request.Station.Id],
                                                                                               PushSingleDataResultTypes.Success,
                                                                                               new Warning[] { Warning.Create(I18NString.Create(Languages.en, task.Result.Content.Message)) });

                                            else
                                                return new PushSingleChargingStationDataResult(wwcpStations[task.Result.Content.Request.Station.Id],
                                                                                               PushSingleDataResultTypes.Error,
                                                                                               new Warning[] { Warning.Create(I18NString.Create(Languages.en, task.Result.Content.Message)) });

                                        }
                                        else
                                            return new PushSingleChargingStationDataResult(wwcpStations[task.Result.Content.Request.Station.Id],
                                                                                           PushSingleDataResultTypes.Error,
                                                                                           new Warning[] {
                                                                                               Warning.Create(I18NString.Create(Languages.en, task.Result.HTTPStatusCode.ToString()))
                                                                                           }.Concat(
                                                                                               task.Result.HTTPBody != null
                                                                                                   ? warnings.AddAndReturnList(I18NString.Create(Languages.en, task.Result.HTTPBody.ToUTF8String()))
                                                                                                   : warnings.AddAndReturnList(I18NString.Create(Languages.en, "No HTTP body received!"))
                                                                                           ));

                });

                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                result   = results.All(_ => _.Result == PushSingleDataResultTypes.Success)

                               ? PushChargingStationDataResult.Success(Id,
                                                                       this,
                                                                       ChargingStations,
                                                                       null,
                                                                       warnings,
                                                                       Runtime)

                               : PushChargingStationDataResult.Error  (Id,
                                                                       this,
                                                                       results.Where(_ => _.Result != PushSingleDataResultTypes.Success),
                                                                       null,
                                                                       warnings,
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
                                                  stations.ULongCount(),
                                                  stations,
                                                  RequestTimeout,
                                                  result,
                                                  Runtime);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnStationPostWWCPResponse));
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
                                 CancellationToken              CancellationToken   = default,
                                 EventTracking_Id?              EventTrackingId     = null,
                                 TimeSpan?                      RequestTimeout      = null)

        {

            #region Initial checks

            if (EVSEStatusUpdates == null)
                throw new ArgumentNullException(nameof(EVSEStatusUpdates), "The given enumeration of EVSE status updates must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPORoaming.CPOClient.RequestTimeout;

            #endregion

            #region Get effective number of connector status to upload

            var Warnings = new List<Warning>();

            var _ConnectorStatus = EVSEStatusUpdates.
                                       //Where       (evsestatusupdate => IncludeChargingStations(evsestatusupdate.EVSE.ChargingStation)).
                                       ToLookup    (evsestatusupdate => evsestatusupdate.Id,
                                                    evsestatusupdate => evsestatusupdate).
                                       ToDictionary(group            => group.Key,
                                                    group            => group.AsEnumerable().OrderByDescending(item => item.NewStatus.Timestamp)).
                                       Select      (evsestatusupdate => {

                                           try
                                           {

                                               var connectorId = evsestatusupdate.Key.ToOIOI(CustomEVSEIdMapper);

                                               if (!connectorId.HasValue)
                                                   return null;

                                               // Only push the current status of the latest status update!
                                               return new Tuple<EVSEStatusUpdate, ConnectorStatus>(
                                                          evsestatusupdate.Value.First(),
                                                          new ConnectorStatus(
                                                              connectorId.Value,
                                                              evsestatusupdate.Value.First().NewStatus.Value.ToOIOI()
                                                          )
                                                      );

                                           }
                                           catch (Exception e)
                                           {
                                               DebugX.  Log(e.Message);
                                               Warnings.Add(Warning.Create(I18NString.Create(Languages.en, e.Message), evsestatusupdate));
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
                                                    Warnings.Where(warning => warning.IsNeitherNullNorEmpty()),
                                                    RequestTimeout);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnConnectorStatusPostRequest));
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

                var semaphore  = new SemaphoreSlim(_maxDegreeOfParallelism,
                                                   _maxDegreeOfParallelism);

                var tasks      = _ConnectorStatus.Select(async connectorStatus => {

                    await semaphore.WaitAsync().ConfigureAwait(false);

                    try
                    {

                        return await CPORoaming.ConnectorPostStatus(connectorStatus.Item2,
                                                                    CPORoaming.CPOClient.ConnectorIdPartnerIdSelector(connectorStatus.Item2.Id),

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
                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnConnectorStatusPostResponse));
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

            ISendPOIData.SetStaticData(IEVSE               EVSE,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken   CancellationToken,
                                       EventTracking_Id?   EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (IncludeChargingStations == null ||
                       (IncludeChargingStations != null && IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToAddQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this, null);

            }

            #endregion


            return (await StationsPost(new[] { EVSE.ChargingStation },

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

            ISendPOIData.AddStaticData(IEVSE               EVSE,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken   CancellationToken,
                                       EventTracking_Id?   EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (IncludeChargingStations == null ||
                       (IncludeChargingStations != null && IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToAddQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this, null);

            }

            #endregion


            return (await StationsPost(new[] { EVSE.ChargingStation },

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

            ISendPOIData.UpdateStaticData(IEVSE               EVSE,
                                          String              PropertyName,
                                          Object?             NewValue,
                                          Object?             OldValue,
                                          Context?            DataSource,
                                          TransmissionTypes   TransmissionType,

                                          DateTime?           Timestamp,
                                          CancellationToken   CancellationToken,
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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (IncludeChargingStations == null ||
                       (IncludeChargingStations != null && IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToUpdateQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this, null);

            }

            #endregion


            return (await StationsPost(new[] { EVSE.ChargingStation },

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

            ISendPOIData.DeleteStaticData(IEVSE               EVSE,
                                          TransmissionTypes   TransmissionType,

                                          DateTime?           Timestamp,
                                          CancellationToken   CancellationToken,
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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (IncludeChargingStations == null ||
                       (IncludeChargingStations != null && IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToUpdateQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushEVSEDataResult.Enqueued(Id, this, null);

            }

            #endregion


            return (await StationsPost(new[] { EVSE.ChargingStation },

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

            ISendPOIData.SetStaticData(IEnumerable<IEVSE>  EVSEs,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken   CancellationToken,
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

            ISendPOIData.AddStaticData(IEnumerable<IEVSE>  EVSEs,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken   CancellationToken,
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

            ISendPOIData.UpdateStaticData(IEnumerable<IEVSE>  EVSEs,
                                          TransmissionTypes   TransmissionType,

                                          DateTime?           Timestamp,
                                          CancellationToken   CancellationToken,
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

            ISendPOIData.DeleteStaticData(IEnumerable<IEVSE>  EVSEs,
                                          TransmissionTypes   TransmissionType,

                                          DateTime?           Timestamp,
                                          CancellationToken   CancellationToken,
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
                                               CancellationToken                   CancellationToken,
                                               EventTracking_Id?                   EventTrackingId,
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
                                     CancellationToken              CancellationToken,
                                     EventTracking_Id               EventTrackingId,
                                     TimeSpan?                      RequestTimeout)

        {

            #region Initial checks

            if (StatusUpdates == null || !StatusUpdates.Any())
                return WWCP.PushEVSEStatusResult.NoOperation(Id, this);

            WWCP.PushEVSEStatusResult result = null;

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == WWCP.TransmissionTypes.Enqueue)
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
                //    DebugX.LogException(e, nameof(WWCPEMPAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                var invokeTimer  = false;
                var LockTaken    = await DataAndStatusLock.WaitAsync(MaxLockWaitingTime);

                try
                {

                    if (LockTaken)
                    {

                        var FilteredUpdates = StatusUpdates.Where(statusupdate => IncludeEVSEIds(statusupdate.Id)).
                                                            ToArray();

                        if (FilteredUpdates.Length > 0)
                        {

                            foreach (var Update in FilteredUpdates)
                            {

                                // Delay the status update until the EVSE data had been uploaded!
                                if (evsesToAddQueue.Any(evse => evse.Id == Update.Id))
                                    evseStatusChangesDelayedQueue.Add(Update);

                                else
                                    evseStatusChangesFastQueue.Add(Update);

                            }

                            invokeTimer = true;

                            result = WWCP.PushEVSEStatusResult.Enqueued(Id, this);

                        }

                        result = WWCP.PushEVSEStatusResult.NoOperation(Id, this);

                    }

                }
                finally
                {
                    if (LockTaken)
                        DataAndStatusLock.Release();
                }

                if (!LockTaken)
                    return WWCP.PushEVSEStatusResult.Error(Id, this, Description: "Could not acquire DataAndStatusLock!");

                if (invokeTimer)
                    FlushEVSEFastStatusTimer.Change(FlushEVSEFastStatusEvery, TimeSpan.FromMilliseconds(-1));

                return result;

            }

            #endregion

            return await ConnectorsPostStatus(StatusUpdates,

                                              Timestamp,
                                              CancellationToken,
                                              EventTrackingId,
                                              RequestTimeout);

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
        async Task<PushChargingStationDataResult>

            ISendPOIData.SetStaticData(IChargingStation    ChargingStation,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken   CancellationToken,
                                       EventTracking_Id?   EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (IncludeChargingStations is null ||
                       (IncludeChargingStations is not null && IncludeChargingStations(ChargingStation)))
                    {

                        StationsToAddQueue.Add(ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                           TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushChargingStationDataResult.Enqueued(
                           Id,
                           this,
                           new IChargingStation [] {
                               ChargingStation
                           }
                       );

            }

            #endregion

            var result = await StationsPost(new IChargingStation[] { ChargingStation },

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingStationDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingStationDataResult>

            ISendPOIData.AddStaticData(IChargingStation    ChargingStation,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken   CancellationToken,
                                       EventTracking_Id?   EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (IncludeChargingStations is null ||
                       (IncludeChargingStations is not null && IncludeChargingStations(ChargingStation)))
                    {

                        StationsToAddQueue.Add(ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushChargingStationDataResult.Enqueued(
                           Id,
                           this,
                           new IChargingStation[] {
                               ChargingStation
                           }
                       );

            }

            #endregion

            var result = await StationsPost(new IChargingStation[] { ChargingStation },

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingStationDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region UpdateStaticData(ChargingStation, PropertyName = null, OldValue = null, NewValue = null, TransmissionType = Enqueue, ...)

        ///// <summary>
        ///// Update the EVSE data of the given charging station within the static EVSE data at the OIOI server.
        ///// </summary>
        ///// <param name="ChargingStation">A charging station.</param>
        ///// <param name="PropertyName">The name of the charging station property to update.</param>
        ///// <param name="OldValue">The old value of the charging station property to update.</param>
        ///// <param name="NewValue">The new value of the charging station property to update.</param>
        ///// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        ///// 
        ///// <param name="Timestamp">The optional timestamp of the request.</param>
        ///// <param name="CancellationToken">An optional token to cancel this request.</param>
        ///// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        ///// <param name="RequestTimeout">An optional timeout for this request.</param>
        //async Task<PushChargingStationDataResult>

        //    ISendPOIData.UpdateStaticData(IChargingStation    ChargingStation,
        //                                  String?             PropertyName,
        //                                  Object?             OldValue,
        //                                  Object?             NewValue,
        //                                  TransmissionTypes   TransmissionType,

        //                                  DateTime?           Timestamp,
        //                                  CancellationToken   CancellationToken,
        //                                  EventTracking_Id?   EventTrackingId,
        //                                  TimeSpan?           RequestTimeout)

        //{

        //    #region Enqueue, if requested...

        //    if (TransmissionType == TransmissionTypes.Enqueue)
        //    {

        //        #region Send OnEnqueueSendCDRRequest event

        //        //try
        //        //{

        //        //    OnEnqueueSendCDRRequest?.Invoke(DateTime.UtcNow,
        //        //                                    Timestamp.Value,
        //        //                                    this,
        //        //                                    EventTrackingId,
        //        //                                    RoamingNetwork.Id,
        //        //                                    ChargeDetailRecord,
        //        //                                    RequestTimeout);

        //        //}
        //        //catch (Exception e)
        //        //{
        //        //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
        //        //}

        //        #endregion

        //        await DataAndStatusLock.WaitAsync();

        //        try
        //        {

        //            if (IncludeChargingStations == null ||
        //               (IncludeChargingStations != null && IncludeChargingStations(ChargingStation)))
        //            {

        //                StationsToUpdateQueue.Add(ChargingStation);

        //                FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

        //            }

        //        }
        //        finally
        //        {
        //            DataAndStatusLock.Release();
        //        }

        //        return PushChargingStationDataResult.Enqueued(
        //                   Id,
        //                   this,
        //                   new IChargingStation[] {
        //                       ChargingStation
        //                   }
        //               );

        //    }

        //    #endregion

        //    var result = await StationsPost(new IChargingStation[] { ChargingStation },

        //                                    Timestamp,
        //                                    CancellationToken,
        //                                    EventTrackingId,
        //                                    RequestTimeout);

        //    return new PushChargingStationDataResult(
        //               result.Id,
        //               this,
        //               result.Result,
        //               Array.Empty<PushSingleChargingStationDataResult>(),
        //               Array.Empty<PushSingleChargingStationDataResult>(),
        //               result.Description,
        //               result.Warnings,
        //               result.Runtime
        //           );

        //}

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
        async Task<PushChargingStationDataResult>

            ISendPOIData.DeleteStaticData(IChargingStation    ChargingStation,
                                          TransmissionTypes   TransmissionType,

                                          DateTime?           Timestamp,
                                          CancellationToken   CancellationToken,
                                          EventTracking_Id?   EventTrackingId,
                                          TimeSpan?           RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    if (IncludeChargingStations == null ||
                       (IncludeChargingStations != null && IncludeChargingStations(ChargingStation)))
                    {

                        StationsToUpdateQueue.Add(ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery, TimeSpan.FromMilliseconds(-1));

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushChargingStationDataResult.Enqueued(
                           Id,
                           this,
                           new IChargingStation[] {
                               ChargingStation
                           }
                       );

            }

            #endregion

            var result = await StationsPost(new IChargingStation[] { ChargingStation },

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingStationDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingStationDataResult>

            ISendPOIData.SetStaticData(IEnumerable<IChargingStation>  ChargingStations,
                                       TransmissionTypes              TransmissionType,

                                       DateTime?                      Timestamp,
                                       CancellationToken              CancellationToken,
                                       EventTracking_Id?              EventTrackingId,
                                       TimeSpan?                      RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    foreach (var chargingStation in ChargingStations)
                    {

                        if (IncludeChargingStations is null ||
                           (IncludeChargingStations is not null && IncludeChargingStations(chargingStation)))
                        {

                            StationsToUpdateQueue.Add(chargingStation);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                               TimeSpan.FromMilliseconds(-1));

                        }

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushChargingStationDataResult.Enqueued(
                           Id,
                           this,
                           ChargingStations
                       );

            }

            #endregion

            var result = await StationsPost(ChargingStations,

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingStationDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingStationDataResult>

            ISendPOIData.AddStaticData(IEnumerable<IChargingStation>  ChargingStations,
                                       TransmissionTypes              TransmissionType,


                                       DateTime?                      Timestamp,
                                       CancellationToken              CancellationToken,
                                       EventTracking_Id?              EventTrackingId,
                                       TimeSpan?                      RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    foreach (var chargingStation in ChargingStations)
                    {

                        if (IncludeChargingStations is null ||
                           (IncludeChargingStations is not null && IncludeChargingStations(chargingStation)))
                        {

                            StationsToUpdateQueue.Add(chargingStation);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                               TimeSpan.FromMilliseconds(-1));

                        }

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushChargingStationDataResult.Enqueued(
                           Id,
                           this,
                           ChargingStations
                       );

            }

            #endregion

            var result = await StationsPost(ChargingStations,

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingStationDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingStationDataResult>

            ISendPOIData.UpdateStaticData(IEnumerable<IChargingStation>  ChargingStations,
                                          TransmissionTypes              TransmissionType,

                                          DateTime?                      Timestamp,
                                          CancellationToken              CancellationToken,
                                          EventTracking_Id?              EventTrackingId,
                                          TimeSpan?                      RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    foreach (var chargingStation in ChargingStations)
                    {

                        if (IncludeChargingStations is null ||
                           (IncludeChargingStations is not null && IncludeChargingStations(chargingStation)))
                        {

                            StationsToUpdateQueue.Add(chargingStation);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                               TimeSpan.FromMilliseconds(-1));

                        }

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushChargingStationDataResult.Enqueued(
                           Id,
                           this,
                           ChargingStations
                       );

            }

            #endregion

            var result = await StationsPost(ChargingStations,

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingStationDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingStationDataResult>

            ISendPOIData.DeleteStaticData(IEnumerable<IChargingStation>  ChargingStations,
                                          TransmissionTypes              TransmissionType,

                                          DateTime?                      Timestamp,
                                          CancellationToken              CancellationToken,
                                          EventTracking_Id?              EventTrackingId,
                                          TimeSpan?                      RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    foreach (var chargingStation in ChargingStations)
                    {

                        if (IncludeChargingStations is null ||
                           (IncludeChargingStations is not null && IncludeChargingStations(chargingStation)))
                        {

                            StationsToUpdateQueue.Add(chargingStation);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                               TimeSpan.FromMilliseconds(-1));

                        }

                    }

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

                return PushChargingStationDataResult.Enqueued(
                           Id,
                           this,
                           ChargingStations
                       );

            }

            #endregion

            var result = await StationsPost(ChargingStations,

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingStationDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       Array.Empty<PushSingleChargingStationDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
                                               CancellationToken                              CancellationToken,
                                               EventTracking_Id?                              EventTrackingId,
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
                                     CancellationToken                         CancellationToken,
                                     EventTracking_Id?                         EventTrackingId,
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
        async Task<PushChargingPoolDataResult>

            ISendPOIData.SetStaticData(IChargingPool       ChargingPool,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken   CancellationToken,
                                       EventTracking_Id?   EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    foreach (var station in ChargingPool)
                    {

                        if (IncludeChargingStations == null ||
                           (IncludeChargingStations != null && IncludeChargingStations(station)))
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

                return PushChargingPoolDataResult.Enqueued(Id, this, new IChargingPool[] { ChargingPool });

            }

            #endregion


            var result = await StationsPost(ChargingPool.ChargingStations,

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingPoolDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingPoolDataResult>

            ISendPOIData.AddStaticData(IChargingPool       ChargingPool,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken   CancellationToken,
                                       EventTracking_Id?   EventTrackingId,
                                       TimeSpan?           RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    foreach (var station in ChargingPool)
                    {

                        if (IncludeChargingStations == null ||
                           (IncludeChargingStations != null && IncludeChargingStations(station)))
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

                return PushChargingPoolDataResult.Enqueued(Id, this, new IChargingPool[] { ChargingPool });

            }

            #endregion


            var result = await StationsPost(ChargingPool.ChargingStations,

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingPoolDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingPoolDataResult>

            ISendPOIData.UpdateStaticData(IChargingPool       ChargingPool,
                                          String?             PropertyName,
                                          Object?             NewValue,
                                          Object?             OldValue,
                                          Context?            DataSource,
                                          TransmissionTypes   TransmissionType,

                                          DateTime?           Timestamp,
                                          CancellationToken   CancellationToken,
                                          EventTracking_Id?   EventTrackingId,
                                          TimeSpan?           RequestTimeout)

        {

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
                //    DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRRequest));
                //}

                #endregion

                await DataAndStatusLock.WaitAsync();

                try
                {

                    foreach (var station in ChargingPool)
                    {

                        if (IncludeChargingStations == null ||
                           (IncludeChargingStations != null && IncludeChargingStations(station)))
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

                return PushChargingPoolDataResult.Enqueued(Id, this, new IChargingPool[] { ChargingPool });

            }

            #endregion


            var result = await StationsPost(ChargingPool.ChargingStations,

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingPoolDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingPoolDataResult>

            ISendPOIData.DeleteStaticData(IChargingPool       ChargingPool,
                                          TransmissionTypes   TransmissionType,

                                          DateTime?           Timestamp,
                                          CancellationToken   CancellationToken,
                                          EventTracking_Id?   EventTrackingId,
                                          TimeSpan?           RequestTimeout)

        {

            // Mark as deleted?
            var result = await StationsPost(ChargingPool.ChargingStations,

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingPoolDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingPoolDataResult>

            ISendPOIData.SetStaticData(IEnumerable<IChargingPool>  ChargingPools,
                                       TransmissionTypes           TransmissionType,

                                       DateTime?                   Timestamp,
                                       CancellationToken           CancellationToken,
                                       EventTracking_Id?           EventTrackingId,
                                       TimeSpan?                   RequestTimeout)

        {

            #region Initial checks

            if (!ChargingPools.Any())
                return new PushChargingPoolDataResult(
                           Id,
                           this,
                           PushDataResultTypes.NoOperation,
                           Array.Empty<PushSingleChargingPoolDataResult>(),
                           Array.Empty<PushSingleChargingPoolDataResult>()
                       );

            #endregion

            var result = await StationsPost(ChargingPools.SafeSelectMany(pool => pool.ChargingStations),

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingPoolDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingPoolDataResult>

            ISendPOIData.AddStaticData(IEnumerable<IChargingPool>  ChargingPools,
                                       TransmissionTypes           TransmissionType,

                                       DateTime?                   Timestamp,
                                       CancellationToken           CancellationToken,
                                       EventTracking_Id?           EventTrackingId,
                                       TimeSpan?                   RequestTimeout)

        {

            #region Initial checks

            if (!ChargingPools.Any())
                return new PushChargingPoolDataResult(
                           Id,
                           this,
                           PushDataResultTypes.NoOperation,
                           Array.Empty<PushSingleChargingPoolDataResult>(),
                           Array.Empty<PushSingleChargingPoolDataResult>()
                       );

            #endregion

            var result = await StationsPost(ChargingPools.SafeSelectMany(pool => pool.ChargingStations),

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingPoolDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingPoolDataResult>

            ISendPOIData.UpdateStaticData(IEnumerable<IChargingPool>  ChargingPools,
                                          TransmissionTypes           TransmissionType,

                                          DateTime?                   Timestamp,
                                          CancellationToken           CancellationToken,
                                          EventTracking_Id?           EventTrackingId,
                                          TimeSpan?                   RequestTimeout)

        {

            #region Initial checks

            if (!ChargingPools.Any())
                return new PushChargingPoolDataResult(
                           Id,
                           this,
                           PushDataResultTypes.NoOperation,
                           Array.Empty<PushSingleChargingPoolDataResult>(),
                           Array.Empty<PushSingleChargingPoolDataResult>()
                       );

            #endregion

            var result = await StationsPost(ChargingPools.SafeSelectMany(pool => pool.ChargingStations),

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingPoolDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
        async Task<PushChargingPoolDataResult>

            ISendPOIData.DeleteStaticData(IEnumerable<IChargingPool>  ChargingPools,
                                          TransmissionTypes           TransmissionType,

                                          DateTime?                   Timestamp,
                                          CancellationToken           CancellationToken,
                                          EventTracking_Id?           EventTrackingId,
                                          TimeSpan?                   RequestTimeout)

        {

            #region Initial checks

            if (!ChargingPools.Any())
                return new PushChargingPoolDataResult(
                           Id,
                           this,
                           PushDataResultTypes.NoOperation,
                           Array.Empty<PushSingleChargingPoolDataResult>(),
                           Array.Empty<PushSingleChargingPoolDataResult>()
                       );

            #endregion

            var result = await StationsPost(ChargingPools.SafeSelectMany(pool => pool.ChargingStations),

                                            Timestamp,
                                            CancellationToken,
                                            EventTrackingId,
                                            RequestTimeout);

            return new PushChargingPoolDataResult(
                       result.Id,
                       this,
                       result.Result,
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       Array.Empty<PushSingleChargingPoolDataResult>(),
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

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
                                               CancellationToken                           CancellationToken,
                                               EventTracking_Id?                           EventTrackingId,
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
                                     CancellationToken                      CancellationToken,
                                     EventTracking_Id?                      EventTrackingId,
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

            ISendPOIData.SetStaticData(IChargingStationOperator  ChargingStationOperator,
                                       TransmissionTypes         TransmissionType,

                                       DateTime?                 Timestamp,
                                       CancellationToken         CancellationToken,
                                       EventTracking_Id?          EventTrackingId,
                                       TimeSpan?                 RequestTimeout)

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

            ISendPOIData.AddStaticData(IChargingStationOperator  ChargingStationOperator,
                                       TransmissionTypes         TransmissionType,

                                       DateTime?                 Timestamp,
                                       CancellationToken         CancellationToken,
                                       EventTracking_Id?         EventTrackingId,
                                       TimeSpan?                 RequestTimeout)

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

            ISendPOIData.UpdateStaticData(IChargingStationOperator  ChargingStationOperator,
                                          String                    PropertyName,
                                          Object?                   NewValue,
                                          Object?                   OldValue,
                                          Context?                  DataSource,
                                          TransmissionTypes         TransmissionType,

                                          DateTime?                 Timestamp,
                                          CancellationToken         CancellationToken,
                                          EventTracking_Id?         EventTrackingId,
                                          TimeSpan?                 RequestTimeout)

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

            ISendPOIData.DeleteStaticData(IChargingStationOperator  ChargingStationOperator,
                                          TransmissionTypes         TransmissionType,

                                          DateTime?                 Timestamp,
                                          CancellationToken         CancellationToken,
                                          EventTracking_Id?         EventTrackingId,
                                          TimeSpan?                 RequestTimeout)

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

            ISendPOIData.SetStaticData(IEnumerable<IChargingStationOperator>  ChargingStationOperators,
                                       TransmissionTypes                      TransmissionType,

                                       DateTime?                              Timestamp,
                                       CancellationToken                      CancellationToken,
                                       EventTracking_Id?                      EventTrackingId,
                                       TimeSpan?                              RequestTimeout)

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

            ISendPOIData.AddStaticData(IEnumerable<IChargingStationOperator>  ChargingStationOperators,
                                       TransmissionTypes                      TransmissionType,

                                       DateTime?                              Timestamp,
                                       CancellationToken                      CancellationToken,
                                       EventTracking_Id?                      EventTrackingId,
                                       TimeSpan?                              RequestTimeout)

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

            ISendPOIData.UpdateStaticData(IEnumerable<IChargingStationOperator>  ChargingStationOperators,
                                          TransmissionTypes                      TransmissionType,

                                          DateTime?                              Timestamp,
                                          CancellationToken                      CancellationToken,
                                          EventTracking_Id?                      EventTrackingId,
                                          TimeSpan?                              RequestTimeout)

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

            ISendPOIData.DeleteStaticData(IEnumerable<IChargingStationOperator>  ChargingStationOperators,
                                          TransmissionTypes                      TransmissionType,

                                          DateTime?                              Timestamp,
                                          CancellationToken                      CancellationToken,
                                          EventTracking_Id?                      EventTrackingId,
                                          TimeSpan?                              RequestTimeout)

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
                                               CancellationToken                                      CancellationToken,
                                               EventTracking_Id?                                      EventTrackingId,
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
                                     CancellationToken                                 CancellationToken,
                                     EventTracking_Id?                                 EventTrackingId,
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

            ISendPOIData.SetStaticData(IRoamingNetwork     RoamingNetwork,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken   CancellationToken,
                                       EventTracking_Id?   EventTrackingId,
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

            ISendPOIData.AddStaticData(IRoamingNetwork     RoamingNetwork,
                                       TransmissionTypes   TransmissionType,

                                       DateTime?           Timestamp,
                                       CancellationToken   CancellationToken,
                                       EventTracking_Id?   EventTrackingId,
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

            ISendPOIData.UpdateStaticData(IRoamingNetwork     RoamingNetwork,
                                          String              PropertyName,
                                          Object?             NewValue,
                                          Object?             OldValue,
                                          Context?            DataSource,
                                          TransmissionTypes   TransmissionType,

                                          DateTime?           Timestamp,
                                          CancellationToken   CancellationToken,
                                          EventTracking_Id?   EventTrackingId,
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

            ISendPOIData.DeleteStaticData(IRoamingNetwork     RoamingNetwork,
                                          TransmissionTypes   TransmissionType,

                                          DateTime?           Timestamp,
                                          CancellationToken   CancellationToken,
                                          EventTracking_Id?   EventTrackingId,
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
                                               CancellationToken                             CancellationToken,
                                               EventTracking_Id?                             EventTrackingId,
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
                                     CancellationToken                        CancellationToken,
                                     EventTracking_Id?                        EventTrackingId,
                                     TimeSpan?                                RequestTimeout)


                => Task.FromResult(PushRoamingNetworkStatusResult.NoOperation(Id, this));

        #endregion

        #endregion

        #endregion


        #region AuthorizeStart(           LocalAuthentication, ChargingLocation = null, ChargingProduct = null, SessionId = null, OperatorId = null, ...)

        /// <summary>
        /// Create an authorize start request at the given EVSE.
        /// </summary>
        /// <param name="LocalAuthentication">An user identification.</param>
        /// <param name="ChargingLocation">The charging location.</param>
        /// <param name="ChargingProduct">An optional charging product.</param>
        /// <param name="SessionId">An optional session identification.</param>
        /// <param name="CPOPartnerSessionId">An optional session identification of the CPO.</param>
        /// <param name="OperatorId">An optional charging station operator identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public async Task<AuthStartResult>

            AuthorizeStart(LocalAuthentication               LocalAuthentication,
                           ChargingLocation?                 ChargingLocation      = null,
                           ChargingProduct?                  ChargingProduct       = null,   // [maxlength: 100]
                           ChargingSession_Id?               SessionId             = null,
                           ChargingSession_Id?               CPOPartnerSessionId   = null,
                           WWCP.ChargingStationOperator_Id?  OperatorId            = null,

                           DateTime?                         Timestamp             = null,
                           EventTracking_Id?                 EventTrackingId       = null,
                           TimeSpan?                         RequestTimeout        = null,
                           CancellationToken                 CancellationToken     = default)

        {

            #region Initial checks

            if (LocalAuthentication == null)
                throw new ArgumentNullException(nameof(LocalAuthentication),  "The given authentication token must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPORoaming.CPOClient.RequestTimeout;

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
                                                null,
                                                Id,
                                                OperatorId,
                                                LocalAuthentication,
                                                ChargingLocation,
                                                ChargingProduct,
                                                SessionId,
                                                CPOPartnerSessionId,
                                                new ISendAuthorizeStartStop[0],
                                                RequestTimeout);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeStartRequest));
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
                                                     SessionId:  SessionId,
                                                     Runtime:    Runtime);

            }

            else if (!LocalAuthentication.AuthToken.HasValue ||
                     !LocalAuthentication.AuthToken.ToOIOI().HasValue)
            {

                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                result   = AuthStartResult.NotAuthorized(
                               Id,
                               this,
                               SessionId:   SessionId,
                               ProviderId:  DefaultProviderId,
                               Runtime:     Runtime
                           );

            }

            else
            {

                var response = await CPORoaming.
                                         RFIDVerify(LocalAuthentication.AuthToken.ToOIOI().Value,

                                                    Timestamp,
                                                    CancellationToken,
                                                    EventTrackingId,
                                                    RequestTimeout);


                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                if (response?.HTTPStatusCode == HTTPStatusCode.OK &&
                    response?.Content        != null              &&
                    response?.Content.Code   == ResponseCodes.Success)
                {

                    result = AuthStartResult.Authorized(
                                 Id,
                                 this,
                                 SessionId:   SessionId,
                                 ProviderId:  DefaultProviderId,
                                 Runtime:     Runtime
                             );

                }

                else
                    result = AuthStartResult.NotAuthorized(
                                 Id,
                                 this,
                                 SessionId:   SessionId,
                                 ProviderId:  DefaultProviderId,
                                 Runtime:     Runtime
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
                                                 null,
                                                 Id,
                                                 OperatorId,
                                                 LocalAuthentication,
                                                 ChargingLocation,
                                                 ChargingProduct,
                                                 SessionId,
                                                 CPOPartnerSessionId,
                                                 new ISendAuthorizeStartStop[0],
                                                 RequestTimeout,
                                                 result,
                                                 Runtime);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeStartResponse));
            }

            #endregion

            return result;

        }

        #endregion

        #region AuthorizeStop (SessionId, LocalAuthentication, ChargingLocation = null,                                           OperatorId = null, ...)

        /// <summary>
        /// Create an authorize stop request at the given EVSE.
        /// </summary>
        /// <param name="SessionId">The session identification from the AuthorizeStart request.</param>
        /// <param name="LocalAuthentication">A user identification.</param>
        /// <param name="ChargingLocation">The charging location.</param>
        /// <param name="CPOPartnerSessionId">An optional session identification of the CPO.</param>
        /// <param name="OperatorId">An optional charging station operator identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public async Task<AuthStopResult>

            AuthorizeStop(ChargingSession_Id                SessionId,
                          LocalAuthentication               LocalAuthentication,
                          ChargingLocation?                 ChargingLocation      = null,
                          ChargingSession_Id?               CPOPartnerSessionId   = null,
                          WWCP.ChargingStationOperator_Id?  OperatorId            = null,

                          DateTime?                         Timestamp             = null,
                          EventTracking_Id?                 EventTrackingId       = null,
                          TimeSpan?                         RequestTimeout        = null,
                          CancellationToken                 CancellationToken     = default)
        {

            #region Initial checks

            if (LocalAuthentication  == null)
                throw new ArgumentNullException(nameof(LocalAuthentication), "The given authentication token must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPORoaming.CPOClient.RequestTimeout;

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
                                               null,
                                               Id,
                                               OperatorId,
                                               ChargingLocation,
                                               SessionId,
                                               CPOPartnerSessionId,
                                               LocalAuthentication,
                                               RequestTimeout);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeStopRequest));
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
                                                    SessionId:  SessionId,
                                                    Runtime:    Runtime);
            }

            else if (!LocalAuthentication.AuthToken.HasValue ||
                     !LocalAuthentication.AuthToken.ToOIOI().HasValue)
            {

                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                result   = AuthStopResult.NotAuthorized(
                               Id,
                               this,
                               SessionId:   SessionId,
                               ProviderId:  DefaultProviderId,
                               Runtime:     Runtime
                           );

            }

            else
            {

                var response = await CPORoaming.
                                         RFIDVerify(LocalAuthentication.AuthToken.ToOIOI().Value,

                                                    Timestamp,
                                                    CancellationToken,
                                                    EventTrackingId,
                                                    RequestTimeout).ConfigureAwait(false);


                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;

                if (response?.HTTPStatusCode == HTTPStatusCode.OK &&
                    response?.Content        != null              &&
                    response?.Content.Code   == ResponseCodes.Success)
                {

                    result = AuthStopResult.Authorized(
                                 Id,
                                 this,
                                 SessionId:   SessionId,
                                 ProviderId:  DefaultProviderId,
                                 Runtime:     Runtime
                             );

                }
                else
                    result = AuthStopResult.NotAuthorized(
                                 Id,
                                 this,
                                 SessionId:   SessionId,
                                 ProviderId:  DefaultProviderId,
                                 Runtime:     Runtime
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
                                                null,
                                                Id,
                                                OperatorId,
                                                ChargingLocation,
                                                SessionId,
                                                CPOPartnerSessionId,
                                                LocalAuthentication,
                                                RequestTimeout,
                                                result,
                                                Runtime);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnAuthorizeStopResponse));
            }

            #endregion

            return result;

        }

        #endregion


        #region SendChargeDetailRecords(ChargeDetailRecords, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Send charge detail records to an OIOI server.
        /// </summary>
        /// <param name="ChargeDetailRecords">An enumeration of charge detail records.</param>
        /// <param name="TransmissionType">Whether to send the CDR directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public async Task<SendCDRsResult>

            SendChargeDetailRecords(IEnumerable<ChargeDetailRecord>  ChargeDetailRecords,
                                    TransmissionTypes                TransmissionType    = TransmissionTypes.Enqueue,

                                    DateTime?                        Timestamp           = null,
                                    EventTracking_Id?                EventTrackingId     = null,
                                    TimeSpan?                        RequestTimeout      = null,
                                    CancellationToken                CancellationToken   = default)

        {

            #region Initial checks

            if (ChargeDetailRecords == null)
                throw new ArgumentNullException(nameof(ChargeDetailRecords),  "The given enumeration of charge detail records must not be null!");


            if (!Timestamp.HasValue)
                Timestamp = DateTime.UtcNow;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPORoaming.CPOClient.RequestTimeout;


            DateTime        Endtime;
            TimeSpan        Runtime;
            SendCDRsResult  results;

            #endregion

            #region Filter charge detail records

            var ForwardedCDRs  = new List<ChargeDetailRecord>();
            var FilteredCDRs   = new List<SendCDRResult>();

            foreach (var cdr in ChargeDetailRecords)
            {

                if (ChargeDetailRecordFilter(cdr) == ChargeDetailRecordFilters.forward)
                    ForwardedCDRs.Add(cdr);

                else
                    FilteredCDRs.Add(SendCDRResult.Filtered(DateTime.UtcNow,
                                                            cdr,
                                                            Warning: Warning.Create(I18NString.Create(Languages.en, "This charge detail record was filtered!"))));

            }

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
                                          ChargeDetailRecords,
                                          RequestTimeout);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRsRequest));
            }

            #endregion


            #region if disabled => 'AdminDown'...

            if (DisableSendChargeDetailRecords)
            {

                Endtime  = DateTime.UtcNow;
                Runtime  = Endtime - StartTime;
                results   = SendCDRsResult.AdminDown(DateTime.UtcNow,
                                                     Id,
                                                     this,
                                                     ChargeDetailRecords,
                                                     Runtime: Runtime);

            }

            #endregion

            else
            {

                var invokeTimer = false;
                var LockTaken    = await FlushChargeDetailRecordsLock.WaitAsync(TimeSpan.FromSeconds(60));

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
                                                                 ChargeDetailRecords,
                                                                 RequestTimeout);

                            }
                            catch (Exception e)
                            {
                                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRsRequest));
                            }

                            #endregion

                            foreach (var ChargeDetailRecord in ChargeDetailRecords)
                            {

                                try
                                {

                                    var connectorId = ChargeDetailRecord.EVSEId.ToOIOI(CustomEVSEIdMapper);
                                    if (connectorId.HasValue)
                                    {

                                        chargeDetailRecordsQueue.Add(ChargeDetailRecord.ToOIOI(CPOClient.ConnectorIdPartnerIdSelector(connectorId.Value),
                                                                                               CustomEVSEIdMapper,
                                                                                               CustomChargeDetailRecord2Session));

                                        SendCDRsResults.Add(SendCDRResult.Enqueued(DateTime.UtcNow,
                                                                                   ChargeDetailRecord));

                                    }
                                    else
                                        SendCDRsResults.Add(SendCDRResult.CouldNotConvertCDRFormat(DateTime.UtcNow,
                                                                                                   ChargeDetailRecord,
                                                                                                   Warning: Warning.Create(I18NString.Create(Languages.en, "Could not parse connector identification!"))));

                                }
                                catch (Exception e)
                                {
                                    SendCDRsResults.Add(SendCDRResult.CouldNotConvertCDRFormat(DateTime.UtcNow,
                                                                                               ChargeDetailRecord,
                                                                                               Warning: Warning.Create(I18NString.Create(Languages.en, e.Message))));
                                }

                            }

                            Endtime      = DateTime.UtcNow;
                            Runtime      = Endtime - StartTime;
                            results      = SendCDRsResult.Enqueued(DateTime.UtcNow,
                                                                   Id,
                                                                   this,
                                                                   ChargeDetailRecords,
                                                                   I18NString.Create(Languages.en, "Enqueued for at least " + FlushChargeDetailRecordsEvery.TotalSeconds + " seconds!"),
                                                                   //SendCDRsResults.SafeWhere(cdrresult => cdrresult.Result != SendCDRResultTypes.Enqueued),
                                                                   Runtime: Runtime);
                            invokeTimer  = true;

                        }

                        #endregion

                        #region ...or send at once!

                        else
                        {

                            HTTPResponse<SessionPostResponse> response;
                            SendCDRResult result;

                            foreach (var chargeDetailRecord in ChargeDetailRecords)
                            {

                                try
                                {

                                    var connectorId = chargeDetailRecord.EVSEId.ToOIOI(CustomEVSEIdMapper);
                                    if (connectorId.HasValue)
                                    {

                                        response = await CPORoaming.SessionPost(chargeDetailRecord.ToOIOI(CPOClient.ConnectorIdPartnerIdSelector(connectorId.Value),
                                                                                                          CustomEVSEIdMapper,
                                                                                                          CustomChargeDetailRecord2Session),

                                                                                Timestamp,
                                                                                CancellationToken,
                                                                                EventTrackingId,
                                                                                RequestTimeout);


                                        if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                                            response.Content        != null              &&
                                            response.Content.Code   == ResponseCodes.Success)
                                        {

                                            result = SendCDRResult.Success(DateTime.UtcNow,
                                                                           chargeDetailRecord);

                                        }

                                        else
                                            result = SendCDRResult.Error(DateTime.UtcNow,
                                                                         chargeDetailRecord,
                                                                         Warning.Create(I18NString.Create(Languages.en, response.HTTPBodyAsUTF8String)));

                                    }
                                    else
                                        result = SendCDRResult.CouldNotConvertCDRFormat(DateTime.UtcNow,
                                                                                        chargeDetailRecord,
                                                                                        Warning.Create(I18NString.Create(Languages.en, "Could not parse connector identification!")));

                                }
                                catch (Exception e)
                                {
                                    result = SendCDRResult.CouldNotConvertCDRFormat(DateTime.UtcNow,
                                                                                    chargeDetailRecord,
                                                                                    Warning.Create(I18NString.Create(Languages.en, e.Message)));
                                }

                                SendCDRsResults.Add(result);
                                RoamingNetwork.SessionsStore.CDRForwarded(chargeDetailRecord.SessionId, result);

                            }

                            Endtime  = DateTime.UtcNow;
                            Runtime  = Endtime - StartTime;

                            if (SendCDRsResults.All(cdrresult => cdrresult.Result == SendCDRResultTypes.Success))
                                results = SendCDRsResult.Success(DateTime.UtcNow,
                                                                 Id,
                                                                 this,
                                                                 ChargeDetailRecords,
                                                                 Runtime: Runtime);

                            else
                                results = SendCDRsResult.Error(DateTime.UtcNow,
                                                               Id,
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
                        results  = SendCDRsResult.Timeout(DateTime.UtcNow,
                                                          Id,
                                                          this,
                                                          ChargeDetailRecords,
                                                          I18NString.Create(Languages.en, "Could not " + (TransmissionType == TransmissionTypes.Enqueue ? "enqueue" : "send") + " charge detail records!"),
                                                          //ChargeDetailRecords.SafeSelect(cdr => new SendCDRResult(cdr, SendCDRResultTypes.Timeout)),
                                                          Runtime: Runtime);

                    }

                    #endregion

                }
                finally
                {
                    if (LockTaken)
                        FlushChargeDetailRecordsLock.Release();
                }

                if (invokeTimer)
                    FlushChargeDetailRecordsTimer.Change(FlushChargeDetailRecordsEvery, TimeSpan.FromMilliseconds(-1));

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
                                           ChargeDetailRecords,
                                           RequestTimeout,
                                           results,
                                           Runtime);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(WWCPCPOAdapter) + "." + nameof(OnSendCDRsResponse));
            }

            #endregion

            return results;

        }

        #endregion


        // -----------------------------------------------------------------------------------------------------


        #region (timer) FlushEVSEDataAndStatus()

        protected override Boolean SkipFlushEVSEDataAndStatusQueues()
            => StationsToAddQueue.           Count == 0 &&
               StationsToUpdateQueue.        Count == 0 &&
               evseStatusChangesDelayedQueue.Count == 0 &&
               StationsToRemoveQueue.        Count == 0;

        protected override async Task FlushEVSEDataAndStatusQueues()
        {

            #region Get a copy of all current EVSE data and delayed status

            var StationsToAddQueueCopy                 = new HashSet<IChargingStation>();
            var StationsToUpdateQueueCopy              = new HashSet<IChargingStation>();
            var StationsStatusChangesDelayedQueueCopy  = new List   <EVSEStatusUpdate>();
            var StationsToRemoveQueueCopy              = new HashSet<IChargingStation>();
            var StationsUpdateLogCopy                  = new Dictionary<Station,          PropertyUpdateInfo[]>();
            var ChargingStationsUpdateLogCopy          = new Dictionary<IChargingStation, PropertyUpdateInfo[]>();
            var ChargingPoolsUpdateLogCopy             = new Dictionary<IChargingPool,    PropertyUpdateInfo[]>();

            await DataAndStatusLock.WaitAsync();

            try
            {

                // Copy 'EVSEs to add', remove originals...
                StationsToAddQueueCopy                      = new HashSet<IChargingStation>      (StationsToAddQueue);
                StationsToAddQueue.Clear();

                // Copy 'EVSEs to update', remove originals...
                StationsToUpdateQueueCopy                   = new HashSet<IChargingStation>      (StationsToUpdateQueue);
                StationsToUpdateQueue.Clear();

                // Copy 'EVSE status changes', remove originals...
                StationsStatusChangesDelayedQueueCopy       = new List<EVSEStatusUpdate>         (evseStatusChangesDelayedQueue);
                StationsStatusChangesDelayedQueueCopy.AddRange(StationsToAddQueueCopy.SelectMany(stations => stations.EVSEs).Select(evse => new EVSEStatusUpdate(evse.Id, evse.Status, evse.Status)));
                evseStatusChangesDelayedQueue.Clear();

                // Copy 'EVSEs to remove', remove originals...
                StationsToRemoveQueueCopy                   = new HashSet<IChargingStation>      (StationsToRemoveQueue);
                StationsToRemoveQueue.Clear();

                // Copy EVSE property updates
                //////evsesUpdateLog.           ForEach(_ => StationsUpdateLogCopy.           Add(_.Key, _.Value.ToArray()));
                evsesUpdateLog.Clear();

                // Copy charging station property updates
                chargingStationsUpdateLog.ForEach(_ => ChargingStationsUpdateLogCopy.Add(_.Key, _.Value.ToArray()));
                chargingStationsUpdateLog.Clear();

                // Copy charging pool property updates
                chargingPoolsUpdateLog.   ForEach(_ => ChargingPoolsUpdateLogCopy.   Add(_.Key, _.Value.ToArray()));
                chargingPoolsUpdateLog.Clear();


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

            #region Send new stations data

            if (StationsToAddQueueCopy.Any())
            {

                var StationsToAddResult = await StationsPost(StationsToAddQueueCopy,
                                                             EventTrackingId: EventTrackingId).
                                                ConfigureAwait(false);

                //foreach (var evseId in StationsToUpdateResult.SuccessfulEVSEs)
                //    SuccessfullyUploadedEVSEs.Add(evseId.Id);

                if (StationsToAddResult.Warnings.Any())
                {
                    try
                    {

                        SendOnWarnings(DateTime.UtcNow,
                                       nameof(WWCPCPOAdapter) + Id,
                                       nameof(StationsToAddResult),
                                       StationsToAddResult.Warnings);

                    }
                    catch
                    { }
                }

            }

            #endregion

            #region Send changed stations data

            if (StationsToUpdateQueueCopy.Any())
            {

                // Surpress station data updates for all newly added stations
                foreach (var _station in StationsToUpdateQueueCopy.Where(station => StationsToAddQueueCopy.Contains(station)).ToArray())
                    StationsToUpdateQueueCopy.Remove(_station);

                if (StationsToUpdateQueueCopy.Any())
                {

                    var StationsToUpdateResult = await StationsPost(StationsToUpdateQueueCopy,
                                                                    EventTrackingId: EventTrackingId).
                                                       ConfigureAwait(false);

                    //foreach (var evseId in StationsToUpdateResult.SuccessfulEVSEs)
                    //    SuccessfullyUploadedEVSEs.Add(evseId.Id);

                    if (StationsToUpdateResult.Warnings.Any())
                    {
                        try
                        {

                            SendOnWarnings(DateTime.UtcNow,
                                           nameof(WWCPCPOAdapter) + Id,
                                           nameof(StationsToUpdateResult),
                                           StationsToUpdateResult.Warnings);

                        }
                        catch
                        { }
                    }

                }

            }

            #endregion

            #region Send changed evses status

            if (!DisablePushStatus &&
                StationsStatusChangesDelayedQueueCopy.Count > 0)
            {

                var StationsStatusChangesResult = await ConnectorsPostStatus(StationsStatusChangesDelayedQueueCopy,
                                                                             EventTrackingId: EventTrackingId).
                                                        ConfigureAwait(false);

                //foreach (var evseId in StationsToUpdateResult.SuccessfulEVSEs)
                //    SuccessfullyUploadedEVSEs.Add(evseId.Id);

                if (StationsStatusChangesResult.Warnings.Any())
                {
                    try
                    {

                        SendOnWarnings(DateTime.UtcNow,
                                       nameof(WWCPCPOAdapter) + Id,
                                       nameof(StationsStatusChangesResult),
                                       StationsStatusChangesResult.Warnings);

                    }
                    catch
                    { }
                }

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
            => evseStatusChangesFastQueue.Count == 0;

        protected override async Task FlushEVSEFastStatusQueues()
        {

            #region Get a copy of all current EVSE data and delayed status

            var EVSEStatusFastQueueCopy = new List<EVSEStatusUpdate>();

            var LockTaken = await DataAndStatusLock.WaitAsync(MaxLockWaitingTime);

            try
            {

                if (LockTaken)
                {

                    // Copy 'EVSE status changes', remove originals...
                    EVSEStatusFastQueueCopy = new List<EVSEStatusUpdate>(evseStatusChangesFastQueue.Where(evsestatuschange => !StationsToAddQueue.Any(station => station.EVSEs.Any(evse => evse.Id == evsestatuschange.Id))));

                    // Add all evse status changes of EVSE *NOT YET UPLOADED* into the delayed queue...
                    var EVSEStatusChangesDelayed = evseStatusChangesFastQueue.Where(evsestatuschange => StationsToAddQueue.Any(station => station.EVSEs.Any(evse => evse.Id == evsestatuschange.Id))).ToArray();

                    if (EVSEStatusChangesDelayed.Length > 0)
                        evseStatusChangesDelayedQueue.AddRange(EVSEStatusChangesDelayed);

                    evseStatusChangesFastQueue.Clear();

                    // Stop the timer. Will be rescheduled by next EVSE status change...
                    FlushEVSEFastStatusTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

                }

            }
            finally
            {
                if (LockTaken)
                    DataAndStatusLock.Release();
            }

            #endregion

            #region Send changed EVSE status

            if (EVSEStatusFastQueueCopy.Count > 0)
            {

                var ConnectorsPostStatusResult = await ConnectorsPostStatus(EVSEStatusFastQueueCopy,
                                                                            DateTime.UtcNow,
                                                                            new CancellationTokenSource().Token,
                                                                            EventTracking_Id.New,
                                                                            DefaultRequestTimeout).
                                                       ConfigureAwait(false);

                if (ConnectorsPostStatusResult.Warnings.Any())
                {

                    SendOnWarnings(DateTime.UtcNow,
                                   nameof(WWCPCPOAdapter) + Id,
                                   nameof(ConnectorsPostStatusResult),
                                   ConnectorsPostStatusResult.Warnings);

                }

            }

            #endregion

        }

        #endregion

        #region (timer) FlushChargeDetailRecords()

        protected override Boolean SkipFlushChargeDetailRecordsQueues()
            => chargeDetailRecordsQueue.Count == 0;

        protected override async Task FlushChargeDetailRecordsQueues(IEnumerable<Session> ChargingSessions)
        {

            HTTPResponse<SessionPostResponse> response;
            SendCDRResult result;

            foreach (var chargingSession in ChargingSessions)
            {

                try
                {

                    response = await CPORoaming.SessionPost(chargingSession,

                                                            DateTime.UtcNow,
                                                            new CancellationTokenSource().Token,
                                                            EventTracking_Id.New,
                                                            DefaultRequestTimeout).
                                                ConfigureAwait(false);

                    if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                        response.Content        != null &&
                        response.Content.Code   == 0)
                    {

                        result = SendCDRResult.Success(DateTime.UtcNow,
                                                       chargingSession.GetInternalDataAs<ChargeDetailRecord>(OIOIMapper.WWCP_CDR),
                                                       Runtime: response.Runtime);

                    }

                    else
                        result = SendCDRResult.Error(DateTime.UtcNow,
                                                     chargingSession.GetInternalDataAs<ChargeDetailRecord>(OIOIMapper.WWCP_CDR),
                                                     Warning.Create(I18NString.Create(Languages.en, response.HTTPBodyAsUTF8String)),
                                                     Runtime: response.Runtime);

                }
                catch (Exception e)
                {
                    result = SendCDRResult.Error(DateTime.UtcNow,
                                                 chargingSession.GetInternalDataAs<ChargeDetailRecord>(OIOIMapper.WWCP_CDR),
                                                 Warning.Create(I18NString.Create(Languages.en, e.Message)),
                                                 Runtime: TimeSpan.Zero);
                }

                RoamingNetwork.SessionsStore.CDRForwarded(chargingSession.Id.ToWWCP(), result);

            }

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
        public override Int32 CompareTo(Object Object)
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
