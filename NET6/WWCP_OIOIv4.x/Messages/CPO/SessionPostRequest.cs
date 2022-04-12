/*
 * Copyright (c) 2014-2022 GraphDefined GmbH
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
using org.GraphDefined.Vanaheimr.Hermod.JSON;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

    /// <summary>
    /// An OIOI Session Post request.
    /// </summary>
    public class SessionPostRequest : ARequest<SessionPostRequest>
    {

        #region Properties

        /// <summary>
        /// A charging session.
        /// </summary>
        [Mandatory]
        public Session  Session   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create an OIOI Session Post JSON/HTTP request.
        /// </summary>
        /// <param name="Session">A charging session.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public SessionPostRequest(Session             Session,

                                  DateTime?           Timestamp           = null,
                                  CancellationToken?  CancellationToken   = null,
                                  EventTracking_Id    EventTrackingId     = null,
                                  TimeSpan?           RequestTimeout      = null)

            : base(Timestamp,
                   CancellationToken,
                   EventTrackingId,
                   RequestTimeout)

        {

            this.Session = Session;

        }

        #endregion


        #region Documentation

        // {
        //     "session-post": {
        //         "user": {
        //             "identifier": "12345678",
        //             "identifier-type": "rfid"
        //         },
        //         "session-id": "abcdef-123456-abc123-456def",
        //         "connector-id": "DE*8PS*ETABCD*1",
        //         "session-interval": {
        //             "start": "2010-01-01T11:00:00+00:00",
        //             "stop": "2010-01-01T17:00:00+00:00"
        //         },
        //         "charging-interval": {
        //             "start": "2010-01-01T12:00:00+00:00",
        //             "stop": "2010-01-01T16:00:00+00:00"
        //         },
        //         "energy-consumed": 16.5,
        //         "partner-identifier": "123456-123456-abcdef-abc123-456def"
        //     }
        // }

        #endregion

        #region (static) Parse(SessionPostRequestJSON, CustomSessionPostRequestParser = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI Session Post request.
        /// </summary>
        /// <param name="SessionPostRequestJSON">The JSON to parse.</param>
        /// <param name="CustomSessionPostRequestParser">A delegate to parse custom SessionPost requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static SessionPostRequest Parse(JObject                                          SessionPostRequestJSON,
                                               CustomJObjectParserDelegate<SessionPostRequest>  CustomSessionPostRequestParser   = null,
                                               OnExceptionDelegate                              OnException                      = null)
        {

            if (TryParse(SessionPostRequestJSON,
                         out SessionPostRequest sessionPostRequest,
                         CustomSessionPostRequestParser,
                         OnException))
            {
                return sessionPostRequest;
            }

            return null;

        }

        #endregion

        #region (static) Parse(SessionPostRequestText, CustomSessionPostRequestParser = null, OnException = null)

        /// <summary>
        /// Parse the given text representation of an OIOI Session Post request.
        /// </summary>
        /// <param name="SessionPostRequestText">The text to parse.</param>
        /// <param name="CustomSessionPostRequestParser">A delegate to parse custom SessionPost requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static SessionPostRequest Parse(String                                           SessionPostRequestText,
                                               CustomJObjectParserDelegate<SessionPostRequest>  CustomSessionPostRequestParser   = null,
                                               OnExceptionDelegate                              OnException                      = null)
        {

            if (TryParse(SessionPostRequestText,
                         out SessionPostRequest sessionPostRequest,
                         CustomSessionPostRequestParser,
                         OnException))
            {
                return sessionPostRequest;
            }

            return null;

        }

        #endregion

        #region (static) TryParse(SessionPostRequestJSON, out SessionPostRequest, CustomSessionPostRequestParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI Session Post request.
        /// </summary>
        /// <param name="SessionPostRequestJSON">The JSON to parse.</param>
        /// <param name="SessionPostRequest">The parsed Session Post request.</param>
        /// <param name="CustomSessionPostRequestParser">A delegate to parse custom SessionPost requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject                                          SessionPostRequestJSON,
                                       out SessionPostRequest                           SessionPostRequest,
                                       CustomJObjectParserDelegate<SessionPostRequest>  CustomSessionPostRequestParser   = null,
                                       OnExceptionDelegate                              OnException                      = null)
        {

            try
            {

                //var SessionJSON = Session.TryParse(SessionPostRequestJSON["session-post"] as JObject);

                SessionPostRequest = new SessionPostRequest(

                                         null

                                     );


                if (CustomSessionPostRequestParser != null)
                    SessionPostRequest = CustomSessionPostRequestParser(SessionPostRequestJSON,
                                                                        SessionPostRequest);

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, SessionPostRequestJSON, e);

                SessionPostRequest = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(SessionPostRequestText, out SessionPostRequest, CustomSessionPostRequestParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI Session Post request.
        /// </summary>
        /// <param name="SessionPostRequestText">The text to parse.</param>
        /// <param name="SessionPostRequest">The parsed Session Post request.</param>
        /// <param name="CustomSessionPostRequestParser">A delegate to parse custom SessionPost requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String                                        SessionPostRequestText,
                                       out SessionPostRequest                        SessionPostRequest,
                                       CustomJObjectParserDelegate<SessionPostRequest>  CustomSessionPostRequestParser   = null,
                                       OnExceptionDelegate                           OnException                      = null)
        {

            try
            {

                if (TryParse(JObject.Parse(SessionPostRequestText),
                             out SessionPostRequest,
                             CustomSessionPostRequestParser,
                             OnException))
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                OnException?.Invoke(DateTime.UtcNow, SessionPostRequestText, e);
            }

            SessionPostRequest = null;
            return false;

        }

        #endregion

        #region ToJSON(CustomSessionPostRequestSerializer = null, CustomSessionSerializer = null)

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        /// <param name="CustomSessionPostRequestSerializer">A delegate to serialize custom CustomSessionPost requests.</param>
        /// <param name="CustomSessionSerializer">A delegate to serialize custom Session JSON objects.</param>
        public JObject ToJSON(CustomJObjectSerializerDelegate<SessionPostRequest>  CustomSessionPostRequestSerializer   = null,
                              CustomJObjectSerializerDelegate<Session>             CustomSessionSerializer              = null)
        {

            var JSON = JSONObject.Create(
                               new JProperty("session-post",  Session.ToJSON(CustomSessionSerializer))
                           );

            return CustomSessionPostRequestSerializer != null
                       ? CustomSessionPostRequestSerializer(this, JSON)
                       : JSON;

        }

        #endregion


        #region Operator overloading

        #region Operator == (SessionPost1, SessionPost2)

        /// <summary>
        /// Compares two session post requests for equality.
        /// </summary>
        /// <param name="SessionPost1">An session post request.</param>
        /// <param name="SessionPost2">Another session post request.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (SessionPostRequest SessionPost1, SessionPostRequest SessionPost2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(SessionPost1, SessionPost2))
                return true;

            // If one is null, but not both, return false.
            if ((SessionPost1 is null) || (SessionPost2 is null))
                return false;

            return SessionPost1.Equals(SessionPost2);

        }

        #endregion

        #region Operator != (SessionPost1, SessionPost2)

        /// <summary>
        /// Compares two session post requests for inequality.
        /// </summary>
        /// <param name="SessionPost1">An session post request.</param>
        /// <param name="SessionPost2">Another session post request.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (SessionPostRequest SessionPost1, SessionPostRequest SessionPost2)
            => !(SessionPost1 == SessionPost2);

        #endregion

        #endregion

        #region IEquatable<SessionPostRequest> Members

        #region Equals(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        /// <returns>true|false</returns>
        public override Boolean Equals(Object Object)
        {

            if (Object is null)
                return false;

            if (!(Object is SessionPostRequest SessionPostRequest))
                return false;

            return Equals(SessionPostRequest);

        }

        #endregion

        #region Equals(SessionPostRequest)

        /// <summary>
        /// Compares two Session Post requests for equality.
        /// </summary>
        /// <param name="SessionPostRequest">A Session Post request to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(SessionPostRequest SessionPostRequest)
        {

            if (SessionPostRequest is null)
                return false;

            return Session.Equals(SessionPostRequest.Session);

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
                return Session.GetHashCode();
            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat("Session Post '",
                             Session.Id +
                             "'");

        #endregion

    }

}
