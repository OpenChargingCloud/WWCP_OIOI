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

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// An OIOI User.
    /// </summary>
    public class User : IEquatable<User>
    {

        #region Properties

        /// <summary>
        /// The identifier is something that uniquely identifies
        /// the customer, depending on the identifier type.
        /// </summary>
        [Mandatory]
        public String           Identifier        { get; }

        /// <summary>
        /// How to identify the user/customer/driver.
        /// </summary>
        [Mandatory]
        public IdentifierTypes  IdentifierType    { get; }

        /// <summary>
        /// A token can be used to authenticate the user.
        /// If for example the identifier type is username and the
        /// identifier is the user’s username, then token is used
        /// for authentication instead of a password.
        /// </summary>
        [Optional]
        public String           Token             { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI connector.
        /// </summary>
        /// <param name="Identifier">The identifier is something that uniquely identifies the customer, depending on the identifier type.</param>
        /// <param name="IdentifierType">How to identify the user/customer/driver.</param>
        /// <param name="Token">A token can be used to authenticate the user.</param>
        public User(String           Identifier,
                    IdentifierTypes  IdentifierType,
                    String           Token = null)
        {

            #region Initial checks

            if (Identifier.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Identifier),  "The given identifier must not be null or empty!");

            #endregion

            this.Identifier      = Identifier;
            this.IdentifierType  = IdentifierType;
            this.Token           = Token ?? "";

        }

        #endregion


        #region Documentation

        // {
        //     "identifier":       "12345678",
        //     "identifier-type":  "rfid"
        // }

        #endregion

        #region (static) Parse(UserJSON)

        /// <summary>
        /// Parse the given JSON representation of an OIOI user.
        /// </summary>
        /// <param name="UserJSON">The JSON to parse.</param>
        public static User Parse(JObject UserJSON)
        {

            User _User;

            if (TryParse(UserJSON, out _User))
                return _User;

            return null;

        }

        #endregion

        #region (static) Parse(UserText)

        /// <summary>
        /// Parse the given text representation of an OIOI user.
        /// </summary>
        /// <param name="UserText">The text to parse.</param>
        public static User Parse(String UserText)
        {

            User _User;

            if (TryParse(UserText, out _User))
                return _User;

            return null;

        }

        #endregion

        #region (static) TryParse(UserText, out User, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI user.
        /// </summary>
        /// <param name="UserText">The text to parse.</param>
        /// <param name="User">The parsed user.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String               UserText,
                                       out User             User,
                                       OnExceptionDelegate  OnException  = null)
        {

            try
            {

                return TryParse(JObject.Parse(UserText),
                                out User,
                                OnException);

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, UserText, e);

                User = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(UserJSON, out User, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI user.
        /// </summary>
        /// <param name="UserJSON">The JSON to parse.</param>
        /// <param name="User">The parsed user.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject              UserJSON,
                                       out User             User,
                                       OnExceptionDelegate  OnException  = null)
        {

            try
            {

                User = new User(UserJSON.MapValueOrFail   ("identifier",
                                                           value => value.Value<String>(),
                                                           "Invalid or missing JSON property 'identifier'!"),

                                UserJSON.MapValueOrFail   ("identifier-type",
                                                           value => Map(value.Value<String>()),
                                                           "Invalid or missing JSON property 'identifier-type'!"),

                                UserJSON.MapValueOrDefault("token",
                                                           value => value.Value<String>(),
                                                           String.Empty));

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, UserJSON, e);

                User = null;
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

                   new JProperty("identifier",       Identifier),
                   new JProperty("identifier-type",  IdentifierType.ToString()),

                   Token.IsNotNullOrEmpty()
                       ? new JProperty("speed",      Token)
                       : null
               );

        #endregion


        #region (static) Map(IdentifierType)

        public static IdentifierTypes Map(String IdentifierType)
        {

            switch (IdentifierType)
            {

                case "evco-id":
                    return IdentifierTypes.EVCOId;

                case "rfid":
                    return IdentifierTypes.RFID;

                case "username":
                    return IdentifierTypes.Username;

                default:
                    return IdentifierTypes.Unknown;

            }

        }

        public static String Map(IdentifierTypes IdentifierType)
        {

            switch (IdentifierType)
            {

                case IdentifierTypes.EVCOId:
                    return "evco-id";

                case IdentifierTypes.RFID:
                    return "rfid";

                case IdentifierTypes.Username:
                    return "username";

                default:
                    return "unknown";

            }

        }

        #endregion


        #region Operator overloading

        #region Operator == (User1, User2)

        /// <summary>
        /// Compares two users for equality.
        /// </summary>
        /// <param name="User1">A user.</param>
        /// <param name="User2">Another user.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (User User1, User User2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(User1, User2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) User1 == null) || ((Object) User2 == null))
                return false;

            return User1.Equals(User2);

        }

        #endregion

        #region Operator != (User1, User2)

        /// <summary>
        /// Compares two users for inequality.
        /// </summary>
        /// <param name="User1">A user.</param>
        /// <param name="User2">Another user.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (User User1, User User2)

            => !(User1 == User2);

        #endregion

        #endregion

        #region IEquatable<User> Members

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

            var User = Object as User;
            if ((Object) User == null)
                return false;

            return this.Equals(User);

        }

        #endregion

        #region Equals(User)

        /// <summary>
        /// Compares two users for equality.
        /// </summary>
        /// <param name="User">A user to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(User User)
        {

            if ((Object) User == null)
                return false;

            return Identifier.    Equals(User.IdentifierType) &&
                   IdentifierType.Equals(User.Identifier);

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

                return Identifier.     GetHashCode() * 5 ^
                       IdentifierType. GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Identifier, " (", IdentifierType, ")", Token.IsNotNullOrEmpty() ? " + " + Token : "");

        #endregion

    }

}