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

using System;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// The type of connectors.
    /// </summary>
    [Flags]
    public enum ConnectorTypes
    {

        /// <summary>
        /// Unknown or unspecified
        /// </summary>
        UNKNOWN      = 0,

        /// <summary>
        /// Type 2
        /// </summary>
        Type2            = 1,

        /// <summary>
        /// Combo
        /// </summary>
        Combo            = 1 <<  1,

        /// <summary>
        /// Chademo
        /// </summary>
        Chademo          = 1 <<  2,

        /// <summary>
        /// Schuko
        /// </summary>
        Schuko           = 1 <<  3,

        /// <summary>
        /// Type 3
        /// </summary>
        Type3            = 1 <<  4,

        /// <summary>
        /// CEE Blue
        /// </summary>
        CeeBlue          = 1 <<  5,

        /// <summary>
        /// 3Pin Square
        /// </summary>
        ThreePinSquare   = 1 <<  6,

        /// <summary>
        /// Type 1
        /// </summary>
        Type1            = 1 <<  7,

        /// <summary>
        /// CEE Red
        /// </summary>
        CeeRed           = 1 <<  8,

        /// <summary>
        /// CEE 2 Poles
        /// </summary>
        Cee2Poles        = 1 <<  9,

        /// <summary>
        /// Tesla
        /// </summary>
        Tesla            = 1 << 10,

        /// <summary>
        /// Scame
        /// </summary>
        Scame            = 1 << 11,

        /// <summary>
        /// Nema 5
        /// </summary>
        Nema5            = 1 << 12,

        /// <summary>
        /// CEE Plus
        /// </summary>
        CeePlus          = 1 << 13,

        /// <summary>
        /// T 13
        /// </summary>
        T13              = 1 << 14,

        /// <summary>
        /// T 15
        /// </summary>
        T15              = 1 << 15,

        /// <summary>
        /// T 23
        /// </summary>
        T23              = 1 << 16,

        /// <summary>
        /// Marechal
        /// </summary>
        Marechal         = 1 << 17,

        /// <summary>
        /// Type E
        /// </summary>
        TypeE            = 1 << 18

    }

}
