using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CfdiService.Model
{
    public enum UserTypes { User = 1, Employee = 2, Client = 3, Uploader = 4, System = 5 }
    public enum OperationTypes { UserCreated = 1, EmployeeCreated = 2, ClientUserCreated = 3, DocumentStored = 4, SignedBy = 5,
            UserUpdated = 6, EmployeeUpdated = 7, ClientUserUpdated = 8, DocumentUpdated = 9, UserLogged = 10, EmployeeLogged = 11,
            DocumentRejected = 12 }
    public enum ObjectTypes { None = 0, Batch = 1, Client = 2, ClientUser = 3, Company = 4, Document = 5, Employee = 6, EmployeeCode = 7,
        EmployeeSecurityQuestion = 8, SignaturePurchase = 9, User = 10 }

    public class Logs
    {
        [Key]
        public int LogId { get; set; }
        public OperationTypes Type { get; set; }
        public string Comments { get; set; }
        public DateTime Timestamp { get; set; }
        public int ExecutedBy { get; set; }
        public UserTypes UserType { get; set; }
        public int ObjectId { get; set; }
        public ObjectTypes ObjectType { get; set; }

        public static UserTypes ConvertRoleToUserType(string role)
        {
            switch (role)
            {
                case "ADMIN":
                    return UserTypes.User;
                case "EMPLOYEE":
                    return UserTypes.Employee;
                case "CLIENT":
                    return UserTypes.Client;
                case "UPLOADER":
                    return UserTypes.Uploader;
            }
            return UserTypes.Uploader;
        }
    }
}