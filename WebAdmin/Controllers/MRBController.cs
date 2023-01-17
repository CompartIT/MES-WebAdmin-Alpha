using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using WebAdmin.Models;

namespace WebAdmin.Controllers
{
    public class MRBController : BaseController
    {
        public ActionResult MRBPrintListView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("MRBPrintListView"))
            {
                return Redirect("/Login/Home");
            }

            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            ViewBag.UserID = user.UserName;
            return View();
        }


        public JsonResult GetMRBPrintList(DataTablesParameters param)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string filter = string.Empty;
            string searchby = string.Empty;


            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                filter = string.Format(" and (JobNum like '%{0}%' or OprDesc like '%{0}%' or ReasonCode like '%{0}%' or OPCode like '%{0}%') ", param.Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and OPCode like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[1].Search.Value);
            }

            if (!user.UserRole.Equals("1"))
            {
                filter += string.Format(" and ReportId = '{0}'", user.UserName);
            }

            string cmdText = string.Format(@"
                    select count(*) as recordsTotal
                    from WebDiscrepHeader
                    where Company = '{0}' and Status = '0' {2}
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                user.UserName,
                filter
            );
            DataTableCollection dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtc[0].Rows[0]["recordsTotal"].ToString());

            cmdText = string.Format(@"
                        select top {1} *
                        from (
                            select ROW_NUMBER() over(order by {3} {4}) as rownum,*
                            from WebDiscrepHeader
                            where Company = '{0}' and Status = '0' {5}
                        ) as Discrep
                        where rownum > {2} and  Discrep.Company = '{0}' and Status = '0' {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );

            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleDiscrepHeader> list = new List<SimpleDiscrepHeader>();
            foreach (DataRow row in dtc[0].Rows)
            {
                SimpleDiscrepHeader discrep = new SimpleDiscrepHeader();

                discrep.id = row["id"].ToString();
                discrep.JobNum = row["JobNum"].ToString();
                discrep.OprSeq = row["OprSeq"].ToString();
                discrep.OpCode = row["OpCode"].ToString();
                discrep.OprDesc = row["OprDesc"].ToString();
                discrep.Quantity = row["Quantity"].ToString();
                discrep.ReasonCode = row["ReasonCode"].ToString();
                discrep.ReasonDesc = row["ReasonDesc"].ToString();
                discrep.PrintDateTime = row["PrintDateTime"].ToString();
                discrep.PrintUserId = row["PrintUserId"].ToString();

                list.Add(discrep);
            }
            DataTablesResult<SimpleDiscrepHeader> result = new DataTablesResult<SimpleDiscrepHeader>(param.Draw, recordsTotal, recordsTotal, list);
            return Json(result);
        }

        public JsonResult GetMRBPrintListNew(DataTablesParameters param)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string filter = string.Empty;
            int filterType = 0;

            if (!string.IsNullOrEmpty(param.Search.Value))
            {//all
                filter = param.Search.Value;
                filterType = 10;
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {//OPCode
                filter = param.Columns[0].Search.Value;
                filterType = 0;
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {//JobNum
                filter = param.Columns[1].Search.Value;
                filterType = 1;
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {//JobNum
                filter = param.Columns[2].Search.Value;
                filterType = 2;
            }

            string cmdText = string.Format(@"SP_GetMRBProcessList '{0}',{1},{2},'{3}','{4}',{5},'{6}'",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}CompanyID
                    param.Length,                                       //{1}PageSize
                    param.Start,                                        //{2}PageStart
                    param.OrderBy + " " + param.OrderDir,               //{3}OrderBy Column + OrderBy Direction ASC/DESC
                    filter,                                             //{4}QueryString
                    filterType,                                         //{5}QueryType
                    user.UserName                        //{6}UserName
                );

            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);
            int recordsTotal = int.Parse(ds.Tables[0].Rows[0]["recordsTotal"].ToString());

            List<SimpleDiscrepHeader> list = new List<SimpleDiscrepHeader>();
            foreach (DataRow row in ds.Tables[1].Rows)
            {
                SimpleDiscrepHeader discrep = new SimpleDiscrepHeader();

                discrep.id = row["id"].ToString();
                discrep.JobNum = row["JobNum"].ToString();
                discrep.OprSeq = row["OprSeq"].ToString();
                discrep.OpCode = row["OpCode"].ToString();
                discrep.OprDesc = row["OprDesc"].ToString();
                discrep.Quantity = row["Quantity"].ToString();
                discrep.ReasonCode = row["ReasonCode"].ToString();
                discrep.ReasonDesc = row["ReasonDesc"].ToString();
                discrep.PrintDateTime = row["PrintDateTime"].ToString();
                discrep.PrintUserId = row["PrintUserId"].ToString();

                list.Add(discrep);
            }
            DataTablesResult<SimpleDiscrepHeader> result = new DataTablesResult<SimpleDiscrepHeader>(param.Draw, recordsTotal, recordsTotal, list);
            return Json(result);
        }

        public ActionResult MRBProcessListView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("MRBProcessListView"))
            {
                return Redirect("/Login/Home");
            }

            string cmdText = string.Empty;

            cmdText = string.Format(@"
                    select VendorID,Name
                    from Erp.Vendor
                    where Company = '{0}' 
                    order by VendorID
                ",
                ConfigurationManager.AppSettings["CompanyCode"]
            );
            DataTableCollection dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

            //获取供应商
            List<string> vendors = new List<string>();
            foreach(DataRow row in dtc[0].Rows)
            {
                vendors.Add(string.Format("{0}-{1}",row["VendorID"],row["Name"]));
            }

            //获取原因
            List<SimpleDiscrepReason> MRBReasons = new List<SimpleDiscrepReason>();
            SimpleDiscrepReason simpleDiscrepReason = new SimpleDiscrepReason();
            string strSql = string.Format(@"MRB_GetReason '{0}'", user.Language);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
            foreach (DataRow dr in ds.Tables[0].Rows) {
                simpleDiscrepReason = new SimpleDiscrepReason();
                simpleDiscrepReason.MRBReasonCode = dr["DefectCode"].ToString();
                simpleDiscrepReason.MRBReasonDesc = dr["Description_CH"].ToString();
                MRBReasons.Add(simpleDiscrepReason);
            }

            ViewBag.Vendors = vendors;
            ViewBag.MRBReasons = MRBReasons;
            return View();
        }
        
        public JsonResult GetMRBProcessList(DataTablesParameters param)
        {
            string filter = string.Empty;
            string searchby = string.Empty;


            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                filter = string.Format(" and (MRBNum = {0} or JobNum like '%{0}%' or OprDesc like '%{0}%' or ReasonCode like '%{0}%') ", param.Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and MRBNum like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[1].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                if (param.Columns[2].Search.Value.Equals(GetResValue("Txt_Unhandled")))
                {
                    filter = " and Status = '1' ";
                }
                else if (param.Columns[2].Search.Value.Equals(GetResValue("Txt_Handled")))
                {
                    filter = " and Status = '2' ";
                }
            }

            string cmdText = string.Format(@"
                    select count(1) as recordsTotal
                    from WebDiscrepHeader
                    where Company = '{0}' and Status <> '9' and Status <> '0' {1}
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter
            );
            DataTableCollection dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtc[0].Rows[0]["recordsTotal"].ToString());

            cmdText = string.Format(@"
                        select top {1} *
                        from (
                            select ROW_NUMBER() over(order by {3} {4}) as rownum,*
                            from WebDiscrepHeader
                            where Company = '{0}' and Status <> '9'  and Status <> '0' {5}
                        ) as Discrep
                        where rownum > {2} and  Discrep.Company = '{0}' and Status <> '9'  and Status <> '0' {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );

            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleDiscrepHeader> list = new List<SimpleDiscrepHeader>();
            foreach (DataRow row in dtc[0].Rows)
            {
                SimpleDiscrepHeader discrep = new SimpleDiscrepHeader();

                discrep.id = row["id"].ToString();
                discrep.JobNum = row["JobNum"].ToString();
                discrep.OprSeq = row["OprSeq"].ToString();
                discrep.OpCode = row["OpCode"].ToString();
                discrep.OprDesc = row["OprDesc"].ToString();
                discrep.Quantity = row["Quantity"].ToString();
                discrep.ReasonCode = row["ReasonCode"].ToString();
                discrep.ReasonDesc = row["ReasonDesc"].ToString();
                discrep.PrintUserId = row["PrintUserId"].ToString();
                discrep.PrintDateTime = row["PrintDateTime"].ToString();
                discrep.PrintCount = row["PrintCount"].ToString();
                discrep.VendorNum = row["VendorNum"].ToString();
                discrep.VendorName = row["VendorName"].ToString();
                discrep.Status = row["Status"].ToString();

                list.Add(discrep);
            }
            DataTablesResult<SimpleDiscrepHeader> result = new DataTablesResult<SimpleDiscrepHeader>(param.Draw, recordsTotal, recordsTotal, list);
            return Json(result);
        }

        public JsonResult GetMRBProcessListNew(DataTablesParameters param)
        {
            string filter = string.Empty;
            string searchby = string.Empty;
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                if (param.Search.Value.Equals(GetResValue("Txt_Unhandled")))
                {
                    filter = " and Status = '1' ";
                }
                else if (param.Search.Value.Equals(GetResValue("Txt_Handled")))
                {
                    filter = " and Status = '2' ";
                }
                else
                {
                    filter = string.Format(" and (id like '%{0}%' or JobNum like '%{0}%' or OprDesc like '%{0}%' or ReasonCode like '%{0}%') ", param.Search.Value);
                }
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and id like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[1].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                if (param.Columns[2].Search.Value.Equals(GetResValue("Txt_Unhandled")))
                {
                    filter = " and Status = '1' ";
                }
                else if (param.Columns[2].Search.Value.Equals(GetResValue("Txt_Handled")))
                {
                    filter = " and Status = '2' ";
                }
            }

            string cmdText = string.Format(@"
                    select count(1) as recordsTotal
                    from WebDiscrepHeader
                    where Company = '{0}' and Status <> '9' and Status <> '0' {1}
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter
            );
            DataTableCollection dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtc[0].Rows[0]["recordsTotal"].ToString());

            cmdText = string.Format(@"
                        select top {1} *
                        from (
                            select ROW_NUMBER() over(order by {3} {4}) as rownum, a.*,
                                b.ReasonDescNew, isnull(c.OpDesc, a.OprDesc) OprDescNew
                            from WebDiscrepHeader a
							outer apply(select case '{6}' when 'zh-cn' then Description_CH when 'ms' then Description_MS else Description_EN end ReasonDescNew
										from WebDiscrepReason x 
										where a.ReasonCode = x.DefectCode) b
							outer apply(select case '{6}' when 'zh-cn' then OpDesc when 'ms' then OpDesc_MS else OpDesc_EN end OpDesc
										from OpMaster x 
										where a.OpCode = x.OpCode and a.Company = x.Company) c
                            where Company = '{0}' and Status <> '9' and Status <> '5'  and Status <> '0' {5}
                        ) as Discrep
                        where rownum > {2} and  Discrep.Company = '{0}' and Status <> '9' and Status <> '5' and Status <> '0' {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter,                                              //{5}   filter
                    user.Language
                );

            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleDiscrepHeader> list = new List<SimpleDiscrepHeader>();
            foreach (DataRow row in dtc[0].Rows)
            {
                SimpleDiscrepHeader discrep = new SimpleDiscrepHeader();

                discrep.id = row["id"].ToString();
                discrep.JobNum = row["JobNum"].ToString();
                discrep.OprSeq = row["OprSeq"].ToString();
                discrep.OpCode = row["OpCode"].ToString();
                discrep.OprDesc = row["OprDesc"].ToString();
                discrep.Quantity = row["Quantity"].ToString();
                discrep.ReasonCode = row["ReasonCode"].ToString();
                discrep.ReasonDesc = row["ReasonDescNew"].ToString();
                discrep.PrintUserId = row["PrintUserId"].ToString();
                discrep.PrintDateTime = row["PrintDateTime"].ToString();
                discrep.PrintCount = row["PrintCount"].ToString();
                discrep.VendorNum = row["VendorNum"].ToString();
                discrep.VendorName = row["VendorName"].ToString();
                discrep.Status = row["Status"].ToString();

                list.Add(discrep);
            }
            DataTablesResult<SimpleDiscrepHeader> result = new DataTablesResult<SimpleDiscrepHeader>(param.Draw, recordsTotal, recordsTotal, list);
            return Json(result);
        }


        public JsonResult UpdateMRBProcess(string MRBNum, string JobNum, string ReworkQty, string ScrapQty,
            string DebitNote,string VendorRemark,string OperRemark,string Category,string Remark,
            string MRBReason, string ScrapPartNum)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string cmdText = string.Empty;
            DataTableCollection dtc;
            string MRBReasonCode = "", MRBReasonDesc = "";
            if (MRBReason.IndexOf('-') >= 0) {
                MRBReasonCode = MRBReason.Split('-')[0];
                MRBReasonDesc = MRBReason.Split('-')[1];
            }

            cmdText = string.Format(@"
                    update WebDiscrepHeader
                    set 
                        Status = '2',
                        ProcessId = '{1}',
                        ProcessTime = GETDATE(),
                        ReworkQty = {2},
                        ScrapQty = {3},
                        DebitNote = '{4}',
                        VendorRemark = N'{5}',
                        OperRemark = N'{6}',
                        Category = N'{7}',
                        Remark = N'{8}',
                        MRBReasonCode = '{9}',
                        MRBReasonDesc = N'{10}',
                        ScrapPartNum = '{11}'
                    where id = {0}
                ",
                MRBNum,
                user.UserName,
                ReworkQty,
                ScrapQty,
                DebitNote,
                VendorRemark,
                OperRemark,
                Category,
                Remark,
                MRBReasonCode,
                MRBReasonDesc,
                ScrapPartNum
            );
            LogHelper.Debug(cmdText);
            SqlHelper.ExecteNonQueryText(cmdText, null);

            cmdText = string.Format(@"
                    select OprSeq
                    from WebDiscrepHeader
                    where id = {0}
                ",
                MRBNum
            );
            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
            string discrepSeq = string.Empty;

            if (dtc[0].Rows.Count > 0)
            {
                discrepSeq = dtc[0].Rows[0]["OprSeq"].ToString();
            }

            //Get the Identity after the Record is saved
            var getResult = "";
            var getGid = "";
            if (!ReworkQty.Equals("0"))
            {
                cmdText = string.Format(@"
                        insert into WebReworkJobHead(Company,ParentJobNum,EpicorJobNum,PartNum,RevisionNum,DrawNum,PartDescription,ProdQty,IUM,DiscrepSeq,DiscrepId)
                        select top 1 Company,'{0}',EpicorJobNum,PartNum,RevisionNum,DrawNum,PartDescription,{1},IUM,{2},{3}
                        from WebJobHead
                        where JobNum = '{0}'
                        Select @@Identity
                    ",
                    JobNum,
                    ReworkQty,
                    discrepSeq,
                    MRBNum
                );
                //SqlHelper.ExecteNonQueryText(cmdText, null);
                getGid = Convert.ToString(SqlHelper.ExecuteScalar(CommandType.Text, cmdText));
                getResult = "OK";
            }

            SimpleResult result = new SimpleResult();
            result.Desc = getGid;
            result.Result = getResult;
            return Json(result);
        }

        public JsonResult UpdateMRBProcess_new(string MRBNum, string JobNum, string ReworkQty, string ScrapQty, string DebitNote, string VendorRemark, string OperRemark, string Category, string Remark)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            //Get the Identity after the Record is saved
            var getResult = "";
            var getGid = "";

            string connectionString = ConfigurationManager.AppSettings["CompartConn"];
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.Transaction = trans;

                    var cmdText = string.Format(@"
                            update WebDiscrepHeader
                            set 
                                Status = '2',
                                ProcessId = '{1}',
                                ProcessTime = GETDATE(),
                                ReworkQty = {2},
                                ScrapQty = {3},
                                DebitNote = '{4}',
                                VendorRemark = N'{5}',
                                OperRemark = N'{6}',
                                Category = N'{7}',
                                Remark = N'{8}'
                            where id = {0}
                        ",
                        MRBNum,
                        user.UserName,
                        ReworkQty,
                        ScrapQty,
                        DebitNote,
                        VendorRemark,
                        OperRemark,
                        Category,
                        Remark
                    );
                    LogHelper.Debug(cmdText);
                    cmd.CommandText = cmdText;
                    cmd.ExecuteNonQuery();

                    SqlDataReader drGetData;
                    SqlCommand cmdGetOprSeq = new SqlCommand();
                    cmdGetOprSeq.Connection = conn;
                    cmdGetOprSeq.Transaction = trans;
                    var strSQL = string.Format(@"
                        select OprSeq
                        from WebDiscrepHeader
                        where id = {0}",
                        MRBNum
                    );
                    cmdGetOprSeq.CommandText = strSQL;
                    drGetData = cmdGetOprSeq.ExecuteReader();

                    string discrepSeq = string.Empty;

                    if(drGetData.Read())
                    {
                        discrepSeq = drGetData["OprSeq"].ToString();

                    }
                    drGetData.Close();

                    if (!ReworkQty.Equals("0"))
                    {
                        SqlCommand sqlCommand = new SqlCommand();
                        sqlCommand.Connection = conn;
                        sqlCommand.Transaction = trans;

                        cmdText = string.Format(@"
                        insert into WebReworkJobHead(Company,ParentJobNum,EpicorJobNum,PartNum,RevisionNum,DrawNum,PartDescription,ProdQty,IUM,DiscrepSeq,DiscrepId)
                        select top 1 Company,'{0}',EpicorJobNum,PartNum,RevisionNum,DrawNum,PartDescription,{1},IUM,{2},{3}
                        from WebJobHead
                        where JobNum = '{0}'
                        Select @@Identity
                    ",
                            JobNum,
                            ReworkQty,
                            discrepSeq,
                            MRBNum
                        );
                        //SqlHelper.ExecteNonQueryText(cmdText, null);
                        sqlCommand.CommandText = cmdText;
                        getGid = Convert.ToString(sqlCommand.ExecuteScalar());
                    }
                    getResult = "OK";

                    trans.Commit();

                }
                catch(Exception ex)
                {
                    LogHelper.Error(ex);
                    getResult = "Fail";
                    trans.Rollback();
                }
            }
            SimpleResult result = new SimpleResult();
            result.Desc = getGid;
            result.Result = getResult;
            return Json(result);

        }

        //================================================== Quick MRB Scanning ==================================
        //==========
        //Adding the Quick MRB Scanning function
        public ActionResult MRBQuickHandlingView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("MRBQuickHandlingView"))
            {
                return Redirect("/Login/Home");
            }

            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            ViewBag.UserID = user.UserName;

            string cmdText = string.Empty;

            cmdText = string.Format(@"
                    select VendorID,Name
                    from Erp.Vendor
                    where Company = '{0}' 
                    order by VendorID
                ",
                ConfigurationManager.AppSettings["CompanyCode"]
            );
            DataTableCollection dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

            List<string> vendors = new List<string>();
            foreach (DataRow row in dtc[0].Rows)
            {
                vendors.Add(string.Format("{0}-{1}", row["VendorID"], row["Name"]));
            }

            //OP list
            cmdText = string.Format(@"
                            
                            select *
                            from erp.OpMaster
                            where Company = '{0}' 
                            order by opcode
                        ",
        ConfigurationManager.AppSettings["CompanyCode"]
    );

            DataTableCollection dtcWebJob = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

            List<SimpleOpMaster> list = new List<SimpleOpMaster>();
            foreach (DataRow row in dtcWebJob[0].Rows)
            {
                SimpleOpMaster op = new SimpleOpMaster();
                op.OpCode = row["OpCode"].ToString() + ":" + row["OpDesc"].ToString();
                op.OpDesc = row["OpDesc"].ToString();
                list.Add(op);
            }

            ViewBag.opList = list;
            ViewBag.Vendors = vendors;
            return View();
        }


        public JsonResult MRBQuickCheck(string MRBID)
        {

            var cmdText = string.Format(@"
                       select * from WebDiscrepHeader where ID='{0}'  
                    ",
                    MRBID
                );

            SqlDataReader drGetData= SqlHelper.ExecuteReader(CommandType.Text, cmdText);

            SimpleDiscrepHeader discrep = new SimpleDiscrepHeader();

            BaseResponse<SimpleDiscrepHeader> returnResponse = new BaseResponse<SimpleDiscrepHeader>();

            if (drGetData.Read())
            {

                discrep.id = drGetData["id"].ToString();
                discrep.JobNum = drGetData["JobNum"].ToString();
                discrep.OprSeq = drGetData["OprSeq"].ToString();
                discrep.OpCode = drGetData["OpCode"].ToString();
                discrep.OprDesc = drGetData["OprDesc"].ToString();
                discrep.Quantity = drGetData["Quantity"].ToString();
                discrep.ReasonCode = drGetData["ReasonCode"].ToString();
                discrep.ReasonDesc = drGetData["ReasonDesc"].ToString();
                discrep.PrintUserId = drGetData["PrintUserId"].ToString();
                discrep.PrintDateTime = drGetData["PrintDateTime"].ToString();
                discrep.PrintCount = drGetData["PrintCount"].ToString();
                discrep.VendorNum = drGetData["VendorNum"].ToString();
                discrep.VendorName = drGetData["VendorName"].ToString();
                discrep.Status = drGetData["Status"].ToString();

                var getStatus = drGetData["Status"].ToString();
                if (getStatus == "1")
                {
                    returnResponse.MsgCode = "OK";
                }
                else if(getStatus=="9")
                {
                    returnResponse.MsgCode = "NotReceipt";
                }
                else 
                {
                    returnResponse.MsgCode = "Handled";
                }
                returnResponse.Data = discrep;
            }
            else
            {
                discrep = null;
                returnResponse.MsgCode = "NoData";
                returnResponse.Data = null;

            }
            drGetData.Close();

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }


        //After the MRB disposition, gettting the info from ReworkJobRead for creating the Rework process
        public JsonResult GetFromReworkJobHead(string JobNum)
        {

            var cmdText = string.Format(@"
                       select * from WebDiscrepHeader where JobNum='{0}'  and Status <> '9'  and Status <> '0' 
                    ",
                    JobNum
                );

            SqlDataReader drGetData = SqlHelper.ExecuteReader(CommandType.Text, cmdText);

            SimpleDiscrepHeader discrep = new SimpleDiscrepHeader();

            BaseResponse<SimpleDiscrepHeader> returnResponse = new BaseResponse<SimpleDiscrepHeader>();

            if (drGetData.Read())
            {

                discrep.id = drGetData["id"].ToString();
                discrep.JobNum = drGetData["JobNum"].ToString();
                discrep.OprSeq = drGetData["OprSeq"].ToString();
                discrep.OpCode = drGetData["OpCode"].ToString();
                discrep.OprDesc = drGetData["OprDesc"].ToString();
                discrep.Quantity = drGetData["Quantity"].ToString();
                discrep.ReasonCode = drGetData["ReasonCode"].ToString();
                discrep.ReasonDesc = drGetData["ReasonDesc"].ToString();
                discrep.PrintUserId = drGetData["PrintUserId"].ToString();
                discrep.PrintDateTime = drGetData["PrintDateTime"].ToString();
                discrep.PrintCount = drGetData["PrintCount"].ToString();
                discrep.VendorNum = drGetData["VendorNum"].ToString();
                discrep.VendorName = drGetData["VendorName"].ToString();
                discrep.Status = drGetData["Status"].ToString();

                returnResponse.MsgCode = "OK";
                returnResponse.Data = discrep;
            }
            else
            {
                discrep = null;
                returnResponse.MsgCode = "NoData";
                returnResponse.Data = null;

            }
            drGetData.Close();

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }
        //===========
        //================================================== Quick MRB Scanning Ends ==================================

    }
}