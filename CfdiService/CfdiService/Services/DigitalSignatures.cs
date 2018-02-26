using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Aspose.Pdf.Forms;
using System;
using System.Collections;
using System.IO;
using Aspose.Pdf.Text;

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
                string temppath = NomiFileAccess.GetFilePathTemp(originalPdfDocument);
                log.Info("Path original: " + originalPdfDocumentPath);
                log.Info("Path signed: " + signedpath);
                log.Info("Path temp: " + temppath);
                using (Aspose.Pdf.Document document = new Aspose.Pdf.Document(originalPdfDocumentPath))
                {
                    //Draw Rectangle
                    Aspose.Pdf.Drawing.Graph canvas = new Aspose.Pdf.Drawing.Graph(100, 400);
                    document.Pages[document.Pages.Count].Paragraphs.Add(canvas);
                    Aspose.Pdf.Drawing.Rectangle rectt = new Aspose.Pdf.Drawing.Rectangle(0, 250, 230, 150);
                    rectt.GraphInfo.Color = Aspose.Pdf.Color.Black;
                    rectt.GraphInfo.DashPhase = 5;
                    canvas.Shapes.Add(rectt);
                    //Company Authorization Text
                    Aspose.Pdf.Text.TextFragment title = new Aspose.Pdf.Text.TextFragment("AUTORIZADO POR: NOMISIGN S.A. DE C.V.");
                    title.Position = new Position(100, 750);
                    title.TextState.FontSize = 8;
                    title.TextState.Font = FontRepository.FindFont("Arial");
                    title.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilder = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilder.AppendText(title);
                    //Employee Auotrization Name
                    Aspose.Pdf.Text.TextFragment titleemp = new Aspose.Pdf.Text.TextFragment(string.Format("FIRMADO POR: {0}", originalPdfDocument.Employee.FullName));
                    titleemp.Position = new Position(100, 730);
                    titleemp.TextState.FontSize = 8;
                    titleemp.TextState.Font = FontRepository.FindFont("Arial");
                    titleemp.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilderemp = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilderemp.AppendText(titleemp);
                    //Hash Code 1P
                    string hash = originalPdfDocument.FileHash;
                    if (!string.IsNullOrEmpty(hash))
                    {
                        Aspose.Pdf.Text.TextFragment titlehash = new Aspose.Pdf.Text.TextFragment(string.Format("HASH: {0}", hash.Substring(0, 42)));
                        titlehash.Position = new Position(100, 710);
                        titlehash.TextState.FontSize = 8;
                        titlehash.TextState.Font = FontRepository.FindFont("Arial");
                        titlehash.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                        TextBuilder textBuilderhash = new TextBuilder(document.Pages[document.Pages.Count]);
                        textBuilderhash.AppendText(titlehash);
                        //Hash Code 2P
                        if (hash.Length > 42)
                        {
                            Aspose.Pdf.Text.TextFragment titlehash2 = new Aspose.Pdf.Text.TextFragment(hash.Substring(42));
                            titlehash2.Position = new Position(100, 700);
                            titlehash2.TextState.FontSize = 8;
                            titlehash2.TextState.Font = FontRepository.FindFont("Arial");
                            titlehash2.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                            TextBuilder textBuilderhash2 = new TextBuilder(document.Pages[document.Pages.Count]);
                            textBuilderhash2.AppendText(titlehash2);
                        }
                    }
                    //Last login
                    Aspose.Pdf.Text.TextFragment lastLogEmp = new Aspose.Pdf.Text.TextFragment(string.Format("FECHA DE INICIO DE SESIÓN: {0}", originalPdfDocument.Employee.LastLoginDate.ToString("dd/MM/yyyy HH:mm:ss fff")));
                    lastLogEmp.Position = new Position(100, 680);
                    lastLogEmp.TextState.FontSize = 8;
                    lastLogEmp.TextState.Font = FontRepository.FindFont("Arial");
                    lastLogEmp.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilderlastLog = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilderlastLog.AppendText(lastLogEmp);

                    //Timestamp
                    Aspose.Pdf.Text.TextFragment timestamp = new Aspose.Pdf.Text.TextFragment(string.Format("FECHA DE FIRMA: {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss fff")));
                    timestamp.Position = new Position(100, 660);
                    timestamp.TextState.FontSize = 8;
                    timestamp.TextState.Font = FontRepository.FindFont("Arial");
                    timestamp.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilderTimestamp = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilderTimestamp.AppendText(timestamp);

                    document.Save(temppath);
                }
                using (Aspose.Pdf.Document document = new Aspose.Pdf.Document(temppath))
                {
                    using (PdfFileSignature signature = new PdfFileSignature(document))
                    {
                        PKCS7 pkcs = new PKCS7(_rootDiskPath + @"\" + _pfxFileName, _pfxFilePassword); // "RQP@ssw0rd"); // Use PKCS7/PKCS7Detached objects
                        pkcs.Location = "Mexico";
                        pkcs.Reason = "Approved by: " + originalPdfDocument.Employee.FullName;
                        pkcs.ShowProperties = false;
                        DocMDPSignature docMdpSignature = new DocMDPSignature(pkcs, DocMDPAccessPermissions.FillingInForms);
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(90, 620, 230, 150);
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
                File.Delete(temppath);
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