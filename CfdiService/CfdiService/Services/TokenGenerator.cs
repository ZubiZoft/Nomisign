using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService.Services
{
    public static class TokenGenerator
    {
        public static string GetToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}