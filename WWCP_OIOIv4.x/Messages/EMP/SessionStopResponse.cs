/*
 * Copyright (c) 2014-2018 GraphDefined GmbH
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

namespace org.GraphDefined.WWCP.OIOIv4_x.EMP
{

#pragma warning disable CS0659
#pragma warning disable CS0661

    /// <summary>
    /// An OIOI SessionStop result.
    /// </summary>
    public class SessionStopResponse : AResponse<SessionStopRequest,
                                                 SessionStopResponse>
    {

        #region Constructor(s)

        /// <summary>
        /// Create a new SessionStop result.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Code">The response code of the corresponding SessionStop request.</param>
        /// <param name="Message">The response message of the corresponding SessionStop request.</param>
        /// <param name="CustomData">A read-only dictionary of custom key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific semi-structured data.</param>
        public SessionStopResponse(SessionStopRequest                   Request,
                                   ResponseCodes                        Code,
                                   String                               Message,
                                   IReadOnlyDictionary<String, Object>  CustomData    = null,
                                   Action<SessionStopResponse>          CustomMapper  = null)

            : base(Request,
                   Code,
                   Message,
                   CustomData,
                   CustomMapper)

        { }

        #endregion


        #region Documentation

        // {
        //     "result": {
        //         "code":    0,
        //         "message": "Success."
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

            if (TryParse(Request,
                         JSON,
                         out SessionStopResponse _SessionStopResponse,
                         CustomMapper,
                         OnException))
            {
                return _SessionStopResponse;
            }

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out Acknowledgement, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of a SessionStop response.
        /// </summary>
        /// <param name="Request">The corresponding SessionStop request.</param>
        /// <param name="JSON">The JSON to parse.</param>
        /// <param name="SessionStopResponse">The parsed SessionStop response</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static Boolean TryParse(SessionStopRequest                                  Request,
                                       JObject                                             JSON,
                                       out SessionStopResponse                             SessionStopResponse,
                                       CustomMapperDelegate<SessionStopResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                 OnException   = null)
        {

            try
            {

                var ResultJSON  = JSON["result"];

                if (ResultJSON == null)
                {
                    SessionStopResponse = null;
                    return false;
                }

                SessionStopResponse = new SessionStopResponse(
                                          Request,
                                          (ResponseCodes) ResultJSON["code"].Value<Int32>(),
                                          ResultJSON["message"].Value<String>()
                                      );

                if (CustomMapper != null)
                    SessionStopResponse = CustomMapper(JSON,
                                                       SessionStopResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, JSON, e);

                SessionStopResponse = null;
                return false;

            }

        }

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

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()
            => String.Concat("SessionStop response: ", Code.ToString(), " / ", Message);

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

            #region Constructor(s)

            public Builder(SessionStopResponse Response = null)

                : base(Response?.Request,
                       Response)

            { }

            #endregion

            #region (implicit) "ToImmutable()"

            /// <summary>
            /// Return an immutable SessionStop response.
            /// </summary>
            /// <param name="Builder">A SessionStop response builder.</param>
            public static implicit operator SessionStopResponse(Builder Builder)

                => new SessionStopResponse(Builder.Request,
                                           Builder.Code,
                                           Builder.Message,
                                           Builder.CustomData,
                                           Builder.CustomMapper);

            #endregion

        }

        #endregion

    }

#pragma warning restore CS0661
#pragma warning restore CS0659

}
