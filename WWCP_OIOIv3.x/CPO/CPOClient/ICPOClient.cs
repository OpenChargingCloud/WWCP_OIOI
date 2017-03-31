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
using System.Threading.Tasks;

using org.GraphDefined.Vanaheimr.Hermod.HTTP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x.CPO
{

    /// <summary>
    /// The common interface of all OIOI CPO clients.
    /// </summary>
    public interface ICPOClient
    {

        #region Properties

        /// <summary>
        /// The default request timeout for this client.
        /// </summary>
        TimeSpan?        RequestTimeout     { get; }

        /// <summary>
        /// The API key for all requests.
        /// </summary>
        String           APIKey             { get; }

        /// <summary>
        /// The default communication partner identification for all requests.
        /// </summary>
        Partner_Id       DefaultPartnerId   { get; }

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
