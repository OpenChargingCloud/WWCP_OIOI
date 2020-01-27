/*
 * Copyright (c) 2016-2020 GraphDefined GmbH
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

namespace org.GraphDefined.WWCP.OIOIv4_x
{

    /// <summary>
    /// An OIOI Response Codes.
    /// </summary>
    public enum ResponseCodes
    {

        // 0xx - Success
        Success                                                 = 000,
        SuccessfullyStartedAChargingSession                     = 011,
        // The customer is charging at the EVSE

        SuccessfullyAuthorizedAChargingSession                  = 012,
        // The customer must now plug in the cable to start

        // 1xx - PlugSurfing Errors
        SystemError                                             = 100,
        DatabaseError                                           = 101,
        SystemTimeout                                           = 102,
        AuthenticationFailed_NoPositiveAuthenticationResponse   = 140,
        AuthenticationFailed_InvalidEmailOrPassword             = 141,
        AuthenticationFailed_InvalidEmail                       = 142,
        AuthenticationFailed_EmailAlreadyExists                 = 143,
        AuthenticationFailed_EmailDoesNotExist                  = 144,
        AuthenticationFailed_UserTokenNotValid                  = 145,
        EntityNotFound                                          = 180,
        EVSENotFound                                            = 181,
        SessionNotFound                                         = 182,
        CompanyNotFound                                         = 183,
        VehicleNotFound                                         = 184,
        SubscriptionPlanNotFound                                = 185,
        GroupNotFound                                           = 186,
        EVSEID_DoesNotSupportDirectPay                          = 187,
        EVSEID_DoesNotSupportRemoteStop                         = 188,
        EVCOID_Error                                            = 190,
        EVCOID_NotFound                                         = 191,
        EVCOID_Locked                                           = 192,
        EVCOID_HasNoValidPaymentMethod                          = 193,

        // 2xx - Client Errors
        ClientRequestError                                      = 200,
        InvalidAPIKey                                           = 210,
        InvalidPartnerIdentifier                                = 211,
        APIKeyNotAllowedToAccessTheRequestedResource            = 220,
        InvalidRequestFormat                                    = 230,
        InvalidHTTPResponse                                     = 1240,
        InvalidResponseFormat                                   = 1241,

        // 3xx - Operator and EVSE Errors
        System_Error                                            = 300,
        System_Timeout                                          = 302,
        EVSE_Error                                              = 310,
        EVSE_Timeout                                            = 312,
        EVSE_AlreadyInUse                                       = 320,
        EVSE_NoEVConnected                                      = 321,

        // 4xx - Hub Errors
        Hub_SystemError                                         = 400,
        Hub_SystemTimeout                                       = 402,

        // 8xx - Payment Provider Errors
        Payment_SystemError                                     = 800,
        Payment_SystemTimeout                                   = 802,
        Payment_ThisUserIsNotAllowedToUseThisMethod             = 805,
        Payment_InvalidFormat                                   = 830,
        Payment_InvalidPaymentMethod                            = 850,
        Payment_BankTransferError                               = 860,
        Payment_BankAccountNotValid                             = 861,
        Payment_InvalidName                                     = 862,
        Payment_InvalidIBAN                                     = 863,
        Payment_InvalidBIC                                      = 864,
        Payment_CreditCardError                                 = 870,
        Payment_CreditCardNotValid                              = 871,
        Payment_InvalidCardHolderName                           = 872,
        Payment_InvalidCreditCardNumber                         = 874,
        Payment_InvalidExpirationDate                           = 875,
        Payment_InvalidCVC                                      = 876,
        Payment_PayPalError                                     = 880

    }

}
