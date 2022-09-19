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

using System;
using System.Collections.Generic;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// The current timestamped status of an OIOI connector.
    /// </summary>
    public class ConnectorStatus : AInternalData,
                                   IEquatable <ConnectorStatus>,
                                   IComparable<ConnectorStatus>
    {

        #region Properties

        /// <summary>
        /// The unique identification of the connector.
        /// </summary>
        public Connector_Id          Id           { get; }

        /// <summary>
        /// The current status of the connector.
        /// </summary>
        public ConnectorStatusTypes  Status       { get; }

        /// <summary>
        /// The timestamp of the current status of the connector.
        /// </summary>
        public DateTime              Timestamp    { get; }

        /// <summary>
        /// The timestamped status of the connector.
        /// </summary>
        public Timestamped<ConnectorStatusTypes> Combined
            => new Timestamped<ConnectorStatusTypes>(Timestamp, Status);

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new connector status.
        /// </summary>
        /// <param name="Id">The unique identification of the connector.</param>
        /// <param name="Status">The current status of the connector.</param>
        /// <param name="Timestamp">An optional timestamp of the current status of the connector.</param>
        /// <param name="CustomData">An optional dictionary of customer-specific data.</param>
        public ConnectorStatus(Connector_Id                         Id,
                               ConnectorStatusTypes                 Status,
                               DateTime?                            Timestamp    = null,
                               IReadOnlyDictionary<String, Object>  CustomData   = null)

            : base(null,
                   CustomData)

        {

            this.Id         = Id;
            this.Status     = Status;
            this.Timestamp  = Timestamp ?? DateTime.UtcNow;

        }

        #endregion


        #region Operator overloading

        #region Operator == (ConnectorStatus1, ConnectorStatus2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatus1">An EVSE status.</param>
        /// <param name="ConnectorStatus2">Another EVSE status.</param>
        /// <returns>true|false</returns>
        public static Boolean operator == (ConnectorStatus ConnectorStatus1, ConnectorStatus ConnectorStatus2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(ConnectorStatus1, ConnectorStatus2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) ConnectorStatus1 == null) || ((Object) ConnectorStatus2 == null))
                return false;

            return ConnectorStatus1.Equals(ConnectorStatus2);

        }

        #endregion

        #region Operator != (ConnectorStatus1, ConnectorStatus2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatus1">An EVSE status.</param>
        /// <param name="ConnectorStatus2">Another EVSE status.</param>
        /// <returns>true|false</returns>
        public static Boolean operator != (ConnectorStatus ConnectorStatus1, ConnectorStatus ConnectorStatus2)
            => !(ConnectorStatus1 == ConnectorStatus2);

        #endregion

        #region Operator <  (ConnectorStatus1, ConnectorStatus2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatus1">An EVSE status.</param>
        /// <param name="ConnectorStatus2">Another EVSE status.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (ConnectorStatus ConnectorStatus1, ConnectorStatus ConnectorStatus2)
        {

            if ((Object) ConnectorStatus1 == null)
                throw new ArgumentNullException(nameof(ConnectorStatus1), "The given ConnectorStatus1 must not be null!");

            return ConnectorStatus1.CompareTo(ConnectorStatus2) < 0;

        }

        #endregion

        #region Operator <= (ConnectorStatus1, ConnectorStatus2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatus1">An EVSE status.</param>
        /// <param name="ConnectorStatus2">Another EVSE status.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (ConnectorStatus ConnectorStatus1, ConnectorStatus ConnectorStatus2)
            => !(ConnectorStatus1 > ConnectorStatus2);

        #endregion

        #region Operator >  (ConnectorStatus1, ConnectorStatus2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatus1">An EVSE status.</param>
        /// <param name="ConnectorStatus2">Another EVSE status.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (ConnectorStatus ConnectorStatus1, ConnectorStatus ConnectorStatus2)
        {

            if ((Object) ConnectorStatus1 == null)
                throw new ArgumentNullException(nameof(ConnectorStatus1), "The given ConnectorStatus1 must not be null!");

            return ConnectorStatus1.CompareTo(ConnectorStatus2) > 0;

        }

        #endregion

        #region Operator >= (ConnectorStatus1, ConnectorStatus2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatus1">An EVSE status.</param>
        /// <param name="ConnectorStatus2">Another EVSE status.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (ConnectorStatus ConnectorStatus1, ConnectorStatus ConnectorStatus2)
            => !(ConnectorStatus1 < ConnectorStatus2);

        #endregion

        #endregion

        #region IComparable<ConnectorStatus> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            if (!(Object is ConnectorStatus))
                throw new ArgumentException("The given object is not a ConnectorStatus!",
                                            nameof(Object));

            return CompareTo((ConnectorStatus) Object);

        }

        #endregion

        #region CompareTo(ConnectorStatus)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatus">An object to compare with.</param>
        public Int32 CompareTo(ConnectorStatus ConnectorStatus)
        {

            if ((Object) ConnectorStatus == null)
                throw new ArgumentNullException(nameof(ConnectorStatus), "The given ConnectorStatus must not be null!");

            // Compare EVSE Ids
            var _Result = Id.       CompareTo(ConnectorStatus.Id);

            // If equal: Compare EVSE status
            if (_Result == 0)
                _Result = Status.   CompareTo(ConnectorStatus.Status);

            // If equal: Compare timestamps
            if (_Result == 0)
                _Result = Timestamp.CompareTo(ConnectorStatus.Timestamp);

            return _Result;

        }

        #endregion

        #endregion

        #region IEquatable<ConnectorStatus> Members

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

            if (!(Object is ConnectorStatus))
                return false;

            return Equals((ConnectorStatus) Object);

        }

        #endregion

        #region Equals(ConnectorStatus)

        /// <summary>
        /// Compares two EVSE identifications for equality.
        /// </summary>
        /// <param name="ConnectorStatus">An EVSE identification to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(ConnectorStatus ConnectorStatus)
        {

            if ((Object) ConnectorStatus == null)
                return false;

            return Id.       Equals(ConnectorStatus.Id)     &&
                   Status.   Equals(ConnectorStatus.Status) &&
                   Timestamp.Equals(ConnectorStatus.Timestamp);

        }

        #endregion

        #endregion

        #region (override) GetHashCode()

        /// <summary>
        /// Return the HashCode of this object.
        /// </summary>
        /// <returns>The HashCode of this object.</returns>
        public override Int32 GetHashCode()
        {
            unchecked
            {

                return Id.       GetHashCode() * 7 ^
                       Status.   GetHashCode() * 5 ^
                       Timestamp.GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Id, " -> ",
                             Status,
                             " since ",
                             Timestamp.ToIso8601());

        #endregion

    }

}
