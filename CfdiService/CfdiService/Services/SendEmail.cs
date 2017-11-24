using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace CfdiService.Services
{
    public class SendEmail
    {
        public static void SendEmailMessage(string toAddress, string subject, string body)
        {
            SendUserEmail(toAddress, subject, body);
        }

        private static void SendUserEmail(string toAddress, string subject, string body)
        {
            // TODO: move all email settings to web.config or DB
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.To.Add(toAddress);
            message.To.Add("ted@ogrean.com");
            message.Subject = subject;
            message.From = new System.Net.Mail.MailAddress("tmogrean@gmail.com");
            message.Body = body;
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com");
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("tmogrean@gmail.com", "maryjo22");
            smtp.Send(message);
        }
    }
}