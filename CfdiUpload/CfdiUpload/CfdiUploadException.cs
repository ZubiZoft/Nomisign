using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CfdiService.Shapes;

namespace CfdiService.Upload
{
    public class CfdiUploadException : ApplicationException
    {
        private static string[] resultMsgs = { "",
            "The license balance will not allow for this upload",
            "The status of the customer account does not allow uploads",
            "The referenced batch has failed or has been cancelled"
        };

        public CfdiUploadException(string msg, Exception ex = null)
            : base(msg, ex)
        {
        }

        public CfdiUploadException(BatchResultCode result)
            : base(resultMsgs[(int)result])
        {
        }
    }
}
