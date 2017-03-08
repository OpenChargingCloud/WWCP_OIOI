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
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using org.GraphDefined.Vanaheimr.Illias;
using Newtonsoft.Json.Linq;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    public class Acknowledgement<TRequest> : AResponse<TRequest,
                                                       Acknowledgement<TRequest>>

        where TRequest : class, IRequest

    {

        #region Properties

        /// <summary>
        /// The result of the operation.
        /// </summary>
        public Boolean             Success              { get; }

        #endregion

        #region Constructor(s)

        #region Acknowledgement(Request, Result, ...)

        /// <summary>
        /// Create a new OIOI acknowledgement.
        /// </summary>
        /// <param name="Request">The request leading to this response.</param>
        /// <param name="Result">The result of the operation.</param>
        public Acknowledgement(TRequest                        Request,
                                Boolean                         Result,
                                //IReadOnlyDictionary<String, Object> CustomData = null,
                                CustomMapper2Delegate<Builder>  CustomMapper      = null)

            : base(Request)

        {

            #region Initial checks

            if (Request == null)
                throw new ArgumentNullException(nameof(Request),  "The given request object must not be null!");

            #endregion

            this.Success            = Result;

            if (CustomMapper != null)
            {

                var Builder = CustomMapper.Invoke(new Builder(this));

                this.Success            = Builder.Success;
                this.CustomData        = Builder.CustomData;

            }

        }

        #endregion

        #region Acknowledgement(Request, SessionId, ...)

        /// <summary>
        /// Create a new OIOI 'positive' acknowledgement.
        /// </summary>
        /// <param name="Request">The request leading to this response.</param>
        public Acknowledgement(TRequest                        Request,
                               CustomMapper2Delegate<Builder>  CustomMapper = null)

            : this(Request,
                   true,
                   CustomMapper)

        { }

        #endregion

        #endregion


        #region Documentation

        // {
        //     "station-post": {
        //         "success": true
        //     }
        // }

        #endregion

        #region (static) Parse(JSON)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI acknowledgement.
        /// </summary>
        /// <param name="JSON">The JSON to parse.</param>
        public static Acknowledgement<TRequest> Parse(TRequest                                                  Request,
                                                      JObject                                                   JSON,
                                                      String                                                    PropertyKey,
                                                      String                                                    PropertyKey2,
                                                      CustomMapperDelegate<Acknowledgement<TRequest>, Builder>  CustomMapper  = null,
                                                      OnExceptionDelegate                                       OnException   = null)
        {

            Acknowledgement<TRequest> _Acknowledgement;

            if (TryParse(Request, JSON, PropertyKey, PropertyKey2, out _Acknowledgement, CustomMapper, OnException))
                return _Acknowledgement;

            return null;

        }

        #endregion

        #region (static) TryParse(Request, JSON, out Acknowledgement, CustomMapper = null, OnException = null)

        /// <summary>
        /// Parse the given JSON representation of an OIOI acknowledgement.
        /// </summary>
        /// <param name="JSON">The JSON to parse.</param>
        /// <param name="Acknowledgement">The parsed acknowledgement</param>
        public static Boolean TryParse(TRequest                                                  Request,
                                       JObject                                                   JSON,
                                       String                                                    PropertyKey,
                                       String                                                    PropertyKey2,
                                       out Acknowledgement<TRequest>                             Acknowledgement,
                                       CustomMapperDelegate<Acknowledgement<TRequest>, Builder>  CustomMapper  = null,
                                       OnExceptionDelegate                                       OnException   = null)
        {

            try
            {

                var InnerJSON  = JSON[PropertyKey];

                if (InnerJSON == null)
                {
                    Acknowledgement = null;
                    return false;
                }

                Acknowledgement = new Acknowledgement<TRequest>(
                                      Request,
                                      InnerJSON[PropertyKey2].Value<Boolean>() == true
                                  );

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, JSON, e);

                Acknowledgement = null;
                return false;

            }

        }

        #endregion

        #region ToJSON(PropertyKey)

        /// <summary>
        /// Return a JSON-representation of this object.
        /// </summary>
        public JObject ToJSON(String PropertyKey)

            => new JObject(
                   new JProperty(PropertyKey, new JObject(
                       new JProperty("success",  Success)
                   ))
               );

        #endregion


        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()
            => "Success = " + Success;

        #endregion


        public override bool Equals(Acknowledgement<TRequest> ARequest)
        {
            throw new NotImplementedException();
        }


        public class Builder : ABuilder
        {

            #region Properties

            public TRequest                    Request      { get; set; }

            /// <summary>
            /// The result of the operation.
            /// </summary>
            public Boolean                     Success      { get; set; }

            public Dictionary<String, Object>  CustomData   { get; set; }

            #endregion

            public Builder(Acknowledgement<TRequest> Acknowledgement = null)
            {

                if (Acknowledgement != null)
                {

                    this.Request     = Acknowledgement.Request;
                    this.Success     = Acknowledgement.Success;
                    this.CustomData  = new Dictionary<String, Object>();

                    if (Acknowledgement.CustomData != null)
                        foreach (var item in Acknowledgement.CustomData)
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


    /// <summary>
    /// An OIOI Acknowledgement.
    /// </summary>
    public class Acknowledgement
    {

        #region Properties

        /// <summary>
        /// The result of the operation.
        /// </summary>
        public Boolean Success { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI acknowledgement.
        /// </summary>
        /// <param name="Success">The result of the operation.</param>
        public Acknowledgement(Boolean Success)
        {

            this.Success = Success;

        }

        #endregion


        #region Documentation

        // {
        //     "station-post": {
        //         "success": true
        //     }
        // }

        #endregion

        #region (static) Parse(JSON)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI acknowledgement.
        /// </summary>
        /// <param name="JSON">The JSON to parse.</param>
        public static Acknowledgement Parse(JObject JSON)
        {

            Acknowledgement _Acknowledgement;

            if (TryParse(JSON, out _Acknowledgement))
                return _Acknowledgement;

            return null;

        }

        #endregion

        #region (static) TryParse(JSON, out Acknowledgement)

        /// <summary>
        /// Parse the given JSON representation of an OIOI acknowledgement.
        /// </summary>
        /// <param name="JSON">The JSON to parse.</param>
        /// <param name="Acknowledgement">The parsed acknowledgement</param>
        public static Boolean TryParse(JObject              JSON,
                                       String               PropertyKey,
                                       out Acknowledgement  Acknowledgement,
                                       OnExceptionDelegate  OnException = null)
        {

            try
            {

                var InnerJSON  = JSON[PropertyKey];

                if (InnerJSON == null)
                {
                    Acknowledgement = null;
                    return false;
                }

                Acknowledgement = new Acknowledgement(
                                      InnerJSON["success"].Value<Boolean>() == true
                                  );

                return true;

            }
            catch (Exception e)
            {


                OnException?.Invoke(DateTime.Now, JSON, e);

                Acknowledgement = null;

                return false;

            }

        }

        #endregion

        #region ToJSON(PropertyKey)

        /// <summary>
        /// Return a JSON-representation of this object.
        /// </summary>
        public JObject ToJSON(String PropertyKey)

            => new JObject(
                   new JProperty(PropertyKey, new JObject(
                       new JProperty("success",  Success)
                   ))
               );

        #endregion


        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()
            => "Success = " + Success;

        #endregion

    }

}

