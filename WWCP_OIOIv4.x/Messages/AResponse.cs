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
using org.GraphDefined.Vanaheimr.Hermod;
using System;
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// An abstract response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class AResponse<TRequest, TResponse> : IResponse<TResponse>

        where TRequest  : class, IRequest
        where TResponse : class, IResponse<TResponse>

    {

        #region Properties

        /// <summary>
        /// The request leading to this response.
        /// </summary>
        public TRequest                             Request             { get; }

        /// <summary>
        /// The response code of the corresponding ConnectorPostStatus request.
        /// </summary>
        public ResponseCodes                        Code                { get; }

        /// <summary>
        /// The response message of the corresponding ConnectorPostStatus request.
        /// </summary>
        public String                               Message             { get; }

        /// <summary>
        /// An optional read-only dictionary of customer-specific key-value pairs.
        /// </summary>
        public IReadOnlyDictionary<String, Object>  CustomData          { get; protected set; }

        /// <summary>
        /// Whether the response has customer-specific key-value pairs defined.
        /// </summary>
        public Boolean HasCustomData
            => CustomData != null && CustomData.Count > 0;

        /// <summary>
        /// An optional mapper for customer-specific key-value pairs.
        /// </summary>
        public Action<TResponse>                    CustomMapper        { get; }

        /// <summary>
        /// The timestamp of the response message creation.
        /// </summary>
        public DateTime                             ResponseTimestamp   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new generic OIOI response.
        /// </summary>
        /// <param name="Request">The OIOI request leading to this result.</param>
        /// <param name="Code">The response code of the corresponding ConnectorPostStatus request.</param>
        /// <param name="Message">The response message of the corresponding ConnectorPostStatus request.</param>
        /// <param name="CustomData">An optional read-only dictionary of customer-specific key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific key-value pairs.</param>
        /// <param name="ResponseTimestamp">An optional timestamp of the response message creation.</param>
        protected AResponse(TRequest                             Request,
                            ResponseCodes                        Code,
                            String                               Message,
                            IReadOnlyDictionary<String, Object>  CustomData         = null,
                            Action<TResponse>                    CustomMapper       = null,
                            DateTime?                            ResponseTimestamp  = null)
        {

            this.Request            = Request;
            this.Code               = Code;
            this.Message            = Message;
            this.CustomData         = CustomData;
            this.CustomMapper       = CustomMapper;
            this.ResponseTimestamp  = ResponseTimestamp ?? DateTime.UtcNow;

        }

        #endregion


        #region (virtual) ToJSON()

        /// <summary>
        /// Return a JSON-representation of this object.
        /// </summary>
        public virtual JObject ToJSON()

            => JSONObject.Create(
                   new JProperty("result", JSONObject.Create(

                       new JProperty("code",     (UInt32) Code),
                       new JProperty("message",  Message)

                   ))
               );

        #endregion


        #region IEquatable<AResponse> Members

        #region Equals(TResponse)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="TResponse">Another generic response.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public virtual Boolean Equals(TResponse TResponse)
        {

            if (TResponse == null)
                return false;

            return Code.   Equals(TResponse.Code) &&
                   Message.Equals(TResponse.Message);

        }

        #endregion

        #endregion

        #region GetHashCode()

        /// <summary>
        /// Return the HashCode of this object.
        /// </summary>
        /// <returns>The HashCode of this object.</returns>
        public override Int32 GetHashCode()
        {
            unchecked
            {

                return Code.   GetHashCode() * 3 ^
                       Message.GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()
            => String.Concat(GetType().Name, " response: ", Code.ToString(), " / ", Message);

        #endregion

    }

}
