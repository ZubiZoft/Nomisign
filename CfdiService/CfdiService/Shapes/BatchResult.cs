using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CfdiService.Shapes
{
    public enum BatchResultCode { Ok = 0,  LicenseBalance = 1, AccountStatus = 2, Cancelled = 3 }

    public class BatchResult
    {
        public int BatchId = 0;
        public int ContentCount = 0;
        public BatchResultCode ResultCode = 0;

        public BatchResult(int batchId, BatchResultCode result, int count = 0)
        {
            BatchId = batchId;
            ResultCode = result;
            ContentCount = count;
        }
    }
}