using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CfdiService.Model
{
    public enum SignStatus { Invalid = 0, Unsigned = 1, Signed = 2, Refused = 3 }

    public class Document
    {
        public int DocumentId { set; get; }
        public Nullable<int> BatchId { get; set; }
        public virtual Batch Batch { get; set; }
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }
        public DateTime UploadTime { get; set; }
        public DateTime PayperiodDate { get; set; }
        public string PathToFile { get; set; }
        public string FileHash { get; set; }
        public SignStatus SignStatus { get; set; }
        public string PathToSignatureFile { get; set; }  // what is this?
        public string SignatureFileHash { get; set; }
    }
}