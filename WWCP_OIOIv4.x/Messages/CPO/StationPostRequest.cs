/*
 * Copyright (c) 2014-2023 GraphDefined GmbH
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
using System.Threading;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.JSON;

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x.CPO
{

    /// <summary>
    /// A station post request.
    /// </summary>
    public class StationPostRequest : ARequest<StationPostRequest>
    {

        #region Properties

        /// <summary>
        /// A charging station.
        /// </summary>
        public Station     Station             { get; }

        /// <summary>
        /// The partner identifier of the partner that shall be associated with this station.
        /// </summary>
        public Partner_Id  PartnerIdentifier   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create an OIOI Station Post JSON/HTTP request.
        /// </summary>
        /// <param name="Station">A charging station.</param>
        /// <param name="PartnerIdentifier">The partner identifier of the partner that shall be associated with this station.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public StationPostRequest(Station              Station,
                                  Partner_Id           PartnerIdentifier,

                                  DateTime?            Timestamp           = null,
                                  CancellationToken?   CancellationToken   = null,
                                  EventTracking_Id     EventTrackingId     = null,
                                  TimeSpan?            RequestTimeout      = null)

            : base(Timestamp,
                   CancellationToken,
                   EventTrackingId,
                   RequestTimeout)

        {

            this.Station            = Station ?? throw new ArgumentNullException(nameof(Station), "The given charging station must not be null!");
            this.PartnerIdentifier  = PartnerIdentifier;

        }

        #endregion


        #region Documentation

        // {
        //
        //     "station-post": {
        //
        //         "station": {
        //             "id":            "abcdef-12345",
        //             "name":          "test",
        //             "description":   "Nice station!",
        //             "latitude":      1.123,
        //             "longitude":     2.345,
        //             "address": {
        //                 "street":        "streetname",
        //                 "street-number": "123a",
        //                 "city":          "Berlin",
        //                 "zip":           "10243",
        //                 "country":       "DE"
        //             },
        //             "contact": {
        //                 "phone":     "+49 30 8122321",
        //                 "fax":       "+49 30 8122322",
        //                 "web":       "www.example.com",
        //                 "email":     "contact@example.com"
        //             },
        //             "cpo-id":        "DE*8PS",
        //             "is-open-24":    false,
        //             "connectors": [
        //                 {
        //                     "id":        "DE*8PS*E123456",
        //                     "name":      "Schuko",
        //                     "speed":     3.7
        //                 },
        //                 {
        //                     "id":        "DE*8PS*E123457",
        //                     "name":      "Type2",
        //                     "speed":     11.1
        //                 }
        //             ],
        //             "open-hour-notes": [
        //                 {
        //                     "times":     ["07:30", "19:00"],
        //                     "days":      ["Mo", "Fr"]
        //                 },
        //                 {
        //                     "times":     ["09:00", "15:00"],
        //                     "days":      ["Sa", "Sa"]
        //                 }
        //             ],
        //             "notes":                     "Additional info.",
        //             "is-reservable":             false,
        //             "floor-level":               1,
        //             "is-free-charge":            false,
        //             "total-parking":             2,
        //             "is-green-power-available":  false,
        //             "is-plugin-charge":          false,
        //             "is-roofed":                 false,
        //             "is-private":                false,
        //             "deleted":                   true
        //         },
        //
        //         "partner-identifier":    "1"
        //
        //     }
        //
        // }

        #endregion

        #region (static) Parse(StationPostRequestJSON, CustomStationPostRequestParser = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI station post request.
        /// </summary>
        /// <param name="StationPostRequestJSON">The JSON to parse.</param>
        /// <param name="CustomStationPostRequestParser">A delegate to parse custom StationPost requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static StationPostRequest Parse(JObject                                          StationPostRequestJSON,
                                               CustomJObjectParserDelegate<StationPostRequest>  CustomStationPostRequestParser   = null,
                                               OnExceptionDelegate                              OnException                      = null)
        {

            if (TryParse(StationPostRequestJSON,
                         out StationPostRequest stationPostRequest,
                         CustomStationPostRequestParser,
                         OnException))
            {
                return stationPostRequest;
            }

            return null;

        }

        #endregion

        #region (static) Parse(StationPostRequestText, CustomStationPostRequestParser = null, OnException = null)

        /// <summary>
        /// Parse the given text representation of an OIOI station post request.
        /// </summary>
        /// <param name="StationPostRequestText">The text to parse.</param>
        /// <param name="CustomStationPostRequestParser">A delegate to parse custom StationPost requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static StationPostRequest Parse(String                                           StationPostRequestText,
                                               CustomJObjectParserDelegate<StationPostRequest>  CustomStationPostRequestParser   = null,
                                               OnExceptionDelegate                              OnException                      = null)
        {

            if (TryParse(StationPostRequestText,
                         out StationPostRequest stationPostRequest,
                         CustomStationPostRequestParser,
                         OnException))
            {
                return stationPostRequest;
            }

            return null;

        }

        #endregion

        #region (static) TryParse(StationPostRequestJSON, out StationPostRequest, CustomStationPostRequestParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI station post request.
        /// </summary>
        /// <param name="StationPostRequestJSON">The JSON to parse.</param>
        /// <param name="StationPostRequest">The parsed station post request.</param>
        /// <param name="CustomStationPostRequestParser">A delegate to parse custom StationPost requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject                                          StationPostRequestJSON,
                                       out StationPostRequest                           StationPostRequest,
                                       CustomJObjectParserDelegate<StationPostRequest>  CustomStationPostRequestParser   = null,
                                       OnExceptionDelegate                              OnException                      = null)
        {

            try
            {

                var StationPostJSON  = StationPostRequestJSON["station-post"];
                var Station          = StationPostJSON["station"];
                var PartnerId        = StationPostJSON["partner-identifier"];

                StationPostRequest = new StationPostRequest(

                                         null,
                                         Partner_Id.Parse(PartnerId.Value<String>())

                                     );


                if (CustomStationPostRequestParser != null)
                    StationPostRequest = CustomStationPostRequestParser(StationPostRequestJSON,
                                                                        StationPostRequest);

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now, StationPostRequestJSON, e);

                StationPostRequest = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(StationPostRequestText, out StationPostRequest, CustomStationPostRequestParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI station post request.
        /// </summary>
        /// <param name="StationPostRequestText">The text to parse.</param>
        /// <param name="StationPostRequest">The parsed station post request.</param>
        /// <param name="CustomStationPostRequestParser">A delegate to parse custom StationPost requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String                                           StationPostRequestText,
                                       out StationPostRequest                           StationPostRequest,
                                       CustomJObjectParserDelegate<StationPostRequest>  CustomStationPostRequestParser   = null,
                                       OnExceptionDelegate                              OnException                      = null)
        {

            try
            {

                if (TryParse(JObject.Parse(StationPostRequestText),
                             out StationPostRequest,
                             CustomStationPostRequestParser,
                             OnException))
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                OnException?.Invoke(org.GraphDefined.Vanaheimr.Illias.Timestamp.Now, StationPostRequestText, e);
            }

            StationPostRequest = null;
            return false;

        }

        #endregion

        #region ToJSON(CustomStationPostRequestSerializer = null, CustomStationSerializer = null, CustomAddressSerializer = null, CustomConnectorSerializer = null)

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        /// <param name="CustomStationPostRequestSerializer">A delegate to serialize custom StationPost request.</param>
        /// <param name="CustomStationSerializer">A delegate to serialize custom Station JSON objects.</param>
        /// <param name="CustomAddressSerializer">A delegate to serialize custom Address JSON objects.</param>
        /// <param name="CustomConnectorSerializer">A delegate to serialize custom Connector JSON objects.</param>
        public JObject ToJSON(CustomJObjectSerializerDelegate<StationPostRequest>  CustomStationPostRequestSerializer   = null,
                              CustomJObjectSerializerDelegate<Station>             CustomStationSerializer              = null,
                              CustomJObjectSerializerDelegate<Address>             CustomAddressSerializer              = null,
                              CustomJObjectSerializerDelegate<Connector>           CustomConnectorSerializer            = null)
        {

            var JSON = JSONObject.Create(
                           new JProperty("station-post", JSONObject.Create(

                               new JProperty("station",             Station.          ToJSON(CustomStationSerializer,
                                                                                             CustomAddressSerializer,
                                                                                             CustomConnectorSerializer)),

                               new JProperty("partner-identifier",  PartnerIdentifier.ToString())

                           )));

            return CustomStationPostRequestSerializer != null
                       ? CustomStationPostRequestSerializer(this, JSON)
                       : JSON;

        }

        #endregion


        #region Operator overloading

        #region Operator == (StationPost1, StationPost2)

        /// <summary>
        /// Compares two station post requests for equality.
        /// </summary>
        /// <param name="StationPost1">An station post request.</param>
        /// <param name="StationPost2">Another station post request.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (StationPostRequest StationPost1, StationPostRequest StationPost2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(StationPost1, StationPost2))
                return true;

            // If one is null, but not both, return false.
            if ((StationPost1 is null) || (StationPost2 is null))
                return false;

            return StationPost1.Equals(StationPost2);

        }

        #endregion

        #region Operator != (StationPost1, StationPost2)

        /// <summary>
        /// Compares two station post requests for inequality.
        /// </summary>
        /// <param name="StationPost1">An station post request.</param>
        /// <param name="StationPost2">Another station post request.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (StationPostRequest StationPost1, StationPostRequest StationPost2)

            => !(StationPost1 == StationPost2);

        #endregion

        #endregion

        #region IEquatable<StationPostRequest> Members

        #region Equals(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        /// <returns>true|false</returns>
        public override Boolean Equals(Object Object)
        {

            if (Object is null)
                return false;

            if (!(Object is StationPostRequest StationPostRequest))
                return false;

            return Equals(StationPostRequest);

        }

        #endregion

        #region Equals(StationPostRequest)

        /// <summary>
        /// Compares two station post requests for equality.
        /// </summary>
        /// <param name="StationPostRequest">A station post request to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(StationPostRequest StationPostRequest)
        {

            if (StationPostRequest is null)
                return false;

            return Station.          Equals(StationPostRequest.Station) &&
                   PartnerIdentifier.Equals(StationPostRequest.PartnerIdentifier);

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

                return Station.          GetHashCode() * 5 ^
                       PartnerIdentifier.GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat("Station Post of '",
                             Station.Id +
                             "' / '" +
                             PartnerIdentifier +
                             "'");

        #endregion

    }

}
