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
using System.Linq;
using System.Threading.Tasks;

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

    #region OnStationPostRequest/-Response

    /// <summary>
    /// A delegate called whenever a charging station will be send upstream.
    /// </summary>
    public delegate Task OnStationPostRequestDelegate (DateTime                                LogTimestamp,
                                                       DateTime                                RequestTimestamp,
                                                       CPOClient                               Sender,
                                                       String                                  SenderId,
                                                       EventTracking_Id                        EventTrackingId,
                                                       Station                                 Station,
                                                       Partner_Id                              PartnerId,
                                                       TimeSpan?                               RequestTimeout);

    /// <summary>
    /// A delegate called whenever a charging station had been sent upstream.
    /// </summary>
    public delegate Task OnStationPostResponseDelegate(DateTime                                LogTimestamp,
                                                       DateTime                                RequestTimestamp,
                                                       CPOClient                               Sender,
                                                       String                                  SenderId,
                                                       EventTracking_Id                        EventTrackingId,
                                                       Station                                 Station,
                                                       Partner_Id                              PartnerId,
                                                       TimeSpan?                               RequestTimeout,
                                                       StationPostResponse                     Result,
                                                       TimeSpan                                Duration);

    #endregion

    #region OnConnectorPostStatusRequest/-Response

    /// <summary>
    /// A delegate called whenever a charging connector status will be send upstream.
    /// </summary>
    public delegate Task OnConnectorPostStatusRequestDelegate (DateTime                                      LogTimestamp,
                                                               DateTime                                      RequestTimestamp,
                                                               CPOClient                                     Sender,
                                                               String                                        SenderId,
                                                               EventTracking_Id                              EventTrackingId,
                                                               Connector_Id                                  Id,
                                                               ConnectorStatusTypes                          Status,
                                                               Partner_Id                                    PartnerId,
                                                               TimeSpan?                                     RequestTimeout);

    /// <summary>
    /// A delegate called whenever a charging connector status had been sent upstream.
    /// </summary>
    public delegate Task OnConnectorPostStatusResponseDelegate(DateTime                                      LogTimestamp,
                                                               DateTime                                      RequestTimestamp,
                                                               CPOClient                                     Sender,
                                                               String                                        SenderId,
                                                               EventTracking_Id                              EventTrackingId,
                                                               Connector_Id                                  Id,
                                                               ConnectorStatusTypes                          Status,
                                                               Partner_Id                                    PartnerId,
                                                               TimeSpan?                                     RequestTimeout,
                                                               ConnectorPostStatusResponse                   Result,
                                                               TimeSpan                                      Duration);

    #endregion

    #region OnRFIDVerifyRequest/-Response

    /// <summary>
    /// A delegate called whenever a RFID identification verification will be send upstream.
    /// </summary>
    public delegate Task OnRFIDVerifyRequestDelegate (DateTime                                LogTimestamp,
                                                      DateTime                                RequestTimestamp,
                                                      CPOClient                               Sender,
                                                      String                                  SenderId,
                                                      EventTracking_Id                        EventTrackingId,
                                                      RFID_Id                                 RFIDId,
                                                      TimeSpan?                               RequestTimeout);

    /// <summary>
    /// A delegate called whenever a RFID identification verification had been sent upstream.
    /// </summary>
    public delegate Task OnRFIDVerifyResponseDelegate(DateTime                                LogTimestamp,
                                                      DateTime                                RequestTimestamp,
                                                      CPOClient                               Sender,
                                                      String                                  SenderId,
                                                      EventTracking_Id                        EventTrackingId,
                                                      RFID_Id                                 RFIDId,
                                                      TimeSpan?                               RequestTimeout,
                                                      RFIDVerifyResponse                      Result,
                                                      TimeSpan                                Duration);

    #endregion

    #region OnSessionPostRequest/-Response

    /// <summary>
    /// A delegate called whenever a charging session will be send upstream.
    /// </summary>
    public delegate Task OnSessionPostRequestDelegate (DateTime                                LogTimestamp,
                                                       DateTime                                RequestTimestamp,
                                                       CPOClient                               Sender,
                                                       String                                  SenderId,
                                                       EventTracking_Id                        EventTrackingId,
                                                       Session                                 Session,
                                                       TimeSpan?                               RequestTimeout);

    /// <summary>
    /// A delegate called whenever a charging session had been sent upstream.
    /// </summary>
    public delegate Task OnSessionPostResponseDelegate(DateTime                                LogTimestamp,
                                                       DateTime                                RequestTimestamp,
                                                       CPOClient                               Sender,
                                                       String                                  SenderId,
                                                       EventTracking_Id                        EventTrackingId,
                                                       Session                                 Session,
                                                       TimeSpan?                               RequestTimeout,
                                                       SessionPostResponse                     Result,
                                                       TimeSpan                                Duration);

    #endregion

}
