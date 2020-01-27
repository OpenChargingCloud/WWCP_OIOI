/*
 * Copyright (c) 2014-2020 GraphDefined GmbH
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
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.EMP
{

    /// <summary>
    /// An OIOI Station Get Surface request.
    /// </summary>
    public class StationGetSurfaceRequest : ARequest<StationGetSurfaceRequest>
    {

        #region Properties

        /// <summary>
        /// The minimum latitude of the area you are querying.
        /// </summary>
        [Mandatory]
        public Single                       MinLat                  { get; }

        /// <summary>
        /// The maximum latitude of the area you are querying.
        /// </summary>
        [Mandatory]
        public Single                       MaxLat                  { get; }

        /// <summary>
        /// The minimum longitude of the area you are querying.
        /// </summary>
        [Mandatory]
        public Single                       MinLong                 { get; }

        /// <summary>
        /// The maximum longitude of the area you are querying.
        /// </summary>
        [Mandatory]
        public Single                       MaxLong                 { get; }

        /// <summary>
        /// Filter the result and include the following connector types.
        /// </summary>
        public IEnumerable<ConnectorTypes>  IncludeConnectorTypes   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create an OIOI Station Get Surface JSON/HTTP request.
        /// </summary>
        /// <param name="MinLat">The minimum latitude of the area you are querying.</param>
        /// <param name="MaxLat">The maximum latitude of the area you are querying.</param>
        /// <param name="MinLong">The minimum longitude of the area you are querying.</param>
        /// <param name="MaxLong">The maximum longitude of the area you are querying.</param>
        /// 
        /// <param name="IncludeConnectorTypes">Filter the result and include the following connector types.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public StationGetSurfaceRequest(Single                       MinLat,
                                        Single                       MaxLat,
                                        Single                       MinLong,
                                        Single                       MaxLong,
                                        IEnumerable<ConnectorTypes>  IncludeConnectorTypes  = null,

                                        DateTime?                    Timestamp              = null,
                                        CancellationToken?           CancellationToken      = null,
                                        EventTracking_Id             EventTrackingId        = null,
                                        TimeSpan?                    RequestTimeout         = null)

            : base(Timestamp,
                   CancellationToken,
                   EventTrackingId,
                   RequestTimeout)

        {

            this.MinLat                 = MinLat;
            this.MaxLat                 = MaxLat;
            this.MinLong                = MinLong;
            this.MaxLong                = MaxLong;

            this.IncludeConnectorTypes  = IncludeConnectorTypes;

        }

        #endregion


        #region Documentation

        // {
        //     "station-get-surface": {
        //
        //         "min-lat":  0,
        //         "max-lat":  45,
        //         "min-long": 30,
        //         "max-long": 40,
        //
        //         "filters": {
        //
        //             "excludes": [
        //                 11131
        //             ],
        //
        //             "company-types": [
        //                 "hotel"
        //             ],
        //
        //             "connector-types": [
        //                 "Type2"
        //             ],
        //
        //             "connector-speeds-greater":   3,
        //             "connector-speeds-less":    100,
        //
        //             "operator-ids": [
        //                 122,
        //                 32
        //             ],
        //
        //             "payable": [
        //                 "app",
        //                 "rfid"
        //             ]
        //
        //         }
        //     }
        // }

        #endregion

        #region (static) Parse(StationGetSurfaceRequestJSON, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI Station Get Surface request.
        /// </summary>
        /// <param name="StationGetSurfaceRequestJSON">The JSON to parse.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static StationGetSurfaceRequest Parse(JObject              StationGetSurfaceRequestJSON,
                                                     OnExceptionDelegate  OnException = null)
        {

            StationGetSurfaceRequest _StationGetSurfaceRequest;

            if (TryParse(StationGetSurfaceRequestJSON, out _StationGetSurfaceRequest, OnException))
                return _StationGetSurfaceRequest;

            return null;

        }

        #endregion

        #region (static) Parse(StationGetSurfaceRequestText, OnException = null)

        /// <summary>
        /// Parse the given text representation of an OIOI Station Get Surface request.
        /// </summary>
        /// <param name="StationGetSurfaceRequestText">The text to parse.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static StationGetSurfaceRequest Parse(String               StationGetSurfaceRequestText,
                                                     OnExceptionDelegate  OnException = null)
        {

            StationGetSurfaceRequest _StationGetSurfaceRequest;

            if (TryParse(StationGetSurfaceRequestText, out _StationGetSurfaceRequest, OnException))
                return _StationGetSurfaceRequest;

            return null;

        }

        #endregion

        #region (static) TryParse(StationGetSurfaceRequestJSON, out StationGetSurfaceRequest, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI Station Get Surface request.
        /// </summary>
        /// <param name="StationGetSurfaceRequestJSON">The JSON to parse.</param>
        /// <param name="StationGetSurfaceRequest">The parsed Station Get Surface request.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject                       StationGetSurfaceRequestJSON,
                                       out StationGetSurfaceRequest  StationGetSurfaceRequest,
                                       OnExceptionDelegate           OnException  = null)
        {

            try
            {

                var StationGetSurface  = StationGetSurfaceRequestJSON["station-get-surface"];
                var Filters            = StationGetSurface           ["filters"];

                StationGetSurfaceRequest = new StationGetSurfaceRequest(

                                               StationGetSurface["min-lat" ].Value<Single>(),
                                               StationGetSurface["max-lat" ].Value<Single>(),
                                               StationGetSurface["min-long"].Value<Single>(),
                                               StationGetSurface["max-long"].Value<Single>(),

                                               Filters != null && Filters["connector-types"] != null && Filters["connector-types"] is JArray
                                                   ? (Filters["connector-types"] as JArray).
                                                          SafeSelect(token => token.Value<String>().AsConnectorType()).
                                                          Where     (type  => type != ConnectorTypes.UNKNOWN)
                                                   : null

                                           );

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, StationGetSurfaceRequestJSON, e);

                StationGetSurfaceRequest = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(StationGetSurfaceRequestText, out StationGetSurfaceRequest, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI Station Get Surface request.
        /// </summary>
        /// <param name="StationGetSurfaceRequestText">The text to parse.</param>
        /// <param name="StationGetSurfaceRequest">The parsed Station Get Surface request.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String                        StationGetSurfaceRequestText,
                                       out StationGetSurfaceRequest  StationGetSurfaceRequest,
                                       OnExceptionDelegate           OnException  = null)
        {

            try
            {

                if (TryParse(JObject.Parse(StationGetSurfaceRequestText),
                             out StationGetSurfaceRequest,
                             OnException))

                    return true;

            }
            catch (Exception e)
            {
                OnException?.Invoke(DateTime.UtcNow, StationGetSurfaceRequestText, e);
            }

            StationGetSurfaceRequest = null;
            return false;

        }

        #endregion

        #region ToJSON()

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        public JObject ToJSON()

            => new JObject(new JObject(
                               new JProperty("station-get-surface", JSONObject.Create(

                                   new JProperty("min-lat",  MinLat),
                                   new JProperty("max-lat",  MaxLat),
                                   new JProperty("min-long", MinLong),
                                   new JProperty("max-long", MinLong),

                                   IncludeConnectorTypes != null && IncludeConnectorTypes.Any()

                                       ? new JProperty("filters", JSONObject.Create(

                                             IncludeConnectorTypes != null && IncludeConnectorTypes.Any()
                                                 ? new JProperty("connector-types",
                                                                 new JArray(IncludeConnectorTypes.SafeSelect(type => type.AsText())))
                                                 : null

                                        ))

                                       : null

                               ))
                           ));

        #endregion


        #region Operator overloading

        #region Operator == (StationGetSurface1, StationGetSurface2)

        /// <summary>
        /// Compares two session post requests for equality.
        /// </summary>
        /// <param name="StationGetSurface1">An session post request.</param>
        /// <param name="StationGetSurface2">Another session post request.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (StationGetSurfaceRequest StationGetSurface1, StationGetSurfaceRequest StationGetSurface2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(StationGetSurface1, StationGetSurface2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) StationGetSurface1 == null) || ((Object) StationGetSurface2 == null))
                return false;

            return StationGetSurface1.Equals(StationGetSurface2);

        }

        #endregion

        #region Operator != (StationGetSurface1, StationGetSurface2)

        /// <summary>
        /// Compares two session post requests for inequality.
        /// </summary>
        /// <param name="StationGetSurface1">An session post request.</param>
        /// <param name="StationGetSurface2">Another session post request.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (StationGetSurfaceRequest StationGetSurface1, StationGetSurfaceRequest StationGetSurface2)
            => !(StationGetSurface1 == StationGetSurface2);

        #endregion

        #endregion

        #region IEquatable<StationGetSurfaceRequest> Members

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

            var StationGetSurface = Object as StationGetSurfaceRequest;
            if ((Object) StationGetSurface == null)
                return false;

            return Equals(StationGetSurface);

        }

        #endregion

        #region Equals(StationGetSurfaceRequest)

        /// <summary>
        /// Compares two Station Get Surface requests for equality.
        /// </summary>
        /// <param name="StationGetSurfaceRequest">A Station Get Surface request to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(StationGetSurfaceRequest StationGetSurfaceRequest)
        {

            if ((Object) StationGetSurfaceRequest == null)
                return false;

            return MinLat. Equals(StationGetSurfaceRequest.MinLat)  &&
                   MaxLat. Equals(StationGetSurfaceRequest.MaxLat)  &&
                   MinLong.Equals(StationGetSurfaceRequest.MinLong) &&
                   MaxLong.Equals(StationGetSurfaceRequest.MaxLong);

        }

        #endregion

        #endregion

        #region GetHashCode()

        /// <summary>
        /// Return the HashCode of this object.
        /// </summary>
        /// <returns>The HashCode of this object.</returns>
        public override Int32 GetHashCode()
        {
            unchecked
            {

                return MinLat. GetHashCode() * 11 ^
                       MaxLat. GetHashCode() *  7 ^
                       MinLong.GetHashCode() *  5 ^
                       MaxLong.GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat("Station Get Surface ",
                             "[", MinLat, "/", MinLong,
                               " -> ",
                                  MaxLat, "/", MaxLong, "]");

        #endregion

    }

}
