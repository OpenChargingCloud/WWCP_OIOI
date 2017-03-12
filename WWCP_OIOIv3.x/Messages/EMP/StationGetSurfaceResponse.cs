/*
 * Copyright (c) 2014-2017 GraphDefined GmbH
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
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x.EMP
{

    /// <summary>
    /// An OIOI StationGetSurface response.
    /// </summary>
    public class StationGetSurfaceResponse : AResponse<StationGetSurfaceRequest,
                                                       StationGetSurfaceResponse>
    {

        #region Properties

        /// <summary>
        /// An enumeration of charging stations.
        /// </summary>
        public IEnumerable<Station>  Stations   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI StationGetSurface response.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Stations">An enumeration of charging stations.</param>
        /// <param name="CustomData">An optional read-only dictionary of customer-specific key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific key-value pairs.</param>
        public StationGetSurfaceResponse(StationGetSurfaceRequest             Request,
                                         IEnumerable<Station>                 Stations,
                                         IReadOnlyDictionary<String, Object>  CustomData    = null,
                                         Action<StationGetSurfaceResponse>    CustomMapper  = null)

            : base(Request,
                   CustomData,
                   CustomMapper)

        {

            this.Stations  = Stations;

        }

        #endregion


        #region Documentation

        // {
        //     "stations": [
        //
        //         {
        //             "id":                      1169,
        //             "name":                    "Marktparkhaus am Südwall",
        //             "latitude":                51.516123,
        //             "longitude":                6.322554,
        //             "dynamic_status_summary":  null,
        //             "owner_type":              null
        //         },
        //
        //         {
        //             "id":                      1622,
        //             "name":                    "Markt",
        //             "latitude":                51.51599,
        //             "longitude":               6.322551,
        //             "dynamic_status_summary":  null,
        //             "owner_type":              null
        //         }
        //
        //     ]
        // }

        #endregion

        #region (static) Parse   (Request, JSON,                                CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI StationGetSurface response.
        /// </summary>
        /// <param name="Request">The corresponding StationPost request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static StationGetSurfaceResponse Parse(StationGetSurfaceRequest                                  Request,
                                                      JObject                                                   JSON,
                                                      CustomMapperDelegate<StationGetSurfaceResponse, Builder>  CustomMapper  = null,
                                                      OnExceptionDelegate                                       OnException   = null)
        {

            StationGetSurfaceResponse _StationGetSurfaceResponse;

            if (TryParse(Request, JSON, out _StationGetSurfaceResponse, CustomMapper, OnException))
                return _StationGetSurfaceResponse;

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out StationGetSurfaceResponse, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI StationGetSurface response.
        /// </summary>
        /// <param name="Request">The corresponding StationPost request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="StationGetSurfaceResponse">The parsed StationPost response.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static Boolean TryParse(StationGetSurfaceRequest                                  Request,
                                       JObject                                                   JSON,
                                       out StationGetSurfaceResponse                             StationGetSurfaceResponse,
                                       CustomMapperDelegate<StationGetSurfaceResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                       OnException   = null)
        {

            try
            {

                var InnerJSON  = JSON["stations"];

                if (InnerJSON == null)
                {
                    StationGetSurfaceResponse = null;
                    return false;
                }

                StationGetSurfaceResponse = new StationGetSurfaceResponse(
                                                Request,
                                                InnerJSON.SafeSelect(station => Station.Parse(station as JObject))
                                            );

                if (CustomMapper != null)
                    StationGetSurfaceResponse = CustomMapper(JSON,
                                                       StationGetSurfaceResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, JSON, e);

                StationGetSurfaceResponse = null;
                return false;

            }

        }

        #endregion

        #region ToJSON()

        /// <summary>
        /// Return a JSON-representation of this object.
        /// </summary>
        public JObject ToJSON()

            => new JObject(
                   new JProperty("stations",
                                 new JArray(Stations.SafeSelect(station => station.ToJSON())))
               );

        #endregion


        #region Operator overloading

        #region Operator == (StationGetSurfaceResponse1, StationGetSurfaceResponse2)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="StationGetSurfaceResponse1">A response.</param>
        /// <param name="StationGetSurfaceResponse2">Another response.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (StationGetSurfaceResponse StationGetSurfaceResponse1, StationGetSurfaceResponse StationGetSurfaceResponse2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(StationGetSurfaceResponse1, StationGetSurfaceResponse2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) StationGetSurfaceResponse1 == null) || ((Object) StationGetSurfaceResponse2 == null))
                return false;

            return StationGetSurfaceResponse1.Equals(StationGetSurfaceResponse2);

        }

        #endregion

        #region Operator != (StationGetSurfaceResponse1, StationGetSurfaceResponse2)

        /// <summary>
        /// Compares two responses for inequality.
        /// </summary>
        /// <param name="StationGetSurfaceResponse1">A response.</param>
        /// <param name="StationGetSurfaceResponse2">Another response.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (StationGetSurfaceResponse StationGetSurfaceResponse1, StationGetSurfaceResponse StationGetSurfaceResponse2)
            => !(StationGetSurfaceResponse1 == StationGetSurfaceResponse2);

        #endregion

        #endregion

        #region IEquatable<StationGetSurfaceResponse> Members

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

            // Check if the given object is a response.
            var AResponse = Object as StationGetSurfaceResponse;
            if ((Object) AResponse == null)
                return false;

            return Equals(AResponse);

        }

        #endregion

        #region Equals(StationGetSurfaceResponse)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="StationGetSurfaceResponse">A response to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(StationGetSurfaceResponse StationGetSurfaceResponse)
        {

            if ((Object) StationGetSurfaceResponse == null)
                return false;

            return Stations.Equals(StationGetSurfaceResponse.Stations);

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
                return Stations.GetHashCode();
            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()
            => "StationGetSurface response: " + Stations.ULongCount() + " station(s)";

        #endregion


        #region ToBuilder()

        /// <summary>
        /// Create a builder to edit this response.
        /// </summary>
        public Builder ToBuilder()
            => new Builder(this);

        #endregion

        #region (class) Builder

        /// <summary>
        /// A StationGetSurface response builder.
        /// </summary>
        public class Builder : AResponseBuilder<StationGetSurfaceRequest,
                                                StationGetSurfaceResponse>
        {

            #region Properties

            /// <summary>
            /// An enumeration of charging stations.
            /// </summary>
            public IEnumerable<Station>  Stations   { get; set; }

            #endregion

            #region Constructor(s)

            internal Builder(StationGetSurfaceResponse Response = null)

                : base(Response?.Request,
                       Response)

            {

                if (Response != null)
                {

                    this.Request   = Response.Request;
                    this.Response  = Response;
                    this.Stations  = Response.Stations;

                    if (Response.CustomData != null)
                        foreach (var item in Response.CustomData)
                            CustomData.Add(item.Key, item.Value);

                }

            }

            #endregion

            #region ToImmutable()

            /// <summary>
            /// Return an immutable StationGetSurface response.
            /// </summary>
            public StationGetSurfaceResponse ToImmutable()

                => new StationGetSurfaceResponse(Request,
                                                 Stations,
                                                 CustomData,
                                                 CustomMapper);

            #endregion

        }

        #endregion


    }

}
