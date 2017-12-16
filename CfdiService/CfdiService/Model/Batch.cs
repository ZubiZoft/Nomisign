using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace CfdiService.Model
{
    public enum BatchStatus { Invalid = 0, Open = 1, Completed = 2, Cancelled = 3, Abandoned = 4 }

    public class Batch
    {
        public Batch()
        {
            Documents = new HashSet<Document>();
        }
        public int BatchId { set; get; }
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public DateTime BatchOpenTime { get; set; }
        public DateTime BatchCloseTime { get; set; }
        public int ItemCount { get; set; }
        public int ActualItemCount { get; set; }
        public string WorkDirectory { get; set; }
        public string ApiKey { get; set; }
        public BatchStatus BatchStatus { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
    }
}