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
        public static void SendSMSMsg(string cellPhoneNumber, string smsBody)
        {
            const string accountSid = "AC7d4cbd0f3db4469d6987ea846fb5b234";
            const string authToken = "a8407e9400297d0197feb63fc40380d0";
            TwilioClient.Init(accountSid, authToken);


            var to = new PhoneNumber("+" + cellPhoneNumber);
            var message = MessageResource.Create(to,
                                                 from: new PhoneNumber("+12162085984"),
                                                 body: smsBody);

            Console.WriteLine(message.Sid);
        }
    }
}