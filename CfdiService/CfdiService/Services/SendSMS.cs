using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace CfdiService.Services
{
    public class SendSMS
    {
        private static readonly string _fromPhoneNumber = System.Configuration.ConfigurationManager.AppSettings["fromPhoneNumber"];
        private static readonly string _accountSID = System.Configuration.ConfigurationManager.AppSettings["accountSID"];
        private static readonly string _accountAuthToken = System.Configuration.ConfigurationManager.AppSettings["accountAuthToken"];

        public static void SendSMSMsg(string cellPhoneNumber, string smsBody)
        {
            //const string accountSid = _accountSID; // "AC7d4cbd0f3db4469d6987ea846fb5b234";
            //const string authToken = "a8407e9400297d0197feb63fc40380d0";
            TwilioClient.Init(_accountSID, _accountAuthToken);


            var to = new PhoneNumber("+" + cellPhoneNumber);
            var message = MessageResource.Create(to,
                                                 from: new PhoneNumber("+" + _fromPhoneNumber),
                                                 body: smsBody);

            Console.WriteLine(message.Sid);
        }
    }
}