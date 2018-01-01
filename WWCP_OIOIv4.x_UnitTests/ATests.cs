/*
 * Copyright (c) 2016-2018 GraphDefined GmbH
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

using NUnit.Framework;
using Newtonsoft.Json.Linq;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.UnitTests
{

    /// <summary>
    /// Abstract Unit Tests.
    /// </summary>
    public abstract class ATests
    {

        #region Data

        protected readonly IPv4Address                                  RemoteAddress = IPv4Address.Localhost;
        //protected readonly IPv4Address                                  RemoteAddress = IPv4Address.Parse("80.148.29.35");
        //protected readonly IPv4Address                                  RemoteAddress = IPv4Address.Parse("138.201.28.98");
        //protected readonly IPPort                                       RemotePort    = IPPort.Parse(8000);
        protected readonly IPPort                                       RemotePort    = IPPort.Parse(4567);

        protected          HTTPServer<RoamingNetworks, RoamingNetwork>  HTTPAPI;
        protected          RoamingNetworks                              _RoamingNetworks;
        protected          RoamingNetwork                               _RoamingNetwork;
        //protected          WWCP_HTTPAPI                                 WWCPAPI;
        protected readonly TimeSpan                                     Timeout  = TimeSpan.FromSeconds(20);

        protected          DNSClient                                    _DNSClient;
        protected          HTTPClient                                   _HTTPClient;

        #endregion

        #region ATests()

        protected ATests()
        {

            _DNSClient = new DNSClient(SearchForIPv6DNSServers: false);

            if (RemoteAddress == IPv4Address.Localhost)
            {

                HTTPAPI = new HTTPServer<RoamingNetworks, RoamingNetwork>(
                              TCPPort:            RemotePort,
                              DefaultServerName:  "GraphDefined OIOI Unit Tests",
                              DNSClient:          _DNSClient
                          );

                HTTPAPI.AttachTCPPort(CPO.CPOClient.DefaultRemotePort);

                _RoamingNetwork   = new RoamingNetwork(RoamingNetwork_Id.Parse("PlugSurfing"));
                _RoamingNetworks  = new RoamingNetworks(_RoamingNetwork);

                HTTPAPI.TryAddTenants(HTTPHostname.Any, _RoamingNetworks);
                HTTPAPI.Start();

            }

        }

        #endregion


        #region Init()

        [OneTimeSetUp]
        public void Init()
        {

            _HTTPClient = new HTTPClient(RemoteAddress,
                                         RemotePort: RemotePort,
                                         DNSClient:  _DNSClient);

        }

        #endregion


        #region Cleanup()

        [TearDown]
        public void Cleanup()
        {

            var      URI                = "/RNs";
            String[] RoamingNetworkIds  = null;

            using (var HTTPTask  = _HTTPClient.Execute(client => client.GET(URI,
                                                                            requestbuilder => {
                                                                                requestbuilder.Host         = "localhost";
                                                                                requestbuilder.ContentType  = HTTPContentType.JSON_UTF8;
                                                                                requestbuilder.Accept.Add(HTTPContentType.JSON_UTF8);
                                                                            }),
                                                                             RequestTimeout: Timeout,
                                                                             CancellationToken: new CancellationTokenSource().Token))

            {

                HTTPTask.Wait(Timeout);

                using (var HTTPResult = HTTPTask.Result)
                {

                    Assert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode);

                    RoamingNetworkIds = JArray.Parse(HTTPResult.HTTPBody.ToUTF8String()).
                                               AsEnumerable().
                                               Select(v => (v as JObject)["RoamingNetworkId"].Value<String>()).
                                               ToArray();

                }

            }


            foreach (var RoamingNetworkId in RoamingNetworkIds)
            {

                URI = "/RNs/" + RoamingNetworkId;

                using (var HTTPTask  = _HTTPClient.Execute(client => client.DELETE(URI,
                                                                                   requestbuilder => {
                                                                                       requestbuilder.Host         = "localhost";
                                                                                       requestbuilder.ContentType  = HTTPContentType.JSON_UTF8;
                                                                                       requestbuilder.Accept.Add(HTTPContentType.JSON_UTF8);
                                                                                   }),
                                                                                    RequestTimeout: Timeout,
                                                                                    CancellationToken: new CancellationTokenSource().Token))

                {

                    HTTPTask.Wait(Timeout);

                    using (var HTTPResult = HTTPTask.Result)
                    {

                        Assert.AreEqual(HTTPStatusCode.OK, HTTPResult.HTTPStatusCode);

                    }

                }

            }



            if (RemoteAddress == IPv4Address.Localhost)
                HTTPAPI.Shutdown();

        }

        #endregion

    }

}
