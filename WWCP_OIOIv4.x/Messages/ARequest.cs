﻿/*
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
using System.Threading;

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// An abstract generic OIOI request message.
    /// </summary>
    public abstract class ARequest<TRequest> : IRequest<TRequest>

        where TRequest : class

    {

        #region Data

        /// <summary>
        /// The default request timeout.
        /// </summary>
        public static readonly TimeSpan DefaultRequestTimeout = TimeSpan.FromSeconds(60);

        #endregion

        #region Properties

        /// <summary>
        /// The optional timestamp of the request.
        /// </summary>
        public DateTime?           Timestamp           { get; }

        /// <summary>
        /// An optional token to cancel this request.
        /// </summary>
        public CancellationToken?  CancellationToken   { get; }

        /// <summary>
        /// An optional event tracking identification for correlating this request with other events.
        /// </summary>
        public EventTracking_Id    EventTrackingId     { get; }

        /// <summary>
        /// An optional timeout for this request.
        /// </summary>
        public TimeSpan?           RequestTimeout      { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new generic OIOI request message.
        /// </summary>
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public ARequest(DateTime?           Timestamp           = null,
                        CancellationToken?  CancellationToken   = null,
                        EventTracking_Id    EventTrackingId     = null,
                        TimeSpan?           RequestTimeout      = null)
        {

            this.Timestamp          = Timestamp.        HasValue ? Timestamp            : DateTime.UtcNow;
            this.CancellationToken  = CancellationToken.HasValue ? CancellationToken    : new CancellationTokenSource().Token;
            this.EventTrackingId    = EventTrackingId ?? EventTracking_Id.New;
            this.RequestTimeout     = RequestTimeout.   HasValue ? RequestTimeout.Value : DefaultRequestTimeout;

        }

        #endregion


        #region IEquatable<ARequest> Members

        /// <summary>
        /// Compare two requests for equality.
        /// </summary>
        /// <param name="ARequest">Another abstract generic OIOI request.</param>
        public abstract Boolean Equals(TRequest ARequest);

        #endregion

    }

}
