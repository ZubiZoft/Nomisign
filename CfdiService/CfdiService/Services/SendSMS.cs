using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
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
        private static readonly string _quiubasAuthToken = System.Configuration.ConfigurationManager.AppSettings["quiubasAuthToken"];
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

       
        public static bool SendSMSQuiubo(string msg, string number, out string res)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values.Add("to_number", number);
                values.Add("message", msg);
                log.Info("Number to send : " + number);
                log.Info("message to send : " + msg);
                
                //client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("Authorization", "Basic " + _quiubasAuthToken);
                var respose = client.UploadValues("https://api.quiubas.com/sms", values);
                string json = Encoding.Default.GetString(respose);
                log.Info("json : " + json);
                try
                {
                    Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (dic.ContainsKey("Error"))
                    {
                        res = dic["error"];
                        return false;
                    }
                    res = dic["id"];
                }
                catch
                {
                    res = "Error";
                    return false;
                }
            }
            return true;
        }

    }
}