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

using Newtonsoft.Json.Linq;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

    /// <summary>
    /// A delegate which allows you to modify the JSON representation of stations before sending them upstream.
    /// </summary>
    /// <param name="Station">A station.</param>
    /// <param name="JSON">The JSON representation of a station.</param>
    public delegate JObject Station2JSONDelegate        (Station           Station,
                                                         JObject           JSON);

    /// <summary>
    /// A delegate which allows you to modify the JSON representation of connector status before sending them upstream.
    /// </summary>
    /// <param name="ConnectorStatus">A connector status.</param>
    /// <param name="JSON">The JSON representation of a connector status.</param>
    public delegate JObject ConnectorStatus2JSONDelegate(ConnectorStatus   ConnectorStatus,
                                                         JObject           JSON);

    /// <summary>
    /// A delegate which allows you to modify the JSON representation of charging sessions before sending them upstream.
    /// </summary>
    /// <param name="Session">A charging session.</param>
    /// <param name="JSON">The JSON representation of a charge detail record.</param>
    public delegate JObject Session2JSONDelegate        (Session           Session,
                                                         JObject           JSON);

    /// <summary>
    /// A delegate for post-processing JSON before sending it upstream.
    /// </summary>
    /// <param name="JSON">A JSON element to process.</param>
    public delegate JObject JSONPostProcessingDelegate  (JObject           JSON);

}
