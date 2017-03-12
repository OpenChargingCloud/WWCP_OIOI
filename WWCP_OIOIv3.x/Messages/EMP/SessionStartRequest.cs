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
using System.Threading;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x.EMP
{

    /// <summary>
    /// An OIOI Session Start request.
    /// </summary>
    public class SessionStartRequest : ARequest<SessionStartRequest>
    {

        #region Properties

        /// <summary>
        /// The customer who wants to charge.
        /// </summary>
        [Mandatory]
        public User               User               { get; }

        /// <summary>
        /// The connector where the customer wants to charge.
        /// </summary>
        [Mandatory]
        public Connector_Id       ConnectorId        { get; }

        /// <summary>
        /// The payment method the customer wants to use for paying this charging session.
        /// </summary>
        [Optional]
        public PaymentReference?  PaymentReference   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create an OIOI Session Start JSON/HTTP request.
        /// </summary>
        /// <param name="User">A customer who wants to charge.</param>
        /// <param name="ConnectorId">The connector where the customer wants to charge.</param>
        /// <param name="PaymentReference">The payment method the customer wants to use for paying this charging session.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public SessionStartRequest(User                User,
                                   Connector_Id        ConnectorId,
                                   PaymentReference?   PaymentReference    = null,

                                   DateTime?           Timestamp           = null,
                                   CancellationToken?  CancellationToken   = null,
                                   EventTracking_Id    EventTrackingId     = null,
                                   TimeSpan?           RequestTimeout      = null)

            : base(Timestamp,
                   CancellationToken,
                   EventTrackingId,
                   RequestTimeout)

        {

            this.User              = User;
            this.ConnectorId       = ConnectorId;
            this.PaymentReference  = PaymentReference;

        }

        #endregion


        #region Documentation

        // {
        //    "session-start": {
        //
        //        "user": {
        //            "identifier-type":  "evco-id",
        //            "identifier":       "DE*8PS*123456*7",
        //            "token":            "..."   [optional]
        //        },
        //
        //        "connector-id":       "1356",
        //        "payment-reference":  "1212"    [optional]
        //
        //    }
        // }

        #endregion

        #region (static) Parse(SessionStartRequestJSON, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI Session Start request.
        /// </summary>
        /// <param name="SessionStartRequestJSON">The JSON to parse.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static SessionStartRequest Parse(JObject              SessionStartRequestJSON,
                                                OnExceptionDelegate  OnException = null)
        {

            SessionStartRequest _SessionStartRequest;

            if (TryParse(SessionStartRequestJSON, out _SessionStartRequest, OnException))
                return _SessionStartRequest;

            return null;

        }

        #endregion

        #region (static) Parse(SessionStartRequestText, OnException = null)

        /// <summary>
        /// Parse the given text representation of an OIOI Session Start request.
        /// </summary>
        /// <param name="SessionStartRequestText">The text to parse.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static SessionStartRequest Parse(String               SessionStartRequestText,
                                                OnExceptionDelegate  OnException = null)
        {

            SessionStartRequest _SessionStartRequest;

            if (TryParse(SessionStartRequestText, out _SessionStartRequest, OnException))
                return _SessionStartRequest;

            return null;

        }

        #endregion

        #region (static) TryParse(SessionStartRequestJSON,  out SessionStartRequest, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI Session Start request.
        /// </summary>
        /// <param name="SessionStartRequestJSON">The JSON to parse.</param>
        /// <param name="SessionStartRequest">The parsed Session Start request.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject                  SessionStartRequestJSON,
                                       out SessionStartRequest  SessionStartRequest,
                                       OnExceptionDelegate      OnException  = null)
        {

            try
            {

                var SessionStart = SessionStartRequestJSON["session-start"];

                SessionStartRequest = new SessionStartRequest(

                                         User.Parse(SessionStart["user"] as JObject),
                                         Connector_Id.Parse(SessionStart["connector-id"].Value<String>()),

                                         SessionStart["payment-reference"] != null
                                             ? new Nullable<PaymentReference>(OIOIv3_x.PaymentReference.Parse(SessionStart["payment-reference"].Value<String>()))
                                             : null

                                     );

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, SessionStartRequestJSON, e);

                SessionStartRequest = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(SessionStartText, out SessionStart, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI Session Start request.
        /// </summary>
        /// <param name="SessionStartRequestText">The text to parse.</param>
        /// <param name="SessionStartRequest">The parsed Session Start request.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String                   SessionStartRequestText,
                                       out SessionStartRequest  SessionStartRequest,
                                       OnExceptionDelegate      OnException  = null)
        {

            try
            {

                if (TryParse(JObject.Parse(SessionStartRequestText),
                             out SessionStartRequest,
                             OnException))

                    return true;

            }
            catch (Exception e)
            {
                OnException?.Invoke(DateTime.Now, SessionStartRequestText, e);
            }

            SessionStartRequest = null;
            return false;

        }

        #endregion

        #region ToJSON()

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        public JObject ToJSON()

            => new JObject(new JObject(
                               new JProperty("session-start", JSONObject.Create(

                                   new JProperty("user",          User.       ToJSON()),
                                   new JProperty("connector-id",  ConnectorId.ToString()),

                                   PaymentReference.HasValue
                                       ? new JProperty("payment-reference", PaymentReference.ToString())
                                       : null

                               ))
                           ));

        #endregion


        #region Operator overloading

        #region Operator == (SessionStart1, SessionStart2)

        /// <summary>
        /// Compares two Session Start requests for equality.
        /// </summary>
        /// <param name="SessionStart1">An Session Start request.</param>
        /// <param name="SessionStart2">Another Session Start request.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (SessionStartRequest SessionStart1, SessionStartRequest SessionStart2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(SessionStart1, SessionStart2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) SessionStart1 == null) || ((Object) SessionStart2 == null))
                return false;

            return SessionStart1.Equals(SessionStart2);

        }

        #endregion

        #region Operator != (SessionStart1, SessionStart2)

        /// <summary>
        /// Compares two Session Start requests for inequality.
        /// </summary>
        /// <param name="SessionStart1">An Session Start request.</param>
        /// <param name="SessionStart2">Another Session Start request.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (SessionStartRequest SessionStart1, SessionStartRequest SessionStart2)
            => !(SessionStart1 == SessionStart2);

        #endregion

        #endregion

        #region IEquatable<SessionStartRequest> Members

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

            var SessionStart = Object as SessionStartRequest;
            if ((Object) SessionStart == null)
                return false;

            return Equals(SessionStart);

        }

        #endregion

        #region Equals(SessionStartRequest)

        /// <summary>
        /// Compares two Session Start requests for equality.
        /// </summary>
        /// <param name="SessionStartRequest">A Session Start request to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(SessionStartRequest SessionStartRequest)
        {

            if ((Object) SessionStartRequest == null)
                return false;

            return User.Equals(SessionStartRequest.User) &&
                   ConnectorId.Equals(SessionStartRequest.ConnectorId) &&

                   (!PaymentReference.HasValue && !SessionStartRequest.PaymentReference.HasValue) ||
                   (PaymentReference.HasValue && SessionStartRequest.PaymentReference.HasValue && PaymentReference.Value.Equals(SessionStartRequest.PaymentReference.Value));

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

                       (PaymentReference.HasValue
                            ? PaymentReference.GetHashCode()
                            : 0);

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat("Session Start: ",
                             User.Identifier,
                             " at ",
                             ConnectorId,
                             PaymentReference.HasValue ? " via " + PaymentReference : "");

        #endregion

    }

}
