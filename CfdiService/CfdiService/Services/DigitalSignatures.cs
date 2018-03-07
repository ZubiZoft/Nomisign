using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Aspose.Pdf.Forms;
using System;
using System.Collections;
using System.IO;
using Aspose.Pdf.Text;
using System.Globalization;
using System.Threading;

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
                var docCount = 0;
                using (Aspose.Pdf.Document document = new Aspose.Pdf.Document(originalPdfDocumentPath))
                {
                    docCount = document.Pages.Count;

                    //Draw Rectangle
                    Aspose.Pdf.Drawing.Graph canvas = new Aspose.Pdf.Drawing.Graph(250, 450);
                    document.Pages[document.Pages.Count].Paragraphs.Add(canvas);
                    Aspose.Pdf.Drawing.Rectangle rectt = new Aspose.Pdf.Drawing.Rectangle(0, 230, 410, 220);
                    rectt.GraphInfo.Color = Aspose.Pdf.Color.Black;
                    rectt.GraphInfo.DashPhase = 5;
                    canvas.Shapes.Add(rectt);
                    //Company Authorization Text
                    /*Aspose.Pdf.Text.TextFragment title = new Aspose.Pdf.Text.TextFragment("FIRMA EXPEDIDA POR: NOMISIGN SA DE CV");
                    title.Position = new Position(100, 750);
                    title.TextState.FontSize = 8;
                    title.TextState.Font = FontRepository.FindFont("Arial");
                    title.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilder = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilder.AppendText(title);*/
                    Aspose.Pdf.Text.TextFragment title = new Aspose.Pdf.Text.TextFragment(string.Format("FIRMA DIGITAL DE {0} EN DONDE MANIFIESTA SU ACEPTACION", originalPdfDocument.Employee.FullName));
                    title.Position = new Position(100, 750);
                    title.TextState.FontSize = 8;
                    title.TextState.Font = FontRepository.FindFont("Arial");
                    title.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilder = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilder.AppendText(title);
                    Aspose.Pdf.Text.TextFragment title2 = new Aspose.Pdf.Text.TextFragment(string.Format("A LOS TERMINOS Y CONCEPTOS DEL RECIBO NUMERO {0} DE FECHA {1}", originalPdfDocument.DocumentId.ToString(), originalPdfDocument.PayperiodDate.ToString("dd/MM/yyyy")));
                    title2.Position = new Position(100, 730);
                    title2.TextState.FontSize = 8;
                    title2.TextState.Font = FontRepository.FindFont("Arial");
                    title2.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilder2 = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilder2.AppendText(title2);
                    //Employee Auotrization Name
                    Aspose.Pdf.Text.TextFragment companyTitle = new Aspose.Pdf.Text.TextFragment(string.Format("PARA: {0}", originalPdfDocument.Company.CompanyName));
                    companyTitle.Position = new Position(100, 710);
                    companyTitle.TextState.FontSize = 8;
                    companyTitle.TextState.Font = FontRepository.FindFont("Arial");
                    companyTitle.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilderCompany = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilderCompany.AppendText(companyTitle);
                    //Employee Auotrization Name
                    Aspose.Pdf.Text.TextFragment titleemp = new Aspose.Pdf.Text.TextFragment("FIRMA DE ACEPTACIÓN POR: ");
                    titleemp.Position = new Position(100, 690);
                    titleemp.TextState.FontSize = 8;
                    titleemp.TextState.Font = FontRepository.FindFont("Arial");
                    titleemp.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilderemp = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilderemp.AppendText(titleemp);

                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    TextInfo textInfo = cultureInfo.TextInfo;
                    var namet = textInfo.ToTitleCase(originalPdfDocument.Employee.FullName.ToLower());
                    Aspose.Pdf.Text.TextFragment titleempName = new Aspose.Pdf.Text.TextFragment(string.Format("{0}",
                        namet));
                    titleempName.Position = new Position(100, 660);
                    titleempName.TextState.FontSize = 16;
                    titleempName.TextState.Font = FontRepository.FindFont("Vladimir Script");
                    titleempName.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilderempName = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilderempName.AppendText(titleempName);
                    //DateTime Now
                    string today = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    string todayt = DateTime.Now.ToString("ddMMyyyyHHmmssfff");
                    //Hash Code 1P
                    string hash = originalPdfDocument.FileHash;
                    if (!string.IsNullOrEmpty(hash))
                    {
                        Aspose.Pdf.Text.TextFragment titlehash = new Aspose.Pdf.Text.TextFragment(string.Format("HASH: {0}-{1}-{2}-", 
                            hash, originalPdfDocument.Employee.EmployeeId, originalPdfDocument.DocumentId));
                        titlehash.Position = new Position(100, 640);
                        titlehash.TextState.FontSize = 8;
                        titlehash.TextState.Font = FontRepository.FindFont("Arial");
                        titlehash.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                        TextBuilder textBuilderhash = new TextBuilder(document.Pages[document.Pages.Count]);
                        textBuilderhash.AppendText(titlehash);

                        Aspose.Pdf.Text.TextFragment titlehash2 = new Aspose.Pdf.Text.TextFragment(string.Format("{0}-{1}-{2}",
                            originalPdfDocument.Employee.LastLoginDate.ToString("ddMMyyyyHHmmssfff"), todayt, originalPdfDocument.Employee.SessionToken));
                        titlehash2.Position = new Position(100, 630);
                        titlehash2.TextState.FontSize = 8;
                        titlehash2.TextState.Font = FontRepository.FindFont("Arial");
                        titlehash2.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                        TextBuilder textBuilderhash2 = new TextBuilder(document.Pages[document.Pages.Count]);
                        textBuilderhash2.AppendText(titlehash2);
                        //Hash Code 2P
                        /*if (hash.Length > 42)
                        {
                            Aspose.Pdf.Text.TextFragment titlehash2 = new Aspose.Pdf.Text.TextFragment(hash.Substring(42));
                            titlehash2.Position = new Position(100, 690);
                            titlehash2.TextState.FontSize = 8;
                            titlehash2.TextState.Font = FontRepository.FindFont("Arial");
                            titlehash2.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                            TextBuilder textBuilderhash2 = new TextBuilder(document.Pages[document.Pages.Count]);
                            textBuilderhash2.AppendText(titlehash2);
                        }*/
                    }
                    //Last login
                    Aspose.Pdf.Text.TextFragment lastLogEmp = new Aspose.Pdf.Text.TextFragment(string.Format("TOKEN DE SESIÓN: {0}", originalPdfDocument.Employee.SessionToken));
                    lastLogEmp.Position = new Position(100, 600);
                    lastLogEmp.TextState.FontSize = 8;
                    lastLogEmp.TextState.Font = FontRepository.FindFont("Arial");
                    lastLogEmp.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilderlastLog = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilderlastLog.AppendText(lastLogEmp);

                    //Timestamp
                    Aspose.Pdf.Text.TextFragment timestamp = new Aspose.Pdf.Text.TextFragment(string.Format("FECHA DE INICIO DE SESIÓN: {0}", originalPdfDocument.Employee.LastLoginDate.ToString("dd/MM/yyyy HH:mm:ss fff")));
                    timestamp.Position = new Position(100, 580);
                    timestamp.TextState.FontSize = 8;
                    timestamp.TextState.Font = FontRepository.FindFont("Arial");
                    timestamp.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilderTimestamp = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilderTimestamp.AppendText(timestamp);

                    //TOKEN
                    Aspose.Pdf.Text.TextFragment token = new Aspose.Pdf.Text.TextFragment(string.Format("FECHA DE FIRMA DEL RECIBO: {0}", today));
                    token.Position = new Position(100, 560);
                    token.TextState.FontSize = 8;
                    token.TextState.Font = FontRepository.FindFont("Arial");
                    token.TextState.FontStyle = Aspose.Pdf.Text.FontStyles.Regular;
                    TextBuilder textBuilderToken = new TextBuilder(document.Pages[document.Pages.Count]);
                    textBuilderToken.AppendText(token);

                    document.Save(temppath);
                }
                /*using (Aspose.Pdf.Document document = new Aspose.Pdf.Document(temppath))
                {
                    using (PdfFileSignature signature = new PdfFileSignature(document))
                    {
                        PKCS7 pkcs = new PKCS7(_rootDiskPath + @"\" + _pfxFileName, _pfxFilePassword); // "RQP@ssw0rd"); // Use PKCS7/PKCS7Detached objects
                        pkcs.Location = "Mexico";
                        pkcs.Reason = "Approved by: " + originalPdfDocument.Employee.FullName;
                        pkcs.ShowProperties = false;
                        DocMDPSignature docMdpSignature = new DocMDPSignature(pkcs, DocMDPAccessPermissions.AnnotationModification);
                        System.Drawing.Rectangle rect = new System.Drawing.Rectangle(90, 620, 230, 150);
                        log.Info("document.Pages.Count : " + document.Pages.Count);
                        // Create any of the three signature types
                        signature.Certify(document.Pages.Count, "Signature Reason", "Contact", "Location", true, rect, docMdpSignature);
                        // Save output PDF file
                        signature.Save(signedpath);
                        // Create backup of si gned copy to location 2 - no database record needed
                        NomiFileAccess.BackupFileToLocation2(originalPdfDocument.CompanyId, signedpath);
                    }
                }*/
                PdfFileSignature pdfSign = new PdfFileSignature();
                pdfSign.BindPdf(temppath);
                // Create a rectangle for signature location
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(90, 550, 410, 220);
                // Set signature appearance
                //pdfSign.SignatureAppearance = dataDir + "aspose-logo.jpg";
                // Create any of the three signature types
                PKCS7 pkcs = new PKCS7(_rootDiskPath + @"\" + _pfxFileName, _pfxFilePassword);
                pkcs.Location = "Mexico";
                pkcs.Reason = "Approved by: " + originalPdfDocument.Employee.FullName;
                pkcs.ShowProperties = false;

                pdfSign.Sign(docCount, "Signature Reason", "Contact", "Location", true, rect, pkcs);
                // Save output PDF file
                pdfSign.Save(signedpath);
                pdfSign.Dispose();

                // Create backup of si gned copy to location 2 - no database record needed
                NomiFileAccess.BackupFileToLocation2(originalPdfDocument.CompanyId, signedpath);

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
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(100, 95, 400, 100);
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