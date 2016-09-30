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

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    /// <summary>
    /// An OIOI address.
    /// </summary>
    public class Address : IEquatable<Address>
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
        /// The city.
        /// </summary>
        public String   City           { get; }

        /// <summary>
        /// The postal code.
        /// </summary>
        public String   ZIP            { get; }

        /// <summary>
        /// The city.
        /// </summary>
        public Country  Country        { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new minimal address.
        /// </summary>
        /// <param name="Street">The name of the street.</param>
        /// <param name="StreetNumber">The street number.</param>
        /// <param name="City">The city.</param>
        /// <param name="ZIP">The postal code</param>
        /// <param name="Country">The country.</param>
        public Address(String   Street,
                       String   StreetNumber,
                       String   City,
                       String   ZIP,
                       Country  Country)
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
        // },

        #endregion

        #region (static) Parse(AddressJSON)

        /// <summary>
        /// Parse the given JSON representation of an OIOI address.
        /// </summary>
        /// <param name="AddressJSON">The JSON to parse.</param>
        public static Address Parse(JObject AddressJSON)
        {

            Address _Address;

            if (TryParse(AddressJSON, out _Address))
                return _Address;

            return null;

        }

        #endregion

        #region (static) Parse(AddressText)

        /// <summary>
        /// Parse the given text representation of an OIOI address.
        /// </summary>
        /// <param name="AddressText">The text to parse.</param>
        public static Address Parse(String AddressText)
        {

            Address _Address;

            if (TryParse(AddressText, out _Address))
                return _Address;

            return null;

        }

        #endregion

        #region (static) TryParse(AddressText, out Address, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI address.
        /// </summary>
        /// <param name="AddressText">The text to parse.</param>
        /// <param name="Address">The parsed address.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String               AddressText,
                                       out Address          Address,
                                       OnExceptionDelegate  OnException  = null)
        {

            try
            {

                return TryParse(JObject.Parse(AddressText),
                                out Address,
                                OnException);

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, AddressText, e);

                Address = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(AddressJSON, out Address, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI Address.
        /// </summary>
        /// <param name="AddressJSON">The JSON to parse.</param>
        /// <param name="Address">The parsed Address.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject              AddressJSON,
                                       out Address          Address,
                                       OnExceptionDelegate  OnException  = null)
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

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, AddressJSON, e);

                Address = null;
                return false;

            }

        }

        #endregion

        #region ToJSON()

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        public JObject ToJSON()

            => JSONObject.Create(
                   new JProperty("street",         Street),
                   new JProperty("street-number",  StreetNumber),
                   new JProperty("city",           City),
                   new JProperty("zip",            ZIP),
                   new JProperty("country",        Country.Alpha2Code)
               );

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
            if (Object.ReferenceEquals(Address1, Address2))
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
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Street, " ", StreetNumber, ", ",
                             ZIP,    " ", City,         ", ",
                             Country.CountryName.FirstText);

        #endregion

    }

}
