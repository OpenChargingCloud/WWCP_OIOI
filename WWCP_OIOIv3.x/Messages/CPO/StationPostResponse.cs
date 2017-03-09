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
using System.Xml.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using Newtonsoft.Json.Linq;
using org.GraphDefined.Vanaheimr.Hermod;
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x.CPO
{

    /// <summary>
    /// An OIOI StationPost result.
    /// </summary>
    public class StationPostResponse : AResponse<StationPostRequest,
                                                 StationPostResponse>
    {

        #region Properties

        /// <summary>
        /// The result of the corresponding StationPost request.
        /// </summary>
        public Boolean  Success   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI StationPost result.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Success">The result of the corresponding StationPost request.</param>
        /// <param name="CustomData">A read-only dictionary of custom key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific semi-structured data.</param>
        public StationPostResponse(StationPostRequest                   Request,
                                   Boolean                              Success,
                                   IReadOnlyDictionary<String, Object>  CustomData    = null,
                                   Action<StationPostResponse>          CustomMapper  = null)

            : base(Request,
                   CustomData,
                   CustomMapper)

        {

            this.Success  = Success;

        }

        #endregion


        #region Documentation

        // {
        //     "station-post": {
        //         "success": true
        //     }
        // }

        #endregion

        #region (static) Parse   (Request, JSON,                      CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI StationPost response.
        /// </summary>
        /// <param name="Request">The corresponding StationPost request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static StationPostResponse Parse(StationPostRequest                                  Request,
                                                JObject                                             JSON,
                                                CustomMapperDelegate<StationPostResponse, Builder>  CustomMapper  = null,
                                                OnExceptionDelegate                                 OnException   = null)
        {

            StationPostResponse _StationPostResponse;

            if (TryParse(Request, JSON, out _StationPostResponse, CustomMapper, OnException))
                return _StationPostResponse;

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out Acknowledgement, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI StationPost response.
        /// </summary>
        /// <param name="JSON">The JSON to parse.</param>
        /// <param name="StationPostResponse">The parsed StationPost response</param>
        public static Boolean TryParse(StationPostRequest                                        Request,
                                       JObject                                                   JSON,
                                       out StationPostResponse                                   StationPostResponse,
                                       CustomMapperDelegate<StationPostResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                       OnException   = null)
        {

            try
            {

                var InnerJSON  = JSON["session"];

                if (InnerJSON == null)
                {
                    StationPostResponse = null;
                    return false;
                }

                StationPostResponse = new StationPostResponse(
                                          Request,
                                          InnerJSON["success"].Value<Boolean>() == true
                                      );

                if (CustomMapper != null)
                    StationPostResponse = CustomMapper(JSON,
                                                       StationPostResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, JSON, e);

                StationPostResponse = null;
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
                   new JProperty("session", JSONObject.Create(

                       new JProperty("success",  Success)

                   ))
               );

        #endregion


        #region Operator overloading

        #region Operator == (StationPostResponse1, StationPostResponse2)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="StationPostResponse1">A response.</param>
        /// <param name="StationPostResponse2">Another response.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (StationPostResponse StationPostResponse1, StationPostResponse StationPostResponse2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(StationPostResponse1, StationPostResponse2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) StationPostResponse1 == null) || ((Object) StationPostResponse2 == null))
                return false;

            return StationPostResponse1.Equals(StationPostResponse2);

        }

        #endregion

        #region Operator != (StationPostResponse1, StationPostResponse2)

        /// <summary>
        /// Compares two responses for inequality.
        /// </summary>
        /// <param name="StationPostResponse1">A response.</param>
        /// <param name="StationPostResponse2">Another response.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (StationPostResponse StationPostResponse1, StationPostResponse StationPostResponse2)
            => !(StationPostResponse1 == StationPostResponse2);

        #endregion

        #endregion

        #region IEquatable<StationPostResponse> Members

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
            var AResponse = Object as StationPostResponse;
            if ((Object) AResponse == null)
                return false;

            return Equals(AResponse);

        }

        #endregion

        #region Equals(StationPostResponse)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="StationPostResponse">A response to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(StationPostResponse StationPostResponse)
        {

            if ((Object) StationPostResponse == null)
                return false;

            return Success.Equals(StationPostResponse.Success);

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
                return Success.GetHashCode();
            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()
            => "StationPost response: " + Success.ToString();

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
        /// A StationPost response builder.
        /// </summary>
        public class Builder : AResponseBuilder<StationPostRequest,
                                                StationPostResponse>
        {

            #region Properties

            /// <summary>
            /// The result of the operation.
            /// </summary>
            public Boolean                     Success      { get; set; }

            /// <summary>
            /// Explains what the problem was, whenever 'success' was false.
            /// </summary>
            public String                      Reason       { get; set; }

            public Dictionary<String, Object>  CustomData   { get; set; }

            #endregion

            #region Constructor(s)

            public Builder(StationPostResponse Response = null)

                : base(Response.Request,
                       Response)

            {

                if (Response != null)
                {

                    this.Success     = Response.Success;
                    this.CustomData  = new Dictionary<String, Object>();

                    if (Response.CustomData != null)
                        foreach (var item in Response.CustomData)
                            CustomData.Add(item.Key, item.Value);

                }

            }

            #endregion

            #region ToImmutable()

            /// <summary>
            /// Return an immutable StationPost response.
            /// </summary>
            public StationPostResponse ToImmutable()

                => new StationPostResponse(Request,
                                           Success,
                                           CustomData,
                                           CustomMapper);

            #endregion

        }

        #endregion

    }

}
