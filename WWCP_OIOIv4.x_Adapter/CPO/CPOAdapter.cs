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

using System.Text.RegularExpressions;

using Org.BouncyCastle.Crypto.Parameters;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

using cloud.charging.open.protocols.WWCP;
using Telegram.Bot.Types;
using System.Linq;

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x.CPO
{

    /// <summary>
    /// A WWCP wrapper for the OIOI CPO Roaming client which maps
    /// WWCP data structures onto OIOI data structures and vice versa.
    /// </summary>
    public class CPOAdapter : AWWCPCSOAdapter<Session>,
                              ICSORoamingProvider,
                              IEquatable <CPOAdapter>,
                              IComparable<CPOAdapter>,
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
                                                               CPOAdapter  Sender,
                                                               Exception       Exception);

        public event OnWWCPCPOAdapterExceptionDelegate OnWWCPCPOAdapterException;

        #endregion


        public delegate void FlushServiceQueuesDelegate(CPOAdapter Sender, TimeSpan Every);

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
        public CPOAdapter(CSORoamingProvider_Id                            Id,
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
            this.DisableSendStatus                                     = DisablePushStatus;
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

                RemoteStartResult? response = null;

                var EVSEId   = ConnectorId.ToWWCP(CustomConnectorIdMapper);

                if (!EVSEId.HasValue)
                    response = RemoteStartResult.UnknownLocation(TimeSpan.Zero);

                else
                    response = await RoamingNetwork.
                                         RemoteStart(CSORoamingProvider:    this,
                                                     ChargingLocation:      ChargingLocation.FromEVSEId(EVSEId),
                                                     RemoteAuthentication:  RemoteAuthentication.FromRemoteIdentification(WWCP.EMobilityAccount_Id.Parse(eMAId.ToString())),
                                                     SessionId:             ChargingSession_Id.NewRandom(),
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

                var response = await RoamingNetwork.RemoteStop(CSORoamingProvider:    this,
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


                Result? SessionStopResult = null;

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
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        private async Task<AddOrUpdateChargingStationsResult>

            StationsPost(IEnumerable<IChargingStation>  ChargingStations,

                         DateTime?                      Timestamp           = null,
                         EventTracking_Id?              EventTrackingId     = null,
                         TimeSpan?                      RequestTimeout      = null,
                         CancellationToken              CancellationToken   = default)

        {

            #region Initial checks

            Timestamp       ??= org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
            EventTrackingId ??= EventTracking_Id.New;
            RequestTimeout  ??= CPORoaming.CPOClient.RequestTimeout;

            #endregion

            #region Get effective number of stations to upload

            var warnings       = new List<Warning>();

            var wwcpStations   = new Dictionary<Station_Id, IChargingStation>();

            var stationTuples  = ChargingStations.Where (chargingStation => chargingStation is not null && IncludeChargingStations(chargingStation)).
                                                  Select(chargingStation => {

                                                      try
                                                      {

                                                          wwcpStations.Add(chargingStation.Id.ToOIOI(), chargingStation);

                                                          var oioiStation = chargingStation.ToOIOI(CustomOperatorIdMapper,
                                                                                                   CustomEVSEIdMapper,
                                                                                                   ChargingStation2Station);

                                                          if (oioiStation is not null)
                                                              return new Tuple<IChargingStation, Station>(chargingStation,
                                                                                                          oioiStation);

                                                      }
                                                      catch (Exception e)
                                                      {
                                                          warnings.Add(Warning.Create(e.Message.ToI18NString(), chargingStation));
                                                      }

                                                      return null;

                                                  }).
                                                  Where(stationTuple => stationTuple is not null).
                                                  Cast<Tuple<IChargingStation, Station>>().
                                                  ToArray() ?? Array.Empty<Tuple<IChargingStation, Station>>();

            #endregion

            #region Send OnStationPostWWCPRequest event

            var startTime = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;

            try
            {

                OnStationPostWWCPRequest?.Invoke(startTime,
                                                 Timestamp.Value,
                                                 this,
                                                 Id,
                                                 EventTrackingId,
                                                 RoamingNetwork.Id,
                                                 stationTuples.ULongCount(),
                                                 stationTuples,
                                                 warnings.Where(warning => warning.IsNeitherNullNorEmpty()),
                                                 RequestTimeout);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnStationPostWWCPRequest));
            }

            #endregion


            DateTime                            endtime;
            TimeSpan                            runtime;
            AddOrUpdateChargingStationsResult?  result   = null;

            if (DisablePushData)
            {

                endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
                runtime  = endtime - startTime;
                result   = AddOrUpdateChargingStationsResult.AdminDown(
                               ChargingStations,
                               Id,
                               this,
                               EventTrackingId,
                               Warnings: warnings
                           );

            }

            else if (stationTuples.Length == 0)
            {

                endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
                runtime  = endtime - startTime;
                result   = AddOrUpdateChargingStationsResult.NoOperation(
                               ChargingStations,
                               Id,
                               this,
                               EventTrackingId,
                               Warnings: warnings
                           );

            }

            else
            {

                // Plugsurfing does not like too many concurrent requests
                var semaphore  = new SemaphoreSlim(_maxDegreeOfParallelism,
                                                   _maxDegreeOfParallelism);

                var tasks      = stationTuples.Select(async chargingStationTuple => {

                    await semaphore.WaitAsync().ConfigureAwait(false);

                    try
                    {

                        return await CPORoaming.StationPost(chargingStationTuple.Item2,
                                                            CPORoaming.CPOClient.StationPartnerIdSelector(chargingStationTuple.Item2),

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
                                      task.Result.Content is not null)
                                  {

                                      if (task.Result.Content.Code == ResponseCodes.Success)
                                          return new AddOrUpdateChargingStationResult(
                                                     wwcpStations[task.Result.Content.Request.Station.Id],
                                                     CommandResult.Success,
                                                     EventTrackingId,
                                                     Id,
                                                     this,
                                                     null,
                                                     AddedOrUpdated.Add,
                                                     null,
                                                     new[] {
                                                         Warning.Create(task.Result.Content.Message)
                                                     }
                                                 );

                                      else
                                          return new AddOrUpdateChargingStationResult(
                                                     wwcpStations[task.Result.Content.Request.Station.Id],
                                                     CommandResult.Error,
                                                     EventTrackingId,
                                                     Id,
                                                     this,
                                                     null,
                                                     AddedOrUpdated.Failed,
                                                     null,
                                                     new[] {
                                                         Warning.Create(task.Result.Content.Message)
                                                     }
                                                 );

                                  }
                                  else
                                      return new AddOrUpdateChargingStationResult(
                                                 wwcpStations[task.Result.Content.Request.Station.Id],
                                                 CommandResult.Error,
                                                 EventTrackingId,
                                                 Id,
                                                 this,
                                                 null,
                                                 AddedOrUpdated.Failed,
                                                 null,
                                                 new[] {
                                                     Warning.Create(task.Result.HTTPStatusCode.ToString())
                                                 }.Concat(
                                                     task.Result.HTTPBody is not null
                                                         ? warnings.AddAndReturnList(Warning.Create(task.Result.HTTPBody.ToUTF8String()))
                                                         : warnings.AddAndReturnList(Warning.Create("No HTTP body received!"))
                                                 ));

                });

                endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
                runtime  = endtime - startTime;

                result   = results.All(result => result.Result == CommandResult.Success)

                               ? AddOrUpdateChargingStationsResult.Added(
                                     ChargingStations,
                                     Id,
                                     this,
                                     EventTrackingId,
                                     null,
                                     warnings,
                                     runtime)

                               : AddOrUpdateChargingStationsResult.Error(
                                     ChargingStations,
                                     //results.Where(_ => _.Result != PushSingleDataResultTypes.Success),
                                     "".ToI18NString(),
                                     EventTrackingId,
                                     Id,
                                     this,
                                     warnings,
                                     runtime
                                 );


            }


            #region Send OnStationPostResponse event

            try
            {

                OnStationPostWWCPResponse?.Invoke(endtime,
                                                  Timestamp.Value,
                                                  this,
                                                  Id,
                                                  EventTrackingId,
                                                  RoamingNetwork.Id,
                                                  stationTuples.ULongCount(),
                                                  stationTuples,
                                                  RequestTimeout,
                                                  result,
                                                  runtime);

            }
            catch (Exception e)
            {
                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnStationPostWWCPResponse));
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
                Timestamp = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;

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

            var StartTime = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;

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
                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnConnectorStatusPostRequest));
            }

            #endregion


            DateTime              Endtime;
            TimeSpan              Runtime;
            PushEVSEStatusResult  result;

            if (DisableSendStatus)
            {
                Endtime = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
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

                Endtime = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
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
                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnConnectorStatusPostResponse));
            }

            #endregion

            return result;

        }

        #endregion


        #region (Set/Add/Update/Delete) EVSE(s)...

        #region AddEVSE         (EVSE, TransmissionType = Enqueue, ...)

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
        public override async Task<AddEVSEResult>

            AddEVSE(IEVSE               EVSE,
                    TransmissionTypes   TransmissionType    = TransmissionTypes.Enqueue,

                    DateTime?           Timestamp           = null,
                    EventTracking_Id?   EventTrackingId     = null,
                    TimeSpan?           RequestTimeout      = null,
                    CancellationToken   CancellationToken   = default)

        {

            #region Initital checks

            if (EVSE.ChargingStation is null)
                return AddEVSEResult.ArgumentError(
                           EVSE,
                           I18NString.Create(Languages.en, "The given EVSE must reference a charging station!"),
                           EventTrackingId,
                           Id,
                           this,
                           EVSE.ChargingStation
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    if (IncludeChargingStations is     null ||
                       (IncludeChargingStations is not null && IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToAddQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                           TimeSpan.FromMilliseconds(-1));

                    }

                    return AddEVSEResult.Enqueued(
                               EVSE,
                               EventTrackingId,
                               Id,
                               this,
                               EVSE.ChargingStation
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(new[] { EVSE.ChargingStation },

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new AddEVSEResult(
                       EVSE,
                       result.Result,
                       result.EventTrackingId,
                       result.SenderId,
                       result.Sender,
                       EVSE.ChargingStation,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region AddOrUpdateEVSE (EVSE, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Set the given EVSE as new static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="EVSE">An EVSE to upload.</param>
        /// <param name="TransmissionType">Whether to send the EVSE directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<AddOrUpdateEVSEResult>

            AddOrUpdateEVSE(IEVSE               EVSE,
                            TransmissionTypes   TransmissionType    = TransmissionTypes.Enqueue,

                            DateTime?           Timestamp           = null,
                            EventTracking_Id?   EventTrackingId     = null,
                            TimeSpan?           RequestTimeout      = null,
                            CancellationToken   CancellationToken   = default)

        {

            #region Initital checks

            if (EVSE.ChargingStation is null)
                return AddOrUpdateEVSEResult.ArgumentError(
                           EVSE,
                           I18NString.Create(Languages.en, "The given EVSE must reference a charging station!"),
                           EventTrackingId,
                           Id,
                           this,
                           EVSE.ChargingStation
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    if (IncludeChargingStations is     null ||
                       (IncludeChargingStations is not null && IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToAddQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                           TimeSpan.FromMilliseconds(-1));

                    }

                    return AddOrUpdateEVSEResult.Enqueued(
                               EVSE,
                               EventTrackingId,
                               Id,
                               this,
                               EVSE.ChargingStation
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(new[] { EVSE.ChargingStation },

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new AddOrUpdateEVSEResult(
                       EVSE,
                       result.Result,
                       result.EventTrackingId,
                       result.SenderId,
                       result.Sender,
                       EVSE.ChargingStation,
                       AddedOrUpdated.Add,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region UpdateEVSE      (EVSE, PropertyName = null, OldValue = null, NewValue = null, TransmissionType = Enqueue, ...)

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
        public override async Task<UpdateEVSEResult>

            UpdateEVSE(IEVSE               EVSE,
                       String              PropertyName,
                       Object?             NewValue,
                       Object?             OldValue,
                       Context?            DataSource,
                       TransmissionTypes   TransmissionType    = TransmissionTypes.Enqueue,

                       DateTime?           Timestamp           = null,
                       EventTracking_Id?   EventTrackingId     = null,
                       TimeSpan?           RequestTimeout      = null,
                       CancellationToken   CancellationToken   = default)

        {

            #region Initial checks

            if (EVSE.ChargingStation is null)
                return UpdateEVSEResult.ArgumentError(
                           EVSE,
                           I18NString.Create(Languages.en, "The given EVSE must reference a charging station!"),
                           EventTrackingId,
                           Id,
                           this,
                           EVSE.ChargingStation
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    if (IncludeChargingStations is     null ||
                       (IncludeChargingStations is not null && IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToUpdateQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                           TimeSpan.FromMilliseconds(-1));

                    }

                    return UpdateEVSEResult.Enqueued(
                               EVSE,
                               EventTrackingId,
                               Id,
                               this,
                               EVSE.ChargingStation
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(new[] { EVSE.ChargingStation },

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new UpdateEVSEResult(
                       EVSE,
                       result.Result,
                       result.EventTrackingId,
                       result.SenderId,
                       result.Sender,
                       EVSE.ChargingStation,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region DeleteEVSE      (EVSE, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Delete the static data of the given EVSE.
        /// </summary>
        /// <param name="EVSE">An EVSE to delete.</param>
        /// <param name="TransmissionType">Whether to send the EVSE deletion directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<DeleteEVSEResult>

            DeleteEVSE(IEVSE               EVSE,
                       TransmissionTypes   TransmissionType    = TransmissionTypes.Enqueue,

                       DateTime?           Timestamp           = null,
                       EventTracking_Id?   EventTrackingId     = null,
                       TimeSpan?           RequestTimeout      = null,
                       CancellationToken   CancellationToken   = default)

        {

            #region Initial checks

            if (EVSE.ChargingStation is null)
                return DeleteEVSEResult.ArgumentError(
                           EVSE,
                           I18NString.Create(Languages.en, "The given EVSE must reference a charging station!"),
                           EventTrackingId,
                           Id,
                           this,
                           EVSE.ChargingStation
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    if (IncludeChargingStations is     null ||
                       (IncludeChargingStations is not null && IncludeChargingStations(EVSE.ChargingStation)))
                    {

                        StationsToUpdateQueue.Add(EVSE.ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                           TimeSpan.FromMilliseconds(-1));

                    }

                    return DeleteEVSEResult.Enqueued(
                               EVSE,
                               EventTrackingId,
                               Id,
                               this,
                               EVSE.ChargingStation
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(new[] { EVSE.ChargingStation },

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new DeleteEVSEResult(
                       EVSE,
                       result.Result,
                       result.EventTrackingId,
                       result.SenderId,
                       result.Sender,
                       EVSE.ChargingStation,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion


        #region AddEVSEs        (EVSEs, ...)

        /// <summary>
        /// Add the given enumeration of EVSEs to the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="EVSEs">An enumeration of EVSEs.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<AddEVSEsResult>

            AddEVSEs(IEnumerable<IEVSE>  EVSEs,
                     TransmissionTypes   TransmissionType    = TransmissionTypes.Enqueue,

                     DateTime?           Timestamp           = null,
                     EventTracking_Id?   EventTrackingId     = null,
                     TimeSpan?           RequestTimeout      = null,
                     CancellationToken   CancellationToken   = default)

        {

            #region Initial checks

            if (!EVSEs.Any())
                return AddEVSEsResult.NoOperation(
                           EVSEs,
                           Id,
                           this,
                           EventTrackingId
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    foreach (var evse in EVSEs)
                    {
                        if (evse.ChargingStation is not null)
                        {

                            if (IncludeChargingStations is     null ||
                               (IncludeChargingStations is not null && IncludeChargingStations(evse.ChargingStation)))
                            {

                                StationsToAddQueue.Add(evse.ChargingStation);

                                FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                                   TimeSpan.FromMilliseconds(-1));

                            }

                        }
                    }

                    return AddEVSEsResult.Enqueued(
                               EVSEs,
                               Id,
                               this,
                               EventTrackingId
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(EVSEs.Select(evse            => evse.ChargingStation).
                                                  Where (chargingStation => chargingStation is not null).
                                                  Cast<ChargingStation>().
                                                  Distinct(),

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new AddEVSEsResult(
                       result.Result,
                       result.SuccessfulItems.SelectMany(res => res.ChargingStation!.EVSEs).Intersect(EVSEs).Select(evse => new AddEVSEResult(evse, CommandResult.Success)),
                       result.RejectedItems.  SelectMany(res => res.ChargingStation!.EVSEs).Intersect(EVSEs).Select(evse => new AddEVSEResult(evse, CommandResult.Error)),
                       result.SenderId,
                       result.Sender,
                       result.EventTrackingId,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region AddOrUpdateEVSEs(EVSEs, ...)

        /// <summary>
        /// Set the given enumeration of EVSEs as new static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="EVSEs">An enumeration of EVSEs.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<AddOrUpdateEVSEsResult>

            AddOrUpdateEVSEs(IEnumerable<IEVSE>  EVSEs,
                             TransmissionTypes   TransmissionType    = TransmissionTypes.Enqueue,

                             DateTime?           Timestamp           = null,
                             EventTracking_Id?   EventTrackingId     = null,
                             TimeSpan?           RequestTimeout      = null,
                             CancellationToken   CancellationToken   = default)

        {

            #region Initial checks

            if (!EVSEs.Any())
                return AddOrUpdateEVSEsResult.NoOperation(
                           EVSEs,
                           Id,
                           this,
                           EventTrackingId
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    foreach (var evse in EVSEs)
                    {
                        if (evse.ChargingStation is not null)
                        {

                            if (IncludeChargingStations is     null ||
                               (IncludeChargingStations is not null && IncludeChargingStations(evse.ChargingStation)))
                            {

                                StationsToAddQueue.Add(evse.ChargingStation);

                                FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                                   TimeSpan.FromMilliseconds(-1));

                            }

                        }
                    }

                    return AddOrUpdateEVSEsResult.Enqueued(
                               EVSEs,
                               Id,
                               this,
                               EventTrackingId
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(EVSEs.Select(evse            => evse.ChargingStation).
                                                  Where (chargingStation => chargingStation is not null).
                                                  Cast<ChargingStation>().
                                                  Distinct(),

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new AddOrUpdateEVSEsResult(
                       result.Result,
                       result.SuccessfulItems.SelectMany(res => res.ChargingStation!.EVSEs).Intersect(EVSEs).Select(evse => new AddOrUpdateEVSEResult(evse, CommandResult.Success)),
                       result.RejectedItems.  SelectMany(res => res.ChargingStation!.EVSEs).Intersect(EVSEs).Select(evse => new AddOrUpdateEVSEResult(evse, CommandResult.Error)),
                       result.SenderId,
                       result.Sender,
                       result.EventTrackingId,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region UpdateEVSEs     (EVSEs, ...)

        /// <summary>
        /// Update the given enumeration of EVSEs within the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="EVSEs">An enumeration of EVSEs.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<UpdateEVSEsResult>

            UpdateEVSEs(IEnumerable<IEVSE>  EVSEs,
                        TransmissionTypes   TransmissionType    = TransmissionTypes.Enqueue,

                        DateTime?           Timestamp           = null,
                        EventTracking_Id?   EventTrackingId     = null,
                        TimeSpan?           RequestTimeout      = null,
                        CancellationToken   CancellationToken   = default)

        {

            #region Initial checks

            if (!EVSEs.Any())
                return UpdateEVSEsResult.NoOperation(
                           EVSEs,
                           Id,
                           this,
                           EventTrackingId
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    foreach (var evse in EVSEs)
                    {
                        if (evse.ChargingStation is not null)
                        {

                            if (IncludeChargingStations is     null ||
                               (IncludeChargingStations is not null && IncludeChargingStations(evse.ChargingStation)))
                            {

                                StationsToAddQueue.Add(evse.ChargingStation);

                                FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                                   TimeSpan.FromMilliseconds(-1));

                            }

                        }
                    }

                    return UpdateEVSEsResult.Enqueued(
                               EVSEs,
                               Id,
                               this,
                               EventTrackingId
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(EVSEs.Select(evse            => evse.ChargingStation).
                                                  Where (chargingStation => chargingStation is not null).
                                                  Cast<ChargingStation>().
                                                  Distinct(),

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new UpdateEVSEsResult(
                       result.Result,
                       result.SuccessfulItems.SelectMany(res => res.ChargingStation!.EVSEs).Intersect(EVSEs).Select(evse => new UpdateEVSEResult(evse, CommandResult.Success)),
                       result.RejectedItems.  SelectMany(res => res.ChargingStation!.EVSEs).Intersect(EVSEs).Select(evse => new UpdateEVSEResult(evse, CommandResult.Error)),
                       result.SenderId,
                       result.Sender,
                       result.EventTrackingId,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion


        #region UpdateEVSEStatus     (StatusUpdates,      TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the given enumeration of EVSE status updates.
        /// </summary>
        /// <param name="EVSEStatusUpdates">An enumeration of EVSE status updates.</param>
        /// <param name="TransmissionType">Whether to send the EVSE status updates directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        async Task<PushEVSEStatusResult>

            ISendStatus.UpdateEVSEStatus(IEnumerable<EVSEStatusUpdate>  EVSEStatusUpdates,
                                         TransmissionTypes              TransmissionType,

                                         DateTime?                      Timestamp,
                                         EventTracking_Id               EventTrackingId,
                                         TimeSpan?                      RequestTimeout,
                                         CancellationToken              CancellationToken)

        {

            #region Initial checks

            if (EVSEStatusUpdates == null || !EVSEStatusUpdates.Any())
                return WWCP.PushEVSEStatusResult.NoOperation(Id, this);

            WWCP.PushEVSEStatusResult result = null;

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == WWCP.TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                        var FilteredUpdates = EVSEStatusUpdates.Where(statusupdate => IncludeEVSEIds(statusupdate.Id)).
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

            return await ConnectorsPostStatus(EVSEStatusUpdates,

                                              Timestamp,
                                              CancellationToken,
                                              EventTrackingId,
                                              RequestTimeout);

        }

        #endregion

        #endregion

        #region (Set/Add/Update/Delete) Charging station(s)...

        #region AddChargingStation         (ChargingStation, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Add the EVSE data of the given charging station to the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStation">A charging station.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<AddChargingStationResult>

            AddChargingStation(IChargingStation    ChargingStation,
                               TransmissionTypes   TransmissionType    = TransmissionTypes.Enqueue,

                               DateTime?           Timestamp           = null,
                               EventTracking_Id?   EventTrackingId     = null,
                               TimeSpan?           RequestTimeout      = null,
                               CancellationToken   CancellationToken   = default)

        {

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    if (IncludeChargingStations is     null ||
                       (IncludeChargingStations is not null && IncludeChargingStations(ChargingStation)))
                    {

                        StationsToAddQueue.Add(ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                           TimeSpan.FromMilliseconds(-1));

                    }

                    return AddChargingStationResult.Enqueued(
                               ChargingStation,
                               EventTrackingId,
                               Id,
                               this
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(new[] { ChargingStation },

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new AddChargingStationResult(
                       ChargingStation,
                       result.Result,
                       result.EventTrackingId,
                       result.SenderId,
                       result.Sender,
                       null,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region AddOrUpdateChargingStation (ChargingStation, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Set the EVSE data of the given charging station as new static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStation">A charging station.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<AddOrUpdateChargingStationResult>

            AddOrUpdateChargingStation(IChargingStation    ChargingStation,
                                       TransmissionTypes   TransmissionType    = TransmissionTypes.Enqueue,

                                       DateTime?           Timestamp           = null,
                                       EventTracking_Id?   EventTrackingId     = null,
                                       TimeSpan?           RequestTimeout      = null,
                                       CancellationToken   CancellationToken   = default)

        {

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    if (IncludeChargingStations is null ||
                       (IncludeChargingStations is not null && IncludeChargingStations(ChargingStation)))
                    {

                        StationsToAddQueue.Add(ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                           TimeSpan.FromMilliseconds(-1));

                    }

                    return AddOrUpdateChargingStationResult.Enqueued(
                               ChargingStation,
                               EventTrackingId,
                               Id,
                               this
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(new[] { ChargingStation },

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new AddOrUpdateChargingStationResult(
                       ChargingStation,
                       result.Result,
                       result.EventTrackingId,
                       result.SenderId,
                       result.Sender,
                       null,
                       AddedOrUpdated.Add,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region UpdateChargingStation      (ChargingStation,  PropertyName, NewValue, OldValue = null, DataSource = null, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Add the EVSE data of the given charging station to the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStation">A charging station to update.</param>
        /// <param name="PropertyName">The name of the charging station property to update.</param>
        /// <param name="NewValue">The new value of the charging station property to update.</param>
        /// <param name="OldValue">The optional old value of the charging station property to update.</param>
        /// <param name="DataSource">An optional data source or context for the charging station property update.</param>
        /// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<UpdateChargingStationResult>

            UpdateChargingStation(IChargingStation   ChargingStation,
                                  String             PropertyName,
                                  Object?            NewValue,
                                  Object?            OldValue            = null,
                                  Context?           DataSource          = null,
                                  TransmissionTypes  TransmissionType    = TransmissionTypes.Enqueue,

                                  DateTime?          Timestamp           = null,
                                  EventTracking_Id?  EventTrackingId     = null,
                                  TimeSpan?          RequestTimeout      = null,
                                  CancellationToken  CancellationToken   = default)

        {

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    if (IncludeChargingStations is     null ||
                       (IncludeChargingStations is not null && IncludeChargingStations(ChargingStation)))
                    {

                        StationsToAddQueue.Add(ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                           TimeSpan.FromMilliseconds(-1));

                    }

                    return UpdateChargingStationResult.Enqueued(
                               ChargingStation,
                               EventTrackingId,
                               Id,
                               this
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(new[] { ChargingStation },

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new UpdateChargingStationResult(
                       ChargingStation,
                       result.Result,
                       result.EventTrackingId,
                       result.SenderId,
                       result.Sender,
                       null,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region DeleteChargingStation      (ChargingStation, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Delete the EVSE data of the given charging station from the static EVSE data at the OIOI server.
        /// </summary>
        /// <param name="ChargingStation">A charging station.</param>
        /// <param name="TransmissionType">Whether to send the charging pool update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<DeleteChargingStationResult>

            DeleteChargingStation(IChargingStation    ChargingStation,
                                  TransmissionTypes   TransmissionType    = TransmissionTypes.Enqueue,

                                  DateTime?           Timestamp           = null,
                                  EventTracking_Id?   EventTrackingId     = null,
                                  TimeSpan?           RequestTimeout      = null,
                                  CancellationToken   CancellationToken   = default)

        {

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    if (IncludeChargingStations is     null ||
                       (IncludeChargingStations is not null && IncludeChargingStations(ChargingStation)))
                    {

                        StationsToAddQueue.Add(ChargingStation);

                        FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                           TimeSpan.FromMilliseconds(-1));

                    }

                    return DeleteChargingStationResult.Enqueued(
                               ChargingStation,
                               EventTrackingId,
                               Id,
                               this
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(new[] { ChargingStation },

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new DeleteChargingStationResult(
                       ChargingStation,
                       result.Result,
                       result.EventTrackingId,
                       result.SenderId,
                       result.Sender,
                       null,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion


        #region AddChargingStations        (ChargingStations, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Add the EVSE data of the given enumeration of charging stations to the static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingStations">An enumeration of charging stations.</param>
        /// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<AddChargingStationsResult>

            AddChargingStations(IEnumerable<IChargingStation>  ChargingStations,
                                TransmissionTypes              TransmissionType    = TransmissionTypes.Enqueue,

                                DateTime?                      Timestamp           = null,
                                EventTracking_Id?              EventTrackingId     = null,
                                TimeSpan?                      RequestTimeout      = null,
                                CancellationToken              CancellationToken   = default)

        {

            #region Initial checks

            if (!ChargingStations.Any())
                return AddChargingStationsResult.NoOperation(
                           ChargingStations,
                           Id,
                           this,
                           EventTrackingId
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    foreach (var chargingStation in ChargingStations)
                    {

                        if (IncludeChargingStations is     null ||
                           (IncludeChargingStations is not null && IncludeChargingStations(chargingStation)))
                        {

                            StationsToAddQueue.Add(chargingStation);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                               TimeSpan.FromMilliseconds(-1));

                        }

                    }

                    return AddChargingStationsResult.Enqueued(
                               ChargingStations,
                               Id,
                               this,
                               EventTrackingId
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(ChargingStations,

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new AddChargingStationsResult(
                       result.Result,
                       result.SuccessfulItems.Select(res => new AddChargingStationResult(res.ChargingStation!, CommandResult.Success)),
                       result.RejectedItems.  Select(res => new AddChargingStationResult(res.ChargingStation!, CommandResult.Error)),
                       result.SenderId,
                       result.Sender,
                       result.EventTrackingId,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region AddOrUpdateChargingStations(ChargingStations, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Set the EVSE data of the given enumeration of charging stations as new static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingStations">An enumeration of charging stations.</param>
        /// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<AddOrUpdateChargingStationsResult>

            AddOrUpdateChargingStations(IEnumerable<IChargingStation>  ChargingStations,
                                        TransmissionTypes              TransmissionType    = TransmissionTypes.Enqueue,

                                        DateTime?                      Timestamp           = null,
                                        EventTracking_Id?              EventTrackingId     = null,
                                        TimeSpan?                      RequestTimeout      = null,
                                        CancellationToken              CancellationToken   = default)

        {

            #region Initial checks

            if (!ChargingStations.Any())
                return AddOrUpdateChargingStationsResult.NoOperation(
                           ChargingStations,
                           Id,
                           this,
                           EventTrackingId
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    foreach (var chargingStation in ChargingStations)
                    {

                        if (IncludeChargingStations is     null ||
                           (IncludeChargingStations is not null && IncludeChargingStations(chargingStation)))
                        {

                            StationsToAddQueue.Add(chargingStation);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                               TimeSpan.FromMilliseconds(-1));

                        }

                    }

                    return AddOrUpdateChargingStationsResult.Enqueued(
                               ChargingStations,
                               Id,
                               this,
                               EventTrackingId
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(ChargingStations,

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new AddOrUpdateChargingStationsResult(
                       result.Result,
                       result.SuccessfulItems,
                       result.RejectedItems,
                       result.SenderId,
                       result.Sender,
                       result.EventTrackingId,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region UpdateChargingStations     (ChargingStations, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Update the EVSE data of the given enumeration of charging stations within the static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingStations">An enumeration of charging stations.</param>
        /// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<UpdateChargingStationsResult>

            UpdateChargingStations(IEnumerable<IChargingStation>  ChargingStations,
                                   TransmissionTypes              TransmissionType    = TransmissionTypes.Enqueue,

                                   DateTime?                      Timestamp           = null,
                                   EventTracking_Id?              EventTrackingId     = null,
                                   TimeSpan?                      RequestTimeout      = null,
                                   CancellationToken              CancellationToken   = default)

        {

            #region Initial checks

            if (!ChargingStations.Any())
                return UpdateChargingStationsResult.NoOperation(
                           ChargingStations,
                           Id,
                           this,
                           EventTrackingId
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    foreach (var chargingStation in ChargingStations)
                    {

                        if (IncludeChargingStations is     null ||
                           (IncludeChargingStations is not null && IncludeChargingStations(chargingStation)))
                        {

                            StationsToAddQueue.Add(chargingStation);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                               TimeSpan.FromMilliseconds(-1));

                        }

                    }

                    return UpdateChargingStationsResult.Enqueued(
                               ChargingStations,
                               Id,
                               this,
                               EventTrackingId
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(ChargingStations,

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new UpdateChargingStationsResult(
                       result.Result,
                       result.SuccessfulItems.Select(res => new UpdateChargingStationResult(res.ChargingStation!, CommandResult.Success)),
                       result.RejectedItems.  Select(res => new UpdateChargingStationResult(res.ChargingStation!, CommandResult.Error)),
                       result.SenderId,
                       result.Sender,
                       result.EventTrackingId,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

        #endregion

        #region DeleteChargingStations     (ChargingStations, TransmissionType = Enqueue, ...)

        /// <summary>
        /// Delete the EVSE data of the given enumeration of charging stations from the static EVSE data at the OICP server.
        /// </summary>
        /// <param name="ChargingStations">An enumeration of charging stations.</param>
        /// <param name="TransmissionType">Whether to send the charging station update directly or enqueue it for a while.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        public override async Task<DeleteChargingStationsResult>

            DeleteChargingStations(IEnumerable<IChargingStation>  ChargingStations,
                                   TransmissionTypes              TransmissionType    = TransmissionTypes.Enqueue,

                                   DateTime?                      Timestamp           = null,
                                   EventTracking_Id?              EventTrackingId     = null,
                                   TimeSpan?                      RequestTimeout      = null,
                                   CancellationToken              CancellationToken   = default)

        {

            #region Initial checks

            if (!ChargingStations.Any())
                return DeleteChargingStationsResult.NoOperation(
                           ChargingStations,
                           Id,
                           this,
                           EventTrackingId
                       );

            #endregion

            #region Enqueue, if requested...

            if (TransmissionType == TransmissionTypes.Enqueue)
            {

                #region Send OnEnqueueSendCDRRequest event

                //try
                //{

                //    OnEnqueueSendCDRRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                await DataAndStatusLock.WaitAsync(CancellationToken);

                try
                {

                    foreach (var chargingStation in ChargingStations)
                    {

                        if (IncludeChargingStations is     null ||
                           (IncludeChargingStations is not null && IncludeChargingStations(chargingStation)))
                        {

                            StationsToAddQueue.Add(chargingStation);

                            FlushEVSEDataAndStatusTimer.Change(FlushEVSEDataAndStatusEvery,
                                                               TimeSpan.FromMilliseconds(-1));

                        }

                    }

                    return DeleteChargingStationsResult.Enqueued(
                               ChargingStations,
                               Id,
                               this,
                               EventTrackingId
                           );

                }
                finally
                {
                    DataAndStatusLock.Release();
                }

            }

            #endregion


            var result = await StationsPost(ChargingStations,

                                            Timestamp,
                                            EventTrackingId,
                                            RequestTimeout,
                                            CancellationToken);

            return new DeleteChargingStationsResult(
                       result.Result,
                       result.SuccessfulItems.Select(res => new DeleteChargingStationResult(res.ChargingStation!, CommandResult.Success)),
                       result.RejectedItems.  Select(res => new DeleteChargingStationResult(res.ChargingStation!, CommandResult.Error)),
                       result.SenderId,
                       result.Sender,
                       result.EventTrackingId,
                       result.Description,
                       result.Warnings,
                       result.Runtime
                   );

        }

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
                Timestamp = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPORoaming.CPOClient.RequestTimeout;

            #endregion

            #region Send OnAuthorizeStartRequest event

            var StartTime = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;

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
                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnAuthorizeStartRequest));
            }

            #endregion


            DateTime         Endtime;
            TimeSpan         Runtime;
            AuthStartResult  result;

            if (DisableAuthentication)
            {

                Endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
                Runtime  = Endtime - StartTime;
                result   = AuthStartResult.AdminDown(Id,
                                                     this,
                                                     SessionId:  SessionId,
                                                     Runtime:    Runtime);

            }

            else if (!LocalAuthentication.AuthToken.HasValue ||
                     !LocalAuthentication.AuthToken.ToOIOI().HasValue)
            {

                Endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
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


                Endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
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
                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnAuthorizeStartResponse));
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
                Timestamp = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;

            if (EventTrackingId == null)
                EventTrackingId = EventTracking_Id.New;

            if (!RequestTimeout.HasValue)
                RequestTimeout = CPORoaming.CPOClient.RequestTimeout;

            #endregion

            #region Send OnAuthorizeStopRequest event

            var StartTime = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;

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
                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnAuthorizeStopRequest));
            }

            #endregion


            DateTime        Endtime;
            TimeSpan        Runtime;
            AuthStopResult  result;

            if (DisableAuthentication)
            {
                Endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
                Runtime  = Endtime - StartTime;
                result   = AuthStopResult.AdminDown(Id,
                                                    this,
                                                    SessionId:  SessionId,
                                                    Runtime:    Runtime);
            }

            else if (!LocalAuthentication.AuthToken.HasValue ||
                     !LocalAuthentication.AuthToken.ToOIOI().HasValue)
            {

                Endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
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


                Endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
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
                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnAuthorizeStopResponse));
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
                Timestamp = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;

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
                    FilteredCDRs.Add(SendCDRResult.Filtered(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                            cdr,
                                                            Warning: Warning.Create(I18NString.Create(Languages.en, "This charge detail record was filtered!"))));

            }

            #endregion

            #region Send OnSendCDRsRequest event

            var StartTime = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;

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
                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnSendCDRsRequest));
            }

            #endregion


            #region if disabled => 'AdminDown'...

            if (DisableSendChargeDetailRecords)
            {

                Endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
                Runtime  = Endtime - StartTime;
                results   = SendCDRsResult.AdminDown(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                                OnEnqueueSendCDRsRequest?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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
                                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnSendCDRsRequest));
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

                                        SendCDRsResults.Add(SendCDRResult.Enqueued(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                                                   ChargeDetailRecord));

                                    }
                                    else
                                        SendCDRsResults.Add(SendCDRResult.CouldNotConvertCDRFormat(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                                                                   ChargeDetailRecord,
                                                                                                   Warning: Warning.Create(I18NString.Create(Languages.en, "Could not parse connector identification!"))));

                                }
                                catch (Exception e)
                                {
                                    SendCDRsResults.Add(SendCDRResult.CouldNotConvertCDRFormat(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                                                               ChargeDetailRecord,
                                                                                               Warning: Warning.Create(I18NString.Create(Languages.en, e.Message))));
                                }

                            }

                            Endtime      = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
                            Runtime      = Endtime - StartTime;
                            results      = SendCDRsResult.Enqueued(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                                            result = SendCDRResult.Success(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                                           chargeDetailRecord);

                                        }

                                        else
                                            result = SendCDRResult.Error(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                                         chargeDetailRecord,
                                                                         Warning.Create(I18NString.Create(Languages.en, response.HTTPBodyAsUTF8String)));

                                    }
                                    else
                                        result = SendCDRResult.CouldNotConvertCDRFormat(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                                                        chargeDetailRecord,
                                                                                        Warning.Create(I18NString.Create(Languages.en, "Could not parse connector identification!")));

                                }
                                catch (Exception e)
                                {
                                    result = SendCDRResult.CouldNotConvertCDRFormat(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                                                    chargeDetailRecord,
                                                                                    Warning.Create(I18NString.Create(Languages.en, e.Message)));
                                }

                                SendCDRsResults.Add(result);
                                RoamingNetwork.SessionsStore.CDRForwarded(chargeDetailRecord.SessionId, result);

                            }

                            Endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
                            Runtime  = Endtime - StartTime;

                            if (SendCDRsResults.All(cdrresult => cdrresult.Result == SendCDRResultTypes.Success))
                                results = SendCDRsResult.Success(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                                 Id,
                                                                 this,
                                                                 ChargeDetailRecords,
                                                                 Runtime: Runtime);

                            else
                                results = SendCDRsResult.Error(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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

                        Endtime  = org.GraphDefined.Vanaheimr.Illias.Timestamp.Now;
                        Runtime  = Endtime - StartTime;
                        results  = SendCDRsResult.Timeout(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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
                DebugX.LogException(e, nameof(CPOAdapter) + "." + nameof(OnSendCDRsResponse));
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

                        SendOnWarnings(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                       nameof(CPOAdapter) + Id,
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

                            SendOnWarnings(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                           nameof(CPOAdapter) + Id,
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

            if (!DisableSendStatus &&
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

                        SendOnWarnings(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                       nameof(CPOAdapter) + Id,
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

            //            SendOnWarnings(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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
                                                                            org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                                            new CancellationTokenSource().Token,
                                                                            EventTracking_Id.New,
                                                                            DefaultRequestTimeout).
                                                       ConfigureAwait(false);

                if (ConnectorsPostStatusResult.Warnings.Any())
                {

                    SendOnWarnings(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                   nameof(CPOAdapter) + Id,
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

                                                            org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                            new CancellationTokenSource().Token,
                                                            EventTracking_Id.New,
                                                            DefaultRequestTimeout).
                                                ConfigureAwait(false);

                    if (response.HTTPStatusCode == HTTPStatusCode.OK &&
                        response.Content        != null &&
                        response.Content.Code   == 0)
                    {

                        result = SendCDRResult.Success(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                       chargingSession.GetInternalDataAs<ChargeDetailRecord>(OIOIMapper.WWCP_CDR),
                                                       Runtime: response.Runtime);

                    }

                    else
                        result = SendCDRResult.Error(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
                                                     chargingSession.GetInternalDataAs<ChargeDetailRecord>(OIOIMapper.WWCP_CDR),
                                                     Warning.Create(I18NString.Create(Languages.en, response.HTTPBodyAsUTF8String)),
                                                     Runtime: response.Runtime);

                }
                catch (Exception e)
                {
                    result = SendCDRResult.Error(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now,
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
        public static Boolean operator == (CPOAdapter WWCPCPOAdapter1, CPOAdapter WWCPCPOAdapter2)
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
        public static Boolean operator != (CPOAdapter WWCPCPOAdapter1, CPOAdapter WWCPCPOAdapter2)
            => !(WWCPCPOAdapter1 == WWCPCPOAdapter2);

        #endregion

        #region Operator <  (WWCPCPOAdapter1, WWCPCPOAdapter2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="WWCPCPOAdapter1">A WWCPCPOAdapter.</param>
        /// <param name="WWCPCPOAdapter2">Another WWCPCPOAdapter.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (CPOAdapter  WWCPCPOAdapter1, CPOAdapter  WWCPCPOAdapter2)
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
        public static Boolean operator <= (CPOAdapter WWCPCPOAdapter1, CPOAdapter WWCPCPOAdapter2)
            => !(WWCPCPOAdapter1 > WWCPCPOAdapter2);

        #endregion

        #region Operator >  (WWCPCPOAdapter1, WWCPCPOAdapter2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="WWCPCPOAdapter1">A WWCPCPOAdapter.</param>
        /// <param name="WWCPCPOAdapter2">Another WWCPCPOAdapter.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (CPOAdapter WWCPCPOAdapter1, CPOAdapter WWCPCPOAdapter2)
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
        public static Boolean operator >= (CPOAdapter WWCPCPOAdapter1, CPOAdapter WWCPCPOAdapter2)
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

            if (!(Object is CPOAdapter WWCPCPOAdapter))
                throw new ArgumentException("The given object is not an WWCPCPOAdapter!", nameof(Object));

            return CompareTo(WWCPCPOAdapter);

        }

        #endregion

        #region CompareTo(WWCPCPOAdapter)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="WWCPCPOAdapter">An WWCPCPOAdapter object to compare with.</param>
        public Int32 CompareTo(CPOAdapter WWCPCPOAdapter)
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

            if (!(Object is CPOAdapter WWCPCPOAdapter))
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
        public Boolean Equals(CPOAdapter WWCPCPOAdapter)
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
