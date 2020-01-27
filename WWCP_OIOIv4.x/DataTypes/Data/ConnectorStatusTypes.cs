/*
 * Copyright (c) 2016-2020 GraphDefined GmbH
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

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// The current dynamic status of an OIOI connector.
    /// </summary>
    public enum ConnectorStatusTypes
    {

        /// <summary>
        /// The status or connector is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The connector is available.
        /// </summary>
        Available,

        /// <summary>
        /// The connector is occupied.
        /// </summary>
        Occupied,

        /// <summary>
        /// The connector is offline.
        /// </summary>
        Offline,

        /// <summary>
        /// The connector is reserved.
        /// </summary>
        Reserved

    }

}
