using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAdmin.Models
{
    public class SimpleBasicData
    {
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string CustomerPartNum { get; set; }
        public string Revision { get; set; }
        public string AltMethod { get; set; }
        public string OPList { get; set; }
    }

    public class SimpleTestInstrument {
        public string TestInstrument { get; set; }
        public string TestInstrument_EN { get; set; }
        public string Category { get; set; }
    }

    public class SimpleMachine
    {
        public string MachineName { get; set; }
        public string MachineType { get; set; }
    }
}