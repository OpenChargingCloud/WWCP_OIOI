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
using System.Xml.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using Newtonsoft.Json.Linq;
using org.GraphDefined.Vanaheimr.Hermod;
using System.Collections.Generic;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x.CPO
{

    /// <summary>
    /// An OIOI RFIDVerify response.
    /// </summary>
    public class RFIDVerifyResponse : AResponse<RFIDVerifyRequest,
                                                RFIDVerifyResponse>,
                                      IEquatable<RFIDVerifyResponse>
    {

        #region Properties

        /// <summary>
        /// The response of the corresponding RFIDVerify request.
        /// </summary>
        public Boolean  Success   { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI RFIDVerify response.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Success">The response of the corresponding RFIDVerify request.</param>
        /// <param name="CustomData">An optional read-only dictionary of customer-specific key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific key-value pairs.</param>
        public RFIDVerifyResponse(RFIDVerifyRequest                    Request,
                                  Boolean                              Success,
                                  IReadOnlyDictionary<String, Object>  CustomData    = null,
                                  Action<RFIDVerifyResponse>           CustomMapper  = null)

            : base(Request,
                   CustomData,
                   CustomMapper)

        {

            this.Success  = Success;

        }

        #endregion


        #region Documentation

        // {
        //     "rfid-verify": {
        //         "verified": true
        //     }
        // }

        #endregion

        #region (static) Parse   (Request, JSON,                         CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI RFIDVerify response.
        /// </summary>
        /// <param name="Request">The corresponding RFIDVerify request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static RFIDVerifyResponse Parse(RFIDVerifyRequest                                  Request,
                                               JObject                                            JSON,
                                               CustomMapperDelegate<RFIDVerifyResponse, Builder>  CustomMapper  = null,
                                               OnExceptionDelegate                                OnException   = null)
        {

            RFIDVerifyResponse _RFIDVerifyResponse;

            if (TryParse(Request, JSON, out _RFIDVerifyResponse, CustomMapper, OnException))
                return _RFIDVerifyResponse;

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out RFIDVerifyResponse, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI RFIDVerify response.
        /// </summary>
        /// <param name="Request">The corresponding RFIDVerify request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="RFIDVerifyResponse">The parsed RFIDVerify response.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static Boolean TryParse(RFIDVerifyRequest                                  Request,
                                       JObject                                            JSON,
                                       out RFIDVerifyResponse                             RFIDVerifyResponse,
                                       CustomMapperDelegate<RFIDVerifyResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                OnException   = null)
        {

            try
            {

                var InnerJSON  = JSON["rfid-verify"];

                if (InnerJSON == null)
                {
                    RFIDVerifyResponse = null;
                    return false;
                }

                RFIDVerifyResponse = new RFIDVerifyResponse(
                                         Request,
                                         InnerJSON["verified"].Value<Boolean>() == true
                                     );

                if (CustomMapper != null)
                    RFIDVerifyResponse = CustomMapper(JSON,
                                                      RFIDVerifyResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, JSON, e);

                RFIDVerifyResponse = null;
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
                   new JProperty("rfid-verify", JSONObject.Create(

                       new JProperty("verified", Success)

                   ))
               );

        #endregion


        #region Operator overloading

        #region Operator == (RFIDVerifyResponse1, RFIDVerifyResponse2)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="RFIDVerifyResponse1">A response.</param>
        /// <param name="RFIDVerifyResponse2">Another response.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (RFIDVerifyResponse RFIDVerifyResponse1, RFIDVerifyResponse RFIDVerifyResponse2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(RFIDVerifyResponse1, RFIDVerifyResponse2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) RFIDVerifyResponse1 == null) || ((Object) RFIDVerifyResponse2 == null))
                return false;

            return RFIDVerifyResponse1.Equals(RFIDVerifyResponse2);

        }

        #endregion

        #region Operator != (RFIDVerifyResponse1, RFIDVerifyResponse2)

        /// <summary>
        /// Compares two responses for inequality.
        /// </summary>
        /// <param name="RFIDVerifyResponse1">A response.</param>
        /// <param name="RFIDVerifyResponse2">Another response.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (RFIDVerifyResponse RFIDVerifyResponse1, RFIDVerifyResponse RFIDVerifyResponse2)
            => !(RFIDVerifyResponse1 == RFIDVerifyResponse2);

        #endregion

        #endregion

        #region IEquatable<RFIDVerifyResponse> Members

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
            var AResponse = Object as RFIDVerifyResponse;
            if ((Object) AResponse == null)
                return false;

            return Equals(AResponse);

        }

        #endregion

        #region Equals(RFIDVerifyResponse)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="RFIDVerifyResponse">A response to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(RFIDVerifyResponse RFIDVerifyResponse)
        {

            if ((Object) RFIDVerifyResponse == null)
                return false;

            return Success.Equals(RFIDVerifyResponse.Success);

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
            => "RFIDVerify response: " + Success.ToString();

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
        /// A RFIDVerify response builder.
        /// </summary>
        public class Builder : AResponseBuilder<RFIDVerifyRequest,
                                                RFIDVerifyResponse>
        {

            #region Properties

            /// <summary>
            /// The response of the operation.
            /// </summary>
            public Boolean  Success   { get; set; }

            #endregion

            #region Constructor(s)

            internal Builder(RFIDVerifyResponse Response = null)

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
            /// Return an immutable RFIDVerify response.
            /// </summary>
            public RFIDVerifyResponse ToImmutable()

                => new RFIDVerifyResponse(Request,
                                          Success,
                                          CustomData,
                                          CustomMapper);

            #endregion

        }

        #endregion


    }

}
