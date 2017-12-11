using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CfdiService.Model
{
    public enum SignStatus { Invalid = 0, SinFirma = 1, Firmado = 2, Rechazado = 3 }

    public class Document
    {
        public int DocumentId { set; get; }
        public Nullable<int> BatchId { get; set; }
        [ForeignKey("BatchId")]
        public virtual Batch Batch { get; set; }
        public int EmployeeId { get; set; }
        public int CompanyId { get; set; }
        public virtual Employee Employee { get; set; }
        public DateTime UploadTime { get; set; }
        public DateTime PayperiodDate { get; set; }
        public string PathToFile { get; set; }
        public string FileHash { get; set; }
        public SignStatus SignStatus { get; set; }
        public string EmployeeConcern { get; set; } 
        public int AlwaysShow { get; set; } // 0 = no, 1 = yes
    }
}