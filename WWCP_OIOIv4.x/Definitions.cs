/*
 * Copyright (c) 2016-2023 GraphDefined GmbH
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

namespace cloud.charging.open.protocols.OIOIv4_x
{

    /// <summary>
    /// Common OIOI definitions.
    /// </summary>
    public static class Definitions
    {

        /// <summary>
        /// The hostname of the default PlugSurfing API.
        /// </summary>
        public static readonly String DefaultAPI        = "api.plugsurfing.com";

        /// <summary>
        /// The hostname of the PlugSurfing developer API.
        /// </summary>
        public static readonly String DevAPI            = "dev-api.plugsurfing.com";

        /// <summary>
        /// The default URI prefix of the PlugSurfing API.
        /// </summary>
        public static readonly String DefaultURLPrefix  = "/api/v4/request";

    }

}
