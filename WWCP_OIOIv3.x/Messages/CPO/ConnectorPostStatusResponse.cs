/*
 * Copyright (c) 2014-2017 GraphDefined GmbH
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

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x.CPO
{

    /// <summary>
    /// An OIOI ConnectorPostStatus response.
    /// </summary>
    public class ConnectorPostStatusResponse : AResponse<ConnectorPostStatusRequest,
                                                         ConnectorPostStatusResponse>
    {

        #region Properties

        /// <summary>
        /// The response of the corresponding ConnectorPostStatus request.
        /// </summary>
        public Boolean  Success   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI ConnectorPostStatus response.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Success">The response of the corresponding ConnectorPostStatus request.</param>
        /// <param name="CustomData">An optional read-only dictionary of customer-specific key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific key-value pairs.</param>
        public ConnectorPostStatusResponse(ConnectorPostStatusRequest           Request,
                                           Boolean                              Success,
                                           IReadOnlyDictionary<String, Object>  CustomData    = null,
                                           Action<ConnectorPostStatusResponse>  CustomMapper  = null)

            : base(Request,
                   CustomData,
                   CustomMapper)

        {

            this.Success  = Success;

        }

        #endregion


        #region Documentation

        // {
        //     "connector-post-status": {
        //         "success": true
        //     }
        // }

        #endregion

        #region (static) Parse   (Request, JSON,                                  CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI ConnectorPostStatus response.
        /// </summary>
        /// <param name="Request">The corresponding ConnectorPostStatus request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static ConnectorPostStatusResponse Parse(ConnectorPostStatusRequest                                  Request,
                                                        JObject                                                     JSON,
                                                        CustomMapperDelegate<ConnectorPostStatusResponse, Builder>  CustomMapper  = null,
                                                        OnExceptionDelegate                                         OnException   = null)
        {

            ConnectorPostStatusResponse _ConnectorPostStatusResponse;

            if (TryParse(Request, JSON, out _ConnectorPostStatusResponse, CustomMapper, OnException))
                return _ConnectorPostStatusResponse;

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out ConnectorPostStatusResponse, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI ConnectorPostStatus response.
        /// </summary>
        /// <param name="Request">The corresponding ConnectorPostStatus request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="ConnectorPostStatusResponse">The parsed ConnectorPostStatus response.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static Boolean TryParse(ConnectorPostStatusRequest                                  Request,
                                       JObject                                                     JSON,
                                       out ConnectorPostStatusResponse                             ConnectorPostStatusResponse,
                                       CustomMapperDelegate<ConnectorPostStatusResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                         OnException   = null)
        {

            try
            {

                var InnerJSON  = JSON["connector-post-status"];

                if (InnerJSON == null)
                {
                    ConnectorPostStatusResponse = null;
                    return false;
                }

                ConnectorPostStatusResponse = new ConnectorPostStatusResponse(
                                                  Request,
                                                  InnerJSON["success"].Value<Boolean>() == true
                                              );

                if (CustomMapper != null)
                    ConnectorPostStatusResponse = CustomMapper(JSON,
                                                      ConnectorPostStatusResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, JSON, e);

                ConnectorPostStatusResponse = null;
                return false;

            }

        }

        #endregion

        #region ToJSON()

        /// <summary>
        /// Return a JSON-representation of this object.
        /// </summary>
        public JObject ToJSON()

            => new JObject(
                   new JProperty("session", JSONObject.Create(

                       new JProperty("success", Success)

                   ))
               );

        #endregion


        #region Operator overloading

        #region Operator == (ConnectorPostStatusResponse1, ConnectorPostStatusResponse2)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="ConnectorPostStatusResponse1">A response.</param>
        /// <param name="ConnectorPostStatusResponse2">Another response.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (ConnectorPostStatusResponse ConnectorPostStatusResponse1, ConnectorPostStatusResponse ConnectorPostStatusResponse2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(ConnectorPostStatusResponse1, ConnectorPostStatusResponse2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) ConnectorPostStatusResponse1 == null) || ((Object) ConnectorPostStatusResponse2 == null))
                return false;

            return ConnectorPostStatusResponse1.Equals(ConnectorPostStatusResponse2);

        }

        #endregion

        #region Operator != (ConnectorPostStatusResponse1, ConnectorPostStatusResponse2)

        /// <summary>
        /// Compares two responses for inequality.
        /// </summary>
        /// <param name="ConnectorPostStatusResponse1">A response.</param>
        /// <param name="ConnectorPostStatusResponse2">Another response.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (ConnectorPostStatusResponse ConnectorPostStatusResponse1, ConnectorPostStatusResponse ConnectorPostStatusResponse2)
            => !(ConnectorPostStatusResponse1 == ConnectorPostStatusResponse2);

        #endregion

        #endregion

        #region IEquatable<ConnectorPostStatusResponse> Members

        #region Equals(Object)

        /// <summary>
        /// Compares two instances of this object.
        /// </summary>
        /// <param name="Object">An object to compare with.</param>
        /// <returns>true|false</returns>
        public override Boolean Equals(Object Object)
        {

            if (Object == null)
                return false;

            // Check if the given object is a response.
            var AResponse = Object as ConnectorPostStatusResponse;
            if ((Object) AResponse == null)
                return false;

            return Equals(AResponse);

        }

        #endregion

        #region Equals(ConnectorPostStatusResponse)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="ConnectorPostStatusResponse">A response to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public override Boolean Equals(ConnectorPostStatusResponse ConnectorPostStatusResponse)
        {

            if ((Object) ConnectorPostStatusResponse == null)
                return false;

            return Success.Equals(ConnectorPostStatusResponse.Success);

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
                return Success.GetHashCode();
            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()
            => "ConnectorPostStatus response: " + Success.ToString();

        #endregion


        #region ToBuilder()

        /// <summary>
        /// Create a builder to edit this response.
        /// </summary>
        public Builder ToBuilder()
            => new Builder(this);

        #endregion

        #region (class) Builder

        /// <summary>
        /// A ConnectorPostStatus response builder.
        /// </summary>
        public class Builder : AResponseBuilder<ConnectorPostStatusRequest,
                                                ConnectorPostStatusResponse>
        {

            #region Properties

            /// <summary>
            /// The response of the operation.
            /// </summary>
            public Boolean  Success   { get; set; }

            #endregion

            #region Constructor(s)

            internal Builder(ConnectorPostStatusResponse Response = null)

                : base(Response?.Request,
                       Response)

            {

                if (Response != null)
                {

                    this.Request  = Response.Request;
                    this.Success  = Response.Success;

                    if (Response.CustomData != null)
                        foreach (var item in Response.CustomData)
                            CustomData.Add(item.Key, item.Value);

                }

            }

            #endregion

            #region ToImmutable()

            /// <summary>
            /// Return an immutable ConnectorPostStatus response.
            /// </summary>
            public ConnectorPostStatusResponse ToImmutable()

                => new ConnectorPostStatusResponse(Request,
                                                   Success,
                                                   CustomData,
                                                   CustomMapper);

            #endregion

        }

        #endregion

    }

}
