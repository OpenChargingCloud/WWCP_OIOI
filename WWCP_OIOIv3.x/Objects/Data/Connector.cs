﻿/*
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
    /// An OIOI connector (EVSE).
    /// </summary>
    public class Connector : IEquatable<Connector>
    {

        #region Properties

        /// <summary>
        /// The unique EVSE identification of the connector.
        /// </summary>
        public EVSE_Id         Id        { get; }

        /// <summary>
        /// The type of the connector.
        /// </summary>
        public ConnectorTypes  Name      { get; }

        /// <summary>
        /// The maximum charging speed in kW.
        /// </summary>
        public Single          Speed     { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI connector.
        /// </summary>
        /// <param name="Id">The unique EVSE identification of the connector.</param>
        /// <param name="Name">The type of the connector.</param>
        /// <param name="Speed">The maximum charging speed in kW.</param>
        public Connector(EVSE_Id         Id,
                         ConnectorTypes  Name,
                         Single          Speed)
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

        #region (static) Parse(ConnectorJSON)

        /// <summary>
        /// Parse the given JSON representation of an OIOI connector.
        /// </summary>
        /// <param name="ConnectorJSON">The JSON to parse.</param>
        public static Connector Parse(JObject ConnectorJSON)
        {

            Connector _Connector;

            if (TryParse(ConnectorJSON, out _Connector))
                return _Connector;

            return null;

        }

        #endregion

        #region (static) Parse(ConnectorText)

        /// <summary>
        /// Parse the given text representation of an OIOI connector.
        /// </summary>
        /// <param name="ConnectorText">The text to parse.</param>
        public static Connector Parse(String ConnectorText)
        {

            Connector _Connector;

            if (TryParse(ConnectorText, out _Connector))
                return _Connector;

            return null;

        }

        #endregion

        #region (static) TryParse(ConnectorText, out Connector, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI connector.
        /// </summary>
        /// <param name="ConnectorText">The text to parse.</param>
        /// <param name="Connector">The parsed connector.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String               ConnectorText,
                                       out Connector        Connector,
                                       OnExceptionDelegate  OnException  = null)
        {

            try
            {

                return TryParse(JObject.Parse(ConnectorText),
                                out Connector,
                                OnException);

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, ConnectorText, e);

                Connector = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(ConnectorJSON, out Connector, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI connector.
        /// </summary>
        /// <param name="ConnectorJSON">The JSON to parse.</param>
        /// <param name="Connector">The parsed connector.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject              ConnectorJSON,
                                       out Connector        Connector,
                                       OnExceptionDelegate  OnException  = null)
        {

            try
            {

                Connector = new Connector(ConnectorJSON.MapValueOrFail("id",
                                                                       value => EVSE_Id.Parse(value.Value<String>()),
                                                                       "Invalid or missing JSON property 'Id'!"),

                                          ConnectorJSON.MapValueOrFail("name",
                                                                       value => Map(value.Value<String>()),
                                                                       "Invalid or missing JSON property 'name'!"),

                                          ConnectorJSON.MapValueOrFail("speed",
                                                                       value => value.Value<Single>(),
                                                                       "Invalid or missing JSON property 'speed'!"));

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, ConnectorJSON, e);

                Connector = null;
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
                   new JProperty("id",     Id.   ToString()),
                   new JProperty("name",   Name. ToString()),
                   new JProperty("speed",  Speed)//.ToString("N1").Replace(",", "."))
               );

        #endregion


        #region (static) Map(ConnectorType)

        public static ConnectorTypes Map(String ConnectorType)
        {

            switch (ConnectorType)
            {

                case "Type2":
                    return ConnectorTypes.Type2;

                case "Combo":
                    return ConnectorTypes.Combo;

                case "Chademo":
                    return ConnectorTypes.Chademo;

                case "Schuko":
                    return ConnectorTypes.Schuko;

                case "Type3":
                    return ConnectorTypes.Type3;

                case "CeeBlue":
                    return ConnectorTypes.CeeBlue;

                case "3PinSquare":
                    return ConnectorTypes.ThreePinSquare;

                case "Type1":
                    return ConnectorTypes.Type1;

                case "CeeRed":
                    return ConnectorTypes.CeeRed;

                case "Cee2Poles":
                    return ConnectorTypes.Cee2Poles;

                case "Tesla":
                    return ConnectorTypes.Tesla;

                case "Scame":
                    return ConnectorTypes.Scame;

                case "Nema5":
                    return ConnectorTypes.Nema5;

                case "CeePlus":
                    return ConnectorTypes.CeePlus;

                case "T13":
                    return ConnectorTypes.T13;

                case "T15":
                    return ConnectorTypes.T15;

                case "T23":
                    return ConnectorTypes.T23;

                case "Marechal":
                    return ConnectorTypes.Marechal;

                case "TypeE":
                    return ConnectorTypes.TypeE;

                default:
                    return ConnectorTypes.Unspecified;

            }

        }

        public static String Map(ConnectorTypes ConnectorType)
        {

            switch (ConnectorType)
            {

                case ConnectorTypes.Type2:
                    return "Type2";

                case ConnectorTypes.Combo:
                    return "Combo";

                case ConnectorTypes.Chademo:
                    return "Chademo";

                case ConnectorTypes.Schuko:
                    return "Schuko";

                case ConnectorTypes.Type3:
                    return "Type3";

                case ConnectorTypes.CeeBlue:
                    return "CeeBlue";

                case ConnectorTypes.ThreePinSquare:
                    return "3PinSquare";

                case ConnectorTypes.Type1:
                    return "Type1";

                case ConnectorTypes.CeeRed:
                    return "CeeRed";

                case ConnectorTypes.Cee2Poles:
                    return "Cee2Poles";

                case ConnectorTypes.Tesla:
                    return "Tesla";

                case ConnectorTypes.Scame:
                    return "Scame";

                case ConnectorTypes.Nema5:
                    return "Nema5";

                case ConnectorTypes.CeePlus:
                    return "CeePlus";

                case ConnectorTypes.T13:
                    return "T13";

                case ConnectorTypes.T15:
                    return "T15";

                case ConnectorTypes.T23:
                    return "T23";

                case ConnectorTypes.Marechal:
                    return "Marechal";

                case ConnectorTypes.TypeE:
                    return "TypeE";

                default:
                    return "unspecified";

            }

        }

        #endregion


        #region Operator overloading

        #region Operator == (Connector1, Connector2)

        /// <summary>
        /// Compares two connectors for equality.
        /// </summary>
        /// <param name="Connector1">A connector.</param>
        /// <param name="Connector2">Another connector.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (Connector Connector1, Connector Connector2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(Connector1, Connector2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) Connector1 == null) || ((Object) Connector2 == null))
                return false;

            return Connector1.Equals(Connector2);

        }

        #endregion

        #region Operator != (Connector1, Connector2)

        /// <summary>
        /// Compares two connectors for inequality.
        /// </summary>
        /// <param name="Connector1">A connector.</param>
        /// <param name="Connector2">Another connector.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (Connector Connector1, Connector Connector2)

            => !(Connector1 == Connector2);

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
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Id, " having ", Name, " / ", Speed.ToString("N1"), " kW");

        #endregion

    }

}