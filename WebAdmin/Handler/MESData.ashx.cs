using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAdmin.Models;
using System.Data.SqlClient;
using System.Web.Script.Serialization;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using NPOI;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace WebAdmin.Handler
{
    /// <summary>
    /// MESData 的摘要说明
    /// </summary>
    public class MESData : IHttpHandler
    {

        /*Kanban*/
        public void ProcessRequest(HttpContext context)
        {
            //Base on the commandType
            if (context.Request["CommandType"] != null)
            {
                var strCommandType = context.Request["CommandType"].ToString();

                System.Reflection.MethodInfo method = this.GetType().GetMethod(strCommandType);

                if (method != null)
                {
                    method.Invoke(this, new object[] { context });
                }
            }
        }

        public void GetProcessTimeComparision(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            int start = context.Request["start"] != null ? int.Parse(context.Request["start"]) : 0;
            int length = context.Request["length"] != null ? int.Parse(context.Request["length"]) : 10;

            int orderColunmIndex = context.Request["order[0][column]"] != null ? int.Parse(context.Request["order[0][column]"]) : 0;
            var orderColumnName = context.Request["columns[" + orderColunmIndex + "][data]"];
            var orderDirection = context.Request["order[0][dir]"] != null ? context.Request["order[0][dir]"] : "desc";


            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";

            int recordsTotal;

            var getResult = DAL_MES.GetProcessTimeComparision(out recordsTotal, start, length, SqlQuery, orderColumnName, orderDirection);

            var returnReulst = new
            {
                orderColumnName = orderColumnName,
                orderDirection = orderDirection,
                orderColumn = orderColunmIndex,
                startaa = start,
                length = length,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = getResult.Select(p => new ProcessTimeComparisionEntity { JobNum = p.JobNum, PartNum = p.PartNum, UserID = p.UserID, MachineID = p.MachineID, PDAID = p.PDAID, OpCode = p.OpCode, StartTime = p.StartTime, EndTime = p.EndTime, StandardOprTime = p.StandardOprTime, ActualOprTime = p.ActualOprTime, Percentage = p.Percentage, ABSPercentageVariance = p.ABSPercentageVariance })
            };
            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Get FE's Kanban list list
        /// </summary>

        public void GetShopFloorKanbans(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            IEnumerable<ShopFloorLocationEntity> LocationGroup;

            var getResult = DAL_MES.GetShopFloorKanbans(out LocationGroup);

            var returnReulst = new
            {
                data = getResult.Select(p => new ShopFloorKanbanEntity { JobNum = p.JobNum, UserID = p.UserID, MachineID = p.MachineID, Location = p.Location, PDAID = p.Location, OpCode = p.OpCode, OpDesc = p.OpDesc, OpGroup = p.OpGroup, ReportingQTY = p.ReportingQTY, LaborQTY = p.LaborQTY, ScrapQTY = p.ScrapQTY, DiscrepQTY = p.DiscrepQTY, TransTime = p.TransTime }),
                locationGroup = LocationGroup.Select(C => new ShopFloorLocationEntity { Location = C.Location })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Get FE's Kanban list group by Location
        /// </summary>

        public void GetShopFloorKanbansByLocation(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";


            var getResult = DAL_MES.GetShopFloorKanbansByLocation(SqlQuery);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ShopFloorGroupedKanbanEntity { LocationGroup = p.LocationGroup, LocationOrder = p.LocationOrder, ShopFloorKanbanList = p.ShopFloorKanbanList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }



        /// <summary>
        /// Get FE's Kanban list group by Location
        /// </summary>

        public void GetShopFloorKanbansByLocation_FA(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " and " + sqlCriteria + "";


            var getResult = DAL_MES.GetShopFloorKanbansByLocation_FA(SqlQuery);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ShopFloorGroupedKanbanEntity { LocationGroup = p.LocationGroup, ShopFloorKanbanList = p.ShopFloorKanbanList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Showing Welding
        /// </summary>
        /// <param name="context"></param>
        public void GetShopFloorKanbansByLocation_Welding(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = " 1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";


            var getResult = DAL_MES.GetShopFloorKanbansByLocation_Welding(SqlQuery);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ShopFloorGroupedKanbanEntity { LocationGroup = p.LocationGroup, LocationOrder = p.LocationOrder, ShopFloorKanbanList = p.ShopFloorKanbanList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Showing EBW
        /// </summary>
        /// <param name="context"></param>
        public void GetShopFloorKanbansByLocation_EBW(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = " 1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";


            var getResult = DAL_MES.GetShopFloorKanbansByLocation_EBW(SqlQuery);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ShopFloorGroupedKanbanEntity { LocationGroup = p.LocationGroup, LocationOrder = p.LocationOrder, ShopFloorKanbanList = p.ShopFloorKanbanList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Showing Burnishing
        /// </summary>
        /// <param name="context"></param>
        public void GetShopFloorKanbansByLocation_Burnishing(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = " 1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";


            var getResult = DAL_MES.GetShopFloorKanbansByLocation_Burnishing(SqlQuery);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ShopFloorGroupedKanbanEntity { LocationGroup = p.LocationGroup, LocationOrder = p.LocationOrder, ShopFloorKanbanList = p.ShopFloorKanbanList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Showing Lapping
        /// </summary>
        /// <param name="context"></param>
        public void GetShopFloorKanbansByLocation_Lapping(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = " 1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";


            var getResult = DAL_MES.GetShopFloorKanbansByLocation_Lapping(SqlQuery);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ShopFloorGroupedKanbanEntity { LocationGroup = p.LocationGroup, LocationOrder = p.LocationOrder, ShopFloorKanbanList = p.ShopFloorKanbanList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Get all Machine List
        /// </summary>

        public void GetMachineList(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = " ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += "  " + sqlCriteria + "";


            var getResult = DAL_MES.GetMachineList(SqlQuery);
            var getFA = DAL_MES.GetFAMachineList();

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                FAMachines = getFA.Select(s => new ShopFloorFAMachineList { MachineID = s.MachineID }),
                data = getResult.Select(p => new ShopFloorMachineList { LocationGroup = p.LocationGroup, LocationGroupCount = p.LocationGroupCount, MachineType = p.MachineType, MachineList = p.MachineList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Get all FA Machine List
        /// </summary>

        public void GetMachineList_FA(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " and " + sqlCriteria + "";


            var getResult = DAL_MES.GetMachineList_FA(SqlQuery);
            var getFA = DAL_MES.GetFAMachineList();

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                FAMachines = getFA.Select(s => new ShopFloorFAMachineList { MachineID = s.MachineID }),
                data = getResult.Select(p => new ShopFloorMachineList { LocationGroup = p.LocationGroup, LocationGroupCount = p.LocationGroupCount, MachineType = p.MachineType, MachineList = p.MachineList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Get all Welding machines 
        /// </summary>
        /// <param name="context"></param>
        public void GetWeldingMachine(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " and " + sqlCriteria + "";


            var getResult = DAL_MES.GetWeldingList(SqlQuery);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ShopFloorMachineList { LocationGroup = p.LocationGroup, LocationGroupCount = p.LocationGroupCount, MachineType = p.MachineType, MachineList = p.MachineList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Get all Welding machines 
        /// </summary>
        /// <param name="context"></param>
        public void GetEBWMachine(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " and " + sqlCriteria + "";


            var getResult = DAL_MES.GetEBWList(SqlQuery);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ShopFloorMachineList { LocationGroup = p.LocationGroup, LocationGroupCount = p.LocationGroupCount, MachineType = p.MachineType, MachineList = p.MachineList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Get all Welding machines 
        /// </summary>
        /// <param name="context"></param>
        public void GetBurnishingMachine(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " and " + sqlCriteria + "";


            var getResult = DAL_MES.GetBurnishingList(SqlQuery);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ShopFloorMachineList { LocationGroup = p.LocationGroup, LocationGroupCount = p.LocationGroupCount, MachineType = p.MachineType, MachineList = p.MachineList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Get all Lapping machines 
        /// </summary>
        /// <param name="context"></param>
        public void GetLappingMachine(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " and " + sqlCriteria + "";


            var getResult = DAL_MES.GetLappingList(SqlQuery);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ShopFloorMachineList { LocationGroup = p.LocationGroup, LocationGroupCount = p.LocationGroupCount, MachineType = p.MachineType, MachineList = p.MachineList })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        public void GetWIP(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            int start = context.Request["start"] != null ? int.Parse(context.Request["start"]) : 0;
            int length = context.Request["length"] != null ? int.Parse(context.Request["length"]) : 10;

            int orderColunmIndex = context.Request["order[0][column]"] != null ? int.Parse(context.Request["order[0][column]"]) : 0;
            var orderColumnName = context.Request["columns[" + orderColunmIndex + "][data]"];
            var orderDirection = context.Request["order[0][dir]"] != null ? context.Request["order[0][dir]"] : "desc";


            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";

            int recordsTotal;

            var getResult = DAL_MES.GetWIP(out recordsTotal, start, length, SqlQuery, orderColumnName, orderDirection);

            var returnReulst = new
            {
                orderColumnName = orderColumnName,
                orderDirection = orderDirection,
                orderColumn = orderColunmIndex,
                startaa = start,
                length = length,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = getResult.Select(p => new WIPEntity { Jtype = p.Jtype, TransType = p.TransType, MONo = p.MONo, ProductFamily = p.ProductFamily, ItemCode = p.ItemCode, HeatCode = p.HeatCode, LotNo = p.LotNo, OPCode = p.OPCode, OPCodeNext = p.OPCodeNext, OPDesc = p.OPDesc, OPDescNext = p.OPDescNext, WDate = p.WDate, WIPQTY = p.WIPQTY, UserId = p.UserId })
            };
            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        public void GetWIP2(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();


            int orderColunmIndex = context.Request["order[0][column]"] != null ? int.Parse(context.Request["order[0][column]"]) : 0;
            var orderColumnName = context.Request["columns[" + orderColunmIndex + "][data]"];
            var orderDirection = context.Request["order[0][dir]"] != null ? context.Request["order[0][dir]"] : "desc";


            var SqlQuery = "1=1 ";

            int recordsTotal;

            var getResult = DAL_MES.GetWIP2(out recordsTotal, SqlQuery, orderColumnName, orderDirection);

            var returnReulst = new
            {
                orderColumnName = orderColumnName,
                orderDirection = orderDirection,
                orderColumn = orderColunmIndex,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = getResult.Select(p => new WIPEntity { Jtype = p.Jtype, TransType = p.TransType, MONo = p.MONo, ProductFamily = p.ProductFamily, ItemCode = p.ItemCode, HeatCode = p.HeatCode, LotNo = p.LotNo, OPCode = p.OPCode, OPCodeNext = p.OPCodeNext, OPDesc = p.OPDesc, OPDescNext = p.OPDescNext, WDate = p.WDate, WIPQTY = p.WIPQTY, UserId = p.UserId })
            };
            context.Response.Write(_serializer.Serialize(returnReulst));
        }


        public void GetInputToBE(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();
            _serializer.MaxJsonLength = int.MaxValue;

            int start = context.Request["start"] != null ? int.Parse(context.Request["start"]) : 0;
            int length = context.Request["length"] != null ? int.Parse(context.Request["length"]) : 10;

            int orderColunmIndex = context.Request["order[0][column]"] != null ? int.Parse(context.Request["order[0][column]"]) : 0;
            var orderColumnName = context.Request["columns[" + orderColunmIndex + "][data]"];
            var orderDirection = context.Request["order[0][dir]"] != null ? context.Request["order[0][dir]"] : "desc";


            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";

            int recordsTotal;

            var getResult = DAL_MES.GetInputToBE(out recordsTotal, start, length, SqlQuery, orderColumnName, orderDirection);

            var returnReulst = new
            {
                orderColumnName = orderColumnName,
                orderDirection = orderDirection,
                orderColumn = orderColunmIndex,
                startaa = start,
                length = length,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = getResult.Select(p => new InputToBEEntity { Site = p.Site, JobNum = p.Job, Job = p.Job, Product = p.Product, FromOPCode = p.FromOPCode, Quantity = p.Quantity, StartDatetime = p.StartDatetime, ToOPCode = p.ToOPCode, TotalQty = p.TotalQty, TransType = p.TransType, UserID = p.UserID })
            };
            context.Response.Write(_serializer.Serialize(returnReulst));
        }


        public void GetMRBRework(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            int start = context.Request["start"] != null ? int.Parse(context.Request["start"]) : 0;
            int length = context.Request["length"] != null ? int.Parse(context.Request["length"]) : 10;

            int orderColunmIndex = context.Request["order[0][column]"] != null ? int.Parse(context.Request["order[0][column]"]) : 0;
            var orderColumnName = context.Request["columns[" + orderColunmIndex + "][data]"];
            var orderDirection = context.Request["order[0][dir]"] != null ? context.Request["order[0][dir]"] : "desc";


            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";

            int recordsTotal;

            var getResult = DAL_MES.GetMRBRework(out recordsTotal, start, length, SqlQuery, orderColumnName, orderDirection);

            var returnReulst = new
            {
                orderColumnName = orderColumnName,
                orderDirection = orderDirection,
                orderColumn = orderColunmIndex,
                startaa = start,
                length = length,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = getResult.Select(p => new MRBReworkEEntity { JType = p.JType, LotNo = p.LotNo, OpCode = p.OpCode, ItemCode = p.ItemCode, EpicorJobNum = p.EpicorJobNum, PrintDateTime = p.PrintDateTime, PrintUserId = p.PrintUserId, ProdQty = p.ProdQty, ProductFamily = p.ProductFamily, Site = p.Site, TransTime = p.TransTime, Transtype = p.Transtype, WDate = p.WDate })
            };
            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Get the Monthly's output report
        /// </summary>
        /// <param name="context"></param>
        public void GetDialyOutput(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            string strSQL = "SELECT TranDate, sum(InTranQty) as StoreInQty,-1*SUM(OutTranQty) as StoreOutQty,cast(SUM(InExtCost)/6.8 as decimal(12,0)) as StoreInUSD,cast(-1*SUM(OutExtCost)/6.8 as decimal(12,0)) as StoreOutUSD FROM Epicor_OutputData where TranDate<GETDATE() and TranDate>'2019-01-15' Group by TranDate order by TranDate";
            DataTable dtGetData = SqlHelper.ExecuteDataTable(SQLConnectionSelection.EpicorReportingData, CommandType.Text, strSQL);

            List<object> list = new List<object>();
            for (int i = 0; i < dtGetData.Rows.Count; i++)
            {
                var obj = new { TranDate = Convert.ToDateTime(dtGetData.Rows[i]["TranDate"]).ToString("MM/dd"), StoreInQty = dtGetData.Rows[i]["StoreInQty"], StoreOutQty = dtGetData.Rows[i]["StoreOutQty"], StoreInUSD = dtGetData.Rows[i]["StoreInUSD"], StoreOutUSD = dtGetData.Rows[i]["StoreOutUSD"] };
                list.Add(obj);
            }


            JavaScriptSerializer _serializer = new JavaScriptSerializer();
            context.Response.Write(_serializer.Serialize(list));

        }

        /// <summary>
        /// Download all the BOM cost Analysis into Excel
        /// </summary>
        /// <param name="context"></param>
        public void DownloadBOMCost(HttpContext context)
        {
            var strSQL = "select [Bom Type], [Parent Part#], [Revi], [Bom Level], [Bom Path], [Part Num], [UOM Code], [Required QTY], [Material Rate], [Material Cost], [Labor Cost], [Burden Cost], [OS Cost], [Total Cost] from V_BOMAnalysis order by RN ";
            DataTable dt = SqlHelper.ExecuteDataTable(SQLConnectionSelection.Epicor, CommandType.Text, strSQL);

            context.Response.Clear();
            context.Response.ClearContent();
            context.Response.ClearHeaders();
            context.Response.Cache.SetCacheability(System.Web.HttpCacheability.Public);

            context.Response.ContentType = "application/octet-stream";
            string fullName = HttpUtility.UrlEncode("file.xlsx");
            context.Response.AddHeader("Content-Disposition", "inline; filename=file.xlsx");

            long fileSize;
            MemoryStream ms = TableToExcel(dt, fullName, out fileSize);
            byte[] data = ms.GetBuffer();

            context.Response.AddHeader("Content-Length", fileSize.ToString());
            context.Response.BinaryWrite(data);
            context.Response.Flush();
            context.Response.End();
        }


        /// <summary>
        /// Process Info Kanban information
        /// </summary>
        /// <param name="context"></param>
        public void GetProcessKanban(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            var ProcessType = context.Request["ProcessType"].ToString();

            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";


            var getResult = DAL_MES.GetProcessKanbanInfo(SqlQuery, ProcessType);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = getResult.Select(p => new ProcessKanbanEntity { ProductFamily = p.ProductFamily, SumQTY = p.SumQTY, JobCount = p.JobCount, SumJobCost = p.SumJobCost, ProcessKanbanDetailList = p.ProcessKanbanDetailList.Select(s => new ProcessKanbanDetailEntity { ProductFamily = s.ProductFamily, PartNum = s.PartNum, TransType = s.TransType, JobNum = s.JobNum, OpCode = s.OpCode, OpCodeNext = s.OpCodeNext, OprQty = s.OprQty, ReportingQty = s.ReportingQty, LaborQty = s.LaborQty, TransTime = s.TransTime, JobCost = s.JobCost, StdCostUS = s.StdCostUS, Aging = s.Aging }) })
            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }


        /// <summary>
        /// Process Info Kanban information -- For Awaiting to show the grouping
        /// </summary>
        /// <param name="context"></param>
        public void GetProcessKanbanAwaiting(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            JavaScriptSerializer _serializer = new JavaScriptSerializer();

            var SqlQuery = "1=1 ";

            var sqlCriteria = context.Request["SqlCriteria"].ToString();
            var ProcessType = context.Request["ProcessType"].ToString();

            if (sqlCriteria != "")
                SqlQuery += " " + sqlCriteria + "";


            var getResult = DAL_MES.GetProcessKanbanInfo_Awaiting(SqlQuery, ProcessType);

            var returnReulst = new
            {
                serverTimeStamp = DateTime.Now.ToLongTimeString(),
                data = new ProcessKanbanAwaitingEntity
                {
                    ProcessKanbanGroupA = getResult.ProcessKanbanGroupA.Select(p => new ProcessKanbanEntity { ProductFamily = p.ProductFamily, SumQTY = p.SumQTY, JobCount = p.JobCount, SumJobCost = p.SumJobCost, ProcessKanbanDetailList = p.ProcessKanbanDetailList.Select(s => new ProcessKanbanDetailEntity { ProductFamily = s.ProductFamily, PartNum = s.PartNum, TransType = s.TransType, JobNum = s.JobNum, OpCode = s.OpCode, OpCodeNext = s.OpCodeNext, OprQty = s.OprQty, ReportingQty = s.ReportingQty, LaborQty = s.LaborQty, TransTime = s.TransTime, JobCost = s.JobCost, StdCostUS = s.StdCostUS, Aging = s.Aging }) }),
                    ProcessKanbanGroupB = getResult.ProcessKanbanGroupB.Select(p => new ProcessKanbanEntity { ProductFamily = p.ProductFamily, SumQTY = p.SumQTY, JobCount = p.JobCount, SumJobCost = p.SumJobCost, ProcessKanbanDetailList = p.ProcessKanbanDetailList.Select(s => new ProcessKanbanDetailEntity { ProductFamily = s.ProductFamily, PartNum = s.PartNum, TransType = s.TransType, JobNum = s.JobNum, OpCode = s.OpCode, OpCodeNext = s.OpCodeNext, OprQty = s.OprQty, ReportingQty = s.ReportingQty, LaborQty = s.LaborQty, TransTime = s.TransTime, JobCost = s.JobCost, StdCostUS = s.StdCostUS, Aging = s.Aging }) }),
                    ProcessKanbanGroupC = getResult.ProcessKanbanGroupC.Select(p => new ProcessKanbanEntity { ProductFamily = p.ProductFamily, SumQTY = p.SumQTY, JobCount = p.JobCount, SumJobCost = p.SumJobCost, ProcessKanbanDetailList = p.ProcessKanbanDetailList.Select(s => new ProcessKanbanDetailEntity { ProductFamily = s.ProductFamily, PartNum = s.PartNum, TransType = s.TransType, JobNum = s.JobNum, OpCode = s.OpCode, OpCodeNext = s.OpCodeNext, OprQty = s.OprQty, ReportingQty = s.ReportingQty, LaborQty = s.LaborQty, TransTime = s.TransTime, JobCost = s.JobCost, StdCostUS = s.StdCostUS, Aging = s.Aging }) })
                }

            };

            context.Response.Write(_serializer.Serialize(returnReulst));
        }

        /// <summary>
        /// Process Info Kanban information -- Get Process's output
        /// </summary>
        /// <param name="context"></param>
        public void GetProcessOutput(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            var SqlQuery = "1=1 ";

            //var sqlCriteria = context.Request["SqlCriteria"].ToString();
            //var ProcessType = context.Request["ProcessType"].ToString();

            //if (sqlCriteria != "")
            //    SqlQuery += " " + sqlCriteria + "";


            DataTable dtGetData = SqlHelper.ExecuteDataTable(SQLConnectionSelection.MES, CommandType.StoredProcedure, "SP_Kanban_ProcessOutput");

            List<object> list = new List<object>();
            for (int i = 0; i < dtGetData.Rows.Count; i++)
            {
                var obj = new { MonthDay = dtGetData.Rows[i]["MonthDay"], JobCost = dtGetData.Rows[i]["JobCost"], LotCount = dtGetData.Rows[i]["LotCount"], JobQTY = dtGetData.Rows[i]["JobQTY"] };
                list.Add(obj);
            }


            JavaScriptSerializer _serializer = new JavaScriptSerializer();
            context.Response.Write(_serializer.Serialize(list));

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

            //Report title head
            IRow reportTitleHeadrow = sheet.CreateRow(0);
            ICellStyle reportTitleStyle = workbook.CreateCellStyle();
            IFont titleFont = workbook.CreateFont();
            titleFont.FontHeightInPoints = 16;
            titleFont.Color = NPOI.HSSF.Util.HSSFColor.Grey80Percent.Index;
            reportTitleStyle.SetFont(titleFont);
            reportTitleHeadrow.Height = 40 * 20;
            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, dt.Columns.Count - 1));
            reportTitleStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Left;
            reportTitleStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            reportTitleStyle.Indention = 2;
            ICell titleCell = reportTitleHeadrow.CreateCell(0);
            titleCell.SetCellValue("Compart BOM Cost Analysis Report");
            titleCell.CellStyle = reportTitleStyle;

            //head
            IRow headrow = sheet.CreateRow(1);
            ICellStyle headStyle = workbook.CreateCellStyle();
            headStyle.FillPattern = FillPattern.SolidForeground;
            headStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.DarkBlue.Index;
            IFont font = workbook.CreateFont();
            font.FontHeightInPoints = 11;
            font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
            headStyle.SetFont(font);
            headStyle.WrapText = true;
            headrow.Height = 40 * 20;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                ICell headcell = headrow.CreateCell(i);
                if (i == 4)
                {
                    sheet.SetColumnWidth(i, 50 * 256);
                }
                else
                {
                    sheet.SetColumnWidth(i, 10 * 256);
                }
                headcell.SetCellValue(dt.Columns[i].ColumnName);
                headcell.CellStyle = headStyle;
            }

            //===Rows
            //If BOM's level is 0 then show different color
            ICellStyle firstBOMStyle = workbook.CreateCellStyle();
            firstBOMStyle.FillPattern = FillPattern.SolidForeground;
            firstBOMStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightBlue.Index;

            ICellStyle numericStyle = workbook.CreateCellStyle();
            IDataFormat dataformat = workbook.CreateDataFormat();
            numericStyle.DataFormat = dataformat.GetFormat("0.0000");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet.CreateRow(i + 2);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    ICell cell = row.CreateCell(j, CellType.Numeric);

                    //BOM's Level
                    if (dt.Rows[i][3].ToString() == "0")
                    {
                        cell.CellStyle = firstBOMStyle;

                    }
                    // cell.SetCellValue( dt.Rows[i][j].ToString().Trim());

                    var drValue = dt.Rows[i][j].ToString().Trim();
                    switch (dt.Columns[j].DataType.ToString())
                    {
                        case "System.String":// string   
                            cell.SetCellValue(drValue);
                            break;
                        case "System.DateTime"://Date  
                            DateTime dateV;
                            DateTime.TryParse(drValue, out dateV);
                            cell.SetCellValue(dateV);

                            //cell.CellStyle = dateStyle;//格式化显示   
                            break;
                        case "System.Boolean"://boolean  
                            bool boolV = false;
                            bool.TryParse(drValue, out boolV);
                            cell.SetCellValue(boolV);
                            break;
                        case "System.Int16"://Int   
                        case "System.Int32":
                        case "System.Int64":
                        case "System.Byte":
                            int intV = 0;
                            int.TryParse(drValue, out intV);
                            cell.SetCellValue(intV);
                            break;
                        case "System.Decimal"://float  
                        case "System.Double":
                            double doubV = 0;
                            double.TryParse(drValue, out doubV);
                            cell.CellStyle = numericStyle;
                            cell.SetCellValue(doubV);
                            break;
                        case "System.DBNull"://Null & empty   
                            cell.SetCellValue("");
                            break;
                        default:
                            cell.SetCellValue("");
                            break;
                    }

                }

            }

            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);

                fileSize = ms.ToArray().Length;
                return ms;
            }
        }

        public void GetExcel(HttpContext context)
        {
            string exportTemplatePath = "~/Document/TempletExcel/SIM档案管理.xls";
            DataTable dt = new DataTable(); // DataTable 数据源
            string download = GetPathByDataTableToExcel(dt, exportTemplatePath);
            context.Response.Write(download);
        }

        /// <summary>
        /// DataTable填充Excel
        /// 存储excel
        /// 返回excel下载路径
        /// </summary>
        /// <param name="sourceTable">数据源</param>
        /// <param name="exportTemplatePath">模板路径</param>
        /// <returns>下载路径</returns>
        public string GetPathByDataTableToExcel(DataTable sourceTable, string exportTemplatePath)
        {
            /// ********************************需要引入NPOI组件*********************************************

            HSSFWorkbook workbook = null;
            MemoryStream ms = null;
            ISheet sheet = null;
            string templetFileName = HttpContext.Current.Server.MapPath(exportTemplatePath);
            FileStream file = new FileStream(templetFileName, FileMode.Open, FileAccess.Read);
            workbook = new HSSFWorkbook(file);
            string httpurl = "";
            try
            {

                ms = new MemoryStream();
                sheet = workbook.GetSheetAt(0);  //第一个Sheet页面
                int rowIndex = 1;  //行索引，  0为第一行，1为第二行

                //遍历DataTable 填充所有数据
                foreach (DataRow row in sourceTable.Rows)
                {
                    HSSFRow dataRow = (HSSFRow)sheet.CreateRow(rowIndex);
                    foreach (DataColumn column in sourceTable.Columns)
                        dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                    ++rowIndex;
                }

                workbook.Write(ms);
                ms.Flush();
            }
            catch (Exception)
            {

            }
            finally
            {
                ms.Close();
                sheet = null;
                workbook = null;

                //~/Document/TemporaryDocuments/  是项目下相对路径的文件存放地址，也可进行修改

                string tempExcelName = Path.GetFileNameWithoutExtension(templetFileName) + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(templetFileName);
                string tempExcel = "~/Document/TemporaryDocuments/" + tempExcelName;

                //文件另存
                System.IO.File.WriteAllBytes(HttpContext.Current.Server.MapPath(tempExcel), ms.GetBuffer());

                //获取项目绝对路径地址
                string url = HttpContext.Current.Request.Url.AbsoluteUri.ToString().Split('/')[0] + "//" + HttpContext.Current.Request.Url.Authority.ToString();


                var virtualPath = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
                string fileName = "";
                if (virtualPath != "/")
                {
                    //有子应用程序
                    fileName = virtualPath + "/Document/TemporaryDocuments/" + tempExcelName;
                }
                else
                {
                    fileName = "/Document/TemporaryDocuments/" + tempExcelName;
                }

                //拼接文件相对地址
                //string fileName = "/Document/TemporaryDocuments/" + tempExcelName;

                //返回文件url地址
                httpurl = url + fileName;

                //清除历史文件，避免历史文件越来越多，可进行删除
                DirectoryInfo dyInfo = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/Document/TemporaryDocuments/"));
                //获取文件夹下所有的文件
                foreach (FileInfo feInfo in dyInfo.GetFiles())
                {
                    //判断文件日期是否小于两天前，是则删除
                    if (feInfo.CreationTime < DateTime.Today.AddDays(-2))
                        feInfo.Delete();
                }
            }

            //返回下载地址
            return httpurl;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}