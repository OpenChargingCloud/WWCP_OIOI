/*
 * Copyright (c) 2016-2018 GraphDefined GmbH
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
using System.Threading;
using System.Net.Security;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.JSON;
using org.GraphDefined.Vanaheimr.Hermod.SOAP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.EMP
{

    /// <summary>
    /// The common interface of all OIOI EMP Clients.
    /// </summary>
    public interface IEMPClient : IHTTPClient
    {

        #region Events

        #region OnStationGetSurfaceRequest/-Response

        /// <summary>
        /// An event fired whenever a charging station search request will be send.
        /// </summary>
        event OnStationGetSurfaceRequestDelegate   OnStationGetSurfaceRequest;

        /// <summary>
        /// An event fired whenever a HTTP request searching for charging stations will be send.
        /// </summary>
        event ClientRequestLogHandler              OnStationGetSurfaceHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging station search request had been received.
        /// </summary>
        event ClientResponseLogHandler             OnStationGetSurfaceHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging station search request had been received.
        /// </summary>
        event OnStationGetSurfaceResponseDelegate  OnStationGetSurfaceResponse;

        #endregion

        #region OnSessionStartRequest/-Response

        /// <summary>
        /// An event fired whenever a charging session start request will be send.
        /// </summary>
        event OnSessionStartRequestDelegate   OnSessionStartRequest;

        /// <summary>
        /// An event fired whenever a HTTP request starting a charging session will be send.
        /// </summary>
        event ClientRequestLogHandler         OnSessionStartHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging session start request had been received.
        /// </summary>
        event ClientResponseLogHandler        OnSessionStartHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging session start request had been received.
        /// </summary>
        event OnSessionStartResponseDelegate  OnSessionStartResponse;

        #endregion

        #region OnSessionStopRequest/-Response

        /// <summary>
        /// An event fired whenever a charging session stop request will be send.
        /// </summary>
        event OnSessionStopRequestDelegate   OnSessionStopRequest;

        /// <summary>
        /// An event fired whenever a HTTP request stopping a charging session will be send.
        /// </summary>
        event ClientRequestLogHandler        OnSessionStopHTTPRequest;

        /// <summary>
        /// An event fired whenever a HTTP response to a charging session stop request had been received.
        /// </summary>
        event ClientResponseLogHandler       OnSessionStopHTTPResponse;

        /// <summary>
        /// An event fired whenever a response to a charging session stop request had been received.
        /// </summary>
        event OnSessionStopResponseDelegate  OnSessionStopResponse;

        #endregion

        #endregion

    }

}
