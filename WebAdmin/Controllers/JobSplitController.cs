using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

using WebAdmin.Models;

namespace WebAdmin.Controllers
{
    public class JobSplitController : BaseController
    {
        public JsonResult CheckHeatCode(string jobnum)
        {
            SimpleResult result = new SimpleResult();

            string cmdText = string.Empty;

            cmdText = string.Format(@"
                        select id
                        from WebHeatcode
                        where Company = '{0}' and JobNum = '{1}'
                    ",
                ConfigurationManager.AppSettings["CompanyCode"],
                jobnum
            );
            DataTableCollection dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            if (dtc[0].Rows.Count > 0)
            {
                result.Result = "true";
            }
            else
            {
                result.Result = "false";
            }

            return Json(result);
        }
        public JsonResult UploadHeatCode(string jobnum)
        {
            try
            {
                HttpPostedFileBase file = Request.Files[0];

                string filepath = string.Format("{0}tmp\\{1}.xls", Server.MapPath("~/"), Guid.NewGuid());

                file.SaveAs(filepath);
                string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + filepath + ";" + "Extended Properties=Excel 8.0;";
                OleDbConnection conn = new OleDbConnection(strConn);
                conn.Open();
                string strExcel = "";
                OleDbDataAdapter myCommand = null;
                DataSet ds = null;
                strExcel = "select * from [Sheet1$]";
                myCommand = new OleDbDataAdapter(strExcel, strConn);
                ds = new DataSet();
                myCommand.Fill(ds, "table1");

                int revision = 0;
                string cmdText = string.Empty;

                cmdText = string.Format(@"
                        select ISNULL(MAX(Revision),0) as Revision
                        from WebHeatcode
                        where Company = '{0}' and JobNum = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    jobnum
                );
                DataTableCollection dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                if (dtc[0].Rows.Count > 0)
                {
                    revision = int.Parse(dtc[0].Rows[0]["Revision"].ToString());
                }

                revision++;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    cmdText = string.Format(@"
                            insert into WebHeatcode(Company,Revision,JobNum,PartNum,RewJobNum,Heatcode)
                            values('{0}',{1},'{2}','{3}','{4}','{5}')
                        ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        revision,
                        jobnum,
                        dr[0].ToString(),
                        dr[1].ToString(),
                        dr[2].ToString()
                    );

                    SqlHelper.ExecteNonQueryText(cmdText, null);
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult JobTravelerPrint(List<string> jobList)
        {
            SimpleResult result = new SimpleResult();

            foreach (string job in jobList)
            {
                //
            }


            return Json(result);
        }

        //GetWebJobQueryList
        public JsonResult GetWebJobQueryList(DataTablesParameters param,string JobType)
        {
            LogHelper.Debug("GetWebJobQueryList Started");
            string filter = string.Empty;
            string order = string.Empty;
            string searchby = string.Empty;


            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                filter = string.Format(" and (JobNum like '%{0}%' or PartNum like '%{0}%' or DrawNum like '%{0}%' or PartDescription like '%{0}%') ", param.Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                filter = string.Format(" and PartNum like '%{0}%' ", param.Columns[2].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[3].Search.Value))
            {
                filter = string.Format(" and PartDescription like '%{0}%' ", param.Columns[3].Search.Value);
            }

            if (JobType.Equals("C"))
            {
                filter += " and ISNULL(ShortChar01,'') = 'C'";
            }
            else
            {
                filter += " and ISNULL(ShortChar01,'') <> 'C'";
            }

            string cmdText = string.Format(@"
                    select count(JobNum) recordsTotal
                    from 
                    WebJobHead
                    where Company = '{0}' {1}
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter
            );
            DataTableCollection dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtcWebJob[0].Rows[0]["recordsTotal"].ToString());
            cmdText = string.Format(@"
                        select top {1} *
                        from (
	                        select ROW_NUMBER() over(order by {3} {4}) as rownum,*
	                        from (
                                select 
                                    JH.id,JH.Company,JH.JobNum,JH.ParentJobNum,JH.EpicorJobNum,JH.PartNum,JH.RevisionNum,JH.DrawNum,JH.PartDescription,JH.ProdQty,JH.IUM,JH.JobClosed,JH.ClosedDate,
	                                '' as CurOprSeq,
	                                '' as CutOprQty,
	                                '' as CurLocation,
	                                '' as CurOperCode,
	                                '' as CurOprStatus,
                                    JH.ShortChar01,
                                    JH.PrintUserId,
									JH.PrintDateTime,
									JH.PrintCount
                                from WebJobHead as JH
                            ) as JHO
                        ) as JobHead
                        where rownum > {2} and  JobHead.Company = '{0}' {5}
                        order by {3} {4}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );



            dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);
            LogHelper.Debug(cmdText);
            List<SimpleEpicorJob> jobList = new List<SimpleEpicorJob>();
            LogHelper.Debug("Prepare JobList Start");
            foreach (DataRow row in dtcWebJob[0].Rows)
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
                //job.CurOpGroup = JobHelper.GetOpGroup(row["CurOperCode"].ToString());
                ////job.CurOpTransfer = JobHelper.GetOpTransfer(row["CurOperCode"].ToString()).Equals("True")?"需要转移":"不需要转移";
                //job.CurOpTransfer = JobHelper.GetOpTransfer(row["CurOperCode"].ToString());
                job.JobType = row["ShortChar01"].ToString();
                job.PrintUserId = row["PrintUserId"].ToString();
                job.PrintDateTime = row["PrintDateTime"].ToString();
                job.PrintCount = row["PrintCount"].ToString();

                jobList.Add(job);
            }
            LogHelper.Debug("Prepare JobList End");
            DataTablesResult<SimpleEpicorJob> result = new DataTablesResult<SimpleEpicorJob>(param.Draw, recordsTotal, recordsTotal, jobList);
            LogHelper.Debug("GetWebJobQueryList End");
            return Json(result);
        }
        public ActionResult WebJobQueryView(string JobType)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("WebAssemblyJobQueryView") && !CheckRight("WebJobQueryView"))
            {
                return Redirect("/Login/Home");
            }

            ViewBag.JobType = JobType;
            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            ViewBag.UserID = user.UserName;
            return View();
        }
        
        public string CheckWebJobSplit(string jobnum, string splitopr, string splitqty, string oprqty)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string result = "0";
            string cmdText = string.Empty;
            DataTableCollection dtc;

            try
            {

                string connectionString = ConfigurationManager.AppSettings["CompartConn"];
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    DataSet ds = new DataSet();
                    SqlDataAdapter adapter = new SqlDataAdapter();

                    SqlCommand cmd = new SqlCommand();
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    SqlTransaction trans = conn.BeginTransaction();
                    cmd.Connection = conn;
                    cmd.Transaction = trans;
                    try
                    {
                        //创建建子工单
                        //WebJobHead
                        cmdText = string.Format(@"
                                insert into WebJobHead(Company,JobNum,ParentJobNum,EpicorJobNum,PartNum,RevisionNum,DrawNum,PartDescription,ProdQty,IUM,JobClosed,Split,ShortChar01,HeatCode,MaterialId,PartDesc,CustomVersion,CreatedBy,Division)
                                select JH.Company,'{1}-{2}' as JobNum,JH.JobNum as ParentJobNum,JH.EpicorJobNum,JH.PartNum,JH.RevisionNum,JH.DrawNum,JH.PartDescription,(JS.OprQty - JS.SplitQty) as ProdQty,JH.IUM,0 as JobClosed,0 as Split,JH.ShortChar01,JH.HeatCode,JH.MaterialId,JH.PartDesc,JH.CustomVersion,'{3}',JH.Division
                                from WebJobHead as JH
                                left join WebJobSplit as JS on JH.Company = JS.Company and JH.JobNum = JS.JobNum and JS.Split = 0 and JS.OperSeq = {2}
                                where JH.Company = '{0}' and JH.JobNum = '{1}'
                            ",
                            ConfigurationManager.AppSettings["CompanyCode"],
                            jobnum,
                            splitopr,
                            user.UserName
                        );
                        cmd.CommandText = cmdText;
                        cmd.ExecuteNonQuery();

                        //WebJobOpr
                        cmdText = string.Format(@"
                                insert into WebJobOper(Company,JobNum,AssemblySeq,OprSeq,OpCode,OpDesc,Backflush,OprQty,LaborQty,DiscrepQty,ScrapQty,JobStatus,SubContract)
                                select Company,'{1}-{2}',AssemblySeq,OprSeq,OpCode,OpDesc,Backflush,
                                (
	                                case when 
	                                (
		                                select top 1 OprSeq as FirstOpr
		                                from WebJobOper as JO2
		                                where JO1.Company = JO2.Company and JO1.JobNum = JO2.JobNum and JO2.OprSeq >= {2}
		                                order by OprSeq
	                                ) = JO1.OprSeq then ({4} - {3}) else 0 end
                                ) as OprQty,
                                0,0,0,'INIT',SubContract
                                from WebJobOper as JO1
                                where Company = '{0}' and JobNum = '{1}' and OprSeq >= {2}
                            ",
                            ConfigurationManager.AppSettings["CompanyCode"],
                            jobnum,
                            splitopr,
                            splitqty,
                            oprqty
                        );
                        cmd.CommandText = cmdText;
                        cmd.ExecuteNonQuery();

                        //更新原工单状态WebJobSplit.Split = 1
                        cmdText = string.Format(@"
                                update WebJobSplit
                                set Split = 1
                                where Company = '{0}' and JobNum = '{1}' and OperSeq = {2} and Split = 0
                            ",
                            ConfigurationManager.AppSettings["CompanyCode"],
                            jobnum,
                            splitopr
                        );
                        cmd.CommandText = cmdText;
                        cmd.ExecuteNonQuery();

                        trans.Commit();
                    }
                    catch(Exception ex)
                    {
                        trans.Rollback();
                        LogHelper.Error(ex);
                    }
                }
                
                result = "1";
            }
            catch (Exception ex)
            {

            }

            return result;
        }
        public string WebJobSplit(string id, string copys)
        {
            try
            {
                string cmdText = string.Format(@"
                    select *
                    from WebJobHead
                    where id = {0}
                    ",
                    id
                );
                DataTableCollection dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                if (dtcWebJob[0].Rows.Count == 0) 
                { 
                    LogHelper.Error(string.Format("Can't find Web Job #{0}", id));
                    return "";
                }

                string cmdTextCount = string.Format(@"
                    select JobNum
                    from WebJobHead
                    where Company = '{0}' and ParentJobNum = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],            //0:Company
                    dtcWebJob[0].Rows[0]["JobNum"]                              //ParentJobNum       
                );
                DataTableCollection dtcWebJobCount = SqlHelper.GetTable(CommandType.Text, cmdTextCount, null);

                int subJobCount = dtcWebJobCount[0].Rows.Count + 1;

                DataRow row = dtcWebJob[0].Rows[0];
                string cmdTextSplit = string.Format(@"
                        insert into WebJobHead(Company,ParentJobNum,JobNum,PartNum,RevisionNum,DrawNum,PartDescription,ProdQty,IUM,EpicorJobNum,Division)
                                values('{0}','{1}','{1}-{8}','{2}','{3}','{4}','{5}',{6},'{7}','{9}','{10}')
                            ",
                            ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                            row["JobNum"],                                      //1:JobNum
                            row["PartNum"],                                     //2:PartNum
                            row["RevisionNum"],                                 //3:RevisionNum
                            row["DrawNum"],                                     //4:DrawNum
                            row["PartDescription"],                             //5:PartDescription
                            copys,                                              //6:ProdQty
                            row["IUM"],                                         //7:IUM
                            string.Format("{0:d2}", subJobCount),               //8:SubJobCount
                            row["EpicorJobNum"],
                            row["Division"]
                        );
                SqlHelper.ExecteNonQueryText(cmdTextSplit, null);

                string cmdTextJobOpr = string.Format(@"
                        select ISNULL(MAX(OprSeq),0)  as MaxOprSeq
                        from WebLaborDtl
                        where Company = '{0}' and JobNum = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                    row["JobNum"]                                       //1:JobNum
                );
                DataTableCollection dtcSplitJObOpr = SqlHelper.GetTable(CommandType.Text, cmdTextJobOpr, null);

                string cmdTextSplitJobOpr = string.Format(@"
                        insert into WebJobOper(Company,JobNum,AssemblySeq,OprSeq,OpCode,OpDesc,Backflush)
                        select Company,'{1}-{2}',AssemblySeq,OprSeq,OpCode,OpDesc,Backflush
                        from WebJobOper
                        where Company = '{0}' and JobNum = '{1}' and OprSeq >= {3}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                    row["JobNum"],                                      //1:JobNum
                    string.Format("{0:d2}", subJobCount),               //2:SubJobCount
                    dtcSplitJObOpr[0].Rows[0]["MaxOprSeq"]
                );
                SqlHelper.ExecteNonQueryText(cmdTextSplitJobOpr, null);

            }
            catch (Exception ex)
            {
                LogHelper.Error("WebJobSplit Error", ex);
            }



            return "";
        }

        public ActionResult SplitSubJobView(string id)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            string cmdText = string.Format(@"
                        select *
                        from WebJobHead
                        where id = {0}
                    ",
                    id
                );

            DataTableCollection dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);
            SimpleEpicorJob job = new SimpleEpicorJob();

            foreach (DataRow row in dtcWebJob[0].Rows)
            {

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

            }
            ViewBag.Job = job;
            return View();
        }

        public ActionResult WebJobListView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("WebJobListView"))
            {
                return Redirect("/Login/Home");
            }

            List<SimpleEpicorJob> jobList = new List<SimpleEpicorJob>();
            try
            {
                string cmdText = string.Format(@"
                        select JH.Company,JH.JobNum,JH.ParentJobNum,JH.EpicorJobNum,JH.PartNum,JH.RevisionNum,JH.DrawNum,JH.PartDescription,JH.ProdQty,JH.IUM,JH.JobClosed,JH.ClosedDate,JH.Split,JH.SplitDate,JS.SplitQty,JS.OperSeq
                        from WebJobHead as JH
                        inner join WebJobSplit as JS on JH.Company = JS.Company and JH.JobNum = JS.JobNum
                        where JH.Company = '{0}' and JS.Split = 0
                        order by JH.JobNum
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"]
                );

                DataTableCollection dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);


                foreach (DataRow row in dtcWebJob[0].Rows)
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
                    job.SplitPerQty = row["SplitQty"].ToString();
                    job.SplitOpr = row["OperSeq"].ToString();
                    jobList.Add(job);
                }

                LogHelper.Info(string.Format("Prepare Epicor Job Successfully, total {0} jobs", dtcWebJob.Count));
            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Format("Prepare Epicor Job Error"), ex);
            }

            ViewBag.JobList = jobList;
            return View();
        }

        /*
         Epicor工单拆分成Web工单
        */
        public string EpicorJobSplit(List<string> jobList)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string result = "OK";
            try
            {

                string jobFilter = string.Empty;
                foreach (string job in jobList)
                {
                    jobFilter += string.Format("'{0}',", job);
                }

                if (!string.IsNullOrEmpty(jobFilter))
                {
                    jobFilter = string.Format(" and id in({0})", jobFilter.Substring(0, jobFilter.Length - 1));
                }

                string cmdText = string.Format(@"
                        select top 100 *
                        from EpicorJobHead
                        where Company = '{0}' and Split = 0 and SplitPerQty is not null and SplitPerQty > 0 {1}
                        order by JobNum
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    jobFilter
                );

                DataTableCollection dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                List<SimpleEpicorJob> jobs = new List<SimpleEpicorJob>();
                string cmdTextSplit = string.Empty;
                string cmdTextJobOper = string.Empty;
                foreach (DataRow row in dtcWebJob[0].Rows)
                {
                    double pkgQty = double.Parse(row["SplitPerQty"].ToString());
                    double prodQty = double.Parse(row["ProdQty"].ToString());
                    double losseQty = prodQty % pkgQty;
                    int copys = (int)Math.Ceiling(prodQty / pkgQty);
                    bool noError = true;
                    string Division = row["Division"].ToString();

                    for (int i = 1; i <= copys; i++)
                    {
                        bool headInserted = false;
                        
                        if (losseQty > 0 && i == copys)
                        {
                            pkgQty = losseQty;
                        }
                        cmdTextSplit = string.Format(@"
                                    insert into WebJobHead(Company,ParentJobNum,EpicorJobNum,JobNum,PartNum,RevisionNum,DrawNum,PartDescription,ProdQty,IUM,ShortChar01,HeatCode,MaterialId,PartDesc,CustomVersion,CreatedBy,Division)
                                    values('{0}','{1}','{1}','{1}-{8}','{2}','{3}','{4}','{5}',{6},'{7}','{9}','{10}','{11}','{12}','{13}','{14}','{15}')
                                ",
                               ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                               row["JobNum"],                                      //1:JobNum
                               row["PartNum"],                                     //2:PartNum
                               row["RevisionNum"],                                 //3:RevisionNum
                               row["DrawNum"],                                     //4:DrawNum
                               row["PartDescription"],                             //5:PartDescription
                               pkgQty,                                             //6:ProdQty
                               row["IUM"],                                         //7:IUM
                               string.Format("{0:d4}", i),                         //8:SubJobSeq
                               row["ShortChar01"],                                 //9:ShortChar01-Assemblly Job
                               row["HeatCode"],
                               row["MaterialId"],
                               row["PartDesc"],
                               row["CustomVersion"],
                               user.UserName,
                               Division                                             //15.Division
                        );
                        headInserted = SqlHelper.ExecteNonQueryText(cmdTextSplit, null);
                        LogHelper.Info("Job Split cmdTextSplit:" + cmdTextSplit);

                        if (headInserted) {
                            cmdTextJobOper = string.Format(@"
                                    insert into WebJobOper(Company,JobNum,AssemblySeq,OprSeq,OpCode,OpDesc,Backflush,OprQty,LaborQty,DiscrepQty,ScrapQty,JobStatus,SubContract,CreatedBy)
                                    select JO1.Company,'{1}-{2}',JO1.AssemblySeq,JO1.OprSeq,JO1.OpCode,JO1.OpDesc,JO1.Backflush,
	                                        (
		                                        case when 
		                                        (
			                                        select top 1 MIN(OprSeq) as FirstOpr
			                                        from EpicorJobOper as JO2
			                                        where JO1.Company = JO2.Company and JO1.JobNum = JO2.JobNum
			                                        group by OprSeq
		                                        ) = JO1.OprSeq then {3} else 0 end
	                                        ) as OprQty,0,0,0,'INIT',JO1.SubContract,'{4}'
                                    from EpicorJobOper as JO1
                                    where JO1.Company = '{0}' and JO1.JobNum = '{1}'

                                ",
                                ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                                row["JobNum"],                                      //1:EpicorJobNum
                                string.Format("{0:d4}", i),                         //2:SubJobSeq
                                pkgQty,                                             //3:PkgQty
                                user.UserName
                            );
                            LogHelper.Info("Job split cmdTextJobOper" + cmdTextJobOper);
                            SqlHelper.ExecteNonQueryText(cmdTextJobOper, null);
                        }
                        else
                        {
                            noError = false;
                        }

                    }
                    if (noError)
                    {
                        string cmdTextUpdate = string.Format(@"
                                update EpicorJobHead
                                set Split = 1
                                where id = {0}
                            ",
                           row["id"]
                        );

                        SqlHelper.ExecteNonQueryText(cmdTextUpdate, null);

                    }
                    else {
                        result = "ERROR";
                    }
                    
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("EpicorJobSplit Error", ex);
                result = ex.Message;
            }


            //return RedirectToAction("EpicorJobListView");
            return result;
        }

        public string SetSplitCopys(string id, string copys)
        {
            string cmdText = string.Format(@"
                    update EpicorJobHead
                    set SplitPerQty = {1}
                    where id = {0} 
                ",
                id,
                copys
            );

            SqlHelper.ExecteNonQueryText(cmdText, null);
            return "";
        }

        /*
         * 设置拆分数量
         */
        public ActionResult SetSplitCopysView(string id)
        {
            string cmdText = string.Format(@"
                        select *
                        from EpicorJobHead
                        where id = {0}
                    ",
                    id
                );

            DataTableCollection dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);
            SimpleEpicorJob job = new SimpleEpicorJob();

            foreach (DataRow row in dtcWebJob[0].Rows)
            {

                job.id = row["id"].ToString();
                job.Company = row["Company"].ToString();
                job.JobNum = row["JobNum"].ToString();
                job.PartNum = row["PartNum"].ToString();
                job.RevisionNum = row["RevisionNum"].ToString();
                job.DrawNum = row["DrawNum"].ToString();
                job.PartDescription = row["PartDescription"].ToString();
                job.ProdQty = row["ProdQty"].ToString();
                job.IUM = row["IUM"].ToString();
                job.Split = row["Split"].ToString();
                job.SplitPerQty = row["SplitPerQty"].ToString();

            }
            ViewBag.Job = job;

            return View();
        }
        public JsonResult GetAssemblyJobList(DataTablesParameters param)
        {
            string filter = string.Empty;
            string searchby = string.Empty;


            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                if (param.Search.Value.Equals(GetResValue("Txt_Unsplit")))
                {
                    filter = " and Split = 0 ";
                }
                else if (param.Search.Value.Equals(GetResValue("Txt_Splitted")))
                {
                    filter = " and Split = 1 ";
                }
                else
                {
                    filter = string.Format(" and (JobNum like '%{0}%' or PartNum like '%{0}%' or DrawNum like '%{0}%' or PartDescription like '%{0}%') ", param.Search.Value);
                }
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and PartNum like '%{0}%' ", param.Columns[1].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                filter = string.Format(" and PartDescription like '%{0}%' ", param.Columns[2].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[3].Search.Value))
            {
                if (param.Columns[3].Search.Value.Equals(GetResValue("Txt_Unsplit")))
                {
                    filter = " and Split = 0 ";
                }
                else if (param.Columns[3].Search.Value.Equals(GetResValue("Txt_Splitted")))
                {
                    filter = " and Split = 1 ";
                }
                    
            }

            string cmdText = string.Format(@"
                    select count(JobNum) recordsTotal
                    from EpicorJobHead
                    where Company = '{0}' {1} and ShortChar01 = 'C'
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter
            );
            DataTableCollection dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtcWebJob[0].Rows[0]["recordsTotal"].ToString());

            cmdText = string.Format(@"
                        select top {1} *
                        from (
	                        select ROW_NUMBER() over(order by {3} {4}) as rownum,*
	                        from EpicorJobHead
                            where  Company = '{0}' and ShortChar01 = 'C' {5}
                        ) as JobHead
                        where rownum > {2} and  JobHead.Company = '{0}' and ShortChar01 = 'C' {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );

            dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleEpicorJob> jobList = new List<SimpleEpicorJob>();
            foreach (DataRow row in dtcWebJob[0].Rows)
            {
                SimpleEpicorJob job = new SimpleEpicorJob();
                job.id = row["id"].ToString();
                job.Company = row["Company"].ToString();
                job.JobNum = row["JobNum"].ToString();
                job.PartNum = row["PartNum"].ToString();
                job.RevisionNum = row["RevisionNum"].ToString();
                job.DrawNum = row["DrawNum"].ToString();
                job.PartDescription = row["PartDescription"].ToString();
                job.ProdQty = row["ProdQty"].ToString();
                job.IUM = row["IUM"].ToString();
                job.Split = row["Split"].ToString();
                job.SplitPerQty = row["SplitPerQty"].ToString();
                
                jobList.Add(job);
            }

            DataTablesResult<SimpleEpicorJob> result = new DataTablesResult<SimpleEpicorJob>(param.Draw, recordsTotal, recordsTotal, jobList);
            return Json(result);
        }
        public JsonResult GetEpicorJobList(DataTablesParameters param)
        {
            string filter = string.Empty;
            string searchby = string.Empty;


            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                filter = string.Format(" and (JobNum like '%{0}%' or PartNum like '%{0}%' or DrawNum like '%{0}%' or PartDescription like '%{0}%') ", param.Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and PartNum like '%{0}%' ", param.Columns[1].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                filter = string.Format(" and PartDescription like '%{0}%' ", param.Columns[2].Search.Value);
            }

            string cmdText = string.Format(@"
                    select count(JobNum) recordsTotal
                    from EpicorJobHead
                    where Company = '{0}' and Split = 0 {1} and ShortChar01 <> 'C'
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter
            );
            DataTableCollection dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtcWebJob[0].Rows[0]["recordsTotal"].ToString());

            cmdText = string.Format(@"
                        select top {1} *
                        from (
	                        select ROW_NUMBER() over(order by {3} {4}) as rownum,*
	                        from EpicorJobHead
                            where  Company = '{0}' and Split = 0 and ShortChar01 <> 'C' {5}
                        ) as JobHead
                        where rownum > {2} and  JobHead.Company = '{0}' and JobHead.Split = 0 and ShortChar01 <> 'C' {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );

            dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleEpicorJob> jobList = new List<SimpleEpicorJob>();
            foreach (DataRow row in dtcWebJob[0].Rows)
            {
                SimpleEpicorJob job = new SimpleEpicorJob();
                job.id = row["id"].ToString();
                job.Company = row["Company"].ToString();
                job.JobNum = row["JobNum"].ToString();
                job.PartNum = row["PartNum"].ToString();
                job.RevisionNum = row["RevisionNum"].ToString();
                job.DrawNum = row["DrawNum"].ToString();
                job.PartDescription = row["PartDescription"].ToString();
                job.ProdQty = row["ProdQty"].ToString();
                job.IUM = row["IUM"].ToString();
                job.Split = row["Split"].ToString();
                job.SplitPerQty = row["SplitPerQty"].ToString();

                jobList.Add(job);
            }

            DataTablesResult<SimpleEpicorJob> result = new DataTablesResult<SimpleEpicorJob>(param.Draw, recordsTotal, recordsTotal, jobList);
            return Json(result);
        }

        public JsonResult GetWebJobList(DataTablesParameters param)
        {
            string filter = string.Empty;
            string searchby = string.Empty;


            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                if (param.Search.Value.Equals(GetResValue("Txt_Unsplit")))
                {
                    filter = " and Split = 0 ";
                }
                else if (param.Search.Value.Equals(GetResValue("Txt_Splitted")))
                {
                    filter = " and Split = 1 ";
                }
                else
                {
                    filter = string.Format(" and (JobNum like '%{0}%' or PartNum like '%{0}%' or DrawNum like '%{0}%' or PartDescription like '%{0}%') ", param.Search.Value);
                }
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                filter = string.Format(" and PartNum like '%{0}%' ", param.Columns[2].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[3].Search.Value))
            {
                filter = string.Format(" and PartDescription like '%{0}%' ", param.Columns[3].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[4].Search.Value))
            {
                if (param.Columns[4].Search.Value.Equals(GetResValue("Txt_Unsplit")))
                {
                    filter = " and Split = 0 ";
                }
                else if (param.Columns[4].Search.Value.Equals(GetResValue("Txt_Splitted")))
                {
                    filter = " and Split = 1 ";
                }

            }

            string cmdText = string.Format(@"
                    select count(JobNum) recordsTotal
                    from 
                    (
                        select JH.id,JH.Company,JH.JobNum,JH.ParentJobNum,JH.EpicorJobNum,JH.PartNum,JH.RevisionNum,JH.DrawNum,JH.PartDescription,JH.ProdQty,JH.IUM,JH.JobClosed,JH.ClosedDate,JS.Split,JH.SplitDate,JS.SplitQty,JS.OperSeq,JS.JobStatus
                        from WebJobHead as JH
                        inner join (
	                        select JS1.*,JO1.JobStatus
	                        from WebJobSplit as JS1
	                        left join WebJobOper as JO1 on JS1.Company = JO1.Company and JS1.JobNum = JO1.JobNum and JS1.OperSeq = JO1.OprSeq
                        ) as JS on JH.Company = JS.Company and JH.JobNum = JS.JobNum
                        where JH.Company = '{0}'
                    )
                    as WebJobHead
                    where Company = '{0}' {1}
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter
            );
            DataTableCollection dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtcWebJob[0].Rows[0]["recordsTotal"].ToString());

            /*
            cmdText = string.Format(@"
                        select top {1} *
                        from (
	                        select ROW_NUMBER() over(order by {3} {4}) as rownum,*
	                        from WebJobHead
                            where  Company = '{0}' {5}
                        ) as JobHead
                        where rownum > {2} and  JobHead.Company = '{0}' {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );
            */

            cmdText = string.Format(@"
                        select top {1} *
                        from (
	                        select ROW_NUMBER() over(order by {3} {4}) as rownum,*
	                        from 
                            (
                                select JH.id,JH.Company,JH.JobNum,JH.ParentJobNum,JH.EpicorJobNum,JH.PartNum,JH.RevisionNum,JH.DrawNum,JH.PartDescription,JH.ProdQty,JH.IUM,JH.JobClosed,JH.ClosedDate,JS.Split,JH.SplitDate,JS.SplitQty,JS.OperSeq,JS.OprQty,JS.JobStatus
                                from WebJobHead as JH
                                inner join (
	                                select JS1.*,JO1.JobStatus
	                                from WebJobSplit as JS1
	                                left join WebJobOper as JO1 on JS1.Company = JO1.Company and JS1.JobNum = JO1.JobNum and JS1.OperSeq = JO1.OprSeq
                                ) as JS on JH.Company = JS.Company and JH.JobNum = JS.JobNum
                                where JH.Company = '{0}'
                            )
                            as WebJobHead
                            where  Company = '{0}' {5}
                        ) as JobHead
                        where rownum > {2} and  JobHead.Company = '{0}' {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );

            dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleEpicorJob> jobList = new List<SimpleEpicorJob>();
            foreach (DataRow row in dtcWebJob[0].Rows)
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
                job.SplitPerQty = row["SplitQty"].ToString();
                job.SplitOpr = row["OperSeq"].ToString();
                job.CurOperQty = row["OprQty"].ToString();
                job.CurOprStatus = row["JobStatus"].ToString();
                job.Split = row["Split"].ToString();
                jobList.Add(job);
            }

            DataTablesResult<SimpleEpicorJob> result = new DataTablesResult<SimpleEpicorJob>(param.Draw, recordsTotal, recordsTotal, jobList);
            return Json(result);
        }

        public ActionResult AssemblyJobListView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("AssemblyJobListView"))
            {
                return Redirect("/Login/Home");
            }

            JobSyncHelper helper = JobSyncHelper.CreateInstance();
            helper.SyncEpicorJob();

            ViewBag.Language = user.Language;
            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            return View();
        }

        /*
        载入Epicor工单拆分页面
        */
        public ActionResult EpicorJobListView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("EpicorJobListView"))
            {
                return Redirect("/Login/Home");
            }

            List<SimpleEpicorJob> jobList = new List<SimpleEpicorJob>();

            JobSyncHelper helper = JobSyncHelper.CreateInstance();
            helper.SyncEpicorJob();

            //刷新Epicor工单拆分列表
            try
            {
                /*
                string cmdText = string.Format(@"
                        select *
                        from EpicorJobHead
                        where Company = '{0}' and Split = 0
                        order by JobNum
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"]
                );

                DataTableCollection dtcWebJob = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                List<SimpleEpicorJob> jobs = new List<SimpleEpicorJob>();
                foreach (DataRow row in dtcWebJob[0].Rows)
                {
                    SimpleEpicorJob job = new SimpleEpicorJob();
                    job.id = row["id"].ToString();
                    job.Company = row["Company"].ToString();
                    job.JobNum = row["JobNum"].ToString();
                    job.PartNum = row["PartNum"].ToString();
                    job.RevisionNum = row["RevisionNum"].ToString();
                    job.DrawNum = row["DrawNum"].ToString();
                    job.PartDescription = row["PartDescription"].ToString();
                    job.ProdQty = row["ProdQty"].ToString();
                    job.IUM = row["IUM"].ToString();
                    job.Split = row["Split"].ToString();
                    job.SplitPerQty = row["SplitPerQty"].ToString();

                    jobList.Add(job);
                }

                LogHelper.Info(string.Format("Prepare Epicor Job Successfully, total {0} jobs", dtcWebJob[0].Rows.Count));
                */
            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Format("Prepare Epicor Job Error"), ex);
            }

            ViewBag.JobList = jobList;
            return View();
        }
    }
}