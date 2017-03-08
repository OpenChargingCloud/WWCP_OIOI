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
using System.Linq;
using System.Collections.Generic;

using org.GraphDefined.Vanaheimr.Illias;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    /// <summary>
    /// Helper methods to map OIOI data type values to
    /// WWCP data type values and vice versa.
    /// </summary>
    public static class OIOIMapper
    {

        public static Station_Id         ToOIOI(this ChargingStation_Id  ChargingStationId)
            => Station_Id.Parse(ChargingStationId.ToString());

        public static ChargingStation_Id ToWWCP(this Station_Id          StationId)
            => ChargingStation_Id.Parse(StationId.ToString());

        public static Connector_Id       ToOIOI(this EVSE_Id             EVSEId)
            => Connector_Id.Parse(EVSEId.ToString());

        public static EVSE_Id            ToWWCP(this Connector_Id        ConnectorId)
            => EVSE_Id.Parse(ConnectorId.ToString());


        #region ToOIOI(this WWCPAddress)

        /// <summary>
        /// Maps a WWCP address to an OIOI address.
        /// </summary>
        /// <param name="WWCPAddress">A WWCP address.</param>
        public static Address ToOIOI(this WWCP.Address WWCPAddress)

            => new Address(WWCPAddress.Street,
                           WWCPAddress.HouseNumber,
                           WWCPAddress.City.FirstText,
                           WWCPAddress.PostalCode,
                           WWCPAddress.Country);


        /// <summary>
        /// Maps an OIOI address type to a WWCP address type.
        /// </summary>
        /// <param name="OIOIAddress">A address type.</param>
        public static WWCP.Address ToWWCP(this Address OIOIAddress)

            => new WWCP.Address(OIOIAddress.Street,
                                OIOIAddress.StreetNumber,
                                null,
                                OIOIAddress.ZIP,
                                null,
                                I18NString.Create(Languages.unknown, OIOIAddress.City),
                                OIOIAddress.Country);

        #endregion

        #region  ToOIOI(this PlugType)

        public static ConnectorTypes ToOIOI(this PlugTypes PlugType)
        {

            switch (PlugType)
            {

                case PlugTypes.TeslaConnector:
                case PlugTypes.TESLA_Roadster:
                case PlugTypes.TESLA_ModelS:
                    return ConnectorTypes.Tesla;

                case PlugTypes.TypeEFrenchStandard:
                    return ConnectorTypes.TypeE;

                case PlugTypes.TypeFSchuko:
                    return ConnectorTypes.Schuko;


                case PlugTypes.Type1Connector_CableAttached:
                    return ConnectorTypes.Type1;

                case PlugTypes.Type2Outlet:
                case PlugTypes.Type2Connector_CableAttached:
                    return ConnectorTypes.Type2;

                case PlugTypes.Type3Outlet:
                    return ConnectorTypes.Type3;


                case PlugTypes.CHAdeMO:
                    return ConnectorTypes.Chademo;

                // Combo
                // CeeBlue
                // ThreePinSquare
                // CeeRed
                // Cee2Poles
                // Scame
                // Nema5
                // CeePlus
                // T13
                // T15
                // T23
                // Marechal

                //case PlugTypes.AVCONConnector:
                //case PlugTypes.SmallPaddleInductive:
                //case PlugTypes.LargePaddleInductive:
                //case PlugTypes.NEMA5_20,
                //case PlugTypes.TypeGBritishStandard,
                //case PlugTypes.TypeJSwissStandard,
                //case PlugTypes.IEC60309SinglePhase,
                //case PlugTypes.IEC60309ThreePhase,
                //case PlugTypes.CCSCombo1Plug_CableAttached,
                //case PlugTypes.CCSCombo2Plug_CableAttached,
                default:
                    return ConnectorTypes.Unspecified;

            }

        }

        #endregion

        #region  ToOIOI(this EVSE)

        public static Connector ToOIOI(this EVSE EVSE)

            => new Connector(EVSE.Id.ToOIOI(),
                             EVSE.SocketOutlets.First().Plug.ToOIOI(),
                             EVSE.MaxPower.HasValue ? EVSE.MaxPower.Value : 0);

        #endregion


        #region ToOIOI(this ChargingStation, EVSE2EVSEDataRecord = null)

        /// <summary>
        /// Convert a WWCP charging station into a corresponding OIOI charging station.
        /// </summary>
        /// <param name="ChargingStation">A WWCP charging station.</param>
        /// <param name="ChargingStation2Station">A delegate to process a charging station, e.g. before pushing it to a roaming provider.</param>
        /// <returns>The corresponding OIOI charging station.</returns>
        public static Station ToOIOI(this WWCP.ChargingStation            ChargingStation,
                                     CPO.ChargingStation2StationDelegate  ChargingStation2Station = null)
        {

            var _Station = new Station(ChargingStation.Id.ToOIOI(),
                                       ChargingStation.Name.FirstText,
                                       ChargingStation.GeoLocation.Value,
                                       ChargingStation.Address.ToOIOI(),
                                       null, // Contact
                                       ChargingStation.Operator.Id,
                                       ChargingStation.OpeningTimes != null ? ChargingStation.OpeningTimes.IsOpen24Hours : true,
                                       ChargingStation.EVSEs.Select(evse => evse.ToOIOI()),
                                       ChargingStation.Description.FirstText,
                                       ChargingStation.OpeningTimes);

            return ChargingStation2Station != null
                       ? ChargingStation2Station(ChargingStation, _Station)
                       : _Station;

        }

        #endregion



    }

}
