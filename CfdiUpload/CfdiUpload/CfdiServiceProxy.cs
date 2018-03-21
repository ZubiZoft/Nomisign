
using CfdiService.Shapes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace CfdiService.Upload
{
    public class CfdiServiceProxy
    {
        string svcUrl;
        HttpClient client;
        string companyId;

        public CfdiServiceProxy(string svcUrl, string companyId)
        {
            this.svcUrl = svcUrl;
            this.companyId = companyId;
            client = new HttpClient();
            client.BaseAddress = new Uri(svcUrl);
            client.DefaultRequestHeaders.Accept.Clear();
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
                throw new CfdiUploadException($"OpenBatch request to cfdiService failed with result code: {response.StatusCode} ({(int)response.StatusCode})");

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

        public string UploadFiles(string companyid, List<FileUpload> upload)
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
            }
            else
                throw new CfdiUploadException($"AddFile request to cfdiService failed with result code: {response.StatusCode} ({(int)response.StatusCode})");

            return result;
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
    }
}
