/*
 * Copyright (c) 2016 GraphDefined GmbH
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
using System.Threading.Tasks;

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    #region OnSessionStartRequest/-Response

    /// <summary>
    /// A delegate called whenever a session start request was received.
    /// </summary>
    public delegate Task OnSessionStartRequestDelegate (DateTime                                LogTimestamp,
                                                        DateTime                                RequestTimestamp,
                                                        CPOServer                               Sender,
                                                        EventTracking_Id                        EventTrackingId,
                                                        User                                    User,
                                                        EVSE_Id                                 ConnectorId,
                                                        PaymentReference                        PaymentReference);

    /// <summary>
    /// A delegate called whenever a response to a session start request was sent.
    /// </summary>
    public delegate Task OnSessionStartResponseDelegate(DateTime                                LogTimestamp,
                                                        DateTime                                RequestTimestamp,
                                                        CPOServer                               Sender,
                                                        EventTracking_Id                        EventTrackingId,
                                                        User                                    User,
                                                        EVSE_Id                                 ConnectorId,
                                                        PaymentReference                        PaymentReference,
                                                        Result                                  Result,
                                                        TimeSpan                                Duration);

    #endregion

    #region OnSessionStopRequest/-Response

    /// <summary>
    /// A delegate called whenever a session start request was received.
    /// </summary>
    public delegate Task OnSessionStopRequestDelegate (DateTime                                LogTimestamp,
                                                       DateTime                                RequestTimestamp,
                                                       CPOServer                               Sender,
                                                       EventTracking_Id                        EventTrackingId,
                                                       User                                    User,
                                                       EVSE_Id                                 ConnectorId,
                                                       ChargingSession_Id                      SessionId);

    /// <summary>
    /// A delegate called whenever a response to a session start request was sent.
    /// </summary>
    public delegate Task OnSessionStopResponseDelegate(DateTime                                LogTimestamp,
                                                       DateTime                                RequestTimestamp,
                                                       CPOServer                               Sender,
                                                       EventTracking_Id                        EventTrackingId,
                                                       User                                    User,
                                                       EVSE_Id                                 ConnectorId,
                                                       ChargingSession_Id                      SessionId,
                                                       Result                                  Result,
                                                       TimeSpan                                Duration);

    #endregion

}
