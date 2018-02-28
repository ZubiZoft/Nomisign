using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace CfdiService.Authentication
{
    public class EmployeeIdentity : IIdentity
    {
        public string Name { get; set; }

        public string AuthenticationType { get; }
        public bool IsAuthenticated { get; }

        public CfdiService.Shapes.EmployeeShape employeeShape = new CfdiService.Shapes.EmployeeShape();

        public EmployeeIdentity(CfdiService.Shapes.EmployeeShape employeeShape)
        {
            this.employeeShape = employeeShape;
            IsAuthenticated = true;
            Name = this.employeeShape.EmployeeId.ToString();
        }
    }
}