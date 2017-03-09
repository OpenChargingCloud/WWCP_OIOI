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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x.CPO
{

    /// <summary>
    /// Extention methods for the OIOI CPO client interface.
    /// </summary>
    public static class ICPOClientExtentions
    {

        #region StationPost(Station, PartnerIdentification, ...)

        /// <summary>
        /// Upload the given charging station.
        /// </summary>
        /// <param name="Station">A charging station.</param>
        /// <param name="PartnerIdentification">The partner identifier of the partner that shall be associated with this station.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public static async Task<HTTPResponse<StationPostResponse>>

            StationPost(this ICPOClient     ICPOClient,
                        Station             Station,
                        Partner_Id          PartnerIdentification,

                        DateTime?           Timestamp           = null,
                        CancellationToken?  CancellationToken   = null,
                        EventTracking_Id    EventTrackingId     = null,
                        TimeSpan?           RequestTimeout      = null)


                => await ICPOClient.StationPost(new StationPostRequest(Station,
                                                                       PartnerIdentification,

                                                                       Timestamp,
                                                                       CancellationToken,
                                                                       EventTrackingId,
                                                                       RequestTimeout.HasValue ? RequestTimeout.Value : ICPOClient.RequestTimeout));

        #endregion

        #region ConnectorPostStatus(ConnectorStatus, PartnerId, ...)

        /// <summary>
        /// Update the status of a charging connector on the OIOI server.
        /// </summary>
        /// <param name="ConnectorStatus">A connector status.</param>
        /// <param name="PartnerIdentification">The partner identifier of the partner that owns the connector.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public static async Task<HTTPResponse<ConnectorPostStatusResponse>>

            ConnectorPostStatus(this ICPOClient      ICPOClient,
                                ConnectorStatus      ConnectorStatus,
                                Partner_Id           PartnerIdentification,

                                DateTime?            Timestamp              = null,
                                CancellationToken?   CancellationToken      = null,
                                EventTracking_Id     EventTrackingId        = null,
                                TimeSpan?            RequestTimeout         = null)

            => await ICPOClient.ConnectorPostStatus(new ConnectorPostStatusRequest(ConnectorStatus,
                                                                                   PartnerIdentification,

                                                                                   Timestamp,
                                                                                   CancellationToken,
                                                                                   EventTrackingId,
                                                                                   RequestTimeout.HasValue ? RequestTimeout.Value : ICPOClient.RequestTimeout));

        #endregion

        #region RFIDVerify(RFIDId, ...)

        /// <summary>
        /// Verifying a RFID identification via the OIOI server.
        /// </summary>
        /// <param name="RFIDId">A RFID identification.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public static async Task<HTTPResponse<RFIDVerifyResponse>>

            RFIDVerify(this ICPOClient      ICPOClient,
                       RFID_Id              RFIDId,

                       DateTime?            Timestamp           = null,
                       CancellationToken?   CancellationToken   = null,
                       EventTracking_Id     EventTrackingId     = null,
                       TimeSpan?            RequestTimeout      = null)


            => await ICPOClient.RFIDVerify(new RFIDVerifyRequest(RFIDId,

                                                                 Timestamp,
                                                                 CancellationToken,
                                                                 EventTrackingId,
                                                                 RequestTimeout.HasValue ? RequestTimeout.Value : ICPOClient.RequestTimeout));

        #endregion

        #region RFIDVerify(RFIDId, ...)

        /// <summary>
        /// Upload the given charging session onto the OIOI server.
        /// </summary>
        /// <param name="Session">A charging session.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public static async Task<HTTPResponse<SessionPostResponse>>

            SessionPost(this ICPOClient      ICPOClient,
                        Session              Session,

                        DateTime?            Timestamp           = null,
                        CancellationToken?   CancellationToken   = null,
                        EventTracking_Id     EventTrackingId     = null,
                        TimeSpan?            RequestTimeout      = null)


            => await ICPOClient.SessionPost(new SessionPostRequest(Session,

                                                                   Timestamp,
                                                                   CancellationToken,
                                                                   EventTrackingId,
                                                                   RequestTimeout.HasValue ? RequestTimeout.Value : ICPOClient.RequestTimeout));

        #endregion

    }

}
