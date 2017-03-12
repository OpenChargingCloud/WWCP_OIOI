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
            => Station_Id.        Parse(ChargingStationId.ToString());

        public static ChargingStation_Id ToWWCP(this Station_Id          StationId)
            => ChargingStation_Id.Parse(StationId.ToString());

        public static Connector_Id       ToOIOI(this EVSE_Id             EVSEId)
            => Connector_Id.      Parse(EVSEId.ToString());

        public static EVSE_Id            ToWWCP(this Connector_Id        ConnectorId)
            => EVSE_Id.           Parse(ConnectorId.ToString());




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

        #region  ToOIOI(this AuthToken)

        public static RFID_Id ToOIOI(this Auth_Token AuthToken)
            => RFID_Id.Parse(AuthToken.ToString());

        #endregion

        #region  ToOIOI(this EVSEStatusType)

        public static ConnectorStatusTypes ToOIOI(this EVSEStatusTypes EVSEStatusType)
        {

            switch (EVSEStatusType)
            {

                case EVSEStatusTypes.Offline:
                    return ConnectorStatusTypes.Offline;

                case EVSEStatusTypes.Available:
                    return ConnectorStatusTypes.Available;

                case EVSEStatusTypes.Reserved:
                    return ConnectorStatusTypes.Reserved;

                case EVSEStatusTypes.Charging:
                    return ConnectorStatusTypes.Occupied;

                //case EVSEStatusTypes.Unspecified
                //case EVSEStatusTypes.Planned
                //case EVSEStatusTypes.InDeployment
                //case EVSEStatusTypes.OutOfService
                //case EVSEStatusTypes.Blocked
                //case EVSEStatusTypes.WaitingForPlugin
                //case EVSEStatusTypes.PluggedIn
                //case EVSEStatusTypes.DoorNotClosed
                //case EVSEStatusTypes.Faulted
                //case EVSEStatusTypes.Private
                //case EVSEStatusTypes.Deleted

                default:
                    return ConnectorStatusTypes.Unknown;

            }

        }

        #endregion


        #region ToOIOI(this ChargingStation, ChargingStation2Station = null)

        /// <summary>
        /// Convert a WWCP charging station into a corresponding OIOI charging station.
        /// </summary>
        /// <param name="ChargingStation">A WWCP charging station.</param>
        /// <param name="ChargingStation2Station">A delegate to process a charging station, e.g. before pushing it to a roaming provider.</param>
        /// <returns>The corresponding OIOI charging station.</returns>
        public static Station ToOIOI(this ChargingStation                 ChargingStation,
                                     CPO.ChargingStation2StationDelegate  ChargingStation2Station = null)
        {

            var _Station = new Station(ChargingStation.Id.ToOIOI(),
                                       ChargingStation.Name.FirstText,
                                       ChargingStation.GeoLocation.Value,
                                       ChargingStation.Address.ToOIOI(),
                                       new Contact("Phone",
                                                   "Fax",
                                                   "Web",
                                                   "EMail"),
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


        #region ChargeDetailRecord <-> Session

        /// <summary>
        /// Convert the given WWCP charging session identification into an OIOI session identification.
        /// </summary>
        /// <param name="ChargingSessionId">A WWCP charging session identification.</param>
        public static Session_Id ToOIOI(this ChargingSession_Id ChargingSessionId)
            => Session_Id.Parse(ChargingSessionId.ToString());


        /// <summary>
        /// Convert the given OIOI session identification into a WWCP charging session identification.
        /// </summary>
        /// <param name="SessionId">An OIOI session identification.</param>
        public static ChargingSession_Id ToWWCP(this Session_Id SessionId)
            => ChargingSession_Id.Parse(SessionId.ToString());


        /// <summary>
        /// Convert a WWCP charge detail record into a corresponding OIOI charging session.
        /// </summary>
        /// <param name="ChargeDetailRecord">A WWCP charge detail record.</param>
        /// <param name="ChargeDetailRecord2Session">An optional delegate to customize the transformation.</param>
        public static Session ToOIOI(this ChargeDetailRecord                 ChargeDetailRecord,
                                     CPO.ChargeDetailRecord2SessionDelegate  ChargeDetailRecord2Session = null)

        {

            return null;

            //var Session = new Session(
            //                  ChargeDetailRecord.EVSEId.Value.ToOIOI(),
            //                  ChargeDetailRecord.SessionId.ToOIOI(),
            //                  ChargeDetailRecord.SessionTime.Value.StartTime,
            //                  ChargeDetailRecord.SessionTime.Value.EndTime.Value,
            //                  ChargeDetailRecord.IdentificationStart.ToOIOI(),
            //                  ChargeDetailRecord.ChargingProduct?.Id.ToOIOI(),
            //                  null, // PartnerSessionId
            //                  ChargeDetailRecord.SessionTime.HasValue ? ChargeDetailRecord.SessionTime.Value.StartTime : new DateTime?(),
            //                  ChargeDetailRecord.SessionTime.HasValue ? ChargeDetailRecord.SessionTime.Value.EndTime : null,
            //                  ChargeDetailRecord.EnergyMeteringValues != null && ChargeDetailRecord.EnergyMeteringValues.Any() ? ChargeDetailRecord.EnergyMeteringValues.First().Value : new Single?(),
            //                  ChargeDetailRecord.EnergyMeteringValues != null && ChargeDetailRecord.EnergyMeteringValues.Any() ? ChargeDetailRecord.EnergyMeteringValues.Last(). Value : new Single?(),
            //                  ChargeDetailRecord.EnergyMeteringValues != null && ChargeDetailRecord.EnergyMeteringValues.Any() ? ChargeDetailRecord.EnergyMeteringValues.Select((Timestamped<Single> v) => v.Value) : null,
            //                  ChargeDetailRecord.ConsumedEnergy,
            //                  ChargeDetailRecord.MeteringSignature
            //              );

            //if (ChargeDetailRecord2Session != null)
            //    return ChargeDetailRecord2Session(ChargeDetailRecord, Session);

            //return Session;

        }


        /// <summary>
        /// Convert an OIOI charging session into a corresponding WWCP charge detail record.
        /// </summary>
        /// <param name="Session">An OIOI charging session.</param>
        /// <param name="Session2ChargeDetailRecord">An optional delegate to customize the transformation.</param>
        public static ChargeDetailRecord ToWWCP(this Session                            Session,
                                                CPO.Session2ChargeDetailRecordDelegate  Session2ChargeDetailRecord = null)
        {

            return null;

            //var ChargeDetailRecord = new ChargeDetailRecord(Session.Id.ToWWCP(),
            //                                                EVSEId:                Session.ConnectorId.ToWWCP(),
            //                                                ChargingProduct:       ChargeDetailRecord.PartnerProductId.HasValue
            //                                                                           ? new ChargingProduct(ChargeDetailRecord.PartnerProductId.Value.ToWWCP())
            //                                                                           : null,
            //                                                SessionTime:           new StartEndDateTime(ChargeDetailRecord.SessionStart, ChargeDetailRecord.SessionEnd),
            //                                                EnergyMeteringValues:  new List<Timestamped<Single>> {

            //                                                                           new Timestamped<Single>(
            //                                                                               ChargeDetailRecord.ChargingStart.  Value,
            //                                                                               ChargeDetailRecord.MeterValueStart.Value
            //                                                                           ),

            //                                                                           new Timestamped<Single>(
            //                                                                               ChargeDetailRecord.ChargingEnd.  Value,
            //                                                                               ChargeDetailRecord.MeterValueEnd.Value
            //                                                                           )

            //                                                                       },
            //                                                //MeterValuesInBetween
            //                                                //ConsumedEnergy
            //                                                MeteringSignature:  ChargeDetailRecord.MeteringSignature);

            //if (Session2ChargeDetailRecord != null)
            //    return Session2ChargeDetailRecord(Session, ChargeDetailRecord);

            //return ChargeDetailRecord;

        }

        #endregion





    }

}
