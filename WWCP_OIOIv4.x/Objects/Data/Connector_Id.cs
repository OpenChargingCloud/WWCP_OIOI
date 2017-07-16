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
using System.Text.RegularExpressions;

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// The unique identification of a connector.
    /// </summary>
    public struct Connector_Id : IId,
                                 IEquatable<Connector_Id>,
                                 IComparable<Connector_Id>

    {

        #region Data

        //ToDo: Replace with better randomness!
        private static readonly Random _Random = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// The regular expression for parsing a connector identification.
        /// </summary>
        public static readonly Regex  ConnectorId_RegEx = new Regex(@"^([A-Za-z]{2}\*?[A-Za-z0-9]{3})\*?E([A-Za-z0-9\*]{1,30})$",
                                                                    RegexOptions.IgnorePatternWhitespace);

        #endregion

        #region Properties

        /// <summary>
        /// The charging station operator identification.
        /// </summary>
        public ChargingStationOperator_Id  OperatorId   { get; }

        /// <summary>
        /// The suffix of the identification.
        /// </summary>
        public String                      Suffix       { get; }

        /// <summary>
        /// Returns the length of the identification.
        /// </summary>
        public UInt64 Length
            => OperatorId.Length + 2 + (UInt64) Suffix.Length;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Generate a new connector identification based on the given
        /// charging station operator and identification suffix.
        /// </summary>
        /// <param name="OperatorId">The unique identification of a charging station operator.</param>
        /// <param name="Suffix">The suffix of the connector identification.</param>
        private Connector_Id(ChargingStationOperator_Id  OperatorId,
                             String                      Suffix)
        {

            #region Initial checks

            if (Suffix.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Suffix),  "The connector identification suffix must not be null or empty!");

            #endregion

            this.OperatorId  = OperatorId;
            this.Suffix      = Suffix;

        }

        #endregion


        #region Random(OperatorId, Mapper = null)

        /// <summary>
        /// Generate a new unique identification of a connector.
        /// </summary>
        /// <param name="OperatorId">The unique identification of a charging station operator.</param>
        /// <param name="Mapper">A delegate to modify the newly generated connector identification.</param>
        public static Connector_Id Random(ChargingStationOperator_Id  OperatorId,
                                          Func<String, String>        Mapper  = null)


            => new Connector_Id(OperatorId,
                                Mapper != null ? Mapper(_Random.RandomString(12)) : _Random.RandomString(12));

        #endregion

        #region Parse(Text)

        /// <summary>
        /// Parse the given string as a connector identification.
        /// </summary>
        /// <param name="Text">A text representation of a connector identification.</param>
        public static Connector_Id Parse(String Text)
        {

            #region Initial checks

            if (Text.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Text), "The given text representation of a connector identification must not be null or empty!");

            #endregion

            var MatchCollection = ConnectorId_RegEx.Matches(Text);

            if (MatchCollection.Count != 1)
                throw new ArgumentException("Illegal text representation of a connector identification: '" + Text + "'!",
                                            nameof(Text));

            ChargingStationOperator_Id _OperatorId;

            if (ChargingStationOperator_Id.TryParse(MatchCollection[0].Groups[1].Value, out _OperatorId))
                return new Connector_Id(_OperatorId,
                                        MatchCollection[0].Groups[2].Value);

            throw new ArgumentException("Illegal connector identification '" + Text + "'!",
                                        nameof(Text));

        }

        #endregion

        #region Parse(OperatorId, Suffix)

        /// <summary>
        /// Parse the given string as a connector identification.
        /// </summary>
        /// <param name="OperatorId">The unique identification of a charging station operator.</param>
        /// <param name="Suffix">The suffix of the connector identification.</param>
        public static Connector_Id Parse(ChargingStationOperator_Id  OperatorId,
                                         String                      Suffix)
        {

            if (!ConnectorId_RegEx.IsMatch(OperatorId.ToString() + "*E" + Suffix))
                    throw new ArgumentException("Illegal connector identification suffix '" + Suffix + "'!",
                                                nameof(Suffix));

            return new Connector_Id(OperatorId,
                                    Suffix);

        }

        #endregion

        #region TryParse(Text, out Connector_Id)

        /// <summary>
        /// Parse the given string as a connector identification.
        /// </summary>
        public static Boolean TryParse(String Text, out Connector_Id ConnectorId)
        {

            #region Initial checks

            if (Text.IsNullOrEmpty())
            {
                ConnectorId = default(Connector_Id);
                return false;
            }

            #endregion

            try
            {

                ConnectorId = default(Connector_Id);

                var _MatchCollection = ConnectorId_RegEx.Matches(Text);

                if (_MatchCollection.Count != 1)
                    return false;

                ChargingStationOperator_Id _OperatorId;

                // New format...
                if (ChargingStationOperator_Id.TryParse(_MatchCollection[0].Groups[1].Value, out _OperatorId))
                {

                    ConnectorId = new Connector_Id(_OperatorId,
                                                   _MatchCollection[0].Groups[2].Value);

                    return true;

                }

            }
#pragma warning disable RCS1075  // Avoid empty catch clause that catches System.Exception.
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch (Exception e)
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
#pragma warning restore RCS1075  // Avoid empty catch clause that catches System.Exception.
            { }

            ConnectorId = default(Connector_Id);
            return false;

        }

        #endregion

        #region Clone

        /// <summary>
        /// Clone this connector identification.
        /// </summary>
        public Connector_Id Clone

            => new Connector_Id(OperatorId.Clone,
                                new String(Suffix.ToCharArray()));

        #endregion


        #region Replace(Old, New)

        /// <summary>
        /// Returns a new connector identification in which all occurrences of the
        /// specified string value are replaced with the new value.
        /// </summary>
        /// <param name="OldValue">The string to be replaced.</param>
        /// <param name="NewValue">The new string value.</param>
        public Connector_Id Replace(String  OldValue,
                                    String  NewValue)

            => Parse(ToString().Replace(OldValue, NewValue));

        #endregion


        #region Operator overloading

        #region Operator == (ConnectorId1, ConnectorId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorId1">A connector identification.</param>
        /// <param name="ConnectorId2">Another connector identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator == (Connector_Id ConnectorId1, Connector_Id ConnectorId2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(ConnectorId1, ConnectorId2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) ConnectorId1 == null) || ((Object) ConnectorId2 == null))
                return false;

            return ConnectorId1.Equals(ConnectorId2);

        }

        #endregion

        #region Operator != (ConnectorId1, ConnectorId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorId1">A connector identification.</param>
        /// <param name="ConnectorId2">Another connector identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator != (Connector_Id ConnectorId1, Connector_Id ConnectorId2)
            => !(ConnectorId1 == ConnectorId2);

        #endregion

        #region Operator <  (ConnectorId1, ConnectorId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorId1">A connector identification.</param>
        /// <param name="ConnectorId2">Another connector identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (Connector_Id ConnectorId1, Connector_Id ConnectorId2)
        {

            if ((Object) ConnectorId1 == null)
                throw new ArgumentNullException(nameof(ConnectorId1), "The given ConnectorId1 must not be null!");

            return ConnectorId1.CompareTo(ConnectorId2) < 0;

        }

        #endregion

        #region Operator <= (ConnectorId1, ConnectorId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorId1">A connector identification.</param>
        /// <param name="ConnectorId2">Another connector identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (Connector_Id ConnectorId1, Connector_Id ConnectorId2)
            => !(ConnectorId1 > ConnectorId2);

        #endregion

        #region Operator >  (ConnectorId1, ConnectorId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorId1">A connector identification.</param>
        /// <param name="ConnectorId2">Another connector identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (Connector_Id ConnectorId1, Connector_Id ConnectorId2)
        {

            if ((Object) ConnectorId1 == null)
                throw new ArgumentNullException(nameof(ConnectorId1), "The given ConnectorId1 must not be null!");

            return ConnectorId1.CompareTo(ConnectorId2) > 0;

        }

        #endregion

        #region Operator >= (ConnectorId1, ConnectorId2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="ConnectorId1">A connector identification.</param>
        /// <param name="ConnectorId2">Another connector identification.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (Connector_Id ConnectorId1, Connector_Id ConnectorId2)
            => !(ConnectorId1 < ConnectorId2);

        #endregion

        #endregion

        #region IComparable<EVSEId> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            if (!(Object is Connector_Id))
                throw new ArgumentException("The given object is not a connector identification!");

            return CompareTo((Connector_Id) Object);

        }

        #endregion

        #region CompareTo(EVSEId)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="EVSEId">An object to compare with.</param>
        public Int32 CompareTo(Connector_Id EVSEId)
        {

            if ((Object) EVSEId == null)
                throw new ArgumentNullException(nameof(EVSEId),  "The given connector identification must not be null!");

            // Compare the length of the identifications
            var _Result = this.Length.CompareTo(EVSEId.Length);

            // If equal: Compare charging operator identifications
            if (_Result == 0)
                _Result = OperatorId.CompareTo(EVSEId.OperatorId);

            // If equal: Compare suffix
            if (_Result == 0)
                _Result = String.Compare(Suffix, EVSEId.Suffix, StringComparison.Ordinal);

            return _Result;

        }

        #endregion

        #endregion

        #region IEquatable<EVSEId> Members

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

            if (!(Object is Connector_Id))
                return false;

            return Equals((Connector_Id) Object);

        }

        #endregion

        #region Equals(EVSEId)

        /// <summary>
        /// Compares two connector identifications for equality.
        /// </summary>
        /// <param name="EVSEId">An connector identification to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(Connector_Id EVSEId)
        {

            if ((Object) EVSEId == null)
                return false;

            return OperatorId.Equals(EVSEId.OperatorId) &&
                   Suffix.    Equals(EVSEId.Suffix);

        }

        #endregion

        #endregion

        #region (override) GetHashCode()

        /// <summary>
        /// Return the HashCode of this object.
        /// </summary>
        /// <returns>The HashCode of this object.</returns>
        public override Int32 GetHashCode()

            => OperatorId.GetHashCode() ^
               Suffix.    GetHashCode();

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// ISO-IEC-15118 – Annex H "Specification of Identifiers"
        /// </summary>
        public override String ToString()
            => String.Concat(OperatorId, "*E", Suffix);

        #endregion

    }

}
