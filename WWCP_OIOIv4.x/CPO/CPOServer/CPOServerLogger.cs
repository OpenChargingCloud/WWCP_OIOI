﻿/*
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

using org.GraphDefined.Vanaheimr.Illias;
using org.GraphDefined.Vanaheimr.Hermod.HTTP;
using org.GraphDefined.Vanaheimr.Hermod.Logging;

#endregion

namespace cloud.charging.open.protocols.OIOIv4_x.CPO
{

    /// <summary>
    /// The CPO server logger.
    /// </summary>
    public class CPOServerLogger : HTTPServerLogger
    {

        #region Data

        /// <summary>
        /// The default context for this logger.
        /// </summary>
        public const String DefaultContext = "OIOI_CPOServer";

        #endregion

        #region Properties

        /// <summary>
        /// The linked OIOI CPO server.
        /// </summary>
        public CPOServer CPOServer { get; }

        #endregion

        #region Constructor(s)

        #region CPOServerLogger(CPOServer, Context = DefaultContext, LogfileCreator = null)

        /// <summary>
        /// Create a new CPO server logger using the default logging delegates.
        /// </summary>
        /// <param name="CPOServer">A OIOI CPO server.</param>
        /// <param name="LoggingPath">The logging path.</param>
        /// <param name="Context">A context of this API.</param>
        /// <param name="LogfileCreator">A delegate to create a log file from the given context and log file name.</param>
        public CPOServerLogger(CPOServer                CPOServer,
                               String                   LoggingPath,
                               String                   Context         = DefaultContext,
                               LogfileCreatorDelegate?  LogfileCreator  = null)

            : this(CPOServer,
                   LoggingPath,
                   Context.IsNotNullOrEmpty() ? Context : DefaultContext,
                   null,
                   null,
                   null,
                   null,

                   LogfileCreator: LogfileCreator)

        { }

        #endregion

        #region CPOServerLogger(CPOServer, Context, ... Logging delegates ...)

        /// <summary>
        /// Create a new CPO server logger using the given logging delegates.
        /// </summary>
        /// <param name="CPOServer">A OIOI CPO server.</param>
        /// <param name="LoggingPath">The logging path.</param>
        /// <param name="Context">A context of this API.</param>
        /// 
        /// <param name="LogHTTPRequest_toConsole">A delegate to log incoming HTTP requests to console.</param>
        /// <param name="LogHTTPResponse_toConsole">A delegate to log HTTP requests/responses to console.</param>
        /// <param name="LogHTTPRequest_toDisc">A delegate to log incoming HTTP requests to disc.</param>
        /// <param name="LogHTTPResponse_toDisc">A delegate to log HTTP requests/responses to disc.</param>
        /// 
        /// <param name="LogHTTPRequest_toNetwork">A delegate to log incoming HTTP requests to a network target.</param>
        /// <param name="LogHTTPResponse_toNetwork">A delegate to log HTTP requests/responses to a network target.</param>
        /// <param name="LogHTTPRequest_toHTTPSSE">A delegate to log incoming HTTP requests to a HTTP server sent events source.</param>
        /// <param name="LogHTTPResponse_toHTTPSSE">A delegate to log HTTP requests/responses to a HTTP server sent events source.</param>
        /// 
        /// <param name="LogHTTPError_toConsole">A delegate to log HTTP errors to console.</param>
        /// <param name="LogHTTPError_toDisc">A delegate to log HTTP errors to disc.</param>
        /// <param name="LogHTTPError_toNetwork">A delegate to log HTTP errors to a network target.</param>
        /// <param name="LogHTTPError_toHTTPSSE">A delegate to log HTTP errors to a HTTP server sent events source.</param>
        /// 
        /// <param name="LogfileCreator">A delegate to create a log file from the given context and log file name.</param>
        public CPOServerLogger(CPOServer                    CPOServer,
                               String                       LoggingPath,
                               String                       Context,

                               HTTPRequestLoggerDelegate?   LogHTTPRequest_toConsole    = null,
                               HTTPResponseLoggerDelegate?  LogHTTPResponse_toConsole   = null,
                               HTTPRequestLoggerDelegate?   LogHTTPRequest_toDisc       = null,
                               HTTPResponseLoggerDelegate?  LogHTTPResponse_toDisc      = null,

                               HTTPRequestLoggerDelegate?   LogHTTPRequest_toNetwork    = null,
                               HTTPResponseLoggerDelegate?  LogHTTPResponse_toNetwork   = null,
                               HTTPRequestLoggerDelegate?   LogHTTPRequest_toHTTPSSE    = null,
                               HTTPResponseLoggerDelegate?  LogHTTPResponse_toHTTPSSE   = null,

                               HTTPResponseLoggerDelegate?  LogHTTPError_toConsole      = null,
                               HTTPResponseLoggerDelegate?  LogHTTPError_toDisc         = null,
                               HTTPResponseLoggerDelegate?  LogHTTPError_toNetwork      = null,
                               HTTPResponseLoggerDelegate?  LogHTTPError_toHTTPSSE      = null,

                               LogfileCreatorDelegate?      LogfileCreator              = null)

            : base(CPOServer.HTTPServer,
                   LoggingPath,
                   Context.IsNotNullOrEmpty() ? Context : DefaultContext,

                   LogHTTPRequest_toConsole,
                   LogHTTPResponse_toConsole,
                   LogHTTPRequest_toDisc,
                   LogHTTPResponse_toDisc,

                   LogHTTPRequest_toNetwork,
                   LogHTTPResponse_toNetwork,
                   LogHTTPRequest_toHTTPSSE,
                   LogHTTPResponse_toHTTPSSE,

                   LogHTTPError_toConsole,
                   LogHTTPError_toDisc,
                   LogHTTPError_toNetwork,
                   LogHTTPError_toHTTPSSE,

                   LogfileCreator)

        {

            #region Initial checks

            if (CPOServer == null)
                throw new ArgumentNullException(nameof(CPOServer), "The given CPO server must not be null!");

            this.CPOServer = CPOServer;

            #endregion

            #region Register remote start/stop log events

            RegisterEvent2("AnyRequest",
                           handler => CPOServer.OnAnyHTTPRequest += handler,
                           handler => CPOServer.OnAnyHTTPRequest -= handler,
                           "Requests", "All").
                RegisterDefaultConsoleLogTarget(this).
                RegisterDefaultDiscLogTarget(this);

            RegisterEvent2("AnyResponse",
                           handler => CPOServer.OnAnyHTTPResponse += handler,
                           handler => CPOServer.OnAnyHTTPResponse -= handler,
                           "Responses", "All").
                RegisterDefaultConsoleLogTarget(this).
                RegisterDefaultDiscLogTarget(this);


            RegisterEvent("SessionStartRequest",
                          handler => CPOServer.OnSessionStartHTTPRequest += handler,
                          handler => CPOServer.OnSessionStartHTTPRequest -= handler,
                          "SessionStart", "Requests", "All").
                RegisterDefaultConsoleLogTarget(this).
                RegisterDefaultDiscLogTarget(this);

            RegisterEvent("SessionStartResponse",
                          handler => CPOServer.OnSessionStartHTTPResponse += handler,
                          handler => CPOServer.OnSessionStartHTTPResponse -= handler,
                          "SessionStart", "Responses", "All").
                RegisterDefaultConsoleLogTarget(this).
                RegisterDefaultDiscLogTarget(this);


            RegisterEvent("SessionStopRequest",
                          handler => CPOServer.OnSessionStopHTTPRequest += handler,
                          handler => CPOServer.OnSessionStopHTTPRequest -= handler,
                          "SessionStop", "Requests", "All").
                RegisterDefaultConsoleLogTarget(this).
                RegisterDefaultDiscLogTarget(this);

            RegisterEvent("SessionStopResponse",
                          handler => CPOServer.OnSessionStopHTTPResponse += handler,
                          handler => CPOServer.OnSessionStopHTTPResponse -= handler,
                          "SessionStop", "Responses", "All").
                RegisterDefaultConsoleLogTarget(this).
                RegisterDefaultDiscLogTarget(this);

            #endregion

        }

        #endregion

        #endregion

    }

}
