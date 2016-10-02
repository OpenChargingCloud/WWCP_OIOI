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
    /// The unique identification of an OIOI Payment Reference.
    /// </summary>
    public class PaymentReference : IId,
                                    IEquatable<PaymentReference>,
                                    IComparable<PaymentReference>

    {

        #region Data

        /// <summary>
        /// The internal identification.
        /// </summary>
        protected readonly String _Id;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the length of the identificator.
        /// </summary>
        public UInt64 Length
            => (UInt64)_Id.Length;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Generate a new OIOI communication payment reference
        /// based on the given string.
        /// </summary>
        /// <param name="Text">The value of the payment reference.</param>
        private PaymentReference(String Text)
        {

            #region Initial checks

            if (Text.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Text),  "The given payment reference must not be null or empty!");

            #endregion

            this._Id = Text;

        }

        #endregion


        #region Parse(Text)

        /// <summary>
        /// Parse the given string as a payment reference.
        /// </summary>
        /// <param name="Text">A text representation of a payment reference.</param>
        public static PaymentReference Parse(String Text)

            => new PaymentReference(Text);

        #endregion

        #region TryParse(Text, out PaymentReference)

        /// <summary>
        /// Parse the given string as a payment reference.
        /// </summary>
        /// <param name="Text">A text representation of a payment reference.</param>
        /// <param name="PaymentReference">The parsed payment reference.</param>
        public static Boolean TryParse(String Text, out PaymentReference PaymentReference)
        {
            try
            {
                PaymentReference = new PaymentReference(Text);
                return true;
            }
            catch (Exception)
            {
                PaymentReference = null;
                return false;
            }
        }

        #endregion

        #region Clone

        /// <summary>
        /// Clone this payment reference.
        /// </summary>
        public PaymentReference Clone

            => new PaymentReference(_Id);

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
            if (Object.ReferenceEquals(PaymentReference1, PaymentReference2))
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
                throw new ArgumentNullException(nameof(PaymentReference1),  "The given payment reference must not be null!");

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
                throw new ArgumentNullException(nameof(PaymentReference1),  "The given payment reference must not be null!");

            return PaymentReference1.CompareTo(PaymentReference2) > 0;

        }

        #endregion

        #region Operator >= (PaymentReference1, PaymentReference2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="PaymentReference1">A PaymentReference.</param>
        /// <param name="PaymentReference2">Another PaymentReference.</param>
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
                throw new ArgumentNullException(nameof(Object),  "The given object must not be null!");

            // Check if the given object is a payment reference.
            var PaymentReference = Object as PaymentReference;
            if ((Object) PaymentReference == null)
                throw new ArgumentException("The given object is not a payment reference!", nameof(Object));

            return CompareTo(PaymentReference);

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

            // Compare the length of the payment references
            var _Result = this.Length.CompareTo(PaymentReference.Length);

            // If equal: Compare payment references
            if (_Result == 0)
                _Result = String.Compare(_Id, PaymentReference._Id, StringComparison.Ordinal);

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

            // Check if the given object is a payment reference.
            var PaymentReference = Object as PaymentReference;
            if ((Object) PaymentReference == null)
                return false;

            return this.Equals(PaymentReference);

        }

        #endregion

        #region Equals(PaymentReference)

        /// <summary>
        /// Compares two payment references for equality.
        /// </summary>
        /// <param name="PaymentReference">A payment reference to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(PaymentReference PaymentReference)
        {

            if ((Object) PaymentReference == null)
                return false;

            return _Id.Equals(PaymentReference._Id);

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
