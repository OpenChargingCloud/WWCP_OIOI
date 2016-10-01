﻿/*
 * Copyright (c) 2016 GraphDefined GmbH
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

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    /// <summary>
    /// An OIOI Charging Session.
    /// </summary>
    public class Session : IEquatable<Session>
    {

        #region Properties

        /// <summary>
        /// A unique identification that identifies this session.
        /// </summary>
        [Mandatory]
        public ChargingSession_Id  SessionId            { get; }

        /// <summary>
        /// The unique identification of the user/customer/driver.
        /// </summary>
        [Mandatory]
        public User                User                 { get; }

        /// <summary>
        /// The EVSE Id of the connector where the session took place.
        /// </summary>
        [Mandatory]
        public EVSE_Id             ConnectorId          { get; }

        /// <summary>
        /// The start and, optionally, stop timestamps of the session.
        /// </summary>
        [Optional]
        public StartEndDateTime    SessionInterval      { get; }

        /// <summary>
        /// The start and stop timestamps of charging.
        /// </summary>
        [Optional]
        public StartEndDateTime?   ChargingInterval     { get; }

        /// <summary>
        /// The consumed energy in kWh.
        /// </summary>
        [Optional]
        public Single?             EnergyConsumed       { get; }

        /// <summary>
        /// The partner identifier of the partner that shall be associated with this CDR.
        /// </summary>
        [Mandatory]
        public String              PartnerIdentifier    { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI charging session.
        /// </summary>
        /// <param name="SessionId">A unique identification that identifies this session.</param>
        /// <param name="User">The unique identification of the user/customer/driver.</param>
        /// <param name="ConnectorId">The EVSE Id of the connector where the session took place.</param>
        /// <param name="SessionInterval">The start and, optionally, stop timestamps of the session.</param>
        /// <param name="ChargingInterval">The start and stop timestamps of charging.</param>
        /// <param name="EnergyConsumed">The consumed energy in kWh.</param>
        /// <param name="PartnerIdentifier">The partner identifier of the partner that shall be associated with this CDR.</param>
        public Session(ChargingSession_Id  SessionId,
                       User                User,
                       EVSE_Id             ConnectorId,
                       StartEndDateTime    SessionInterval,
                       StartEndDateTime?   ChargingInterval   = null,
                       Single?             EnergyConsumed     = null,
                       String              PartnerIdentifier  = null)

        {

            #region Initial checks

            if (SessionId == null)
                throw new ArgumentNullException(nameof(SessionId),    "The given unique charging session identification must not be null!");

            if (User == null)
                throw new ArgumentNullException(nameof(User),         "The given user must not be null!");

            if (ConnectorId == null)
                throw new ArgumentNullException(nameof(ConnectorId),  "The given charging connector identification must not be null!");

            #endregion

            this.SessionId          = SessionId;
            this.User               = User;
            this.ConnectorId        = ConnectorId;
            this.SessionInterval    = SessionInterval;
            this.ChargingInterval   = ChargingInterval;
            this.EnergyConsumed     = EnergyConsumed;
            this.PartnerIdentifier  = PartnerIdentifier;

        }

        #endregion


        #region Documentation

        // {
        //
        //     "user": {
        //         "identifier":       "12345678",
        //         "identifier-type":  "rfid"
        //     },
        //
        //     "session-id":    "abcdef-123456-abc123-456def",
        //     "connector-id":  "DE*8PS*TABCDE*1",
        //
        //     "session-interval": {
        //         "start":  "2010-01-01T11:00:00+00:00",
        //         "stop":   "2010-01-01T17:00:00+00:00"
        //     },
        //
        //     "charging-interval": {
        //         "start":  "2010-01-01T12:00:00+00:00",
        //         "stop":   "2010-01-01T16:00:00+00:00"
        //     },
        //
        //     "energy-consumed":     16.5,
        //
        //     "partner-identifier":  "123456-123456-abcdef-abc123-456def"
        //
        // }

        #endregion


        #region ToJSON()

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        public JObject ToJSON()

            => JSONObject.Create(

                   new JProperty("user",                       User.        ToJSON()),
                   new JProperty("session-id",                 SessionId.   ToString()),
                   new JProperty("connector-id",               ConnectorId. ToString()),
                   new JProperty("session-interval",           JSONObject.Create(

                                                                   new JProperty("start", SessionInterval.StartTime.ToIso8601()),

                                                                   SessionInterval.EndTime.HasValue
                                                                       ? new JProperty("stop", SessionInterval.EndTime.Value.ToIso8601())
                                                                       : null

                                                               )),

                   ChargingInterval.HasValue
                       ? new JProperty("charging-interval",    JSONObject.Create(

                                                                   new JProperty("start", ChargingInterval.Value.StartTime.ToIso8601()),

                                                                   ChargingInterval.Value.EndTime.HasValue
                                                                       ? new JProperty("stop", ChargingInterval.Value.EndTime.Value.ToIso8601())
                                                                       : null

                                                               ))
                       : null,

                   EnergyConsumed.HasValue
                       ? new JProperty("energy-consumed",      EnergyConsumed.Value)
                       : null,

                   PartnerIdentifier.IsNotNullOrEmpty()
                       ?  new JProperty("partner-identifier",  PartnerIdentifier)
                       : null

               );

        #endregion


        #region Operator overloading

        #region Operator == (Session1, Session2)

        /// <summary>
        /// Compares two charging sessions for equality.
        /// </summary>
        /// <param name="Session1">A charging session.</param>
        /// <param name="Session2">Another charging session.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (Session Session1, Session Session2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(Session1, Session2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) Session1 == null) || ((Object) Session2 == null))
                return false;

            return Session1.Equals(Session2);

        }

        #endregion

        #region Operator != (Session1, Session2)

        /// <summary>
        /// Compares two charging sessions for inequality.
        /// </summary>
        /// <param name="Session1">A charging session.</param>
        /// <param name="Session2">Another charging session.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (Session Session1, Session Session2)

            => !(Session1 == Session2);

        #endregion

        #endregion

        #region IEquatable<Session> Members

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

            // Check if the given object is a charging session.
            var Session = Object as Session;
            if ((Object) Session == null)
                return false;

            return this.Equals(Session);

        }

        #endregion

        #region Equals(Session)

        /// <summary>
        /// Compares two charging sessions for equality.
        /// </summary>
        /// <param name="Session">A charging session to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(Session Session)
        {

            if ((Object) Session == null)
                return false;

            return SessionId.         Equals(Session.SessionId)           &&
                   User.              Equals(Session.User)                &&
                   ConnectorId.       Equals(Session.ConnectorId)         &&
                   SessionInterval.   Equals(Session.SessionInterval);
                   //ChargingInterval.  Equals(Session.ChargingInterval)    &&
                   //EnergyConsumed.    Equals(Session.EnergyConsumed)      &&
                   //PartnerIdentifier. Equals(Session.PartnerIdentifier);

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

                return SessionId.          GetHashCode() * 79 ^
                       User.               GetHashCode() * 73 ^
                       ConnectorId.        GetHashCode() * 67 ^
                       SessionInterval.    GetHashCode() * 61;
                       //ChargingInterval.   GetHashCode() * 43 ^
                       //EnergyConsumed.     GetHashCode() * 71 ^
                       //PartnerIdentifier.  GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(SessionId, " for ", User.Identifier, " at ", ConnectorId);

        #endregion

    }

}