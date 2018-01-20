using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace CfdiService.Services
{
    public class SendEmail
    {
        private static readonly bool _alwaysAddTed = Boolean.Parse(System.Configuration.ConfigurationManager.AppSettings["alwaysAddTed"]);
        private static readonly int _emailSmtpPort = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["emailSmtpPortNumber"]);
        private static readonly string _emailAccount = System.Configuration.ConfigurationManager.AppSettings["emailAccount"];
        private static readonly string _emailAccountPassword = System.Configuration.ConfigurationManager.AppSettings["emailAccountPassword"];
        private static readonly string _emailFromAddress = System.Configuration.ConfigurationManager.AppSettings["emailFromAddress"];
        private static readonly string _emailSmtpAddress = System.Configuration.ConfigurationManager.AppSettings["emailSmtpAddress"];

        public static void SendEmailMessage(string toAddress, string subject, string body)
        {
            SendUserEmail(toAddress, subject, body);
        }

        private static void SendUserEmail(string toAddress, string subject, string body)
        {
            // TODO: move all email settings to web.config or DB
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.To.Add(toAddress);
            // for testing only!!!
            if (_alwaysAddTed)
            {
                message.To.Add("ted@ogrean.com");
            }
            message.Subject = subject;
            message.From = new System.Net.Mail.MailAddress(_emailFromAddress);
            message.Body = body;
            using (System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(_emailSmtpAddress, _emailSmtpPort))
            {
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(_emailAccount, _emailAccountPassword);
                smtp.Send(message);
            }
        }
    }
}