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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    public delegate TB CustomMapper2Delegate<TB>(TB ResponseBuilder);

    public delegate T CustomMapperDelegate<T, TB>(JObject JSON, TB ResponseBuilder);


    /// <summary>
    /// The common interface of a response message.
    /// </summary>
    public interface IResponse<TResponse> : IEquatable<TResponse>

        where TResponse : class

    {

        /// <summary>
        /// An optional read-only dictionary of customer-specific key-value pairs.
        /// </summary>
        IReadOnlyDictionary<String, Object>  CustomData          { get; }

        /// <summary>
        /// Whether the response has customer-specific key-value pairs defined.
        /// </summary>
        Boolean                              HasCustomData       { get; }

        /// <summary>
        /// An optional mapper for customer-specific key-value pairs.
        /// </summary>
        Action<TResponse>                    CustomMapper        { get; }

        /// <summary>
        /// The timestamp of the response message creation.
        /// </summary>
        DateTime                             ResponseTimestamp   { get; }

    }

}
