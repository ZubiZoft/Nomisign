using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace CfdiService.Shapes
{
    public class ClientShape
    {
        public int ClientCompanyID { get; set; }
        public string ClientCompanyName { get; set; }
        public string ClientCompanyRFC { get; set; }

        public static ClientShape FromDataModel(Client client, HttpRequestMessage request)
        {
            var clientShape = new ClientShape
            {
                ClientCompanyID = client.ClientCompanyID,
                ClientCompanyName = client.ClientCompanyName,
                ClientCompanyRFC = client.ClientCompanyRFC,
            };
            return clientShape;
        }

        public static Client ToDataModel(ClientShape clientShape, Client client = null)
        {
            if (client == null)
                client = new Client();

            client.ClientCompanyID = clientShape.ClientCompanyID;
            client.ClientCompanyName = clientShape.ClientCompanyName;
            client.ClientCompanyRFC = clientShape.ClientCompanyRFC;

            return client;
        }
    }
}