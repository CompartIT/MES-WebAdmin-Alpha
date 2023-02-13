using System;

namespace WebAdmin
{
    /*Kanban*/
    public class BAL_MES
    {
        public enum ProcessKanbanType
        {
            Awaiting = 0,
            ScanIn = 1,
            Output = 2
        }


        public static string GetProcessKanbanSQL(string type)
        {
            var returnSQL = "";
            switch (type)
            {
                case "Awaiting":
                    returnSQL = "select left(B.PartNum, 2) as ProdFamily, B.PartNum,A.TransType,A.UserId,A.JobNum,A.OprSeq,A.OpCode,A.OpGroup,A.OprSeqNext,A.OpCodeNext,A.OprQty,A.ReportingQty,A.LaborQty,A.TransTime,S.StdCostUS,JobCost=(S.StdCostUS*A.ReportingQty),datediff(HOUR,A.TransTime,getdate()) as Aging, case when datediff(HOUR,A.TransTime,getdate())<24 then 'GroupA' when datediff(HOUR,A.TransTime,getdate())>=24 and datediff(HOUR,A.TransTime,getdate())<72 then 'GroupB' else 'GroupC' end as AgingGroup from WebTransaction A inner join WebJobHead B on a.JobNum = B.JobNum inner join Epicor_PartStdCost S on B.PartNum=S.PartNum where not exists(select 1 from WebTransaction where JobNum = A.JobNum and ID > A.id)and((A.OpCode = 'NB13' and A.transType = 'OPRRECEIPT') or(A.OpCodeNext = 'NB13' and(A.TransType ='OPREND' or A.TransType = 'BEMATERIALRECEIPT' or A.TransType = 'MATERIALRECEIPT')) ) and A.ReportingQty<>0";
                    break;
                case "ScanIn":
                    returnSQL = "select left(B.PartNum,2) as ProdFamily, B.PartNum,A.TransType,A.UserId,A.JobNum,A.OprSeq,A.OpCode,A.OpGroup,A.OprSeqNext,A.OpCodeNext,A.OprQty,A.ReportingQty,A.LaborQty,A.TransTime,S.StdCostUS,JobCost=(S.StdCostUS*A.ReportingQty),datediff(HOUR,A.TransTime,getdate()) as Aging from WebTransaction  A inner join WebJobHead B on a.JobNum=B.JobNum inner join Epicor_PartStdCost S on B.PartNum=S.PartNum where not exists(select 1 from WebTransaction where JobNum=A.JobNum and ID>A.id)and A.OpCode='NB13' and A.TransType='OPRSTART' and datediff(dd,A.TransTime,getdate())=0";
                    break;
                case "Output":
                    returnSQL = "select left(B.PartNum,2) as ProdFamily, B.PartNum,A.TransType,A.UserId,A.JobNum,A.OprSeq,A.OpCode,A.OpGroup,A.OprSeqNext,A.OpCodeNext,A.OprQty,A.ReportingQty,A.LaborQty,A.TransTime,S.StdCostUS,JobCost=(S.StdCostUS*A.ReportingQty),datediff(HOUR,A.TransTime,getdate()) as Aging  from WebTransaction  A inner join WebJobHead B on a.JobNum=B.JobNum inner join Epicor_PartStdCost S on B.PartNum=S.PartNum where A.OpCode='NB13' and A.TransType='OPREND' and ReportingQty<>0 and datediff(dd,A.TransTime,getdate())=0";
                    break;
            }

            return returnSQL;
        }

    }
}