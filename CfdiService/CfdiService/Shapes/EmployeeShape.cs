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
        public string CreatedByUserName { get; set; }
        public int CreatedByUserId { get; set; }
        public string PasswordHash { get; set; }
        public string EmailAddress { get; set; }
        public string CellPhoneNumber { get; set; }
        public string CellPhoneCarrier { get; set; }
        public string LastLogin { get; set; }

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
                EmailAddress = employee.EmailAddress,
                PasswordHash = employee.PasswordHash,
                CreatedByUserName = employee.CreatedByUser.DisplayName,
                CellPhoneCarrier = employee.CellPhoneCarrier,
                CellPhoneNumber = employee.CellPhoneNumber,
                LastLogin = employee.LastLogin.ToShortDateString(),
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
            var now = DateTime.Now;
            if(DateTime.TryParse(employeeShape.LastLogin, out now))
            { employee.LastLogin = now; }
            employee.EmployeeId = employeeShape.EmployeeId;
            employee.CompanyId = employeeShape.CompanyId;
            employee.FirstName = employeeShape.FirstName;
            employee.LastName1 = employeeShape.LastName1;
            employee.LastName2 = employeeShape.LastName2;
            employee.CURP = employeeShape.CURP;
            employee.RFC = employeeShape.RFC;
            employee.EmailAddress = employeeShape.EmailAddress;
            employee.PasswordHash = employeeShape.PasswordHash;
            employee.CellPhoneCarrier = employeeShape.CellPhoneCarrier;
            employee.CellPhoneNumber = employeeShape.CellPhoneNumber;
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