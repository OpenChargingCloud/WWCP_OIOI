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
using System.Threading;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.JSON;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.EMP
{

    /// <summary>
    /// An OIOI Session Stop request.
    /// </summary>
    public class SessionStopRequest : ARequest<SessionStopRequest>
    {

        #region Properties

        /// <summary>
        /// The customer who wants to stop charging.
        /// </summary>
        [Mandatory]
        public User          User          { get; }

        /// <summary>
        /// The connector where the customer wants to stop charging.
        /// </summary>
        [Mandatory]
        public Connector_Id  ConnectorId   { get; }

        /// <summary>
        /// The payment method the customer wants to use for paying this charging session.
        /// </summary>
        [Optional]
        public Session_Id?   SessionId     { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create an OIOI Session Stop JSON/HTTP request.
        /// </summary>
        /// <param name="User">The customer who wants to stop charging.</param>
        /// <param name="ConnectorId">The connector where the customer wants to stop charging.</param>
        /// <param name="SessionId">The payment method the customer wants to use for paying this charging session.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public SessionStopRequest(User                User,
                                  Connector_Id        ConnectorId,
                                  Session_Id?         SessionId           = null,

                                  DateTime?           Timestamp           = null,
                                  CancellationToken?  CancellationToken   = null,
                                  EventTracking_Id    EventTrackingId     = null,
                                  TimeSpan?           RequestTimeout      = null)

            : base(Timestamp,
                   CancellationToken,
                   EventTrackingId,
                   RequestTimeout)

        {

            this.User         = User;
            this.ConnectorId  = ConnectorId;
            this.SessionId    = SessionId;

        }

        #endregion


        #region Documentation

        // {
        //    "session-stop": {
        //
        //        "user": {
        //            "identifier-type":  "evco-id",
        //            "identifier":       "DE*8PS*123456*7"
        //        },
        //
        //        "connector-id":  "1356",
        //        "session-id":    "1234..."
        //
        //    }
        // }

        #endregion

        #region (static) Parse(SessionStopRequestJSON, CustomSessionStopRequestParser = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI Session Stop request.
        /// </summary>
        /// <param name="SessionStopRequestJSON">The JSON to parse.</param>
        /// <param name="CustomSessionStopRequestParser">A delegate to parse custom SessionStop requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static SessionStopRequest Parse(JObject                                       SessionStopRequestJSON,
                                               CustomJObjectParserDelegate<SessionStopRequest>  CustomSessionStopRequestParser   = null,
                                               OnExceptionDelegate                           OnException                      = null)
        {

            if (TryParse(SessionStopRequestJSON,
                         out SessionStopRequest _SessionStopRequest,
                         CustomSessionStopRequestParser,
                         OnException))

                return _SessionStopRequest;

            return null;

        }

        #endregion

        #region (static) Parse(SessionStopRequestText, CustomSessionStopRequestParser = null, OnException = null)

        /// <summary>
        /// Parse the given text representation of an OIOI Session Stop request.
        /// </summary>
        /// <param name="SessionStopRequestText">The text to parse.</param>
        /// <param name="CustomSessionStopRequestParser">A delegate to parse custom SessionStop requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static SessionStopRequest Parse(String                                        SessionStopRequestText,
                                               CustomJObjectParserDelegate<SessionStopRequest>  CustomSessionStopRequestParser   = null,
                                               OnExceptionDelegate                           OnException                      = null)
        {

            if (TryParse(SessionStopRequestText,
                         out SessionStopRequest _SessionStopRequest,
                         CustomSessionStopRequestParser,
                         OnException))

                return _SessionStopRequest;

            return null;

        }

        #endregion

        #region (static) TryParse(SessionStopRequestJSON, out SessionStopRequest, CustomSessionStopRequestParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI Session Stop request.
        /// </summary>
        /// <param name="SessionStopRequestJSON">The JSON to parse.</param>
        /// <param name="SessionStopRequest">The parsed Session Stop request.</param>
        /// <param name="CustomSessionStopRequestParser">A delegate to parse custom SessionStop requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject                                       SessionStopRequestJSON,
                                       out SessionStopRequest                        SessionStopRequest,
                                       CustomJObjectParserDelegate<SessionStopRequest>  CustomSessionStopRequestParser   = null,
                                       OnExceptionDelegate                           OnException                      = null)
        {

            try
            {

                var SessionStop = SessionStopRequestJSON["session-stop"];

                SessionStopRequest = new SessionStopRequest(

                                         User.Parse(SessionStop["user"] as JObject),
                                         Connector_Id.Parse(SessionStop["connector-id"].Value<String>()),

                                         SessionStop["session-id"] != null
                                             ? new Session_Id?(Session_Id.Parse(SessionStop["session-id"].Value<String>()))
                                             : null

                                     );


                if (CustomSessionStopRequestParser != null)
                    SessionStopRequest = CustomSessionStopRequestParser(SessionStopRequestJSON,
                                                                        SessionStopRequest);

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, SessionStopRequestJSON, e);

                SessionStopRequest = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(SessionStopRequestText, out SessionStopRequest, CustomSessionStopRequestParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI Session Stop request.
        /// </summary>
        /// <param name="SessionStopRequestText">The text to parse.</param>
        /// <param name="SessionStopRequest">The parsed Session Stop request.</param>
        /// <param name="CustomSessionStopRequestParser">A delegate to parse custom SessionStop requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String                                        SessionStopRequestText,
                                       out SessionStopRequest                        SessionStopRequest,
                                       CustomJObjectParserDelegate<SessionStopRequest>  CustomSessionStopRequestParser   = null,
                                       OnExceptionDelegate                           OnException                      = null)
        {

            try
            {

                if (TryParse(JObject.Parse(SessionStopRequestText),
                             out SessionStopRequest,
                             CustomSessionStopRequestParser,
                             OnException))

                    return true;

            }
            catch (Exception e)
            {
                OnException?.Invoke(DateTime.UtcNow, SessionStopRequestText, e);
            }

            SessionStopRequest = null;
            return false;

        }

        #endregion

        #region ToJSON(CustomSessionStopRequestSerializer = null, CustomUserSerializer = null)

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        /// <param name="CustomSessionStopRequestSerializer">A delegate to serialize custom SessionStop requests.</param>
        /// <param name="CustomUserSerializer">A delegate to serialize custom User JSON objects.</param>
        public JObject ToJSON(CustomJObjectSerializerDelegate<SessionStopRequest>  CustomSessionStopRequestSerializer   = null,
                              CustomJObjectSerializerDelegate<User>                CustomUserSerializer                 = null)
        {

            var JSON = JSONObject.Create(
                           new JProperty("session-stop", JSONObject.Create(

                               new JProperty("user",          User.       ToJSON(CustomUserSerializer)),
                               new JProperty("connector-id",  ConnectorId.ToString()),

                               SessionId.HasValue
                                   ? new JProperty("session-id",  SessionId.ToString())
                                   : null

                           )));

            return CustomSessionStopRequestSerializer != null
                       ? CustomSessionStopRequestSerializer(this, JSON)
                       : JSON;

        }

        #endregion


        #region Operator overloading

        #region Operator == (SessionStop1, SessionStop2)

        /// <summary>
        /// Compares two Session Stop requests for equality.
        /// </summary>
        /// <param name="SessionStop1">An Session Stop request.</param>
        /// <param name="SessionStop2">Another Session Stop request.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (SessionStopRequest SessionStop1, SessionStopRequest SessionStop2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(SessionStop1, SessionStop2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) SessionStop1 == null) || ((Object) SessionStop2 == null))
                return false;

            return SessionStop1.Equals(SessionStop2);

        }

        #endregion

        #region Operator != (SessionStop1, SessionStop2)

        /// <summary>
        /// Compares two Session Stop requests for inequality.
        /// </summary>
        /// <param name="SessionStop1">An Session Stop request.</param>
        /// <param name="SessionStop2">Another Session Stop request.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (SessionStopRequest SessionStop1, SessionStopRequest SessionStop2)
            => !(SessionStop1 == SessionStop2);

        #endregion

        #endregion

        #region IEquatable<SessionStopRequest> Members

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

            var SessionStop = Object as SessionStopRequest;
            if ((Object) SessionStop == null)
                return false;

            return Equals(SessionStop);

        }

        #endregion

        #region Equals(SessionStopRequest)

        /// <summary>
        /// Compares two Session Stop requests for equality.
        /// </summary>
        /// <param name="SessionStopRequest">A Session Stop request to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(SessionStopRequest SessionStopRequest)
        {

            if ((Object) SessionStopRequest == null)
                return false;

            return User.       Equals(SessionStopRequest.User)        &&
                   ConnectorId.Equals(SessionStopRequest.ConnectorId) &&

                   (!SessionId.HasValue && !SessionStopRequest.SessionId.HasValue) ||
                   (SessionId.HasValue && SessionStopRequest.SessionId.HasValue && SessionId.Value.Equals(SessionStopRequest.SessionId.Value));

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

                return User.       GetHashCode() * 7 ^
                       ConnectorId.GetHashCode() * 5 ^

                       (SessionId.HasValue
                            ? SessionId.GetHashCode()
                            : 0);

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat("Session Stop: ",
                             User.Identifier,
                             " at ",
                             ConnectorId,
                             SessionId.HasValue ? " via " + SessionId : "");

        #endregion


    }

}
