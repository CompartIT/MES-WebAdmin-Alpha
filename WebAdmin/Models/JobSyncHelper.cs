using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Data;

namespace WebAdmin.Models
{
    public class JobSyncHelper
    {
        private static JobSyncHelper _instance = null;
        private static readonly object obj = new object();

        public static JobSyncHelper CreateInstance()
        {
            if (_instance == null)
            {
                lock (obj)
                {
                    if (_instance == null)
                    {
                        _instance = new JobSyncHelper();
                    }
                }
            }
            return _instance;
        }

        private JobSyncHelper()
        {

        }

        public void SyncEpicorJob()
        {
            string cmdAddJob = string.Empty;
            string cmdAddOper = string.Empty;
            string cmdSync = string.Empty;
            string currJob = string.Empty;
            string Company = ConfigurationManager.AppSettings["CompanyCode"];

            lock (obj)
            {
                LogHelper.Info("Job Sync Start");
                try
                {
                    //从Epicor中获取工单信息，存入EpicorJobHead和EpicorJobOper

                    //-----Updated On 4/16/2021
                    //Use column number02 as the filed for Job QTY + Setup scrap and adjustment for PMC to input, and this will repalce ProdQty as will need the new QTY for printing the lot traveller
                    //
                    string cmdText = string.Format(@"
                            select
	                            JH.JobNum as JobNum,
	                            JH.PartNum as PartNum,
	                            JH.RevisionNum as RevisionNum,
	                            JH.DrawNum as DrawNum,
	                            JH.PartDescription as PartDescription,
                                JH.number02 as ProdQty,
	                            JH.ProdQty as EpicorProdQty,
	                            JH.IUM as IUM,
	                            JO.AssemblySeq as AssemblySeq,
	                            JO.OprSeq as OprSeq,
	                            OM.OpCode as OpCode,
	                            JO.OpDesc as OpDesc,
	                            0 as SplitPerQty,
	                            0 as Backflush,
                                JH.ShortChar01 as ShortChar01,
                                JO.SubContract as SubContract,
                                JH.Checkbox01 as IsSpecMtl, JH.Character09 VendorId
                            from JobHead as JH
                            left join erp.JobOper as JO on JH.Company = JO.Company and JH.JobNum = JO.JobNum
                            left join erp.OpMaster as OM on JO.OpCode = OM.OpCode and JH.Company = OM.Company
                            where JH.Company = '{0}' and JobClosed = 0 and WebSync_c = 0 and exists(select * from erp.JobMtl as JM where JH.JobNum = JM.JobNum and JM.IssuedQty > 0) and JO.OpCode <> 'CU01'
                                and ('{0}' != '19268F'
                                        or ('{0}' = '19268F'
                                                and exists(select 1 from erp.part x where JH.PartNum = x.PartNum and JH.Company = x.Company and x.ClassID like '2%')
                                            )
                                    )
                            order by JH.JobNum,JO.OprSeq
                        ",
                        Company,
                        ConfigurationManager.AppSettings["SkipOperList"]
                    );

                   DataTableCollection dtcEpicJob = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);
                    LogHelper.Info("ok");
                    int syncCount = 0;

                    foreach (DataRow row in dtcEpicJob[0].Rows)
                    {
                        if (!currJob.Equals(row["JobNum"]))
                        {
                            currJob = row["JobNum"].ToString();

                            string cmdTextHeatcode = string.Format(@"
                                 SELECT TOP(1) 
                                    JOH.ShortChar01 AS IsAssemblyJob,
                                    JOH.Character10 as CustomerVersion,
                                    JOMT.HeatCode, JOMT.GRRNO,
                                    JOMT.MaterialDesc,
                                    case when JOH.Character04 = '' then JOMT.Specification else JOH.Character04 end Specification,
                                    FGPart.ShortChar01 AS DrawNum,
                                    FGPart.Number01 as SplitQty,
                                    PartPC.XRefPartNum as CustomerPartNumber,
                                    left(FGPart.ClassID,1) Division
                                FROM JobHead AS JOH
                                INNER JOIN Part AS FGPart ON JOH.Company = FGPart.Company AND JOH.PartNum = FGPart.PartNum
                                INNER JOIN 
	                                (
		                                SELECT JOM.MtlSeq, JOM.JobNum,
                                            Case when isnull(joh.shortchar03,'')<>'' then JOH.shortchar03 
                                            else 
                                                case when '{1}' = '19268F' then 
                                                   case when charindex('[', Pl.HeatCode) > 0 then substring(Pl.HeatCode, 1, charindex('[', Pl.HeatCode)-1) else Pl.HeatCode end
                                                else LEFT(REPLACE(ISNULL(ITN.LotNum,'NNNN'),' ',''),4) end 
                                            end AS HeatCode, Pl.GRRNO,
                                            PH.PartDescription AS MaterialDesc, PH.Specification FROM Erp.JobMtl AS JOM
		                                INNER JOIN JobHead AS JOH ON JOH.JobNum = JOM.JobNum
		                                INNER Join Jobmtl AS JOMTL ON JOMTL.JobNum = JOH.JobNum  AND JOMTL.Company = JOH.Company  and   JOMTL.IssuedQty >0 
		                                INNER JOIN Part AS PMTL ON JOMTL.Company =PMTL.Company AND JOMTL.PartNum = PMTL.PartNum   and  PMTL.ClassId  in('FG','SFG','DM','1001','1002','1003','1004','2001','2002','2003','2004')
		                                INNER JOIN Part AS PH ON JOMTL.Company = PH.Company AND JOMTL.PartNum = PH.PartNum
		                                LEFT JOIN Erp.PartTran AS ITN ON ITN.JobNum = JOM.JobNum AND JOM.PartNum = ITN.PartNum  and ITN.TranType='STK-MTL'
                                        outer apply(select top 1 case when len(ltrim(x.Character01))=0 then x.lotnum else x.Character01 end HeatCode,
                                                        x.PartLotDescription GRRNO
                                                    from PartLot x 
                                                    where ITN.Company = x.Company and ITN.PartNum = x.PartNum and ITN.LotNum = x.LotNum
                                                        and x.Company = '19268F'
                                                    )  Pl
		                                WHERE  
		                                JOM.JobNum='{0}'
	                                ) AS JOMT ON JOMT.JobNum = JOH.JobNum
                                LEFT JOIN Erp.partxrefint  AS PartPC ON FGPart.Company = PartPC.Company AND FGPart.PartNum = PartPC.PartNum  LEFT JOIN erp.jobProd AS MTO ON MTO.JobNum = JOH.JobNum AND MTO.Company = JOH.Company
                                LEFT JOIN OrderHed AS SOH ON SOH.OrderNum = MTO.OrderNum AND SOH.Company = MTO.Company
                                WHERE  JOH.JobNum = '{0}'
                                ORDER BY JOH.JobNum, CASE WHEN LEFT(JOMT.HeatCode,4) IN ('NNNN','0000') THEN 999 ELSE 0 END, JOMT.HeatCode DESC
                                ",
                                currJob,
                                Company
                            );
                            DataTableCollection dtcEpicJobHeatCode = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdTextHeatcode, null);

                            string heatCode = string.Empty;
                            string GRRNO = string.Empty;
                            string materialId = string.Empty;
                            string partDesc = string.Empty;
                            string customVersion = string.Empty;
                            string splitQty = string.Empty;
                            string drawNum = string.Empty;
                            string customerPartNumber = string.Empty;
                            string Division = string.Empty;

                            if (dtcEpicJobHeatCode != null && dtcEpicJobHeatCode[0].Rows.Count > 0)
                            {
                                DataRow rowHeatCode = dtcEpicJobHeatCode[0].Rows[0];
                                heatCode = rowHeatCode["HeatCode"].ToString();
                                GRRNO = rowHeatCode["GRRNO"].ToString();
                                partDesc  = rowHeatCode["MaterialDesc"].ToString();
                                materialId = rowHeatCode["Specification"].ToString();
                                splitQty = rowHeatCode["SplitQty"].ToString();
                                drawNum = rowHeatCode["DrawNum"].ToString();
                                customVersion = rowHeatCode["CustomerVersion"].ToString();
                                customerPartNumber = rowHeatCode["CustomerPartNumber"].ToString();
                                Division = rowHeatCode["Division"].ToString();
                                //XRefPartNum : 新建一个字段CustomerPartNumber
                                // 

                            }

                            cmdAddJob = string.Format(@"
                                if not exists(select 1 from EpicorJobHead where Company = '{0}' and JobNum = '{1}')
                                    insert into EpicorJobHead(Company,JobNum,PartNum,RevisionNum,DrawNum,PartDescription,ProdQty,EpicorProdQty,IUM,SplitPerQty,ShortChar01,HeatCode,MaterialId,PartDesc,CustomVersion,CustomerPartNum,Division,IsSpecMtl,VendorId,GRRNO)
                                    values('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}',{17},'{18}','{19}')
                                ",
                                ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                                row["JobNum"],                                      //1:JobNum
                                row["PartNum"],                                     //2:PartNum
                                row["RevisionNum"],                                 //3:RevisionNum
                                drawNum,                                            //4:DrawNum
                                row["PartDescription"].ToString().Replace("'","‘"), //5:PartDescription
                                decimal.Parse(row["ProdQty"].ToString())<=0? row["EpicorProdQty"]: row["ProdQty"],    //6:ProdQty
                                row["EpicorProdQty"],                               //7:Epicor ProdQty
                                row["IUM"],                                         //8:IUM
                                splitQty.Replace("0.00","0"),                                           //9:SplitPerQty
                                row["ShortChar01"],                                 //10:ShortChar01
                                heatCode,                                           //11:HeatCode
                                materialId,                                         //12:Material ID
                                partDesc.ToString().Replace("'", "‘"),             //13:Material Description
                                customVersion,                                      //14:客户PO号
                                customerPartNumber,                                 //15:客户物料代码
                                Division,                                            //16.Division
                                Convert.ToBoolean(row["IsSpecMtl"]) == true ? 1 : 0,                   //17:IsSpecMtl
                                row["VendorId"],                                     //18:VendorId
                                GRRNO                                                //19:GRRNO
                            );
                            LogHelper.Info(cmdAddJob);
                            SqlHelper.ExecteNonQueryText(cmdAddJob, null);
                            syncCount++;

                            LogHelper.Info("insert end");

                            cmdSync = string.Format(@"
                                    update erp.JobHead_UD
                                    set WebSync_c = 1
                                    where ForeignSysRowID in (
	                                    select SysRowID
	                                    from erp.JobHead
	                                    where Company = '{0}' and JobNum = '{1}'
                                    ) 
                                ",
                                ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                                row["JobNum"]                                       //1:JobNum
                            );
                            SqlHelper.ExecteNonQuery(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdSync, null);
                        }

                        cmdAddOper = string.Format(@"
                            if not exists(select 1 from EpicorJobOper where Company = '{0}' and JobNum = '{1}' and OpCode = '{4}')
                                insert into EpicorJobOper(Company,JobNum,AssemblySeq,OprSeq,OpCode,OpDesc,Backflush,SubContract)
                                values('{0}','{1}',{2},{3},'{4}','{5}',{6},'{7}')
                            ",
                            ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                            row["JobNum"],                                      //1:JobNum
                            row["AssemblySeq"],                                 //2:AssemblySeq
                            row["OprSeq"],                                      //3:OprSeq
                            row["OpCode"],                                      //4:OpCode
                            row["OpDesc"].ToString().Replace("'", "‘"),         //5:OpDesc
                            row["Backflush"],                                   //6:Backflush
                            row["SubContract"]                                  //7:SubContract
                        );
                        SqlHelper.ExecteNonQueryText(cmdAddOper, null);
                        
                    }

                    string cmdTextClean = string.Format(@"
                            delete 
                            from EpicorJobOper
                            where not exists(
	                            select * 
	                            from EpicorJobHead
	                            where JobNum = EpicorJobOper.JobNum
                            )
                        "
                    );
                    SqlHelper.ExecteNonQueryText(cmdTextClean, null);

                    LogHelper.Info(string.Format("Sync {0} Jobs", syncCount));

                }
                catch (Exception ex)
                {
                    LogHelper.Error("Sync Epicor Job Error", ex);
                    LogHelper.Info(cmdAddJob);
                    LogHelper.Info(cmdSync);
                    LogHelper.Info(cmdAddOper);

                }


                LogHelper.Info("Job Sync End");
            }
           
        }
    }
}