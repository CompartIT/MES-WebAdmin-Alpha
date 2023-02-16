using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAdmin.Models
{
    public class SimpleEpicorJob: WebPrint
    {
        public string id { get; set; }
        public string Company { get; set; }
        public string JobNum { get; set; }
        public string ParentJobNum { get; set; }
        public string PartNum { get; set; }
        public string RevisionNum { get; set; }
        public string DrawNum { get; set; }
        public string PartDescription { get; set; }
        public string ProdQty { get; set; }
        public string IUM { get; set; }
        public string Split { get; set; }
        public string SplitPerQty { get; set; }
        public string JobClosed { get; set; }
        public string ClosedDate { get; set; }
        public string SplitDate { get; set; }
        public string SplitOpr { get; set; }
        public string CurOperSeq { get; set; }
        public string CurOperCode { get; set; }
        public string CurOperQty { get; set; }
        public string CurLocation { get; set; }
        public string CurOprStatus { get; set; }
        public string CurOpGroup { get; set; }
        public string CurOpTransfer { get; set; }
        public string OpDesc { get; set; }
        public string DiscrepReason { get; set;}
        public string JobType { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDateTime { get; set; }
        public string HeatCode { get; set; }
    }
}