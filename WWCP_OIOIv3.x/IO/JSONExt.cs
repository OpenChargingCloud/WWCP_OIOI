/*
 * Copyright (c) 2016 GraphDefined GmbH
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

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    public static class JSONExt
    {

        #region ValueOrDefault(this ParentJObject, PropertyName, DefaultValue = null)

        /// <summary>
        /// Return the value of the JSON property or the given default value.
        /// </summary>
        /// <param name="ParentJObject">The JSON parent object.</param>
        /// <param name="PropertyName">The property name to match.</param>
        /// <param name="DefaultValue">A default value.</param>
        public static JToken ValueOrDefault(this JObject  ParentJObject,
                                            String        PropertyName,
                                            String        DefaultValue = null)
        {

            #region Initial checks

            if (ParentJObject == null)
                return DefaultValue;

            #endregion

            JToken JSONValue = null;

            if (ParentJObject.TryGetValue(PropertyName, out JSONValue))
                return JSONValue;

            return DefaultValue;

        }

        #endregion

        #region ValueOrFail   (this ParentJObject, PropertyName, ExceptionMessage = null)

        /// <summary>
        /// Return the value of the JSON property or the given default value.
        /// </summary>
        /// <param name="ParentJObject">The JSON parent object.</param>
        /// <param name="PropertyName">The property name to match.</param>
        /// <param name="ExceptionMessage">An optional exception message.</param>
        public static JToken ValueOrFail(this JObject  ParentJObject,
                                         String        PropertyName,
                                         String        ExceptionMessage = null)
        {

            #region Initial checks

            if (ParentJObject == null)
                throw new ArgumentNullException(nameof(ParentJObject),  "The given JSON object must not be null!");

            #endregion

            JToken JSONValue = null;

            if (ParentJObject.TryGetValue(PropertyName, out JSONValue))
                return JSONValue;

            throw new Exception(ExceptionMessage.IsNotNullOrEmpty() ? ExceptionMessage : "The given JSON property does not exist!");

        }

        #endregion


        #region MapValueOrDefault(ParentJObject, PropertyName, ValueMapper, DefaultValue = null)

        /// <summary>
        /// Return the mapped value of the JSON property or the given default value.
        /// </summary>
        /// <param name="ParentJObject">The JSON parent object.</param>
        /// <param name="PropertyName">The property name to match.</param>
        /// <param name="ValueMapper">A delegate to map the JSON property value.</param>
        /// <param name="DefaultValue">A default value.</param>
        public static T MapValueOrDefault<T>(this JObject     ParentJObject,
                                             String           PropertyName,
                                             Func<JToken, T>  ValueMapper,
                                             T                DefaultValue = default(T))
        {

            #region Initial checks

            if (ParentJObject == null)
                return DefaultValue;

            #endregion

            JToken JSONValue;

            if (ParentJObject.TryGetValue(PropertyName, out JSONValue))
            {

                try
                {
                    return ValueMapper(JSONValue);
                }
#pragma warning disable RCS1075  // Avoid empty catch clause that catches System.Exception.
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch (Exception)
#pragma warning restore RECS0022
#pragma warning restore RCS1075
                { }

            }

            return DefaultValue;

        }

        #endregion

        #region MapValueOrFail   (ParentJObject, PropertyName, ValueMapper, ExceptionMessage = null)

        /// <summary>
        /// Return the mapped value of the JSON property or throw an exception
        /// having the given optional message.
        /// </summary>
        /// <param name="ParentJObject">The JSON parent object.</param>
        /// <param name="PropertyName">The property name to match.</param>
        /// <param name="ValueMapper">A delegate to map the JSON property value.</param>
        /// <param name="ExceptionMessage">An optional exception message.</param>
        public static T MapValueOrFail<T>(this JObject     ParentJObject,
                                          String           PropertyName,
                                          Func<JToken, T>  ValueMapper,
                                          String           ExceptionMessage = null)
        {

            #region Initial checks

            if (ParentJObject == null)
                throw new ArgumentNullException(nameof(ParentJObject),  "The given JSON object must not be null!");

            if (ValueMapper == null)
                throw new ArgumentNullException(nameof(ValueMapper),    "The given JSON value mapper delegate must not be null!");

            #endregion

            JToken JSONValue;

            if (ParentJObject.TryGetValue(PropertyName, out JSONValue))
            {

                try
                {
                    return ValueMapper(JSONValue);
                }
                catch (Exception e)
                {
                    throw ExceptionMessage.IsNotNullOrEmpty() ? new Exception(ExceptionMessage) : e;
                }

            }

            throw new Exception(ExceptionMessage.IsNotNullOrEmpty() ? ExceptionMessage : "The given JSON property does not exist!");

        }

        #endregion

    }

}