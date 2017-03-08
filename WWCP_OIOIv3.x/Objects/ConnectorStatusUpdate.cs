/*
 * Copyright (c) 2016-2017 GraphDefined GmbH
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
using System.Linq;

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    /// <summary>
    /// A connector status update.
    /// </summary>
    public struct ConnectorStatusUpdate : IEquatable <ConnectorStatusUpdate>,
                                          IComparable<ConnectorStatusUpdate>
    {

        #region Properties

        /// <summary>
        /// The unique identification of the connector.
        /// </summary>
        public Connector_Id                      Id          { get; }

        /// <summary>
        /// The old timestamped status of the connector.
        /// </summary>
        public Timestamped<ConnectorStatusTypes>  OldStatus   { get; }

        /// <summary>
        /// The new timestamped status of the connector.
        /// </summary>
        public Timestamped<ConnectorStatusTypes>  NewStatus   { get; }

        #endregion

        #region Constructor(s)

        #region ConnectorStatusUpdate(Id, OldStatus, NewStatus)

        /// <summary>
        /// Create a new connector status update.
        /// </summary>
        /// <param name="Id">The unique identification of the connector.</param>
        /// <param name="OldStatus">The old timestamped status of the connector.</param>
        /// <param name="NewStatus">The new timestamped status of the connector.</param>
        public ConnectorStatusUpdate(Connector_Id                      Id,
                                     Timestamped<ConnectorStatusTypes>  OldStatus,
                                     Timestamped<ConnectorStatusTypes>  NewStatus)

        {

            this.Id         = Id;
            this.OldStatus  = OldStatus;
            this.NewStatus  = NewStatus;

        }

        #endregion

        #region ConnectorStatusUpdate(Id, OldStatus, NewStatus)

        /// <summary>
        /// Create a new connector status update.
        /// </summary>
        /// <param name="Id">The unique identification of the connector.</param>
        /// <param name="OldStatus">The old timestamped status of the connector.</param>
        /// <param name="NewStatus">The new timestamped status of the connector.</param>
        public ConnectorStatusUpdate(Connector_Id     Id,
                                     ConnectorStatus  OldStatus,
                                     ConnectorStatus  NewStatus)

        {

            this.Id         = Id;
            this.OldStatus  = OldStatus.Combined;
            this.NewStatus  = NewStatus.Combined;

        }

        #endregion

        #endregion


        #region Operator overloading

        #region Operator == (ConnectorStatusUpdate1, ConnectorStatusUpdate2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatusUpdate1">A connector status update.</param>
        /// <param name="ConnectorStatusUpdate2">Another connector status update.</param>
        /// <returns>true|false</returns>
        public static Boolean operator == (ConnectorStatusUpdate ConnectorStatusUpdate1, ConnectorStatusUpdate ConnectorStatusUpdate2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(ConnectorStatusUpdate1, ConnectorStatusUpdate2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) ConnectorStatusUpdate1 == null) || ((Object) ConnectorStatusUpdate2 == null))
                return false;

            return ConnectorStatusUpdate1.Equals(ConnectorStatusUpdate2);

        }

        #endregion

        #region Operator != (ConnectorStatusUpdate1, ConnectorStatusUpdate2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatusUpdate1">A connector status update.</param>
        /// <param name="ConnectorStatusUpdate2">Another connector status update.</param>
        /// <returns>true|false</returns>
        public static Boolean operator != (ConnectorStatusUpdate ConnectorStatusUpdate1, ConnectorStatusUpdate ConnectorStatusUpdate2)
            => !(ConnectorStatusUpdate1 == ConnectorStatusUpdate2);

        #endregion

        #region Operator <  (ConnectorStatusUpdate1, ConnectorStatusUpdate2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatusUpdate1">A connector status update.</param>
        /// <param name="ConnectorStatusUpdate2">Another connector status update.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (ConnectorStatusUpdate ConnectorStatusUpdate1, ConnectorStatusUpdate ConnectorStatusUpdate2)
        {

            if ((Object) ConnectorStatusUpdate1 == null)
                throw new ArgumentNullException(nameof(ConnectorStatusUpdate1), "The given ConnectorStatusUpdate1 must not be null!");

            return ConnectorStatusUpdate1.CompareTo(ConnectorStatusUpdate2) < 0;

        }

        #endregion

        #region Operator <= (ConnectorStatusUpdate1, ConnectorStatusUpdate2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatusUpdate1">A connector status update.</param>
        /// <param name="ConnectorStatusUpdate2">Another connector status update.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (ConnectorStatusUpdate ConnectorStatusUpdate1, ConnectorStatusUpdate ConnectorStatusUpdate2)
            => !(ConnectorStatusUpdate1 > ConnectorStatusUpdate2);

        #endregion

        #region Operator >  (ConnectorStatusUpdate1, ConnectorStatusUpdate2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatusUpdate1">A connector status update.</param>
        /// <param name="ConnectorStatusUpdate2">Another connector status update.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (ConnectorStatusUpdate ConnectorStatusUpdate1, ConnectorStatusUpdate ConnectorStatusUpdate2)
        {

            if ((Object) ConnectorStatusUpdate1 == null)
                throw new ArgumentNullException(nameof(ConnectorStatusUpdate1), "The given ConnectorStatusUpdate1 must not be null!");

            return ConnectorStatusUpdate1.CompareTo(ConnectorStatusUpdate2) > 0;

        }

        #endregion

        #region Operator >= (ConnectorStatusUpdate1, ConnectorStatusUpdate2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatusUpdate1">A connector status update.</param>
        /// <param name="ConnectorStatusUpdate2">Another connector status update.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (ConnectorStatusUpdate ConnectorStatusUpdate1, ConnectorStatusUpdate ConnectorStatusUpdate2)
            => !(ConnectorStatusUpdate1 < ConnectorStatusUpdate2);

        #endregion

        #endregion

        #region IComparable<ConnectorStatusUpdate> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            if (!(Object is ConnectorStatusUpdate))
                throw new ArgumentException("The given object is not a EVSEStatus!",
                                            nameof(Object));

            return CompareTo((ConnectorStatusUpdate) Object);

        }

        #endregion

        #region CompareTo(ConnectorStatusUpdate)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorStatusUpdate">An object to compare with.</param>
        public Int32 CompareTo(ConnectorStatusUpdate ConnectorStatusUpdate)
        {

            if ((Object) ConnectorStatusUpdate == null)
                throw new ArgumentNullException(nameof(ConnectorStatusUpdate), "The given connector status update must not be null!");

            // Compare EVSE Ids
            var _Result = Id.CompareTo(ConnectorStatusUpdate.Id);

            // If equal: Compare the new connector status
            if (_Result == 0)
                _Result = NewStatus.CompareTo(ConnectorStatusUpdate.NewStatus);

            // If equal: Compare the old connector status
            if (_Result == 0)
                _Result = OldStatus.CompareTo(ConnectorStatusUpdate.OldStatus);

            return _Result;

        }

        #endregion

        #endregion

        #region IEquatable<ConnectorStatusUpdate> Members

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

            if (!(Object is ConnectorStatusUpdate))
                return false;

            return Equals((ConnectorStatusUpdate) Object);

        }

        #endregion

        #region Equals(ConnectorStatusUpdate)

        /// <summary>
        /// Compares two connector status updates for equality.
        /// </summary>
        /// <param name="ConnectorStatusUpdate">An connector status update to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(ConnectorStatusUpdate ConnectorStatusUpdate)
        {

            if ((Object) ConnectorStatusUpdate == null)
                return false;

            return Id.       Equals(ConnectorStatusUpdate.Id)        &&
                   OldStatus.Equals(ConnectorStatusUpdate.OldStatus) &&
                   NewStatus.Equals(ConnectorStatusUpdate.NewStatus);

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
                       OldStatus.GetHashCode() * 5 ^
                       NewStatus.GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Id, ": ",
                             OldStatus,
                             " -> ",
                             NewStatus);

        #endregion

    }

}
