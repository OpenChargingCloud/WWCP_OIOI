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
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.JSON;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// An OIOI address.
    /// </summary>
    public class Address : ACustomData,
                           IEquatable<Address>,
                           IComparable<Address>,
                           IComparable
    {

        #region Properties

        /// <summary>
        /// The name of the street.
        /// </summary>
        public String   Street         { get; }

        /// <summary>
        /// The street number.
        /// </summary>
        public String   StreetNumber   { get; }

        /// <summary>
        /// The name of the city.
        /// </summary>
        public String   City           { get; }

        /// <summary>
        /// The postal code.
        /// </summary>
        public String   ZIP            { get; }

        /// <summary>
        /// The country.
        /// </summary>
        public Country  Country        { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new minimal address.
        /// </summary>
        /// <param name="Street">The name of the street.</param>
        /// <param name="StreetNumber">The street number.</param>
        /// <param name="City">The name of the city.</param>
        /// <param name="ZIP">The postal code</param>
        /// <param name="Country">The country.</param>
        /// 
        /// <param name="CustomData">An optional dictionary of customer-specific data.</param>
        public Address(String                               Street,
                       String                               StreetNumber,
                       String                               City,
                       String                               ZIP,
                       Country                              Country,

                       IReadOnlyDictionary<String, Object>  CustomData   = null)

            : base(CustomData)

        {

            this.Street         = Street;
            this.StreetNumber   = StreetNumber;
            this.City           = City;
            this.ZIP            = ZIP;
            this.Country        = Country;

        }

        #endregion


        #region Documentation

        // {
        //     "street":         "streetname",
        //     "street-number":  "123",
        //     "city":           "Berlin",
        //     "zip":            "10243",
        //     "country":        "DE"
        // }

        #endregion

        #region (static) Parse(AddressJSON, CustomAddressParser = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI address.
        /// </summary>
        /// <param name="AddressJSON">The JSON to parse.</param>
        /// <param name="CustomAddressParser">A delegate to parse custom Address JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Address Parse(JObject                            AddressJSON,
                                    CustomJObjectParserDelegate<Address>  CustomAddressParser   = null,
                                    OnExceptionDelegate                OnException           = null)
        {

            if (TryParse(AddressJSON,
                         out Address _Address,
                         CustomAddressParser,
                         OnException))

                return _Address;

            return null;

        }

        #endregion

        #region (static) Parse(AddressText, CustomAddressParser = null, OnException = null)

        /// <summary>
        /// Parse the given text representation of an OIOI address.
        /// </summary>
        /// <param name="AddressText">The text to parse.</param>
        /// <param name="CustomAddressParser">A delegate to parse custom Address JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Address Parse(String                             AddressText,
                                    CustomJObjectParserDelegate<Address>  CustomAddressParser   = null,
                                    OnExceptionDelegate                OnException           = null)
        {

            if (TryParse(AddressText,
                         out Address _Address,
                         CustomAddressParser,
                         OnException))

                return _Address;

            return null;

        }

        #endregion

        #region (static) TryParse(AddressText, out Address, CustomAddressParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI address.
        /// </summary>
        /// <param name="AddressText">The text to parse.</param>
        /// <param name="Address">The parsed address.</param>
        /// <param name="CustomAddressParser">A delegate to parse custom Address JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String                             AddressText,
                                       out Address                        Address,
                                       CustomJObjectParserDelegate<Address>  CustomAddressParser   = null,
                                       OnExceptionDelegate                OnException           = null)
        {

            try
            {

                return TryParse(JObject.Parse(AddressText),
                                out Address,
                                CustomAddressParser,
                                OnException);

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, AddressText, e);

                Address = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(AddressJSON, out Address, CustomAddressParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI Address.
        /// </summary>
        /// <param name="AddressJSON">The JSON to parse.</param>
        /// <param name="Address">The parsed address.</param>
        /// <param name="CustomAddressParser">A delegate to parse custom Address JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject                            AddressJSON,
                                       out Address                        Address,
                                       CustomJObjectParserDelegate<Address>  CustomAddressParser   = null,
                                       OnExceptionDelegate                OnException           = null)
        {

            try
            {

                Address = new Address(

                              AddressJSON.ValueOrDefault("street",         String.Empty).Value<String>().Trim(),
                              AddressJSON.ValueOrDefault("street-number",  String.Empty).Value<String>().Trim(),
                              AddressJSON.ValueOrDefault("city",           String.Empty).Value<String>().Trim(),
                              AddressJSON.ValueOrDefault("zip",            String.Empty).Value<String>().Trim(),

                              AddressJSON.MapValueOrFail("country",
                                                         value => Country.ParseAlpha2Code(value.Value<String>().Trim()),
                                                         "Invalid or missing JSON property 'country'!")
                          );


                if (CustomAddressParser != null)
                    Address = CustomAddressParser(AddressJSON,
                                                  Address);

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, AddressJSON, e);

                Address = null;
                return false;

            }

        }

        #endregion

        #region ToJSON(CustomAddressSerializer = null)

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        /// <param name="CustomAddressSerializer">A delegate to serialize custom Address JSON objects.</param>
        public JObject ToJSON(CustomJObjectSerializerDelegate<Address> CustomAddressSerializer  = null)
        {

            var JSON = JSONObject.Create(
                           new JProperty("street",         Street),
                           new JProperty("street-number",  StreetNumber),
                           new JProperty("city",           City),
                           new JProperty("zip",            ZIP),
                           new JProperty("country",        Country.Alpha2Code)
                       );

            return CustomAddressSerializer != null
                       ? CustomAddressSerializer(this, JSON)
                       : JSON;

        }

        #endregion


        #region Operator overloading

        #region Operator == (Address1, Address2)

        /// <summary>
        /// Compares two addresses for equality.
        /// </summary>
        /// <param name="Address1">An address.</param>
        /// <param name="Address2">Another address.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (Address Address1, Address Address2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(Address1, Address2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) Address1 == null) || ((Object) Address2 == null))
                return false;

            return Address1.Equals(Address2);

        }

        #endregion

        #region Operator != (Address1, Address2)

        /// <summary>
        /// Compares two addresses for inequality.
        /// </summary>
        /// <param name="Address1">An address.</param>
        /// <param name="Address2">Another address.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (Address Address1, Address Address2)

            => !(Address1 == Address2);

        #endregion

        #region Operator <  (Address1, Address2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Address1">An address.</param>
        /// <param name="Address2">Another address.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (Address Address1, Address Address2)
        {

            if ((Object) Address1 == null)
                throw new ArgumentNullException(nameof(Address1), "The given Address1 must not be null!");

            return Address1.CompareTo(Address2) < 0;

        }

        #endregion

        #region Operator <= (Address1, Address2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Address1">An address.</param>
        /// <param name="Address2">Another address.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (Address Address1, Address Address2)
            => !(Address1 > Address2);

        #endregion

        #region Operator >  (Address1, Address2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Address1">An address.</param>
        /// <param name="Address2">Another address.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (Address Address1, Address Address2)
        {

            if ((Object)Address1 == null)
                throw new ArgumentNullException(nameof(Address1), "The given Address1 must not be null!");

            return Address1.CompareTo(Address2) > 0;

        }

        #endregion

        #region Operator >= (Address1, Address2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Address1">An address.</param>
        /// <param name="Address2">Another address.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (Address Address1, Address Address2)
            => !(Address1 < Address2);

        #endregion

        #endregion

        #region IComparable<Address> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            var Address = Object as Address;
            if ((Object)Address == null)
                throw new ArgumentException("The given object is not an address identification!", nameof(Object));

            return CompareTo(Address);

        }

        #endregion

        #region CompareTo(Address)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Address">An object to compare with.</param>
        public Int32 CompareTo(Address Address)
        {

            if ((Object) Address == null)
                throw new ArgumentNullException(nameof(Address), "The given address must not be null!");

            var c = Country.     CompareTo(Address.Country);
            if (c != 0)
                return c;

            c = ZIP.             CompareTo(Address.ZIP);
            if (c != 0)
                return c;

            c = City.            CompareTo(Address.City);
            if (c != 0)
                return c;

            c = Street.          CompareTo(Address.Street);
            if (c != 0)
                return c;

            return StreetNumber. CompareTo(Address.StreetNumber);

        }

        #endregion

        #endregion

        #region IEquatable<Address> Members

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

            // Check if the given object is an address.
            var Address = Object as Address;
            if ((Object) Address == null)
                return false;

            return this.Equals(Address);

        }

        #endregion

        #region Equals(Address)

        /// <summary>
        /// Compares two addresses for equality.
        /// </summary>
        /// <param name="Address">An address to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(Address Address)
        {

            if ((Object) Address == null)
                return false;

            return Street.       Equals(Address.Street)       &&
                   StreetNumber. Equals(Address.StreetNumber) &&
                   City.         Equals(Address.City)         &&
                   ZIP.          Equals(Address.ZIP)          &&
                   Country.      Equals(Address.Country);

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

                return Street.       GetHashCode() * 31 ^
                       StreetNumber. GetHashCode() * 23 ^
                       City.         GetHashCode() * 17 ^
                       ZIP.          GetHashCode() * 11 ^
                       Country.      GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Street, " ", StreetNumber, ", ",
                             ZIP,    " ", City,         ", ",
                             Country.CountryName.FirstText());

        #endregion

    }

}
