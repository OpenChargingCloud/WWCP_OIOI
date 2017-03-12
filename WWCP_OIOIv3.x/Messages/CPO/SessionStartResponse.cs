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
    /// An OIOI SessionStart result.
    /// </summary>
    public class SessionStartResponse : AResponse<SessionStartRequest,
                                                  SessionStartResponse>
    {

        #region Properties

        /// <summary>
        /// The result of the corresponding SessionStart request.
        /// </summary>
        public Boolean      Success       { get; }

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
        /// Create a new OIOI SessionStart result.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Success">The result of the corresponding SessionStart request.</param>
        /// <param name="SessionId">An optional unique identification of the charging session.</param>
        /// <param name="IsStoppable">An optional indication whether the session can be stopped via "session-stop" API call.</param>
        /// <param name="CustomData">A read-only dictionary of custom key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific semi-structured data.</param>
        public SessionStartResponse(SessionStartRequest                  Request,
                                    Boolean                              Success,
                                    Session_Id?                          SessionId     = null,
                                    Boolean?                             IsStoppable   = null,
                                    IReadOnlyDictionary<String, Object>  CustomData    = null,
                                    Action<SessionStartResponse>         CustomMapper  = null)

            : base(Request,
                   CustomData,
                   CustomMapper)

        {

            this.Success      = Success;
            this.SessionId    = SessionId;
            this.IsStoppable  = IsStoppable;

        }

        #endregion


        #region Documentation

        // {
        //     "session-start": {
        //         "success":       true
        //         "session-id":    "abcdef-123456-abc123-456def",
        //         "is-stoppable":  true
        //     }
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

            SessionStartResponse _SessionStartResponse;

            if (TryParse(Request, JSON, out _SessionStartResponse, CustomMapper, OnException))
                return _SessionStartResponse;

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out Acknowledgement, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI SessionStart response.
        /// </summary>
        /// <param name="JSON">The JSON to parse.</param>
        /// <param name="SessionStartResponse">The parsed SessionStart response</param>
        public static Boolean TryParse(SessionStartRequest                                  Request,
                                       JObject                                              JSON,
                                       out SessionStartResponse                             SessionStartResponse,
                                       CustomMapperDelegate<SessionStartResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                  OnException   = null)
        {

            try
            {

                var InnerJSON  = JSON["session-start"];

                if (InnerJSON == null)
                {
                    SessionStartResponse = null;
                    return false;
                }

                SessionStartResponse = new SessionStartResponse(
                                           Request,
                                           InnerJSON["success"].Value<Boolean>(),

                                           InnerJSON["session-id"] != null
                                               ? new Nullable<Session_Id>(Session_Id.Parse(InnerJSON["session-id"].Value<String>()))
                                               : null,

                                           InnerJSON["is-stoppable"] != null
                                               ? new Nullable<Boolean>(InnerJSON["is-stoppable"].Value<Boolean>())
                                               : null

                                       );

                if (CustomMapper != null)
                    SessionStartResponse = CustomMapper(JSON,
                                                       SessionStartResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, JSON, e);

                SessionStartResponse = null;
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
                   new JProperty("session-start", JSONObject.Create(

                       new JProperty("success",             Success),

                       SessionId.HasValue
                           ? new JProperty("session-id",    SessionId.ToString())
                           : null,

                       IsStoppable.HasValue
                           ? new JProperty("is-stoppable",  IsStoppable)
                           : null

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
            if (Object.ReferenceEquals(SessionStartResponse1, SessionStartResponse2))
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

            return Success.Equals(SessionStartResponse.Success) &&

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

                return Success.GetHashCode() * 7 ^

                       (SessionId.HasValue
                            ? SessionId.GetHashCode()
                            : 0) * 5 ^

                       (IsStoppable.HasValue
                            ? IsStoppable.GetHashCode()
                            : 0);

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat("SessionStart response: ",
                             Success.ToString(),

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
            /// The result of the operation.
            /// </summary>
            public Boolean                     Success       { get; set; }

            /// <summary>
            /// The optional unique identification of the charging session.
            /// </summary>
            public Session_Id?                 SessionId     { get; set; }

            /// <summary>
            /// The optional indication whether the session can be stopped via "session-stop" API call.
            /// </summary>
            public Boolean?                    IsStoppable   { get; set; }

            /// <summary>
            /// Explains what the problem was, whenever 'success' was false.
            /// </summary>
            public String                      Reason        { get; set; }

            public Dictionary<String, Object>  CustomData    { get; set; }

            #endregion

            #region Constructor(s)

            public Builder(SessionStartResponse Response = null)

                : base(Response.Request,
                       Response)

            {

                if (Response != null)
                {

                    this.Success      = Response.Success;
                    this.SessionId    = Response.SessionId;
                    this.IsStoppable  = Response.IsStoppable;
                    this.CustomData   = new Dictionary<String, Object>();

                    if (Response.CustomData != null)
                        foreach (var item in Response.CustomData)
                            CustomData.Add(item.Key, item.Value);

                }

            }

            #endregion

            #region ToImmutable()

            /// <summary>
            /// Return an immutable SessionStart response.
            /// </summary>
            public SessionStartResponse ToImmutable()

                => new SessionStartResponse(Request,
                                            Success,
                                            SessionId,
                                            IsStoppable,
                                            CustomData,
                                            CustomMapper);

            #endregion

        }

        #endregion

    }

}
