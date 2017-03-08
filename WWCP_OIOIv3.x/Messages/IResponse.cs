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
using System.Collections.Generic;
using System.Xml.Linq;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    public delegate T CustomMapperDelegate<T>(XElement XML, T ResponseBuilder);

    public delegate TB CustomMapper2Delegate<TB>(TB ResponseBuilder);

    public delegate TB CustomMapperDelegate<T, TB>(XElement XML, TB ResponseBuilder);


    /// <summary>
    /// The common interface of an OIOI response message.
    /// </summary>
    public interface IResponse
    {

        /// <summary>
        /// The machine-readable result code.
        /// </summary>
   //     Result    Result              { get; }

        /// <summary>
        /// The timestamp of the response message creation.
        /// </summary>
        DateTime  ResponseTimestamp   { get; }

    }

}
