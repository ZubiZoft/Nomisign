using CfdiService.Model;
using CfdiService.Shapes;
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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string _rootSystemPath;
        private static Dictionary<int, string> companyPaths1;
        private static Dictionary<int, string> companyPaths2;

        static NomiFileAccess()
        {
            // get system path - system path is a subsection of 
            // root path in case multiple system records are ever supported
            ModelDbContext db = new ModelDbContext();
            _rootSystemPath = db.Settings.FirstOrDefault().SystemFilePath1;

            // cache company root path guids
            companyPaths1 = db.Companies.ToDictionary(t => t.CompanyId, t => t.DocStoragePath1);
            companyPaths2 = db.Companies.ToDictionary(t => t.CompanyId, t => t.DocStoragePath2);
        }

        internal static string GetFilePath(Document document)
        {
            verifyCompanyCache1(document.CompanyId);
            // get full path to file
            // path to file is computed
            // 1. root from web.config
            // 2. system working directory
            // 3. company id
            // 4. batch Id
            var fullFilePath = Path.Combine(RootFilePath, RootSystemPath, string.Format(@"{0}\{1}\", companyPaths1[document.CompanyId], document.Batch.WorkDirectory));
            // return image
            return Path.Combine(fullFilePath, document.PathToFile + ".pdf");
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

        public static bool WriteCompanyAgreementFile(Company company, FileUpload fileInfo)
        {
            try
            {
                verifyCompanyCache1(company.CompanyId);
                // path to file is computed
                // 1. root from web.config
                // 2. system working directory
                // 3. company id
                // 4. batch Id
                var fullFilePath = Path.Combine(RootFilePath, RootSystemPath, string.Format(@"{0}\", companyPaths1[company.CompanyId]));

                // make sure path exists, if not this will create it
                Directory.CreateDirectory(fullFilePath);

                // need to manage root path from this class properties
                SaveByteArrayAsImage(Path.Combine(fullFilePath, fileInfo.FileName), fileInfo.PDFContent); 
            }
            catch (Exception ex)
            {
                log.Error("Error writing company file to disk: ", ex);
                return false;
            }
            return true;
        }

        public static bool WriteEncodedFile(Document document, string base64String, string extension)
        {
            try
            {
                verifyCompanyCache1(document.CompanyId);
                // path to file is computed
                // 1. root from web.config
                // 2. system working directory
                // 3. company id
                // 4. batch Id
                var fullFilePath = Path.Combine(RootFilePath, RootSystemPath, string.Format(@"{0}\{1}\", companyPaths1[document.CompanyId], document.Batch.WorkDirectory));

                // make sure path exists, if not this will create it
                Directory.CreateDirectory(fullFilePath);

                // need to manage root path from this class properties
                SaveByteArrayAsImage(Path.Combine(fullFilePath, document.PathToFile + extension), base64String);
            }
            catch(Exception ex)
            {
                log.Error("Error writing file to disk: ", ex);
                return false;
            }
            return true;
        }

        public static bool WriteFile(Document document, string content, string extension)
        {
            try
            {
                verifyCompanyCache1(document.CompanyId);
                // path to file is computed
                // 1. root from web.config
                // 2. system working directory
                // 3. company id
                // 4. batch Id
                var fullFilePath = Path.Combine(RootFilePath, RootSystemPath, string.Format(@"{0}\{1}\", companyPaths1[document.CompanyId], document.Batch.WorkDirectory));

                // make sure path exists, if not this will create it
                Directory.CreateDirectory(fullFilePath);

                // need to manage root path from this class properties
                File.WriteAllText(Path.Combine(fullFilePath, document.PathToFile + extension), content);
            }
            catch (Exception ex)
            {
                log.Error("Error writing file to disk: ", ex);
                return false;
            }
            return true;
        }

        public static string CopyCompanyAgreementFileForEmployee(int companyId, string workingDirectory, string companyAgreementFile)
        {
            try
            {
                verifyCompanyCache1(companyId);
                // path to file is computed
                // 1. root from web.config
                // 2. system working directory
                // 3. company id
                // 4. batch Id
                var fullFileDestPath = Path.Combine(RootFilePath, RootSystemPath, string.Format(@"{0}\{1}\", companyPaths1[companyId], workingDirectory));
                var fileSourceDocumentPath = Path.Combine(RootFilePath, RootSystemPath, companyPaths1[companyId]);

                // make sure path exists, if not this will create it
                Directory.CreateDirectory(fullFileDestPath);

                var fileName = Guid.NewGuid().ToString();
                // need to manage root path from this class properties
                File.Copy(fileSourceDocumentPath + "\\" + companyAgreementFile, Path.Combine(fullFileDestPath, fileName + Strings.PDF_EXT));
                return fileName;
            }
            catch (Exception ex)
            {
                log.Error("Error copying file to disk: ", ex);
                return string.Empty;
            }
        }

        private static void verifyCompanyCache1(int companyId)
        {
            // verify company working directory path
            if (!companyPaths1.ContainsKey(companyId))
            {
                ModelDbContext db = new ModelDbContext();
                // try refresh the data cache
                // cache company root path guids
                companyPaths1 = db.Companies.ToDictionary(t => t.CompanyId, t => t.DocStoragePath1);
            }

            // verify company working directory path
            if (!companyPaths1.ContainsKey(companyId))
            {
                throw new Exception("Invalid Company Id");
            }
        }

        private static void verifyCompanyCache2(int companyId)
        {
            // verify company working directory path
            if (!companyPaths2.ContainsKey(companyId))
            {
                ModelDbContext db = new ModelDbContext();
                // try refresh the data cache
                // cache company root path guids
                companyPaths2 = db.Companies.ToDictionary(t => t.CompanyId, t => t.DocStoragePath2);
            }

            // verify company working directory path
            if (!companyPaths2.ContainsKey(companyId))
            {
                throw new Exception("Invalid Company Id");
            }
        }

        public static string GetFile(Document document)
        {
            verifyCompanyCache1(document.CompanyId);
            // get full path to file
            // path to file is computed
            // 1. root from web.config
            // 2. system working directory
            // 3. company id
            // 4. batch Id
            var fullFilePath = Path.Combine(RootFilePath, RootSystemPath, string.Format(@"{0}\{1}\", companyPaths1[document.CompanyId], document.Batch.WorkDirectory));
            // return image
            return GetDocBytesAsbase64(fullFilePath, document.PathToFile);
        }

        public static void BackupFileToLocation2(int companyId, string location1FilePath)
        {
            verifyCompanyCache1(companyId);
            verifyCompanyCache2(companyId);

            var locaiton2FilePath = location1FilePath.Replace(companyPaths1[companyId], companyPaths2[companyId]);
            Directory.CreateDirectory(Path.GetDirectoryName(locaiton2FilePath));
            File.Copy(location1FilePath, locaiton2FilePath, true); // does this create the entire path???
        }

        private static string GetDocBytesAsbase64(string path, string docNameGuid)
        {
            try
            {
                var fullFilePath = Path.Combine(path, docNameGuid + ".pdf");

                if (File.Exists(fullFilePath))
                {
                    using (MemoryStream pdfAsImage = ConvertPdfToJpg(fullFilePath))
                    {
                        //Byte[] bytes = File.ReadAllBytes(fullFilePath);
                        Byte[] bytes = pdfAsImage.ToArray();
                        String file = Convert.ToBase64String(bytes);
                        return "data:image/png;base64," + file;
                    }
                }
                log.Error("Document Not Found: " + fullFilePath);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("Document Not Found!!");
                // TODO: this dont seem to work!!
                return "data:text/plain;base64," + System.Convert.ToBase64String(plainTextBytes);
            }
            catch(Exception ex)
            {
                log.Error("Document Not Found: " + path + "\\" + docNameGuid + ".pdf", ex);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes("Document Not Found!!");
                // TODO: this dont seem to work!!
                return "data:text/plain;base64," + System.Convert.ToBase64String(plainTextBytes);
            }

        }

        private static MemoryStream ConvertPdfToJpg(string fullFilePath)
        {
            Aspose.Pdf.Document pdfDoc = new Aspose.Pdf.Document(fullFilePath);
            return pdfDoc.ConvertPageToPNGMemoryStream(pdfDoc.Pages[1]);
        }

        private static void SaveByteArrayAsImage(string fullOutputPath, string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);

            using (System.IO.FileStream stream = new FileStream(fullOutputPath, FileMode.CreateNew))
            {
                System.IO.BinaryWriter writer =
                    new BinaryWriter(stream);
                writer.Write(bytes, 0, bytes.Length);
                writer.Close();
            }
        }
    }
}