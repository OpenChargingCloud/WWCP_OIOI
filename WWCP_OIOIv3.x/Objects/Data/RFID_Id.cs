﻿/*
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

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    /// <summary>
    /// A RFID identificator.
    /// </summary>
    public struct RFID_Id : IId,
                            IEquatable<RFID_Id>,
                            IComparable<RFID_Id>
    {

        #region Data

        /// <summary>
        /// The internal identification.
        /// </summary>
        private readonly String InternalId;

        #endregion

        #region Properties

        /// <summary>
        /// The length of the RFID identificator.
        /// </summary>
        public UInt64 Length
            => (UInt64)InternalId.Length;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Generate a new RFID identificator based on the given string.
        /// </summary>
        /// <param name="Text">The value of the RFID identificator.</param>
        private RFID_Id(String Text)
        {
            InternalId = Text;
        }

        #endregion


        #region Parse(Text)

        /// <summary>
        /// Parse the given string as a RFID identificator.
        /// </summary>
        /// <param name="Text">A text representation of a RFID identificator.</param>
        public static RFID_Id Parse(String Text)
        {

            if (Text.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Text), "The given text representation of a RFID identificator must not be null!");

            Text = Text.Trim();

            if (Text.Length == 8 ||
                Text.Length == 14 ||
                Text.Length == 20)
            {

                RFID_Id _RFIDId = default(RFID_Id);

                if (TryParse(Text, out _RFIDId))
                    return _RFIDId;

                throw new ArgumentException("Invalid RFID identificator!", nameof(Text));

            }

            throw new ArgumentException("Invalid length of the RFID identificator (only 8, 14, 20 chars allowed)!", nameof(Text));

        }

        #endregion

        #region TryParse(Text, out Token)

        /// <summary>
        /// Parse the given string as a RFID identificator.
        /// </summary>
        /// <param name="Text">A text representation of a RFID identificator.</param>
        /// <param name="Token">The parsed RFID identificator.</param>
        public static Boolean TryParse(String Text, out RFID_Id Token)
        {

            if (Text.IsNullOrEmpty())
            {
                Token = default(RFID_Id);
                return false;
            }

            Text = Text.Trim().ToUpper();

            if (Text.Length ==  8 ||
                Text.Length == 14 ||
                Text.Length == 20)
            {

                try
                {
                    Token = new RFID_Id(Text);
                    return true;
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
#pragma warning disable RCS1075  // Avoid empty catch clause that catches System.Exception.
                catch (Exception)
#pragma warning restore RCS1075  // Avoid empty catch clause that catches System.Exception.
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                { }

            }

            Token = default(RFID_Id);
            return false;

        }

        #endregion

        #region Clone

        /// <summary>
        /// Clone this RFID identificator.
        /// </summary>
        public RFID_Id Clone
            => new RFID_Id(new String(InternalId.ToCharArray()));

        #endregion


        #region Operator overloading

        #region Operator == (TokenId1, TokenId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="TokenId1">An RFID identificator.</param>
        /// <param name="TokenId2">Another RFID identificator.</param>
        /// <returns>true|false</returns>
        public static Boolean operator == (RFID_Id TokenId1, RFID_Id TokenId2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(TokenId1, TokenId2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) TokenId1 == null) || ((Object) TokenId2 == null))
                return false;

            return TokenId1.Equals(TokenId2);

        }

        #endregion

        #region Operator != (TokenId1, TokenId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="TokenId1">An RFID identificator.</param>
        /// <param name="TokenId2">Another RFID identificator.</param>
        /// <returns>true|false</returns>
        public static Boolean operator != (RFID_Id TokenId1, RFID_Id TokenId2)
            => !(TokenId1 == TokenId2);

        #endregion

        #region Operator <  (TokenId1, TokenId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="TokenId1">An RFID identificator.</param>
        /// <param name="TokenId2">Another RFID identificator.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (RFID_Id TokenId1, RFID_Id TokenId2)
        {

            if ((Object) TokenId1 == null)
                throw new ArgumentNullException(nameof(TokenId1),  "The given RFID identificator must not be null!");

            return TokenId1.CompareTo(TokenId2) < 0;

        }

        #endregion

        #region Operator <= (TokenId1, TokenId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="TokenId1">An RFID identificator.</param>
        /// <param name="TokenId2">Another RFID identificator.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (RFID_Id TokenId1, RFID_Id TokenId2)
            => !(TokenId1 > TokenId2);

        #endregion

        #region Operator >  (TokenId1, TokenId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="TokenId1">An RFID identificator.</param>
        /// <param name="TokenId2">Another RFID identificator.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (RFID_Id TokenId1, RFID_Id TokenId2)
        {

            if ((Object) TokenId1 == null)
                throw new ArgumentNullException(nameof(TokenId1),  "The given RFID identificator must not be null!");

            return TokenId1.CompareTo(TokenId2) > 0;

        }

        #endregion

        #region Operator >= (TokenId1, TokenId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="TokenId1">An RFID identificator.</param>
        /// <param name="TokenId2">Another RFID identificator.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (RFID_Id TokenId1, RFID_Id TokenId2)
            => !(TokenId1 < TokenId2);

        #endregion

        #endregion

        #region IComparable<RFIDId> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            if (!(Object is RFID_Id))
                throw new ArgumentException("The given object is not a RFID identification!",
                                            nameof(Object));

            return CompareTo((RFID_Id) Object);

        }

        #endregion

        #region CompareTo(RFIDId)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="RFIDId">An object to compare with.</param>
        public Int32 CompareTo(RFID_Id RFIDId)
        {

            if ((Object) RFIDId == null)
                throw new ArgumentNullException(nameof(RFIDId),  "The given RFID identificator must not be null!");

            return String.Compare(InternalId, RFIDId.InternalId, StringComparison.Ordinal);

        }

        #endregion

        #endregion

        #region IEquatable<RFIDId> Members

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

            if (!(Object is RFID_Id))
                return false;

            return Equals((RFID_Id) Object);

        }

        #endregion

        #region Equals(Token)

        /// <summary>
        /// Compares two RFID identificators for equality.
        /// </summary>
        /// <param name="RFIDId">An RFID identificator to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(RFID_Id RFIDId)
        {

            if ((Object) RFIDId == null)
                return false;

            return InternalId.Equals(RFIDId.InternalId);

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
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()
            => InternalId;

        #endregion

    }

}