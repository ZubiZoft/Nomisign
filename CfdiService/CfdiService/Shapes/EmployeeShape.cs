using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class EmployeeShape
    {
        public int EmployeeId { set; get; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string FirstName { get; set; }
        public string LastName1 { get; set; }
        public string LastName2 { get; set; }
        public string CURP { get; set; }
        public string RFC { get; set; }
        public int CreatedByUserId { get; set; }

        public static EmployeeShape FromDataModel(Employee employee, HttpRequestMessage request)
        {
            var employeeShape = new EmployeeShape
            {
                EmployeeId = employee.EmployeeId,
                UserId = employee.UserId,
                CompanyId = employee.CompanyId,
                FirstName = employee.FirstName,
                LastName1 = employee.LastName1,
                LastName2 = employee.LastName2,
                CURP = employee.CURP,
                RFC = employee.RFC,
                CreatedByUserId = employee.CreatedByUserId,
                Links = new LinksClass()
            };

            employeeShape.Links.SelfUri = request.GetLinkUri($"employees/{employeeShape.EmployeeId}");
            return employeeShape;
        }

        public static Employee ToDataModel(EmployeeShape employeeShape, Employee employee = null)
        {
            if (employee == null)
                employee = new Employee();

            employee.UserId = employeeShape.UserId;
            employee.EmployeeId = employeeShape.EmployeeId;
            employee.CompanyId = employeeShape.CompanyId;
            employee.FirstName = employeeShape.FirstName;
            employee.LastName1 = employeeShape.LastName1;
            employee.LastName2 = employeeShape.LastName2;
            employee.CURP = employeeShape.CURP;
            employee.RFC = employeeShape.RFC;
            employee.CreatedByUserId = employeeShape.CreatedByUserId;

            return employee;
        }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }
        public LinksClass Links { get; set; }
    }
}