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

    /// <summary>
    /// An OIOI Object Mapper.
    /// </summary>
    public static class ObjectMapper
    {

        public static IdentifierTypes AsIdentifierTypes(this String Text)
        {

            switch (Text)
            {

                case "evco-id":
                    return IdentifierTypes.EVCOId;

                case "rfid":
                    return IdentifierTypes.RFID;

                case "username":
                    return IdentifierTypes.Username;

                default:
                    return IdentifierTypes.Unknown;

            }

        }

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

                default:
                    return "unknown";

            }

        }


    }

}