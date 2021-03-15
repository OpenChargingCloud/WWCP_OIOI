/*
 * Copyright (c) 2014-2021 GraphDefined GmbH
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

    /// <summary>
    /// An OIOI SessionStart result.
    /// </summary>
    public class SessionStartResponse : AResponse<SessionStartRequest,
                                                  SessionStartResponse>
    {

        #region Properties

        /// <summary>
        /// The optional unique identification of the charging session.
        /// </summary>
        public Session_Id?  SessionId     { get; }

        /// <summary>
        /// The optional indication whether the session can be stopped via "session-stop" API call.
        /// </summary>
        public Boolean?     IsStoppable   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new SessionStart result.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Code">The response code of the corresponding SessionStart request.</param>
        /// <param name="Message">The response message of the corresponding SessionStart request.</param>
        /// <param name="SessionId">An optional unique identification of the charging session.</param>
        /// <param name="IsStoppable">An optional indication whether the session can be stopped via "session-stop" API call.</param>
        /// <param name="CustomData">A read-only dictionary of custom key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific semi-structured data.</param>
        public SessionStartResponse(SessionStartRequest                  Request,
                                    ResponseCodes                        Code,
                                    String                               Message,
                                    Session_Id?                          SessionId     = null,
                                    Boolean?                             IsStoppable   = null,
                                    IReadOnlyDictionary<String, Object>  CustomData    = null,
                                    Action<SessionStartResponse>         CustomMapper  = null)

            : base(Request,
                   Code,
                   Message,
                   CustomData,
                   CustomMapper)

        {

            this.SessionId    = SessionId;
            this.IsStoppable  = IsStoppable;

        }

        #endregion


        #region Documentation

        // {
        //
        //     //- Optional --------------------------------------
        //     "session-start": {
        //         "session-id":    "abcdef-123456-abc123-456def",
        //         "is-stoppable":  true
        //     },
        //
        //     "result": {
        //         "code":          0,
        //         "message":       "Success."
        //     }
        //
        // }

        #endregion

        #region (static) Parse   (Request, JSON,                      CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI SessionStart response.
        /// </summary>
        /// <param name="Request">The corresponding SessionStart request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static SessionStartResponse Parse(SessionStartRequest                                  Request,
                                                 JObject                                              JSON,
                                                 CustomMapperDelegate<SessionStartResponse, Builder>  CustomMapper  = null,
                                                 OnExceptionDelegate                                  OnException   = null)
        {

            if (TryParse(Request,
                         JSON,
                         out SessionStartResponse _SessionStartResponse,
                         CustomMapper,
                         OnException))
            {
                return _SessionStartResponse;
            }

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out Acknowledgement, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of a SessionStart response.
        /// </summary>
        /// <param name="Request">The corresponding SessionStart request.</param>
        /// <param name="JSON">The JSON to parse.</param>
        /// <param name="SessionStartResponse">The parsed SessionStart response</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static Boolean TryParse(SessionStartRequest                                  Request,
                                       JObject                                              JSON,
                                       out SessionStartResponse                             SessionStartResponse,
                                       CustomMapperDelegate<SessionStartResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                  OnException   = null)
        {

            try
            {

                var ResultJSON = JSON["result"];

                if (ResultJSON == null)
                {
                    SessionStartResponse = null;
                    return false;
                }


                var SessionStart = JSON["session-start"];


                SessionStartResponse = new SessionStartResponse(

                                           Request,
                                           (ResponseCodes) ResultJSON["code"].Value<Int32>(),
                                           ResultJSON["message"].Value<String>(),

                                           SessionStart?["session-id"]   != null
                                               ? new Session_Id?(Session_Id.Parse(ResultJSON["session-id"].Value<String>()))
                                               : null,

                                           SessionStart?["is-stoppable"] != null
                                               ? new Boolean?   (ResultJSON["is-stoppable"].Value<Boolean>())
                                               : null

                                       );

                if (CustomMapper != null)
                    SessionStartResponse = CustomMapper(JSON,
                                                       SessionStartResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, JSON, e);

                SessionStartResponse = null;
                return false;

            }

        }

        #endregion

        #region ToJSON()

        /// <summary>
        /// Return a JSON-representation of this object.
        /// </summary>
        public override JObject ToJSON()

            => JSONObject.Create(

                   SessionId.HasValue || IsStoppable.HasValue
                       ? new JProperty("session-start", JSONObject.Create(

                             SessionId.HasValue
                                 ? new JProperty("session-id", SessionId.ToString())
                                 : null,

                             IsStoppable.HasValue
                                 ? new JProperty("is-stoppable", IsStoppable)
                                 : null

                         ))
                       : null,

                   new JProperty("result", JSONObject.Create(

                       new JProperty("code",     (UInt32) Code),
                       new JProperty("message",  Message)

                   ))
               );

        #endregion


        #region Operator overloading

        #region Operator == (SessionStartResponse1, SessionStartResponse2)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="SessionStartResponse1">A response.</param>
        /// <param name="SessionStartResponse2">Another response.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (SessionStartResponse SessionStartResponse1, SessionStartResponse SessionStartResponse2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(SessionStartResponse1, SessionStartResponse2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) SessionStartResponse1 == null) || ((Object) SessionStartResponse2 == null))
                return false;

            return SessionStartResponse1.Equals(SessionStartResponse2);

        }

        #endregion

        #region Operator != (SessionStartResponse1, SessionStartResponse2)

        /// <summary>
        /// Compares two responses for inequality.
        /// </summary>
        /// <param name="SessionStartResponse1">A response.</param>
        /// <param name="SessionStartResponse2">Another response.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (SessionStartResponse SessionStartResponse1, SessionStartResponse SessionStartResponse2)
            => !(SessionStartResponse1 == SessionStartResponse2);

        #endregion

        #endregion

        #region IEquatable<SessionStartResponse> Members

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

            var AResponse = Object as SessionStartResponse;
            if ((Object) AResponse == null)
                return false;

            return Equals(AResponse);

        }

        #endregion

        #region Equals(SessionStartResponse)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="SessionStartResponse">A response to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(SessionStartResponse SessionStartResponse)
        {

            if ((Object) SessionStartResponse == null)
                return false;

            return Code.   Equals(SessionStartResponse.Code)    &&
                   Message.Equals(SessionStartResponse.Message) &&

                   ((SessionId   == null && SessionStartResponse.SessionId   == null) ||
                    (SessionId   != null && SessionStartResponse.SessionId   != null && SessionId.  Equals(SessionStartResponse.SessionId))) &&

                   ((IsStoppable == null && SessionStartResponse.IsStoppable == null) ||
                    (IsStoppable != null && SessionStartResponse.IsStoppable != null && IsStoppable.Equals(SessionStartResponse.IsStoppable)));

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

                return Code.   GetHashCode() * 7 ^
                       Message.GetHashCode() * 5 ^

                       (SessionId.HasValue
                            ? SessionId.GetHashCode()
                            : 0) * 3 ^

                       (IsStoppable.HasValue
                            ? IsStoppable.GetHashCode()
                            : 0);

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat("SessionStart response: ",
                             Code.ToString(),
                             ", '", Message, "'",

                             SessionId.HasValue
                                 ? ", SessionId: " + SessionId.ToString()
                                 : "",

                             IsStoppable.HasValue
                                 ? ", stoppable"
                                 : "");

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
        /// A SessionStart response builder.
        /// </summary>
        public class Builder : AResponseBuilder<SessionStartRequest,
                                                SessionStartResponse>
        {

            #region Properties

            /// <summary>
            /// The optional unique identification of the charging session.
            /// </summary>
            public Session_Id?  SessionId     { get; set; }

            /// <summary>
            /// The optional indication whether the session can be stopped via "session-stop" API call.
            /// </summary>
            public Boolean?     IsStoppable   { get; set; }

            #endregion

            #region Constructor(s)

            public Builder(SessionStartResponse Response = null)

                : base(Response?.Request,
                       Response)

            {

                this.SessionId    = Response?.SessionId;
                this.IsStoppable  = Response?.IsStoppable;

            }

            #endregion

            #region (implicit) "ToImmutable()"

            /// <summary>
            /// Return an immutable SessionStart response.
            /// </summary>
            /// <param name="Builder">A SessionStart response builder.</param>
            public static implicit operator SessionStartResponse(Builder Builder)

                => new SessionStartResponse(Builder.Request,
                                            Builder.Code,
                                            Builder.Message,
                                            Builder.SessionId,
                                            Builder.IsStoppable,
                                            Builder.CustomData,
                                            Builder.CustomMapper);

            #endregion

        }

        #endregion

    }

}
