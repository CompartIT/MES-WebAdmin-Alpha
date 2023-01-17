using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAdmin.Models
{
    public class SimpleMyEWI 
    {
        public string ListType { get; set; }
        public string Qty { get; set; }
        public string QtyNormal { get; set; }
        public string QtyFA { get; set; }
        public string QtyTemp { get; set; }
        public string QtyPercent { get; set; }
        public string QtyPercentShow { get; set; }
        public string IsToDo { get; set; }
        public string RowIndex { get; set; }
        public string DetailIndex { get; set; }
    }

    public class SimpleEWI
    {
        public string ID { get; set; }
        public string ControlPlanNum { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string CustomerPartNum { get; set; }
        public string OriginDate { get; set; }
        public string KeyContact { get; set; }
        public string CoreTeam { get; set; }
        public string LatestDate { get; set; }
        public string LatestRev { get; set; }
        public string CreateBy { get; set; }
        public string CreateDate { get; set; }
        public string StatusDesc { get; set; }
        public string StatusLog { get; set; }
        public string RevLog { get; set; }
    }

    public class SimplePart {
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string CustomerPartNum { get; set; }
    }

    public class SimpleEWIMain
    {
        public string ID { get; set; }
        public string MainID { get; set; }
        public string ControlPlanNum { get; set; }
        public string ProductStage { get; set; }
        public string KeyContact { get; set; }
        public string OriginDate { get; set; }
        public string LatestDate { get; set; }
        public string LatestRev { get; set; }
        public string CurrentRev { get; set; }
        public string CurrentRevActive { get; set; }
        public string CustomerPartNum { get; set; }
        public string CustomerRev { get; set; }
        public string PartNum { get; set; }
        public string CoreTeam { get; set; }
        public string CustomerEngApprovalDate { get; set; }
        public string PartDesc { get; set; }
        public string SupplierApprovalDate { get; set; }
        public string CustomerQualityApprovalDate { get; set; }
        public string Supplier { get; set; }
        public string SupplierCode { get; set; }
        public string OtherApprovalDate { get; set; }
        public string OtherApprovalDate2 { get; set; }
        public string ReviewBy { get; set; }
        public string PCN_ECN_NO { get; set; }
        public string Description { get; set; }
        public string RevisionAndAltMethod { get; set; }
        public string ApprovalStatus { get; set; }
        public string EditStatus { get; set; }
        public string JobNum { get; set; }
        public string NSType { get; set; }
        public string WIType { get; set; }
    }

    public class SimpleEWIECN
    {
        public string ID { get; set; }
        public string RevisionID { get; set; }
        public string ECNNo { get; set; }
        public string ECRNo { get; set; }
        public string Title { get; set; }
        public string ChangeReason { get; set; }
        public string ChangeFrom { get; set; }
        public string ChangeTo { get; set; }
        public string Remark { get; set; }
        public string RelateDept { get; set; }
    }

    public class SimpleNewRevision
    {
        public string LatestRev { get; set; }
        public string CustomerRev { get; set; }
    }

    public class SimpleStatus
    {
        public string StatusDesc { get; set; }
       
        public string StatusLog { get; set; }
        public string IsFinish { get; set; }
    }

    public class SimpleEWIRevision {
        public string ID { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string CustomerPartNum { get; set; }
        public string CustomerRev { get; set; }
        public string Revision { get; set; }
        public string EffectiveDate { get; set; }
        public string PCN_ECN_NO { get; set; }
        public string Iniatior { get; set; }
        public string Description { get; set; }
        public string StatusDesc { get; set; }
        public string StatusLog { get; set; }
        public string ApprovalStatus { get; set; }
        public string EditStatus { get; set; }
        public string EWIFileName { get; set; }
        public string CustomerFileName { get; set; }
        public string CADFileName { get; set; }
    }

    public class SimpleEWIOP { 
        public string OpCode { get; set; }
        public string OpCodeDesc { get; set; }
        public string OprSeq { get; set; }
    }

    public class SimpleEWIBasic {
        public string ID { get; set; }
        public string OpCode { get; set; }
        public string Machine { get; set; }
        public string OpDesc { get; set; }
        public string AttentionItem { get; set; }
        public string CycleTime { get; set; }
        public string FixtureNo { get; set; }
        public string FixtureFilePath { get; set; }
        public string Definition { get; set; }
        public string ResponsePlan { get; set; }
        public string WICode { get; set; }
    }

    public class SimpleEWICriteria {
        public string ID { get; set; }
        public string CriteriaSeq { get; set; }
        public string Criteria { get; set; }
        public string Figure { get; set; }
        public string Tolerance { get; set; }
        public string TestFrequency { get; set; }
        public string TestInstrument { get; set; }
        public string ChangeColumn { get; set; }
    }

    public class SimpleEWIMedia
    {
        public string ID { get; set; }
        public string File { get; set; }
        public string Desc { get; set; }
    }

    public class SimpleEWICheck
    { 
        public string ID { get; set; }
        public string PartNum { get; set; }
        public string PartDesc { get; set; }
        public string Revision { get; set; }
        public string Description { get; set; }
        public string Iniatior { get; set; }
        public string CreateDate { get; set; }
        public string CanCheck { get; set; }
        public string StatusDesc { get; set; }
        public string StatusLog { get; set; }
        public string WIType { get; set; }
    }

    public class SimpleCurRight { 
        /// <summary>
        /// 当前审批权限
        /// </summary>
        public string CheckRight { get; set; }
        /// <summary>
        /// 当前菜单
        /// </summary>
        public string Menu { get; set; }
    }

}