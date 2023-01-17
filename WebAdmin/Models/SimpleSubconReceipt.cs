using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAdmin.Models
{
    public class SimpleSubconReceipt:WebPrint
    {
        public string id { get; set; }
        public string Company { get; set; }
        public string JobNum { get; set; }
        public string OprSeq { get; set; }
        public string OpCode { get; set; }
        public string OprDesc { get; set; }
        public string PackNum { get; set; }
        public string PartNum { get; set; }
        public string PONum { get; set; }
        public string POLine { get; set; }
        public string PORelNum { get; set; }
        public string VendorNum { get; set; }
        public string VendorName { get; set; }
        public string Qty { get; set; }
        public string EntryPerson { get; set; }
        public string EntryTime { get; set; }
        public string Sync { get; set; }
    }
}