using CfdiService.Model;
using CfdiService.Services;
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
        //public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string FirstName { get; set; }
        public string LastName1 { get; set; }
        public string LastName2 { get; set; }
        public string FullName { get; set; }
        public string CURP { get; set; }
        public string RFC { get; set; }
        public string CreatedByUserName { get; set; }
        public int CreatedByUserId { get; set; }
        public string PasswordHash { get; set; }
        public string EmailAddress { get; set; }
        public string CellPhoneNumber { get; set; }
        //public string CellPhoneCarrier { get; set; }
        public string LastLogin { get; set; }
        public EmployeeStatusType EmployeeStatus { get; set; }
        public string SecurityCode { get; set; }
        public bool HasContractToSign { get; set; }
        public string SessionToken { get; set; }
        public static EmployeeShape FromDataModel(Employee employee, HttpRequestMessage request)
        {
            var employeeShape = new EmployeeShape
            {
                EmployeeId = employee.EmployeeId,
                //UserId = employee.UserId,
                CompanyId = employee.CompanyId,
                FirstName = employee.FirstName,
                LastName1 = employee.LastName1,
                LastName2 = employee.LastName2,
                FullName = !string.IsNullOrEmpty(employee.FullName) ? employee.FullName : (String.Format("{0} {1} {2}", employee.FirstName, employee.LastName1, employee.LastName2)),
                CURP = employee.CURP,
                RFC = employee.RFC,
                EmailAddress = employee.EmailAddress,
                PasswordHash = String.Empty, // employee.PasswordHash.  no need to ever let this out
                CellPhoneNumber = employee.CellPhoneNumber,
                LastLogin = employee.LastLoginDate < employee.CreatedDate ? 
                        employee.CreatedDate.ToShortDateString() : employee.LastLoginDate.ToShortDateString(),
                CreatedByUserId = employee.CreatedByUserId,
                EmployeeStatus = employee.EmployeeStatus,
                SessionToken = employee.SessionToken,
                HasContractToSign = false,
                Links = new LinksClass()
            };

            // if this is not a DB op, created by user is null, so check
            if (null != employee.CreatedByUser)
            {
                employeeShape.CreatedByUserName = employee.CreatedByUser.DisplayName;
            }
            employeeShape.Links.SelfUri = request.GetLinkUri($"employees/{employeeShape.EmployeeId}");
            return employeeShape;
        }

        public static EmployeeShape FromDataModel(Employee employee)
        {
            var employeeShape = new EmployeeShape
            {
                EmployeeId = employee.EmployeeId,
                //UserId = employee.UserId,
                CompanyId = employee.CompanyId,
                FirstName = employee.FirstName,
                LastName1 = employee.LastName1,
                LastName2 = employee.LastName2,
                FullName = !string.IsNullOrEmpty(employee.FullName) ? employee.FullName : (String.Format("{0} {1} {2}", employee.FirstName, employee.LastName1, employee.LastName2)),
                CURP = employee.CURP,
                RFC = employee.RFC,
                EmailAddress = employee.EmailAddress,
                PasswordHash = String.Empty, // employee.PasswordHash.  no need to ever let this out
                CellPhoneNumber = employee.CellPhoneNumber,
                LastLogin = employee.LastLoginDate.ToShortDateString(),
                CreatedByUserId = employee.CreatedByUserId,
                EmployeeStatus = employee.EmployeeStatus,
                HasContractToSign = false,
                Links = new LinksClass()
            };

            // if this is not a DB op, created by user is null, so check
            if (null != employee.CreatedByUser)
            {
                employeeShape.CreatedByUserName = employee.CreatedByUser.DisplayName;
            }
            return employeeShape;
        }

        public static Employee ToDataModel(EmployeeShape employeeShape, Employee employee = null)
        {
            if (employee == null)
                employee = new Employee();

            var now = DateTime.Now;
            if(DateTime.TryParse(employeeShape.LastLogin, out now))
            { employee.LastLoginDate = now; }
            //employee.EmployeeId = employeeShape.EmployeeId;
            //employee.CompanyId = employeeShape.CompanyId;
            employee.FirstName = employeeShape.FirstName;
            employee.LastName1 = employeeShape.LastName1;
            employee.LastName2 = employeeShape.LastName2;
            employee.FullName = string.IsNullOrEmpty(employeeShape.FullName) ? 
                    string.Format("{0} {1} {2}", employeeShape.FirstName, employeeShape.LastName1, employeeShape.LastName2) : 
                    employeeShape.FullName;
            employee.CURP = employeeShape.CURP;
            employee.RFC = employeeShape.RFC;
            employee.EmailAddress = employeeShape.EmailAddress;
            employee.CellPhoneNumber = employeeShape.CellPhoneNumber;
            employee.CreatedByUserId = employeeShape.CreatedByUserId;
            employee.EmployeeStatus = employeeShape.EmployeeStatus;
            if (employee.EmployeeStatus == EmployeeStatusType.PasswordFailureLocked)
            {
                employee.FailedLoginCount = 0;
                employee.SessionToken = "";
            }
            return employee;
        }

        public class LinksClass
        {
            public string SelfUri { get; set; }
        }
        public LinksClass Links { get; set; }
        
    }
}