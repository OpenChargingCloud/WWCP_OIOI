/*
 * Copyright (c) 2014-2017 GraphDefined GmbH
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

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x.CPO
{

    /// <summary>
    /// An OIOI connector post status request.
    /// </summary>
    public class ConnectorPostStatusRequest : ARequest<ConnectorPostStatusRequest>
    {

        #region Properties

        /// <summary>
        /// A connector status.
        /// </summary>
        public ConnectorStatus  Status              { get; }

        /// <summary>
        /// The partner identifier of the partner that owns the connector.
        /// </summary>
        public Partner_Id       PartnerIdentifier   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create an OIOI connector post status JSON/HTTP request.
        /// </summary>
        /// <param name="Status">A connector status.</param>
        /// <param name="PartnerIdentifier">The partner identifier of the partner that shall be associated with this station.</param>
        /// 
        /// <param name="Timestamp">The optional timestamp of the request.</param>
        /// <param name="CancellationToken">An optional token to cancel this request.</param>
        /// <param name="EventTrackingId">An optional event tracking identification for correlating this request with other events.</param>
        /// <param name="RequestTimeout">An optional timeout for this request.</param>
        public ConnectorPostStatusRequest(ConnectorStatus        Status,
                                          Partner_Id             PartnerIdentifier,

                                          DateTime?              Timestamp           = null,
                                          CancellationToken?     CancellationToken   = null,
                                          EventTracking_Id       EventTrackingId     = null,
                                          TimeSpan?              RequestTimeout      = null)

            : base(Timestamp,
                   CancellationToken,
                   EventTrackingId,
                   RequestTimeout)

        {

            #region Initial checks

            //if (PartnerIdentifier == null)
            //    PartnerIdentifier = CPOClient.DefaultPartnerId;

            #endregion

            this.Status             = Status;
            this.PartnerIdentifier  = PartnerIdentifier;

        }

        #endregion


        #region Documentation

        // {
        //     "connector-post-status": {
        //         "connector-id":        "DE*8PS*E123456",
        //         "partner-identifier":  "123456-123456-abcdef-abc123-456def",
        //         "status":              "Available"
        //     }
        // }

        #endregion

        #region (static) Parse(ConnectorPostStatusRequestJSON, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI connector post status request.
        /// </summary>
        /// <param name="ConnectorPostStatusRequestJSON">The JSON to parse.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static ConnectorPostStatusRequest Parse(JObject              ConnectorPostStatusRequestJSON,
                                                       OnExceptionDelegate  OnException = null)
        {

            ConnectorPostStatusRequest _ConnectorPostStatusRequest;

            if (TryParse(ConnectorPostStatusRequestJSON, out _ConnectorPostStatusRequest, OnException))
                return _ConnectorPostStatusRequest;

            return null;

        }

        #endregion

        #region (static) Parse(ConnectorPostStatusRequestText, OnException = null)

        /// <summary>
        /// Parse the given text representation of an OIOI connector post status request.
        /// </summary>
        /// <param name="ConnectorPostStatusRequestText">The text to parse.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static ConnectorPostStatusRequest Parse(String               ConnectorPostStatusRequestText,
                                                       OnExceptionDelegate  OnException = null)
        {

            ConnectorPostStatusRequest _ConnectorPostStatusRequest;

            if (TryParse(ConnectorPostStatusRequestText, out _ConnectorPostStatusRequest, OnException))
                return _ConnectorPostStatusRequest;

            return null;

        }

        #endregion

        #region (static) TryParse(ConnectorPostStatusRequestJSON,  out ConnectorPostStatusRequest, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI connector post status request.
        /// </summary>
        /// <param name="ConnectorPostStatusRequestJSON">The JSON to parse.</param>
        /// <param name="ConnectorPostStatusRequest">The parsed connector post status request.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject                         ConnectorPostStatusRequestJSON,
                                       out ConnectorPostStatusRequest  ConnectorPostStatusRequest,
                                       OnExceptionDelegate             OnException  = null)
        {

            try
            {

                var ConnectorPostStatus  = ConnectorPostStatusRequestJSON["connector-post-status"];

                ConnectorPostStatusRequest = new ConnectorPostStatusRequest(
                                                 new ConnectorStatus(
                                                     Connector_Id.Parse(ConnectorPostStatus["connector-id"].Value<String>()),
                                                     ConnectorPostStatus["status"].Value<String>().AsConnectorStatusType(),
                                                     DateTime.Now
                                                 ),
                                                 Partner_Id.Parse(ConnectorPostStatus["partner-identifier"].Value<String>())
                                             );

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, ConnectorPostStatusRequestJSON, e);

                ConnectorPostStatusRequest = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(PushEVSEDataText, out PushEVSEData, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI connector post status request.
        /// </summary>
        /// <param name="ConnectorPostStatusRequestText">The text to parse.</param>
        /// <param name="ConnectorPostStatusRequest">The parsed connector post status request.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String                          ConnectorPostStatusRequestText,
                                       out ConnectorPostStatusRequest  ConnectorPostStatusRequest,
                                       OnExceptionDelegate             OnException  = null)
        {

            try
            {

                if (TryParse(JObject.Parse(ConnectorPostStatusRequestText),
                             out ConnectorPostStatusRequest,
                             OnException))

                    return true;

            }
            catch (Exception e)
            {
                OnException?.Invoke(DateTime.Now, ConnectorPostStatusRequestText, e);
            }

            ConnectorPostStatusRequest = null;
            return false;

        }

        #endregion

        #region ToJSON()

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        public JObject ToJSON()

            => new JObject(new JProperty("connector-post-status", new JObject(
                               new JProperty("connector-id",        Status.Id.        ToString()),
                               new JProperty("partner-identifier",  PartnerIdentifier.ToString()),
                               new JProperty("status",              Status.Status.    AsText())
                           )));

        #endregion


        #region Operator overloading

        #region Operator == (PushEVSEData1, PushEVSEData2)

        /// <summary>
        /// Compares two push EVSE data requests for equality.
        /// </summary>
        /// <param name="PushEVSEData1">An push EVSE data request.</param>
        /// <param name="PushEVSEData2">Another push EVSE data request.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (ConnectorPostStatusRequest PushEVSEData1, ConnectorPostStatusRequest PushEVSEData2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(PushEVSEData1, PushEVSEData2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) PushEVSEData1 == null) || ((Object) PushEVSEData2 == null))
                return false;

            return PushEVSEData1.Equals(PushEVSEData2);

        }

        #endregion

        #region Operator != (PushEVSEData1, PushEVSEData2)

        /// <summary>
        /// Compares two push EVSE data requests for inequality.
        /// </summary>
        /// <param name="PushEVSEData1">An push EVSE data request.</param>
        /// <param name="PushEVSEData2">Another push EVSE data request.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (ConnectorPostStatusRequest PushEVSEData1, ConnectorPostStatusRequest PushEVSEData2)
            => !(PushEVSEData1 == PushEVSEData2);

        #endregion

        #endregion

        #region IEquatable<ConnectorPostStatusRequest> Members

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

            var PushEVSEData = Object as ConnectorPostStatusRequest;
            if ((Object) PushEVSEData == null)
                return false;

            return Equals(PushEVSEData);

        }

        #endregion

        #region Equals(ConnectorPostStatusRequest)

        /// <summary>
        /// Compares two station post requests for equality.
        /// </summary>
        /// <param name="ConnectorPostStatusRequest">A station post request to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(ConnectorPostStatusRequest ConnectorPostStatusRequest)
        {

            if ((Object) ConnectorPostStatusRequest == null)
                return false;

            return Status.           Equals(ConnectorPostStatusRequest.Status) &&
                   PartnerIdentifier.Equals(ConnectorPostStatusRequest.PartnerIdentifier);

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

                return Status.           GetHashCode() * 5 ^
                       PartnerIdentifier.GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat("connector post status '",
                             Status.Id + " -> " + Status.Status.ToString() +
                             "' / '" +
                             PartnerIdentifier +
                             "'");

        #endregion


    }

}
