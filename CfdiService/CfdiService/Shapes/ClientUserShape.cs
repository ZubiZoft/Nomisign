using CfdiService.Model;
using CfdiService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class ClientUserShape
    {
        public int ClientUserID { get; set; }
        public int ClientCompanyID { set; get; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string UserStatus { get; set; }
        public string LastLogin { get; set; }
        public bool ForcePasswordReset { get; set; }
        public int CreatedByUserId { get; set; }
        public string ClientUserType { get; set; }
        public string DateUserCreated { get; set; }

        public static ClientUserShape FromDataModel(ClientUser clientUser, HttpRequestMessage request)
        {
            var clientUserShape = new ClientUserShape
            {
                ClientUserID = clientUser.ClientUserID,
                ClientCompanyID = clientUser.ClientCompanyID,
                EmailAddress = clientUser.EmailAddress,
                DisplayName = clientUser.DisplayName,
                PhoneNumber = clientUser.PhoneNumber,
                PasswordHash = string.Empty, // user.PasswordHash, this should always return empty, or not at all.  for login and password reset dont use the shape 
                UserStatus = clientUser.UserStatus.ToString(),
                ClientUserType = clientUser.ClientUserType.ToString(),
                LastLogin = clientUser.LastLogin.ToShortDateString(),
                ForcePasswordReset = clientUser.ForcePasswordReset,
                CreatedByUserId = clientUser.CreatedByUserId,
                DateUserCreated = clientUser.DateUserCreated.ToString()
            };
            return clientUserShape;
        }

        public static ClientUser ToDataModel(ClientUserShape clientUserShape, ClientUser clientUser = null)
        {
            if (clientUser == null)
                clientUser = new ClientUser();

            clientUser.ClientUserID = clientUserShape.CreatedByUserId;
            clientUser.ClientCompanyID = clientUserShape.ClientCompanyID;
            clientUser.CreatedByUserId = clientUserShape.CreatedByUserId;
            clientUser.EmailAddress = clientUserShape.EmailAddress;
            clientUser.DisplayName = clientUserShape.DisplayName;
            clientUser.PhoneNumber = clientUserShape.PhoneNumber;
            clientUser.ForcePasswordReset = clientUserShape.ForcePasswordReset;

            if (!String.IsNullOrEmpty(clientUserShape.PasswordHash))
            {
                clientUser.PasswordHash = EncryptionService.Sha256_hash(clientUserShape.PasswordHash, string.Empty);
            }

            // user status enum
            UserStatusType status = UserStatusType.Active;
            Enum.TryParse<UserStatusType>(clientUserShape.UserStatus, out status);
            clientUser.UserStatus = status;

            // user type enum
            ClientUserAdminType type = ClientUserAdminType.Invalid;
            Enum.TryParse<ClientUserAdminType>(clientUserShape.ClientUserType, out type);
            clientUser.ClientUserType = type;

            return clientUser;
        }
    }
}