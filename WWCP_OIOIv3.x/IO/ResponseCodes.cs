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

namespace org.GraphDefined.WWCP.OIOIv3_x
{

    /// <summary>
    /// OIOI response codes.
    /// </summary>
    public enum ResponseCodes
    {

        // You must retry later, honoring any "Retry-After"-header included in the response.
        // Your services must implement exponential back-off.

        // //0xx     Success
        // 000     Succes
        // 011     Successfully started a charging session. The customer is charging at the EVSE.
        // 012     Successfully authorized a charging session. The customer must now plug in the cable to start.

        // //1xx     PlugSurfing Errors
        // 100     System error
        // 101     Database error
        // 102     System timeout
        // 140     Authentication failed: No positive authentication response
        // 141     Authentication failed: Invalid email or password
        // 142     Authentication failed: Invalid email
        // 143     Authentication failed: Email already exists
        // 144     Authentication failed: Email does not exist
        // 145     Authentication failed: User token not valid
        // 180     Entity not found
        // 181     EVSE not found
        // 182     Session not found
        // 183     Company not found
        // 184     Vehicle not found
        // 185     Subscription plan not found
        // 186     Group not found
        // 190     EVCO ID error
        // 191     EVCO ID not found
        // 192     EVCO ID locked
        // 193     EVCO ID has no valid payment method

        // //2xx     Client Error
        // 200     Client request error
        // 210     Invalid API key
        // 211     Invalid partner identifier
        // 220     API key not allowed to access the requested resource
        // 230     Invalid request format

        // //3xx     Operator and EVSE Errors
        // 300     System error
        // 302     System timeout
        // 310     EVSE error
        // 312     EVSE timeout
        // 320     EVSE already in use
        // 321     No EV connected to EVSE

        // //4xx     Hub Errors
        // 400     System error
        // 402     System timeout

        // //8xx     Payment Provider Errors
        // 800     System error
        // 802     System timeout
        // 830     Invalid format
        // 860     Bank transfer error
        // 861     Bank account not valid
        // 862     Invalid name
        // 863     Invalid IBAN
        // 864     Invalid BIC
        // 870     Credit card error
        // 871     Credit card not valid
        // 872     Invalid card holder name
        // 874     Invalid credit card number
        // 875     Invalid expiration date
        // 876     Invalid CVC
        // 880     PayPal error

    }

}
