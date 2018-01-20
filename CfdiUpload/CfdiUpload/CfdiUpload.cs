using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;

namespace CfdiService.Upload
{ 
    class CfdiUpload
    {
        static void Main(string[] args)
        {
            string CfdiServiceUrl = ConfigurationManager.AppSettings["CfdiServiceUrl"];
            string CfdiServiceKey = ConfigurationManager.AppSettings["CfdiServiceKey"];
            string CompanyId = ConfigurationManager.AppSettings["CompanyId"];

            string uploadDirectory = Environment.CurrentDirectory;
            if (args.Length >= 3 && args[1] == "-d")
                uploadDirectory = args[2];
            else if (args.Length > 1)
            {
                Console.WriteLine("CfdiUpload: usage: CfdiUpload [-d upload_directory]");
                Environment.Exit(1);
            }
            CfdiUpload uploader = new CfdiUpload(uploadDirectory, CfdiServiceUrl, CompanyId, CfdiServiceKey);
            uploader.CompanyRfc = ConfigurationManager.AppSettings["CompanyRfc"]; ;

            try
            {
                Console.WriteLine("Cfdi Uploader v1.0");
                Console.WriteLine("Uploading from directory: " + uploadDirectory);
                uploader.Upload();
            }
            catch (Exception ex)
            {
                string inner = ex.InnerException != null ? $" ({ex.InnerException.Message})" : "";
                Console.WriteLine($"Error: {ex.Message}{inner}");
            }
            uploader.CloseBatch();

            Console.WriteLine("Upload Completed");
            Console.ReadLine();
            Environment.Exit(0);
        }

        public static int MaxXMLSize = 20000;
        public string CfdiServiceBaseUrl { get; set; }
        public string CompanyRfc { get; set; }
        public string CompanyApiKey { get; set; }
        public string UploadDirectory { get; set; }
        public string CompanyId { get; set; }
        public string BatchId { get; set; }
        public CfdiServiceProxy cfdiService { get; set; }

        public CfdiUpload(string dir, string baseUrl, string companyId, string apiKey)
        {
            CompanyId = companyId;
            CompanyApiKey = apiKey;
            UploadDirectory = dir;
            BatchId = String.Empty;
            CfdiServiceBaseUrl = baseUrl;
            cfdiService = new CfdiServiceProxy(baseUrl, companyId);
        }

        public void Upload()
        {
            List<string> uploadFiles = GetUploadFiles();
            CreateBatch(uploadFiles.Count);
            foreach (string fn in uploadFiles)
            {
                string xmlContent = File.ReadAllText(fn);
                Byte[] bytes = File.ReadAllBytes(Path.ChangeExtension(fn, "pdf"));
                String pdfContent = Convert.ToBase64String(bytes);

                var fileUpload = new FileUpload
                {
                    XMLContent = xmlContent,
                    PDFContent = pdfContent,
                    FileName = fn,
                    FileHash = ComputeFileHash(xmlContent)
                };
                BatchResult br = cfdiService.UploadFile(BatchId, fileUpload);
                if (br.ResultCode == BatchResultCode.Ok)
                    File.Move(fn, fn + ".tx");
                else
                    throw new CfdiUploadException(br.ResultCode);
            }
        }

        private List<string> GetUploadFiles()
        {
            IEnumerable<string> files = Directory.EnumerateFiles(UploadDirectory, "*.xml");
            List<string> uploadFiles = new List<string>();

            string testFile = Path.Combine(UploadDirectory, Path.GetRandomFileName());
            try
            {
                File.WriteAllText(testFile, "test");
            }
            catch (Exception ex)
            {
                throw new CfdiUploadException($"The upload directory is not writable {UploadDirectory}", ex);
            }
            finally
            {
                try { File.Delete(testFile); } catch (Exception) { }
            }

            foreach (string fn in files)
            {
                if (ValidCfdiXML(fn))
                {
                    if (ValidateHasPDF(fn))
                        uploadFiles.Add(fn);
                    else
                        Console.WriteLine($"{fn} does not have a matching PDF file.");
                }
                else
                    Console.WriteLine($"{fn} is not a valid Cfdi XML file.");
                
            }
            return uploadFiles;            
        }

        private void CreateBatch(int fileCount)
        {
            OpenBatch batch = new OpenBatch
            {
                CompanyRfc = CompanyRfc,
                ApiKey = CompanyApiKey,
                FileCount = fileCount
            };

            BatchResult result = cfdiService.OpenBatch(batch);
            if (result.ResultCode != BatchResultCode.Ok)
                throw new CfdiUploadException(result.ResultCode);

            BatchId = result.BatchId.ToString();
        }

        public void CloseBatch()
        {
            cfdiService.CloseBatch(BatchId);
        }

        private bool ValidCfdiXML(string fn)
        {
            FileInfo fi = new FileInfo(fn);
            if (fi.Length < MaxXMLSize)
            {
                string xml = File.ReadAllText(fn);
                return (xml.Contains("<cfdi:Comprobante") && xml.Contains("<nomina12:Nomina"));
            }
            return false;
        }

        private bool ValidateHasPDF(string fn)
        {
            string pdf = Path.ChangeExtension(fn, "pdf");
            return File.Exists(pdf);
        }

        private string ComputeFileHash(string content)
        {
            SHA256 mySHA256 = SHA256Managed.Create();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            byte[] hashValue = mySHA256.ComputeHash(ms);
            var stringBuilder = new StringBuilder();
            foreach (byte b in hashValue)
                stringBuilder.AppendFormat("{0:x2}", b);

            return stringBuilder.ToString();
        }
    }
}
