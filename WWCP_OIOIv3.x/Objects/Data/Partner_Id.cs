/*
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

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    /// <summary>
    /// The unique identification of an OIOI Communication Partner.
    /// </summary>
    public class Partner_Id : IId,
                              IEquatable<Partner_Id>,
                              IComparable<Partner_Id>

    {

        #region Data

        /// <summary>
        /// The internal identification.
        /// </summary>
        protected readonly String _Id;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the length of the identification.
        /// </summary>
        public UInt64 Length
            => (UInt64)_Id.Length;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Generate a new OIOI communication partner identification
        /// based on the given string.
        /// </summary>
        /// <param name="Text">The value of the communication partner identification.</param>
        private Partner_Id(String Text)
        {

            #region Initial checks

            if (Text.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Text),  "The given communication partner identification must not be null or empty!");

            #endregion

            this._Id = Text;

        }

        #endregion


        #region Parse(Text)

        /// <summary>
        /// Parse the given string as a communication partner identification.
        /// </summary>
        /// <param name="Text">A text representation of a communication partner identification.</param>
        public static Partner_Id Parse(String Text)

            => new Partner_Id(Text);

        #endregion

        #region TryParse(Text, out PartnerId)

        /// <summary>
        /// Parse the given string as a communication partner identification.
        /// </summary>
        /// <param name="Text">A text representation of a communication partner identification.</param>
        /// <param name="PartnerId">The parsed communication partner identification.</param>
        public static Boolean TryParse(String Text, out Partner_Id PartnerId)
        {
            try
            {
                PartnerId = new Partner_Id(Text);
                return true;
            }
            catch (Exception)
            {
                PartnerId = null;
                return false;
            }
        }

        #endregion

        #region Clone

        /// <summary>
        /// Clone this communication partner identification.
        /// </summary>
        public Partner_Id Clone

            => new Partner_Id(_Id);

        #endregion


        #region Operator overloading

        #region Operator == (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A communication partner identification.</param>
        /// <param name="PartnerId2">Another communication partner identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator == (Partner_Id PartnerId1, Partner_Id PartnerId2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(PartnerId1, PartnerId2))
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
        /// <param name="PartnerId1">A communication partner identification.</param>
        /// <param name="PartnerId2">Another communication partner identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator != (Partner_Id PartnerId1, Partner_Id PartnerId2)

            => !(PartnerId1 == PartnerId2);

        #endregion

        #region Operator <  (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A communication partner identification.</param>
        /// <param name="PartnerId2">Another communication partner identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (Partner_Id PartnerId1, Partner_Id PartnerId2)
        {

            if ((Object) PartnerId1 == null)
                throw new ArgumentNullException(nameof(PartnerId1),  "The given communication partner identification must not be null!");

            return PartnerId1.CompareTo(PartnerId2) < 0;

        }

        #endregion

        #region Operator <= (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A communication partner identification.</param>
        /// <param name="PartnerId2">Another communication partner identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (Partner_Id PartnerId1, Partner_Id PartnerId2)

            => !(PartnerId1 > PartnerId2);

        #endregion

        #region Operator >  (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A communication partner identification.</param>
        /// <param name="PartnerId2">Another communication partner identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (Partner_Id PartnerId1, Partner_Id PartnerId2)
        {

            if ((Object) PartnerId1 == null)
                throw new ArgumentNullException(nameof(PartnerId1),  "The given communication partner identification must not be null!");

            return PartnerId1.CompareTo(PartnerId2) > 0;

        }

        #endregion

        #region Operator >= (PartnerId1, PartnerId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId1">A PartnerId.</param>
        /// <param name="PartnerId2">Another PartnerId.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (Partner_Id PartnerId1, Partner_Id PartnerId2)

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
                throw new ArgumentNullException(nameof(Object),  "The given object must not be null!");

            // Check if the given object is a communication partner identification.
            var PartnerId = Object as Partner_Id;
            if ((Object) PartnerId == null)
                throw new ArgumentException("The given object is not a communication partner identification!", nameof(Object));

            return CompareTo(PartnerId);

        }

        #endregion

        #region CompareTo(PartnerId)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PartnerId">An object to compare with.</param>
        public Int32 CompareTo(Partner_Id PartnerId)
        {

            if ((Object) PartnerId == null)
                throw new ArgumentNullException(nameof(PartnerId),  "The given communication partner identification must not be null!");

            // Compare the length of the communication partner identifications
            var _Result = this.Length.CompareTo(PartnerId.Length);

            // If equal: Compare communication partner identifications
            if (_Result == 0)
                _Result = String.Compare(_Id, PartnerId._Id, StringComparison.Ordinal);

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

            // Check if the given object is a communication partner identification.
            var PartnerId = Object as Partner_Id;
            if ((Object) PartnerId == null)
                return false;

            return this.Equals(PartnerId);

        }

        #endregion

        #region Equals(PartnerId)

        /// <summary>
        /// Compares two communication partner identifications for equality.
        /// </summary>
        /// <param name="PartnerId">A communication partner identification to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(Partner_Id PartnerId)
        {

            if ((Object) PartnerId == null)
                return false;

            return _Id.Equals(PartnerId._Id);

        }

        #endregion

        #endregion

        #region GetHashCode()

        /// <summary>
        /// Return the HashCode of this object.
        /// </summary>
        /// <returns>The HashCode of this object.</returns>
        public override Int32 GetHashCode()

            => _Id.GetHashCode();

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => _Id;

        #endregion

    }

}
