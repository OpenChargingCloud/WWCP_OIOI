/*
 * Copyright (c) 2014-2020 GraphDefined GmbH
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
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.JSON;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

#pragma warning disable CS0659
#pragma warning disable CS0661

    /// <summary>
    /// An OIOI RFIDVerify response.
    /// </summary>
    public class RFIDVerifyResponse : AResponse<RFIDVerifyRequest,
                                                RFIDVerifyResponse>
    {

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI RFIDVerify response.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Code">The response code of the corresponding RFIDVerify request.</param>
        /// <param name="Message">The response message of the corresponding RFIDVerify request.</param>
        /// <param name="CustomData">An optional read-only dictionary of customer-specific key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific key-value pairs.</param>
        public RFIDVerifyResponse(RFIDVerifyRequest                    Request,
                                  ResponseCodes                        Code,
                                  String                               Message,
                                  IReadOnlyDictionary<String, Object>  CustomData    = null,
                                  Action<RFIDVerifyResponse>           CustomMapper  = null)

            : base(Request,
                   Code,
                   Message,
                   CustomData,
                   CustomMapper)

        { }

        #endregion


        #region Documentation

        // {
        //     "result": {
        //         "code":    0,
        //         "message": "Success."
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

            if (TryParse(Request,
                         JSON,
                         out RFIDVerifyResponse _RFIDVerifyResponse,
                         CustomMapper,
                         OnException))
            {
                return _RFIDVerifyResponse;
            }

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

                var ResultJSON  = JSON["result"];

                if (ResultJSON == null)
                {
                    RFIDVerifyResponse = null;
                    return false;
                }

                RFIDVerifyResponse = new RFIDVerifyResponse(
                                         Request,
                                         (ResponseCodes) ResultJSON["code"].Value<Int32>(),
                                         ResultJSON["message"].Value<String>()
                                     );

                if (CustomMapper != null)
                    RFIDVerifyResponse = CustomMapper(JSON,
                                                      RFIDVerifyResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, JSON, e);

                RFIDVerifyResponse = null;
                return false;

            }

        }

        #endregion


        public static RFIDVerifyResponse

            Success(RFIDVerifyRequest                    Request,
                    String                               Message     = null,
                    IReadOnlyDictionary<String, Object>  CustomData  = null)

                => new RFIDVerifyResponse(Request,
                                           ResponseCodes.Success,
                                           Message ?? "Success",
                                           CustomData);


        public static RFIDVerifyResponse

            ClientRequestError(RFIDVerifyRequest                    Request,
                               String                               Message     = null,
                               IReadOnlyDictionary<String, Object>  CustomData  = null)

                => new RFIDVerifyResponse(Request,
                                           ResponseCodes.ClientRequestError,
                                           Message ?? "ClientRequestError",
                                           CustomData);


        public static RFIDVerifyResponse

            InvalidRequestFormat(RFIDVerifyRequest                    Request,
                                 String                               Message,
                                 IReadOnlyDictionary<String, Object>  CustomData  = null)

                => new RFIDVerifyResponse(Request,
                                           ResponseCodes.InvalidRequestFormat,
                                           Message,
                                           CustomData);


        public static RFIDVerifyResponse

            InvalidResponseFormat(RFIDVerifyRequest                    Request,
                                  HTTPResponse                         JSONResponse  = null,
                                  IReadOnlyDictionary<String, Object>  CustomData    = null)

                => new RFIDVerifyResponse(Request,
                                           ResponseCodes.InvalidResponseFormat,
                                           JSONResponse?.ToString(),
                                           CustomData);


        #region ToJSON(CustomRFIDVerifyResponseSerializer = null)

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        /// <param name="CustomRFIDVerifyResponseSerializer">A delegate to serialize custom RFIDVerifyResponse JSON objects.</param>
        public JObject ToJSON(CustomJObjectSerializerDelegate<RFIDVerifyResponse> CustomRFIDVerifyResponseSerializer   = null)

        {

            var JSON = JSONObject.Create(

                           new JProperty("code",           Code.ToString()),

                           Message.IsNotNullOrEmpty()
                               ? new JProperty("message",  Message)
                               : null

                       );

            return CustomRFIDVerifyResponseSerializer != null
                       ? CustomRFIDVerifyResponseSerializer(this, JSON)
                       : JSON;

        }

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
            if (ReferenceEquals(RFIDVerifyResponse1, RFIDVerifyResponse2))
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

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a text representation of this object.
        /// </summary>
        public override String ToString()
            => String.Concat("RFIDVerify response: ", Code.ToString(), " / ", Message);

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

            #region Constructor(s)

            internal Builder(RFIDVerifyResponse Response = null)

                : base(Response?.Request,
                       Response)

            { }

            #endregion

            #region (implicit) "ToImmutable()"

            /// <summary>
            /// Return an immutable RFIDVerify response.
            /// </summary>
            /// <param name="Builder">A RFIDVerify response builder.</param>
            public static implicit operator RFIDVerifyResponse(Builder Builder)

                => new RFIDVerifyResponse(Builder.Request,
                                          Builder.Code,
                                          Builder.Message,
                                          Builder.CustomData,
                                          Builder.CustomMapper);

            #endregion

        }

        #endregion

    }

#pragma warning restore CS0661
#pragma warning restore CS0659

}
