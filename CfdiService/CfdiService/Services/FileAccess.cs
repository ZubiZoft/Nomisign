using CfdiService.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace CfdiService.Services
{
    public static class NomiFileAccess
    {
        private static readonly string _rootDiskPath = System.Configuration.ConfigurationManager.AppSettings["rootDiskPath"];
        private static string _rootSystemPath;
        private static Dictionary<int, string> companyPaths;
        
        static NomiFileAccess()
        {
            // get system path - system path is a subsection of 
            // root path in case multiple system records are ever supported
            ModelDbContext db = new ModelDbContext();
            _rootSystemPath = db.Settings.FirstOrDefault().SystemFilePath1;

            // cache company root path guids
            companyPaths = db.Companies.ToDictionary(t => t.CompanyId, t => t.DocStoragePath1);
        }
            
        private static string RootFilePath
        {
            get
            {
                return _rootDiskPath;
            }
        }

        private static string RootSystemPath
        {
            get
            {
                return _rootSystemPath;
            }
        }


        public static bool WriteFile(Document document, string base64String)
        {
            try
            {
                verifyCompanyCache(document.CompanyId);
                // path to file is computed
                // 1. root from web.config
                // 2. system working directory
                // 3. company id
                // 4. batch Id
                var fullFilePath = Path.Combine(RootFilePath, RootSystemPath, string.Format(@"{0}\{1}\", companyPaths[document.CompanyId], document.Batch.WorkDirectory));

                // make sure path exists, if not this will create it
                Directory.CreateDirectory(fullFilePath);

                // need to manage root path from this class properties
                SaveByteArrayAsImage(Path.Combine(fullFilePath, document.PathToFile + ".jpg"), base64String); // TODO: jpg needs removed and PDF supported
            }
            catch(Exception ex)
            {
                // log exception
                return false;
            }
            return true;
        }

        private static void verifyCompanyCache(int companyId)
        {
            // verify company working directory path
            if (!companyPaths.ContainsKey(companyId))
            {
                ModelDbContext db = new ModelDbContext();
                // try refresh the data cache
                // cache company root path guids
                companyPaths = db.Companies.ToDictionary(t => t.CompanyId, t => t.DocStoragePath1);
            }

            // verify company working directory path
            if (!companyPaths.ContainsKey(companyId))
            {
                throw new Exception("Invalid Company Id");
            }
        }

        public static string GetFile(Document document)
        {
            verifyCompanyCache(document.CompanyId);
            // get full path to file
            // path to file is computed
            // 1. root from web.config
            // 2. system working directory
            // 3. company id
            // 4. batch Id
            var fullFilePath = Path.Combine(RootFilePath, RootSystemPath, string.Format(@"{0}\{1}\", companyPaths[document.CompanyId], document.Batch.WorkDirectory));
            // return image
            return GetDocBytesAsbase64(fullFilePath, document.PathToFile);
        }


        private static string GetDocBytesAsbase64(string path, string docNameGuid)
        {
            try
            {
                var fullFilePath = Path.Combine(path, docNameGuid + ".jpg");
                if (File.Exists(fullFilePath))
                {
                    Byte[] bytes = File.ReadAllBytes(fullFilePath);
                    String file = Convert.ToBase64String(bytes);
                    return "data:image/jpeg;base64," + file;
                }

                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("Document Not Found!!");
                // TODO: this dont seem to work!!
                return "data:text/plain;base64," + System.Convert.ToBase64String(plainTextBytes);
            }
            catch
            {
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("Document Not Found!!");
                // TODO: this dont seem to work!!
                return "data:text/plain;base64," + System.Convert.ToBase64String(plainTextBytes);
            }

        }

        private static void SaveByteArrayAsImage(string fullOutputPath, string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
                image.Save(fullOutputPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

        }
    }
}