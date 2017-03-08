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

using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;

#endregion

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    /// <summary>
    /// An OIOI Result.
    /// </summary>
    public class Result : IEquatable<Result>
    {

        #region Properties

        [Mandatory]
        public UInt32  Code       { get; }

        [Mandatory]
        public String  Message    { get; }

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Create a new OIOI result.
        /// </summary>
        /// <param name="Code">The result code.</param>
        /// <param name="Message">An optional result message.</param>
        private Result(UInt32  Code,
                       String  Message  = null)
        {

            this.Code     = Code;
            this.Message  = Message;

        }

        #endregion


        #region Documentation

        // {
        //     "result": {
        //         "code":     0,
        //         "message":  "Success."
        //     }
        // }

        #endregion

        #region (static) Parse(ResultJSON)

        /// <summary>
        /// Parse the given JSON representation of an OIOI result.
        /// </summary>
        /// <param name="ResultJSON">The JSON to parse.</param>
        public static Result Parse(JObject ResultJSON)
        {

            Result _Result;

            if (TryParse(ResultJSON, out _Result))
                return _Result;

            return null;

        }

        #endregion

        #region (static) Parse(ResultText)

        /// <summary>
        /// Parse the given text representation of an OIOI result.
        /// </summary>
        /// <param name="ResultText">The text to parse.</param>
        public static Result Parse(String ResultText)
        {

            Result _Result;

            if (TryParse(ResultText, out _Result))
                return _Result;

            return null;

        }

        #endregion

        #region (static) TryParse(ResultText, out Result, OnException = null)

        /// <summary>
        /// Try to parse the given text representation of an OIOI result.
        /// </summary>
        /// <param name="ResultText">The text to parse.</param>
        /// <param name="Result">The parsed Result.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(String               ResultText,
                                       out Result             Result,
                                       OnExceptionDelegate  OnException  = null)
        {

            try
            {

                return TryParse(JObject.Parse(ResultText),
                                out Result,
                                OnException);

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, ResultText, e);

                Result = null;
                return false;

            }

        }

        #endregion

        #region (static) TryParse(ResultJSON, out Result, OnException = null)

        /// <summary>
        /// Try to parse the given JSON representation of an OIOI Result.
        /// </summary>
        /// <param name="ResultJSON">The JSON to parse.</param>
        /// <param name="Result">The parsed Result.</param>
        /// <param name="OnException">An optional delegate called whenever an exception occured.</param>
        public static Boolean TryParse(JObject              ResultJSON,
                                       out Result             Result,
                                       OnExceptionDelegate  OnException  = null)
        {

            try
            {

                Result = new Result(ResultJSON.MapValueOrFail   ("code",
                                                                 value => value.Value<UInt32>(),
                                                                 "Invalid or missing JSON property 'code'!"),

                                    ResultJSON.MapValueOrDefault("message",
                                                                 value => value.Value<String>(),
                                                                 String.Empty));

                return true;

            }
            catch (Exception e)
            {

                OnException?.Invoke(DateTime.Now, ResultJSON, e);

                Result = null;
                return false;

            }

        }

        #endregion

        #region ToJSON()

        /// <summary>
        /// Return a JSON representation of this object.
        /// </summary>
        public JObject ToJSON()

            => JSONObject.Create(
                   new JProperty("code",     Code),
                   new JProperty("message",  Message)
               );

        #endregion

        #region ToUTF8Bytes()

        /// <summary>
        /// Return a byte array representation of this object.
        /// </summary>
        public Byte[] ToUTF8Bytes()

            => ToJSON().ToUTF8Bytes();

        #endregion


        #region (static) Success(Message = null)

        /// <summary>
        /// Return a successful result having the given optional message.
        /// </summary>
        /// <param name="Message">An optional success message.</param>
        public static Result Success(String Message = null)

            => new Result(0,
                          Message ?? "Success.");

        #endregion

        #region (static) Error(Code, Message = null)

        /// <summary>
        /// Return a unsuccessful result having the given optional message.
        /// </summary>
        /// <param name="Code">The result code.</param>
        /// <param name="Message">An optional result message.</param>
        public static Result Error(UInt32  Code,
                                   String  Message  = null)

            => new Result(Code,
                          Message ?? "Error.");

        #endregion


        #region (static) UserTokenNotValid

        /// <summary>
        /// 145 - Authentication failed: User token not valid.
        /// </summary>
        public static Result UserTokenNotValid

            => new Result(145, "Authentication failed: User token not valid");

        #endregion


        #region Operator overloading

        #region Operator == (Result1, Result2)

        /// <summary>
        /// Compares two results for equality.
        /// </summary>
        /// <param name="Result1">A result.</param>
        /// <param name="Result2">Another result.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public static Boolean operator == (Result Result1, Result Result2)
        {

            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(Result1, Result2))
                return true;

            // If one is null, but not both, return false.
            if (((Object) Result1 == null) || ((Object) Result2 == null))
                return false;

            return Result1.Equals(Result2);

        }

        #endregion

        #region Operator != (Result1, Result2)

        /// <summary>
        /// Compares two results for inequality.
        /// </summary>
        /// <param name="Result1">A result.</param>
        /// <param name="Result2">Another result.</param>
        /// <returns>False if both match; True otherwise.</returns>
        public static Boolean operator != (Result Result1, Result Result2)

            => !(Result1 == Result2);

        #endregion

        #endregion

        #region IEquatable<Result> Members

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

            // Check if the given object is a result.
            var Result = Object as Result;
            if ((Object) Result == null)
                return false;

            return this.Equals(Result);

        }

        #endregion

        #region Equals(Result)

        /// <summary>
        /// Compares two results for equality.
        /// </summary>
        /// <param name="Result">A result to compare with.</param>
        /// <returns>True if both match; False otherwise.</returns>
        public Boolean Equals(Result Result)
        {

            if ((Object) Result == null)
                return false;

            return Code.   Equals(Result.Code) &&
                   Message.Equals(Result.Message);

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

                return Code.    GetHashCode() * 11 ^
                       Message. GetHashCode();

            }
        }

        #endregion

        #region (override) ToString()

        /// <summary>
        /// Return a string representation of this object.
        /// </summary>
        public override String ToString()

            => String.Concat(Code, Message.IsNotNullOrEmpty() ? " => " + Message : "");

        #endregion

    }

}