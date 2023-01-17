using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using WebAdmin.Models;
using System.Net.Mail;
using System.Web.Mail;
using System.Net;
using Microsoft.Reporting.WebForms;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using PdfSharp.Drawing;
using PdfSharp.Charting;
using PdfSharp.SharpZipLib;
using PdfSharp.Pdf.IO;

namespace WebAdmin.Controllers
{
    public class EWIController : BaseController
    {
        public string MailFromAddress = ConfigurationManager.AppSettings["MailFromAddress"];
        public bool UseSsl = bool.Parse(ConfigurationManager.AppSettings["UseSsl"]);
        public string UserName = ConfigurationManager.AppSettings["UserName"];
        public string Password = ConfigurationManager.AppSettings["Password"];
        public string DoMain = ConfigurationManager.AppSettings["DoMain"];
        public string ServerName = ConfigurationManager.AppSettings["ServerName"];
        public int ServerPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
        public string EngUser = ConfigurationManager.AppSettings["EngUser"];
        public string QMUser = ConfigurationManager.AppSettings["QMUser"];
        public string ReviewUser = ConfigurationManager.AppSettings["ReviewUser"];
        public string ApproveUser = ConfigurationManager.AppSettings["ApproveUser"];
        public string ECNUser = ConfigurationManager.AppSettings["ECNUser"];
        public string DccUser = ConfigurationManager.AppSettings["DccUser"];
        public string ECNEmail = ConfigurationManager.AppSettings["ECNEmail"];
        public string ECNShowUser = ConfigurationManager.AppSettings["ECNShowUser"];

        public SimpleCurRight InitialCurRight()
        {
            SimpleCurRight CurRight = new SimpleCurRight();
            CurRight.Menu = "";
            CurRight.CheckRight = "0";
            return CurRight;
        }

        public SimpleCurRight GetCurRight()
        {
            //获取权限
            SimpleCurRight CurRight = InitialCurRight();
            if (Session["CurRight"] == null)
                Session["CurRight"] = CurRight;
            else
                CurRight = (SimpleCurRight)Session["CurRight"];

            return CurRight;
        }

        #region MY EWI
        public ActionResult MyEWIView()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //检查菜单权限
            if (!CheckRight("MyEWI"))
            {
                return Redirect("/Login/Home");
            }

            string MyEWIType = "", CurUser = user.UserName + ";";
            if (EngUser.IndexOf(CurUser) > -1)
                MyEWIType = "0";
            if (QMUser.IndexOf(CurUser) > -1)
                MyEWIType = "1" + MyEWIType;
            if (ReviewUser.IndexOf(CurUser) > -1)
                MyEWIType = "2" + MyEWIType;
            if (ApproveUser.IndexOf(CurUser) > -1)
                MyEWIType = "3" + MyEWIType;
            if (ECNUser.IndexOf(CurUser) > -1)
                MyEWIType = "4" + MyEWIType;
            if (DccUser.IndexOf(CurUser) > -1)
                MyEWIType = "5" + MyEWIType;

            string cmdText = string.Format(@"EWI_MyEWI_GetList {0},'{1}'",
                    MyEWIType,
                    user.Language
                );
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);

            List<SimpleMyEWI> MyEWIList = new List<SimpleMyEWI>();
            SimpleMyEWI MyEWI = new SimpleMyEWI();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                MyEWI = new SimpleMyEWI();
                MyEWI.ListType = row["ListType"].ToString();
                MyEWI.Qty = row["Qty"].ToString();
                MyEWI.QtyNormal = row["QtyNormal"].ToString();
                MyEWI.QtyFA = row["QtyFA"].ToString();
                MyEWI.QtyTemp = row["QtyTemp"].ToString();
                MyEWI.QtyPercent = row["QtyPercent"].ToString();
                MyEWI.QtyPercentShow = row["QtyPercentShow"].ToString();
                MyEWI.IsToDo = row["IsToDo"].ToString();
                MyEWI.RowIndex = row["RowIndex"].ToString();
                MyEWI.DetailIndex = row["DetailIndex"].ToString();

