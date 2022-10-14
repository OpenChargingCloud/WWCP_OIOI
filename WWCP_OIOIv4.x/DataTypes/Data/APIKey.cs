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

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x
{

    /// <summary>
    /// An API key.
    /// </summary>
    public readonly struct APIKey : IId,
                                    IEquatable<APIKey>,
                                    IComparable<APIKey>

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
        /// The length of the clearing house identificator.
        /// </summary>
        public UInt64 Length
            => (UInt64) InternalId?.Length;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new API key based on the given string.
        /// </summary>
        private APIKey(String Text)
        {
            InternalId = Text;
        }

        #endregion


        #region Parse   (Text)

        /// <summary>
        /// Parse the given string as an API key.
        /// </summary>
        /// <param name="Text">A text-representation of an API key.</param>
        public static APIKey Parse(String Text)
        {

            #region Initial checks

            if (Text != null)
                Text = Text.Trim();

            if (Text.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Text), "The given text-representation of an API key must not be null or empty!");

            #endregion

            if (TryParse(Text, out APIKey apiKey))
                return apiKey;

            throw new ArgumentException("Invalid text-representation of an API key: '" + Text + "'!", nameof(Text));

        }

        #endregion

        #region TryParse(Text)

        /// <summary>
        /// Try to parse the given string as an API key.
        /// </summary>
        /// <param name="Text">A text-representation of an API key.</param>
        public static APIKey? TryParse(String Text)
        {

            if (TryParse(Text, out APIKey apiKey))
                return apiKey;

            return default;

        }

        #endregion

        #region TryParse(Text, out APIKey)

        /// <summary>
        /// Try to parse the given string as an API key.
        /// </summary>
        /// <param name="Text">A text-representation of an API key.</param>
        /// <param name="APIKey">The parsed API key.</param>
        public static Boolean TryParse(String Text, out APIKey APIKey)
        {

            #region Initial checks

            if (Text != null)
                Text = Text.Trim();

            if (Text.IsNullOrEmpty())
            {
                APIKey = default;
                return false;
            }

            #endregion

            try
            {
                APIKey = new APIKey(Text);
                return true;
            }

#pragma warning disable RCS1075  // Avoid empty catch clause that catches System.Exception.
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch (Exception)
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
#pragma warning restore RCS1075  // Avoid empty catch clause that catches System.Exception.
            { }

            APIKey = default;
            return false;

        }

        #endregion

        #region Clone

        /// <summary>
        /// Clone this API key.
        /// </summary>
        public APIKey Clone

            => new APIKey(
                   new String(InternalId.ToCharArray())
               );

        #endregion


        #region Operator overloading

        #region Operator == (APIKeyId1, APIKeyId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="APIKeyId1">An API key.</param>
        /// <param name="APIKeyId2">Another API key.</param>
        /// <returns>true|false</returns>
        public static Boolean operator == (APIKey APIKeyId1,
                                           APIKey APIKeyId2)

            => APIKeyId1.Equals(APIKeyId2);

        #endregion

        #region Operator != (APIKeyId1, APIKeyId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="APIKeyId1">An API key.</param>
        /// <param name="APIKeyId2">Another API key.</param>
        /// <returns>true|false</returns>
        public static Boolean operator != (APIKey APIKeyId1,
                                           APIKey APIKeyId2)

            => !APIKeyId1.Equals(APIKeyId2);

        #endregion

        #region Operator <  (APIKeyId1, APIKeyId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="APIKeyId1">An API key.</param>
        /// <param name="APIKeyId2">Another API key.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (APIKey APIKeyId1,
                                          APIKey APIKeyId2)

            => APIKeyId1.CompareTo(APIKeyId2) < 0;

        #endregion

        #region Operator <= (APIKeyId1, APIKeyId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="APIKeyId1">An API key.</param>
        /// <param name="APIKeyId2">Another API key.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (APIKey APIKeyId1,
                                           APIKey APIKeyId2)

            => APIKeyId1.CompareTo(APIKeyId2) <= 0;

        #endregion

        #region Operator >  (APIKeyId1, APIKeyId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="APIKeyId1">An API key.</param>
        /// <param name="APIKeyId2">Another API key.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (APIKey APIKeyId1,
                                          APIKey APIKeyId2)

            => APIKeyId1.CompareTo(APIKeyId2) > 0;

        #endregion

        #region Operator >= (APIKeyId1, APIKeyId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="APIKeyId1">An API key.</param>
        /// <param name="APIKeyId2">Another API key.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (APIKey APIKeyId1,
                                           APIKey APIKeyId2)

            => APIKeyId1.CompareTo(APIKeyId2) >= 0;

        #endregion

        #endregion

        #region IComparable<APIKey> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)

            => Object is APIKey apiKey
                   ? CompareTo(apiKey)
                   : throw new ArgumentException("The given object is not an API key!",
                                                 nameof(Object));

        #endregion

        #region CompareTo(APIKey)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="APIKey">An object to compare with.</param>
        public Int32 CompareTo(APIKey APIKey)

            => String.Compare(InternalId,
                              APIKey.InternalId,
                              StringComparison.Ordinal);

        #endregion

        #endregion

        #region IEquatable<APIKey> Members

        #region Equals(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        /// <returns>true|false</returns>
        public override Boolean Equals(Object Object)

            => Object is APIKey apiKey &&
                   Equals(apiKey);

        #endregion

        #region Equals(APIKey)

        /// <summary>
        /// Compares two APIKeys for equality.
        /// </summary>
        /// <param name="APIKey">An API key to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(APIKey APIKey)

            => String.Equals(InternalId,
                             APIKey.InternalId,
                             StringComparison.Ordinal);

        #endregion

        #endregion

        #region GetHashCode()

        /// <summary>
        /// Return the hash code of this object.
        /// </summary>
        /// <returns>The hash code of this object.</returns>
        public override Int32 GetHashCode()

            => InternalId.GetHashCode();

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text-representation of this object.
        /// </summary>
        public override String ToString()

            => InternalId;

        #endregion

    }

}
