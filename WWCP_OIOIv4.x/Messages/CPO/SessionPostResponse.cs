/*
 * Copyright (c) 2014-2023 GraphDefined GmbH
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

namespace cloud.charging.open.protocols.OIOIv4_x.CPO
{

#pragma warning disable CS0659
#pragma warning disable CS0661

    /// <summary>
    /// An OIOI SessionPost response.
    /// </summary>
    public class SessionPostResponse : AResponse<SessionPostRequest,
                                                 SessionPostResponse>
    {

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI SessionPost response.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Code">The response code of the corresponding SessionPost request.</param>
        /// <param name="Message">The response message of the corresponding SessionPost request.</param>
        /// <param name="CustomData">An optional read-only dictionary of customer-specific key-value pairs.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific key-value pairs.</param>
        public SessionPostResponse(SessionPostRequest                   Request,
                                   ResponseCodes                        Code,
                                   String                               Message,
                                   IReadOnlyDictionary<String, Object>  CustomData    = null,
                                   Action<SessionPostResponse>          CustomMapper  = null)

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

        #region (static) Parse   (Request, JSON,                          CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI SessionPost response.
        /// </summary>
        /// <param name="Request">The corresponding StationPost request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static SessionPostResponse Parse(SessionPostRequest                                  Request,
                                                JObject                                             JSON,
                                                CustomMapperDelegate<SessionPostResponse, Builder>  CustomMapper  = null,
                                                OnExceptionDelegate                                 OnException   = null)
        {

            if (TryParse(Request,
                         JSON,
                         out SessionPostResponse _SessionPostResponse,
                         CustomMapper,
                         OnException))
            {
                return _SessionPostResponse;
            }

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out SessionPostResponse, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI SessionPost response.
        /// </summary>
        /// <param name="Request">The corresponding StationPost request.</param>
        /// <param name="JSON">The JSON response to be parsed.</param>
        /// <param name="SessionPostResponse">The parsed StationPost response.</param>
        /// <param name="CustomMapper">An optional delegate to customize the transformation.</param>
        /// <param name="OnException">A delegate to handle exceptions.</param>
        public static Boolean TryParse(SessionPostRequest                                  Request,
                                       JObject                                             JSON,
                                       out SessionPostResponse                             SessionPostResponse,
                                       CustomMapperDelegate<SessionPostResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                 OnException   = null)
        {

            try
            {

                var ResultJSON  = JSON["result"];

                if (ResultJSON == null)
                {
                    SessionPostResponse = null;
                    return false;
                }

                SessionPostResponse = new SessionPostResponse(
                                          Request,
                                          (ResponseCodes) ResultJSON["code"].Value<Int32>(),
                                          ResultJSON["message"].Value<String>()
                                      );

                if (CustomMapper != null)
                    SessionPostResponse = CustomMapper(JSON,
                                                       SessionPostResponse.ToBuilder());

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(Timestamp.Now, JSON, e);

                SessionPostResponse = null;
                return false;

            }

        }

        #endregion


        public static SessionPostResponse

            Success(SessionPostRequest                   Request,
                    String                               Message     = null,
                    IReadOnlyDictionary<String, Object>  CustomData  = null)

                => new SessionPostResponse(Request,
                                           ResponseCodes.Success,
                                           Message ?? "Success",
                                           CustomData);


        public static SessionPostResponse

            ClientRequestError(SessionPostRequest                   Request,
                               String                               Message     = null,
                               IReadOnlyDictionary<String, Object>  CustomData  = null)

                => new SessionPostResponse(Request,
                                           ResponseCodes.ClientRequestError,
                                           Message ?? "ClientRequestError",
                                           CustomData);


        public static SessionPostResponse

            InvalidRequestFormat(SessionPostRequest                   Request,
                                 String                               Message,
                                 IReadOnlyDictionary<String, Object>  CustomData  = null)

                => new SessionPostResponse(Request,
                                           ResponseCodes.InvalidRequestFormat,
                                           Message,
                                           CustomData);


        public static SessionPostResponse

            InvalidResponseFormat(SessionPostRequest                   Request,
                                  HTTPResponse                         JSONResponse  = null,
                                  IReadOnlyDictionary<String, Object>  CustomData    = null)

                => new SessionPostResponse(Request,
                                           ResponseCodes.InvalidResponseFormat,
                                           JSONResponse?.HTTPBodyAsUTF8String?.ToString(),
                                           CustomData);


        #region Operator overloading

        #region Operator == (SessionPostResponse1, SessionPostResponse2)

        /// <summary>
        /// Compares two responses for equality.
        /// </summary>
        /// <param name="SessionPostResponse1">A response.</param>
        /// <param name="SessionPostResponse2">Another response.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (SessionPostResponse SessionPostResponse1, SessionPostResponse SessionPostResponse2)
        {

            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(SessionPostResponse1, SessionPostResponse2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) SessionPostResponse1 == null) || ((Object) SessionPostResponse2 == null))
                return false;

            return SessionPostResponse1.Equals(SessionPostResponse2);

        }

        #endregion

        #region Operator != (SessionPostResponse1, SessionPostResponse2)

        /// <summary>
        /// Compares two responses for inequality.
        /// </summary>
        /// <param name="SessionPostResponse1">A response.</param>
        /// <param name="SessionPostResponse2">Another response.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (SessionPostResponse SessionPostResponse1, SessionPostResponse SessionPostResponse2)
            => !(SessionPostResponse1 == SessionPostResponse2);

        #endregion

        #endregion

        #region IEquatable<SessionPostResponse> Members

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
            var AResponse = Object as SessionPostResponse;
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
            => String.Concat("SessionPost response: ", Code.ToString(), " / ", Message);

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
        /// A SessionPost response builder.
        /// </summary>
        public class Builder : AResponseBuilder<SessionPostRequest,
                                                SessionPostResponse>
        {

            #region Constructor(s)

            internal Builder(SessionPostResponse Response = null)

                : base(Response?.Request,
                       Response)

            { }

            #endregion

            #region (implicit) "ToImmutable()"

            /// <summary>
            /// Return an immutable SessionPost response.
            /// </summary>
            /// <param name="Builder">A SessionPost response builder.</param>
            public static implicit operator SessionPostResponse(Builder Builder)

                => new SessionPostResponse(Builder.Request,
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
