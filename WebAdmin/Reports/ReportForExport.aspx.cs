using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Reflection;
using System.IO;
using System.Data.SqlClient;
using Microsoft.Reporting.WebForms;
using WebAdmin.Models;
using System.Threading;
using System.Text;
using NPOI;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace WebAdmin.Reports
{
    public partial class ReportForExport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var ReportFormat = "";

            if (!Page.IsPostBack)
            {
                if (Request.QueryString["ReportFormat"] != null)
                {
                    ReportFormat = Request.QueryString["ReportFormat"].ToString();

                    //Base on the report name 
                    if (Request.QueryString["ReportName"] != null)
                    {
                        var strReportName = Request.QueryString["ReportName"].ToString();
                        var ReportCriteria = Request.QueryString["ReportCriteria"].ToString();
                        var OrderBy = Request.QueryString["orderBy"].ToString();
                        var OrderDir = Request.QueryString["dir"].ToString();

                        MethodInfo method = this.GetType().GetMethod(strReportName);

                        if (method != null)
                        {
                            method.Invoke(this, new object[] { ReportFormat, ReportCriteria, OrderBy, OrderDir });
                        }
                    }
                }
                else if (Request.Form["ReportName"] != null)
                {
                    ReportFormat = Request.Form["ReportFormat"].ToString();

                    //Base on the report name 
                    if (Request.Form["ReportName"] != null)
                    {
                        var strReportName = Request.Form["ReportName"].ToString();
                        var ReportCriteria = Request.Form["ReportCriteria"].ToString();
                        var OrderBy = Request.Form["orderBy"].ToString();
                        var OrderDir = Request.Form["dir"].ToString();

                        MethodInfo method = this.GetType().GetMethod(strReportName);

                        if (method != null)
                        {
                            method.Invoke(this, new object[] { ReportFormat, ReportCriteria, OrderBy, OrderDir });
                        }
                    }
                }
                else
                {
                    Response.Write("invalid Request");
                }
            }
        }

        /// <summary>
        /// Process Time Comparison report
        /// </summary>
        /// <param name="ReportFormat"></param>
        /// <param name="ReportCriteria"></param>
        /// <param name="OrderBy"></param>
        /// <param name="OrderDir"></param>
        public void ProcessTimeComparison(string ReportFormat, string ReportCriteria, string OrderBy, string OrderDir)
        {
            var strSQL = "select * from V_test where 1=1 " + ReportCriteria + " order by  " + OrderBy + " " + OrderDir + "";
            DataTable dt = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            List<MESReportDataSources> dataSourceCollection = new List<MESReportDataSources>();
            dataSourceCollection.Add(new MESReportDataSources { DataSetName = "DataSet1", DataTable = dt });

            var reportPath = "ProcessTimeComparisonReport.rdlc";

            //Load report
            LoadReport(reportPath, ReportFormat, dataSourceCollection);

        }

        /// <summary>
        /// WIP
        /// </summary>
        /// <param name="ReportFormat"></param>
        /// <param name="ReportCriteria"></param>
        /// <param name="OrderBy"></param>
        /// <param name="OrderDir"></param>
        public void WIPReport(string ReportFormat, string ReportCriteria, string OrderBy, string OrderDir)
        {
            var strSQL = "select * from V_WIP where 1=1 " + ReportCriteria + " order by  " + OrderBy + " " + OrderDir + "";
            DataTable dt = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            List<MESReportDataSources> dataSourceCollection = new List<MESReportDataSources>();
            dataSourceCollection.Add(new MESReportDataSources { DataSetName = "DataSet1", DataTable = dt });

            var reportPath = "WebWipReport.rdlc";

            //Load report
            LoadReport(reportPath, ReportFormat, dataSourceCollection);

        }

        /// <summary>
        /// Input TO BE Report
        /// </summary>
        /// <param name="ReportFormat"></param>
        /// <param name="ReportCriteria"></param>
        /// <param name="OrderBy"></param>
        /// <param name="OrderDir"></param>
        public void InputToBEReport(string ReportFormat, string ReportCriteria, string OrderBy, string OrderDir)
        {
            var strSQL = "select * from V_InputToBE where 1=1 " + ReportCriteria + " order by  " + OrderBy + " " + OrderDir + "";
            DataTable dt = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            List<MESReportDataSources> dataSourceCollection = new List<MESReportDataSources>();
            dataSourceCollection.Add(new MESReportDataSources { DataSetName = "DataSet1", DataTable = dt });

            var reportPath = "InputToBEReport.rdlc";

            //Load report
            LoadReport(reportPath, ReportFormat, dataSourceCollection);

        }

        /// <summary>
        /// MRB Rework Report
        /// </summary>
        /// <param name="ReportFormat"></param>
        /// <param name="ReportCriteria"></param>
        /// <param name="OrderBy"></param>
        /// <param name="OrderDir"></param>
        public void MRBReworkReport(string ReportFormat, string ReportCriteria, string OrderBy, string OrderDir)
        {
            var strSQL = "select * from V_MRBRework where 1=1 " + ReportCriteria + " order by  " + OrderBy + " " + OrderDir + "";
            DataTable dt = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            List<MESReportDataSources> dataSourceCollection = new List<MESReportDataSources>();
            dataSourceCollection.Add(new MESReportDataSources { DataSetName = "DataSet1", DataTable = dt });

            var reportPath = "MRBReworkInfoReport.rdlc";

            //Load report
            LoadReport(reportPath, ReportFormat, dataSourceCollection);

        }

        /// <summary>
        /// MRB Rework Report
        /// </summary>
        /// <param name="ReportFormat"></param>
        /// <param name="ReportCriteria"></param>
        /// <param name="OrderBy"></param>
        /// <param name="OrderDir"></param>
        public void MRBDetailsReport(string ReportFormat, string ReportCriteria, string OrderBy, string OrderDir)
        {
            var strSQL = "select * from V_MRBDetails where 1=1 " + ReportCriteria + " order by  " + OrderBy + " " + OrderDir + "";
            DataTable dt = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            List<MESReportDataSources> dataSourceCollection = new List<MESReportDataSources>();
            dataSourceCollection.Add(new MESReportDataSources { DataSetName = "DataSet1", DataTable = dt });

            var reportPath = "MRBDetailsReport.rdlc";

            //Load report
            LoadReport(reportPath, ReportFormat, dataSourceCollection);

        }

        /// <summary>
        /// MRB Rework Report
        /// </summary>
        /// <param name="ReportFormat"></param>
        /// <param name="ReportCriteria"></param>
        /// <param name="OrderBy"></param>
        /// <param name="OrderDir"></param>
        public void BOMCostAnalysisReport(string ReportFormat, string ReportCriteria, string OrderBy, string OrderDir)
        {
            var strSQL = "select * from V_BOMAnalysis ";
            DataTable dt = SqlHelper.ExecuteDataTable(SQLConnectionSelection.Epicor, CommandType.Text, strSQL);

            List<MESReportDataSources> dataSourceCollection = new List<MESReportDataSources>();
            dataSourceCollection.Add(new MESReportDataSources { DataSetName = "DataSet1", DataTable = dt });

            var reportPath = "BOMCostAnalysisReport.rdlc";

            //Load report
            LoadReport(reportPath, ReportFormat, dataSourceCollection);

        }

        public void BOMCostAnalysisFullDownload(string ReportFormat, string ReportCriteria, string OrderBy, string OrderDir)
        {
            var strSQL = "select  * from V_BOMAnalysis ";
            DataTable dt = SqlHelper.ExecuteDataTable(SQLConnectionSelection.Epicor, CommandType.Text, strSQL);

            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.Cache.SetCacheability(System.Web.HttpCacheability.Public);

            Response.ContentType = "application/octet-stream";
            string fullName = HttpUtility.UrlEncode("file.xlsx");
            Response.AddHeader("Content-Disposition", "inline; filename=file.xlsx");

            long fileSize;
            MemoryStream ms = TableToExcel(dt, fullName, out fileSize);
            byte[] data = ms.GetBuffer();

            Response.AddHeader("Content-Length", fileSize.ToString());
            Response.BinaryWrite(data);
            Response.Flush();
            Response.End();
        }

        public MemoryStream TableToExcel(DataTable dt, string file, out long fileSize)
        {
            //create workbook
            IWorkbook workbook;
            string fileExt = Path.GetExtension(file).ToLower();
            if (fileExt == ".xlsx")
                workbook = new XSSFWorkbook();
            else if (fileExt == ".xls")
                workbook = new HSSFWorkbook();
            else
                workbook = null;
            //worksheet
            ISheet sheet = workbook.CreateSheet("Sheet1");

            //head
            IRow headrow = sheet.CreateRow(0);
            for (int i = 0; i < 0; i++)
            {
                ICell headcell = headrow.CreateCell(i);
                headcell.SetCellValue(dt.Columns[i].ColumnName);
            }
            //Rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    ICell cell = row.CreateCell(j);
                    cell.SetCellValue(dt.Rows[i][j].ToString());
                }
            }

            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);

                fileSize = ms.ToArray().Length;
                return ms;
            }
        }

        protected void LoadReport(string ReportPath, string ReportFormat, List<MESReportDataSources> dataSourceCollection)
        {
            ReportViewer1.LocalReport.ReportPath = Server.MapPath(ReportPath);

            foreach (MESReportDataSources dataSource in dataSourceCollection)
            {
                ReportDataSource reportDataSource = new ReportDataSource(dataSource.DataSetName, dataSource.DataTable);

                ReportViewer1.LocalReport.DataSources.Add(reportDataSource);
            }
            ReportViewer1.LocalReport.Refresh();

            Microsoft.Reporting.WebForms.Warning[] warnings;
            string[] strStreamIds;
            string strMimeType;
            string strEncoding;
            string strFileNameExtension;

            byte[] bytes = this.ReportViewer1.LocalReport.Render(ReportFormat, null, out strMimeType, out strEncoding, out strFileNameExtension, out strStreamIds, out warnings);

            //var strFileName = "MESReport." + strFileNameExtension;
            //var strFilePath = Server.MapPath("/Reports/" + strFileName);

            //using (System.IO.FileStream fs = new FileStream(strFilePath, FileMode.Create))
            //{
            //    fs.Write(bytes, 0, bytes.Length);
            //}

            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = strMimeType;
            Response.Cache.SetCacheability(System.Web.HttpCacheability.Public);
            Response.AddHeader("Content-Disposition", "inline; filename=MESReport." + strFileNameExtension);
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }
    }
}