using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService
{
    public static class Strings
    {
        /// <summary>
        /// Previous english strings and teh new one in spanish so i know
        /// </summary>
        // TODO: move these settings to a cache class so it does not get pulled from web.config every time
        //string msgBody = String.Format("Dear {0} {1},\r\n\r\nWecome to the nomisign application.  Please visit the site at http://{2}/nomisign/account/{3} to complete your user setup.\r\n\r\nPlease use the email address of {4} to set up your password!",
        //    employee.FirstName,
        //    employee.LastName1,
        //    httpDomain,
        //    employee.EmployeeId, 
        //    employee.EmailAddress);

        //string smsBody = String.Format("Your user has been created for the Nomisign application.  Please visit the site at http://{0}/nomisign/account/{1} or check email for login deatils",
        //    httpDomain, employee.EmployeeId );

        // Spanish SMS and email message
        public static readonly string newEmployeeWelcomeMessge = "Bienvenidos de Nomisign, a la aplicación para revisar y firmar sus nominas. Visite este link http://{0}/nomisign/account/{1} para crear su contraseña";

        public static readonly string newEmployeeWelcomeMessgeEmailSubject = "Bienvenidos de Nomisign";

        public static readonly string verifyPhoneNumberSMSMessage = "Este mensaje es para comprobar el numero del celular. Por favor compruebe que su CRUP está correcto";

        // Password
        // public static readonly string password = "Contraseña";
        // public static readonly string verifyPassword = "Verificación de Contraseña";

        // "Please visit nomisign site for review of new docs
        public static readonly string visitSiteTosignDocumentMessage = "Visita a Nomisign para revisar y firmar su documento";

        // ""review your new docs""
        public static readonly string visitSiteTosignDocumentMessageEmailSubject = "revisar y firmar su documento";

        public static readonly string verifyCurpNumberSMSText = "Este mensaje es para comprobar el numero del celular. Por favor compruebe que su CRUP está correcto";
    }
}