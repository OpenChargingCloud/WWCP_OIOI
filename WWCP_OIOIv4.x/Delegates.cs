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

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// A delegate for filtering charging stations.
    /// </summary>
    /// <param name="Station">A charging station.</param>
    public delegate Boolean IncludeStationsDelegate       (Station          Station);

    /// <summary>
    /// A delegate for filtering charging stations identifications.
    /// </summary>
    /// <param name="StationId">A charging station identification.</param>
    public delegate Boolean IncludeStationIdsDelegate     (Station_Id       StationId);

    /// <summary>
    /// A delegate for filtering connector status.
    /// </summary>
    /// <param name="ConnectorStatus">A connector status.</param>
    public delegate Boolean IncludeConnectorStatusDelegate(ConnectorStatus  ConnectorStatus);

}
