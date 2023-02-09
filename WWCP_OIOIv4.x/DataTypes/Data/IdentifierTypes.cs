/*
 * Copyright (c) 2016-2023 GraphDefined GmbH
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

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x
{

    /// <summary>
    /// Extension methods for identifier types.
    /// </summary>
    public static class IdentifierTypesExtensions
    {

        #region Parse(this Text)

        /// <summary>
        /// Parse the text representation of the given identifier type.
        /// </summary>
        /// <param name="Text">A text representation of an identifier type.</param>
        public static IdentifierTypes Parse(String Text)
        {

            switch (Text.ToLower())
            {

                case "evco-id":
                    return IdentifierTypes.EVCOId;

                case "rfid":
                    return IdentifierTypes.RFID;

                case "username":
                    return IdentifierTypes.Username;

                case "token":
                    return IdentifierTypes.Token;

                default:
                    return IdentifierTypes.Unknown;

            }

        }

        #endregion

        #region AsText(this IdentifierType)

        /// <summary>
        /// Return a text representation of the given identifier type.
        /// </summary>
        /// <param name="IdentifierType">An identifier type.</param>
        public static String AsText(this IdentifierTypes IdentifierType)
        {

            switch (IdentifierType)
            {

                case IdentifierTypes.EVCOId:
                    return "evco-id";

                case IdentifierTypes.RFID:
                    return "rfid";

                case IdentifierTypes.Username:
                    return "username";

                case IdentifierTypes.Token:
                    return "token";

                default:
                    return "unknown";

            }

        }

        #endregion

    }



    /// <summary>
    /// OIOI Identifier types.
    /// </summary>
    public enum IdentifierTypes
    {

        /// <summary>
        /// The identifier type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// EVCO Identification
        /// </summary>
        EVCOId,

        /// <summary>
        /// RFID Identification
        /// </summary>
        RFID,

        /// <summary>
        /// Username (with password)
        /// </summary>
        Username,

        /// <summary>
        /// Token
        /// </summary>
        Token

    }

}
