/*
 * Copyright (c) 2014-2020 GraphDefined GmbH
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
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using org.GraphDefined.WWCP.OIOIv4_x.CPO;

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod.DNS;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.SOAP;

#endregion

namespace org.GraphDefined.WWCP.OIOIv4_x.WebAPI
{

    /// <summary>
    /// OIOI+ HTTP API extention methods.
    /// </summary>
    public static class ExtentionMethods
    {

        #region ParseRoamingNetwork(this HTTPRequest, HTTPServer, out RoamingNetwork, out HTTPResponse)

        /// <summary>
        /// Parse the given HTTP request and return the roaming network
        /// for the given HTTP hostname and HTTP query parameter
        /// or an HTTP error response.
        /// </summary>
        /// <param name="HTTPRequest">A HTTP request.</param>
        /// <param name="HTTPServer">A HTTP server.</param>
        /// <param name="RoamingNetwork">The roaming network.</param>
        /// <param name="HTTPResponse">A HTTP error response.</param>
        /// <returns>True, when roaming network was found; false else.</returns>
        public static Boolean ParseRoamingNetwork(this HTTPRequest                             HTTPRequest,
                                                  HTTPServer<RoamingNetworks, RoamingNetwork>  HTTPServer,
                                                  out RoamingNetwork                           RoamingNetwork,
                                                  out HTTPResponse                             HTTPResponse)
        {

            if (HTTPServer == null)
                Console.WriteLine("HTTPServer == null!");

            #region Initial checks

            if (HTTPRequest == null)
                throw new ArgumentNullException("HTTPRequest",  "The given HTTP request must not be null!");

            if (HTTPServer == null)
                throw new ArgumentNullException("HTTPServer",   "The given HTTP server must not be null!");

            #endregion

            RoamingNetwork_Id RoamingNetworkId;
                              RoamingNetwork    = null;
                              HTTPResponse      = null;

            if (HTTPRequest.ParsedURLParameters.Length < 1)
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                };

                return false;

            }

            if (!RoamingNetwork_Id.TryParse(HTTPRequest.ParsedURLParameters[0], out RoamingNetworkId))
            {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.BadRequest,
                    Server          = HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Invalid RoamingNetworkId!"" }".ToUTF8Bytes()
                };

                return false;

            }

            RoamingNetwork  = HTTPServer.
                                  GetAllTenants(HTTPRequest.Host).
                                  FirstOrDefault(roamingnetwork => roamingnetwork.Id == RoamingNetworkId);

            if (RoamingNetwork == null) {

                HTTPResponse = new HTTPResponse.Builder(HTTPRequest) {
                    HTTPStatusCode  = HTTPStatusCode.NotFound,
                    Server          = HTTPServer.DefaultServerName,
                    Date            = DateTime.UtcNow,
                    ContentType     = HTTPContentType.JSON_UTF8,
                    Content         = @"{ ""description"": ""Unknown RoamingNetworkId!"" }".ToUTF8Bytes()
                };

                return false;

            }

            return true;

        }

        #endregion

    }


    /// <summary>
    /// A HTTP API providing OIOI+ data structures.
    /// </summary>
    public class OIOIWebAPI
    {

        #region Data

        /// <summary>
        /// The default HTTP URI prefix.
        /// </summary>
        public static readonly HTTPPath                             DefaultURLPathPrefix        = HTTPPath.Parse("/ext/OIOIPlus");

        /// <summary>
        /// The default HTTP realm, if HTTP Basic Authentication is used.
        /// </summary>
        public const           String                               DefaultHTTPRealm            = "Open Charging Cloud OIOIPlus WebAPI";

        //ToDo: http://www.iana.org/form/media-types

        /// <summary>
        /// The HTTP content type for serving OIOI+ XML data.
        /// </summary>
        public static readonly HTTPContentType                      OIOIPlusXMLContentType      = new HTTPContentType("application", "vnd.OIOIPlus+xml", "utf-8", null, null);

        /// <summary>
        /// The HTTP content type for serving OIOI+ HTML data.
        /// </summary>
        public static readonly HTTPContentType                      OIOIPlusHTMLContentType     = new HTTPContentType("application", "vnd.OIOIPlus+html", "utf-8", null, null);


        private readonly XMLNamespacesDelegate                      XMLNamespaces;
        //private readonly EVSE2EVSEDataRecordDelegate                EVSE2EVSEDataRecord;
        //private readonly EVSEStatusUpdate2EVSEStatusRecordDelegate  EVSEStatusUpdate2EVSEStatusRecord;
        //private readonly EVSEDataRecord2XMLDelegate                 EVSEDataRecord2XML;
        //private readonly EVSEStatusRecord2XMLDelegate               EVSEStatusRecord2XML;
        private readonly XMLPostProcessingDelegate                  XMLPostProcessing;

        public static readonly HTTPEventSource_Id                   DebugLogId                  = HTTPEventSource_Id.Parse("OIOIDebugLog");

        #endregion

        #region Properties

        /// <summary>
        /// The HTTP server for serving the OIOI+ WebAPI.
        /// </summary>
        public HTTPServer<RoamingNetworks, RoamingNetwork>  HTTPServer          { get; }

        /// <summary>
        /// The HTTP URI prefix.
        /// </summary>
        public HTTPPath                                     URLPathPrefix       { get; }

        /// <summary>
        /// The HTTP realm, if HTTP Basic Authentication is used.
        /// </summary>
        public String                                       HTTPRealm           { get; }

        /// <summary>
        /// An enumeration of logins for an optional HTTP Basic Authentication.
        /// </summary>
        public IEnumerable<KeyValuePair<String, String>>    HTTPLogins          { get; }


        /// <summary>
        /// Send debug information via HTTP Server Sent Events.
        /// </summary>
        public HTTPEventSource<JObject>                     DebugLog            { get; }


        /// <summary>
        /// The DNS client to use.
        /// </summary>
        public DNSClient                                    DNSClient           { get; }



        private readonly List<WWCPCPOAdapter> _CPOAdapters;

        public IEnumerable<WWCPCPOAdapter> CPOAdapters
            => _CPOAdapters;

        #endregion

        #region Events

        #region Generic HTTP/SOAP server logging

        /// <summary>
        /// An event called whenever a HTTP request came in.
        /// </summary>
        public HTTPRequestLogEvent   RequestLog    = new HTTPRequestLogEvent();

        /// <summary>
        /// An event called whenever a HTTP request could successfully be processed.
        /// </summary>
        public HTTPResponseLogEvent  ResponseLog   = new HTTPResponseLogEvent();

        /// <summary>
        /// An event called whenever a HTTP request resulted in an error.
        /// </summary>
        public HTTPErrorLogEvent     ErrorLog      = new HTTPErrorLogEvent();

        #endregion

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Attach the OIOI+ WebAPI to the given HTTP server.
        /// </summary>
        /// <param name="HTTPServer">A HTTP server.</param>
        /// <param name="URLPathPrefix">An optional prefix for the HTTP URIs.</param>
        /// <param name="HTTPRealm">The HTTP realm, if HTTP Basic Authentication is used.</param>
        /// <param name="HTTPLogins">An enumeration of logins for an optional HTTP Basic Authentication.</param>
        /// 
        /// <param name="XMLNamespaces">An optional delegate to process the XML namespaces.</param>
        /// <param name="EVSE2EVSEDataRecord">An optional delegate to process an EVSE data record before converting it to XML.</param>
        /// <param name="EVSEDataRecord2XML">An optional delegate to process an EVSE data record XML before sending it somewhere.</param>
        /// <param name="EVSEStatusRecord2XML">An optional delegate to process an EVSE status record XML before sending it somewhere.</param>
        /// <param name="XMLPostProcessing">An optional delegate to process the XML after its final creation.</param>
        public OIOIWebAPI(HTTPServer<RoamingNetworks, RoamingNetwork>  HTTPServer,
                          HTTPPath?                                    URLPathPrefix                       = null,
                          String                                       HTTPRealm                           = DefaultHTTPRealm,
                          IEnumerable<KeyValuePair<String, String>>    HTTPLogins                          = null,

                          XMLNamespacesDelegate                        XMLNamespaces                       = null,
                          //EVSE2EVSEDataRecordDelegate                  EVSE2EVSEDataRecord                 = null,
                          //EVSEStatusUpdate2EVSEStatusRecordDelegate    EVSEStatusUpdate2EVSEStatusRecord   = null,
                          //EVSEDataRecord2XMLDelegate                   EVSEDataRecord2XML                  = null,
                          //EVSEStatusRecord2XMLDelegate                 EVSEStatusRecord2XML                = null,
                          XMLPostProcessingDelegate                    XMLPostProcessing                   = null)
        {

            this.HTTPServer                         = HTTPServer    ?? throw new ArgumentNullException(nameof(HTTPServer), "The given HTTP server must not be null!");
            this.URLPathPrefix                      = URLPathPrefix ?? DefaultURLPathPrefix;
            this.HTTPRealm                          = HTTPRealm.IsNotNullOrEmpty() ? HTTPRealm : DefaultHTTPRealm;
            this.HTTPLogins                         = HTTPLogins ?? new KeyValuePair<String, String>[0];
            this.DNSClient                          = HTTPServer.DNSClient;

            this.XMLNamespaces                      = XMLNamespaces;
            //this.EVSE2EVSEDataRecord                = EVSE2EVSEDataRecord;
            //this.EVSEStatusUpdate2EVSEStatusRecord  = EVSEStatusUpdate2EVSEStatusRecord;
            //this.EVSEDataRecord2XML                 = EVSEDataRecord2XML;
            //this.EVSEStatusRecord2XML               = EVSEStatusRecord2XML;
            this.XMLPostProcessing                  = XMLPostProcessing;

            this._CPOAdapters                       = new List<WWCPCPOAdapter>();

            // Link HTTP events...
            HTTPServer.RequestLog   += (HTTPProcessor, ServerTimestamp, Request)                                 => RequestLog. WhenAll(HTTPProcessor, ServerTimestamp, Request);
            HTTPServer.ResponseLog  += (HTTPProcessor, ServerTimestamp, Request, Response)                       => ResponseLog.WhenAll(HTTPProcessor, ServerTimestamp, Request, Response);
            HTTPServer.ErrorLog     += (HTTPProcessor, ServerTimestamp, Request, Response, Error, LastException) => ErrorLog.   WhenAll(HTTPProcessor, ServerTimestamp, Request, Response, Error, LastException);

            var LogfilePrefix                       = "HTTPSSEs" + Path.DirectorySeparatorChar;

            this.DebugLog                           = HTTPServer.AddJSONEventSource(EventIdentification:      DebugLogId,
                                                                                    URLTemplate:              this.URLPathPrefix + "/DebugLog",
                                                                                    MaxNumberOfCachedEvents:  10000,
                                                                                    RetryIntervall:           TimeSpan.FromSeconds(5),
                                                                                    EnableLogging:            true,
                                                                                    LogfilePrefix:            LogfilePrefix);

            RegisterURITemplates();

        }

        #endregion


        #region (private) RegisterURITemplates()

        private void RegisterURITemplates()
        {

            #region / (HTTPRoot)

            HTTPServer.RegisterResourcesFolder(HTTPHostname.Any,
                                               URLPathPrefix + "/",
                                               "org.GraphDefined.WWCP.OIOIv2_2.WebAPI.HTTPRoot",
                                               DefaultFilename: "index.html");

            #endregion

        }

        #endregion



        public void Add(WWCPCPOAdapter CPOAdapter)
        {

            _CPOAdapters.Add(CPOAdapter);

            #region OnRFIDVerifyRequest/-Response

            CPOAdapter.CPOClient.OnRFIDVerifyRequest += async  (LogTimestamp,
                                                                RequestTimestamp,
                                                                Sender,
                                                                SenderId,
                                                                EventTrackingId,
                                                                RFIDId,
                                                                RequestTimeout) => await DebugLog.SubmitEvent("RFIDVerifyRequest",
                                                                                                              JSONObject.Create(
                                                                                                                  new JProperty("timestamp",        RequestTimestamp.ToIso8601()),
                                                                                                                  new JProperty("eventTrackingId",  EventTrackingId. ToString()),
                                                                                                                  new JProperty("RFIDId",           RFIDId.          ToString()),
                                                                                                                  new JProperty("requestTimeout",   Math.Round(RequestTimeout.TotalSeconds, 0))
                                                                                                             ));

            CPOAdapter.CPOClient.OnRFIDVerifyResponse += async (LogTimestamp,
                                                                RequestTimestamp,
                                                                Sender,
                                                                SenderId,
                                                                EventTrackingId,
                                                                RFIDId,
                                                                RequestTimeout,
                                                                Result,
                                                                Runtime) => await DebugLog.SubmitEvent("RFIDVerifyResponse",
                                                                                                       JSONObject.Create(
                                                                                                           new JProperty("timestamp",        RequestTimestamp.ToIso8601()),
                                                                                                           new JProperty("eventTrackingId",  EventTrackingId. ToString()),
                                                                                                           new JProperty("RFIDId",           RFIDId.          ToString()),
                                                                                                           new JProperty("requestTimeout",   Math.Round(RequestTimeout.TotalSeconds, 0)),
                                                                                                           new JProperty("result",           Result.          ToJSON()),
                                                                                                           new JProperty("runtime",          Math.Round(Runtime.TotalMilliseconds, 0))
                                                                                                       ));

            #endregion

        }

    }

}
