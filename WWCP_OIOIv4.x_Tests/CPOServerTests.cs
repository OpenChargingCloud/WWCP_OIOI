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
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using NUnit.Framework;
using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.Sockets;
using org.GraphDefined.Vanaheimr.Hermod.Sockets.TCP;

using cloud.charging.open.protocols.WWCP;
using cloud.charging.open.protocols.OIOIv4_x.CPO;

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x.UnitTests
{

    /// <summary>
    /// CPO Server Unit Tests.
    /// </summary>
    [TestFixture]
    public class CPOServerTests : ATests
    {

        #region Data

        private CPOServer                 _CPOServer;

        private IChargingStationOperator  CSOP01;
        private IChargingPool             CP01;
        private IChargingStation          CS01;
        private IEVSE                     EVSE01;

        #endregion

        #region Constructor(s)

        public CPOServerTests()
        {

            this._CPOServer = CPOServer.AttachToHTTPAPI(HTTPAPI);

            CSOP01 = _RoamingNetwork.CreateChargingStationOperator(WWCP.ChargingStationOperator_Id.Parse("DE*GEF"),
                                                                   I18NString.Create(Languages.de, "GraphDefined"),
                                                                   InitialAdminStatus: ChargingStationOperatorAdminStatusTypes.Operational);

            CP01   = CSOP01.CreateChargingPool   (ChargingPool_Id.   Parse("DE*GEF*P123456"));
            CS01   = CP01.  CreateChargingStation(ChargingStation_Id.Parse("DE*GEF*S123456*A"));
            EVSE01 = CS01.  CreateEVSE           (EVSE_Id.           Parse("DE*GEF*E123456*A*1"));

        }

        #endregion


        #region Test_SessionStart_1()

        [Test]
        public void Test_SessionStart_1()
        {

            var task0001  = _HTTPClient.Execute(client => client.POSTRequest(CPOServer.DefaultURLPathPrefix,
                                                                             requestbuilder => {
                                                                                 requestbuilder.Host         = HTTPHostname.Localhost;
                                                                                 requestbuilder.ContentType  = HTTPContentType.JSON_UTF8;
                                                                                 requestbuilder.Accept.Add(HTTPContentType.JSON_UTF8);
                                                                                 requestbuilder.Content      = JSONObject.Create(

                                                                                                                   new JProperty("session-start", new JObject(

                                                                                                                       new JProperty("user", new JObject(
                                                                                                                           new JProperty("identifier-type", "evco-id"),
                                                                                                                           new JProperty("identifier",      "DE-GDF-123456-7")
                                                                                                                       )),

                                                                                                                       new JProperty("connector-id",       EVSE01.Id.ToString()),
                                                                                                                       new JProperty("payment-reference",  "bitcoin")

                                                                                                                   ))

                                                                                                               ).ToUTF8Bytes();
                                                                             }),
                                                                             RequestTimeout:     Timeout,
                                                                             CancellationToken:  new CancellationTokenSource().Token);

            task0001.Wait(Timeout);
            var result0001 = task0001.Result;

            Assert.AreEqual(HTTPStatusCode.OK, result0001.HTTPStatusCode);
            Assert.AreEqual(new JObject(
                                new JProperty("session-start", new JObject(
                                    new JProperty("success", true)
                                ))
                            ).ToString(),
                            JArray.Parse(result0001.HTTPBody.ToUTF8String()).ToString());

        }

        #endregion

    }

}
