using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAdmin.Models
{
    public class SimpleJobOper
    {
        public string JobNum { get; set; }
        public string AssemblySeq { get; set; }
        public string OprSeq { get; set; }
        public string OprCode { get; set; }
        public string OprDesc { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string OperQty { get; set; }
        public string LaborQty { get; set; }
        public string ScrapQty { get; set; }
        public string DiscrepQty { get; set; }
        public string WIPQty { get; set; }
        public string WIPLocation { get; set; }
        public string OprStatus { get; set; }
        public string Backflush { get; set; }
        public string JobStatus { get; set; }
        public string ErrMessage { get; set; }
        public string ClockInTime { get; set; }
        public string ClockOutTime { get; set; }
        public string UserName { get; set; }
        public string HeatCode { get; set; }
        public string PartNumNew { get; set; }
        public string DrawRev { get; set; }
        public string CusPO { get; set; }
        public string TransType { get; set; }

    }
}