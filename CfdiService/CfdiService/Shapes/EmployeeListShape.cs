using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class EmployeeListShape
    {
        public int EmployeeId { set; get; }
        public string FirstName { get; set; }
        public string LastName1 { get; set; }
        public string LastName2 { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string CellPhoneNumber { get; set; }
        public string CellPhoneCarrier { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string CompanyName { get; set; }
        public string RFC { get; set; }
        public string CURP { get; set; }
        public class LinksClass
        {
            public string SelfUri { get; set; }
        }

        public LinksClass Links { get; set; }

        public static EmployeeListShape FromDataModel(Employee employee, HttpRequestMessage request, string companyName)
        {
            var employeeUserShape = new EmployeeListShape
            {
                EmailAddress = employee.EmailAddress,
                EmployeeId = employee.EmployeeId,
                FirstName = employee.FirstName,
                LastName1 = employee.LastName1,
                LastName2 = employee.LastName2,
                FullName = employee.FullName,
                CellPhoneNumber = employee.CellPhoneNumber,
                LastLoginDate = employee.LastLoginDate,
                CURP = employee.CURP,
                RFC = employee.RFC,
                CompanyName = companyName,
                Links = new LinksClass()
            };

            employeeUserShape.Links.SelfUri = request.GetLinkUri($"employees/{employeeUserShape.EmployeeId}");
            return employeeUserShape;
        }
    }
}