/*
 * Copyright (c) 2014-2018 GraphDefined GmbH
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
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.CPO
{

    /// <summary>
    /// An OIOI StationPost response.
    /// </summary>
    public class StationPostResponse : AResponse<StationPostRequest,
                                                 StationPostResponse>
    {

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI StationPost result.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Code">The response code of the corresponding StationPost request.</param>
        /// <param name="Message">The response message of the corresponding StationPost request.</param>
        /// <param name="CustomData">A read-only dictionary of custom key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific semi-structured data.</param>
        private StationPostResponse(StationPostRequest                   Request,
                                    ResponseCodes                        Code,
                                    String                               Message,
                                    IReadOnlyDictionary<String, Object>  CustomData    = null,
                                    Action<StationPostResponse>          CustomMapper  = null)

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

        #region (static) Parse   (Request, JSON,                      CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI StationPost response.
        /// </summary>
        /// <param name="Request">The corresponding StationPost request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static StationPostResponse Parse(StationPostRequest                                  Request,
                                                JObject                                             JSON,
                                                CustomMapperDelegate<StationPostResponse, Builder>  CustomMapper  = null,
                                                OnExceptionDelegate                                 OnException   = null)
        {

            if (TryParse(Request,
                         JSON,
                         out StationPostResponse _StationPostResponse,
                         CustomMapper,
                         OnException))
            {
                return _StationPostResponse;
            }

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out Acknowledgement, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI StationPost response.
        /// </summary>
        /// <param name="JSON">The JSON to parse.</param>
        /// <param name="StationPostResponse">The parsed StationPost response</param>
        public static Boolean TryParse(StationPostRequest                                  Request,
                                       JObject                                             JSON,
                                       out StationPostResponse                             StationPostResponse,
                                       CustomMapperDelegate<StationPostResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                 OnException   = null)
        {

            try
            {

                var ResultJSON  = JSON["result"];

                if (ResultJSON == null)
                {
                    StationPostResponse = null;
                    return false;
                }

                StationPostResponse = new StationPostResponse(
                                          Request,
                                          (ResponseCodes) ResultJSON["code"].Value<Int32>(),
                                          ResultJSON["message"].Value<String>()
                                      );

                if (CustomMapper != null)
                    StationPostResponse = CustomMapper(JSON,
                                                       StationPostResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.UtcNow, JSON, e);

                StationPostResponse = null;
                return false;

            }

        }

        #endregion



        public static StationPostResponse

            Success(StationPostRequest                   Request,
                    String                               Message     = null,
                    IReadOnlyDictionary<String, Object>  CustomData  = null)

                => new StationPostResponse(Request,
                                           ResponseCodes.Success,
                                           Message ?? "Success",
                                           CustomData);


        public static StationPostResponse

            ClientRequestError(StationPostRequest                   Request,
                               String                               Message     = null,
                               IReadOnlyDictionary<String, Object>  CustomData  = null)

                => new StationPostResponse(Request,
                                           ResponseCodes.ClientRequestError,
                                           Message ?? "ClientRequestError",
                                           CustomData);


        public static StationPostResponse

            InvalidRequestFormat(StationPostRequest                   Request,
                                 String                               Message,
                                 IReadOnlyDictionary<String, Object>  CustomData  = null)

                => new StationPostResponse(Request,
                                           ResponseCodes.InvalidRequestFormat,
                                           Message,
                                           CustomData);


        public static StationPostResponse

            InvalidResponseFormat(StationPostRequest                   Request,
                                  HTTPResponse                         JSONResponse  = null,
                                  IReadOnlyDictionary<String, Object>  CustomData    = null)

                => new StationPostResponse(Request,
                                           ResponseCodes.InvalidResponseFormat,
                                           JSONResponse?.ToString(),
                                           CustomData);



        #region Operator overloading

        #region Operator == (StationPostResponse1, StationPostResponse2)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="StationPostResponse1">A response.</param>
        /// <param name="StationPostResponse2">Another response.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (StationPostResponse StationPostResponse1, StationPostResponse StationPostResponse2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(StationPostResponse1, StationPostResponse2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) StationPostResponse1 == null) || ((Object) StationPostResponse2 == null))
                return false;

            return StationPostResponse1.Equals(StationPostResponse2);

        }

        #endregion

        #region Operator != (StationPostResponse1, StationPostResponse2)

        /// <summary>
        /// Compares two responses for inequality.
        /// </summary>
        /// <param name="StationPostResponse1">A response.</param>
        /// <param name="StationPostResponse2">Another response.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (StationPostResponse StationPostResponse1, StationPostResponse StationPostResponse2)
            => !(StationPostResponse1 == StationPostResponse2);

        #endregion

        #endregion

        #region IEquatable<StationPostResponse> Members

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
            var AResponse = Object as StationPostResponse;
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

            => String.Concat("StationPost response: ", Code.ToString(), " / ", Message);

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
        /// A StationPost response builder.
        /// </summary>
        public class Builder : AResponseBuilder<StationPostRequest,
                                                StationPostResponse>
        {

            #region Properties

            /// <summary>
            /// The response code of the corresponding StationPost request.
            /// </summary>
            public ResponseCodes               Code         { get; set; }

            /// <summary>
            /// The response message of the corresponding StationPost request.
            /// </summary>
            public String                      Message      { get; set; }

            public Dictionary<String, Object>  CustomData   { get; set; }

            #endregion

            #region Constructor(s)

            public Builder(StationPostResponse Response = null)

                : base(Response?.Request,
                       Response)

            {

                if (Response != null)
                {

                    this.Request     = Response.Request;
                    this.Code        = Response.Code;
                    this.Message     = Response.Message;
                    this.CustomData  = new Dictionary<String, Object>();

                    if (Response.CustomData != null)
                        foreach (var item in Response.CustomData)
                            CustomData.Add(item.Key, item.Value);

                }

            }

            #endregion

            #region (implicit) "ToImmutable()"

            /// <summary>
            /// Return an immutable StationPost response.
            /// </summary>
            /// <param name="Builder">A StationPost response builder.</param>
            public static implicit operator StationPostResponse(Builder Builder)

                => new StationPostResponse(Builder.Request,
                                           Builder.Code,
                                           Builder.Message,
                                           Builder.CustomData,
                                           Builder.CustomMapper);

            #endregion

        }

        #endregion

    }

}
