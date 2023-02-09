/*
 * Copyright (c) 2016-2023 GraphDefined GmbH
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

using System.Globalization;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x
{

    /// <summary>
    /// An OIOI connector (EVSE).
    /// </summary>
    public class Connector : AInternalData,
                             IEquatable<Connector>,
                             IComparable<Connector>,
                             IComparable
    {

        #region Properties

        /// <summary>
        /// The unique identification of the connector.
        /// </summary>
        public Connector_Id    Id       { get; }

        /// <summary>
        /// The type of the connector.
        /// </summary>
        public ConnectorTypes  Name     { get; }

        /// <summary>
        /// The maximum charging speed in kW.
        /// </summary>
        public Decimal         Speed    { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI connector.
        /// </summary>
        /// <param name="Id">The unique identification of the connector.</param>
        /// <param name="Name">The type of the connector.</param>
        /// <param name="Speed">The maximum charging speed in kW.</param>
        /// <param name="CustomData">An optional dictionary of customer-specific data.</param>
        public Connector(Connector_Id            Id,
                         ConnectorTypes          Name,
                         Decimal                 Speed,
                         JObject?                CustomData     = null,
                         UserDefinedDictionary?  InternalData   = null)

            : base(CustomData,
                   InternalData)

        {

            this.Id     = Id;
            this.Name   = Name;
            this.Speed  = Speed;

        }

        #endregion


        #region Documentation

        // {
        //     "id":    "DE*8PS*E123456",
        //     "name":  "Schuko",
        //     "speed": 3.7
        // }

        #endregion

        #region (static) Parse   (ConnectorJSON, CustomConnectorParser = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of a connector.
        /// </summary>
        /// <param name="ConnectorJSON">The JSON to parse.</param>
        /// <param name="CustomConnectorParser">A delegate to parse custom connector JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Connector Parse(JObject                                 ConnectorJSON,
                                      CustomJObjectParserDelegate<Connector>  CustomConnectorParser   = null,
                                      OnExceptionDelegate                     OnException             = null)
        {

            if (TryParse(ConnectorJSON,
                         out Connector connector,
                         CustomConnectorParser,
                         OnException))

                return connector;

            return null;

        }

        #endregion

        #region (static) Parse   (ConnectorText, CustomConnectorParser = null, OnException = null)

        /// <summary>
        /// Parse the given text representation of a connector.
        /// </summary>
        /// <param name="ConnectorText">The text to parse.</param>
        /// <param name="CustomConnectorParser">A delegate to parse custom connector JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Connector Parse(String                                  ConnectorText,
                                      CustomJObjectParserDelegate<Connector>  CustomConnectorParser   = null,
                                      OnExceptionDelegate                     OnException             = null)
        {

            if (TryParse(ConnectorText,
                         out Connector connector,
                         CustomConnectorParser,
                         OnException))

                return connector;

            return null;

        }

        #endregion

        #region (static) TryParse(ConnectorJSON, out Connector, CustomConnectorParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of a connector.
        /// </summary>
        /// <param name="ConnectorJSON">The JSON to parse.</param>
        /// <param name="Connector">The parsed connector.</param>
        /// <param name="CustomConnectorParser">A delegate to parse custom connector JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject                                 ConnectorJSON,
                                       out Connector                           Connector,
                                       CustomJObjectParserDelegate<Connector>  CustomConnectorParser   = null,
                                       OnExceptionDelegate                     OnException             = null)
        {

            try
            {

                Connector = new Connector(ConnectorJSON.MapValueOrFail("id",
                                                                       value => Connector_Id.Parse(value.Value<String>()),
                                                                       "Invalid or missing JSON property 'Id'!"),

                                          ConnectorJSON.MapValueOrFail("name",
                                                                       value => value.Value<String>().AsConnectorType(),
                                                                       "Invalid or missing JSON property 'name'!"),

                                          ConnectorJSON.MapValueOrFail("speed",
                                                                       value => value.Value<Decimal>(),
                                                                       "Invalid or missing JSON property 'speed'!"));


                if (CustomConnectorParser != null)
                    Connector = CustomConnectorParser(ConnectorJSON,
                                                      Connector);

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(Timestamp.Now, ConnectorJSON, e);

                Connector = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(ConnectorText, out Connector, CustomConnectorParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of a connector.
        /// </summary>
        /// <param name="ConnectorText">The text to parse.</param>
        /// <param name="Connector">The parsed connector.</param>
        /// <param name="CustomConnectorParser">A delegate to parse custom connector JSON objects.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String                                  ConnectorText,
                                       out Connector                           Connector,
                                       CustomJObjectParserDelegate<Connector>  CustomConnectorParser   = null,
                                       OnExceptionDelegate                     OnException             = null)
        {

            try
            {

                return TryParse(JObject.Parse(ConnectorText),
                                out Connector,
                                CustomConnectorParser,
                                OnException);

            }
            catch (Exception e)
            {

                OnException?.Invoke(Timestamp.Now, ConnectorText, e);

                Connector = null;
                return false;

            }

        }

        #endregion

        #region ToJSON(CustomConnectorSerializer = null)

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        /// <param name="CustomConnectorSerializer">A delegate to serialize custom Connector JSON objects.</param>
        public JObject ToJSON(CustomJObjectSerializerDelegate<Connector> CustomConnectorSerializer = null)
        {

            var JSON = JSONObject.Create(
                           new JProperty("id",     Id.   ToString()),
                           new JProperty("name",   Name. ToString()),
                           new JProperty("speed",  Speed.ToString(CultureInfo.InvariantCulture))//.ToString("N1").Replace(",", "."))
                       );

            return CustomConnectorSerializer != null
                       ? CustomConnectorSerializer(this, JSON)
                       : JSON;

        }

        #endregion


        #region Operator overloading

        #region Operator == (Connector1, Connector2)

        /// <summary>
        /// Compares two connectores for equality.
        /// </summary>
        /// <param name="Connector1">A connector.</param>
        /// <param name="Connector2">Another connector.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (Connector Connector1, Connector Connector2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(Connector1, Connector2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) Connector1 == null) || ((Object) Connector2 == null))
                return false;

            return Connector1.Equals(Connector2);

        }

        #endregion

        #region Operator != (Connector1, Connector2)

        /// <summary>
        /// Compares two connectores for inequality.
        /// </summary>
        /// <param name="Connector1">A connector.</param>
        /// <param name="Connector2">Another connector.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (Connector Connector1, Connector Connector2)

            => !(Connector1 == Connector2);

        #endregion

        #region Operator <  (Connector1, Connector2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Connector1">A connector.</param>
        /// <param name="Connector2">Another connector.</param>
        /// <returns>true|false</returns>
        public static Boolean operator < (Connector Connector1, Connector Connector2)
        {

            if ((Object) Connector1 == null)
                throw new ArgumentNullException(nameof(Connector1), "The given Connector1 must not be null!");

            return Connector1.CompareTo(Connector2) < 0;

        }

        #endregion

        #region Operator <= (Connector1, Connector2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Connector1">A connector.</param>
        /// <param name="Connector2">Another connector.</param>
        /// <returns>true|false</returns>
        public static Boolean operator <= (Connector Connector1, Connector Connector2)
            => !(Connector1 > Connector2);

        #endregion

        #region Operator >  (Connector1, Connector2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Connector1">A connector.</param>
        /// <param name="Connector2">Another connector.</param>
        /// <returns>true|false</returns>
        public static Boolean operator > (Connector Connector1, Connector Connector2)
        {

            if ((Object)Connector1 == null)
                throw new ArgumentNullException(nameof(Connector1), "The given Connector1 must not be null!");

            return Connector1.CompareTo(Connector2) > 0;

        }

        #endregion

        #region Operator >= (Connector1, Connector2)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Connector1">A connector.</param>
        /// <param name="Connector2">Another connector.</param>
        /// <returns>true|false</returns>
        public static Boolean operator >= (Connector Connector1, Connector Connector2)
            => !(Connector1 < Connector2);

        #endregion

        #endregion

        #region IComparable<Connector> Members

        #region CompareTo(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        public Int32 CompareTo(Object Object)
        {

            if (Object == null)
                throw new ArgumentNullException(nameof(Object), "The given object must not be null!");

            var Connector = Object as Connector;
            if ((Object)Connector == null)
                throw new ArgumentException("The given object is not an connector identification!", nameof(Object));

            return CompareTo(Connector);

        }

        #endregion

        #region CompareTo(Connector)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Connector">An object to compare with.</param>
        public Int32 CompareTo(Connector Connector)
        {

            if ((Object) Connector == null)
                throw new ArgumentNullException(nameof(Connector), "The given connector must not be null!");

            return Id.CompareTo(Connector.Id);

        }

        #endregion

        #endregion

        #region IEquatable<Connector> Members

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

            // Check if the given object is a connector.
            var Connector = Object as Connector;
            if ((Object) Connector == null)
                return false;

            return this.Equals(Connector);

        }

        #endregion

        #region Equals(Connector)

        /// <summary>
        /// Compares two connectors for equality.
        /// </summary>
        /// <param name="Connector">A connector to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(Connector Connector)
        {

            if ((Object) Connector == null)
                return false;

            return Id.    Equals(Connector.Id)   &&
                   Name.  Equals(Connector.Name) &&
                   Speed. Equals(Connector.Speed);

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

                return Id.    GetHashCode() * 17 ^
                       Name.  GetHashCode() * 11 ^
                       Speed. GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Id, " having ", Name, " / ", Speed.ToString("N1"), " kW");

        #endregion

    }

}