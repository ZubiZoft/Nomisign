using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using System.Xml.Linq;
using System.Linq;

namespace CfdiService.Upload
{ 
    class CfdiUpload
    {
        static void Main(string[] args)
        {
            string CfdiServiceUrl = ConfigurationManager.AppSettings["CfdiServiceUrl"];
            string CfdiServiceKey = ConfigurationManager.AppSettings["CfdiServiceKey"];
            string CompanyId = ConfigurationManager.AppSettings["CompanyId"];

            Console.WriteLine(string.Format("Started Upload Prcess for Company Id {0}", CompanyId));
            LogErrorMessage(string.Format("Started Upload Prcess for Company Id {0}", CompanyId));

            //Environment.Exit(0);

            string uploadDirectory = Environment.CurrentDirectory;
            if (args.Length >= 3 && args[1] == "-d")
                uploadDirectory = args[2];
            else if (args.Length > 1)
            {
                Console.WriteLine("CfdiUpload: usage: CfdiUpload [-d upload_directory]");
                LogErrorMessage("CfdiUpload: usage: CfdiUpload [-d upload_directory]");
                Environment.Exit(1);
            }
            CfdiUpload uploader = new CfdiUpload(uploadDirectory, CfdiServiceUrl, CompanyId, CfdiServiceKey);
            uploader.CompanyRfc = ConfigurationManager.AppSettings["CompanyRfc"]; ;
            uploader.CompanyApiKey = ConfigurationManager.AppSettings["CfdiServiceKey"]; 
            Console.WriteLine(string.Format("uploader.CompanyApiKey : {0}", uploader.CompanyApiKey));
            LogErrorMessage(string.Format("uploader.CompanyApiKey : {0}", uploader.CompanyApiKey));
            try
            {
                Console.WriteLine("Cfdi Uploader v1.0");
                LogErrorMessage(string.Format("Cfdi Uploader v1.0"));
                Console.WriteLine("Uploading from directory: " + uploadDirectory);
                LogErrorMessage(string.Format("Uploading from directory:  {0}", uploadDirectory));
                uploader.Upload();
            }
            catch (Exception ex)
            {
                string inner = ex.InnerException != null ? $" ({ex.InnerException.Message})" : "";
                Console.WriteLine($"Error: {ex.Message}{inner}");
                LogErrorMessage(string.Format("Error Inner:  {0}", ex.InnerException.Message));
                LogErrorMessage(string.Format("Error Exception:  {0}", ex.StackTrace));
            }
            //uploader.CloseBatch();

            
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
            try
            {
                List<string> uploadFiles = GetUploadFiles();
                List<FileUpload> files = new List<FileUpload>();
                //CreateBatch(uploadFiles.Count);
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

                    if (ValidateValidRfc(fileUpload.XMLContent))
                        files.Add(fileUpload);
                    else
                        LogErrorMessage(string.Format("File Is not going to be Uploaded:  {0}", fileUpload.XMLContent));

                    //if (br.ResultCode == BatchResultCode.Ok)
                    //    File.Move(fn, fn + ".tx");
                    //else
                    //    throw new CfdiUploadException(br.ResultCode);
                }

                LogErrorMessage(string.Format("Files to upload:  {0}", files.Count));

                string br = "";
                Console.WriteLine(string.Format("Nominas to be uploaded: {0}", files.Count));
                if (files.Count > 0)
                    br = cfdiService.UploadFiles(CompanyId, files);
            }
            catch (Exception ex) { LogErrorMessage(string.Format("Ex Stacktrace:  {0}", ex.StackTrace)); LogErrorMessage(string.Format("Ex Message:  {0}", ex.Message)); }
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
                    {
                        uploadFiles.Add(fn);
                    }
                    else
                    {
                        Console.WriteLine($"{fn} does not have a matching PDF file.");
                        LogErrorMessage(string.Format($"{fn} does not have a matching PDF file."));
                    }
                }
                else
                {
                    Console.WriteLine($"{fn} is not a valid Cfdi XML file.");
                    LogErrorMessage(string.Format($"{fn} is not a valid Cfdi XML file."));
                }
                
            }
            return uploadFiles;            
        }

        public bool ValidateValidRfc(string contentxml)
        {
            string CompanyRFC = ConfigurationManager.AppSettings["CompanyRfc"];
            XElement root;
            byte[] content = Encoding.UTF8.GetBytes(contentxml);
            using (MemoryStream ms = new MemoryStream(content))
                root = XElement.Load(ms);

            XNamespace cfdi = "http://www.sat.gob.mx/cfd/3";
            XNamespace nomina12 = "http://www.sat.gob.mx/nomina12";

            //XElement elem = root.Element(cfdi + "Emisor");
            XElement elem = null;
            ElementCheckXMLTagValue(cfdi, "Emisor", root, ref elem);
            XAttribute emisorRfc = null;
            AttributeCheckXMLTagValue("rfc", elem, ref emisorRfc);

            if (emisorRfc.Value.Equals(CompanyRFC))
                return true;
            else
                return false;
        }

        public void ElementCheckXMLTagValue(XNamespace cfdi, string tag, XElement root, ref XElement elem2)
        {
            try { elem2 = root.Element(cfdi + tag); } catch { }
            if (elem2 == null)
            { try { elem2 = root.Element(cfdi + FirstCharToUpper(tag)); } catch { } }
            if (elem2 == null)
            { try { elem2 = root.Element(cfdi + FirstCharToLower(tag)); } catch { } }
            if (elem2 == null)
            { try { elem2 = root.Element(cfdi + tag.ToLower()); } catch { } }
            if (elem2 == null)
            { try { elem2 = root.Element(cfdi + tag.ToUpper()); } catch { } }
        }

        public void AttributeCheckXMLTagValue(string tag, XElement elem, ref XAttribute element)
        {
            try { element = elem.Attribute(tag); } catch { }
            if (element == null)
            { try { element = elem.Attribute(FirstCharToUpper(tag)); } catch { } }
            if (element == null)
            { try { element = elem.Attribute(FirstCharToLower(tag)); } catch { } }
            if (element == null)
            { try { element = elem.Attribute(tag.ToLower()); } catch { } }
            if (element == null)
            { try { element = elem.Attribute(tag.ToUpper()); } catch { } }
        }

        public string FirstCharToLower(string input)
        {
            switch (input)
            {
                case null: return "";
                case "": return "";
                default: return input.First().ToString().ToLower() + input.Substring(1);
            }
        }

        public static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: return "";
                case "": return "";
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }

        private void CreateBatch(int fileCount)
        {
            OpenBatch batch = new OpenBatch
            {
                CompanyRfc = CompanyRfc,
                //ApiKey = CompanyApiKey,
                ApiKey = ConfigurationManager.AppSettings["CfdiServiceKey"],
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

        public static void LogErrorMessage(string message)
        {
            string filename = string.Format("logfile-{0}.log", DateTime.Now.ToString("dd-MM-yyyy"));
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, filename))) //No File? Create
            {
                Stream stream = File.Create(Path.Combine(Environment.CurrentDirectory, filename));
                stream.Close();
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(Environment.CurrentDirectory, filename), true))
                {
                    file.WriteLine(string.Format("[{0}] - {1}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), message));
                }
            }
            else
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(Environment.CurrentDirectory, filename), true))
                {
                    file.WriteLine(string.Format("[{0}] - {1}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), message));
                }
            }

        }
    }
}