                MyEWIList.Add(MyEWI);
            }

            List<string> MyEWIGroupList = MyEWIList.Select(t => t.RowIndex).Distinct().ToList();

            ViewBag.MyEWIType = MyEWIType;
            ViewBag.MyEWIList = MyEWIList;
            ViewBag.MyEWIGroupList = MyEWIGroupList;
            return View();
        }
        #endregion

        #region EWIList
        public ActionResult EWIView(string RoleName)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            SimpleCurRight CurRight = new SimpleCurRight();

            //检查菜单权限
            if (!CheckRight(RoleName))
            {
                return Redirect("/Login/Home");
            }

            //审核权限
            CurRight.CheckRight = "0";
            CurRight.Menu = RoleName;
            Session["CurRight"] = CurRight;
            return View();
        }

        public JsonResult GetEWIList(DataTablesParameters param, string QueryString)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            //获取权限
            SimpleCurRight CurRight = GetCurRight();

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
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = param.Columns[1].Search.Value;
                filterType = 1;
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                filter = param.Columns[2].Search.Value;
                filterType = 2;
            }
            else if (!string.IsNullOrEmpty(param.Columns[3].Search.Value))
            {
                filter = param.Columns[3].Search.Value;
                filterType = 3;
            }
            else if (QueryString != "")
            {
                filter = QueryString;
                filterType = 3;
            }

            string cmdText = string.Format(@"EWI_GetList {0},{1},'{2}','{3}',{4},'{5}','{6}'",
                    param.Length,                                       //{0}PageSize
                    param.Start,                                        //{1}PageStart
                    (param.OrderBy == "" ? "PartNum " + DataTablesOrderDir.Asc : (param.OrderBy + " " + param.OrderDir)),               //{2}OrderBy Column + OrderBy Direction ASC/DESC
                    filter,                                             //{3}QueryString
                    filterType,                                         //{4}QueryType
                    CurRight.Menu,
                    user.Language
                );

            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);
            int recordsTotal = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());

            List<SimpleEWI> EWIList = new List<SimpleEWI>();
            SimpleEWI EWI = new SimpleEWI();
            foreach (DataRow row in ds.Tables[1].Rows)
            {
                EWI = new SimpleEWI();
                EWI.ID = row["ID"].ToString();
                EWI.ControlPlanNum = row["ControlPlanNum"].ToString();
                EWI.PartNum = row["PartNum"].ToString();
                EWI.PartDesc = row["PartDesc"].ToString();
                EWI.CustomerPartNum = row["CustomerPartNum"].ToString();
                EWI.LatestDate = row["LatestDate"].ToString() == "" ? "" : Convert.ToDateTime(row["LatestDate"]).ToString("yyyy-MM-dd");
                EWI.LatestRev = row["LatestRev"].ToString();
                EWI.StatusDesc = row["StatusDesc"].ToString();
                EWI.StatusLog = row["StatusLog"].ToString();
                EWI.RevLog = row["RevLog"].ToString();

                EWIList.Add(EWI);
            }
            DataTablesResult<SimpleEWI> result = new DataTablesResult<SimpleEWI>(param.Draw, recordsTotal, recordsTotal, EWIList);
            return Json(result);
        }

        public JsonResult GetEWIPartInfo(string ID)
        {
            string filter = ID;
            int filterType = 0;

            string cmdText = string.Format(@"EWI_GetData {0},{1}",
                    filter,                                             //{0}QueryID
                    filterType                                          //{1}QueryType
                );

            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);

            SimpleEWI EWI = new SimpleEWI();
            BaseResponse<SimpleEWI> returnResponse = new BaseResponse<SimpleEWI>();
            if (ds.Tables[0].Rows.Count == 1)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                EWI.ID = dr["ID"].ToString();
                EWI.ControlPlanNum = dr["ControlPlanNum"].ToString();
                EWI.PartNum = dr["PartNum"].ToString();
                EWI.PartDesc = dr["PartDesc"].ToString();
                EWI.CustomerPartNum = dr["CustomerPartNum"].ToString();
                EWI.KeyContact = dr["KeyContact"].ToString();
                EWI.CoreTeam = dr["CoreTeam"].ToString();
                EWI.LatestDate = Convert.ToDateTime(dr["LatestDate"]).ToString("yyyy-MM-dd");
                EWI.LatestRev = dr["LatestRev"].ToString();

                returnResponse.MsgCode = "OK";
                returnResponse.Data = EWI;
            }
            else
            {
                EWI = null;
                returnResponse.MsgCode = "NoData";
                returnResponse.Data = null;
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateEWIPartInfo(SimpleEWI EWI)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.MsgCode = "OK";
            returnResponse.Msg = "";

            if (EWI.ID != null)
            {
                int filterType = 0;
                string StrCol = "";
                string StrSql = "";
                if (EWI.ID != "0")
                {
                    StrSql = "EWI_UpdateData";
                    StrCol = string.Format(@"CustomerPartNum = ''{0}'', ControlPlanNum = ''{1}'', 
                        KeyContact = ''{2}'', CoreTeam = ''{3}''",
                        EWI.CustomerPartNum,
                        EWI.ControlPlanNum,
                        EWI.KeyContact,
                        EWI.CoreTeam);
                }
                else
                {
                    StrSql = "EWI_InsertData";
                    StrCol = string.Format(@"''{0}'', ''{1}'', ''{2}'', ''{3}'', ''{4}''",
                        EWI.PartNum,
                        EWI.CustomerPartNum,
                        EWI.ControlPlanNum,
                        EWI.KeyContact,
                        EWI.CoreTeam);
                }

                string cmdText = string.Format(@"{0} '{1}',{2},{3},'{4}','{5}','{6}'",
                        StrSql,
                        StrCol,             //UpdateCol
                        EWI.ID,             //QueryID
                        filterType,         //QueryType
                        user.UserName,
                        user.Language,
                        ConfigurationManager.AppSettings["CompanyCode"]
                        );
                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, cmdText);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
                if (returnResponse.Msg != "")
                    returnResponse.MsgCode = "error";
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPartList(string Part, string page)
        {
            string cmdText = string.Format(@"EWI_GetPart 0, '{0}',{1}", Part, page);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);

            SimplePart part = new SimplePart();
            List<SimplePart> _PartList = new List<SimplePart>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                part = new SimplePart();
                part.PartNum = row["PartNum"].ToString();
                part.PartDesc = row["PartDesc"].ToString();
                part.CustomerPartNum = row["CustomerPartNum"].ToString();

                _PartList.Add(part);
            }

            return Json(_PartList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region EWI 控制文件版本
        public ActionResult EWIMainView(string PartNum, string WIType = "", string RevID = "0")
        {
            //获取权限
            SimpleCurRight CurRight = GetCurRight();
            SysAdmin user = GetAdminInfo();
            SimpleEWIMain _EWIMain = new SimpleEWIMain();
            SimpleEWIECN _EWIECN = new SimpleEWIECN();
            List<string> AltMethodList = new List<string>();
            List<SimpleStatus> StatusList = new List<SimpleStatus>();
            int StatusProcess = 0, MaxProcess = 0;

            if (user == null)
            {
                if (PartNum != "")
                {
                    TempData["Action"] = "EWIMainView";
                    TempData["Controller"] = "EWI";
                    TempData["PartNum"] = PartNum;
                }
                Redirect("/Login/Index");
            }
            else
            {
                //验证菜单权限
                if (!CheckRight(new string[] { "EWI", "EWI_FA", "EWI_NPI", "EWI_QM", "EWI_QM_FA", "EWI_QM_NPI", "EWI_Check" }))
                {
                    return Redirect("/Login/Home");
                }

                string cmdText = string.Format(@"EWI_Main_GetBasicInfo '{0}','{1}','{2}',{3},'{4}'",
                        PartNum,                                    //{0}PartNum
                        CurRight.Menu,
                        WIType,
                        RevID,
                        user.Language
                    );
                DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    _EWIMain.ID = row["ID"].ToString();
                    _EWIMain.MainID = row["MainID"].ToString();
                    _EWIMain.ControlPlanNum = row["ControlPlanNum"].ToString();
                    _EWIMain.ProductStage = row["ProductStage"].ToString();
                    _EWIMain.KeyContact = row["KeyContact"].ToString();
                    _EWIMain.OriginDate = Convert.ToDateTime(row["OriginDate"]).ToString("yyyy-MM-dd");
                    _EWIMain.LatestDate = Convert.ToDateTime(row["LatestDate"]).ToString("yyyy-MM-dd");
                    _EWIMain.LatestRev = row["LatestRev"].ToString();
                    _EWIMain.CustomerPartNum = row["CustomerPartNum"].ToString();
                    _EWIMain.CustomerRev = row["CustomerRev"].ToString();
                    _EWIMain.PartNum = row["PartNum"].ToString();
                    _EWIMain.CoreTeam = row["CoreTeam"].ToString();
                    _EWIMain.CustomerEngApprovalDate = row["CustomerEngApprovalDate"].ToString() == "" ? "" : Convert.ToDateTime(row["CustomerEngApprovalDate"]).ToString("yyyy-MM-dd");
                    _EWIMain.PartDesc = row["PartDesc"].ToString();
                    _EWIMain.SupplierApprovalDate = row["SupplierApprovalDate"].ToString() == "" ? "" : Convert.ToDateTime(row["SupplierApprovalDate"]).ToString("yyyy-MM-dd");
                    _EWIMain.CustomerQualityApprovalDate = row["CustomerQualityApprovalDate"].ToString() == "" ? "" : Convert.ToDateTime(row["CustomerQualityApprovalDate"]).ToString("yyyy-MM-dd");
                    _EWIMain.Supplier = row["Supplier"].ToString();
                    _EWIMain.SupplierCode = row["SupplierCode"].ToString();
                    _EWIMain.OtherApprovalDate = row["OtherApprovalDate"].ToString() == "" ? "" : Convert.ToDateTime(row["OtherApprovalDate"]).ToString("yyyy-MM-dd");
                    _EWIMain.OtherApprovalDate2 = row["OtherApprovalDate2"].ToString() == "" ? "" : Convert.ToDateTime(row["OtherApprovalDate2"]).ToString("yyyy-MM-dd");
                    _EWIMain.ReviewBy = row["ReviewBy"].ToString();
                    _EWIMain.PCN_ECN_NO = row["PCN_ECN_NO"].ToString();
                    _EWIMain.Description = row["Description"].ToString();
                    _EWIMain.RevisionAndAltMethod = "Revision:" + row["RevisionNum"].ToString() + " 替代版本:" + row["AltMethod"].ToString();
                    _EWIMain.ApprovalStatus = row["ApprovalStatus"].ToString();
                    _EWIMain.JobNum = row["JobNum"].ToString();
                    _EWIMain.NSType = row["NSType"].ToString();
                    _EWIMain.WIType = row["WIType"].ToString();
                    _EWIMain.CurrentRev = row["CurrentRev"].ToString();
                    _EWIMain.CurrentRevActive = row["CurrentRevActive"].ToString();
                }

                SimpleStatus tmpStaus;
                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    tmpStaus = new SimpleStatus();
                    tmpStaus.StatusDesc = dr["StatusDesc"].ToString();
                    tmpStaus.StatusLog = dr["StatusLog"].ToString();
                    tmpStaus.IsFinish = dr["IsFinish"].ToString();
                    StatusProcess += dr["IsFinish"].ToString() == "1" ? 1 : 0;
                    MaxProcess += 1;
                    StatusList.Add(tmpStaus);
                }

                if (ds.Tables[2].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[2].Rows[0];
                    _EWIECN.ID = row["ID"].ToString();
                    _EWIECN.RevisionID = row["RevisionID"].ToString();
                    _EWIECN.ECNNo = row["ECNNo"].ToString();
                    _EWIECN.ECRNo = row["ECRNo"].ToString();
                    _EWIECN.Title = row["Title"].ToString();
                    _EWIECN.ChangeReason = row["ChangeReason"].ToString();
                    _EWIECN.ChangeFrom = row["ChangeFrom"].ToString();
                    _EWIECN.ChangeTo = row["ChangeTo"].ToString();
                    _EWIECN.Remark = row["Remark"].ToString();
                    _EWIECN.RelateDept = row["RelateDept"].ToString();
                }

                cmdText = string.Format(@"EWI_Main_GetOprRev '{0}'",
                    PartNum
                );
                ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    AltMethodList.Add("Revision:" + dr["RevisionNum"].ToString() + " 替代版本:" + dr["AltMethod"].ToString());
                }
            }

            ViewBag.CurRight = CurRight;
            ViewBag.EWIMain = _EWIMain;
            ViewBag.AltMethodList = AltMethodList;
            ViewBag.StatusList = StatusList;
            ViewBag.StatusProcess = StatusProcess;
            ViewBag.MaxProcess = MaxProcess;
            ViewBag.EWIECN = _EWIECN;
            ViewBag.ECNUserList = ECNShowUser.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            return View();
        }

        public JsonResult GetEWIRevisionList(DataTablesParameters param, string PartNum, string WIType = "")
        {
            //获取权限
            SimpleCurRight CurRight = GetCurRight();
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string cmdText = string.Format(@"EWI_Main_GetRevList '{0}','{1}','{2}','{3}'",
                    PartNum,                                    //{0}PartNum
                    CurRight.Menu,
                    WIType,
                    user.Language
                );

            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);
            int recordsTotal = ds.Tables[0].Rows.Count;

            List<SimpleEWIRevision> EWIRevisionList = new List<SimpleEWIRevision>();
            SimpleEWIRevision EWIRevision = new SimpleEWIRevision();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                EWIRevision = new SimpleEWIRevision();
                EWIRevision.ID = row["ID"].ToString();
                EWIRevision.PartNum = row["PartNum"].ToString();
                EWIRevision.Revision = row["Revision"].ToString();
                EWIRevision.EffectiveDate = row["EffectiveDate"].ToString() == "" ? "" : Convert.ToDateTime(row["EffectiveDate"]).ToString("yyyy-MM-dd");
                EWIRevision.PCN_ECN_NO = row["PCN_ECN_NO"].ToString();
                EWIRevision.Iniatior = row["Iniatior"].ToString();
                EWIRevision.Description = row["Description"].ToString();
                EWIRevision.StatusDesc = row["StatusDesc"].ToString();
                EWIRevision.StatusLog = row["StatusLog"].ToString();
                EWIRevision.ApprovalStatus = row["ApprovalStatus"].ToString();
                EWIRevision.EditStatus = row["EditStatus"].ToString();

                EWIRevisionList.Add(EWIRevision);
            }
            DataTablesResult<SimpleEWIRevision> result = new DataTablesResult<SimpleEWIRevision>(0, recordsTotal, recordsTotal, EWIRevisionList);
            return Json(result);
        }

        public JsonResult GetAllRevisionList(string Part, string page)
        {
            string cmdText = string.Format(@"EWI_Main_GetRevList2 '{0}',{1}", Part, page);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);

            List<SimpleEWIRevision> EWIRevisionList = new List<SimpleEWIRevision>();
            SimpleEWIRevision EWIRevision = new SimpleEWIRevision();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                EWIRevision = new SimpleEWIRevision();
                EWIRevision.ID = row["ID"].ToString();
                EWIRevision.PartNum = row["PartNum"].ToString();
                EWIRevision.PartDesc = row["PartDesc"].ToString();
                EWIRevision.CustomerPartNum = row["CustomerPartNum"].ToString();
                EWIRevision.Revision = row["Revision"].ToString();
                EWIRevision.EffectiveDate = row["EffectiveDate"].ToString() == "" ? "" : Convert.ToDateTime(row["EffectiveDate"]).ToString("yyyy-MM-dd");
                EWIRevision.PCN_ECN_NO = row["PCN_ECN_NO"].ToString();
                EWIRevision.Iniatior = row["Iniatior"].ToString();
                EWIRevision.Description = row["Description"].ToString();

                EWIRevisionList.Add(EWIRevision);
            }

            return Json(EWIRevisionList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEWINewRevision(string PartNum)
        {
            //获取权限
            SimpleCurRight CurRight = GetCurRight();
            BaseResponse<SimpleNewRevision> returnResponse = new BaseResponse<SimpleNewRevision>();
            returnResponse.MsgCode = "OK";

            string strSql = string.Format("EWI_Main_GetLatestRev '{0}',1,'{1}'", PartNum, CurRight.Menu);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
            SimpleNewRevision simpleNewRevision = new SimpleNewRevision();
            simpleNewRevision.CustomerRev = ds.Tables[0].Rows[0]["CustomerRev"].ToString();
            simpleNewRevision.LatestRev = ds.Tables[0].Rows[0]["LatestRev"].ToString();
            returnResponse.Data = simpleNewRevision;
            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public JsonResult UpdateEWIRevisionInfo(SimpleEWIMain EWIMain, string AltMethod)
        {
            EWIMain.ReviewBy = (EWIMain.ReviewBy == null ? "" : EWIMain.ReviewBy);
            SimpleCurRight CurRight = GetCurRight();
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            BaseResponse<SimpleEWIMain> returnResponse = new BaseResponse<SimpleEWIMain>();
            returnResponse.MsgCode = "Error";
            returnResponse.Msg = "";

            if (EWIMain.ID != null)
            {
                int filterType = 1;
                string StrCol = "";
                string StrSql = "";
                if (EWIMain.ID != "0")
                {
                    StrSql = "EWI_UpdateData";
                    StrCol = string.Format(@"ProductStage = ''{0}'', Revision = ''{1}'', 
                        CustomerRev = ''{2}'', PCN_ECN_NO = ''{3}'', Description = ''{4}'',
                        CustomerEngApprovalDate = {5}, SupplierApprovalDate = {6}, CustomerQualityApprovalDate = {7},
                        OtherApprovalDate = {8}, OtherApprovalDate2 = {9}, Supplier = ''{10}'',
                        SupplierCode = ''{11}'', JobNum = ''{12}'', NSType =''{13}''",
                        EWIMain.ProductStage,
                        EWIMain.LatestRev,
                        EWIMain.CustomerRev,
                        EWIMain.PCN_ECN_NO,
                        EWIMain.Description,
                        EWIMain.CustomerEngApprovalDate == null ? "null" : "''" + EWIMain.CustomerEngApprovalDate + "''",
                        EWIMain.SupplierApprovalDate == null ? "null" : "''" + EWIMain.SupplierApprovalDate + "''",
                        EWIMain.CustomerQualityApprovalDate == null ? "null" : "''" + EWIMain.CustomerQualityApprovalDate + "''",
                        EWIMain.OtherApprovalDate == null ? "null" : "''" + EWIMain.OtherApprovalDate + "''",
                        EWIMain.OtherApprovalDate2 == null ? "null" : "''" + EWIMain.OtherApprovalDate2 + "''",
                        EWIMain.Supplier,
                        EWIMain.SupplierCode,
                        EWIMain.JobNum,
                        EWIMain.NSType);
                }
                else
                {

                    StrSql = string.Format("EWI_Main_CheckInsertRev {0},'{1}'", EWIMain.MainID, CurRight.Menu);
                    returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, StrSql);
                    if (returnResponse.Msg != "")
                        returnResponse.Msg = GetResValue(returnResponse.Msg);

                    StrSql = "EWI_InsertData";
                    StrCol = string.Format(@"''{0}'', ''{1}'', ''{2}'', ''{3}'', ''{4}'', ''{5}'', {6}, 
                            {7}, {8}, {9}, {10}, ''{11}'', ''{12}'', ''{13}'', ''{14}'', {15}, ''{16}''",
                        EWIMain.MainID,
                        EWIMain.ProductStage,
                        EWIMain.LatestRev,
                        EWIMain.CustomerRev,
                        EWIMain.PCN_ECN_NO,
                        EWIMain.Description,
                        EWIMain.CustomerEngApprovalDate == null ? "null" : "''" + EWIMain.CustomerEngApprovalDate + "''",
                        EWIMain.SupplierApprovalDate == null ? "null" : "''" + EWIMain.SupplierApprovalDate + "''",
                        EWIMain.CustomerQualityApprovalDate == null ? "null" : "''" + EWIMain.CustomerQualityApprovalDate + "''",
                        EWIMain.OtherApprovalDate == null ? "null" : "''" + EWIMain.OtherApprovalDate + "''",
                        EWIMain.OtherApprovalDate2 == null ? "null" : "''" + EWIMain.OtherApprovalDate2 + "''",
                        EWIMain.Supplier,
                        EWIMain.SupplierCode,
                        EWIMain.JobNum,
                        EWIMain.NSType,
                        0,
                        AltMethod);
                }

                if (returnResponse.Msg == "")
                {
                    string cmdText = string.Format(@"{0} N'{1}',{2},{3},'{4}',{5},'{6}'",
                            StrSql,
                            StrCol,             //UpdateCol
                            EWIMain.ID,         //QueryID
                            filterType,         //QueryType
                            user.UserName,
                            CurRight.Menu,
                            user.Language
                            );
                    try
                    {
                        DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            EWIMain.ID = ds.Tables[0].Rows[0]["ID"].ToString();
                            EWIMain.LatestDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["LatestDate"]).ToString("yyyy-MM-dd");
                            EWIMain.ApprovalStatus = ds.Tables[0].Rows[0]["ApprovalStatus"].ToString();
                        }
                        returnResponse.MsgCode = "OK";
                    }
                    catch (Exception ex)
                    {
                        returnResponse.MsgCode = "error";
                        returnResponse.Msg = ex.Message;
                    }
                }

                returnResponse.Data = EWIMain;
            }
            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        public JsonResult UpdateEWIECN(SimpleEWIECN EWIECN)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            SimpleCurRight CurRight = GetCurRight();
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.MsgCode = "Error";
            returnResponse.Msg = "";

            string[] ECNUserList = ECNUser.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            string[] ECNEmailList = ECNEmail.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string[] ECNShowUserList = ECNShowUser.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            string RelateEmail = "", RelateUserID = "";
            if (ECNUserList.Length != ECNEmailList.Length || ECNUserList.Length != ECNShowUserList.Length)
            {
                returnResponse.Msg = GetResValue("Txt_ECNUserEmailNotMatched");
            }
            else
            {
                if (EWIECN.RelateDept != null)
                {
                    for (int i = 0; i < ECNShowUserList.Length; i++)
                    {
                        if (EWIECN.RelateDept.IndexOf(ECNShowUserList[i]) >= 0)
                        {
                            RelateEmail += (RelateEmail == "" ? "" : ",") + ECNEmailList[i];
                            RelateUserID += (RelateUserID == "" ? "" : ",") + ECNUserList[i];
                        }
                    }
                }
            }

            if (returnResponse.Msg == "")
            {
                string StrSql = string.Format(@"EWI_UpdateData N'ECNNo = ''{0}'', ECRNo = ''{1}'', 
                    Title = ''{2}'', ChangeReason = ''{3}'', ChangeFrom = ''{4}'', ChangeTo = ''{5}'', 
                    Remark = ''{6}'', RelateDept = ''{7}'', RelateEmail= ''{8}'', RelateUserID= ''{9}''', 
                    {10},{11},'{12}','{13}'",
                    EWIECN.ECNNo, EWIECN.ECRNo, EWIECN.Title, EWIECN.ChangeReason,
                    EWIECN.ChangeFrom, EWIECN.ChangeTo, EWIECN.Remark, EWIECN.RelateDept,
                    RelateEmail, RelateUserID, EWIECN.ID, 11, user.UserName, CurRight.Menu);
                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, StrSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            if (returnResponse.Msg == "")
            {
                returnResponse.MsgCode = "OK";
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CopyRevisionInfo(string PartNum, string CurRevID, string SourceRevID,
            string CurOPCode, string SourceOPCode)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            SimpleCurRight CurRight = GetCurRight();
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            SimpleEWIMain _EWIMain = new SimpleEWIMain();

            if (CurRevID == "")
            {
                returnResponse.MsgCode = "error";
                returnResponse.Msg = GetResValue("Txt_NoRevisionAddFirst");
            }
            else
            {
                string cmdText = string.Format(@"EWI_CopyData {0},{1},'{2}','{3}','{4}'",
                        CurRevID,               //CurRevID
                        SourceRevID,            //SourceRevID
                        CurOPCode,              //CurOPCode
                        SourceOPCode,           //SourceOPCode
                        user.UserName
                        );
                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, cmdText);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
                if (returnResponse.Msg == "")
                {
                    returnResponse.MsgCode = "OK";
                }
            }
            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult InActiveRevision(string RevID)
        {
            SimpleCurRight CurRight = GetCurRight();
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strSql = string.Format(@"EWI_UpdateData N'Active = 0', {0},{1},'{2}','{3}'",
                        RevID, 1, user.UserName, CurRight.Menu);
                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public DataTable dtCriteria, dtImages, dtOPList;

        public JsonResult ExportReport(string PartNum, string RevID, int PrintType)
        {
            string strSql = string.Format("EWI_GetData {0},5", RevID);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
            string FileName = ds.Tables[0].Rows[0]["EWIFileName"].ToString();
            if (PrintType == 2)
                FileName = FileName.Replace(".pdf", ".xls");

            if (FileName == "")
            {
                //生成文件
                FileName = GenerateEWIFile(PartNum, RevID, PrintType);
                //FileName = GenerateDCCEWIFile(PartNum, RevID);
            }

            //导出文件
            string FullFileName = Server.MapPath(@"~/EWIFiles/") + FileName;

            FileStream fs = new FileStream(FullFileName, FileMode.Open);
            byte[] bytes = new byte[(int)fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            fs.Dispose();

            Response.Buffer = true;
            Response.Clear();
            if (PrintType == 1)
                Response.ContentType = "application/pdf";
            else if (PrintType == 2)
                Response.ContentType = "application/vnd.ms-excel";
            Response.AddHeader("content-disposition", "attachment; filename=" + FileName);
            Response.BinaryWrite(bytes); // create the file
            Response.Flush(); // send it to the client to download


            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.MsgCode = "OK";
            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        private void Report_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            switch (e.ReportPath)
            {
                case "EWI_Criteria":
                    e.DataSources.Add(new ReportDataSource("DataSet1", dtCriteria));
                    break;
                case "EWI_Images":
                    e.DataSources.Add(new ReportDataSource("DataSet1", dtImages));
                    break;
                default:
                    e.DataSources.Add(new ReportDataSource("DataSet1", dtOPList));
                    break;
            }
        }
        #endregion

        #region EWI 版本明细
        public ActionResult EWIDetailView(string RevID)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                return RedirectToAction("Index", "Login", new { flag = "ErrorSession" });

            //验证菜单权限
            if (!CheckRight(new string[] { "EWI", "EWI_FA", "EWI_NPI", "EWI_QM", "EWI_QM_FA", "EWI_QM_NPI", "EWI_Check" }))
            {
                return Redirect("/Login/Home");
            }

            string cmdText = @"EWI_GetData 0,3";
            DataSet dsMachineList = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);

            List<string> _MachineList = new List<string>();
            string OP = "";
            foreach (DataRow row in dsMachineList.Tables[0].Rows)
            {
                OP = row["MachineName"].ToString();
                _MachineList.Add(OP);
            }

            SimpleEWIRevision _EWIRevInfo = new SimpleEWIRevision();
            cmdText = string.Format(@"EWI_GetData {0},2", RevID);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                _EWIRevInfo.PartNum = dr["PartNum"].ToString();
                _EWIRevInfo.PartDesc = dr["PartDesc"].ToString();
                _EWIRevInfo.CustomerRev = dr["CustomerRev"].ToString();
                _EWIRevInfo.Revision = dr["Revision"].ToString();
                _EWIRevInfo.ApprovalStatus = dr["ApprovalStatus"].ToString();
                _EWIRevInfo.EditStatus = dr["EditStatus"].ToString();
                _EWIRevInfo.EWIFileName = dr["EWIFileName"].ToString();
                _EWIRevInfo.CustomerFileName = dr["CustomerFileName"].ToString();
                _EWIRevInfo.CADFileName = dr["CADFileName"].ToString();
            }

            ViewBag.MachineList = _MachineList;
            ViewBag.CurRight = (SimpleCurRight)Session["CurRight"];
            ViewBag.EWIRevInfo = _EWIRevInfo;
            return View();
        }

        public JsonResult GetEWIDetailCodeList(string RevID)
        {
            string cmdText = string.Format(@"EWI_Detail_GetBasicInfo {0}", RevID);

            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);

            List<SimpleEWIOP> EWIList = new List<SimpleEWIOP>();
            SimpleEWIOP EWIOP = new SimpleEWIOP();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                EWIOP = new SimpleEWIOP();
                EWIOP.OpCode = row["OpCode"].ToString();
                EWIOP.OpCodeDesc = row["OpCodeDesc"].ToString();
                EWIOP.OprSeq = row["OprSeq"].ToString();

                EWIList.Add(EWIOP);
            }
            DataTablesResult<SimpleEWIOP> result = new DataTablesResult<SimpleEWIOP>(0, 0, 0, EWIList);
            return Json(result);
        }

        public JsonResult GetEWIDetailBasic(string RevID, string OP)
        {
            SimpleEWIBasic bs = new SimpleEWIBasic();
            BaseResponse<SimpleEWIBasic> returnResponse = new BaseResponse<SimpleEWIBasic>();

            string strSql = string.Format("EWI_Detail_GetBasicInfo {0},'{1}',3", RevID, OP);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                bs.ID = row["ID"].ToString();
                bs.OpCode = row["OpCode"].ToString();
                bs.Machine = row["Machine"].ToString();
                bs.OpDesc = row["OpDesc"].ToString();
                bs.AttentionItem = row["AttentionItem"].ToString();
                bs.CycleTime = row["CycleTime"].ToString();
                bs.FixtureNo = row["FixtureNo"].ToString();
                bs.FixtureFilePath = row["FixtureFilePath"].ToString();
                bs.Definition = row["Definition"].ToString();
                bs.ResponsePlan = row["ResponsePlan"].ToString();
                bs.WICode = row["WICode"].ToString();
            }
            returnResponse.Data = bs;
            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEWIDetailCriteria(string RevID, string OP)
        {
            SimpleEWICriteria cr = new SimpleEWICriteria();
            List<SimpleEWICriteria> list = new List<SimpleEWICriteria>();
            BaseResponse<List<SimpleEWICriteria>> returnResponse = new BaseResponse<List<SimpleEWICriteria>>();

            string strSql = string.Format("EWI_Detail_GetBasicInfo {0},'{1}',1", RevID, OP);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                cr = new SimpleEWICriteria();
                cr.ID = row["ID"].ToString();
                cr.CriteriaSeq = row["CriteriaSeq"].ToString();
                cr.Criteria = row["Criteria"].ToString();
                cr.Figure = row["Figure"].ToString();
                cr.Tolerance = row["Tolerance"].ToString();
                cr.TestFrequency = row["TestFrequency"].ToString();
                cr.TestInstrument = row["TestInstrument"].ToString();
                cr.ChangeColumn = row["ChangeColumn"].ToString();
                list.Add(cr);
            }
            returnResponse.Data = list;
            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEWIDetailMedia(string RevID, string OP)
        {
            SimpleEWIMedia md = new SimpleEWIMedia();
            List<SimpleEWIMedia> list = new List<SimpleEWIMedia>();
            BaseResponse<List<SimpleEWIMedia>> returnResponse = new BaseResponse<List<SimpleEWIMedia>>();

            string strSql = string.Format("EWI_Detail_GetBasicInfo {0},'{1}',2", RevID, OP);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
            string urlPrefix = string.Format("{0}://{1}:{2}/", Request.Url.Scheme, Request.Url.Host, Request.Url.Port);
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                md = new SimpleEWIMedia();
                md.ID = row["ID"].ToString();
                md.File = @"<a href='javascript:void(0);' data-val='" + row["FilePath"].ToString() + row["FileName"].ToString() + "' onclick='ViewFile(this)'>" + row["FileName"].ToString() + "</a>";
                md.Desc = row["Desc"].ToString();
                list.Add(md);
            }
            returnResponse.Data = list;
            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateDetailOP(string RevID, List<string> OPList)
        {
            SimpleCurRight CurRight = GetCurRight();

            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strOPList = "";
                foreach (string tmpOP in OPList)
                {
                    strOPList += (strOPList == "" ? "" : ";") + tmpOP;
                }

                string strSql = string.Format("EWI_Detail_UpdateOPList {0},'{1}','{2}','{3}','{4}','{5}'",
                    RevID, strOPList, user.UserName, CurRight.Menu, user.Language,
                    ConfigurationManager.AppSettings["CompanyCode"]);
                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }
            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateDetailBasic(string RevID, string OP, string ColumnName, string NewValue)
        {
            SimpleCurRight CurRight = GetCurRight();
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strSql = string.Format("EWI_Detail_UpdateBasic {0},'{1}','{2}','{3}','{4}','{5}'",
                        RevID, OP, ColumnName, NewValue, user.UserName, CurRight.Menu);

                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateDetailCriteria(string RevID, string OP, string ID, string CriteriaSeq,
            string ColumnName, string NewValue)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strSql = "";
                if (ID == "")
                    strSql = string.Format("EWI_Detail_InsertCriteria {0},'{1}',{2},'{3}',N'{4}','{5}'",
                        RevID, OP, CriteriaSeq, ColumnName, NewValue, user.UserName);
                else
                    strSql = string.Format("EWI_UpdateData N'[{0}]=N''{1}''',{2},{3},'{4}'",
                        ColumnName, NewValue, ID, 2, user.UserName);

                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BacthUpdateDetailCriteria(string RevID, string OP, string DataList)
        {
            SimpleCurRight CurRight = GetCurRight();
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                DataList = DataList.Replace("'", "&apos;");
                string strSql = string.Format("EWI_Detail_BatchUpdateCriteria {0},'{1}',N'{2}','{3}','{4}'",
                        RevID, OP, DataList.Replace("{", "<").Replace("}", ">"), user.UserName, CurRight.Menu);

                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateChangeMark(string CriteriaID, string DataList, string MarkType)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strSql = "";
                if (MarkType == "0")
                {
                    strSql = string.Format("EWI_UpdateData N'[ChangeColumn]=[ChangeColumn] + N''{0}''',{1},{2},'{3}'",
                            DataList, CriteriaID, 2, user.UserName);
                    returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                }
                else
                {
                    string[] ColList = DataList.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string OneCol in ColList)
                    {
                        string TmpCol = OneCol + ";";
                        strSql = string.Format("EWI_UpdateData N'[ChangeColumn]= replace([ChangeColumn],N''{0}'','''')',{1},{2},'{3}'",
                                TmpCol, CriteriaID, 2, user.UserName);
                        returnResponse.Msg += SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                    }
                }
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateDetailMedia(string RevID, string OP, string ID, string CriteriaSeq,
            string ColumnName, string NewValue)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strSql = string.Format("EWI_UpdateData '[{0}]=''{1}''',{2},{3},'{4}'",
                    ColumnName, NewValue, ID, 3, user.UserName);

                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MoveDetailCriteria(string RevID, string OP, string IDList, string SeqList)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strSql = strSql = string.Format("EWI_Detail_MoveSeq {0},'{1}','{2}','{3}','{4}'",
                        RevID, OP, IDList, SeqList, user.UserName);

                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MoveDetailMedia(string RevID, string OP, string IDList, string SeqList)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strSql = strSql = string.Format("EWI_Detail_MoveSeq {0},'{1}','{2}','{3}','{4}',1",
                        RevID, OP, IDList, SeqList, user.UserName);

                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteDetailCriteria(string IDList)
        {
            SimpleCurRight CurRight = GetCurRight();
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strSql = string.Format("EWI_Detail_Delete '{0}','{1}',0,'{2}'",
                    IDList, user.UserName, CurRight.Menu);
                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteDetailMedia(string IDList)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";
            string strSql = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                //删除临时文件夹
                strSql = string.Format("EWI_Detail_GetMedia '{0}','{1}'",
                    IDList, user.UserName);
                DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
                string serverPath = Server.MapPath("..\\UploadFiles\\");
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string tmpPath = dr[1].ToString() == "PDF" ? "PDF\\" : "Images\\";
                    if (System.IO.File.Exists(serverPath + tmpPath + dr[0].ToString()))
                    {
                        System.IO.File.Delete(serverPath + tmpPath + dr[0].ToString());
                    }
                }

                //删除数据库
                strSql = string.Format("EWI_Detail_Delete '{0}','{1}',1",
                    IDList, user.UserName);
                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTotalOPCodeList(string RevID)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string cmdText = string.Format(@"EWI_Detail_GetOPList '{0}','{1}','{2}'",
                    RevID,
                    user.Language,
                    ConfigurationManager.AppSettings["CompanyCode"]
                );
            DataSet dsOPList = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);

            List<SimpleOpMaster> _OPList = new List<SimpleOpMaster>();
            SimpleOpMaster OP = new SimpleOpMaster();
            foreach (DataRow row in dsOPList.Tables[0].Rows)
            {
                OP = new SimpleOpMaster();
                OP.OpCode = row["OpCode"].ToString();
                OP.OpDesc = row["OpDesc"].ToString();
                _OPList.Add(OP);
            }

            DataTablesResult<SimpleOpMaster> result = new DataTablesResult<SimpleOpMaster>(0, 0, 0, _OPList);
            return Json(result);
        }

        public JsonResult GetTestInstrument()
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string cmdText = string.Format(@"EWI_GetData 0,1,'{0}'", user.Language);
            DataSet dsOPList = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);

            List<string> _OPList = new List<string>();
            string OP = "";
            foreach (DataRow row in dsOPList.Tables[0].Rows)
            {
                OP = row["TestInstrument"].ToString();
                _OPList.Add(OP);
            }

            DataTablesResult<string> result = new DataTablesResult<string>(0, 0, 0, _OPList);
            return Json(result);
        }

        /// <summary>
        /// 生成PDF/EXCEL到目录EWIFiles[未盖章]
        /// </summary>
        /// <param name="PartNum">产品编号</param>
        /// <param name="RevID">版本内码</param>
        /// <param name="PrintType">生成类型 0-全部 1-PDF 2-EXCEL</param>
        /// <returns>生成的文件名</returns>
        public string GenerateEWIFile(string PartNum, string RevID, int PrintType = 0)
        {
            string strSql = "", RdlcName = "", FileName = "";
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            RdlcName = @"EWI.rdlc";

            //生成文件
            LocalReport report = new LocalReport();
            report.EnableExternalImages = true;
            report.ReportPath = Server.MapPath(@"~/Reports/") + RdlcName;
            strSql = string.Format("EWI_Report '{0}',{1},'{2}'", PartNum, RevID, user.Language);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
            string Path = "";

            report.DataSources.Add(new ReportDataSource("DataSet1", ds.Tables[0]));
            report.DataSources.Add(new ReportDataSource("DataSet2", ds.Tables[1]));
            report.DataSources.Add(new ReportDataSource("DataSet3", ds.Tables[2]));
            report.DataSources.Add(new ReportDataSource("DataSet4", ds.Tables[6]));
            dtCriteria = ds.Tables[3];
            dtImages = ds.Tables[4];
            dtOPList = ds.Tables[5];
            foreach (DataRow dr in dtImages.Rows)
            {
                Path = Server.MapPath("~/UploadFiles/Images/") + dr["FileName"].ToString();
                dr["FileName"] = "file:///" + Path.Replace("\\", "/");
            }
            report.SubreportProcessing += new SubreportProcessingEventHandler(Report_SubreportProcessing);

            Warning[] warnings;
            string[] streamids;
            string mimeType, encoding, extension;
            byte[] bytes;
            string FullFileName;
            FileStream fs;

            //生成EXCEL
            if (PrintType == 0 || PrintType == 2)
            {
                FileName = HttpUtility.UrlEncode(PartNum + "-EWI-" + ds.Tables[0].Rows[0]["WIType"].ToString() + "-Rev." + ds.Tables[2].Rows[0]["Revision"].ToString() + ".xls", Encoding.UTF8);
                bytes = report.Render("Excel", null, out mimeType, out encoding,
                    out extension, out streamids, out warnings);

                FullFileName = Server.MapPath("~/EWIFiles/") + FileName;
                if (System.IO.File.Exists(FullFileName))
                {
                    System.IO.File.Delete(FullFileName);
                }
                fs = new FileStream(FullFileName, FileMode.Create);
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
                fs.Dispose();
            }

            //生成PDF
            if (PrintType == 0 || PrintType == 1)
            {
                FileName = HttpUtility.UrlEncode(PartNum + "-EWI-" + ds.Tables[0].Rows[0]["WIType"].ToString() + "-Rev." + ds.Tables[2].Rows[0]["Revision"].ToString() + ".pdf", Encoding.UTF8);
                bytes = report.Render("PDF", null, out mimeType, out encoding,
                    out extension, out streamids, out warnings);

                FullFileName = Server.MapPath("~/EWIFiles/") + FileName;
                if (System.IO.File.Exists(FullFileName))
                {
                    System.IO.File.Delete(FullFileName);
                }
                fs = new FileStream(FullFileName, FileMode.Create);
                fs.Write(bytes, 0, bytes.Length);

                fs.Close();
                fs.Dispose();
            }
            return FileName;
        }

        public JsonResult ReGenerateEWIFile(string PartNum, string RevID)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";
            GenerateEWIFile(PartNum, RevID);
            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 重新生成带水印的文件[正常]
        /// </summary>
        /// <param name="PartNum">产品编号</param>
        /// <param name="RevID">版本内码</param>
        public string GenerateDCCEWIFile(string PartNum, string RevID)
        {
            string FileName = GenerateEWIFile(PartNum, RevID);

            string strSql = string.Format("EWI_GetData {0},5", RevID);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
            string FullFileName = Server.MapPath(@"~/EWIFiles/") + FileName;
            int IniReduce = ds.Tables[0].Rows[0]["Revision"].ToString() == "A" ? 1 : 0;

            //重新生成带DCC章的PDF
            //读取PDF，基本设置
            PdfDocument document = PdfReader.Open(FullFileName, PdfDocumentOpenMode.Modify);
            //font
            XFont font = new XFont("Verdana", 94);
            XFont font2 = new XFont("Verdana", 12);
            //brush.
            XBrush brush = new XSolidBrush(XColor.FromArgb(128, 235, 235, 235));
            XBrush brush2 = new XSolidBrush(XColor.FromArgb(128, 255, 0, 0));
            //format.
            var format = new XStringFormat();
            format.Alignment = XStringAlignment.Near;
            format.LineAlignment = XLineAlignment.Near;

            string watermark = "Confidential";
            string DccImage = Server.MapPath("~/EWIFiles/DccControl.png");
            string dt = DateTime.Now.ToString("yyyy.MM.dd");

            for (int i = 0; i < document.Pages.Count; i++)
            {
                PdfPage page = document.Pages[i];

                //水印
                // Get an XGraphics object for drawing beneath the existing content.
                var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);
                // Get the size (in points) of the text.
                var size = gfx.MeasureString(watermark, font);
                // Define a rotation transformation at the center of the page.
                gfx.TranslateTransform(page.Width / 2, page.Height / 2);
                gfx.RotateTransform(-Math.Atan(page.Height / page.Width) * 180 / Math.PI);
                gfx.TranslateTransform(-page.Width / 2, -page.Height / 2);
                // Draw the string.
                gfx.DrawString(watermark, font, brush,
                    new XPoint((page.Width - size.Width) / 2, (page.Height - size.Height) / 2),
                    format);
                gfx.Dispose();

                //加图片
                var gfx2 = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);
                XImage xImage = XImage.FromFile(DccImage);
                if ((i + IniReduce) == 0)
                    gfx2.DrawImage(xImage, new XPoint(690, 142));
                else if ((i + IniReduce) == 1)
                    gfx2.DrawImage(xImage, new XPoint(690, 110));
                else
                    gfx2.DrawImage(xImage, new XPoint(443, 15));

                //写日期
                if ((i + IniReduce) == 0)
                    gfx2.DrawString(dt, font2, brush2, new XPoint(750, 170), format);
                else if ((i + IniReduce) == 1)
                    gfx2.DrawString(dt, font2, brush2, new XPoint(750, 138), format);
                else
                    gfx2.DrawString(dt, font2, brush2, new XPoint(503, 43), format);
            }
            document.Save(FullFileName);
            document.Close();
            document.Dispose();

            return FileName;
        }

        /// <summary>
        /// 重新生成临时的文件[FA]
        /// </summary>
        /// <param name="PartNum">产品编号</param>
        /// <param name="RevID">版本内码</param>
        /// <param name="ReGenerateFile">是否重新生成PDF</param>
        public void GenerateTempEWIFile(string PartNum, string RevID, string NSType, bool ReGenerateFile = true)
        {
            if (ReGenerateFile == true)
                GenerateEWIFile(PartNum, RevID);

            string strSql = string.Format("EWI_GetData {0},5", RevID);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
            string FullFileName = Server.MapPath(@"~/EWIFiles/") + ds.Tables[0].Rows[0]["EWIFileName"].ToString();

            //重新生成带DCC章的PDF
            //读取PDF，基本设置
            PdfDocument document = PdfReader.Open(FullFileName, PdfDocumentOpenMode.Modify);
            //font
            XFont font = new XFont("Verdana", 94);
            XFont font2 = new XFont("Verdana", 12);
            //brush.
            XBrush brush = new XSolidBrush(XColor.FromArgb(128, 235, 235, 235));
            XBrush brush2 = new XSolidBrush(XColor.FromArgb(128, 255, 0, 0));
            //format.
            var format = new XStringFormat();
            format.Alignment = XStringAlignment.Near;
            format.LineAlignment = XLineAlignment.Near;

            string DccImage = "";
            switch (NSType)
            {
                case "FA":
                    DccImage = Server.MapPath("~/EWIFiles/FAControl.png");
                    break;
                case "EngTest":
                    DccImage = Server.MapPath("~/EWIFiles/EngTestControl.png");
                    break;
                case "TempDoc":
                    DccImage = Server.MapPath("~/EWIFiles/TempDocControl.png");
                    break;
                case "NPI":
                    DccImage = Server.MapPath("~/EWIFiles/NPIControl.png");
                    break;
                default:
                    break;
            }
            string dt = DateTime.Now.ToString("yyyy.MM.dd");

            for (int i = 0; i < document.Pages.Count; i++)
            {
                PdfPage page = document.Pages[i];

                //加图片
                var gfx2 = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);
                XImage xImage = XImage.FromFile(DccImage);
                if (i == 0)
                    gfx2.DrawImage(xImage, new XPoint(690, 158));
                else
                    gfx2.DrawImage(xImage, new XPoint(444, 15));

                //写日期
                if (NSType == "NPI" || NSType == "FA")
                {
                    if (i == 0)
                        gfx2.DrawString(dt, font2, brush2, new XPoint(725, 183), format);
                    else
                        gfx2.DrawString(dt, font2, brush2, new XPoint(479, 40), format);
                }
            }
            document.Save(FullFileName);
            document.Close();
            document.Dispose();
        }

        public JsonResult SubmitRev(string RevID, int SubmitType = 0, int CheckType = 1,
            string CheckReason = "", string PartNum = "")
        {
            SimpleCurRight CurRight = (SimpleCurRight)Session["CurRight"];
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
            {
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
                return Json(returnResponse, JsonRequestBehavior.AllowGet);
            }

            //更新状态
            string strSql = "";
            strSql = string.Format(@"EWI_GetData {0},5", RevID);
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);

            if (CheckType == 1)
            {
                //通过
                string EWIFileName = "";
                switch (SubmitType)
                {
                    case 0://提交
                        EWIFileName = GenerateEWIFile(PartNum, RevID);
                        //string IdenQMOrENG = QMUser.IndexOf(user.UserName + ";") > 0 ? "品质 " : "工程 ";
                        strSql = string.Format("EWI_UpdateData '[EWIFileName]=''{0}''',{1},{2},'{3}'",
                            EWIFileName, RevID, 4, user.UserName);
                        break;
                    case 1://主管
                        strSql = string.Format("EWI_UpdateData '[ReviewBy]=''{2}'',[ReviewDate]=getdate()',{0},{1},'{2}'",
                            RevID, 5, user.UserName);
                        break;
                    case 2://经理
                        strSql = string.Format("EWI_UpdateData '[ApproveBy]=''{2}'',[ApproveDate]=getdate()',{0},{1},'{2}'",
                            RevID, 6, user.UserName);
                        break;
                    case 3://ECN
                        strSql = string.Format("EWI_UpdateData '',{0},{1},'{2}'",
                            RevID, 10, user.UserName);
                        break;
                    case 31://主管 ECN
                        if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "1")
                            strSql = string.Format("EWI_UpdateData '[ReviewBy]=''{2}'',[ReviewDate]=getdate()',{0},{1},'{2}'",
                            RevID, 5, user.UserName);
                        else if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "3")
                            strSql = string.Format("EWI_UpdateData '',{0},{1},'{2}'",
                            RevID, 10, user.UserName);
                        break;
                    case 32://经理 ECN
                        if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "2")
                            strSql = string.Format("EWI_UpdateData '[ApproveBy]=''{2}'',[ApproveDate]=getdate()',{0},{1},'{2}'",
                            RevID, 6, user.UserName);
                        else if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "3")
                            strSql = string.Format("EWI_UpdateData '',{0},{1},'{2}'",
                            RevID, 10, user.UserName);
                        break;
                    case 4://DCC
                        strSql = string.Format("EWI_UpdateData '[DCCApproveBy]=''{2}'',[DCCApproveDate]=getdate()',{0},{1},'{2}'",
                            RevID, 7, user.UserName);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //拒绝
                switch (SubmitType)
                {
                    case 0://品质
                        strSql = string.Format("EWI_UpdateData '[ReviewDate]=getdate()',{0},{1},'{2}',N'{3}'",
                            RevID, 14, user.UserName, CheckReason);
                        break;
                    case 1://主管
                        strSql = string.Format("EWI_UpdateData '[ReviewDate]=getdate()',{0},{1},'{2}',N'{3}'",
                            RevID, 15, user.UserName, CheckReason);
                        break;
                    case 2://经理
                        strSql = string.Format("EWI_UpdateData '[ApproveBy]='''',[ApproveDate]=getdate()',{0},{1},'{2}',N'{3}'",
                            RevID, 16, user.UserName, CheckReason);
                        break;
                    case 3://ECN
                        strSql = string.Format("EWI_UpdateData '[DCCApproveBy]='''',[DCCApproveDate]=getdate()',{0},{1},'{2}',N'{3}'",
                            RevID, 20, user.UserName, CheckReason);
                        break;
                    case 31://主管 ECN
                        if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "1")
                            strSql = string.Format("EWI_UpdateData '[ReviewDate]=getdate()',{0},{1},'{2}',N'{3}'",
                            RevID, 15, user.UserName, CheckReason);
                        else if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "3")
                            strSql = string.Format("EWI_UpdateData '[DCCApproveBy]='''',[DCCApproveDate]=getdate()',{0},{1},'{2}',N'{3}'",
                            RevID, 20, user.UserName, CheckReason);
                        break;
                    case 32://经理 ECN
                        if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "2")
                            strSql = string.Format("EWI_UpdateData '[ApproveBy]='''',[ApproveDate]=getdate()',{0},{1},'{2}',N'{3}'",
                            RevID, 16, user.UserName, CheckReason);
                        else if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "3")
                            strSql = string.Format("EWI_UpdateData '[DCCApproveBy]='''',[DCCApproveDate]=getdate()',{0},{1},'{2}',N'{3}'",
                            RevID, 20, user.UserName, CheckReason);
                        break;
                    case 4://DCC
                        strSql = string.Format("EWI_UpdateData '[DCCApproveBy]='''',[DCCApproveDate]=getdate()',{0},{1},'{2}',N'{3}'",
                            RevID, 17, user.UserName, CheckReason);
                        break;
                    default:
                        break;
                }
            }
            returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);

            //FA，则主管审批时生成盖章文件 [先更新，后生成PDF，否则没有生效日期]
            if (ds.Tables[0].Rows[0]["WIType"].ToString() == "FA" && SubmitType == 1)
                GenerateTempEWIFile(PartNum, RevID, ds.Tables[0].Rows[0]["NSType"].ToString());
            //NonStd,
            if (ds.Tables[0].Rows[0]["WIType"].ToString() == "Temp" && SubmitType == 0)
                GenerateTempEWIFile(PartNum, RevID, ds.Tables[0].Rows[0]["NSType"].ToString(), false);
            if (SubmitType == 4)
                GenerateDCCEWIFile(PartNum, RevID);

            //发送邮件
            if (returnResponse.Msg == "")
            {
                string EmailHead = "";
                string EmailTable = "<html><head><title>T</title></head><style type='text/css'>td{border:solid thin LightGrey;} table{border:solid thin LightGrey;}</style><body><div>" + GetResValue("Txt_EWIRevisionInfor") + ":</div><table cellspacing='0'>"
                    + "<tr><td>" + GetResValue("Txt_PartNumber") + "</td><td>" + GetResValue("Txt_Description") + "</td><td>" + GetResValue("Txt_Revision") + "</td><td>" + GetResValue("Txt_Type") + "</td><td>" + GetResValue("Txt_ChangeDescription") + "</td></tr><tr>";
                strSql = string.Format("EWI_GetData {0},2", RevID);
                ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
                string BtnView = "";

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    EmailTable += "<td>" + dr["PartNum"] + "</td>";
                    EmailTable += "<td>" + dr["PartDesc"] + "</td>";
                    EmailTable += "<td>" + dr["Revision"] + "</td>";
                    EmailTable += "<td>" + dr["ProductStage"] + "</td>";
                    EmailTable += "<td>" + dr["Description"] + "</td>";
                    EmailTable += "</tr><tr>";
                    EmailHead = dr["Iniatior"] + GetResValue("Txt_SubmitEWI");
                    BtnView = dr["PartNum"].ToString();
                }

                string ToEmail = "";
                //重新刷新状态
                strSql = string.Format(@"EWI_GetData {0},5", RevID);
                ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
                if (CheckType == 1)
                {
                    switch (SubmitType)
                    {
                        case 0:
                            //提交审批
                            if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "9")
                                ToEmail = ConfigurationManager.AppSettings["EngEmail"];
                            else if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "3")
                                ToEmail = ds.Tables[0].Rows[0]["RelateEmail"].ToString();
                            else
                                ToEmail = ConfigurationManager.AppSettings["ReviewEmail"];
                            break;
                        case 1:
                            //主管
                            if (ds.Tables[0].Rows[0]["WIType"].ToString() == "Normal")
                                ToEmail = ConfigurationManager.AppSettings["ApproveEmail"];
                            else
                                ToEmail = ConfigurationManager.AppSettings["EngEmail"];
                            EmailHead = user.UserName + GetResValue("Txt_ApproveEWI");
                            break;
                        case 2:
                            //经理
                            if ((ds.Tables[0].Rows[0]["WIType"].ToString() == "Normal")
                                && (ds.Tables[0].Rows[0]["RelateEmail"].ToString() != ""))
                                ToEmail = ds.Tables[0].Rows[0]["RelateEmail"].ToString();
                            else
                                ToEmail = ConfigurationManager.AppSettings["DccEmail"];
                            EmailHead = user.UserName + GetResValue("Txt_ApproveEWI");
                            break;
                        case 3:
                            //ECN
                            if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "3")
                                ToEmail = "";
                            else
                                ToEmail = ConfigurationManager.AppSettings["DccEmail"];
                            EmailHead = user.UserName + GetResValue("Txt_ApproveEWI");
                            break;
                        case 31://主管 ECN
                            if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "1")
                            {
                                if (ds.Tables[0].Rows[0]["WIType"].ToString() == "Normal")
                                    ToEmail = ConfigurationManager.AppSettings["ApproveEmail"];
                                else
                                    ToEmail = ConfigurationManager.AppSettings["EngEmail"];
                                EmailHead = user.UserName + GetResValue("Txt_ApproveEWI");
                            }
                            else if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "3")
                            {
                                if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "3")
                                    ToEmail = "";
                                else
                                    ToEmail = ConfigurationManager.AppSettings["DccEmail"];
                                EmailHead = user.UserName + GetResValue("Txt_ApproveEWI");
                            }
                            break;
                        case 32://经理 ECN
                            if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "1")
                            {
                                if ((ds.Tables[0].Rows[0]["WIType"].ToString() == "Normal")
                                && (ds.Tables[0].Rows[0]["RelateEmail"].ToString() != ""))
                                    ToEmail = ds.Tables[0].Rows[0]["RelateEmail"].ToString();
                                else
                                    ToEmail = ConfigurationManager.AppSettings["DccEmail"];
                                EmailHead = user.UserName + GetResValue("Txt_ApproveEWI");
                            }
                            else if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "3")
                            {
                                if (ds.Tables[0].Rows[0]["ApprovalStatus"].ToString() == "3")
                                    ToEmail = "";
                                else
                                    ToEmail = ConfigurationManager.AppSettings["DccEmail"];
                                EmailHead = user.UserName + GetResValue("Txt_ApproveEWI");
                            }
                            break;
                        case 4:
                            //DCC
                            ToEmail = ConfigurationManager.AppSettings["EngEmail"];
                            EmailHead = user.UserName + GetResValue("Txt_ApproveFinishEWI");
                            break;
                        default:
                            break;
                    }
                    BtnView = "<br /><div><a href='" + string.Format("{0}://{1}:{2}/", Request.Url.Scheme, Request.Url.Host, Request.Url.Port)
                        + "EWI/EWICheckView?PartNum=" + BtnView + "'>" + GetResValue("Txt_ClickToView") + "</a></div>";
                }
                else
                {
                    ToEmail = ConfigurationManager.AppSettings["EngEmail"];
                    BtnView = "<br /><div><a href='" + string.Format("{0}://{1}:{2}/", Request.Url.Scheme, Request.Url.Host, Request.Url.Port)
                        + "EWI/EWIMainView?PartNum=" + BtnView + "'>" + GetResValue("Txt_ClickToView") + "</a></div>";
                    EmailHead = user.UserName + GetResValue("Txt_RejectEWI");
                }

                EmailTable = EmailTable.Substring(0, EmailTable.Length - 4);
                EmailTable += "</table>" + BtnView + "</body></html>";

                if (ToEmail != "")
                    SendEmailWeb(ToEmail, EmailHead, EmailTable);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        //提交品质
        public JsonResult NotifyEmail(string RevID, int NotifyType = 8)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strSql = string.Format("EWI_UpdateData '',{0},{1},'{2}'",
                    RevID, NotifyType, user.UserName);
                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);

                string EmailHead = "";
                string EmailTable = "<html><head><title>T</title></head><style type='text/css'>td{border:solid thin LightGrey;} table{border:solid thin LightGrey;}</style><body><div>" + GetResValue("Txt_EWIRevisionInfor") + ":</div><table cellspacing='0'>"
                        + "<tr><td>" + GetResValue("Txt_PartNumber") + "</td><td>" + GetResValue("Txt_Description") + "</td><td>" + GetResValue("Txt_Revision") + "</td><td>" + GetResValue("Txt_Type") + "</td><td>" + GetResValue("Txt_ChangeDescription") + "</td></tr><tr>";
                strSql = string.Format("EWI_GetData {0},2", RevID);
                DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
                string BtnView = "";

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    EmailTable += "<td>" + dr["PartNum"] + "</td>";
                    EmailTable += "<td>" + dr["PartDesc"] + "</td>";
                    EmailTable += "<td>" + dr["Revision"] + "</td>";
                    EmailTable += "<td>" + dr["ProductStage"] + "</td>";
                    EmailTable += "<td>" + dr["Description"] + "</td>";
                    EmailTable += "</tr><tr>";
                }

                string ToEmail = "";
                switch (NotifyType)
                {
                    case 8:
                        ToEmail = ConfigurationManager.AppSettings["QMEmail"];
                        EmailHead = GetResValue("Txt_EWIParameterMaintained");
                        BtnView = "EWI_QM";
                        break;
                    case 10:
                        ToEmail = ConfigurationManager.AppSettings["EngEmail"];
                        EmailHead = GetResValue("Txt_QMSubmitEWI");
                        BtnView = "EWI";
                        break;
                    default:
                        break;
                }

                BtnView = "<br /><div><a href='" + string.Format("{0}://{1}:{2}/", Request.Url.Scheme, Request.Url.Host, Request.Url.Port)
                    + "EWI/EWIView?RoleName=" + BtnView + "'>" + GetResValue("Txt_ClickToView") + "</a></div>";


                EmailTable = EmailTable.Substring(0, EmailTable.Length - 4);
                EmailTable += "</table>" + BtnView + "</body></html>";

                SendEmailWeb(ToEmail, EmailHead, EmailTable);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult NofityStatus(string RevID, int NotifyType = 9)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string strSql = string.Format("EWI_UpdateData '',{0},{1},'{2}'",
                    RevID, NotifyType, user.UserName);
                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        //过时的方法可以发送邮件成功
        public void SendEmailWeb(string mailToAddress, string Head, string Content)
        {
            SmtpMail.SmtpServer = ServerName;
            System.Web.Mail.MailMessage mailMessage = new System.Web.Mail.MailMessage();
            mailMessage.From = MailFromAddress;
            mailMessage.To = mailToAddress;
            mailMessage.Body = Content;
            mailMessage.Subject = Head;
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.BodyFormat = MailFormat.Html;
            SmtpMail.Send(mailMessage);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="mailToAddress">发送人</param>
        /// <param name="Head">邮件标题</param>
        /// <param name="Content">邮件内容</param>
        public void SendEmail(string mailToAddress, string Head, string Content)
        {
            //新建SmtpClient类实例smtpClient对象，using语句块结束时释放smtpClient对象
            using (var smtpClient = new SmtpClient())
            {
                //设置是否使用SSL协议连接
                smtpClient.EnableSsl = UseSsl;
                //设置SMTP服务器名
                smtpClient.Host = ServerName;
                //设置SMTP服务器的端口号
                smtpClient.Port = ServerPort;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                //设置SMTP服务器发送邮件的凭据（用户名和授权码)
                smtpClient.Credentials = new NetworkCredential(UserName, "123", DoMain);
                System.Net.Mail.MailMessage mailMessage = new System.Net.Mail.MailMessage(
                                       MailFromAddress, // 发件人邮箱
                                       mailToAddress,   // 收件人邮箱
                                       Head,            // 电子邮件主题
                                       Content);        // 电子邮件内容
                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                mailMessage.IsBodyHtml = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                try
                {   //如不在线，则发送不成功
                    smtpClient.Send(mailMessage);
                }
                catch (Exception ex)
                { }
            }
        }

        //上传文件
        public JsonResult UploadFile()
        {
            SimpleCurRight CurRight = GetCurRight();
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                string PartNum = Request.Form["PartNum"];
                string Revision = Request.Form["Revision"];
                string RevID = Request.Form["RevID"];
                string OP = Request.Form["OP"];
                string dataURL = Request.Form["UploadFile"].Replace("data:image/png;base64,", "");//将base64头部信息替换
                byte[] bytes = Convert.FromBase64String(dataURL);
                MemoryStream memStream = new MemoryStream(bytes);
                Image curImage = Image.FromStream(memStream);
                Random rdm = new Random();
                string FileName = string.Format("{0}-Rev.{1}-OP.{2}-{3}.png",
                    PartNum,
                    Revision,
                    OP,
                    DateTime.Now.ToString("yyMMddhhmmss")
                    );
                string serverPathImg = Server.MapPath("..\\UploadFiles\\Images\\") + FileName;
                curImage.Save(serverPathImg);

                string FilePath = string.Format("{0}://{1}:{2}/UploadFiles/",
                    Request.Url.Scheme, Request.Url.Host, Request.Url.Port);
                string strSql = string.Format("EWI_Detail_InsertImage {0},'{1}','{2}','{3}','{4}','{5}'",
                    RevID, OP, FilePath, FileName, user.UserName, CurRight.Menu);
                returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                if (returnResponse.Msg != "")
                    returnResponse.Msg = GetResValue(returnResponse.Msg);
            }

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region EWI 审批
        public ActionResult EWICheckView(string PartNum = "")
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
            {
                if (PartNum != "")
                {
                    TempData["Action"] = "EWICheckView";
                    TempData["Controller"] = "EWI";
                    TempData["PartNum"] = PartNum;
                }
                //return RedirectToAction("Index", "Login",
                //    new { strPartNum = PartNum });
                Redirect("/Login/Index");
            }
            else
            {
                SimpleCurRight CurRight = new SimpleCurRight();
                string CurUser = user.UserName + ";";
                CurRight.CheckRight = "";
                CurRight.Menu = "EWI_Check";

                if (ReviewUser.IndexOf(CurUser) > -1)
                    CurRight.CheckRight = "1" + CurRight.CheckRight;
                if (ApproveUser.IndexOf(CurUser) > -1)
                    CurRight.CheckRight = "2" + CurRight.CheckRight;
                if (ECNUser.IndexOf(CurUser) > -1)
                    CurRight.CheckRight = "3" + CurRight.CheckRight;
                if (DccUser.IndexOf(CurUser) > -1)
                    CurRight.CheckRight = "4" + CurRight.CheckRight;
                if (CurRight.CheckRight == "")
                    CurRight.CheckRight = "0";

                Session["CurRight"] = CurRight;
            }

            return View();
        }

        public JsonResult GetCheckList(DataTablesParameters param, string QueryString)
        {
            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            SimpleCurRight CurRight = (SimpleCurRight)Session["CurRight"];
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
            else if (!string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = param.Columns[1].Search.Value;
                filterType = 1;
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                filter = param.Columns[2].Search.Value;
                filterType = 2;
            }
            else if (QueryString != "")
            {
                filter = QueryString;
                filterType = 2;
            }

            string cmdText = string.Format(@"EWI_Check_GetList {0},{1},'{2}','{3}',{4},{5},'{6}'",
                    param.Length,                                       //{0}PageSize
                    param.Start,                                        //{1}PageStart
                    (param.OrderBy == "" ? "PartNum" : param.OrderBy) + " " + param.OrderDir,               //{2}OrderBy Column + OrderBy Direction ASC/DESC
                    filter,                                             //{3}QueryString
                    filterType,                                         //{4}QueryType
                    CurRight.CheckRight,
                    user.Language
                );

            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);
            int recordsTotal = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());

            List<SimpleEWICheck> EWIList = new List<SimpleEWICheck>();
            SimpleEWICheck EWICheck = new SimpleEWICheck();
            foreach (DataRow row in ds.Tables[1].Rows)
            {
                EWICheck = new SimpleEWICheck();
                EWICheck.ID = row["ID"].ToString();
                EWICheck.PartNum = row["PartNum"].ToString();
                EWICheck.PartDesc = row["PartDesc"].ToString();
                EWICheck.Revision = row["Revision"].ToString();
                EWICheck.Description = row["Description"].ToString();
                EWICheck.Iniatior = row["Iniatior"].ToString();
                EWICheck.CanCheck = row["CanCheck"].ToString();
                EWICheck.StatusDesc = row["StatusDesc"].ToString();
                EWICheck.StatusLog = row["StatusLog"].ToString();
                EWICheck.WIType = row["WIType"].ToString();
                EWICheck.CreateDate = Convert.ToDateTime(row["CreateDate"]).ToString("yyyy-MM-dd");

                EWIList.Add(EWICheck);
            }
            DataTablesResult<SimpleEWICheck> result = new DataTablesResult<SimpleEWICheck>(param.Draw, recordsTotal, recordsTotal, EWIList);
            return Json(result);
        }

        #endregion
    }
}
