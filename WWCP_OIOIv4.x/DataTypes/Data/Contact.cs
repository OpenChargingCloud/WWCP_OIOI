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

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.JSON;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// An OIOI contact.
    /// </summary>
    public class Contact : IEquatable<Contact>
    {

        #region Properties

        /// <summary>
        /// A phone number.
        /// </summary>
        public String Phone     { get; }

        /// <summary>
        /// A fax number.
        /// </summary>
        public String Fax       { get; }

        /// <summary>
        /// An URI.
        /// </summary>
        public String Web       { get; }

        /// <summary>
        /// An e-mail address.
        /// </summary>
        public String EMail     { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new contact.
        /// </summary>
        /// <param name="Phone">A phone number.</param>
        /// <param name="Fax">An optional fax number.</param>
        /// <param name="Web">An optional URI.</param>
        /// <param name="EMail">An optional e-mail address.</param>
        public Contact(String  Phone,
                       String  Fax    = null,
                       String  Web    = null,
                       String  EMail  = null)
        {

            #region Initial checks

            if (Phone.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Phone), "The given phone number must not be null or empty!");

            #endregion

            this.Phone  = Phone.Trim();
            this.Fax    = Fax.  IsNotNullOrEmpty() ? Fax.  Trim() : null;
            this.Web    = Web.  IsNotNullOrEmpty() ? Web.  Trim() : null;
            this.EMail  = EMail.IsNotNullOrEmpty() ? EMail.Trim() : null;

        }

        #endregion


        #region Documentation

        // "contact": {
        //     "phone":  "+49 30 8122321",
        //     "fax":    "+49 30 8122322",
        //     "web":    "www.example.com",
        //     "email":  "contact@example.com"
        // }

        #endregion

        #region (static) Parse(ContactText)

        /// <summary>
        /// Parse the given text representation of an OIOI contact.
        /// </summary>
        /// <param name="ContactText">The text to parse.</param>
        public static Contact Parse(String ContactText)
        {

            Contact _Contact;

            if (TryParse(ContactText, out _Contact))
                return _Contact;

            return null;

        }

        #endregion

        #region (static) Parse(ContactJSON)

        /// <summary>
        /// Parse the given JSON representation of an OIOI contact.
        /// </summary>
        /// <param name="ContactJSON">The JSON to parse.</param>
        public static Contact Parse(JObject ContactJSON)
        {

            Contact _Contact;

            if (TryParse(ContactJSON, out _Contact))
                return _Contact;

            return null;

        }

        #endregion

        #region (static) TryParse(ContactJSON, out Contact, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI contact.
        /// </summary>
        /// <param name="ContactJSON">The JSON to parse.</param>
        /// <param name="Contact">The parsed contact.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject              ContactJSON,
                                       out Contact          Contact,
                                       OnExceptionDelegate  OnException  = null)
        {

            try
            {

                Contact = new Contact(ContactJSON.MapValueOrFail   ("phone", phone => phone.Value<String>().Trim()),
                                      ContactJSON.MapValueOrDefault("fax",   phone => phone.Value<String>().Trim()),
                                      ContactJSON.MapValueOrDefault("web",   phone => phone.Value<String>().Trim()),
                                      ContactJSON.MapValueOrDefault("email", phone => phone.Value<String>().Trim()));

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, ContactJSON, e);

                Contact = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(ContactText, out Contact, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI contact.
        /// </summary>
        /// <param name="ContactText">The text to parse.</param>
        /// <param name="Contact">The parsed contact.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String               ContactText,
                                       out Contact          Contact,
                                       OnExceptionDelegate  OnException  = null)
        {

            try
            {

                return TryParse(JObject.Parse(ContactText),
                                out Contact,
                                OnException);

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, ContactText, e);

                Contact = null;
                return false;

            }

        }

        #endregion

        #region ToJSON(CustomContactSerializer = null)

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        /// <param name="CustomContactSerializer">A delegate to serialize custom Contact JSON objects.</param>
        public JObject ToJSON(CustomJObjectSerializerDelegate<Contact> CustomContactSerializer  = null)
        {

            var JSON = JSONObject.Create(

                           new JProperty("phone",        Phone),

                           Fax.IsNotNullOrEmpty()
                               ? new JProperty("fax",    Fax)
                               : null,

                           Web.IsNotNullOrEmpty()
                               ? new JProperty("web",    Web)
                               : null,

                           EMail.IsNotNullOrEmpty()
                               ? new JProperty("email",  EMail)
                               : null

                       );

            return CustomContactSerializer != null
                       ? CustomContactSerializer(this, JSON)
                       : JSON;

        }

        #endregion


        #region Operator overloading

        #region Operator == (Contact1, Contact2)

        /// <summary>
        /// Compares two contacts for equality.
        /// </summary>
        /// <param name="Contact1">A contact.</param>
        /// <param name="Contact2">Another contact.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (Contact Contact1, Contact Contact2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(Contact1, Contact2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) Contact1 == null) || ((Object) Contact2 == null))
                return false;

            return Contact1.Equals(Contact2);

        }

        #endregion

        #region Operator != (Contact1, Contact2)

        /// <summary>
        /// Compares two contacts for inequality.
        /// </summary>
        /// <param name="Contact1">A contact.</param>
        /// <param name="Contact2">Another contact.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (Contact Contact1, Contact Contact2)

            => !(Contact1 == Contact2);

        #endregion

        #endregion

        #region IEquatable<Contact> Members

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

            // Check if the given object is a contact.
            var Contact = Object as Contact;
            if ((Object) Contact == null)
                return false;

            return Equals(Contact);

        }

        #endregion

        #region Equals(Contact)

        /// <summary>
        /// Compares two contacts for equality.
        /// </summary>
        /// <param name="Contact">A contact to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(Contact Contact)
        {

            if ((Object) Contact == null)
                return false;

            return Phone. Equals(Contact.Phone) &&

                   (Fax.  IsNullOrEmpty() == Contact.Fax.  IsNullOrEmpty() ||
                    Fax.  IsNotNullOrEmpty() && Contact.Fax.  IsNotNullOrEmpty() && Fax   == Contact.Fax) &&

                   (Web.  IsNullOrEmpty() == Contact.Web.  IsNullOrEmpty() ||
                    Web.  IsNotNullOrEmpty() && Contact.Web.  IsNotNullOrEmpty() && Web   == Contact.Fax) &&

                   (EMail.IsNullOrEmpty() == Contact.EMail.IsNullOrEmpty() ||
                    EMail.IsNotNullOrEmpty() && Contact.EMail.IsNotNullOrEmpty() && EMail == Contact.Fax);

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

                return Phone.GetHashCode() * 7 ^

                       (Fax.IsNotNullOrEmpty()
                            ? Fax.GetHashCode() * 5
                            : 0) ^

                       (Web.IsNotNullOrEmpty()
                            ? Web.GetHashCode() * 3
                            : 0) ^

                       (EMail.IsNotNullOrEmpty()
                            ? EMail.GetHashCode()
                            : 0);

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Phone,
                             Fax.  IsNotNullOrEmpty() ? ", " + Fax   : "",
                             Web.  IsNotNullOrEmpty() ? ", " + Web   : "",
                             EMail.IsNotNullOrEmpty() ? ", " + EMail : "");

        #endregion

    }

}