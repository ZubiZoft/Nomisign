using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Aspose.Pdf.Forms;
using System;
using System.Collections;
using System.IO;

namespace CfdiService.Services
{
    public static class DigitalSignatures
    {
        private static readonly string _pfxFileName = System.Configuration.ConfigurationManager.AppSettings["pfxFileName"];
        private static readonly string _pfxFilePassword = System.Configuration.ConfigurationManager.AppSettings["pfxFilePassword"];
        //private static readonly string _logoFileName = System.Configuration.ConfigurationManager.AppSettings["logoFileName"];
        //private static readonly string _signXmlFileName = System.Configuration.ConfigurationManager.AppSettings["signXmlFileName"];
        private static readonly string _rootDiskPath = System.Configuration.ConfigurationManager.AppSettings["rootDiskPath"];
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string SignPdfDocument(CfdiService.Model.Document originalPdfDocument)
        {
            try
            {
                // get document path
                string originalPdfDocumentPath = NomiFileAccess.GetFilePath(originalPdfDocument);
                string signedpath = NomiFileAccess.GetFilePathSigned(originalPdfDocument);
                using (Aspose.Pdf.Document document = new Aspose.Pdf.Document(originalPdfDocumentPath))
                {
                    using (PdfFileSignature signature = new PdfFileSignature(document))
                    {
                        PKCS7 pkcs = new PKCS7(_rootDiskPath + @"\" + _pfxFileName, _pfxFilePassword); // "RQP@ssw0rd"); // Use PKCS7/PKCS7Detached objects
                        pkcs.Location = "Mexico";
                        pkcs.Reason = "Approved by: " + originalPdfDocument.Employee.FullName;
                        DocMDPSignature docMdpSignature = new DocMDPSignature(pkcs, DocMDPAccessPermissions.FillingInForms);
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(50, 500, 500, 100);
                        log.Info("document.Pages.Count : " + document.Pages.Count);
                        // Create any of the three signature types
                        signature.Certify(document.Pages.Count, "Signature Reason", "Contact", "Location", true, rect, docMdpSignature);
                        // Save output PDF file
                        signature.Save(signedpath);
                        // Create backup of si gned copy to location 2 - no database record needed
                        NomiFileAccess.BackupFileToLocation2(originalPdfDocument.CompanyId, originalPdfDocumentPath);
                    }
                }
                File.Delete(originalPdfDocumentPath);
                return signedpath;
            }
            catch (Exception ex)
            {
                log.Error("Error signing PDF documents:  " + originalPdfDocument, ex);
            }
            return null;
        }

        public static void SignPdfDocumentMergedDocs(CfdiService.Model.Document modeldoc,ref Document document)
        {
            try
            {
                // get document path
                string originalPdfDocumentPath = NomiFileAccess.GetFilePath(modeldoc);
                
                using (PdfFileSignature signature = new PdfFileSignature(document))
                {
                    PKCS7 pkcs = new PKCS7(_rootDiskPath + @"\" + _pfxFileName, _pfxFilePassword); // "RQP@ssw0rd"); // Use PKCS7/PKCS7Detached objects
                    pkcs.Location = "Mexico";
                    pkcs.Reason = "Approved by: " + modeldoc.Employee.FullName;
                    DocMDPSignature docMdpSignature = new DocMDPSignature(pkcs, DocMDPAccessPermissions.FillingInForms);
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(100, 100, 400, 100);
                    // Create any of the three signature types
                    signature.Certify(1, "Signature Reason", "Contact", "Location", true, rect, docMdpSignature);
                    // Save output PDF file
                    signature.Save(originalPdfDocumentPath);
                    // Create backup of signed copy to location 2 - no database record needed
                    //NomiFileAccess.BackupFileToLocation2(modeldoc.CompanyId, originalPdfDocumentPath);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error signing PDF documents:  " + modeldoc, ex);
            }
        }
    }
}