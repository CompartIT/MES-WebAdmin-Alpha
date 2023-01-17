using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using WebAdmin.Models;
using System.Configuration;

namespace WebAdmin.Controllers
{
    public class JobReturnController : BaseController
    {
        public JsonResult GetOpList(string jobnum)
        {
            string cmdText = string.Empty;
            DataTableCollection dtc;
            List<SimpleOpMaster> list = new List<SimpleOpMaster>();

            cmdText = string.Format(@"
                    select *
                    from erp.OpMaster
                    where Company = '{0}' 
                    order by opcode
                ",
                ConfigurationManager.AppSettings["CompanyCode"]
            );
            dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

            foreach(DataRow row in dtc[0].Rows)
            {
                SimpleOpMaster op = new SimpleOpMaster();

                op.OpCode = row["OpCode"].ToString();
                op.OpDesc = row["OpDesc"].ToString();

                list.Add(op);

            }

            return Json(list);

        }
        public ActionResult JobReturnView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("JobReturnView"))
            {
                return Redirect("/Login/Home");
            }

            string cmdText = string.Empty;
            DataTableCollection dtcWebJob;
            string filter = string.Empty;

            int recordsTotal = 0;
            cmdText = string.Format(@"
                            
                            select *
                            from erp.OpMaster
                            where Company = '{0}' 
                            order by opcode
                        ",
                    ConfigurationManager.AppSettings["CompanyCode"]
                );

            dtcWebJob = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

            List<SimpleOpMaster> list = new List<SimpleOpMaster>();
            foreach (DataRow row in dtcWebJob[0].Rows)
            {
                SimpleOpMaster op = new SimpleOpMaster();
                op.OpCode = row["OpCode"].ToString() + ":" + row["OpDesc"].ToString();
                op.OpDesc = row["OpDesc"].ToString();
                list.Add(op);
            }

