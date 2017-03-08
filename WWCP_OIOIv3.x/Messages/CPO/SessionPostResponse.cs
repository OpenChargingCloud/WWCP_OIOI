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
    /// An OIOI SessionPost result.
    /// </summary>
    public class SessionPostResponse : AResponse<SessionPostRequest,
                                                 SessionPostResponse>
    {

        #region Properties

        /// <summary>
        /// The result of the operation.
        /// </summary>
        public Boolean  Success   { get; }

        /// <summary>
        /// Explains what the problem was, whenever 'success' was false.
        /// </summary>
        public String   Reason    { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI SessionPost result.
        /// </summary>
        /// <param name="Request">The session post request leading to this response.</param>
        /// <param name="Success">The result of the operation.</param>
        /// <param name="Reason">Explains what the problem was, whenever 'success' was false.</param>
        /// <param name="CustomMapper">An optional mapper for customer-specific semi-structured data.</param>
        public SessionPostResponse(SessionPostRequest           Request,
                                   Boolean                      Success,
                                   String                       Reason        = null,
                                   Action<SessionPostResponse>  CustomMapper  = null)

            : base(Request,
                   CustomMapper)

        {

            this.Success  = Success;
            this.Reason   = Reason;

        }

        #endregion


        #region Documentation

        // {
        //     "session": {
        //         "success": true,
        //         "reason":  null
        //     }
        // }

        #endregion

        #region (static) Parse(JSON)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI acknowledgement.
        /// </summary>
        /// <param name="JSON">The JSON to parse.</param>
        public static SessionPostResponse Parse(SessionPostRequest                                  Request,
                                                JObject                                             JSON,
                                                CustomMapperDelegate<SessionPostResponse, Builder>  CustomMapper  = null,
                                                OnExceptionDelegate                                 OnException   = null)
        {

            SessionPostResponse _SessionPostResponse;

            if (TryParse(Request, JSON, out _SessionPostResponse, CustomMapper, OnException))
                return _SessionPostResponse;

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out Acknowledgement, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI acknowledgement.
        /// </summary>
        /// <param name="JSON">The JSON to parse.</param>
        /// <param name="SessionPostResponse">The parsed acknowledgement</param>
        public static Boolean TryParse(SessionPostRequest                                        Request,
                                       JObject                                                   JSON,
                                       out SessionPostResponse                                   SessionPostResponse,
                                       CustomMapperDelegate<SessionPostResponse, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                       OnException   = null)
        {

            try
            {

                var InnerJSON  = JSON["session"];

                if (InnerJSON == null)
                {
                    SessionPostResponse = null;
                    return false;
                }

                SessionPostResponse = new SessionPostResponse(
                                          Request,
                                          InnerJSON["success"].Value<Boolean>() == true,
                                          InnerJSON["reason" ].Value<String>()
                                      );

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, JSON, e);

                SessionPostResponse = null;
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

                       new JProperty("success",  Success),

                       Reason.IsNotNullOrEmpty()
                           ? new JProperty("reason",  Reason)
                           : null

                   ))
               );

        #endregion


        public override bool Equals(SessionPostResponse AResponse)
        {
            throw new NotImplementedException();
        }


        public class Builder : ABuilder
        {

            #region Properties

            public SessionPostRequest          Request      { get; set; }

            /// <summary>
            /// The result of the operation.
            /// </summary>
            public Boolean                     Success      { get; set; }

            /// <summary>
            /// Explains what the problem was, whenever 'success' was false.
            /// </summary>
            public String                      Reason       { get; set; }

            public Dictionary<String, Object>  CustomData   { get; set; }

            #endregion

            public Builder(SessionPostResponse SessionPostResponse = null)
            {

                if (SessionPostResponse != null)
                {

                    this.Request     = SessionPostResponse.Request;
                    this.Success     = SessionPostResponse.Success;
                    this.CustomData  = new Dictionary<String, Object>();

                    if (SessionPostResponse.CustomData != null)
                        foreach (var item in SessionPostResponse.CustomData)
                            CustomData.Add(item.Key, item.Value);

                }

            }


            //public Acknowledgement<T> ToImmutable()

            //    => new Acknowledgement<T>(Request,
            //                              Result,
            //                              StatusCode,
            //                              SessionId,
            //                              PartnerSessionId,
            //                              CustomData);

        }



    }

}
