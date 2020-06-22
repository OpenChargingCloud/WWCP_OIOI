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
    /// An OIOI user.
    /// </summary>
    public class User : IEquatable<User>,
                        IComparable<User>,
                        IComparable
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
        /// Create a new user.
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
            this.Token           = Token;

        }

        #endregion


        #region Documentation

        // {
        //     "identifier":       "12345678",
        //     "identifier-type":  "rfid"
        // }

        #endregion

        #region (static) Parse(UserJSON, CustomUserParser = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI user.
        /// </summary>
        /// <param name="UserJSON">The JSON to parse.</param>
        /// <param name="CustomUserParser">A delegate to parse custom User JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static User Parse(JObject                            UserJSON,
                                 CustomJObjectParserDelegate<User>  CustomUserParser   = null,
                                 OnExceptionDelegate                OnException        = null)
        {

            if (TryParse(UserJSON,
                         out User user,
                         CustomUserParser,
                         OnException))

                return user;

            return null;

        }

        #endregion

        #region (static) Parse(UserText, CustomUserParser = null, OnException = null)

        /// <summary>
        /// Parse the given text representation of an OIOI user.
        /// </summary>
        /// <param name="UserText">The text to parse.</param>
        /// <param name="CustomUserParser">A delegate to parse custom User JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static User Parse(String                             UserText,
                                 CustomJObjectParserDelegate<User>  CustomUserParser   = null,
                                 OnExceptionDelegate                OnException        = null)
        {

            if (TryParse(UserText,
                         out User user,
                         CustomUserParser,
                         OnException))

                return user;

            return null;

        }

        #endregion

        #region (static) TryParse(UserText, out User, CustomUserParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI user.
        /// </summary>
        /// <param name="UserText">The text to parse.</param>
        /// <param name="User">The parsed user.</param>
        /// <param name="CustomUserParser">A delegate to parse custom User JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String                             UserText,
                                       out User                           User,
                                       CustomJObjectParserDelegate<User>  CustomUserParser   = null,
                                       OnExceptionDelegate                OnException        = null)
        {

            try
            {

                return TryParse(JObject.Parse(UserText),
                                out User,
                                CustomUserParser,
                                OnException);

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, UserText, e);

                User = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(UserJSON, out User, CustomUserParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI user.
        /// </summary>
        /// <param name="UserJSON">The JSON to parse.</param>
        /// <param name="User">The parsed user.</param>
        /// <param name="CustomUserParser">A delegate to parse custom User JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject                            UserJSON,
                                       out User                           User,
                                       CustomJObjectParserDelegate<User>  CustomUserParser   = null,
                                       OnExceptionDelegate                OnException        = null)
        {

            try
            {

                User = new User(UserJSON.MapValueOrFail   ("identifier",
                                                           value => value.Value<String>(),
                                                           "Invalid or missing JSON property 'identifier'!"),

                                UserJSON.MapValueOrFail   ("identifier-type",
                                                           value => IdentifierTypesExtentions.AsIdentifierType(value.Value<String>()),
                                                           "Invalid or missing JSON property 'identifier-type'!"),

                                UserJSON.MapValueOrDefault("token",
                                                           value => value.Value<String>(),
                                                           String.Empty));


                if (CustomUserParser != null)
                    User = CustomUserParser(UserJSON,
                                            User);

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, UserJSON, e);

                User = null;
                return false;

            }

        }

        #endregion

        #region ToJSON(CustomUserSerializer = null)

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        /// <param name="CustomUserSerializer">A delegate to serialize custom User JSON objects.</param>
        public JObject ToJSON(CustomJObjectSerializerDelegate<User> CustomUserSerializer  = null)
        {

            var JSON = JSONObject.Create(

                           new JProperty("identifier",       Identifier),
                           new JProperty("identifier-type",  IdentifierType.ToText()),

                           Token.IsNotNullOrEmpty()
                               ? new JProperty("token",      Token)
                               : null

                       );

            return CustomUserSerializer != null
                       ? CustomUserSerializer(this, JSON)
                       : JSON;

        }

        #endregion


        #region Operator overloading

        #region Operator == (User1, User2)

        /// <summary>
        /// Compares two useres for equality.
        /// </summary>
        /// <param name="User1">An user.</param>
        /// <param name="User2">Another user.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (User User1, User User2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(User1, User2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) User1 == null) || ((Object) User2 == null))
                return false;

            return User1.Equals(User2);

        }

        #endregion

        #region Operator != (User1, User2)

        /// <summary>
        /// Compares two useres for inequality.
        /// </summary>
        /// <param name="User1">An user.</param>
        /// <param name="User2">Another user.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (User User1, User User2)

            => !(User1 == User2);

        #endregion

        #region Operator <  (User1, User2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="User1">An user.</param>
        /// <param name="User2">Another user.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (User User1, User User2)
        {

            if ((Object) User1 == null)
                throw new ArgumentNullException(nameof(User1), "The given User1 must not be null!");

            return User1.CompareTo(User2) < 0;

        }

        #endregion

        #region Operator <= (User1, User2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="User1">An user.</param>
        /// <param name="User2">Another user.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (User User1, User User2)
            => !(User1 > User2);

        #endregion

        #region Operator >  (User1, User2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="User1">An user.</param>
        /// <param name="User2">Another user.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (User User1, User User2)
        {

            if ((Object)User1 == null)
                throw new ArgumentNullException(nameof(User1), "The given User1 must not be null!");

            return User1.CompareTo(User2) > 0;

        }

        #endregion

        #region Operator >= (User1, User2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="User1">An user.</param>
        /// <param name="User2">Another user.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (User User1, User User2)
            => !(User1 < User2);

        #endregion

        #endregion

        #region IComparable<User> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            var User = Object as User;
            if ((Object)User == null)
                throw new ArgumentException("The given object is not an user identification!", nameof(Object));

            return CompareTo(User);

        }

        #endregion

        #region CompareTo(User)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="User">An object to compare with.</param>
        public Int32 CompareTo(User User)
        {

            if ((Object) User == null)
                throw new ArgumentNullException(nameof(User), "The given user must not be null!");

            var c = Identifier.CompareTo(User.Identifier);
            if (c != 0)
                return c;

            return IdentifierType.CompareTo(User.IdentifierType);

        }

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
                   IdentifierType.Equals(User.Identifier)     &&

                   ((Token == null && User.Token == null) ||
                    (Token != null && User.Token != null && Token.Equals(User.Token)));

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
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Identifier, " (", IdentifierType, ")", Token.IsNotNullOrEmpty() ? " + " + Token : "");

        #endregion

    }

}