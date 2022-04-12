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
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// An abstract response builder.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class AResponseBuilder<TRequest, TResponse>

        where TRequest  : class, IRequest
        where TResponse : class, IResponse<TResponse>

    {

        #region Properties

        /// <summary>
        /// The request leading to this response.
        /// </summary>
        public TRequest                    Request        { get; set; }

        /// <summary>
        /// The response code of the corresponding StationPost request.
        /// </summary>
        public ResponseCodes               Code           { get; set; }

        /// <summary>
        /// The response message of the corresponding StationPost request.
        /// </summary>
        public String                      Message        { get; set; }

        ///// <summary>
        ///// The response.
        ///// </summary>
        //public TResponse                   Response       { get; set; }

        /// <summary>
        /// An optional dictionary of customer-specific key-value pairs.
        /// </summary>
        public Dictionary<String, Object>  CustomData     { get; }

        /// <summary>
        /// Whether the response has customer-specific key-value pairs defined.
        /// </summary>
        public Boolean HasCustomData
            => CustomData != null && CustomData.Count > 0;

        /// <summary>
        /// An optional mapper for customer-specific key-value pairs.
        /// </summary>
        public Action<TResponse>           CustomMapper   { get; set; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new response builder.
        /// </summary>
        /// <param name="Request">The request leading to this response.</param>
        /// <param name="Response">The response.</param>
        /// <param name="CustomData">An optional dictionary of customer-specific key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific key-value pairs.</param>
        public AResponseBuilder(TRequest                    Request,
                                TResponse                   Response      = default(TResponse),
                                Dictionary<String, Object>  CustomData    = null,
                                Action<TResponse>           CustomMapper  = null)
        {

            this.Request       = Request;
            this.Code          = Response != null ? Response.Code : ResponseCodes.SystemError;
            this.Message       = Response?.Message;

            this.CustomData    = CustomData ?? new Dictionary<String, Object>();
            this.CustomMapper  = CustomMapper;

            if (Response?.CustomData != null)
                foreach (var item in Response.CustomData)
                    CustomData.Add(item.Key, item.Value);

        }

        #endregion

    }

}