            ViewBag.opList = list;
            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            ViewBag.UserID = user.UserName;
            return View();
        }

        public ActionResult JobReturnAdjustView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("JobReturnAdjust"))
            {
                return Redirect("/Login/Home");
            }

            string cmdText = string.Empty;
            DataTableCollection dtcWebJob;
            string filter = string.Empty;

            int recordsTotal = 0;
            cmdText = string.Format(@"
                            
                            select *
                            from erp.OpMaster
                            where Company = '{0}' 
                            order by opcode
                        ",
                    ConfigurationManager.AppSettings["CompanyCode"]
                );

            dtcWebJob = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

            List<SimpleOpMaster> list = new List<SimpleOpMaster>();
            foreach (DataRow row in dtcWebJob[0].Rows)
            {
                SimpleOpMaster op = new SimpleOpMaster();
                op.OpCode = row["OpCode"].ToString() + ":" + row["OpDesc"].ToString();
                op.OpDesc = row["OpDesc"].ToString();
                list.Add(op);
            }

            ViewBag.opList = list;
            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            ViewBag.UserID = user.UserName;
            return View();
        }

        public ActionResult ReworkCenterView(string JobType)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("ReworkCenter"))
            {
                return Redirect("/Login/Home");
            }

            ViewBag.JobType = JobType;
            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            ViewBag.UserID = user.UserName;
            return View();
        }

        public JsonResult GetReturnJobList(DataTablesParameters param)
        {
            string cmdText = string.Empty;
            DataTableCollection dtc;
            string filter = string.Empty;
            
            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                filter = string.Format(" and (ParentJobNum like '%{0}%' or EpicorJobNum like '%{0}%' or PartNum like '%{0}%' or PartDescription like '%{0}%') ", param.Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and ParentJobNum like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and PartNum like '%{0}%' ", param.Columns[1].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and PartDescription like '%{0}%' ", param.Columns[1].Search.Value);
            }

            cmdText = string.Format(@"
                    select count(*) as recordsTotal
                    from WebReworkJobHead
                    where Company = '{0}' and  Split = 0 {1}
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter
            );
            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtc[0].Rows[0]["recordsTotal"].ToString());

            cmdText = string.Format(@"
                        select top {1} *
                        from (
                            select ROW_NUMBER() over(order by {3} {4}) as rownum,*
                            from (
                                select RJ.*,JO.OpCode,JO.OpDesc,DH.ReasonDesc
                                from WebReworkJobHead as RJ
                                left join WebJobOper as JO on RJ.Company = JO.Company and RJ.ParentJobNum = JO.JobNum and RJ.DiscrepSeq = JO.OprSeq
                                left join WebDiscrepHeader as DH on DH.id = RJ.DiscrepId
                            ) as WebReworkJobHead
                            where Company = '{0}' and  Split = 0 {5}
                        ) as RJ
                        where rownum > {2} and  RJ.Company = '{0}' and RJ.Split = 0 {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );

            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleEpicorJob> jobList = new List<SimpleEpicorJob>();
            foreach (DataRow row in dtc[0].Rows)
            {
                SimpleEpicorJob job = new SimpleEpicorJob();
                job.id = row["id"].ToString();
                job.Company = row["Company"].ToString();
                job.ParentJobNum = row["ParentJobNum"].ToString();
                job.PartNum = row["PartNum"].ToString();
                job.RevisionNum = row["RevisionNum"].ToString();
                job.DrawNum = row["DrawNum"].ToString();
                job.PartDescription = row["PartDescription"].ToString();
                job.ProdQty = row["ProdQty"].ToString();
                job.IUM = row["IUM"].ToString();
                job.SplitOpr = row["DiscrepSeq"].ToString();
                job.CurOperCode = row["DiscrepSeq"].ToString();
                job.OpDesc = row["OpDesc"].ToString();
                job.DiscrepReason = row["ReasonDesc"].ToString();

                jobList.Add(job);

            }

            DataTablesResult<SimpleEpicorJob> result = new DataTablesResult<SimpleEpicorJob>(param.Draw, recordsTotal, recordsTotal, jobList);
            return Json(result);
        }

        public JsonResult GetReturnJobAdjustList(DataTablesParameters param)
        {
            string cmdText = string.Empty;
            DataTableCollection dtc;
            string filter = string.Empty;
            string orderBy = param.OrderBy;
            int recordsTotal = 0;
            List<SimpleEpicorJob> jobList = new List<SimpleEpicorJob>();

            try
            {
                if (param.OrderBy.Equals("JobNum"))
                {
                    orderBy = "ReJobNum";
                }

                if (!string.IsNullOrEmpty(param.Search.Value))
                {
                    filter = string.Format(" and (ParentJobNum like '%{0}%' or EpicorJobNum like '%{0}%' or ReJobNum like '%{0}%' or PartNum like '%{0}%' or PartDescription like '%{0}%') ", param.Search.Value);
                }
                else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
                {
                    filter = string.Format(" and (ParentJobNum like '%{0}%' or ReJobNum like '%{0}%')", param.Columns[0].Search.Value);
                }
                else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
                {
                    filter = string.Format(" and PartNum like '%{0}%' ", param.Columns[1].Search.Value);
                }
                else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
                {
                    filter = string.Format(" and PartDescription like '%{0}%' ", param.Columns[1].Search.Value);
                }

                cmdText = string.Format(@"
                    select count(*) as recordsTotal
                    from WebReworkJobHead
                    where Company = '{0}' and  Split = 1 {1}
                ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    filter
                );
                dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                LogHelper.Info(cmdText);

                recordsTotal = int.Parse(dtc[0].Rows[0]["recordsTotal"].ToString());

                cmdText = string.Format(@"
                        select *
                        from (
                            select ROW_NUMBER() over(order by {3} {4}) as rownum,*
                            from (
                                select RJ.*,JO.OpCode,JO.OpDesc,DH.ReasonDesc
                                from WebReworkJobHead as RJ
                                left join WebJobOper as JO on RJ.Company = JO.Company and RJ.ParentJobNum = JO.JobNum and RJ.DiscrepSeq = JO.OprSeq
                                left join WebDiscrepHeader as DH on DH.id = RJ.DiscrepId
                            ) as WebReworkJobHead
                            where Company = '{0}' and  Split = 1 {5}
                        ) as RJ
                        where rownum > {2} and rownum <= {1} + {2} and  RJ.Company = '{0}' and RJ.Split = 1 {5}
                    ",
                        ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                        param.Length,                                       //{1}   PageSize
                        param.Start,                                        //{2}   Start
                        orderBy,                                      //{3}   OrderBy Column
                        param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                        filter                                              //{5}   filter
                    );

                dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                LogHelper.Info(cmdText);

                foreach (DataRow row in dtc[0].Rows)
                {
                    SimpleEpicorJob job = new SimpleEpicorJob();
                    job.id = row["id"].ToString();
                    job.Company = row["Company"].ToString();
                    job.JobNum = row["ReJobNum"].ToString();
                    job.ParentJobNum = row["ParentJobNum"].ToString();
                    job.PartNum = row["PartNum"].ToString();
                    job.RevisionNum = row["RevisionNum"].ToString();
                    job.DrawNum = row["DrawNum"].ToString();
                    job.PartDescription = row["PartDescription"].ToString();
                    job.ProdQty = row["ProdQty"].ToString();
                    job.IUM = row["IUM"].ToString();
                    job.SplitOpr = row["DiscrepSeq"].ToString();
                    job.CurOperCode = row["DiscrepSeq"].ToString();
                    job.OpDesc = row["OpDesc"].ToString();
                    job.DiscrepReason = row["ReasonDesc"].ToString();

                    jobList.Add(job);

                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.ToString());
            }

            DataTablesResult<SimpleEpicorJob> result = new DataTablesResult<SimpleEpicorJob>(param.Draw, recordsTotal, recordsTotal, jobList);
            return Json(result);
        }

        public JsonResult GetEpicorOPList(DataTablesParameters param)
        {
            string cmdText = string.Empty;
            DataTableCollection dtcWebJob;
            string filter = string.Empty;

            int recordsTotal = 0;
            cmdText = string.Format(@"
                            
                            select *
                            from erp.OpMaster
                            where Company = '{0}' 
                            order by opcode
                        ",
                    ConfigurationManager.AppSettings["CompanyCode"]
                );

            dtcWebJob = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

            List<SimpleOpMaster> list = new List<SimpleOpMaster>();
            foreach (DataRow row in dtcWebJob[0].Rows)
            {
                SimpleOpMaster op = new SimpleOpMaster();
                op.OpCode = row["OpCode"].ToString() + ":" + row["OpDesc"].ToString();
                op.OpDesc = row["OpDesc"].ToString();
                list.Add(op);
            }

            DataTablesResult<SimpleOpMaster> result = new DataTablesResult<SimpleOpMaster>(param.Draw, recordsTotal, recordsTotal, list);
            return Json(result);
        }

        public JsonResult CreateReworkJob(string id,string jobNum,List<string> opers)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string cmdText = string.Empty;
            DataTableCollection dtc;

            string jobNumRE = string.Empty;
            string epicorJobNum = string.Empty;
            string firstWebJobNum = string.Empty;

            cmdText = string.Format(@"                    
                    select *
                    from WebReworkJobHead
                    where id = {0}
                ",
                id
            );
            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
            var getNewID = "";
            if (dtc[0].Rows.Count > 0)
            {
                DataRow row = dtc[0].Rows[0];
                int subJobCount = 0;
                //如返工单再拆小工单则出错
                cmdText = string.Format(@"
                        select count(*) as subJobCount
                        from WebJobHead
                        where Company = '{0}' and JobNum like 'R{1}%'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    row["ParentJobNum"]
                );
                //cmdText = string.Format(@"
                //        declare @ReworkPrefix nvarchar(5), @ReworkJobNumPrefix nvarchar(50),
                //            @JobNum	nvarchar(100) = '{0}'
                //        if left(@JobNum,1)='R'
	               //         set @ReworkJobNumPrefix=substring(@JobNum,0,len(@JobNum)-charindex('-',reverse(@JobNum),0)+1)
                //        else
	               //         set @ReworkJobNumPrefix=@JobNum

                //        if(CHARINDEX('R',@JobNum,0))>0 and left (@JobNum,3)<>'FRM'
		              //      set @ReworkPrefix=''
	               //     else
		              //      set @ReworkPrefix='R'

                //        select count(*) as subJobCount
                //        from WebJobHead
                //        where JobNum like @ReworkPrefix+@ReworkJobNumPrefix+'%'
                //    ",
                //    row["ParentJobNum"]
                //);
                DataTableCollection dtcSub = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                if (dtcSub[0].Rows.Count > 0)
                {
                    subJobCount = int.Parse(dtcSub[0].Rows[0]["subJobCount"].ToString());
                }
                else
                {
                    subJobCount = 0;
                }

                subJobCount++;

                string cmdTextPJ = string.Format(@"
                        select ShortChar01,HeatCode,MaterialId,PartDesc,CustomVersion,Division
                        from WebJobHead
                        where Company = '{0}' and JobNum = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    row["ParentJobNum"]
                );
                DataTableCollection dtcSubPJ = SqlHelper.GetTable(CommandType.Text, cmdTextPJ, null);

                string ShortChar01 = string.Empty;
                string HeatCode = string.Empty;
                string MaterialId = string.Empty;
                string PartDesc = string.Empty;
                string CustomVersion = string.Empty;
                string Division = string.Empty;

                if (dtcSubPJ[0].Rows.Count > 0)
                {
                    ShortChar01 = dtcSubPJ[0].Rows[0]["ShortChar01"].ToString();
                    HeatCode = dtcSubPJ[0].Rows[0]["HeatCode"].ToString();
                    MaterialId = dtcSubPJ[0].Rows[0]["MaterialId"].ToString();
                    PartDesc = dtcSubPJ[0].Rows[0]["PartDesc"].ToString();
                    CustomVersion = dtcSubPJ[0].Rows[0]["CustomVersion"].ToString();
                    Division = dtcSubPJ[0].Rows[0]["Division"].ToString();
                }

                jobNumRE = string.Format("R{0}-{1}", row["ParentJobNum"], string.Format("{0:d2}", subJobCount));
                if (jobNum.StartsWith("R"))
                {
                    int count = 0;

                    string cmdTextReJobNum = string.Format(@"
                            select *
                            from WebJobHead
                            where Company = '{0}' and JobNum like '{1}%'
                        ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        jobNum.Substring(0, jobNum.Length - 2)
                    );
                    DataTableCollection dtcReJobNumCount = SqlHelper.GetTable(CommandType.Text, cmdTextReJobNum, null);

                    count = dtcReJobNumCount[0].Rows.Count + 1;
                    jobNumRE = string.Format("{0}{1}", jobNum.Substring(0, jobNum.Length - 2), string.Format("{0:d2}", count));
                }
                else
                {
                    string cmdTextReJobNum = string.Format(@"
                            select *
                            from WebJobHead
                            where JobNum = '{0}'
                        ",
                        jobNum
                    );
                    DataTableCollection dtcReJobNum = SqlHelper.GetTable(CommandType.Text, cmdTextReJobNum, null);
                    if (dtcReJobNum[0].Rows.Count > 0)
                    {
                        epicorJobNum = dtcReJobNum[0].Rows[0]["EpicorJobNum"].ToString();
                        firstWebJobNum = string.Format("{0}{1}", epicorJobNum, jobNum.Substring(epicorJobNum.Length, 5));
                        cmdTextReJobNum = string.Format(@"
                                select *
                                from WebJobHead
                                where Company = '{0}' and JobNum like 'R{1}%'
                            ",
                            ConfigurationManager.AppSettings["CompanyCode"],
                            firstWebJobNum
                        );
                        DataTableCollection dtcReJobNumCount = SqlHelper.GetTable(CommandType.Text, cmdTextReJobNum, null);
                        int count = 0;
                        if (dtcReJobNumCount[0].Rows.Count > 0)
                        {
                            count = dtcReJobNumCount[0].Rows.Count;
                        }
                        count++;
                        jobNumRE = string.Format("R{0}-{1}", firstWebJobNum, string.Format("{0:d2}", count));
                    }

                }

                cmdText = string.Format(@"
                    insert into WebJobHead(Company,ParentJobNum,JobNum,PartNum,RevisionNum,DrawNum,PartDescription,ProdQty,IUM,EpicorJobNum,ShortChar01,HeatCode,MaterialId,PartDesc,CustomVersion,CreatedBy,Division)
                        values('{0}','{1}','{16}','{2}','{3}','{4}','{5}',{6},'{7}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{17}')
                        Select @@Identity
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                    row["ParentJobNum"],                                //1:JobNum
                    row["PartNum"],                                //2:PartNum
                    row["RevisionNum"],                                 //3:RevisionNum
                    row["DrawNum"],                                     //4:DrawNum
                    row["PartDescription"],                             //5:PartDescription
                    row["ProdQty"],                                     //6:ProdQty
                    row["IUM"],                                         //7:IUM
                    string.Format("{0:d2}", subJobCount),               //8:SubJobCount
                    row["EpicorJobNum"],                                //9:
                    ShortChar01,                                        //10:
                    HeatCode,                                           //11:
                    MaterialId,                                         //12:
                    PartDesc,                                           //13:
                    CustomVersion,                                      //14:    
                    user.UserName,                            //15: CreateBy
                    jobNumRE,                                           //16: ReworkJobNum
                    Division                                            //17: Division
                );
                //SqlHelper.ExecteNonQueryText(cmdText, null);
                getNewID =Convert.ToString(SqlHelper.ExecuteScalar(CommandType.Text, cmdText));
                
                int opseq = 1;
                
                foreach(string op in opers)
                {
                    int oprqty = 0;

                    if (opseq == 1)
                    {
                        oprqty = int.Parse(row["ProdQty"].ToString());
                    }

                    cmdText = string.Format(@"
                            select OpDesc
                            from OpMaster
                            where Company = '{0}' and OpCode = '{1}'
                        ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        op
                    );
                    DataTableCollection dtcOp = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);


                    cmdText = string.Format(@"
                            insert into WebJobOper(Company,JobNum,AssemblySeq,OprSeq,OpCode,OpDesc,Backflush,OprQty,SubContract,JobStatus)
                            values('{0}','{7}',0,{3},'{4}','{5}',0,{6},0,'INIT')
                        ",
                        ConfigurationManager.AppSettings["CompanyCode"],    //0
                        row["ParentJobNum"],                                //1
                        string.Format("{0:d2}", subJobCount),               //2
                        opseq * 10,                                         //3
                        op,                                                 //4
                        dtcOp[0].Rows[0]["OpDesc"],                         //5
                        oprqty,                                             //6
                        jobNumRE                                            //7
                    );
                    SqlHelper.ExecteNonQueryText(cmdText, null);

                    opseq++;
                }

                cmdText = string.Format(@"
                        update WebReworkJobHead
                        set Split = 1,
                        ReJobNum = '{1}'
                        where id = {0}
                    ",
                    id,
                    jobNumRE
                );
                SqlHelper.ExecteNonQueryText(cmdText, null);
            }

            //Return the new WebJobHead as object
            var getSQL = "select * from WebJobHead  where ID='"+ getNewID + "'";
            SimpleJobHead simpleJobHead = new SimpleJobHead();

            SqlDataReader drGet = SqlHelper.ExecuteReader(CommandType.Text, getSQL);
            if (drGet.Read())
            {
                simpleJobHead.ID = drGet["ID"].ToString();
                simpleJobHead.PartNum = drGet["PartNum"].ToString();
                simpleJobHead.JobNum = drGet["JobNum"].ToString();
                simpleJobHead.PrintCount = drGet["PrintCount"].ToString();
                simpleJobHead.Revision = drGet["RevisionNum"].ToString();
                simpleJobHead.JobType = drGet["ShortChar01"].ToString();

            }
            drGet.Close();

            BaseResponse<SimpleJobHead> baseResponse = new BaseResponse<SimpleJobHead>();
            baseResponse.MsgCode = "OK";
            baseResponse.Data = simpleJobHead;

            //return Json(jobNumRE);
            return Json(baseResponse);
        }

        public JsonResult UpdateReworkJob(string id, string jobNum, List<string> opers)
        {
            string cmdText = string.Empty;
            DataTableCollection dtc;
            SimpleResult result = new SimpleResult();
            result.Result = "Success";

            //判断返工工单是否已开工
            cmdText = string.Format(@"                    
                    select COUNT(*) as trans
                    from WebTransaction
                    where JobNum = '{0}'
                ",
                jobNum
            );
            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            try
            {
                if (!dtc[0].Rows[0]["trans"].ToString().Equals("0"))
                {
                    //有WebTransaction记录，该工单已开工
                    result.Result = "Error";
                    result.Desc = string.Format(GetResValue("Txt_JobStartNoModify"), jobNum);
                }
                else
                {
                    cmdText = string.Format(@"
                            delete 
                            from WebJobOper 
                            where Company = '{0}' and JobNum = '{1}'
                        ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        jobNum
                    );
                    SqlHelper.ExecteNonQueryText(cmdText, null);

                    int opseq = 1;
                    int prodQty = 0;
                    cmdText = string.Format(@"
                                select ProdQty
                                from WebJobHead
                                where Company = '{0}' and JobNum = '{1}'
                            ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        jobNum
                    );
                    dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                    if (dtc[0].Rows.Count > 0)
                    {
                        double pq = 0;
                        double.TryParse(dtc[0].Rows[0]["ProdQty"].ToString(), out pq);
                        prodQty = (int)pq;
                    }

                    foreach (string op in opers)
                    {
                        int oprqty = 0;

                        if (opseq == 1)
                        {
                            oprqty = prodQty;
                        }

                        cmdText = string.Format(@"
                                select OpDesc
                                from OpMaster
                                where Company = '{0}' and OpCode = '{1}'
                            ",
                            ConfigurationManager.AppSettings["CompanyCode"],
                            op
                        );
                        DataTableCollection dtcOp = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);


                        cmdText = string.Format(@"
                                insert into WebJobOper(Company,JobNum,AssemblySeq,OprSeq,OpCode,OpDesc,Backflush,OprQty,SubContract,JobStatus)
                                values('{0}','{1}',0,{2},'{3}','{4}',0,{5},0,'INIT')
                            ",
                            ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                            jobNum,                                             //1:JobNum
                            opseq * 10,                                         //2:OprSeq
                            op,                                                 //3:OpCode
                            dtcOp[0].Rows[0]["OpDesc"],                         //4:OpDesc
                            oprqty                                              //5:OpDesc
                        );
                        SqlHelper.ExecteNonQueryText(cmdText, null);

                        opseq++;
                    }

                    result.Desc = string.Format(GetResValue("Txt_JobModifySucceeded"), jobNum);
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex);
                result.Result = "Error";
                result.Desc = string.Format(GetResValue("Txt_JobModifyFailed"), jobNum);
            }
            
            return Json(result);
        }

        public JsonResult GetReworkOpers(string jobNum)
        {
            string cmdText = string.Empty;
            DataTableCollection dtcWebJob;
            string filter = string.Empty;

            string epicJobNum = string.Empty;
            /*
            try
            {
                epicJobNum = jobNum.Split('-')[0];
                while (epicJobNum.StartsWith("R"))
                {
                    epicJobNum = epicJobNum.Substring(1, epicJobNum.Length - 1);
                }
            }
            catch(Exception ex)
            {

            }
            */



            /*
            cmdText = string.Format(@"
                            select *
                            from WebJobOper
                            where Company = '{0}' and JobNum = '{1}'
                            order by OprSeq
                        ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    jobNum
                );
                */

            try
            {
                cmdText = string.Format(@"
                        select EpicorJobNum
                        from WebJobHead
                        where Company = '{0}' and  JobNum = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    jobNum
                );
                dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                if (dtcWebJob[0].Rows.Count > 0)
                {
                    epicJobNum = dtcWebJob[0].Rows[0]["EpicorJobNum"].ToString();
                }
            }
            catch(Exception ex)
            {

            }
            

            cmdText = string.Format(@"
                        select a.OpCode, isnull(b.OpDesc, a.OpDesc) OpDesc
                        from EpicorJobOper a
                        left join opmaster b on a.OpCode = b.OpCode and a.Company = b.Company
                        where a.Company = '{0}' and a.JobNum = '{1}'
                        order by OprSeq
                    ",
                ConfigurationManager.AppSettings["CompanyCode"],
                epicJobNum
            );
            dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleOpMaster> list = new List<SimpleOpMaster>();
            foreach (DataRow row in dtcWebJob[0].Rows)
            {
                SimpleOpMaster op = new SimpleOpMaster();
                op.OpCode = row["OpCode"].ToString() + ":" + row["OpDesc"].ToString();
                op.OpDesc = row["OpDesc"].ToString();
                list.Add(op);
            }
            
            return Json(list);
        }

        public JsonResult GetReworkOpersAdjust(string jobNum)
        {
            string cmdText = string.Empty;
            DataTableCollection dtcWebJob;
            string filter = string.Empty;
            
            cmdText = string.Format(@"
                        select *
                        from WebJobOper
                        where Company = '{0}' and JobNum = '{1}'
                        order by OprSeq
                    ",
                ConfigurationManager.AppSettings["CompanyCode"],
                jobNum
            );
            dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleOpMaster> list = new List<SimpleOpMaster>();
            foreach (DataRow row in dtcWebJob[0].Rows)
            {
                SimpleOpMaster op = new SimpleOpMaster();
                op.OpCode = row["OpCode"].ToString() + ":" + row["OpDesc"].ToString();
                op.OpDesc = row["OpDesc"].ToString();
                list.Add(op);
            }

            return Json(list);
        }

        public JsonResult GetWebJobQueryList(DataTablesParameters param, string JobType)
        {
            LogHelper.Debug("GetReworkCenterQueryList Started");
            string filter = string.Empty;
            int filterType = 0;

            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                filter = param.Search.Value;
                filterType = 10;
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = param.Columns[0].Search.Value;
                filterType = 0;
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                filter = param.Columns[2].Search.Value;
                filterType = 1;
            }
            else if (!string.IsNullOrEmpty(param.Columns[3].Search.Value))
            {
                filter = param.Columns[3].Search.Value;
                filterType = 2;
            }

            string cmdText = string.Format(@"SP_GetReworkCentreList '{0}',{1},{2},'{3}','{4}',{5}",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}CompanyID
                    param.Length,                                       //{1}PageSize
                    param.Start,                                        //{2}PageStart
                    param.OrderBy + " " +  param.OrderDir,              //{3}OrderBy Column + OrderBy Direction ASC/DESC
                    filter,                                             //{4}QueryString
                    filterType                                          //{5}QueryType
                );

            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);
            int recordsTotal = int.Parse(ds.Tables[0].Rows[0]["recordsTotal"].ToString());

            LogHelper.Debug(cmdText);
            List<SimpleEpicorJob> jobList = new List<SimpleEpicorJob>();
            LogHelper.Debug("Prepare JobList Start");
            foreach (DataRow row in ds.Tables[1].Rows)
            {
                SimpleEpicorJob job = new SimpleEpicorJob();
                job.id = row["id"].ToString();
                job.Company = row["Company"].ToString();
                job.JobNum = row["JobNum"].ToString();
                job.ParentJobNum = row["ParentJobNum"].ToString();
                job.PartNum = row["PartNum"].ToString();
                job.RevisionNum = row["RevisionNum"].ToString();
                job.DrawNum = row["DrawNum"].ToString();
                job.PartDescription = row["PartDescription"].ToString();
                job.ProdQty = row["ProdQty"].ToString();
                job.IUM = row["IUM"].ToString();
                job.JobClosed = row["JobClosed"].ToString();
                job.SplitPerQty = string.Empty;
                job.SplitOpr = string.Empty;
                job.CurOperSeq = row["CurOprSeq"].ToString();
                job.CurOperCode = row["CurOperCode"].ToString();
                job.CurOperQty = row["CutOprQty"].ToString();
                job.CurLocation = row["CurLocation"].ToString();
                job.CurOprStatus = row["CurOprStatus"].ToString();
                job.JobType = row["ShortChar01"].ToString();
                job.PrintUserId = row["PrintUserId"].ToString();
                job.PrintDateTime = row["PrintDateTime"].ToString();
                job.PrintCount = row["PrintCount"].ToString();
                job.OpDesc = row["OP"].ToString();
                job.DiscrepReason = row["Reason"].ToString();

                jobList.Add(job);
            }
            LogHelper.Debug("Prepare JobList End");
            DataTablesResult<SimpleEpicorJob> result = new DataTablesResult<SimpleEpicorJob>(param.Draw, recordsTotal, recordsTotal, jobList);
            LogHelper.Debug("GetWebJobQueryList End");
            return Json(result);
        }
    }
}