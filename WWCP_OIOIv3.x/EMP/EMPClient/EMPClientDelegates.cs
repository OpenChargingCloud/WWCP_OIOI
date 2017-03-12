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
using System.Threading.Tasks;

using org.GraphDefined.Vanaheimr.Illias;
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x.EMP
{

    ///// <summary>
    ///// A delegate for filtering charging stations.
    ///// </summary>
    ///// <param name="Station">A OIOI charging station.</param>
    //public delegate Boolean IncludeStationsDelegate       (Station          Station);

    ///// <summary>
    ///// A delegate for filtering connector status records.
    ///// </summary>
    ///// <param name="ConnectorStatus">An OIOI connector status.</param>
    //public delegate Boolean IncludeConnectorStatusDelegate(ConnectorStatus  ConnectorStatus);


    #region OnStationGetSurfaceRequest/-Response

    /// <summary>
    /// A delegate called whenever a charging station search request will be send upstream.
    /// </summary>
    public delegate Task OnStationGetSurfaceRequestDelegate (DateTime                      LogTimestamp,
                                                             DateTime                      RequestTimestamp,
                                                             EMPClient                     Sender,
                                                             String                        SenderId,
                                                             EventTracking_Id              EventTrackingId,
                                                             Single                        MinLat,
                                                             Single                        MaxLat,
                                                             Single                        MinLong,
                                                             Single                        MaxLong,
                                                             IEnumerable<ConnectorTypes>   IncludeConnectorTypes,
                                                             TimeSpan?                     RequestTimeout);

    /// <summary>
    /// A delegate called whenever a charging station search request had been sent upstream.
    /// </summary>
    public delegate Task OnStationGetSurfaceResponseDelegate(DateTime                      LogTimestamp,
                                                             DateTime                      RequestTimestamp,
                                                             EMPClient                     Sender,
                                                             String                        SenderId,
                                                             EventTracking_Id              EventTrackingId,
                                                             Single                        MinLat,
                                                             Single                        MaxLat,
                                                             Single                        MinLong,
                                                             Single                        MaxLong,
                                                             IEnumerable<ConnectorTypes>   IncludeConnectorTypes,
                                                             TimeSpan?                     RequestTimeout,
                                                             StationGetSurfaceResponse     Result,
                                                             TimeSpan                      Duration);

    #endregion


}
