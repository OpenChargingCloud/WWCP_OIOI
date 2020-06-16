/*
 * Copyright (c) 2016-2020 GraphDefined GmbH
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

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// The unique identification of an OIOI charging station.
    /// </summary>
    public struct Station_Id : IId,
                               IEquatable<Station_Id>,
                               IComparable<Station_Id>

    {

        #region Data

        /// <summary>
        /// The internal identification.
        /// </summary>
        private readonly String InternalId;

        #endregion

        #region Properties

        /// <summary>
        /// Indicates whether this identification is null or empty.
        /// </summary>
        public Boolean IsNullOrEmpty
            => InternalId.IsNullOrEmpty();

        /// <summary>
        /// The length of the charging station identificator.
        /// </summary>
        public UInt64 Length
            => (UInt64) InternalId.Length;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new charging station identification.
        /// based on the given string.
        /// </summary>
        private Station_Id(String Text)
        {
            InternalId = Text;
        }

        #endregion


        #region Parse(Text)

        /// <summary>
        /// Parse the given string as a charging station identification.
        /// </summary>
        /// <param name="Text">A text representation of a charging station identification.</param>
        public static Station_Id Parse(String Text)
        {

            #region Initial checks

            if (Text != null)
                Text = Text.Trim();

            if (Text.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Text), "The given text representation of a charging station identification must not be null or empty!");

            #endregion

            return new Station_Id(Text);

        }

        #endregion

        #region TryParse(Text, out PartnerId)

        /// <summary>
        /// Parse the given string as a charging station identification.
        /// </summary>
        /// <param name="Text">A text representation of a charging station identification.</param>
        /// <param name="PartnerId">The parsed charging station identification.</param>
        public static Boolean TryParse(String Text, out Station_Id PartnerId)
        {

            #region Initial checks

            if (Text != null)
                Text = Text.Trim();

            if (Text.IsNullOrEmpty())
            {
                PartnerId = default(Station_Id);
                return false;
            }

            #endregion

            try
            {

                PartnerId = new Station_Id(Text);

                return true;

            }

#pragma warning disable RCS1075  // Avoid empty catch clause that catches System.Exception.
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch (Exception)
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
#pragma warning restore RCS1075  // Avoid empty catch clause that catches System.Exception.
            { }

            PartnerId = default(Station_Id);
            return false;

        }

        #endregion

        #region Clone

        /// <summary>
        /// Clone this charging station identification.
        /// </summary>
        public Station_Id Clone

            => new Station_Id(
                   new String(InternalId.ToCharArray())
               );

        #endregion


        #region Operator overloading

        #region Operator == (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A charging station identification.</param>
        /// <param name="PartnerId2">Another charging station identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator == (Station_Id PartnerId1, Station_Id PartnerId2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(PartnerId1, PartnerId2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) PartnerId1 == null) || ((Object) PartnerId2 == null))
                return false;

            return PartnerId1.Equals(PartnerId2);

        }

        #endregion

        #region Operator != (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A charging station identification.</param>
        /// <param name="PartnerId2">Another charging station identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator != (Station_Id PartnerId1, Station_Id PartnerId2)
            => !(PartnerId1 == PartnerId2);

        #endregion

        #region Operator <  (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A charging station identification.</param>
        /// <param name="PartnerId2">Another charging station identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (Station_Id PartnerId1, Station_Id PartnerId2)
        {

            if ((Object) PartnerId1 == null)
                throw new ArgumentNullException(nameof(PartnerId1), "The given PartnerId1 must not be null!");

            return PartnerId1.CompareTo(PartnerId2) < 0;

        }

        #endregion

        #region Operator <= (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A charging station identification.</param>
        /// <param name="PartnerId2">Another charging station identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (Station_Id PartnerId1, Station_Id PartnerId2)
            => !(PartnerId1 > PartnerId2);

        #endregion

        #region Operator >  (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A charging station identification.</param>
        /// <param name="PartnerId2">Another charging station identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (Station_Id PartnerId1, Station_Id PartnerId2)
        {

            if ((Object) PartnerId1 == null)
                throw new ArgumentNullException(nameof(PartnerId1), "The given PartnerId1 must not be null!");

            return PartnerId1.CompareTo(PartnerId2) > 0;

        }

        #endregion

        #region Operator >= (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A charging station identification.</param>
        /// <param name="PartnerId2">Another charging station identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (Station_Id PartnerId1, Station_Id PartnerId2)
            => !(PartnerId1 < PartnerId2);

        #endregion

        #endregion

        #region IComparable<PartnerId> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            if (!(Object is Station_Id))
                throw new ArgumentException("The given object is not a charging station identification!",
                                            nameof(Object));

            return CompareTo((Station_Id) Object);

        }

        #endregion

        #region CompareTo(PartnerId)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId">An object to compare with.</param>
        public Int32 CompareTo(Station_Id PartnerId)
        {

            if ((Object) PartnerId == null)
                throw new ArgumentNullException(nameof(PartnerId),  "The given charging station identification must not be null!");

            // Compare the length of the PartnerIds
            var _Result = this.Length.CompareTo(PartnerId.Length);

            if (_Result == 0)
                _Result = String.Compare(InternalId, PartnerId.InternalId, StringComparison.Ordinal);

            return _Result;

        }

        #endregion

        #endregion

        #region IEquatable<PartnerId> Members

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

            if (!(Object is Station_Id))
                return false;

            return Equals((Station_Id) Object);

        }

        #endregion

        #region Equals(PartnerId)

        /// <summary>
        /// Compares two PartnerIds for equality.
        /// </summary>
        /// <param name="PartnerId">A charging station identification to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(Station_Id PartnerId)
        {

            if ((Object) PartnerId == null)
                return false;

            return InternalId.Equals(PartnerId.InternalId);

        }

        #endregion

        #endregion

        #region GetHashCode()

        /// <summary>
        /// Return the HashCode of this object.
        /// </summary>
        /// <returns>The HashCode of this object.</returns>
        public override Int32 GetHashCode()
            => InternalId.GetHashCode();

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()
            => InternalId;

        #endregion

    }

}
