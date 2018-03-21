using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;

namespace CfdiService.Shapes
{
    public static class RequestExtensions
    {
        private class LinkHelper
        {
            public HttpRequestMessage Request { get; set; }
            public string Template { get; set; }
            public object AddInfo { get; set; }

            private bool TemplateHasParameter(string template, string p)
            {
                return template.Contains(string.Format("{0}", p));
            }

            private string ExpandWithRouteData(string start, IDictionary<string, object> routedata)
            {
                string t = start;
                foreach (string key in routedata.Keys)
                {
                    if (TemplateHasParameter(t, key))
                    {
                        t = t.Replace("{" + key + "}", routedata[key] as string);
                    }
                }
                return t;
            }

            private string ExpandWithObject(string start, object addinfo)
            {
                string t = start;
                if (addinfo != null)
                {
                    PropertyInfo[] properties = AddInfo.GetType().GetProperties();
                    foreach (PropertyInfo prop in properties)
                    {
                        if (TemplateHasParameter(t, prop.Name))
                        {
                            t = t.Replace("{" + prop.Name + "}", prop.GetValue(AddInfo).ToString());
                        }
                    }
                }
                return t;
            }

            private string ExpandTemplate(string template, IDictionary<string, object> routedata, object addinfo)
            {
                string expansion = ExpandWithObject(template, addinfo);
                expansion = ExpandWithRouteData(expansion, routedata);
                return expansion;
            }

            public string Link()
            {
                string template = ExpandTemplate(Template, Request.GetRouteData().Values, AddInfo);
                var apiRoot = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority + "/api";
                var separator = template.StartsWith("/")? "" : "/";
                return apiRoot + separator + template;
            }
        }

        public static string GetLinkUri(this HttpRequestMessage request, string template)
        {
            return new LinkHelper()
            {
                Request = request,
                Template = template
            }.Link();
        }

        public static string GetLinkUri(this HttpRequestMessage request, string template, object addinfo)
        {
            return new LinkHelper()
            {
                Request = request,
                Template = template,
                AddInfo = addinfo
            }.Link();
        }
    }

    public static class IdentityExtensions
    {
        public static string GetName(this IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            Claim claim = claimsIdentity?.FindFirst(ClaimTypes.Name);

            return claim?.Value ?? string.Empty;
        }

        public static string GetRole(this IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            Claim claim = claimsIdentity?.FindFirst(ClaimTypes.Role);

            return claim?.Value ?? string.Empty;
        }
    }
}