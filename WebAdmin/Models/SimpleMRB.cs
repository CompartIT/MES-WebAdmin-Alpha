using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAdmin.Models
{
    public class SimpleDiscrepHeader:WebPrint
    {
        public string id { get; set; }
        public string JobNum { get; set; }
        public string OprSeq { get; set; }
        public string Quantity { get; set; }
        public string ReasonCode { get; set; }
        public string ReportId { get; set; }
        public string ReportPDAId { get; set; }
        public string ReportTime { get; set; }
        public string ReceiptId { get; set; }
        public string ReceiptPDAId { get; set; }
        public string ReceiptTime { get; set; }
        public string ProcessId { get; set; }
        public string ProcessTime { get; set; }
        public string Status { get; set; }
        public string OpCode { get; set; }
        public string OprDesc { get; set; }
        public string ReasonDesc { get; set; }
        //public string PrintDateTime { get; set; }
        //public string PrintUserId { get; set; }
        public string VendorNum { get; set; }
        public string VendorName { get; set; }
    }

    public class SimpleDiscrepReason { 
        public string MRBReasonCode { get; set; }
        public string MRBReasonDesc { get; set; }
    }
}