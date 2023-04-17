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

using org.GraphDefined.Vanaheimr.Illias;

using cloud.charging.open.protocols.WWCP;

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x
{

    /// <summary>
    /// Helper methods to map OIOI data type values to
    /// WWCP data type values and vice versa.
    /// </summary>
    public static class OIOIMapper
    {

        #region ChargingStationId <-> StationId

        public static Station_Id         ToOIOI(this ChargingStation_Id              ChargingStationId)
            => Station_Id.        Parse(ChargingStationId.ToString());

        public static ChargingStation_Id ToWWCP(this Station_Id                      StationId)
            => ChargingStation_Id.Parse(StationId.ToString());

        #endregion

        #region EVSEId            <-> ConnectorId

        public static Connector_Id?      ToOIOI(this EVSE_Id                         EVSEId,
                                                CPO.CustomEVSEIdMapperDelegate       CustomEVSEIdMapper = null)

            => CustomEVSEIdMapper != null
                   ? CustomEVSEIdMapper(EVSEId)
                   : Connector_Id.Parse(EVSEId.ToString());

        public static Connector_Id?      ToOIOI(this EVSE_Id?                        EVSEId,
                                                CPO.CustomEVSEIdMapperDelegate       CustomEVSEIdMapper = null)

            => EVSEId.HasValue
                   ? CustomEVSEIdMapper != null
                         ? CustomEVSEIdMapper(EVSEId.Value)
                         : new Connector_Id?(Connector_Id.Parse(EVSEId.ToString()))
                   : null;

        public static EVSE_Id?           ToWWCP(this Connector_Id                    ConnectorId,
                                                CPO.CustomConnectorIdMapperDelegate  CustomConnectorIdMapper = null)

            => CustomConnectorIdMapper != null
                   ? CustomConnectorIdMapper(ConnectorId)
                   : new EVSE_Id?(EVSE_Id.Parse(ConnectorId.ToString()));

        public static EVSE_Id?           ToWWCP(this Connector_Id?                   ConnectorId,
                                                CPO.CustomConnectorIdMapperDelegate  CustomConnectorIdMapper = null)

            => ConnectorId.HasValue
                   ? CustomConnectorIdMapper != null
                         ? CustomConnectorIdMapper(ConnectorId.Value)
                         : new EVSE_Id?(EVSE_Id.Parse(ConnectorId.ToString()))
                   : null;

        #endregion


        #region ToOIOI(this WWCPAddress)

        /// <summary>
        /// Maps a WWCP address to an OIOI address.
        /// </summary>
        /// <param name="WWCPAddress">A WWCP address.</param>
        public static Address ToOIOI(this org.GraphDefined.Vanaheimr.Illias.Address WWCPAddress)

            => new (WWCPAddress.Street,
                    WWCPAddress.HouseNumber,
                    WWCPAddress.City.FirstText(),
                    WWCPAddress.PostalCode,
                    WWCPAddress.Country);


        /// <summary>
        /// Maps an OIOI address type to a WWCP address type.
        /// </summary>
        /// <param name="OIOIAddress">A address type.</param>
        public static org.GraphDefined.Vanaheimr.Illias.Address ToWWCP(this Address OIOIAddress)

            => new (OIOIAddress.Street,
                    OIOIAddress.ZIP,
                    I18NString.Create(Languages.unknown, OIOIAddress.City),
                    OIOIAddress.Country,
                    OIOIAddress.StreetNumber);

        #endregion

        #region  ToOIOI(this PlugType)

        public static ConnectorTypes ToOIOI(this ChargingPlugTypes PlugType)
        {

            switch (PlugType)
            {

                case ChargingPlugTypes.TeslaConnector:
                case ChargingPlugTypes.TESLA_Roadster:
                case ChargingPlugTypes.TESLA_ModelS:
                    return ConnectorTypes.Tesla;

                case ChargingPlugTypes.TypeEFrenchStandard:
                    return ConnectorTypes.TypeE;

                case ChargingPlugTypes.TypeFSchuko:
                    return ConnectorTypes.Schuko;


                case ChargingPlugTypes.Type1Connector_CableAttached:
                    return ConnectorTypes.Type1;

                case ChargingPlugTypes.Type2Outlet:
                case ChargingPlugTypes.Type2Connector_CableAttached:
                    return ConnectorTypes.Type2;

                case ChargingPlugTypes.Type3Outlet:
                    return ConnectorTypes.Type3;


                case ChargingPlugTypes.CHAdeMO:
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

                //case PlugTypes.CCSCombo2Plug_CableAttached:


                default:
                    return ConnectorTypes.UNKNOWN;

            }

        }

        #endregion

        #region  ToOIOI(this AuthToken)

        public static RFID_Id ToOIOI(this AuthenticationToken AuthToken)
            => RFID_Id.Parse(AuthToken.ToString());

        public static RFID_Id? ToOIOI(this AuthenticationToken? AuthToken)
            => AuthToken.HasValue
                   ? RFID_Id.Parse(AuthToken.ToString())
                   : new RFID_Id?();

        #endregion

        #region  ToOIOI(this EVSEStatusType)

        public static ConnectorStatusTypes ToOIOI(this EVSEStatusTypes EVSEStatusType)
        {

            if      (EVSEStatusType == EVSEStatusTypes.Offline)
                return ConnectorStatusTypes.Offline;

            else if (EVSEStatusType == EVSEStatusTypes.Available)
                return ConnectorStatusTypes.Available;

            else if (EVSEStatusType == EVSEStatusTypes.Reserved)
                return ConnectorStatusTypes.Reserved;

            else if (EVSEStatusType == EVSEStatusTypes.Charging)
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

            else
                return ConnectorStatusTypes.Unknown;

        }

        #endregion


        #region ToOIOI(this ChargingStation, ChargingStation2Station = null)

        /// <summary>
        /// Convert a WWCP charging station into a corresponding OIOI charging station.
        /// </summary>
        /// <param name="ChargingStation">A WWCP charging station.</param>
        /// <param name="CustomOperatorIdMapper">A custom WWCP charging station operator identification to OIOI charging station operator identification mapper.</param>
        /// <param name="CustomEVSEIdMapper">A custom WWCP EVSE identification to OIOI connector identification mapper.</param>
        /// <param name="ChargingStation2Station">A delegate to process a charging station, e.g. before pushing it to a roaming provider.</param>
        /// <returns>The corresponding OIOI charging station.</returns>
        public static Station? ToOIOI(this IChargingStation                 ChargingStation,
                                      CPO.CustomOperatorIdMapperDelegate?   CustomOperatorIdMapper    = null,
                                      CPO.CustomEVSEIdMapperDelegate?       CustomEVSEIdMapper        = null,
                                      CPO.ChargingStation2StationDelegate?  ChargingStation2Station   = null)
        {

            var station = new Station(ChargingStation.Id.         ToOIOI(),
                                      ChargingStation.Name.       FirstText(),
                                      ChargingStation.GeoLocation.Value.Latitude,
                                      ChargingStation.GeoLocation.Value.Longitude,
                                      ChargingStation.Address.    ToOIOI(),
                                      new Contact(
                                          Phone: ChargingStation.Operator.HotlinePhoneNumber?.ToString(),
                                          Web:   ChargingStation.Operator.Homepage?.          ToString(),
                                          EMail: ChargingStation.Operator.EMailAddress?.      ToString()
                                      ),
                                      CustomOperatorIdMapper != null
                                          ? CustomOperatorIdMapper(ChargingStationOperator_Id.Parse(ChargingStation.Operator.Id.ToString()))
                                          : ChargingStationOperator_Id.Parse(ChargingStation.Operator.Id.ToString()),
                                      ChargingStation.OpeningTimes != null ? ChargingStation.OpeningTimes.IsOpen24Hours : true,
                                      ChargingStation.EVSEs.Select(evse => evse.ToOIOI(CustomEVSEIdMapper)),
                                      ChargingStation.Description.FirstText(),
                                      ChargingStation.OpeningTimes);

            return ChargingStation2Station is not null
                       ? ChargingStation2Station(ChargingStation, station)
                       : station;

        }

        #endregion

        #region  ToOIOI(this EVSE,            CustomEVSEIdMapper      = null)

        public static Connector? ToOIOI(this IEVSE                       EVSE,
                                        CPO.CustomEVSEIdMapperDelegate?  CustomEVSEIdMapper = null)

        {

            var connectorId = CustomEVSEIdMapper is not null
                                  ? CustomEVSEIdMapper(EVSE.Id)
                                  : EVSE.Id.ToOIOI();

            if (!connectorId.HasValue)
                return null;

            return new Connector(connectorId.Value,
                                 EVSE.ChargingConnectors.First().Plug.ToOIOI(),
                                 EVSE.MaxPower ?? 0);

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


        public static String WWCP_CDR = "WWCP.CDR";

        /// <summary>
        /// Convert a WWCP charge detail record into a corresponding OIOI charging session.
        /// </summary>
        /// <param name="ChargeDetailRecord">A WWCP charge detail record.</param>
        /// <param name="WWCPChargeDetailRecord2Session">An optional delegate to customize the transformation.</param>
        public static Session ToOIOI(this ChargeDetailRecord                 ChargeDetailRecord,
                                     Partner_Id                              PartnerId,
                                     CPO.CustomEVSEIdMapperDelegate          CustomEVSEIdMapper               = null,
                                     CPO.ChargeDetailRecord2SessionDelegate  WWCPChargeDetailRecord2Session   = null)

        {

            var connectorId = ChargeDetailRecord.EVSEId.Value.ToOIOI(CustomEVSEIdMapper);
            if (!connectorId.HasValue)
                return null;

            var CDR = new Session(Id:                 ChargeDetailRecord.SessionId.ToOIOI(),
                                  User:               ChargeDetailRecord.AuthenticationStart.AuthToken.HasValue
                                                          ? new User(ChargeDetailRecord.AuthenticationStart.AuthToken.           Value.ToString(), IdentifierTypes.RFID)
                                                          : new User(ChargeDetailRecord.AuthenticationStart.RemoteIdentification.Value.ToString(), IdentifierTypes.EVCOId),
                                  ConnectorId:        connectorId.Value,
                                  SessionInterval:    ChargeDetailRecord.SessionTime,
                                  ChargingInterval:   new StartEndDateTime(ChargeDetailRecord.EnergyMeteringValues.First().Timestamp,
                                                                           ChargeDetailRecord.EnergyMeteringValues.Last(). Timestamp),
                                  EnergyConsumed:     ChargeDetailRecord.EnergyMeteringValues.Last().Value - ChargeDetailRecord.EnergyMeteringValues.First().Value,
                                  PartnerIdentifier:  PartnerId);

            if (WWCPChargeDetailRecord2Session != null)
                CDR = WWCPChargeDetailRecord2Session(ChargeDetailRecord, CDR);

            return CDR;

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
