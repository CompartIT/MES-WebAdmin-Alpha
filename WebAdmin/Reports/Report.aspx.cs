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

namespace WebAdmin.Reports
{
    public partial class Report : System.Web.UI.Page
    {
        public DataTable dtCriteria, dtImages, dtOPList;
        private const string _adminSessionKey = "Admin";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string PartNum = "", RevID = Request["RevID"].ToString();
                string strSql = "", RdlcName = "";
                RdlcName = @"EWI.rdlc";
                SysAdmin user = Session[_adminSessionKey] as SysAdmin;

                //生成文件
                LocalReport report = ReportViewer1.LocalReport;
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
                report.Refresh();
            }
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
    }
}