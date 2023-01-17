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
    public class SubcontractController : BaseController
    {
        private static readonly object objOSS = new object();
        private static readonly object objOSR = new object();

        [HttpGet]
        public JsonResult SyncSubconShipment()
        {
            LogHelper.Info("Start OSShp Sync");

            try
            {
                    string cmdText = string.Format(@"
                        select PackNum
                        from WebSubconShipment
                        where Sync = 1
                        group by PackNum
                    "
                );
                DataTableCollection dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                foreach (DataRow row in dtc[0].Rows)
                {
                    JobHelper.SyncSubconShipment(row["PackNum"].ToString());
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                LogHelper.Info("End OSShp Sync");
            }
            
            
            return Json("Done", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SyncSubconReceipt()
        {
            string cmdText = string.Format(@"
                    select PackNum
                    from WebSubconReceipt
                    where Sync = 1
                    group by PackNum
                "
            );
            DataTableCollection dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
            foreach (DataRow row in dtc[0].Rows)
            {
                JobHelper.SyncSubconReceipt(row["PackNum"].ToString());
            }
            
            return Json("Done", JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShipmentPrintView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            ViewBag.UserID = user.UserName;
            return View();
        }

        public ActionResult ShipmentView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("ShipmentView"))
            {
                return Redirect("/Login/Home");
            }

            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            ViewBag.UserID = user.UserName;
            return View();
        }

        public ActionResult ReceiptPrintView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            ViewBag.UserID = user.UserName;
            return View();
        }

        public ActionResult ReceiptView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("ReceiptView"))
            {
                return Redirect("/Login/Home");
            }

            ViewBag.PrintServiceUrl = ConfigurationManager.AppSettings["PrintServiceUrl"].ToString();
            ViewBag.UserID = user.UserName;
            return View();
        }

        public JsonResult GetShipmentist(DataTablesParameters param)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            LogHelper.Debug("GetShipmentist Started");
            string filter = string.Empty;
            string searchby = string.Empty;


            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                if (param.Search.Value.Equals(GetResValue("Txt_Unconfirmed")))
                {
                    filter = " and Sync = '0' ";
                }
                else if (param.Search.Value.Equals(GetResValue("Txt_Confirmed")))
                {
                    filter = " and Sync <> '0' ";
                }
                else
                {
                    filter = string.Format(" and (PackNum like '%{0}%' or JobNum like '%{0}%' or OprSeq like '%{0}%' or VendorNum like '%{0}%') ", param.Search.Value);
                }
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and PackNum like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[1].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                //filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[1].Search.Value);
                if (param.Columns[2].Search.Value.Equals(GetResValue("Txt_Unconfirmed")))
                {
                    filter = " and Sync = '0' ";
                }else if (param.Columns[2].Search.Value.Equals(GetResValue("Txt_Confirmed")))
                {
                    filter = " and Sync <> '0' ";
                }
            }

            if (!user.UserRole.Equals("1"))
            {
                filter += string.Format(" and EntryPerson = '{0}'", user.UserName);
            }

            string cmdText = string.Format(@"
                    select count(*) as recordsTotal
                    from WebSubconShipment
                    where Company = '{0}' {1}
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter,
                user.UserName
            );
            DataTableCollection dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtc[0].Rows[0]["recordsTotal"].ToString());

            cmdText = string.Format(@"
                        select top {1} *
                        from (
                            select ROW_NUMBER() over(order by {3} {4}) as rownum,*
                            from(
                                select SS.*,JO.OpCode,JO.OpDesc,JH.PartNum
                                from WebSubconShipment as SS
                                left join WebJobOper as JO on SS.Company = JO.Company and SS.JobNum = JO.JobNum and SS.OprSeq = JO.OprSeq
                                left join WebJobHead as JH on JH.Company = JO.Company and JH.JobNum = JO.JobNum
                            )as WebSubconShipment
                            where Company = '{0}' {5}
                        ) as Shipment
                        where rownum > {2} and  Shipment.Company = '{0}' {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter,                                             //{5}   filter
                    user.UserName
                );

            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
            LogHelper.Debug(cmdText);

            List<SimpleSubconShipment> list = new List<SimpleSubconShipment>();
            foreach(DataRow row in dtc[0].Rows)
            {
                SimpleSubconShipment shipment = new SimpleSubconShipment();

                shipment.id = row["id"].ToString();
                shipment.JobNum = row["JobNum"].ToString();
                //shipment.OprSeq = row["OprSeq"].ToString();
                shipment.OprSeq = row["OpCode"].ToString() + ":" + row["OpDesc"].ToString();
                shipment.PackNum = row["PackNum"].ToString();
                shipment.PartNum = row["PartNum"].ToString(); ;
                //shipment.VendorNum = JobHelper.GetVendorNameByNum(row["VendorNum"].ToString());
                shipment.VendorNum = row["VendorName"].ToString();
                shipment.Qty = row["Qty"].ToString();
                shipment.EntryPerson = row["EntryPerson"].ToString();
                shipment.EntryTime = row["EntryTime"].ToString();
                shipment.PrintUserId = row["PrintUserId"].ToString();
                shipment.PrintDateTime = row["PrintDateTime"].ToString();
                shipment.PrintCount = row["PrintCount"].ToString();
                shipment.Sync = row["Sync"].ToString();

                list.Add(shipment);
            }
            DataTablesResult<SimpleSubconShipment> result = new DataTablesResult<SimpleSubconShipment>(param.Draw, recordsTotal, recordsTotal, list);
            LogHelper.Debug("GetShipmentist End");
            return Json(result);
        }

        public JsonResult GetShipmentPrintList(DataTablesParameters param)
        {
            string filter = string.Empty;
            string searchby = string.Empty;


            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                filter = string.Format(" and (PackNum like '%{0}%' or JobNum like '%{0}%' or OprSeq like '%{0}%' or VendorNum like '%{0}%') ", param.Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and PackNum like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[1].Search.Value);
            }

            string cmdText = string.Format(@"
                    select count(*) as recordsTotal
                    from WebSubconShipment
                    where Company = '{0}' and (Sync = '1' or Sync = '2') {1}
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
                            from(
                                select SS.*,JO.OpCode,JO.OpDesc
                                from WebSubconShipment as SS
                                left join WebJobOper as JO on SS.Company = JO.Company and SS.JobNum = JO.JobNum and SS.OprSeq = JO.OprSeq
                                left join WebJobHead as JH on JH.Company = JO.Company and JH.JobNum = JO.JobNum
                            )as WebSubconShipment
                            where Company = '{0}' and (Sync = '1' or Sync = '2') {5}
                        ) as Shipment
                        where rownum > {2} and  Shipment.Company = '{0}' and (Sync = '1' or Sync = '2') {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );

            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleSubconShipment> list = new List<SimpleSubconShipment>();
            foreach (DataRow row in dtc[0].Rows)
            {
                SimpleSubconShipment shipment = new SimpleSubconShipment();

                shipment.id = row["id"].ToString();
                shipment.JobNum = row["JobNum"].ToString();
                //shipment.OprSeq = row["OprSeq"].ToString();
                shipment.OprSeq = row["OpCode"].ToString() + ":" + row["OpDesc"].ToString();
                shipment.PackNum = row["PackNum"].ToString();
                shipment.PartNum = "";
                shipment.VendorNum = JobHelper.GetVendorNameByNum(row["VendorNum"].ToString());
                shipment.Qty = row["Qty"].ToString();
                shipment.EntryPerson = row["EntryPerson"].ToString();
                shipment.EntryTime = row["EntryTime"].ToString();

                list.Add(shipment);
            }
            DataTablesResult<SimpleSubconShipment> result = new DataTablesResult<SimpleSubconShipment>(param.Draw, recordsTotal, recordsTotal, list);
            return Json(result);
        }

        public JsonResult ConfirmShipment(List<string> jobList)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string cmdText = string.Empty;
            DataTableCollection dtc;

            string ids = string.Empty;
            foreach (string id in jobList)
            {
                ids += id + ",";
            }

            if (ids.Length > 0)
            {
                ids = ids.Substring(0, ids.Length - 1);
                try
                {
                    //将外包发货记录的置为可同步 Sync = 1
                    /*
                    cmdText = string.Format(@"
                            update WebSubconShipment
                            set Sync = '1'
                            where id in ({0}) 
                        ",
                        ids
                    );
                    SqlHelper.ExecteNonQueryText(cmdText, null);
                    */

                    //更新JobStatus = SHIPPED
                    /*
                    cmdText = string.Format(@"
                            update JO
                            set JobStatus = 'SHIPPED'
                            from WebJobOper as JO
                            left join WebSubconShipment as SS on JO.Company = SS.Company and JO.JobNum = SS.JobNum and JO.OprSeq = ss.OprSeq
                            where SS.id in ({0})
                        ",
                        ids
                    );
                    SqlHelper.ExecteNonQueryText(cmdText, null);
                    */
                    //更新WebSubconShipment
                    foreach (string id in jobList)
                    {
                        string jobnum = string.Empty;
                        string oprseq = string.Empty;
                        string qty = string.Empty;
                        string packNum = string.Empty;

                        
                        cmdText = string.Format(@"
                                select JobNum,OprSeq,Qty,PackNum
                                from WebSubconShipment
                                where id = {0}
                            ",
                            id
                        );
                        dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                        if (dtc[0].Rows.Count > 0)
                        {
                            jobnum = dtc[0].Rows[0]["JobNum"].ToString();
                            oprseq = dtc[0].Rows[0]["OprSeq"].ToString();
                            qty = dtc[0].Rows[0]["Qty"].ToString();
                            packNum = dtc[0].Rows[0]["PackNum"].ToString();

                            //JobHelper.TransferWIP(jobnum, oprseq, "OS-PICKED", oprseq, "OS-SHIPPED", qty);
                            cmdText = string.Format(@"                    
                                    select top 1 *
                                    from WebTransaction
                                    where JobNum = '{1}'
                                    order by id desc
                                ",
                                ConfigurationManager.AppSettings["CompanyCode"],
                                jobnum
                            );
                            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                            if (dtc[0].Rows.Count > 0)
                            {
                                DataRow row = dtc[0].Rows[0];

                                #region 欢乐锁逻辑
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
                                        #region 开始更新前获取Version
                                        int version = 0;
                                        cmdText = string.Format(@"
                                                select Version
                                                from WebJobOper
                                                where Company = '{0}' and JobNum = '{1}' and OprSeq = {2}
                                            ",
                                            ConfigurationManager.AppSettings["CompanyCode"],
                                            jobnum,                             //5:JobNum
                                            row["OprSeq"].ToString()
                                        );
                                        //DataTableCollection dtcPara = SqlHelper.GetTable(CommandType.Text, cmdTextPara, null);
                                        cmd.CommandText = cmdText;
                                        adapter = new SqlDataAdapter();
                                        adapter.SelectCommand = cmd;
                                        ds = new DataSet();
                                        adapter.Fill(ds);
                                        DataTableCollection dtcPara = ds.Tables;

                                        if (dtcPara[0].Rows.Count > 0)
                                        {
                                            int.TryParse(dtcPara[0].Rows[0]["Version"].ToString(), out version);
                                        }
                                        #endregion

                                        
                                        #region 添加WebTransaction
                                        string cmdTextValidation = string.Format(@"
                                                select ISNULL(TransType,'') as TransType
                                                from WebTransaction
                                                where JobNum = '{0}'
                                                order by id desc 
                                            ",
                                            jobnum
                                        );
                                        DataTableCollection dtcValidation = SqlHelper.GetTable(CommandType.Text, cmdTextValidation, null);

                                        if (!(dtcValidation[0].Rows.Count > 0 && dtcValidation[0].Rows[0]["TransType"].ToString().Equals("OSSHIPMENTCONFIRM")))
                                        {
                                            cmdText = string.Format(@"
                                                    insert into WebTransaction(Company,TransType,UserId,MachineId,PDAId,JobNum,OprSeq,OpCode,OpGroup,OprSeqNext,OpCodeNext,OpGroupNext,FromLocation,ToLocation,OprQty,ReportingQty,LaborQty,ScrapQty,DiscrepQty)
                                                    values('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}','{13}',{14},{15},{16},{17},{18})
                                                ",
                                                ConfigurationManager.AppSettings["CompanyCode"],            //0:Company
                                                "OSSHIPMENTCONFIRM",                //1:TransType
                                                user.UserName,            //2:UserId
                                                "WEB",                              //3:MachineId
                                                "WEB",                              //4:PDAId
                                                jobnum,                             //5:JobNum
                                                row["OprSeq"].ToString(),           //6:OprSeq
                                                row["OpCode"].ToString(),           //7:OpCode
                                                row["OpGroup"].ToString(),          //8:OpGroup
                                                row["OprSeqNext"].ToString(),       //9:OprSeqNext
                                                row["OpCodeNext"].ToString(),       //10:OpCodeNext
                                                row["OpGroupNext"].ToString(),      //11:OpGroupNext
                                                "",                                 //12:FromLocation
                                                "",                                 //13:ToLocation
                                                row["OprQty"].ToString(),           //14:OprQty
                                                row["ReportingQty"].ToString(),     //15:ReportingQty
                                                row["LaborQty"].ToString(),         //16:LaborQty
                                                row["ScrapQty"].ToString(),         //17:ScrapQty
                                                row["DiscrepQty"].ToString()        //18:DiscrepQty
                                            );
                                            //SqlHelper.ExecteNonQueryText(cmdText, null);
                                            cmd.CommandText = cmdText;
                                            cmd.ExecuteNonQuery();
                                        }
                                        #endregion

                                        cmdText = string.Format(@"
                                                update WebSubconShipment
                                                set Sync = '1'
                                                where id = {0} 
                                            ",
                                            id
                                        );
                                        cmd.CommandText = cmdText;
                                        cmd.ExecuteNonQuery();

                                        #region 更新结束后，判断换了锁
                                        cmdText = string.Format(@"
                                            update WebJobOper
                                            set Version = Version + 1
                                            where Company = '{0}' and JobNum = '{1}' and OprSeq = {2} and Version = {3}
                                        ",
                                            ConfigurationManager.AppSettings["CompanyCode"],
                                            jobnum,                             
                                            row["OprSeq"].ToString(),
                                            version
                                        );
                                        cmd.CommandText = cmdText;
                                        int valTrans = cmd.ExecuteNonQuery();

                                        if (valTrans > 0)
                                        {
                                            trans.Commit();
                                        }
                                        else
                                        {
                                            trans.Rollback();
                                        }
                                        #endregion


                                    }
                                    catch
                                    {
                                        trans.Rollback();
                                    }
                                }
                                #endregion

                                #region 原来的逻辑
                                if (1==0)
                                {
                                    string cmdTextValidation = string.Format(@"
                                            select ISNULL(TransType,'') as TransType
                                            from WebTransaction
                                            where JobNum = '{0}'
                                            order by id desc 
                                        ",
                                        jobnum
                                    );
                                    DataTableCollection dtcValidation = SqlHelper.GetTable(CommandType.Text, cmdTextValidation, null);

                                    if (!(dtcValidation[0].Rows.Count > 0 && dtcValidation[0].Rows[0]["TransType"].ToString().Equals("OSSHIPMENTCONFIRM")))
                                    {
                                        cmdText = string.Format(@"
                                                insert into WebTransaction(Company,TransType,UserId,MachineId,PDAId,JobNum,OprSeq,OpCode,OpGroup,OprSeqNext,OpCodeNext,OpGroupNext,FromLocation,ToLocation,OprQty,ReportingQty,LaborQty,ScrapQty,DiscrepQty)
                                                values('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}','{13}',{14},{15},{16},{17},{18})
                                            ",
                                            ConfigurationManager.AppSettings["CompanyCode"],            //0:Company
                                            "OSSHIPMENTCONFIRM",                //1:TransType
                                            user.UserName,            //2:UserId
                                            "WEB",                              //3:MachineId
                                            "WEB",                              //4:PDAId
                                            jobnum,                             //5:JobNum
                                            row["OprSeq"].ToString(),           //6:OprSeq
                                            row["OpCode"].ToString(),           //7:OpCode
                                            row["OpGroup"].ToString(),          //8:OpGroup
                                            row["OprSeqNext"].ToString(),       //9:OprSeqNext
                                            row["OpCodeNext"].ToString(),       //10:OpCodeNext
                                            row["OpGroupNext"].ToString(),      //11:OpGroupNext
                                            "",                                 //12:FromLocation
                                            "",                                 //13:ToLocation
                                            row["OprQty"].ToString(),           //14:OprQty
                                            row["ReportingQty"].ToString(),     //15:ReportingQty
                                            row["LaborQty"].ToString(),         //16:LaborQty
                                            row["ScrapQty"].ToString(),         //17:ScrapQty
                                            row["DiscrepQty"].ToString()        //18:DiscrepQty
                                        );
                                        SqlHelper.ExecteNonQueryText(cmdText, null);
                                    }
                                }
                                #endregion

                            }

                        }


                        JobHelper.SyncSubconShipment(packNum);
                    }
                    

                }
                catch(Exception ex)
                {

                }
            }
            

            SimpleResult result = new SimpleResult();
            return Json(result);
        }

        public JsonResult GetReceiptist(DataTablesParameters param)
        {
            string filter = string.Empty;
            string searchby = string.Empty;
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                if (param.Search.Value.Equals(GetResValue("Txt_Unconfirmed")))
                {
                    filter = " and Sync = '0' ";
                }
                else if (param.Search.Value.Equals(GetResValue("Txt_Confirmed")))
                {
                    filter = " and Sync <> '0' ";
                }
                else
                {
                    filter = string.Format(" and (PackNum like '%{0}%' or JobNum like '%{0}%' or OprSeq like '%{0}%' or PONum like '%{0}%') ", param.Search.Value);
                }
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and PackNum like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[1].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                if (param.Columns[2].Search.Value.Equals(GetResValue("Txt_Unconfirmed")))
                {
                    filter = " and Sync = '0' ";
                }
                else if (param.Columns[2].Search.Value.Equals(GetResValue("Txt_Confirmed")))
                {
                    filter = " and Sync <> '0' ";
                }
            }

            string cmdText = string.Format(@"
                    select count(*) as recordsTotal
                    from WebSubconReceipt
                    where Company = '{0}' {1}
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter,
                user.UserName
            );
            DataTableCollection dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtc[0].Rows[0]["recordsTotal"].ToString());

            cmdText = string.Format(@"
                        select top {1} *
                        from (
                            select ROW_NUMBER() over(order by {3} {4}) as rownum,*
                            from (
                                select SR.*,JO.OpCode,JO.OpDesc as OprDesc,JH.PartNum
                                from WebSubconReceipt as SR
                                left join WebJobOper as JO on SR.Company = JO.Company and SR.JobNum = JO.JobNum and SR.OprSeq = JO.OprSeq     
                                left join WebJobHead as JH on JH.Company = JO.Company and JH.JobNum = JO.JobNum      
                            ) as WebSubconReceipt
                            where Company = '{0}' {5}
                        ) as Receipt
                        where rownum > {2} and  Receipt.Company = '{0}' {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy == "" ? "Sync" : param.OrderBy,          //{3}   OrderBy Column
                    param.OrderBy == "" ? DataTablesOrderDir.Asc : param.OrderDir,       //{4}   OrderBy Direction ASC/DESC
                    filter,                                             //{5}   filter
                    user.UserName
                );

            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
            LogHelper.Debug(cmdText);

            List<SimpleSubconReceipt> list = new List<SimpleSubconReceipt>();
            foreach (DataRow row in dtc[0].Rows)
            {
                SimpleSubconReceipt receipt = new SimpleSubconReceipt();

                receipt.id = row["id"].ToString();
                receipt.JobNum = row["JobNum"].ToString();
                receipt.OprSeq = row["OprSeq"].ToString();
                receipt.OpCode = row["OpCode"].ToString();
                receipt.OprDesc = row["OprDesc"].ToString();
                receipt.PackNum = row["PackNum"].ToString();
                receipt.PartNum = row["PartNum"].ToString();
                receipt.PONum = row["PONum"].ToString();
                receipt.POLine = row["POLine"].ToString();
                receipt.PORelNum = row["PORelNum"].ToString();
                //receipt.VendorName = JobHelper.GetVendorNameByPONum(row["PONum"].ToString());
                receipt.VendorName = row["VendorName"].ToString();
                receipt.Qty = row["Qty"].ToString();
                receipt.EntryPerson = row["EntryPerson"].ToString();
                receipt.EntryTime = row["EntryTime"].ToString();
                receipt.PrintUserId = row["PrintUserId"].ToString();
                receipt.PrintDateTime = row["PrintDateTime"].ToString() == "" ? "" : Convert.ToDateTime(row["PrintDateTime"]).ToString("yyyy-MM-dd");
                receipt.PrintCount = row["PrintCount"].ToString();
                receipt.Sync = row["Sync"].ToString();

                list.Add(receipt);
            }
            DataTablesResult<SimpleSubconReceipt> result = new DataTablesResult<SimpleSubconReceipt>(param.Draw, recordsTotal, recordsTotal, list);
            return Json(result);
        }

        public JsonResult GetReceiptPrintList(DataTablesParameters param)
        {
            string filter = string.Empty;
            string searchby = string.Empty;


            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                filter = string.Format(" and (PackNum like '%{0}%' or JobNum like '%{0}%' or OprSeq like '%{0}%' or PONum like '%{0}%') ", param.Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and PackNum like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = string.Format(" and JobNum like '%{0}%' ", param.Columns[1].Search.Value);
            }

            string cmdText = string.Format(@"
                    select count(*) as recordsTotal
                    from WebSubconReceipt
                    where Company = '{0}' and (Sync = '1' or Sync = '2') {1}
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
                            from (
                                select SR.*,JO.OpCode,JO.OpDesc as OprDesc
                                from WebSubconReceipt as SR
                                left join WebJobOper as JO on SR.Company = JO.Company and SR.JobNum = JO.JobNum and SR.OprSeq = JO.OprSeq                        
                            ) as WebSubconReceipt
                            where Company = '{0}' and (Sync = '1' or Sync = '2') {5}
                        ) as Receipt
                        where rownum > {2} and  Receipt.Company = '{0}' and (Receipt.Sync = '1' or Receipt.Sync = '2') {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );

            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleSubconReceipt> list = new List<SimpleSubconReceipt>();
            foreach (DataRow row in dtc[0].Rows)
            {
                SimpleSubconReceipt receipt = new SimpleSubconReceipt();

                receipt.id = row["id"].ToString();
                receipt.JobNum = row["JobNum"].ToString();
                receipt.OprSeq = row["OprSeq"].ToString();
                receipt.OpCode = row["OpCode"].ToString();
                receipt.OprDesc = row["OprDesc"].ToString();
                receipt.PackNum = row["PackNum"].ToString();
                receipt.PartNum = "";
                receipt.PONum = row["PONum"].ToString();
                receipt.POLine = row["POLine"].ToString();
                receipt.PORelNum = row["PORelNum"].ToString();
                receipt.VendorName = JobHelper.GetVendorNameByPONum(row["PONum"].ToString());
                receipt.Qty = row["Qty"].ToString();
                receipt.EntryPerson = row["EntryPerson"].ToString();
                receipt.EntryTime = row["EntryTime"].ToString();

                list.Add(receipt);
            }
            DataTablesResult<SimpleSubconReceipt> result = new DataTablesResult<SimpleSubconReceipt>(param.Draw, recordsTotal, recordsTotal, list);
            return Json(result);
        }

        /// <summary>
        /// 已作废
        /// </summary>
        /// <param name="jobList"></param>
        /// <returns></returns>
        public JsonResult ConfirmReceiptDiscard(List<string> jobList)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            List<string> packList = new List<string>();

            string cmdText = string.Empty;
            DataTableCollection dtc;

            string ids = string.Empty;
            foreach (string id in jobList)
            {
                ids += id + ",";
            }

            if (ids.Length > 0)
            {
                ids = ids.Substring(0, ids.Length - 1);
                try
                {
                    //将外包发货记录的置为可同步 Sync = 1
                    cmdText = string.Format(@"
                            update WebSubconReceipt
                            set Sync = '1'
                            where id in ({0}) 
                        ",
                        ids
                    );
                    SqlHelper.ExecteNonQueryText(cmdText, null);

                    //更新JobStatus = TRANSFERRED
                    /*
                    cmdText = string.Format(@"
                            update JO
                            set JobStatus = 'TRANSFERRED'
                            from WebJobOper as JO
                            left join WebSubconReceipt as SR on JO.Company = SR.Company and JO.JobNum = SR.JobNum and JO.OprSeq = SR.OprSeq
                            where SR.id in ({0})
                        ",
                        ids
                    );
                    SqlHelper.ExecteNonQueryText(cmdText, null);
                    */

                    //更新WebPartWIP
                    foreach (string id in jobList)
                    {
                        string jobnum = string.Empty;
                        string packNum = string.Empty;

                        cmdText = string.Format(@"
                                select JobNum,PackNum
                                from WebSubconReceipt
                                where id = {0}
                            ",
                            id
                        );
                        dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                        if (dtc[0].Rows.Count > 0)
                        {
                            jobnum = dtc[0].Rows[0]["JobNum"].ToString();
                            packNum = dtc[0].Rows[0]["PackNum"].ToString();

                            cmdText = string.Format(@"                    
                                    select top 1 *
                                    from WebTransaction
                                    where JobNum = '{1}'
                                    order by id desc
                                ",
                                ConfigurationManager.AppSettings["CompanyCode"],
                                jobnum
                            );
                            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                            if (dtc[0].Rows.Count > 0)
                            {
                                DataRow row = dtc[0].Rows[0];

                                cmdText = string.Format(@"
                                        insert into WebTransaction(Company,TransType,UserId,MachineId,PDAId,JobNum,OprSeq,OpCode,OpGroup,OprSeqNext,OpCodeNext,OpGroupNext,FromLocation,ToLocation,OprQty,ReportingQty,LaborQty,ScrapQty,DiscrepQty)
                                        values('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}','{13}',{14},{15},{16},{17},{18})
                                    ",
                                    ConfigurationManager.AppSettings["CompanyCode"],            //0:Company
                                    "OSRECEIPTCONFIRM",                //1:TransType
                                    user.UserName,            //2:UserId
                                    "WEB",                              //3:MachineId
                                    "WEB",                              //4:PDAId
                                    jobnum,                             //5:JobNum
                                    row["OprSeq"].ToString(),           //6:OprSeq
                                    row["OpCode"].ToString(),           //7:OpCode
                                    row["OpGroup"].ToString(),          //8:OpGroup
                                    row["OprSeqNext"].ToString(),       //9:OprSeqNext
                                    row["OpCodeNext"].ToString(),       //10:OpCodeNext
                                    row["OpGroupNext"].ToString(),      //11:OpGroupNext
                                    "",                                 //12:FromLocation
                                    "",                                 //13:ToLocation
                                    row["OprQty"].ToString(),           //14:OprQty
                                    row["ReportingQty"].ToString(),     //15:ReportingQty
                                    row["LaborQty"].ToString(),         //16:LaborQty
                                    row["ScrapQty"].ToString(),         //17:ScrapQty
                                    row["DiscrepQty"].ToString()        //18:DiscrepQty
                                );
                                SqlHelper.ExecteNonQueryText(cmdText, null);
                            }

                        }

                        
                    }

                    //JobHelper.SyncSubconReceipt("");


                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }


            SimpleResult result = new SimpleResult();
            return Json(result);
        }


        public JsonResult ConfirmReceipt(List<string> jobList)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            SimpleResult result = new SimpleResult();
            result.Desc = string.Empty;
            
            try
            {
                foreach (string id in jobList)
                {
                    string packNum = string.Empty;
                    string cmdText = string.Empty;
                    DataTableCollection dtc;

                    string poNum = string.Empty;
                    string vendorNum = string.Empty;
                    string vendorName = string.Empty;
                    string JobNum = string.Empty;
                    string OprSeq = string.Empty;

                    string LaborQty = "0";
                    string SplitQty = "0";
                    string ReworkQty = "0";
                    string ScrapQty = "0";
                    string DebitNote = "0";
                    string ReturnToVendor = "0";

                    #region 获取VendorNum和VendorName
                    cmdText = string.Format(@"
                            select PONum,JobNum,OprSeq,Qty
                            from WebSubconReceipt
                            where id = {0}
                        ",
                        id
                    );

                    dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                    if (dtc[0].Rows.Count > 0)
                    {
                        poNum = dtc[0].Rows[0]["PONum"].ToString();
                        JobNum = dtc[0].Rows[0]["JobNum"].ToString();
                        OprSeq = dtc[0].Rows[0]["OprSeq"].ToString();
                        LaborQty = dtc[0].Rows[0]["Qty"].ToString();
                    }
                    else
                    {
                        continue;
                    }

                    cmdText = string.Format(@"
                            select V.VendorNum,V.Name
                            from Erp.Vendor as V
                            left join Erp.POHeader as PH on V.Company = PH.Company and PH.VendorNum = V.VendorNum
                            where V.Company = '{0}' and PH.PONum = {1}
                        ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        poNum
                    );
                    dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);
                    if (dtc[0].Rows.Count > 0)
                    {
                        vendorNum = dtc[0].Rows[0]["VendorNum"].ToString();
                        vendorName = dtc[0].Rows[0]["Name"].ToString();
                    }
                    #endregion

                    string jobnum = string.Empty;

                    cmdText = string.Format(@"
                            select JobNum,PackNum
                            from WebSubconReceipt
                            where id = {0}
                        ",
                        id
                    );
                    dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                    if (dtc[0].Rows.Count > 0)
                    {
                        jobnum = dtc[0].Rows[0]["JobNum"].ToString();
                        packNum = dtc[0].Rows[0]["PackNum"].ToString();

                        cmdText = string.Format(@"                    
                                    select top 1 *
                                    from WebTransaction
                                    where JobNum = '{1}'
                                    order by id desc
                                ",
                            ConfigurationManager.AppSettings["CompanyCode"],
                            jobnum
                        );
                        dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                        if (dtc[0].Rows.Count > 0)
                        {
                            DataRow row = dtc[0].Rows[0];

                            #region 欢乐锁逻辑
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
                                    #region 开始更新前获取Version
                                    int version = 0;
                                    cmdText = string.Format(@"
                                            select Version
                                            from WebJobOper
                                            where Company = '{0}' and JobNum = '{1}' and OprSeq = {2}
                                        ",
                                        ConfigurationManager.AppSettings["CompanyCode"],
                                        JobNum,
                                        OprSeq
                                    );
                                    //DataTableCollection dtcPara = SqlHelper.GetTable(CommandType.Text, cmdTextPara, null);
                                    cmd.CommandText = cmdText;
                                    adapter = new SqlDataAdapter();
                                    adapter.SelectCommand = cmd;
                                    ds = new DataSet();
                                    adapter.Fill(ds);
                                    DataTableCollection dtcPara = ds.Tables;

                                    if (dtcPara[0].Rows.Count > 0)
                                    {
                                        int.TryParse(dtcPara[0].Rows[0]["Version"].ToString(), out version);
                                    }
                                    #endregion


                                    //将外包发货记录的置为可同步 Sync = 1
                                    cmdText = string.Format(@"
                                    update WebSubconReceipt
                                            set Sync = '1',
                                            LaborQty = {1},
                                            ScrapQty = {2},
                                            OSReworkQty = {3},
                                            InternalReworkQty = {4},
                                            ReturnToVendor = {5}
                                            where Sync = '0' and id = {0} 
                                        ",
                                        id,
                                        LaborQty,
                                        ScrapQty,
                                        SplitQty,
                                        ReworkQty,
                                        ReturnToVendor
                                    );
                                    cmd.CommandText = cmdText;
                                    cmd.ExecuteNonQuery();
                                    LogHelper.Info("Batch OS Receipt: " + cmdText);

                                    #region 添加WebTransaction记录
                                    string cmdTextValidation = string.Format(@"
                                            select ISNULL(TransType,'') as TransType
                                            from WebTransaction
                                            where JobNum = '{0}'
                                            order by id desc 
                                        ",
                                        JobNum
                                    );
                                    DataTableCollection dtcValidation = SqlHelper.GetTable(CommandType.Text, cmdTextValidation, null);

                                    //插入到WebTransaction之前需要判断一下该工单的最后一条记录必须不是外包收货确认（OSRECEIPTCONFIRM）
                                    if (!(dtcValidation[0].Rows.Count > 0 && dtcValidation[0].Rows[0]["TransType"].ToString().Equals("OSRECEIPTCONFIRM")))
                                    {
                                        cmdText = string.Format(@"
                                                insert into WebTransaction(Company,TransType,UserId,MachineId,PDAId,JobNum,OprSeq,OpCode,OpGroup,OprSeqNext,OpCodeNext,OpGroupNext,FromLocation,ToLocation,OprQty,ReportingQty,LaborQty,ScrapQty,DiscrepQty)
                                                values('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}','{13}',{14},{15},{16},{17},{18})
                                            ",
                                            ConfigurationManager.AppSettings["CompanyCode"],            //0:Company
                                            "OSRECEIPTCONFIRM",                //1:TransType
                                            user.UserName,            //2:UserId
                                            "WEB",                              //3:MachineId
                                            "WEB",                              //4:PDAId
                                            jobnum,                             //5:JobNum
                                            row["OprSeq"].ToString(),           //6:OprSeq
                                            row["OpCode"].ToString(),           //7:OpCode
                                            row["OpGroup"].ToString(),          //8:OpGroup
                                            row["OprSeqNext"].ToString(),       //9:OprSeqNext
                                            row["OpCodeNext"].ToString(),       //10:OpCodeNext
                                            row["OpGroupNext"].ToString(),      //11:OpGroupNext
                                            "",                                 //12:FromLocation
                                            "",                                 //13:ToLocation
                                            row["OprQty"].ToString(),           //14:OprQty
                                            LaborQty,                           //15:ReportingQty
                                            LaborQty,                           //16:LaborQty
                                            row["ScrapQty"].ToString(),         //17:ScrapQty
                                            ReworkQty                           //18:DiscrepQty
                                        );
                                        //SqlHelper.ExecteNonQueryText(cmdText, null);
                                        cmd.CommandText = cmdText;
                                        int val = cmd.ExecuteNonQuery();
                                    }

                                    #endregion



                                    #region 更新结束后，判断换了锁
                                    cmdText = string.Format(@"
                                            update WebJobOper
                                            set Version = Version + 1
                                            where Company = '{0}' and JobNum = '{1}' and OprSeq = {2} and Version = {3}
                                        ",
                                        ConfigurationManager.AppSettings["CompanyCode"],
                                        JobNum,
                                        OprSeq,
                                        version
                                    );
                                    cmd.CommandText = cmdText;
                                    int valTrans = cmd.ExecuteNonQuery();

                                    if (valTrans > 0)
                                    {
                                        trans.Commit();

                                    }
                                    else
                                    {
                                        trans.Rollback();
                                    }
                                    #endregion
                                }
                                catch
                                {
                                    trans.Rollback();
                                    result.Desc = "批量收货失败";

                                }
                            }
                            #endregion
                        }

                    }
                    JobHelper.SyncSubconReceipt(packNum);

                }
            }
            catch(Exception ex)
            {
                result.Desc = GetResValue("Txt_BatchReceiptFailed");
            }

            return Json(result);
        }
        public JsonResult ConfirmReceiptSingle(string id, string JobNum,string OprSeq,string LaborQty,string SplitQty,string ReworkQty,string ScrapQty,string DebitNote,string ReturnToVendor)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string packNum = string.Empty;
            string cmdText = string.Empty;
            DataTableCollection dtc;

            string poNum = string.Empty;
            string vendorNum = string.Empty;
            string vendorName = string.Empty;

            try
            {

                #region 获取VendorNum和VendorName
                cmdText = string.Format(@"
                        select PONum
                        from WebSubconReceipt
                        where id = {0}
                    ",
                    id
                );

                dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                if (dtc[0].Rows.Count > 0)
                {
                    poNum = dtc[0].Rows[0]["PONum"].ToString();
                }

                cmdText = string.Format(@"
                        select V.VendorNum,V.Name
                        from Erp.Vendor as V
                        left join Erp.POHeader as PH on V.Company = PH.Company and PH.VendorNum = V.VendorNum
                        where V.Company = '{0}' and PH.PONum = {1}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    poNum
                );
                dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);
                if (dtc[0].Rows.Count > 0)
                {
                    vendorNum = dtc[0].Rows[0]["VendorNum"].ToString();
                    vendorName = dtc[0].Rows[0]["Name"].ToString();
                }
                #endregion

                //将外包发货记录的置为可同步 Sync = 1
                /*
                cmdText = string.Format(@"
                    update WebSubconReceipt
                        set Sync = '1',
                        LaborQty = {1},
                        ScrapQty = {2},
                        OSReworkQty = {3},
                        InternalReworkQty = {4}
                        where Sync = '0' and id = {0} 
                    ",
                    id,
                    LaborQty,
                    ScrapQty,
                    SplitQty,
                    ReworkQty
                );
                SqlHelper.ExecteNonQueryText(cmdText, null);
                */

                string jobnum = string.Empty;
                
                cmdText = string.Format(@"
                        select JobNum,PackNum
                        from WebSubconReceipt
                        where id = {0}
                    ",
                    id
                );
                dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                
                if (dtc[0].Rows.Count > 0)
                {
                    jobnum = dtc[0].Rows[0]["JobNum"].ToString();
                    packNum = dtc[0].Rows[0]["PackNum"].ToString();

                    cmdText = string.Format(@"                    
                                    select top 1 *
                                    from WebTransaction
                                    where JobNum = '{1}'
                                    order by id desc
                                ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        jobnum
                    );
                    dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                    if (dtc[0].Rows.Count > 0)
                    {
                        DataRow row = dtc[0].Rows[0];

                        #region 欢乐锁逻辑
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
                                #region 开始更新前获取Version
                                int version = 0;
                                cmdText = string.Format(@"
                                        select Version
                                        from WebJobOper
                                        where Company = '{0}' and JobNum = '{1}' and OprSeq = {2}
                                    ",
                                    ConfigurationManager.AppSettings["CompanyCode"],
                                    JobNum,
                                    OprSeq
                                );
                                //DataTableCollection dtcPara = SqlHelper.GetTable(CommandType.Text, cmdTextPara, null);
                                cmd.CommandText = cmdText;
                                adapter = new SqlDataAdapter();
                                adapter.SelectCommand = cmd;
                                ds = new DataSet();
                                adapter.Fill(ds);
                                DataTableCollection dtcPara = ds.Tables;

                                if (dtcPara[0].Rows.Count > 0)
                                {
                                    int.TryParse(dtcPara[0].Rows[0]["Version"].ToString(), out version);
                                }
                                #endregion

                                
                                //将外包发货记录的置为可同步 Sync = 1
                                cmdText = string.Format(@"
                                    update WebSubconReceipt
                                        set Sync = '1',
                                        LaborQty = {1},
                                        ScrapQty = {2},
                                        OSReworkQty = {3},
                                        InternalReworkQty = {4},
                                        ReturnToVendor = {5}
                                        where Sync = '0' and id = {0} 
                                    ",
                                    id,
                                    LaborQty,
                                    ScrapQty,
                                    SplitQty,
                                    ReworkQty,
                                    ReturnToVendor
                                );
                                cmd.CommandText = cmdText;
                                cmd.ExecuteNonQuery();

                                #region 添加WebTransaction记录
                                string cmdTextValidation = string.Format(@"
                                        select ISNULL(TransType,'') as TransType
                                        from WebTransaction
                                        where JobNum = '{0}'
                                        order by id desc 
                                    ",
                                    JobNum
                                );
                                DataTableCollection dtcValidation = SqlHelper.GetTable(CommandType.Text, cmdTextValidation, null);

                                //插入到WebTransaction之前需要判断一下该工单的最后一条记录必须不是外包收货确认（OSRECEIPTCONFIRM）
                                if (!(dtcValidation[0].Rows.Count > 0 && dtcValidation[0].Rows[0]["TransType"].ToString().Equals("OSRECEIPTCONFIRM")))
                                {
                                    cmdText = string.Format(@"
                                            insert into WebTransaction(Company,TransType,UserId,MachineId,PDAId,JobNum,OprSeq,OpCode,OpGroup,OprSeqNext,OpCodeNext,OpGroupNext,FromLocation,ToLocation,OprQty,ReportingQty,LaborQty,ScrapQty,DiscrepQty)
                                            values('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}','{13}',{14},{15},{16},{17},{18})
                                        ",
                                        ConfigurationManager.AppSettings["CompanyCode"],            //0:Company
                                        "OSRECEIPTCONFIRM",                //1:TransType
                                        user.UserName,            //2:UserId
                                        "WEB",                              //3:MachineId
                                        "WEB",                              //4:PDAId
                                        jobnum,                             //5:JobNum
                                        row["OprSeq"].ToString(),           //6:OprSeq
                                        row["OpCode"].ToString(),           //7:OpCode
                                        row["OpGroup"].ToString(),          //8:OpGroup
                                        row["OprSeqNext"].ToString(),       //9:OprSeqNext
                                        row["OpCodeNext"].ToString(),       //10:OpCodeNext
                                        row["OpGroupNext"].ToString(),      //11:OpGroupNext
                                        "",                                 //12:FromLocation
                                        "",                                 //13:ToLocation
                                        row["OprQty"].ToString(),           //14:OprQty
                                        LaborQty,                           //15:ReportingQty
                                        LaborQty,                           //16:LaborQty
                                        row["ScrapQty"].ToString(),         //17:ScrapQty
                                        ReworkQty                           //18:DiscrepQty
                                    );
                                    //SqlHelper.ExecteNonQueryText(cmdText, null);
                                    cmd.CommandText = cmdText;
                                    int val = cmd.ExecuteNonQuery();

                                    #region 内部原因--返工流程
                                    if (!ReworkQty.Equals("0"))
                                    {
                                        cmdText = string.Format(@"
                                            insert WebDiscrepHeader(Company,JobNum,OprSeq,OpCode,OprDesc,Quantity,ReasonCode,ReasonDesc,ReportId,ReportPDAId,Reporttime,ReceiveId,VendorNum,VendorName)
                                            values('{0}','{1}',{2},'{3}','{4}',{5},'{6}',N'{7}','{8}','{9}','{10}',{11},'{12}',N'{13}')
                                        ",
                                            ConfigurationManager.AppSettings["CompanyCode"],    //0
                                            JobNum,             //1:JobNum
                                            OprSeq,             //2:OprSeq
                                            JobHelper.getJobOperBySeq(JobNum, OprSeq).OprCode,                 //3:OpCode
                                            JobHelper.getJobOperBySeq(JobNum, OprSeq).OprDesc,                 //4:OprDesc
                                            ReworkQty,          //5:Quantity
                                            "OT30",             //6:ReasonCode
                                            "其他",                                                  //7:ReasonDesc
                                            user.UserName,                                //8:ReportId
                                            "WEB",                                                  //9:ReportPDAId
                                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),        //10
                                            id,                 //11:ReceiveId
                                            vendorNum,          //12:VendorNum
                                            vendorName          //13:VendorName
                                        );
                                        //SqlHelper.ExecteNonQueryText(cmdText, null);
                                        cmd.CommandText = cmdText;
                                        val = cmd.ExecuteNonQuery();
                                    }
                                    #endregion

                                    #region 外部供应商原因--需要拆单
                                    if (!SplitQty.Equals("0"))
                                    {
                                        int laborQty = 0;
                                        int splitQty = 0;
                                        int reworkQty = 0;
                                        int scrapQty = 0;
                                        int oprQty = 0;

                                        try
                                        {
                                            laborQty = int.Parse(LaborQty);
                                            splitQty = int.Parse(SplitQty);
                                            reworkQty = int.Parse(ReworkQty);
                                            scrapQty = int.Parse(ScrapQty);
                                            oprQty = laborQty + splitQty + reworkQty + scrapQty;
                                        }
                                        catch (Exception ex)
                                        {

                                        }


                                        if (oprQty > 0)
                                        {
                                            cmdText = string.Format(@"
                                                    update WebJobOper
                                                    set OprQty = {3}
                                                    where Company = '{0}' and JobNum = '{1}' and OprSeq = '{2}' 
                                                ",
                                                ConfigurationManager.AppSettings["CompanyCode"],
                                                JobNum,
                                                OprSeq,
                                                oprQty
                                            );
                                            //SqlHelper.ExecteNonQueryText(cmdText, null);
                                            cmd.CommandText = cmdText;
                                            val = cmd.ExecuteNonQuery();

                                            cmdText = string.Format(@"
                                                    insert into WebJobSplit(Company,JobNum,OperSeq,Split,OprQty,SplitQty)
                                                    values('{0}','{1}',{2},{3},{4},{5})
                                                ",
                                                ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                                                JobNum,                 //1:JobNum
                                                OprSeq,                 //2:OperSeq
                                                "0",                    //3:Split
                                                oprQty,                 //4:OprQty
                                                oprQty - splitQty       //5:SplitQty
                                            );
                                            //SqlHelper.ExecteNonQueryText(cmdText, null);
                                            cmd.CommandText = cmdText;
                                            val = cmd.ExecuteNonQuery();
                                        }


                                    }
                                    #endregion
                                }

                                #endregion



                                #region 更新结束后，判断换了锁
                                cmdText = string.Format(@"
                                        update WebJobOper
                                        set Version = Version + 1
                                        where Company = '{0}' and JobNum = '{1}' and OprSeq = {2} and Version = {3}
                                    ",
                                    ConfigurationManager.AppSettings["CompanyCode"],
                                    JobNum,
                                    OprSeq,
                                    version
                                );
                                cmd.CommandText = cmdText;
                                int valTrans = cmd.ExecuteNonQuery();

                                if (valTrans > 0)
                                {
                                    trans.Commit();
                                    
                                }
                                else
                                {
                                    trans.Rollback();
                                }
                                #endregion
                            }
                            catch
                            {
                                trans.Rollback();
                            }
                        }
                        #endregion

                        #region 原来的逻辑
                        if (1 == 0)
                        {
                            string cmdTextValidation = string.Format(@"
                                    select ISNULL(TransType,'') as TransType
                                    from WebTransaction
                                    where JobNum = '{0}'
                                    order by id desc 
                                ",
                                JobNum
                            );
                            DataTableCollection dtcValidation = SqlHelper.GetTable(CommandType.Text, cmdTextValidation, null);

                            if (!(dtcValidation[0].Rows.Count > 0 && dtcValidation[0].Rows[0]["TransType"].ToString().Equals("OSRECEIPTCONFIRM")))
                            {
                                cmdText = string.Format(@"
                                        insert into WebTransaction(Company,TransType,UserId,MachineId,PDAId,JobNum,OprSeq,OpCode,OpGroup,OprSeqNext,OpCodeNext,OpGroupNext,FromLocation,ToLocation,OprQty,ReportingQty,LaborQty,ScrapQty,DiscrepQty)
                                        values('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}','{11}','{12}','{13}',{14},{15},{16},{17},{18})
                                    ",
                                    ConfigurationManager.AppSettings["CompanyCode"],            //0:Company
                                    "OSRECEIPTCONFIRM",                //1:TransType
                                    user.UserName,            //2:UserId
                                    "WEB",                              //3:MachineId
                                    "WEB",                              //4:PDAId
                                    jobnum,                             //5:JobNum
                                    row["OprSeq"].ToString(),           //6:OprSeq
                                    row["OpCode"].ToString(),           //7:OpCode
                                    row["OpGroup"].ToString(),          //8:OpGroup
                                    row["OprSeqNext"].ToString(),       //9:OprSeqNext
                                    row["OpCodeNext"].ToString(),       //10:OpCodeNext
                                    row["OpGroupNext"].ToString(),      //11:OpGroupNext
                                    "",                                 //12:FromLocation
                                    "",                                 //13:ToLocation
                                    row["OprQty"].ToString(),           //14:OprQty
                                    LaborQty,                           //15:ReportingQty
                                    LaborQty,                           //16:LaborQty
                                    row["ScrapQty"].ToString(),         //17:ScrapQty
                                    ReworkQty                           //18:DiscrepQty
                                );
                                SqlHelper.ExecteNonQueryText(cmdText, null);
                            }
                        }

                        #endregion

                    }

                    JobHelper.SyncSubconReceipt(packNum);
                }


                if (1 == 0)
                {
                    #region
                    if (!ReworkQty.Equals("0"))
                    {
                        //直接进入返工流程
                        /*
                        cmdText = string.Format(@"
                                insert into WebReworkJobHead(Company,ParentJobNum,EpicorJobNum,PartNum,RevisionNum,DrawNum,PartDescription,ProdQty,IUM)
                                select top 1 Company,'{0}',EpicorJobNum,PartNum,RevisionNum,DrawNum,PartDescription,{1},IUM
                                from WebJobHead
                                where JobNum = '{0}'
                            ",
                            JobNum,
                            ReworkQty
                        );
                        SqlHelper.ExecteNonQueryText(cmdText, null);
                        */

                        //现在需要改成走MRB流程
                        cmdText = string.Format(@"
                        insert WebDiscrepHeader(Company,JobNum,OprSeq,OpCode,OprDesc,Quantity,ReasonCode,ReasonDesc,ReportId,ReportPDAId,Reporttime,ReceiveId,VendorNum,VendorName)
                        values('{0}','{1}',{2},'{3}','{4}',{5},'{6}',N'{7}','{8}','{9}','{10}',{11},'{12}',N'{13}')
                    ",
                            ConfigurationManager.AppSettings["CompanyCode"],    //0
                            JobNum,             //1:JobNum
                            OprSeq,             //2:OprSeq
                            JobHelper.getJobOperBySeq(JobNum, OprSeq).OprCode,                 //3:OpCode
                            JobHelper.getJobOperBySeq(JobNum, OprSeq).OprDesc,                 //4:OprDesc
                            ReworkQty,          //5:Quantity
                            "OT30",             //6:ReasonCode
                            "其他",                                                  //7:ReasonDesc
                            user.UserName,                                //8:ReportId
                            "WEB",                                                  //9:ReportPDAId
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),        //10
                            id,                 //11:ReceiveId
                            vendorNum,          //12:VendorNum
                            vendorName          //13:VendorName
                        );
                        SqlHelper.ExecteNonQueryText(cmdText, null);

                    }
                    #endregion
                }

                if (1 == 0)
                {
                    #region
                    if (!SplitQty.Equals("0"))
                    {
                        int laborQty = 0;
                        int splitQty = 0;
                        int reworkQty = 0;
                        int scrapQty = 0;
                        int oprQty = 0;

                        try
                        {
                            laborQty = int.Parse(LaborQty);
                            splitQty = int.Parse(SplitQty);
                            reworkQty = int.Parse(ReworkQty);
                            scrapQty = int.Parse(ScrapQty);
                            oprQty = laborQty + splitQty + reworkQty + scrapQty;
                        }
                        catch (Exception ex)
                        {

                        }


                        if (oprQty > 0)
                        {
                            cmdText = string.Format(@"
                                update WebJobOper
                                set OprQty = {3}
                                where Company = '{0}' and JobNum = '{1}' and OprSeq = '{2}' 
                            ",
                                ConfigurationManager.AppSettings["CompanyCode"],
                                JobNum,
                                OprSeq,
                                oprQty
                            );
                            SqlHelper.ExecteNonQueryText(cmdText, null);

                            cmdText = string.Format(@"
                                insert into WebJobSplit(Company,JobNum,OperSeq,Split,OprQty,SplitQty)
                                values('{0}','{1}',{2},{3},{4},{5})
                            ",
                                ConfigurationManager.AppSettings["CompanyCode"],    //0:Company
                                JobNum,                 //1:JobNum
                                OprSeq,                 //2:OperSeq
                                "0",                    //3:Split
                                oprQty,                 //4:OprQty
                                oprQty - splitQty       //5:SplitQty
                            );
                            SqlHelper.ExecteNonQueryText(cmdText, null);
                        }


                    }
                    #endregion
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            
            SimpleResult result = new SimpleResult();
            return Json(result);
        }
    }
}