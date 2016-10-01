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
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Aegir;
using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    /// <summary>
    /// An OIOI Charging Station.
    /// </summary>
    public class Station : IEquatable<Station>
    {

        #region Properties

        /// <summary>
        /// The unique identification of the charging station.
        /// </summary>
        [Mandatory]
        public ChargingStation_Id           Id                          { get; }

        /// <summary>
        /// The related WWCP charging station.
        /// </summary>
        [Mandatory]
        public ChargingStation              ChargingStation             { get; }

        /// <summary>
        /// The name of the charging station.
        /// </summary>
        [Mandatory]
        public String                       Name                        { get; }

        /// <summary>
        /// A description of the charging station.
        /// </summary>
        [Optional]
        public String                       Description                 { get; }

        /// <summary>
        /// The geo coordinate of the charging station.
        /// </summary>
        [Mandatory]
        public GeoCoordinate                GeoCoordinate               { get; }

        /// <summary>
        /// The address of the charging station.
        /// </summary>
        [Mandatory]
        public Address                      Address                     { get; }

        /// <summary>
        /// The contact options for the charging station.
        /// </summary>
        [Mandatory]
        public Contact                      Contact                     { get; }

        /// <summary>
        /// The unique identification of the charging station operator.
        /// </summary>
        [Mandatory]
        public ChargingStationOperator_Id   CPOId                       { get; }

        /// <summary>
        /// Whether this charging station is open 24 hours a day.
        /// </summary>
        [Mandatory]
        public Boolean IsOpen24Hours
        {
            get
            {

                if (OpeningTime == null)
                    return false;

                return OpeningTime.IsOpen24Hours;

            }
        }

        /// <summary>
        /// All the connectors belonging to the charging station.
        /// </summary>
        [Mandatory]
        public IEnumerable<Connector>       Connectors                  { get; }

        /// <summary>
        /// The opening times of the charging station.
        /// </summary>
        [Optional]
        public OpeningTimes                 OpeningTime                 { get; }

        /// <summary>
        /// Additional notes, for example how to find the charging station.
        /// </summary>
        [Optional]
        String                              Notes                       { get; }

        /// <summary>
        /// Whether this charging station is reservable.
        /// </summary>
        [Optional]
        Boolean                             IsReservable                { get; }

        /// <summary>
        /// On which floor the charging station is located, for example in a parking house.
        /// </summary>
        [Optional]
        Int16?                              FloorLevel                  { get; }

        /// <summary>
        /// Whether the charging is free of any charge/costs.
        /// </summary>
        [Optional]
        Boolean                             IsFreeCharge                { get; }

        /// <summary>
        /// The number of parking spots that are available at the charging station.
        /// </summary>
        [Optional]
        UInt16?                             TotalParking                { get; }

        /// <summary>
        /// Whether the charging energy is coming from renewable sources.
        /// </summary>
        [Optional]
        Boolean                             IsGreenPowerAvailable       { get; }

        /// <summary>
        /// ?
        /// </summary>
        [Optional]
        Boolean                             IsPlugInCharge              { get; }

        /// <summary>
        /// ?
        /// </summary>
        [Optional]
        Boolean                             IsRoofed                    { get; }

        /// <summary>
        /// Whether the charging station is privately owned.
        /// This has multiple implications depending on the connected partner and the station won’t show up everywhere on their platforms.
        /// For details, please contact the connected partner.
        /// </summary>
        [Optional]
        Boolean                             IsPrivate                   { get; }

        /// <summary>
        /// Soft delete this charging station and its related connectors.
        /// </summary>
        [Optional]
        Boolean                             Deleted                     { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI charging station.
        /// </summary>
        /// <param name="Id">The unique identification of the charging station.</param>
        /// <param name="ChargingStation">The related WWCP charging station.</param>
        /// <param name="Name">The name of the charging station.</param>
        /// <param name="GeoCoordinate">The geo coordinate of the charging station.</param>
        /// <param name="Address">The address of the charging station.</param>
        /// <param name="Contact">The contact options for the charging station.</param>
        /// <param name="CPOId">The unique identification of the charging station operator.</param>
        /// <param name="IsOpen24Hours">Whether this charging station is open 24 hours a day.</param>
        /// <param name="Connectors">All the connectors belonging to the charging station.</param>
        /// <param name="Description">An optional description of the charging station.</param>
        /// <param name="OpeningTime">Optional opening times of the charging station.</param>
        /// <param name="Notes">Additional notes, for example how to find the charging station.</param>
        /// <param name="IsReservable">Whether this charging station is reservable.</param>
        /// <param name="FloorLevel"> On which floor the charging station is located, for example in a parking house.</param>
        /// <param name="IsFreeCharge">Whether the charging is free of any charge/costs.</param>
        /// <param name="TotalParking">The number of parking spots that are available at the charging station.</param>
        /// <param name="IsGreenPowerAvailable">Whether the charging energy is coming from renewable sources.</param>
        /// <param name="IsPlugInCharge">?</param>
        /// <param name="IsRoofed">?</param>
        /// <param name="IsPrivate">Whether the charging station is privately owned.</param>
        /// <param name="Deleted">Soft delete this charging station and its related connectors.</param>
        public Station(ChargingStation_Id          Id,
                       ChargingStation             ChargingStation,
                       String                      Name,
                       GeoCoordinate               GeoCoordinate,
                       Address                     Address,
                       Contact                     Contact,
                       ChargingStationOperator_Id  CPOId,
                       Boolean                     IsOpen24Hours,
                       IEnumerable<Connector>      Connectors,
                       String                      Description            = null,
                       OpeningTimes                OpeningTime            = null,
                       String                      Notes                  = null,
                       Boolean                     IsReservable           = false,
                       Int16?                      FloorLevel             = null,
                       Boolean                     IsFreeCharge           = false,
                       UInt16?                     TotalParking           = null,
                       Boolean                     IsGreenPowerAvailable  = false,
                       Boolean                     IsPlugInCharge         = false,
                       Boolean                     IsRoofed               = false,
                       Boolean                     IsPrivate              = false,
                       Boolean                     Deleted                = false)

        {

            #region Initial checks

            if (Id == null)
                throw new ArgumentNullException(nameof(Id),                   "The given unique charging station identification must not be null!");

            if (Name.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(Name),                 "The given charging station name must not be null or empty!");

            if (GeoCoordinate == null)
                throw new ArgumentNullException(nameof(GeoCoordinate),        "The given geo coordinate must not be null!");

            if (Address == null)
                throw new ArgumentNullException(nameof(Address),              "The given address must not be null!");

            if (Contact == null)
                throw new ArgumentNullException(nameof(Contact),              "The given contact must not be null!");

            if (CPOId == null)
                throw new ArgumentNullException(nameof(CPOId),                "The given charging station operator identification must not be null!");

            if (Connectors == null || !Connectors.Any())
                throw new ArgumentNullException(nameof(Connectors),           "The given connectors must not be null or empty!");

            #endregion

            this.Id                     = Id;
            this.ChargingStation        = ChargingStation;
            this.Name                   = Name;
            this.GeoCoordinate          = GeoCoordinate;
            this.Address                = Address;
            this.Contact                = Contact;
            this.CPOId                  = CPOId;
            this.Connectors             = Connectors;
            this.Description            = Description   ?? String.Empty;
            this.OpeningTime            = OpeningTime   ?? (IsOpen24Hours ? OpeningTimes.Open24Hours : null);
            this.Notes                  = Notes         ?? String.Empty;
            this.IsReservable           = IsReservable;
            this.FloorLevel             = FloorLevel    ?? new Int16?();
            this.IsFreeCharge           = IsFreeCharge;
            this.TotalParking           = TotalParking  ?? new UInt16?();
            this.IsGreenPowerAvailable  = IsGreenPowerAvailable;
            this.IsPlugInCharge         = IsPlugInCharge;
            this.IsRoofed               = IsRoofed;
            this.IsPrivate              = IsPrivate;
            this.Deleted              = Deleted;

        }

        #endregion


        #region Documentation

        // {
        //
        //     "id":            "abcdef-12345",
        //     "name":          "test",
        //     "description":   "Nice station!",
        //     "latitude":      1.123,
        //     "longitude":     2.345,
        //
        //     "address": {
        //         "street":         "streetname",
        //         "street-number":  123,
        //         "city":           "Berlin",
        //         "zip":            "10243",
        //         "country":        "DE"
        //     },
        //
        //     "contact": {
        //         "phone":  "+49 30 8122321",
        //         "fax":    "+49 30 8122322",
        //         "web":    "www.example.com",
        //         "email":  "contact@example.com"
        //     },
        //
        //     "cpo-id":      "DE*8PS",
        //     "is-open-24":  false,
        //
        //     "connectors": [
        //         {
        //             "id":     "DE*8PS*E123456",
        //             "name":   "Schuko",
        //             "speed":  3.7
        //         },
        //         {
        //             "id":     "DE*8PS*E123457",
        //             "name":   "Type2",
        //             "speed":  11.1
        //         }
        //     ],
        //
        //     "open-hour-notes": [
        //         {
        //             "times": [
        //                 "07:30",
        //                 "19:00"
        //             ],
        //             "days": [
        //                 "Mo",
        //                 "Fr"
        //             ]
        //         },
        //         {
        //             "times": [
        //                 "09:00",
        //                 "15:00"
        //             ],
        //             "days": [
        //                 "Sa",
        //                 "Sa"
        //             ]
        //         }
        //     ],
        //
        //     "notes":                     "Hello world!",
        //     "is-reservable":             false,
        //     "floor-level":               1,
        //     "is-free-charge":            false,
        //     "total-parking":             2,
        //     "is-green-power-available":  false,
        //     "is-plugin-charge":          false,
        //     "is-roofed":                 false,
        //     "is-private":                false,
        //
        //     "deleted":                   true
        //
        // }

        #endregion


        #region ToJSON()

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        public JObject ToJSON()

            => JSONObject.Create(

                   new JProperty("id",                        Id.ToString()),
                   new JProperty("name",                      Name),

                   Description.IsNotNullOrEmpty()
                       ? new JProperty("description",         Description)
                       : null,

                   new JProperty("latitude",                  GeoCoordinate.Latitude. Value),
                   new JProperty("longitude",                 GeoCoordinate.Longitude.Value),

                   new JProperty("address",                   Address. ToJSON()),
                   new JProperty("contact",                   Contact. ToJSON()),
                   new JProperty("cpo-id",                    CPOId.   ToString()),
                   new JProperty("is-open-24",                IsOpen24Hours),
                   new JProperty("connectors",                JSONArray.Create(
                                                                  Connectors.Select(connector => connector.ToJSON())
                                                              )),
                   new JProperty("open-hour-notes",           ""),

                   Notes.IsNotNullOrEmpty()
                       ? new JProperty("notes",               Notes)
                       : null,

                   new JProperty("is-reservable",             IsReservable),

                   FloorLevel.HasValue
                       ? new JProperty("floor-level",         FloorLevel.Value)
                       : null,

                   new JProperty("is-free-charge",            IsFreeCharge),

                   TotalParking.HasValue
                       ? new JProperty("floor-level",         FloorLevel.Value)
                       : null,

                   new JProperty("is-green-power-available",  IsGreenPowerAvailable),
                   new JProperty("is-plugin-charge",          IsPlugInCharge),
                   new JProperty("is-roofed",                 IsRoofed),
                   new JProperty("is-private",                IsPrivate),
                   new JProperty("deleted",                   Deleted)

               );

        #endregion


        #region Operator overloading

        #region Operator == (Station1, Station2)

        /// <summary>
        /// Compares two charging stations for equality.
        /// </summary>
        /// <param name="Station1">A charging station.</param>
        /// <param name="Station2">Another charging station.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (Station Station1, Station Station2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(Station1, Station2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) Station1 == null) || ((Object) Station2 == null))
                return false;

            return Station1.Equals(Station2);

        }

        #endregion

        #region Operator != (Station1, Station2)

        /// <summary>
        /// Compares two charging stations for inequality.
        /// </summary>
        /// <param name="Station1">A charging station.</param>
        /// <param name="Station2">Another charging station.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (Station Station1, Station Station2)

            => !(Station1 == Station2);

        #endregion

        #endregion

        #region IEquatable<Station> Members

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

            // Check if the given object is a charging station.
            var Station = Object as Station;
            if ((Object) Station == null)
                return false;

            return this.Equals(Station);

        }

        #endregion

        #region Equals(Station)

        /// <summary>
        /// Compares two charging stations for equality.
        /// </summary>
        /// <param name="Station">A charging station to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(Station Station)
        {

            if ((Object) Station == null)
                return false;

            return Id.                    Equals(Station.Id)                    &&
                   Name.                  Equals(Station.Name)                  &&
                   GeoCoordinate.         Equals(Station.GeoCoordinate)         &&
                   Address.               Equals(Station.Address)               &&
                   Contact.               Equals(Station.Contact)               &&
                   CPOId.                 Equals(Station.CPOId)                 &&
                   //Connectors.            Equals(Station.City)                  &&
                   //Description.           Equals(Station.ZIP)                   &&
                   //OpeningTime.           Equals(Station.Street)                &&
                   //Notes.                 Equals(Station.StreetNumber)          &&
                   IsReservable.          Equals(Station.IsReservable)          &&
                   //FloorLevel.            Equals(Station.FloorLevel)            &&
                   IsFreeCharge.          Equals(Station.IsFreeCharge)          &&
                   //TotalParking.          Equals(Station.TotalParking)          &&
                   IsGreenPowerAvailable. Equals(Station.IsGreenPowerAvailable) &&
                   IsPlugInCharge.        Equals(Station.IsPlugInCharge)        &&
                   IsRoofed.              Equals(Station.IsRoofed)              &&
                   IsPrivate.             Equals(Station.IsPrivate);

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

                return Id.                    GetHashCode() * 79 ^
                       Name.                  GetHashCode() * 73 ^
                       GeoCoordinate.         GetHashCode() * 67 ^
                       Address.               GetHashCode() * 61 ^
                       Contact.               GetHashCode() * 59 ^
                       CPOId.                 GetHashCode() * 53 ^
                       //Connectors.            GetHashCode() * 43 ^
                       //Description.           GetHashCode() * 71 ^
                       //OpeningTime.           GetHashCode() * 47 ^
                       //Notes.                 GetHashCode() * 41 ^
                       IsReservable.          GetHashCode() * 37 ^
                       //FloorLevel.            GetHashCode() * 31 ^
                       IsFreeCharge.          GetHashCode() * 29 ^
                       //TotalParking.          GetHashCode() * 23 ^
                       IsGreenPowerAvailable. GetHashCode() * 19 ^
                       IsPlugInCharge.        GetHashCode() * 17 ^
                       IsRoofed.              GetHashCode() * 11 ^
                       IsPrivate.             GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Id, " / ", Name);

        #endregion

    }

}