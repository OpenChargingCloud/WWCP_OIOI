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
    /// An OIOI SessionStop result.
    /// </summary>
    public class SessionStopResponse : AResponse<SessionStopRequest,
                                                 SessionStopResponse>
    {

        #region Properties

        /// <summary>
        /// The result of the corresponding SessionStop request.
        /// </summary>
        public Boolean  Success   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI SessionStop result.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Success">The result of the corresponding SessionStop request.</param>
        /// <param name="CustomData">A read-only dictionary of custom key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific semi-structured data.</param>
        public SessionStopResponse(SessionStopRequest                    Request,
                                    Boolean                              Success,
                                    IReadOnlyDictionary<String, Object>  CustomData    = null,
                                    Action<SessionStopResponse>          CustomMapper  = null)

            : base(Request,
                   CustomData,
                   CustomMapper)

        {

            this.Success      = Success;

        }

        #endregion


        #region Documentation

        // {
        //     "session-stop": {
        //         "success": true
        //     }
        // }

        #endregion

        #region (static) Parse   (Request, JSON,                      CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI SessionStop response.
        /// </summary>
        /// <param name="Request">The corresponding SessionStop request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static SessionStopResponse Parse(SessionStopRequest                                  Request,
                                                JObject                                             JSON,
                                                CustomMapperDelegate<SessionStopResponse, Builder>  CustomMapper  = null,
                                                OnExceptionDelegate                                 OnException   = null)
        {

            SessionStopResponse _SessionStopResponse;

            if (TryParse(Request, JSON, out _SessionStopResponse, CustomMapper, OnException))
                return _SessionStopResponse;

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out Acknowledgement, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI SessionStop response.
        /// </summary>
        /// <param name="JSON">The JSON to parse.</param>
        /// <param name="SessionStopResponse">The parsed SessionStop response</param>
        public static Boolean TryParse(SessionStopRequest                                  Request,
                                       JObject                                             JSON,
                                       out SessionStopResponse                             SessionStopResponse,
                                       CustomMapperDelegate<SessionStopResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                 OnException   = null)
        {

            try
            {

                var InnerJSON  = JSON["session-stop"];

                if (InnerJSON == null)
                {
                    SessionStopResponse = null;
                    return false;
                }

                SessionStopResponse = new SessionStopResponse(
                                           Request,
                                           InnerJSON["success"].Value<Boolean>() == true
                                       );

                if (CustomMapper != null)
                    SessionStopResponse = CustomMapper(JSON,
                                                       SessionStopResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, JSON, e);

                SessionStopResponse = null;
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
                   new JProperty("session-stop", JSONObject.Create(

                       new JProperty("success",  Success)

                   ))
               );

        #endregion


        #region Operator overloading

        #region Operator == (SessionStopResponse1, SessionStopResponse2)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="SessionStopResponse1">A response.</param>
        /// <param name="SessionStopResponse2">Another response.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (SessionStopResponse SessionStopResponse1, SessionStopResponse SessionStopResponse2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(SessionStopResponse1, SessionStopResponse2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) SessionStopResponse1 == null) || ((Object) SessionStopResponse2 == null))
                return false;

            return SessionStopResponse1.Equals(SessionStopResponse2);

        }

        #endregion

        #region Operator != (SessionStopResponse1, SessionStopResponse2)

        /// <summary>
        /// Compares two responses for inequality.
        /// </summary>
        /// <param name="SessionStopResponse1">A response.</param>
        /// <param name="SessionStopResponse2">Another response.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (SessionStopResponse SessionStopResponse1, SessionStopResponse SessionStopResponse2)
            => !(SessionStopResponse1 == SessionStopResponse2);

        #endregion

        #endregion

        #region IEquatable<SessionStopResponse> Members

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

            var AResponse = Object as SessionStopResponse;
            if ((Object) AResponse == null)
                return false;

            return Equals(AResponse);

        }

        #endregion

        #region Equals(SessionStopResponse)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="SessionStopResponse">A response to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(SessionStopResponse SessionStopResponse)
        {

            if ((Object) SessionStopResponse == null)
                return false;

            return Success.Equals(SessionStopResponse.Success);

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

            => String.Concat("SessionStop response: ",
                             Success.ToString());

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
        /// A SessionStop response builder.
        /// </summary>
        public class Builder : AResponseBuilder<SessionStopRequest,
                                                SessionStopResponse>
        {

            #region Properties

            /// <summary>
            /// The result of the operation.
            /// </summary>
            public Boolean                     Success       { get; set; }

            /// <summary>
            /// Explains what the problem was, whenever 'success' was false.
            /// </summary>
            public String                      Reason        { get; set; }

            public Dictionary<String, Object>  CustomData    { get; set; }

            #endregion

            #region Constructor(s)

            public Builder(SessionStopResponse Response = null)

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
            /// Return an immutable SessionStop response.
            /// </summary>
            public SessionStopResponse ToImmutable()

                => new SessionStopResponse(Request,
                                           Success,
                                           CustomData,
                                           CustomMapper);

            #endregion

        }

        #endregion

    }

}
