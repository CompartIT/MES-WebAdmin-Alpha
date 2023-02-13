using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace WebAdmin.Models
{
    /*Kanban*/
    public class ProcessTimeComparisionEntity
    {
        public string JobNum { get; set; }
        public string PartNum { get; set; }
        public string UserID { get; set; }
        public string MachineID { get; set; }
        public string PDAID { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string OpCode { get; set; }
        public int OprSeq { get; set; }
        public string OpGroup { get; set; }
        public decimal StandardOprTime { get; set; }
        public decimal ActualOprTime { get; set; }
        public decimal Variance { get; set; }
        public decimal Percentage { get; set; }
        public decimal ABSPercentageVariance { get; set; }
    }

    public class ShopFloorKanbanEntity
    {
        public string TransType { get; set; }
        public string JobNum { get; set; }
        public string PartNum { get; set; }
        public string UserID { get; set; }
        public string MachineID { get; set; }
        public string Location { get; set; }
        public string PDAID { get; set; }
        public string OpCode { get; set; }
        public string OpDesc { get; set; }
        public string OpGroup { get; set; }
        public int ReportingQTY { get; set; }
        public int LaborQTY { get; set; }
        public int ScrapQTY { get; set; }
        public int DiscrepQTY { get; set; }
        public string TransTime { get; set; }
    }

    public class ShopFloorLocationEntity
    {
        public string Location { get; set; }
    }

    public class ShopFloorGroupedKanbanEntity
    {
        public string LocationGroup { get; set; }
        public int LocationOrder { get; set; }
        public IEnumerable<ShopFloorKanbanEntity> ShopFloorKanbanList { get; set; }

    }

    public class ShopFloorMachineList
    {
        public string LocationGroup { get; set; }
        public int LocationGroupCount { get; set; }
        public IEnumerable<ShopFloorMachineType> MachineType { get; set; }

        public IEnumerable<ShopFloorMachine> MachineList { get; set; }

    }

    public class ShopFloorMachine
    {
        public string MachineID { get; set; }
    }


    public class ShopFloorMachineType
    {
        public string MachineType { get; set; }
        public int MachineCount { get; set; }
    }

    public class ShopFloorMachineTypeList
    {
        public string MachineType { get; set; }
        public string MachineID { get; set; }
    }

    /// <summary>
    /// FA Machine List
    /// </summary>
    public class ShopFloorFAMachineList
    {
        public string MachineID { get; set; }
    }
    //WIP
    public class WIPEntity
    {
        public string Jtype { get; set; }
        public string TransType { get; set; }
        public string MONo { get; set; }
        public string ProductFamily { get; set; }
        public string ItemCode { get; set; }
        public string HeatCode { get; set; }
        public string LotNo { get; set; }
        public string OPCode { get; set; }
        public string OPDesc { get; set; }
        public string OPCodeNext { get; set; }
        public string OPDescNext { get; set; }
        public string WDate { get; set; }
        public decimal WIPQTY { get; set; }
        public string UserId { get; set; }
    }

    //Input to BE
    public class InputToBEEntity
    {
        public string Site { get; set; }
        public string JobNum { get; set; }
        public string FromOPCode { get; set; }
        public string ToOPCode { get; set; }
        public string TransType { get; set; }
        public int TotalQty { get; set; }
        public int Quantity { get; set; }
        public string StartDatetime { get; set; }
        public string UserID { get; set; }
        public string Product { get; set; }
        public string Job { get; set; }
    }

    //MRB Rework
    public class MRBReworkEEntity
    {
        public string JType { get; set; }
        public string Transtype { get; set; }
        public string EpicorJobNum { get; set; }
        public string ProductFamily { get; set; }
        public string ItemCode { get; set; }
        public string LotNo { get; set; }
        public string OpCode { get; set; }
        public string WDate { get; set; }
        public string ProdQty { get; set; }
        public string PrintUserId { get; set; }
        public string PrintDateTime { get; set; }
        public string TransTime { get; set; }
        public string Site { get; set; }
    }

    //Process Kanban
    public class ProcessKanbanEntity
    {
        public string ProductFamily { get; set; }
        public int SumQTY { get; set; }
        public decimal SumJobCost { get; set; }
        public int JobCount { get; set; }
        public IEnumerable<ProcessKanbanDetailEntity> ProcessKanbanDetailList { get; set; }
    }


    //Process Kanban -- Awaiting, showing the groups
    public class ProcessKanbanAwaitingEntity
    {
        public int SumQTY { get; set; }
        public decimal SumJobCost { get; set; }
        public int JobCount { get; set; }

        public IEnumerable<ProcessKanbanEntity> ProcessKanbanGroupA { get; set; }
        public IEnumerable<ProcessKanbanEntity> ProcessKanbanGroupB { get; set; }
        public IEnumerable<ProcessKanbanEntity> ProcessKanbanGroupC { get; set; }
    }


    //Process Kanban details
    public class ProcessKanbanDetailEntity
    {
        public string ProductFamily { get; set; }
        public string PartNum { get; set; }
        public string TransType { get; set; }
        public string JobNum { get; set; }
        public string OpCode { get; set; }
        public string OpCodeNext { get; set; }
        public int OprQty { get; set; }
        public int ReportingQty { get; set; }
        public int LaborQty { get; set; }
        public decimal StdCostUS { get; set; }
        public decimal JobCost { get; set; }
        public string TransTime { get; set; }
        public int Aging { get; set; } //Aging hours
    }

    public class MESReportDataSources
    {
        public string DataSetName { get; set; }
        public DataTable DataTable { get; set; }
    }
}