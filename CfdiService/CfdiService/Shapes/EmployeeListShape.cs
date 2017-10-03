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
        public string EmailAddress { get; set; }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }

        public LinksClass Links { get; set; }

        public static EmployeeListShape FromDataModel(Employee employee, HttpRequestMessage request)
        {
            var employeeUserShape = new EmployeeListShape
            {
                EmailAddress = employee.EmailAddress,
                EmployeeId = employee.EmployeeId,
                FirstName = employee.FirstName,
                LastName1 = employee.LastName1,
                LastName2 = employee.LastName2,
                Links = new LinksClass()
            };

            employeeUserShape.Links.SelfUri = request.GetLinkUri($"employees/{employeeUserShape.EmployeeId}");
            return employeeUserShape;
        }
    }
}