/*
 * Copyright (c) 2016-2021 GraphDefined GmbH
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
    /// The unique identification of an OIOI Payment Reference.
    /// </summary>
    public struct PaymentReference : IId,
                                     IEquatable<PaymentReference>,
                                     IComparable<PaymentReference>

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
        /// The length of the partner identificator.
        /// </summary>
        public UInt64 Length
            => (UInt64) InternalId.Length;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new payment reference based on the given string.
        /// </summary>
        /// <param name="Text">The value of the payment reference.</param>
        private PaymentReference(String Text)
        {
            InternalId = Text;
        }

        #endregion


        #region Parse   (Text)

        /// <summary>
        /// Parse the given string as a payment reference.
        /// </summary>
        /// <param name="Text">A text representation of a payment reference.</param>
        public static PaymentReference Parse(String Text)
        {

            if (TryParse(Text, out PaymentReference _PaymentReference))
                return _PaymentReference;

            throw new ArgumentException("The given text '" + Text + "' is not a valid text representation of a payment reference!", nameof(Text));

        }

        #endregion

        #region TryParse(Text)

        /// <summary>
        /// Try to parse the given string as a payment reference.
        /// </summary>
        /// <param name="Text">A text representation of a payment reference.</param>
        public static PaymentReference? TryParse(String Text)
        {

            if (TryParse(Text, out PaymentReference _PaymentReference))
                return _PaymentReference;

            return null;

        }

        #endregion

        #region TryParse(Text, out PaymentReference)

        /// <summary>
        /// Parse the given string as a payment reference.
        /// </summary>
        /// <param name="Text">A text representation of a payment reference.</param>
        /// <param name="PaymentReference">The parsed payment reference.</param>
        public static Boolean TryParse(String Text, out PaymentReference PaymentReference)
        {

            #region Initial checks

            if (Text != null)
                Text = Text.Trim();

            if (Text.IsNullOrEmpty())
            {
                PaymentReference = default(PaymentReference);
                return false;
            }

            #endregion

            try
            {

                PaymentReference = new PaymentReference(Text);

                return true;

            }

#pragma warning disable RCS1075  // Avoid empty catch clause that catches System.Exception.
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch (Exception)
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
#pragma warning restore RCS1075  // Avoid empty catch clause that catches System.Exception.
            { }

            PaymentReference = default(PaymentReference);
            return false;

        }

        #endregion

        #region Clone

        /// <summary>
        /// Clone this payment reference.
        /// </summary>
        public PaymentReference Clone

            => new PaymentReference(
                   new String(InternalId.ToCharArray())
               );

        #endregion


        #region Operator overloading

        #region Operator == (PaymentReference1, PaymentReference2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PaymentReference1">A payment reference.</param>
        /// <param name="PaymentReference2">Another payment reference.</param>
        /// <returns>true|false</returns>
        public static Boolean operator == (PaymentReference PaymentReference1, PaymentReference PaymentReference2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(PaymentReference1, PaymentReference2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) PaymentReference1 == null) || ((Object) PaymentReference2 == null))
                return false;

            return PaymentReference1.Equals(PaymentReference2);

        }

        #endregion

        #region Operator != (PaymentReference1, PaymentReference2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PaymentReference1">A payment reference.</param>
        /// <param name="PaymentReference2">Another payment reference.</param>
        /// <returns>true|false</returns>
        public static Boolean operator != (PaymentReference PaymentReference1, PaymentReference PaymentReference2)
            => !(PaymentReference1 == PaymentReference2);

        #endregion

        #region Operator <  (PaymentReference1, PaymentReference2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PaymentReference1">A payment reference.</param>
        /// <param name="PaymentReference2">Another payment reference.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (PaymentReference PaymentReference1, PaymentReference PaymentReference2)
        {

            if ((Object) PaymentReference1 == null)
                throw new ArgumentNullException(nameof(PaymentReference1), "The given PaymentReference1 must not be null!");

            return PaymentReference1.CompareTo(PaymentReference2) < 0;

        }

        #endregion

        #region Operator <= (PaymentReference1, PaymentReference2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PaymentReference1">A payment reference.</param>
        /// <param name="PaymentReference2">Another payment reference.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (PaymentReference PaymentReference1, PaymentReference PaymentReference2)
            => !(PaymentReference1 > PaymentReference2);

        #endregion

        #region Operator >  (PaymentReference1, PaymentReference2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PaymentReference1">A payment reference.</param>
        /// <param name="PaymentReference2">Another payment reference.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (PaymentReference PaymentReference1, PaymentReference PaymentReference2)
        {

            if ((Object) PaymentReference1 == null)
                throw new ArgumentNullException(nameof(PaymentReference1), "The given PaymentReference1 must not be null!");

            return PaymentReference1.CompareTo(PaymentReference2) > 0;

        }

        #endregion

        #region Operator >= (PaymentReference1, PaymentReference2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PaymentReference1">A payment reference.</param>
        /// <param name="PaymentReference2">Another payment reference.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (PaymentReference PaymentReference1, PaymentReference PaymentReference2)
            => !(PaymentReference1 < PaymentReference2);

        #endregion

        #endregion

        #region IComparable<PaymentReference> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            if (!(Object is PaymentReference))
                throw new ArgumentException("The given object is not a payment reference!",
                                            nameof(Object));

            return CompareTo((PaymentReference) Object);

        }

        #endregion

        #region CompareTo(PaymentReference)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PaymentReference">An object to compare with.</param>
        public Int32 CompareTo(PaymentReference PaymentReference)
        {

            if ((Object) PaymentReference == null)
                throw new ArgumentNullException(nameof(PaymentReference),  "The given payment reference must not be null!");

            // Compare the length of the PaymentReferences
            var _Result = this.Length.CompareTo(PaymentReference.Length);

            if (_Result == 0)
                _Result = String.Compare(InternalId, PaymentReference.InternalId, StringComparison.Ordinal);

            return _Result;

        }

        #endregion

        #endregion

        #region IEquatable<PaymentReference> Members

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

            if (!(Object is PaymentReference))
                return false;

            return Equals((PaymentReference) Object);

        }

        #endregion

        #region Equals(PaymentReference)

        /// <summary>
        /// Compares two PaymentReferences for equality.
        /// </summary>
        /// <param name="PaymentReference">A payment reference to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(PaymentReference PaymentReference)
        {

            if ((Object) PaymentReference == null)
                return false;

            return InternalId.Equals(PaymentReference.InternalId);

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
