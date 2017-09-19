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

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// OIOI JSON I/O.
    /// </summary>
    public static class JSON_IO
    {

        #region IdentifierTypes

        public static IdentifierTypes AsIdentifierTypes(this String Text)
        {

            if (Text.IsNullOrEmpty())
                return IdentifierTypes.Unknown;

            switch (Text)
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

        public static String AsText(this IdentifierTypes IdentifierType)
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


        #region ConnectorStatusTypes

        public static ConnectorStatusTypes AsConnectorStatusType(this String Text)
        {

            if (Text.IsNullOrEmpty())
                return ConnectorStatusTypes.Unknown;

            switch (Text.ToLower())
            {

                case "available":
                    return ConnectorStatusTypes.Available;

                case "occupied":
                    return ConnectorStatusTypes.Occupied;

                case "offline":
                    return ConnectorStatusTypes.Offline;

                case "reserved":
                    return ConnectorStatusTypes.Reserved;

                default:
                    return ConnectorStatusTypes.Unknown;

            }

        }

        public static String AsText(this ConnectorStatusTypes ConnectorStatusType)
        {

            switch (ConnectorStatusType)
            {

                case ConnectorStatusTypes.Available:
                    return "Available";

                case ConnectorStatusTypes.Occupied:
                    return "Occupied";

                case ConnectorStatusTypes.Offline:
                    return "Offline";

                case ConnectorStatusTypes.Reserved:
                    return "Reserved";

                default:
                    return "Unknown";

            }

        }

        #endregion

        #region ConnectorTypes

        public static ConnectorTypes AsConnectorType(this String Text)
        {

            if (Text.IsNullOrEmpty())
                return ConnectorTypes.UNKNOWN;

            switch (Text.ToLower())
            {

                case "type2":
                    return ConnectorTypes.Type2;

                case "combo":
                    return ConnectorTypes.Combo;

                case "chademo":
                    return ConnectorTypes.Chademo;

                case "schuko":
                    return ConnectorTypes.Schuko;

                case "type3":
                    return ConnectorTypes.Type3;

                case "ceeblue":
                    return ConnectorTypes.CeeBlue;

                case "threepinsquare":
                    return ConnectorTypes.ThreePinSquare;

                case "type1":
                    return ConnectorTypes.Type1;

                case "ceered":
                    return ConnectorTypes.CeeRed;

                case "cee2poles":
                    return ConnectorTypes.Cee2Poles;

                case "tesla":
                    return ConnectorTypes.Tesla;

                case "scame":
                    return ConnectorTypes.Scame;

                case "nema5":
                    return ConnectorTypes.Nema5;

                case "ceeplus":
                    return ConnectorTypes.CeePlus;

                case "t13":
                    return ConnectorTypes.T13;

                case "t15":
                    return ConnectorTypes.T15;

                case "t23":
                    return ConnectorTypes.T23;

                case "marechal":
                    return ConnectorTypes.Marechal;

                case "typee":
                    return ConnectorTypes.TypeE;


                default:
                    return ConnectorTypes.UNKNOWN;

            }

        }

        public static String AsText(this ConnectorTypes ConnectorType)
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
                    return "ThreePinSquare";

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
                    return "UNKNOWN";

            }

        }

        #endregion

    }

}