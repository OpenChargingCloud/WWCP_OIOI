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

namespace org.GraphDefined.WWCP.OIOIv3_x.CPO
{

    /// <summary>
    /// An OIOI SessionPost response.
    /// </summary>
    public class SessionPostResponse : AResponse<SessionPostRequest,
                                                 SessionPostResponse>
    {

        #region Properties

        /// <summary>
        /// The response of the corresponding SessionPost request.
        /// </summary>
        public Boolean  Success   { get; }

        /// <summary>
        /// Explains what the problem was, whenever 'success' was false.
        /// </summary>
        public String   Reason    { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI SessionPost response.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Success">The response of the corresponding SessionPost request.</param>
        /// <param name="Reason">Explains what the problem was, whenever 'success' was false.</param>
        /// <param name="CustomData">An optional read-only dictionary of customer-specific key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific key-value pairs.</param>
        public SessionPostResponse(SessionPostRequest                   Request,
                                   Boolean                              Success,
                                   String                               Reason        = null,
                                   IReadOnlyDictionary<String, Object>  CustomData    = null,
                                   Action<SessionPostResponse>          CustomMapper  = null)

            : base(Request,
                   CustomData,
                   CustomMapper)

        {

            this.Success  = Success;
            this.Reason   = Reason;

        }

        #endregion


        #region Documentation

        // {
        //     "session": {
        //         "success": true,
        //         "reason":  null
        //     }
        // }

        #endregion

        #region (static) Parse   (Request, JSON,                          CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI SessionPost response.
        /// </summary>
        /// <param name="Request">The corresponding StationPost request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static SessionPostResponse Parse(SessionPostRequest                                  Request,
                                                JObject                                             JSON,
                                                CustomMapperDelegate<SessionPostResponse, Builder>  CustomMapper  = null,
                                                OnExceptionDelegate                                 OnException   = null)
        {

            SessionPostResponse _SessionPostResponse;

            if (TryParse(Request, JSON, out _SessionPostResponse, CustomMapper, OnException))
                return _SessionPostResponse;

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out SessionPostResponse, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI SessionPost response.
        /// </summary>
        /// <param name="Request">The corresponding StationPost request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="SessionPostResponse">The parsed StationPost response.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static Boolean TryParse(SessionPostRequest                                  Request,
                                       JObject                                             JSON,
                                       out SessionPostResponse                             SessionPostResponse,
                                       CustomMapperDelegate<SessionPostResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                 OnException   = null)
        {

            try
            {

                var InnerJSON  = JSON["session"];

                if (InnerJSON == null)
                {
                    SessionPostResponse = null;
                    return false;
                }

                SessionPostResponse = new SessionPostResponse(
                                          Request,
                                          InnerJSON["success"].Value<Boolean>() == true,
                                          InnerJSON["reason" ].Value<String>()
                                      );

                if (CustomMapper != null)
                    SessionPostResponse = CustomMapper(JSON,
                                                       SessionPostResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, JSON, e);

                SessionPostResponse = null;
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

                       new JProperty("success", Success),

                       Reason.IsNotNullOrEmpty()
                           ? new JProperty("reason", Reason)
                           : null

                   ))
               );

        #endregion


        #region Operator overloading

        #region Operator == (SessionPostResponse1, SessionPostResponse2)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="SessionPostResponse1">A response.</param>
        /// <param name="SessionPostResponse2">Another response.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (SessionPostResponse SessionPostResponse1, SessionPostResponse SessionPostResponse2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(SessionPostResponse1, SessionPostResponse2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) SessionPostResponse1 == null) || ((Object) SessionPostResponse2 == null))
                return false;

            return SessionPostResponse1.Equals(SessionPostResponse2);

        }

        #endregion

        #region Operator != (SessionPostResponse1, SessionPostResponse2)

        /// <summary>
        /// Compares two responses for inequality.
        /// </summary>
        /// <param name="SessionPostResponse1">A response.</param>
        /// <param name="SessionPostResponse2">Another response.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (SessionPostResponse SessionPostResponse1, SessionPostResponse SessionPostResponse2)
            => !(SessionPostResponse1 == SessionPostResponse2);

        #endregion

        #endregion

        #region IEquatable<SessionPostResponse> Members

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
            var AResponse = Object as SessionPostResponse;
            if ((Object) AResponse == null)
                return false;

            return Equals(AResponse);

        }

        #endregion

        #region Equals(SessionPostResponse)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="SessionPostResponse">A response to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(SessionPostResponse SessionPostResponse)
        {

            if ((Object) SessionPostResponse == null)
                return false;

            return Success.Equals(SessionPostResponse.Success);

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
            => "SessionPost response: " + Success.ToString();

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
        /// A SessionPost response builder.
        /// </summary>
        public class Builder : AResponseBuilder<SessionPostRequest,
                                                SessionPostResponse>
        {

            #region Properties

            /// <summary>
            /// The response of the operation.
            /// </summary>
            public Boolean  Success   { get; set; }

            /// <summary>
            /// Explains what the problem was, whenever 'success' was false.
            /// </summary>
            public String   Reason    { get; set; }

            #endregion

            #region Constructor(s)

            internal Builder(SessionPostResponse Response = null)

                : base(Response?.Request,
                       Response)

            {

                if (Response != null)
                {

                    this.Request       = Response.Request;
                    this.Response      = Response;
                    this.Success       = Response.Success;

                    if (Response.CustomData != null)
                        foreach (var item in Response.CustomData)
                            CustomData.Add(item.Key, item.Value);

                }

            }

            #endregion

            #region ToImmutable()

            /// <summary>
            /// Return an immutable SessionPost response.
            /// </summary>
            public SessionPostResponse ToImmutable()

                => new SessionPostResponse(Request,
                                           Success,
                                           Reason,
                                           CustomData,
                                           CustomMapper);

            #endregion

        }

        #endregion

    }

}
