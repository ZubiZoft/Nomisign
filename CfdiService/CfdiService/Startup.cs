using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace CfdiService
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();
            // WebApiConfig.Register(config);

            config.MapHttpAttributeRoutes();

            // Use JSON as our data format instead of XML
            var formatter = new JsonMediaTypeFormatter();
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            settings.Converters.Add(new DateTimeConverter());
            formatter.SerializerSettings = settings;
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            config.Formatters.Add(formatter);

            appBuilder.UseWebApi(config);
            using (var db = new ModelDbContext())
            {
                db.Documents.Count();
            }
        }
    }

    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "AllCompanies",
                routeTemplate: "companies",
                defaults: new { controller = "PayStubContent", action = "Get" }
                );
        }
    }

    public class DateTimeConverter : DateTimeConverterBase
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return DateTime.Parse(reader.Value.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime dt = (DateTime)value;
            writer.WriteValue(dt.ToFileTimeUtc());
        }
    }
}