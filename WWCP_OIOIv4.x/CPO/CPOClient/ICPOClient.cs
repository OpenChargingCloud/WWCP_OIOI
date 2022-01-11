/*
 * Copyright (c) 2016-2022 GraphDefined GmbH
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

using System.Threading.Tasks;

using org.GraphDefined.Vanaheimr.Hermod.HTTP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

    /// <summary>
    /// The common interface of all OIOI CPO clients.
    /// </summary>
    public interface ICPOClient : IHTTPClient
    {

        #region Properties

        /// <summary>
        /// The API key for all requests.
        /// </summary>
        APIKey                           APIKey                          { get; }

        /// <summary>
        /// A delegate to select a partner identification based on the given station.
        /// </summary>
        PartnerIdForStationDelegate      StationPartnerIdSelector        { get; }

        /// <summary>
        /// A delegate to select a partner identification based on the given connector.
        /// </summary>
        PartnerIdForConnectorIdDelegate  ConnectorIdPartnerIdSelector    { get; }

        #endregion

        #region Events

        #region OnStationPostRequest/-Response

        /// <summary>
        /// An event fired whenever a request posting a charging station will be send.
        /// </summary>
        event OnStationPostRequestDelegate   OnStationPostRequest;

        /// <summary>
        /// An event fired whenever a HTTP request posting a charging station will be send.
        /// </summary>
        event ClientRequestLogHandler        OnStationPostHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging station post request had been received.
        /// </summary>
        event ClientResponseLogHandler       OnStationPostHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging station post request had been received.
        /// </summary>
        event OnStationPostResponseDelegate  OnStationPostResponse;

        #endregion

        #region OnConnectorPostStatusRequest/-Response

        /// <summary>
        /// An event fired whenever a request posting a charging connector status will be send.
        /// </summary>
        event OnConnectorPostStatusRequestDelegate   OnConnectorPostStatusRequest;

        /// <summary>
        /// An event fired whenever a HTTP request posting a charging connector status will be send.
        /// </summary>
        event ClientRequestLogHandler                OnConnectorPostStatusHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging connector status post HTTP request had been received.
        /// </summary>
        event ClientResponseLogHandler               OnConnectorPostStatusHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging connector status post HTTP request had been received.
        /// </summary>
        event OnConnectorPostStatusResponseDelegate  OnConnectorPostStatusResponse;

        #endregion

        #region OnRFIDVerifyRequest/-Response

        /// <summary>
        /// An event fired whenever a request verifying a RFID identification will be send.
        /// </summary>
        event OnRFIDVerifyRequestDelegate   OnRFIDVerifyRequest;

        /// <summary>
        /// An event fired whenever a HTTP request verifying a RFID identification will be send.
        /// </summary>
        event ClientRequestLogHandler       OnRFIDVerifyHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a RFID identification verification request had been received.
        /// </summary>
        event ClientResponseLogHandler      OnRFIDVerifyHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a RFID identification verification request had been received.
        /// </summary>
        event OnRFIDVerifyResponseDelegate  OnRFIDVerifyResponse;

        #endregion

        #region OnSessionPostRequest/-Response

        /// <summary>
        /// An event fired whenever a charging session post request will be send.
        /// </summary>
        event OnSessionPostRequestDelegate   OnSessionPostRequest;

        /// <summary>
        /// An event fired whenever a HTTP request posting charging session will be send.
        /// </summary>
        event ClientRequestLogHandler        OnSessionPostHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging session post request had been received.
        /// </summary>
        event ClientResponseLogHandler       OnSessionPostHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging session post request had been received.
        /// </summary>
        event OnSessionPostResponseDelegate  OnSessionPostResponse;

        #endregion

        #endregion


        Task<HTTPResponse<StationPostResponse>>

            StationPost(StationPostRequest Request);


        Task<HTTPResponse<ConnectorPostStatusResponse>>

            ConnectorPostStatus(ConnectorPostStatusRequest Request);


        Task<HTTPResponse<RFIDVerifyResponse>>

            RFIDVerify(RFIDVerifyRequest Request);


        Task<HTTPResponse<SessionPostResponse>>

            SessionPost(SessionPostRequest Request);


    }

}
