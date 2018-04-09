
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Configuration;
using System.IO;

namespace CfdiService.Upload
{
    public class CfdiServiceProxy
    {
        string svcUrl;
        HttpClient client;
        string companyId;
        string CfdiServiceKey = ConfigurationManager.AppSettings["CfdiServiceKey"];

        public CfdiServiceProxy(string svcUrl, string companyId)
        {
            this.svcUrl = svcUrl;
            this.companyId = companyId;
            client = new HttpClient();
            client.BaseAddress = new Uri(svcUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("ClientType", "uploader");
            client.DefaultRequestHeaders.Add("Authorization", "Basic " + CfdiServiceKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public BatchResult OpenBatch(OpenBatch batch)
        {
            BatchResult result = null;

            Task<HttpResponseMessage> openTask = client.PostAsJsonAsync($"api/upload/openbatch/{companyId}", batch);
            try { openTask.Wait(); }
            catch (Exception) { throw openTask.Exception.InnerException; }

            HttpResponseMessage response = openTask.Result;
            if (response.IsSuccessStatusCode)
            {
                Task<BatchResult> resultTask = response.Content.ReadAsAsync<BatchResult>();
                resultTask.Wait();
                result = resultTask.Result;
            }
            else
            {
                Console.WriteLine(string.Format("Upload failed due there are more nominas than available licenses to use in the system"));
                LogErrorMessage(string.Format("Upload failed due there are more nominas than available licenses to use in the system"));
            }

            return result;
        }

        public async Task<BatchResult> UploadFileAsync(string batchid, FileUpload upload)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync($"api/upload/addfile/{batchid}", upload);
            response.EnsureSuccessStatusCode();
            BatchResult result = await response.Content.ReadAsAsync<BatchResult>();
            return result;
        }

        public BatchResult UploadFile(string batchid, FileUpload upload)
        {
            BatchResult result = null;
            Task<HttpResponseMessage> openTask = client.PostAsJsonAsync($"api/upload/addfile/{batchid}", upload);

            try { openTask.Wait(); }
            catch (Exception) { throw openTask.Exception.InnerException; }

            HttpResponseMessage response = openTask.Result;
            if (response.IsSuccessStatusCode)
            {
                Task<BatchResult> resultTask = response.Content.ReadAsAsync<BatchResult>();
                resultTask.Wait();
                result = resultTask.Result;
            }
            else
                throw new CfdiUploadException($"AddFile request to cfdiService failed with result code: {response.StatusCode} ({(int)response.StatusCode})");

            return result;
        }

        public bool UploadFiles(string companyid, List<FileUpload> upload)
        {
            string result = null;
            Task<HttpResponseMessage> openTask = client.PostAsJsonAsync($"api/upload/uploadfilesfront/{companyid}", upload);

            try { openTask.Wait(); }
            catch (Exception) { throw openTask.Exception.InnerException; }

            HttpResponseMessage response = openTask.Result;
            if (response.IsSuccessStatusCode)
            {
                Task<string> resultTask = response.Content.ReadAsAsync<string>();
                resultTask.Wait();
                result = resultTask.Result;
                return true;
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                Console.WriteLine("Upload was not completed. Please review the company ID, API Key and Company RFC in your configuration file.");
                LogErrorMessage(string.Format("Upload was not completed. Please review the company ID, API Key and Company RFC in your configuration file."));
                return false;
            }
            else
            {
                Console.WriteLine("Upload was not completed due the number of licenses are less than the number of nominas that you are attempting to upload.");
                LogErrorMessage(string.Format("Upload was not completed due the number of licenses are less than the number of nominas that you are attempting to upload."));
                return false;
            }
        }

        public void CloseBatch(string batchid)
        {
            Task<HttpResponseMessage> reqTask = client.GetAsync($"api/upload/closebatch/{batchid}");

            try { reqTask.Wait(); }
            catch (Exception) { throw reqTask.Exception.InnerException; }

            HttpResponseMessage response = reqTask.Result;
            if (!response.IsSuccessStatusCode)
                throw new CfdiUploadException($"CloseBatch request to cfdiService failed with result code: {response.StatusCode} ({(int)response.StatusCode})");
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
