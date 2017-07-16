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

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// OIOI Identifier types.
    /// </summary>
    public enum IdentifierTypes
    {

        /// <summary>
        /// The identifier type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// EVCO Identification.
        /// </summary>
        EVCOId,

        /// <summary>
        /// RFID Identification.
        /// </summary>
        RFID,

        /// <summary>
        /// Username (with password).
        /// </summary>
        Username

    }

}
