/*
 * Copyright (c) 2014-2020 GraphDefined GmbH
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
using org.GraphDefined.Vanaheimr.Hermod.JSON;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

    /// <summary>
    /// An OIOI RFID verify request.
    /// </summary>
    public class RFIDVerifyRequest : ARequest<RFIDVerifyRequest>
    {

        #region Properties

        /// <summary>
        /// A RFID identificator.
        /// </summary>
        public RFID_Id  RFIDId   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create an OIOI RFID verify JSON/HTTP request.
        /// </summary>
        /// <param name="RFIDId">A RFID identificator.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public RFIDVerifyRequest(RFID_Id              RFIDId,

                                 DateTime?            Timestamp           = null,
                                 CancellationToken?   CancellationToken   = null,
                                 EventTracking_Id     EventTrackingId     = null,
                                 TimeSpan?            RequestTimeout      = null)

            : base(Timestamp,
                   CancellationToken,
                   EventTrackingId,
                   RequestTimeout)

        {

            this.RFIDId  = RFIDId;

        }

        #endregion


        #region Documentation

        // {
        //     "rfid-verify": {
        //         "rfid": "12345678ABCDEF"
        //     }
        // }

        #endregion

        #region (static) Parse(RFIDVerifyRequestJSON, CustomRFIDVerifyRequestParser = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI RFID verify request.
        /// </summary>
        /// <param name="RFIDVerifyRequestJSON">The JSON to parse.</param>
        /// <param name="CustomRFIDVerifyRequestParser">A delegate to parse custom RFIDVerify requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static RFIDVerifyRequest Parse(JObject                                      RFIDVerifyRequestJSON,
                                              CustomJObjectParserDelegate<RFIDVerifyRequest>  CustomRFIDVerifyRequestParser   = null,
                                              OnExceptionDelegate                          OnException                     = null)
        {

            if (TryParse(RFIDVerifyRequestJSON,
                         out RFIDVerifyRequest _RFIDVerifyRequest,
                         CustomRFIDVerifyRequestParser,
                         OnException))

                return _RFIDVerifyRequest;

            return null;

        }

        #endregion

        #region (static) Parse(RFIDVerifyRequestText, CustomRFIDVerifyRequestParser = null, OnException = null)

        /// <summary>
        /// Parse the given text representation of an OIOI RFID verify request.
        /// </summary>
        /// <param name="RFIDVerifyRequestText">The text to parse.</param>
        /// <param name="CustomRFIDVerifyRequestParser">A delegate to parse custom RFIDVerify requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static RFIDVerifyRequest Parse(String                                       RFIDVerifyRequestText,
                                              CustomJObjectParserDelegate<RFIDVerifyRequest>  CustomRFIDVerifyRequestParser   = null,
                                              OnExceptionDelegate                          OnException                     = null)
        {

            if (TryParse(RFIDVerifyRequestText,
                         out RFIDVerifyRequest _RFIDVerifyRequest,
                         CustomRFIDVerifyRequestParser,
                         OnException))

                return _RFIDVerifyRequest;

            return null;

        }

        #endregion

        #region (static) TryParse(RFIDVerifyRequestJSON, out RFIDVerifyRequest, CustomRFIDVerifyRequestParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI RFID verify request.
        /// </summary>
        /// <param name="RFIDVerifyRequestJSON">The JSON to parse.</param>
        /// <param name="RFIDVerifyRequest">The parsed RFID verify request.</param>
        /// <param name="CustomRFIDVerifyRequestParser">A delegate to parse custom RFIDVerify requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject                                      RFIDVerifyRequestJSON,
                                       out RFIDVerifyRequest                        RFIDVerifyRequest,
                                       CustomJObjectParserDelegate<RFIDVerifyRequest>  CustomRFIDVerifyRequestParser   = null,
                                       OnExceptionDelegate                          OnException                     = null)
        {

            try
            {

                var RFIDVerify = RFIDVerifyRequestJSON["rfid-verify"];

                RFIDVerifyRequest = new RFIDVerifyRequest(

                                         RFID_Id.Parse(RFIDVerify["rfid"].Value<String>())

                                     );

                if (CustomRFIDVerifyRequestParser != null)
                    RFIDVerifyRequest = CustomRFIDVerifyRequestParser(RFIDVerifyRequestJSON,
                                                                      RFIDVerifyRequest);

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, RFIDVerifyRequestJSON, e);

                RFIDVerifyRequest = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(RFIDVerifyRequestText, out RFIDVerifyRequest, CustomRFIDVerifyRequestParser = null, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI RFID verify request.
        /// </summary>
        /// <param name="RFIDVerifyRequestText">The text to parse.</param>
        /// <param name="RFIDVerifyRequest">The parsed RFID verify request.</param>
        /// <param name="CustomRFIDVerifyRequestParser">A delegate to parse custom RFIDVerify requests.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String                                       RFIDVerifyRequestText,
                                       out RFIDVerifyRequest                        RFIDVerifyRequest,
                                       CustomJObjectParserDelegate<RFIDVerifyRequest>  CustomRFIDVerifyRequestParser   = null,
                                       OnExceptionDelegate                          OnException                     = null)
        {

            try
            {

                if (TryParse(JObject.Parse(RFIDVerifyRequestText),
                             out RFIDVerifyRequest,
                             CustomRFIDVerifyRequestParser,
                             OnException))

                    return true;

            }
            catch (Exception e)
            {
                OnException?.Invoke(DateTime.UtcNow, RFIDVerifyRequestText, e);
            }

            RFIDVerifyRequest = null;
            return false;

        }

        #endregion

        #region ToJSON(CustomSessionRFIDVerifyRequestSerializer = null)

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        /// <param name="CustomSessionRFIDVerifyRequestSerializer">A delegate to serialize custom RFIDVerify requests.</param>
        public JObject ToJSON(CustomJObjectSerializerDelegate<RFIDVerifyRequest>  CustomSessionRFIDVerifyRequestSerializer   = null)
        {

            var JSON = JSONObject.Create(
                           new JProperty("rfid-verify", new JObject(
                               new JProperty("rfid", RFIDId.ToString())
                           )));

            return CustomSessionRFIDVerifyRequestSerializer != null
                       ? CustomSessionRFIDVerifyRequestSerializer(this, JSON)
                       : JSON;

        }

        #endregion


        #region Operator overloading

        #region Operator == (RFIDVerify1, RFIDVerify2)

        /// <summary>
        /// Compares two push EVSE data requests for equality.
        /// </summary>
        /// <param name="RFIDVerify1">An push EVSE data request.</param>
        /// <param name="RFIDVerify2">Another push EVSE data request.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (RFIDVerifyRequest RFIDVerify1, RFIDVerifyRequest RFIDVerify2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(RFIDVerify1, RFIDVerify2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) RFIDVerify1 == null) || ((Object) RFIDVerify2 == null))
                return false;

            return RFIDVerify1.Equals(RFIDVerify2);

        }

        #endregion

        #region Operator != (RFIDVerify1, RFIDVerify2)

        /// <summary>
        /// Compares two push EVSE data requests for inequality.
        /// </summary>
        /// <param name="RFIDVerify1">An push EVSE data request.</param>
        /// <param name="RFIDVerify2">Another push EVSE data request.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (RFIDVerifyRequest RFIDVerify1, RFIDVerifyRequest RFIDVerify2)

            => !(RFIDVerify1 == RFIDVerify2);

        #endregion

        #endregion

        #region IEquatable<RFIDVerifyRequest> Members

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

            var RFIDVerify = Object as RFIDVerifyRequest;
            if ((Object) RFIDVerify == null)
                return false;

            return Equals(RFIDVerify);

        }

        #endregion

        #region Equals(RFIDVerifyRequest)

        /// <summary>
        /// Compares two RFID verify requests for equality.
        /// </summary>
        /// <param name="RFIDVerifyRequest">A RFID verify request to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(RFIDVerifyRequest RFIDVerifyRequest)
        {

            if ((Object) RFIDVerifyRequest == null)
                return false;

            return RFIDId.Equals(RFIDVerifyRequest.RFIDId);

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
                return RFIDId.GetHashCode();
            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat("RFID verify '",
                             RFIDId +
                             "'");

        #endregion

    }

}
