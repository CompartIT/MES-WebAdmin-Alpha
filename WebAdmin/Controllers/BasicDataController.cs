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
using System.Net;
using Microsoft.Reporting.WebForms;
using System.Text;
using System.Runtime.InteropServices;

namespace WebAdmin.Controllers
{
    public class BasicDataController : BaseController
    {
        #region BasicData-EWI
        public ActionResult EWIBasicView()
        {
            //检查菜单权限
            if (!CheckRight("EWIBasic"))
            {
                return Redirect("/Login/Home");
            }
            string strSql = "EWI_GetData 0,4";
            DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);
            List<string> CategoryList = new List<string>();
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                CategoryList.Add(dr[0].ToString());
            }

            ViewBag.CategoryList = CategoryList;
            return View();
        }

        public JsonResult SyncEWIPartNum()
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.MsgCode = "OK";
            returnResponse.Msg = "";

            string cmdText = string.Format("EWI_SyncData 1,'',{0}", ConfigurationManager.AppSettings["CompanyCode"]);
            returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, cmdText);
            if(returnResponse.Msg != "")
                returnResponse.Msg = GetResValue(returnResponse.Msg);

            if (returnResponse.Msg != "")
                returnResponse.MsgCode = "error";

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取EWI基础数据
        /// </summary>
        /// <param name="param">查询参数</param>
        /// <param name="TableType">表类型：0-产品 1-机型 2-仪器</param>
        /// <param name="IsInitial">是否初始化空数据</param>
        /// <returns>Json格式表</returns>
        public JsonResult GetEWIList(DataTablesParameters param, int TableType = 0, bool IsInitial = false)
        {
            string filter = string.Empty;
            int filterType = 0;

            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                filter = param.Search.Value;
                filterType = 10;
            }
            else if (param.Columns.Count > 0 && !string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = param.Columns[0].Search.Value;
                filterType = 0;
            }
            else if (param.Columns.Count > 1 && !string.IsNullOrEmpty(param.Columns[1].Search.Value))
            {
                filter = param.Columns[1].Search.Value;
                filterType = 1;
            }
            else if (param.Columns.Count > 2 && !string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                filter = param.Columns[2].Search.Value;
                filterType = 2;
            }
            else if (param.Columns.Count > 3 && !string.IsNullOrEmpty(param.Columns[3].Search.Value))
            {
                filter = param.Columns[3].Search.Value;
                filterType = 3;
            }

            int recordsTotal = 0;
            List<SimpleBasicData> BasicDataList = new List<SimpleBasicData>();
            List<SimpleMachine> MachineList = new List<SimpleMachine>();
            List<SimpleTestInstrument> TestInstrumentList = new List<SimpleTestInstrument>();
            if (!IsInitial)
            {
                string DefaultOrder = TableType == 0 ? "PartNum " : (TableType == 1 ? "MachineName " : "TestInstrument ");

                string cmdText = string.Format(@"BasicData_GetEWIList {0},{1},'{2}','{3}',{4},{5}",
                        param.Length,                                       //{0}PageSize
                        param.Start,                                        //{1}PageStart
                        (param.OrderBy == "" ? (DefaultOrder + DataTablesOrderDir.Asc) : (param.OrderBy + " " + param.OrderDir)),               //{2}OrderBy Column + OrderBy Direction ASC/DESC
                        filter,                                             //{3}QueryString
                        filterType,                                         //{4}QueryType
                        TableType                                           //{5}表格类型
                    );

                DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, cmdText);
                recordsTotal = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());

                if (TableType == 0)
                {
                    SimpleBasicData basicData = new SimpleBasicData();
                    foreach (DataRow row in ds.Tables[1].Rows)
                    {
                        basicData = new SimpleBasicData();
                        basicData.PartNum = row["PartNum"].ToString();
                        basicData.PartDesc = row["PartDesc"].ToString();
                        basicData.CustomerPartNum = row["CustomerPartNum"].ToString();
                        basicData.Revision = row["RevisionNum"].ToString();
                        basicData.AltMethod = row["AltMethod"].ToString();
                        basicData.OPList = row["OPList"].ToString();

                        BasicDataList.Add(basicData);
                    }
                }
                else if (TableType == 1)
                {
                    SimpleMachine basicMachine = new SimpleMachine();
                    foreach (DataRow row in ds.Tables[1].Rows)
                    {
                        basicMachine = new SimpleMachine();
                        basicMachine.MachineName = row["MachineName"].ToString();
                        basicMachine.MachineType = row["MachineType"].ToString();

                        MachineList.Add(basicMachine);
                    }
                }
                else
                {
                    SimpleTestInstrument basicTestInstrument = new SimpleTestInstrument();
                    foreach (DataRow row in ds.Tables[1].Rows)
                    {
                        basicTestInstrument = new SimpleTestInstrument();
                        basicTestInstrument.TestInstrument = row["TestInstrument"].ToString();
                        basicTestInstrument.TestInstrument_EN = row["TestInstrument_EN"].ToString();
                        basicTestInstrument.Category = row["Category"].ToString();

                        TestInstrumentList.Add(basicTestInstrument);
                    }
                }
            }

            if (TableType == 0)
            {
                DataTablesResult<SimpleBasicData> result = new DataTablesResult<SimpleBasicData>(param.Draw, recordsTotal, recordsTotal, BasicDataList);
                return Json(result);
            }
            else if(TableType == 1)
            {
                DataTablesResult<SimpleMachine> result = new DataTablesResult<SimpleMachine>(param.Draw, recordsTotal, recordsTotal, MachineList);
                return Json(result);
            }
            else
            {
                DataTablesResult<SimpleTestInstrument> result = new DataTablesResult<SimpleTestInstrument>(param.Draw, recordsTotal, recordsTotal, TestInstrumentList);
                return Json(result);
            }
        }

        public JsonResult DeleteTestInstrument(string TestInstrument)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string strSql = string.Format("BasicData_EWI_Delete N'{0}','{1}',{2}",
                    TestInstrument, user.UserName, 0);

            returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
            if (returnResponse.Msg != "")
                returnResponse.Msg = GetResValue(returnResponse.Msg);

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SubmitTestInstrument(string TestInstrument, string TestInstrument_EN, string Category,
            string SubmitType)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                throw new Exception(GetResValue("Txt_SessionExpired"));

            string proc = (SubmitType == "add" ? "BasicData_EWI_Add" : "BasicData_EWI_Edit");
            string TestXML = string.Format("<Main><Detail TestInstrument=\"{0}\" TestInstrument_EN=\"{1}\" Category=\"{2}\" /></Main>", 
                TestInstrument, TestInstrument_EN, Category);
            string strSql = string.Format("{0} N'{1}','{2}',{3}",
                    proc, TestXML, user.UserName, 0);

            returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);

            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
