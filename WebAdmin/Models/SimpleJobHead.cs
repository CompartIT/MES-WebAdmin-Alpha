using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAdmin.Models
{
    public class SimpleJobHead
    {
        public string ID { get; set; }
        public string JobNum { get; set; }
        public string PartNum { get; set; }
        public string Revision { get; set; }
        public string JobType { get; set; }
        public string PrintCount { get; set; }

    }
}