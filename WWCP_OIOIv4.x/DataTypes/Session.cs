/*
 * Copyright (c) 2016-2022 GraphDefined GmbH
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


using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x
{

    /// <summary>
    /// An OIOI charging session.
    /// </summary>
    public class Session : AInternalData,
                           IEquatable<Session>,
                           IComparable<Session>,
                           IComparable
    {

        #region Properties

        /// <summary>
        /// A unique identification that identifies this session.
        /// </summary>
        [Mandatory]
        public Session_Id          Id                   { get; }

        /// <summary>
        /// The unique identification of the user/customer/driver.
        /// </summary>
        [Mandatory]
        public User                User                 { get; }

        /// <summary>
        /// The EVSE Id of the connector where the session took place.
        /// </summary>
        [Mandatory]
        public Connector_Id        ConnectorId          { get; }

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
        public Decimal?            EnergyConsumed       { get; }

        /// <summary>
        /// The partner identifier of the partner that shall be associated with this CDR.
        /// </summary>
        [Mandatory]
        public Partner_Id?         PartnerIdentifier    { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new charging session.
        /// </summary>
        /// <param name="Id">A unique identification that identifies this session.</param>
        /// <param name="User">The unique identification of the user/customer/driver.</param>
        /// <param name="ConnectorId">The EVSE Id of the connector where the session took place.</param>
        /// <param name="SessionInterval">The start and, optionally, stop timestamps of the session.</param>
        /// <param name="ChargingInterval">The start and stop timestamps of charging.</param>
        /// <param name="EnergyConsumed">The consumed energy in kWh.</param>
        /// <param name="PartnerIdentifier">The partner identifier of the partner that shall be associated with this CDR.</param>
        /// 
        /// <param name="CustomData">An optional dictionary of customer-specific data.</param>
        public Session(Session_Id              Id,
                       User                    User,
                       Connector_Id            ConnectorId,
                       StartEndDateTime        SessionInterval,
                       StartEndDateTime?       ChargingInterval    = null,
                       Decimal?                EnergyConsumed      = null,
                       Partner_Id?             PartnerIdentifier   = null,

                       JObject?                CustomData          = null,
                       UserDefinedDictionary?  InternalData        = null)

            : base(CustomData,
                   InternalData)

        {

            this.Id                 = Id;
            this.User               = User ?? throw new ArgumentNullException(nameof(User), "The given user must not be null!");
            this.ConnectorId        = ConnectorId;
            this.SessionInterval    = SessionInterval;
            this.ChargingInterval   = ChargingInterval;
            this.EnergyConsumed     = EnergyConsumed;
            this.PartnerIdentifier  = PartnerIdentifier;

        }

        #endregion


        #region Documentation

        // {
        //     "session-post": {
        //
        //         "user": {
        //             "identifier":        "12345678",
        //             "identifier-type":   "rfid"
        //         },
        //
        //         "session-id":            "abcdef-123456-abc123-456def",
        //         "connector-id":          "DE*8PS*ETABCD*1",
        //
        //         "session-interval": {
        //             "start":             "2010-01-01T11:00:00+00:00",
        //             "stop":              "2010-01-01T17:00:00+00:00"
        //         },
        //
        //         "charging-interval": {
        //             "start":             "2010-01-01T12:00:00+00:00",
        //             "stop":              "2010-01-01T16:00:00+00:00"
        //         },
        //
        //         "energy-consumed":       16.5,
        //
        //         "calculated-cost": {
        //             "amount":            14.32,
        //             "currency":          "EUR"
        //         },
        //
        //         "meter-value-signed": {
        //             "start":             "start-signed-value",
        //             "end":               "end-signed-value"
        //         },
        //
        //         "partner-identifier":    "123456-123456-abcdef-abc123-456def"
        //
        //     }
        // }

        #endregion

        #region ToJSON(CustomSessionSerializer = null)

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        /// <param name="CustomSessionSerializer">A delegate to serialize custom Session JSON objects.</param>
        public JObject ToJSON(CustomJObjectSerializerDelegate<Session> CustomSessionSerializer = null)
        {

            var JSON = JSONObject.Create(

                           new JProperty("user",                       User.        ToJSON()),
                           new JProperty("session-id",                 Id.          ToString()),
                           new JProperty("connector-id",               ConnectorId. ToString()),
                           new JProperty("session-interval",           JSONObject.Create(

                                                                           new JProperty("start", SessionInterval.StartTime.ToIso8601(false)),

                                                                           SessionInterval.EndTime.HasValue
                                                                               ? new JProperty("stop", SessionInterval.EndTime.Value.ToIso8601(false))
                                                                               : null

                                                                       )),

                           ChargingInterval != null
                               ? new JProperty("charging-interval",    JSONObject.Create(

                                                                           new JProperty("start", ChargingInterval.StartTime.ToIso8601(false)),

                                                                           ChargingInterval.EndTime.HasValue
                                                                               ? new JProperty("stop", ChargingInterval.EndTime.Value.ToIso8601(false))
                                                                               : null

                                                                       ))
                               : null,

                           EnergyConsumed.HasValue
                               ? new JProperty("energy-consumed",      EnergyConsumed.Value)
                               : null,

                           PartnerIdentifier.HasValue
                               ?  new JProperty("partner-identifier",  PartnerIdentifier.Value.ToString())
                               : null

                       );

            return CustomSessionSerializer != null
                       ? CustomSessionSerializer(this, JSON)
                       : JSON;

        }

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
            if (ReferenceEquals(Session1, Session2))
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

        #region Operator <  (Session1, Session2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Session1">A charging session.</param>
        /// <param name="Session2">Another charging session.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (Session Session1, Session Session2)
        {

            if ((Object) Session1 == null)
                throw new ArgumentNullException(nameof(Session1), "The given Session1 must not be null!");

            return Session1.CompareTo(Session2) < 0;

        }

        #endregion

        #region Operator <= (Session1, Session2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Session1">A charging session.</param>
        /// <param name="Session2">Another charging session.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (Session Session1, Session Session2)
            => !(Session1 > Session2);

        #endregion

        #region Operator >  (Session1, Session2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Session1">A charging session.</param>
        /// <param name="Session2">Another charging session.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (Session Session1, Session Session2)
        {

            if ((Object)Session1 == null)
                throw new ArgumentNullException(nameof(Session1), "The given Session1 must not be null!");

            return Session1.CompareTo(Session2) > 0;

        }

        #endregion

        #region Operator >= (Session1, Session2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Session1">A charging session.</param>
        /// <param name="Session2">Another charging session.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (Session Session1, Session Session2)
            => !(Session1 < Session2);

        #endregion

        #endregion

        #region IComparable<Session> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            var Session = Object as Session;
            if ((Object)Session == null)
                throw new ArgumentException("The given object is not an session identification!", nameof(Object));

            return CompareTo(Session);

        }

        #endregion

        #region CompareTo(Session)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Session">An object to compare with.</param>
        public Int32 CompareTo(Session Session)
        {

            if ((Object) Session == null)
                throw new ArgumentNullException(nameof(Session), "The given charging session must not be null!");

            return Id.CompareTo(Session.Id);

        }

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

            return Id.               Equals(Session.Id)              &&
                   User.             Equals(Session.User)            &&
                   ConnectorId.      Equals(Session.ConnectorId)     &&
                   SessionInterval.  Equals(Session.SessionInterval) &&
                   PartnerIdentifier.Equals(Session.PartnerIdentifier);

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

                return Id.        GetHashCode() * 17 ^
                       User.             GetHashCode() * 13 ^
                       ConnectorId.      GetHashCode() * 11 ^
                       SessionInterval.  GetHashCode() *  7 ^
                       PartnerIdentifier.GetHashCode() *  5;

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Id, " for ", User.Identifier, " at ", ConnectorId);

        #endregion

    }

}
