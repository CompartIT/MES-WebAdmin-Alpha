using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Reflection;
using WebAdmin.Models;

namespace WebAdmin
{
    /*Kanban*/
    public class DAL_MES:BAL_MES
    {
        private static Encryption encryption = new Encryption();


        /// <summary>
        /// Get Process time comparision list
        /// </summary>
        public static IEnumerable<ProcessTimeComparisionEntity> GetProcessTimeComparision(out int TotalRecords, int intPageStart, int intPageLength, string SqlQuery, string OrderColumn, string OrderDirection)
        {
            var objQueryObjectName = "v_test";

            SqlParameter[] objParams = {
                new SqlParameter("@PageRowStart",intPageStart),
                new SqlParameter("@PageLength",intPageLength),
                new SqlParameter("@TableName",objQueryObjectName),
                new SqlParameter("@FieldConnections","*"),
                new SqlParameter("@SqlQuery",SqlQuery),
                new SqlParameter("@OrderColumn",OrderColumn),
                new SqlParameter("@OrderDirection",OrderDirection)
            };
            DataTable dtRsult = SqlHelper.ExecuteDataTable(CommandType.StoredProcedure, "SP_Pagination", objParams);

            TotalRecords = (int)SqlHelper.ExecuteScalar(CommandType.StoredProcedure, "SP_Pagination_AllRecords",
               new SqlParameter("@TableName", objQueryObjectName),
               new SqlParameter("@SqlQuery", SqlQuery));

            return (from r in dtRsult.AsEnumerable()

                    select new ProcessTimeComparisionEntity
                    {
                        JobNum = r.Field<string>("JobNum"),
                        PartNum = r.Field<string>("PartNum"),
                        UserID = r.Field<string>("UserID"),
                        MachineID = r.Field<string>("MachineID"),
                        PDAID = r.Field<string>("PDAID"),
                        OpCode = r.Field<string>("OpCode"),
                        OpDesc = r.Field<string>("OpDesc"),
                        OprSeq = r.Field<int>("OprSeq"),
                        OpGroup = r.Field<string>("OpGroup"),
                        StartTime = Convert.ToDateTime(r.Field<DateTime>("StartTime")).ToString("MM/dd/yyyy HH:mm:ss"),
                        EndTime =Convert.ToDateTime(r.Field<DateTime>("EndTime")).ToString("MM/dd/yyyy HH:mm:ss"),
                        StandardOprTime = r.Field<decimal>("StandardOprTime"),
                        ActualOprTime = r.Field<decimal>("ActualOprTime"),
                        Percentage = r.Field<decimal?>("PercentageVariance"),
                        ABSPercentageVariance = r.Field<decimal?>("ABSPercentageVariance"),
                    }
                    );
        }

        /// <summary>
        /// Get FE's Kanban list list
        /// </summary>
        public static IEnumerable<ShopFloorKanbanEntity> GetShopFloorKanbans(out IEnumerable<ShopFloorLocationEntity> LocationGroup)
        {
            var strSQL = "select * from V_WIP_FE";

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            LocationGroup = (from s in dtGetData.AsEnumerable()
                             group s by new { t1 = s.Field<string>("location") } into g
                             select new ShopFloorLocationEntity
                             {
                                 Location = g.Key.t1
                             });

            return ( from r in dtGetData.AsEnumerable()

                     select new ShopFloorKanbanEntity
                     {
                         JobNum= r.Field<string>("JobNum"),
                        // PartNum= r.Field<string>("PartNum"),
                         UserID= r.Field<string>("UserID"),
                         MachineID= r.Field<string>("MachineID"),
                         Location= r.Field<string>("Location"),
                         PDAID= r.Field<string>("PDAID"),
                         OpCode= r.Field<string>("OpCode"),
                         OpDesc= r.Field<string>("OpDesc"),
                         OpGroup= r.Field<string>("OpGroup"),
                         ReportingQTY= r.Field<int>("ReportingQTY"),
                         LaborQTY= r.Field<int>("LaborQTY"),
                         ScrapQTY= r.Field<int>("ScrapQTY"),
                         DiscrepQTY= r.Field<int>("DiscrepQTY"),
                         TransTime= Convert.ToDateTime(r.Field<DateTime>("TransTime")).ToString("MM/dd/yyyy HH:mm:ss")
                     }
                
                );
           
        }

        /// <summary>
        /// Get FE's Kanban list list
        /// </summary>
        public static IEnumerable<ShopFloorGroupedKanbanEntity> GetShopFloorKanbansByLocation(string strCriteria)
        {
            var strSQL = "select * from V_WIP_FE where "+ strCriteria;

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);
            var  i = 1;

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g orderby g.Count() descending

                    select new ShopFloorGroupedKanbanEntity
                    {
                        LocationGroup = g.Key.t1,
                        LocationOrder = i++,
                        ShopFloorKanbanList = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("OpGroup"), r.Field<string>("ResourceID")
                            where r.Field<string>("Location") == g.Key.t1
                            select new ShopFloorKanbanEntity {
                                TransType = r.Field<string>("TransType"),
                                JobNum = r.Field<string>("JobNum"),
                                PartNum = r.Field<string>("PartNum"),
                                UserID = r.Field<string>("UserID"),
                                MachineID = r.Field<string>("ResourceID"),
                                Location = r.Field<string>("Location"),
                                PDAID = r.Field<string>("PDAID"),
                                OpCode = r.Field<string>("OpCode"),
                                OpDesc = r.Field<string>("OpDesc"),
                                OpGroup = r.Field<string>("OpGroup"),
                                ReportingQTY = r.Field<int>("ReportingQTY"),
                                LaborQTY = r.Field<int>("LaborQTY"),
                                ScrapQTY = r.Field<int>("ScrapQTY"),
                                DiscrepQTY = r.Field<int>("DiscrepQTY"),
                                TransTime = Convert.ToDateTime(r.Field<DateTime>("TransTime")).ToString("yyyy/MM/dd HH:mm:ss")
                            }
                        )
                    }
                );
        }

        /// <summary>
        /// Get FE's Kanban list list
        /// </summary>
        public static IEnumerable<ShopFloorGroupedKanbanEntity> GetShopFloorKanbansByLocation_FA(string strCriteria)
        {
            var strSQL = "select V_WIP_FE.* from V_WIP_FE inner join WebMachineInfo on V_WIP_FE.ResourceId = WebMachineInfo.MachineId where isnull(FA,'')= 'FA'";

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorGroupedKanbanEntity
                    {
                        LocationGroup = g.Key.t1,
                        ShopFloorKanbanList = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("OpGroup"), r.Field<string>("JobNum")
                            where r.Field<string>("Location") == g.Key.t1
                            select new ShopFloorKanbanEntity
                            {
                                JobNum = r.Field<string>("JobNum"),
                                PartNum = r.Field<string>("PartNum"),
                                UserID = r.Field<string>("UserID"),
                                MachineID = r.Field<string>("ResourceID"),
                                Location = r.Field<string>("Location"),
                                PDAID = r.Field<string>("PDAID"),
                                OpCode = r.Field<string>("OpCode"),
                                OpDesc = r.Field<string>("OpDesc"),
                                OpGroup = r.Field<string>("OpGroup"),
                                ReportingQTY = r.Field<int>("ReportingQTY"),
                                LaborQTY = r.Field<int>("LaborQTY"),
                                ScrapQTY = r.Field<int>("ScrapQTY"),
                                DiscrepQTY = r.Field<int>("DiscrepQTY"),
                                TransTime = Convert.ToDateTime(r.Field<DateTime>("TransTime")).ToString("yyyy/MM/dd HH:mm:ss")
                            }
                        )
                    }
                );
        }

        /// <summary>
        /// Get FE's Kanban list list
        /// </summary>
        public static IEnumerable<ShopFloorGroupedKanbanEntity> GetShopFloorKanbansByLocation_Welding(string strCriteria)
        {
            var strSQL = "with cte as (select A.* from V_WIP_Welding A inner join WebMachineInfo B on a.MachineId=b.MachineId where B.MachineType='Orbtial Welder') select * from cte where " + strCriteria;

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorGroupedKanbanEntity
                    {
                        LocationGroup = g.Key.t1,
                        ShopFloorKanbanList = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("OpGroup"), r.Field<string>("JobNum")
                            where r.Field<string>("Location") == g.Key.t1
                            select new ShopFloorKanbanEntity
                            {
                                JobNum = r.Field<string>("JobNum"),
                                PartNum = r.Field<string>("PartNum"),
                                UserID = r.Field<string>("UserID"),
                                MachineID = r.Field<string>("ResourceID"),
                                Location = r.Field<string>("Location"),
                                PDAID = r.Field<string>("PDAID"),
                                OpCode = r.Field<string>("OpCode"),
                                OpDesc = r.Field<string>("OpDesc"),
                                OpGroup = r.Field<string>("OpGroup"),
                                ReportingQTY = r.Field<int>("ReportingQTY"),
                                LaborQTY = r.Field<int>("LaborQTY"),
                                ScrapQTY = r.Field<int>("ScrapQTY"),
                                DiscrepQTY = r.Field<int>("DiscrepQTY"),
                                TransTime = Convert.ToDateTime(r.Field<DateTime>("TransTime")).ToString("yyyy/MM/dd HH:mm:ss")
                            }
                        )
                    }
                );
        }

        /// <summary>
        /// Get EBW
        /// </summary>
        public static IEnumerable<ShopFloorGroupedKanbanEntity> GetShopFloorKanbansByLocation_EBW(string strCriteria)
        {
            var strSQL = "with cte as (select A.* from V_WIP_EBW A inner join WebMachineInfo B on a.MachineId=b.MachineId where B.MachineType='EBW') select * from cte where " + strCriteria;

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorGroupedKanbanEntity
                    {
                        LocationGroup = g.Key.t1,
                        ShopFloorKanbanList = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("OpGroup"), r.Field<string>("JobNum")
                            where r.Field<string>("Location") == g.Key.t1
                            select new ShopFloorKanbanEntity
                            {
                                JobNum = r.Field<string>("JobNum"),
                                PartNum = r.Field<string>("PartNum"),
                                UserID = r.Field<string>("UserID"),
                                MachineID = r.Field<string>("ResourceID"),
                                Location = r.Field<string>("Location"),
                                PDAID = r.Field<string>("PDAID"),
                                OpCode = r.Field<string>("OpCode"),
                                OpDesc = r.Field<string>("OpDesc"),
                                OpGroup = r.Field<string>("OpGroup"),
                                ReportingQTY = r.Field<int>("ReportingQTY"),
                                LaborQTY = r.Field<int>("LaborQTY"),
                                ScrapQTY = r.Field<int>("ScrapQTY"),
                                DiscrepQTY = r.Field<int>("DiscrepQTY"),
                                TransTime = Convert.ToDateTime(r.Field<DateTime>("TransTime")).ToString("yyyy/MM/dd HH:mm:ss")
                            }
                        )
                    }
                );
        }


        /// <summary>
        /// Get EBW
        /// </summary>
        public static IEnumerable<ShopFloorGroupedKanbanEntity> GetShopFloorKanbansByLocation_Burnishing(string strCriteria)
        {
            var strSQL = "with cte as (select A.* from V_WIP_Burnishing A inner join WebMachineInfo B on a.MachineId=b.MachineId where B.MachineType='Burnishing') select * from cte where " + strCriteria;

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorGroupedKanbanEntity
                    {
                        LocationGroup = g.Key.t1,
                        ShopFloorKanbanList = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("OpGroup"), r.Field<string>("JobNum")
                            where r.Field<string>("Location") == g.Key.t1
                            select new ShopFloorKanbanEntity
                            {
                                JobNum = r.Field<string>("JobNum"),
                                PartNum = r.Field<string>("PartNum"),
                                UserID = r.Field<string>("UserID"),
                                MachineID = r.Field<string>("ResourceID"),
                                Location = r.Field<string>("Location"),
                                PDAID = r.Field<string>("PDAID"),
                                OpCode = r.Field<string>("OpCode"),
                                OpDesc = r.Field<string>("OpDesc"),
                                OpGroup = r.Field<string>("OpGroup"),
                                ReportingQTY = r.Field<int>("ReportingQTY"),
                                LaborQTY = r.Field<int>("LaborQTY"),
                                ScrapQTY = r.Field<int>("ScrapQTY"),
                                DiscrepQTY = r.Field<int>("DiscrepQTY"),
                                TransTime = Convert.ToDateTime(r.Field<DateTime>("TransTime")).ToString("yyyy/MM/dd HH:mm:ss")
                            }
                        )
                    }
                );
        }

        /// <summary>
        /// Get Lapping
        /// </summary>
        public static IEnumerable<ShopFloorGroupedKanbanEntity> GetShopFloorKanbansByLocation_Lapping(string strCriteria)
        {
            var strSQL = "with cte as (select A.* from V_WIP_Lapping A inner join WebMachineInfo B on a.MachineId=b.MachineId where B.MachineType='Lapping Machine') select * from cte where " + strCriteria;

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorGroupedKanbanEntity
                    {
                        LocationGroup = g.Key.t1,
                        ShopFloorKanbanList = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("OpGroup"), r.Field<string>("JobNum")
                            where r.Field<string>("Location") == g.Key.t1
                            select new ShopFloorKanbanEntity
                            {
                                JobNum = r.Field<string>("JobNum"),
                                PartNum = r.Field<string>("PartNum"),
                                UserID = r.Field<string>("UserID"),
                                MachineID = r.Field<string>("ResourceID"),
                                Location = r.Field<string>("Location"),
                                PDAID = r.Field<string>("PDAID"),
                                OpCode = r.Field<string>("OpCode"),
                                OpDesc = r.Field<string>("OpDesc"),
                                OpGroup = r.Field<string>("OpGroup"),
                                ReportingQTY = r.Field<int>("ReportingQTY"),
                                LaborQTY = r.Field<int>("LaborQTY"),
                                ScrapQTY = r.Field<int>("ScrapQTY"),
                                DiscrepQTY = r.Field<int>("DiscrepQTY"),
                                TransTime = Convert.ToDateTime(r.Field<DateTime>("TransTime")).ToString("yyyy/MM/dd HH:mm:ss")
                            }
                        )
                    }
                );
        }


        /// <summary>
        /// Get all Machine list exclusivs FA 
        /// </summary>
        public static IEnumerable<ShopFloorMachineList> GetMachineList(string strCriteria)
        {
            var strSQL = "select * from WebMachineInfo  where isnull(FA,'')<>'FA' and (MachineType='Lathe' OR MachineType='Mill')"+ strCriteria;

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorMachineList
                    {
                        LocationGroup = g.Key.t1,
                        LocationGroupCount = g.Count(),
                        MachineType = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("MachineID")
                            where r.Field<string>("Location") == g.Key.t1
                            group r by new { g1 = r.Field<string>("MachineType") } into m
                            select new ShopFloorMachineType
                            {
                                MachineType = m.Key.g1,
                                MachineCount = m.Count()
                            }
                        ),
                        MachineList = (
                            from l in dtGetData.AsEnumerable()
                            where l.Field<string>("Location") == g.Key.t1


                            select new ShopFloorMachine
                            {
                                MachineID = l.Field<string>("MachineID")
                            }
                        )
                    }
                ); ;
        }

        /// <summary>
        /// Get all FA Machine list
        /// </summary>
        public static IEnumerable<ShopFloorMachineList> GetMachineList_FA(string strCriteria)
        {
            var strSQL = "select * from WebMachineInfo  where isnull(FA,'')='FA'";

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorMachineList
                    {
                        LocationGroup = g.Key.t1,
                        LocationGroupCount = g.Count(),
                        MachineType = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("MachineID")
                            where r.Field<string>("Location") == g.Key.t1
                            group r by new { g1 = r.Field<string>("MachineType") } into m
                            select new ShopFloorMachineType
                            {
                                MachineType = m.Key.g1,
                                MachineCount = m.Count()
                            }
                        ),
                        MachineList = (
                            from l in dtGetData.AsEnumerable()
                            where l.Field<string>("Location") == g.Key.t1


                            select new ShopFloorMachine
                            {
                                MachineID = l.Field<string>("MachineID")
                            }
                        )
                    }
                );
        }
        /// <summary>
        /// Get all Welding machines
        /// </summary>
        public static IEnumerable<ShopFloorMachineList> GetWeldingList(string strCriteria)
        {
            var strSQL = "select * from WebMachineInfo  where MachineType='Orbtial Welder'";

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorMachineList
                    {
                        LocationGroup = g.Key.t1,
                        LocationGroupCount = g.Count(),
                        MachineType = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("MachineID")
                            where r.Field<string>("Location") == g.Key.t1
                            group r by new { g1 = r.Field<string>("MachineType") } into m
                            select new ShopFloorMachineType
                            {
                                MachineType = m.Key.g1,
                                MachineCount = m.Count()
                            }
                        ),
                        MachineList = (
                            from l in dtGetData.AsEnumerable()
                            where l.Field<string>("Location") == g.Key.t1


                            select new ShopFloorMachine
                            {
                                MachineID = l.Field<string>("MachineID")
                            }
                        )
                    }
                ); ;
        }

        /// <summary>
        /// Get all Welding machines
        /// </summary>
        public static IEnumerable<ShopFloorMachineList> GetEBWList(string strCriteria)
        {
            var strSQL = "select * from WebMachineInfo  where MachineType='EBW'";

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorMachineList
                    {
                        LocationGroup = g.Key.t1,
                        LocationGroupCount = g.Count(),
                        MachineType = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("MachineID")
                            where r.Field<string>("Location") == g.Key.t1
                            group r by new { g1 = r.Field<string>("MachineType") } into m
                            select new ShopFloorMachineType
                            {
                                MachineType = m.Key.g1,
                                MachineCount = m.Count()
                            }
                        ),
                        MachineList = (
                            from l in dtGetData.AsEnumerable()
                            where l.Field<string>("Location") == g.Key.t1


                            select new ShopFloorMachine
                            {
                                MachineID = l.Field<string>("MachineID")
                            }
                        )
                    }
                ); ;
        }

        /// <summary>
        /// Get all Welding machines
        /// </summary>
        public static IEnumerable<ShopFloorMachineList> GetBurnishingList(string strCriteria)
        {
            var strSQL = "select * from WebMachineInfo  where MachineType='Burnishing'";

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorMachineList
                    {
                        LocationGroup = g.Key.t1,
                        LocationGroupCount = g.Count(),
                        MachineType = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("MachineID")
                            where r.Field<string>("Location") == g.Key.t1
                            group r by new { g1 = r.Field<string>("MachineType") } into m
                            select new ShopFloorMachineType
                            {
                                MachineType = m.Key.g1,
                                MachineCount = m.Count()
                            }
                        ),
                        MachineList = (
                            from l in dtGetData.AsEnumerable()
                            where l.Field<string>("Location") == g.Key.t1


                            select new ShopFloorMachine
                            {
                                MachineID = l.Field<string>("MachineID")
                            }
                        )
                    }
                ); ;
        }

        /// <summary>
        /// Get all Lapping machines
        /// </summary>
        public static IEnumerable<ShopFloorMachineList> GetLappingList(string strCriteria)
        {
            var strSQL = "select * from WebMachineInfo  where MachineType='Lapping Machine'";

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()
                    orderby s.Field<string>("location")
                    group s by new { t1 = s.Field<string>("location") } into g

                    select new ShopFloorMachineList
                    {
                        LocationGroup = g.Key.t1,
                        LocationGroupCount = g.Count(),
                        MachineType = (
                            from r in dtGetData.AsEnumerable()
                            orderby r.Field<string>("MachineID")
                            where r.Field<string>("Location") == g.Key.t1
                            group r by new { g1 = r.Field<string>("MachineType") } into m
                            select new ShopFloorMachineType
                            {
                                MachineType = m.Key.g1,
                                MachineCount = m.Count()
                            }
                        ),
                        MachineList = (
                            from l in dtGetData.AsEnumerable()
                            where l.Field<string>("Location") == g.Key.t1


                            select new ShopFloorMachine
                            {
                                MachineID = l.Field<string>("MachineID")
                            }
                        )
                    }
                ); ;
        }


        /// <summary>
        /// Get all FA machine list
        /// </summary>
        public static IEnumerable<ShopFloorFAMachineList> GetFAMachineList()
        {
            var strSQL = "select * from WebMachineInfo  where isnull(FA,'')='FA'";

            DataTable dtGetData = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            return (
                    from s in dtGetData.AsEnumerable()

                    select new ShopFloorFAMachineList
                    {
                        MachineID = s.Field<string>("MachineID")
                    }
                );
        }

        /// <summary>
        /// Get WIP data list
        /// </summary>
        public static IEnumerable<WIPEntity> GetWIP(out int TotalRecords, int intPageStart, int intPageLength, string SqlQuery, string OrderColumn, string OrderDirection)
        {
            var strSQL = "select  * from V_WIP where " + SqlQuery + " order by " + OrderColumn + " " + OrderDirection + "";

            DataTable dtRsult = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);
            TotalRecords = dtRsult.Rows.Count;

            var skipIndex = intPageStart;

            return (

                    from r in dtRsult.AsEnumerable()
                    .Skip(skipIndex).Take(intPageLength)

                    select new WIPEntity
                    {
                        Jtype = r.Field<string>("Jtype"),
                        TransType = r.Field<string>("TransType"),
                        MONo = r.Field<string>("MONo"),
                        ProductFamily = r.Field<string>("ProductFamily"),
                        ItemCode = r.Field<string>("ItemCode"),
                        HeatCode = r.Field<string>("HeatCode"),
                        LotNo = r.Field<string>("LotNo"),
                        OPCode = r.Field<string>("OPCode"),
                        OPDesc = r.Field<string>("OPDesc"),
                        OPCodeNext = r.Field<string>("OPCodeNext"),
                        OPDescNext = r.Field<string>("OPDescNext"),
                        WDate = Convert.ToDateTime(r.Field<DateTime>("WDate")).ToString("MM/dd/yyyy"),
                        WIPQTY = r.Field<decimal>("WIPQTY"),
                        UserId = r.Field<string>("UserId"),
                    }
                    );
        }

        /// <summary>
        /// Get WIP data list
        /// </summary>
        public static IEnumerable<WIPEntity> GetWIP2(out int TotalRecords,  string SqlQuery, string OrderColumn, string OrderDirection)
        {
            var strSQL = "select top 100  * from V_WIP ";

            DataTable dtRsult = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);
            TotalRecords = dtRsult.Rows.Count;

            return (

                    from r in dtRsult.AsEnumerable()

                    select new WIPEntity
                    {
                        Jtype = r.Field<string>("Jtype"),
                        TransType = r.Field<string>("TransType"),
                        MONo = r.Field<string>("MONo"),
                        ProductFamily = r.Field<string>("ProductFamily"),
                        ItemCode = r.Field<string>("ItemCode"),
                        HeatCode = r.Field<string>("HeatCode"),
                        LotNo = r.Field<string>("LotNo"),
                        OPCode = r.Field<string>("OPCode"),
                        OPDesc = r.Field<string>("OPDesc"),
                        OPCodeNext = r.Field<string>("OPCodeNext"),
                        OPDescNext = r.Field<string>("OPDescNext"),
                        WDate = Convert.ToDateTime(r.Field<DateTime>("WDate")).ToString("MM/dd/yyyy"),
                        WIPQTY = r.Field<decimal>("WIPQTY"),
                        UserId = r.Field<string>("UserId"),
                    }
                    );
        }

        /// <summary>
        /// Get the Input to BE data list
        /// Use the LINQ to SQl for the paging
        /// </summary>
        public static IEnumerable<InputToBEEntity> GetInputToBE(out int TotalRecords, int intPageStart, int intPageLength, string SqlQuery, string OrderColumn, string OrderDirection)
        {
            var strSQL = "select  * from V_InputToBE where "+SqlQuery+" order by "+OrderColumn+" "+OrderDirection+"";

            DataTable dtRsult = SqlHelper.ExecuteDataTable(CommandType.Text,strSQL);
            TotalRecords = dtRsult.Rows.Count;

            var skipIndex = (intPageStart - 1) * intPageLength;

            return (
                
                    from r in dtRsult.AsEnumerable()
                    .Skip(skipIndex).Take(intPageLength)

                    select new InputToBEEntity
                    {
                        Site = r.Field<string>("Site"),
                        JobNum = r.Field<string>("JobNum"),
                        FromOPCode = r.Field<string>("FromOPCode"),
                        ToOPCode = r.Field<string>("ToOPCode"),
                        TransType = r.Field<string>("TransType"),
                        TotalQty = r.Field<int>("TotalQty"),
                        Quantity = r.Field<int>("Quantity"),
                        Job = r.Field<string>("Job"),
                        StartDatetime = Convert.ToDateTime(r.Field<DateTime>("StartDatetime")).ToString("MM/dd/yyyy"),
                        UserID = r.Field<string>("UserID"),
                        Product = r.Field<string>("Product"),
                    }
                    );
        }

        /// <summary>
        /// Get MRB list
        /// </summary>
        public static IEnumerable<MRBReworkEEntity> GetMRBRework(out int TotalRecords, int intPageStart, int intPageLength, string SqlQuery, string OrderColumn, string OrderDirection)
        {
            var objQueryObjectName = "V_MRBRework";

            SqlParameter[] objParams = {
                new SqlParameter("@PageRowStart",intPageStart),
                new SqlParameter("@PageLength",intPageLength),
                new SqlParameter("@TableName",objQueryObjectName),
                new SqlParameter("@FieldConnections","*"),
                new SqlParameter("@SqlQuery",SqlQuery),
                new SqlParameter("@OrderColumn",OrderColumn),
                new SqlParameter("@OrderDirection",OrderDirection)
            };
            DataTable dtRsult = SqlHelper.ExecuteDataTable(CommandType.StoredProcedure, "SP_Pagination", objParams);

            TotalRecords = dtRsult.Rows.Count;

            return (from r in dtRsult.AsEnumerable()

                    select new MRBReworkEEntity
                    {
                        JType = r.Field<string>("JType"),
                        Transtype = r.Field<string>("Transtype"),
                        EpicorJobNum = r.Field<string>("EpicorJobNum"),
                        ProductFamily = r.Field<string>("ProductFamily"),
                        ItemCode = r.Field<string>("ItemCode"),
                        OpCode = r.Field<string>("OpCode"),
                        LotNo = r.Field<string>("LotNo"),
                        WDate = Convert.ToDateTime(r.Field<DateTime?>("WDate")).ToString("MM/dd/yyyy HH:mm:ss"),
                        ProdQty = r.Field<decimal>("ProdQty").ToString(),
                        PrintUserId = r.Field<string>("PrintUserId"),
                        PrintDateTime = Convert.ToDateTime(r.Field<DateTime?>("PrintDateTime")).ToString("MM/dd/yyyy HH:mm:ss"),
                        TransTime = r.Field<DateTime?>("TransTime") != null ? Convert.ToDateTime(r.Field<DateTime?>("TransTime")).ToString("yyyy-MM-dd") : "",
                        Site = r.Field<string>("Site"),
                    }
                    );
        }

        public static IEnumerable<MRBDetailEntity> GetMRBDetail(out int TotalRecords, int intPageStart, int intPageLength, string SqlQuery, string OrderColumn, string OrderDirection)
        {
            var objQueryObjectName = "V_MRBDetails";

            SqlParameter[] objParams = {
                new SqlParameter("@PageRowStart",intPageStart),
                new SqlParameter("@PageLength",intPageLength),
                new SqlParameter("@TableName",objQueryObjectName),
                new SqlParameter("@FieldConnections","*"),
                new SqlParameter("@SqlQuery",SqlQuery),
                new SqlParameter("@OrderColumn",OrderColumn),
                new SqlParameter("@OrderDirection",OrderDirection)
            };
            DataTable dtRsult = SqlHelper.ExecuteDataTable(CommandType.StoredProcedure, "SP_Pagination", objParams);

            TotalRecords = dtRsult.Rows.Count;

            return (from r in dtRsult.AsEnumerable()
                    select new MRBDetailEntity
                    {
                        MRBID = r.Field<int?>("MRBID"),
                        PartNum = r.Field<string>("PartNum"),
                        EpicorJobNum = r.Field<string>("EpicorJobNum"),
                        JobNum = r.Field<string>("JobNum"),
                        HeatCode = r.Field<string>("HeatCode"),
                        OprSeq = r.Field<int?>("OprSeq"),
                        OPDescription = r.Field<string>("OPDescription"),
                        Reason = r.Field<string>("Reason"),
                        RejQty = r.Field<int?>("RejQty"),
                        MRBCategoryDesc = r.Field<string>("MRBCategoryDesc"),
                        ReportId = r.Field<string>("ReportId"),
                        ReportPDAId = r.Field<string>("ReportPDAId"),
                        ReportTime = r.Field<DateTime?>("ReportTime") != null ? Convert.ToDateTime(r.Field<DateTime?>("ReportTime")).ToString("yyyy-MM-dd hh:mm:ss") : "",
                        AcceptName = r.Field<string>("AcceptName"),
                        ReceiptPDAId = r.Field<string>("ReceiptPDAId"),
                        ReceiptTime = r.Field<DateTime?>("ReceiptTime") != null ? Convert.ToDateTime(r.Field<DateTime?>("ReceiptTime")).ToString("yyyy-MM-dd hh:mm:ss") : "",
                        MHAcceptName = r.Field<string>("MHAcceptName"),
                        MHReceiptPDAId = r.Field<string>("MHReceiptPDAId"),
                        MHReceiptTime = r.Field<DateTime?>("MHReceiptTime") != null ? Convert.ToDateTime(r.Field<DateTime?>("MHReceiptTime")).ToString("yyyy-MM-dd hh:mm:ss") : "",
                        ReworkQty = r.Field<int?>("ReworkQty"),
                        ScrapQty = r.Field<int?>("ScrapQty"),
                        ProcessId = r.Field<string>("ProcessId"),
                        ProcessTime = r.Field<DateTime?>("ProcessTime") != null ? Convert.ToDateTime(r.Field<DateTime?>("ProcessTime")).ToString("yyyy-MM-dd hh:mm:ss") : "",
                        ReJobNum = r.Field<string>("ReJobNum"),
                        Location = r.Field<string>("Location"),
                        ResourceId = r.Field<string>("ResourceId"),
                        Remark = r.Field<string>("Remark"),
                        VendorRemark = r.Field<string>("VendorRemark"),
                        Category = r.Field<string>("Category"),
                        Status1 = r.Field<string>("Status1"),
                        Status = r.Field<string>("Status"),
                        ShortChar01 = r.Field<string>("ShortChar01"),
                        MachineId = r.Field<string>("MachineId"),
                        MfgEmp = r.Field<string>("MfgEmp"),
                        ProdFamily = r.Field<string>("ProdFamily"),
                    }
            );
        }

        /// <summary>
        /// Get the Monthly's Statistics report
        /// </summary>
        /// <param name="TallyNumber"></param>
        /// <returns></returns>
        public static void GetMonthlyOutput()
        {
            string strSQL = "";
            DataTable drGetData = SqlHelper.ExecuteDataTable(SQLConnectionSelection.EpicorReportingData, CommandType.Text,strSQL);

        }
     

      
        /// <summary>
        /// Get the Process Kanban information
        /// </summary>
        public static IEnumerable<ProcessKanbanEntity> GetProcessKanbanInfo(string SqlQuery, string t)
        {
            var strSQL =GetProcessKanbanSQL(t);

            DataTable dtResult = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);


            return (
                    from r in dtResult.AsEnumerable()
                    orderby r.Field<string>("ProdFamily")
                    group r by new { t1=r.Field<string>("ProdFamily") } into g

                    select new ProcessKanbanEntity
                    {
                        ProductFamily=g.Key.t1,
                        SumQTY=g.Sum(n=>n.Field<int>("ReportingQty")),
                        SumJobCost= g.Sum(n => n.Field<decimal>("JobCost")),
                        JobCount= g.Count(n => n.Field<int>("ReportingQty")>0),
                        ProcessKanbanDetailList =(
                            from s in dtResult.AsEnumerable()
                            orderby s.Field<DateTime>("TransTime")
                            where s.Field<string>("ProdFamily")==g.Key.t1
                            select new ProcessKanbanDetailEntity
                            {
                                ProductFamily= s.Field<string>("ProdFamily"),
                                PartNum= s.Field<string>("PartNum"),
                                TransType = s.Field<string>("TransType"),
                                JobNum= s.Field<string>("JobNum"),
                                OpCode= s.Field<string>("OpCode"),
                                OpCodeNext= s.Field<string>("OpCodeNext"),
                                OprQty= s.Field<int>("OprQty"),
                                ReportingQty= s.Field<int>("ReportingQty"),
                                LaborQty= s.Field<int>("LaborQty"),
                                StdCostUS= s.Field<decimal>("StdCostUS"),
                                JobCost= s.Field<decimal>("JobCost"),
                                TransTime =Convert.ToDateTime(s.Field<DateTime>("TransTime")).ToString("MM/dd/yyyy HH:mm:ss"),
                                Aging = s.Field<int>("Aging"),
                            }
                          )
                    }
                    );
        }

        /// <summary>
        /// Get the Process Kanban information -- For Awaiting to show the grouping
        /// </summary>
        public static ProcessKanbanAwaitingEntity GetProcessKanbanInfo_Awaiting(string SqlQuery, string t)
        {
            var strSQL = GetProcessKanbanSQL(t);

            DataTable dtResult = SqlHelper.ExecuteDataTable(CommandType.Text, strSQL);

            int SumQTY, SumJobCost, JobCount;
            var subQ = from s in dtResult.AsEnumerable()
                       group s by new { t = s.Field<string>("partNum") } into sub
                       select new {
                           SumQTY = sub.Sum(n => n.Field<int>("ReportingQty")),
                           SumJobCost = sub.Sum(n => n.Field<decimal>("JobCost")),
                           JobCount = sub.Count(n => n.Field<int>("ReportingQty") > 0),
                       };
                       
            var returnVal = new ProcessKanbanAwaitingEntity
            {
               ProcessKanbanGroupA= (
                    from r in dtResult.AsEnumerable()
                    where r.Field<string>("AgingGroup")== "GroupA"
                    orderby r.Field<string>("ProdFamily")
                    group r by new { t1 = r.Field<string>("ProdFamily") } into g

                    select new ProcessKanbanEntity
                    {
                        ProductFamily = g.Key.t1,
                        SumQTY = g.Sum(n => n.Field<int>("ReportingQty")),
                        SumJobCost = g.Sum(n => n.Field<decimal>("JobCost")),
                        JobCount = g.Count(n => n.Field<int>("ReportingQty") > 0),
                        ProcessKanbanDetailList = (
                            from s in dtResult.AsEnumerable()
                            orderby s.Field<DateTime>("TransTime")
                            where s.Field<string>("ProdFamily") == g.Key.t1 && s.Field<string>("AgingGroup") == "GroupA"
                            select new ProcessKanbanDetailEntity
                            {
                                ProductFamily = s.Field<string>("ProdFamily"),
                                PartNum = s.Field<string>("PartNum"),
                                TransType = s.Field<string>("TransType"),
                                JobNum = s.Field<string>("JobNum"),
                                OpCode = s.Field<string>("OpCode"),
                                OpCodeNext = s.Field<string>("OpCodeNext"),
                                OprQty = s.Field<int>("OprQty"),
                                ReportingQty = s.Field<int>("ReportingQty"),
                                LaborQty = s.Field<int>("LaborQty"),
                                StdCostUS = s.Field<decimal>("StdCostUS"),
                                JobCost = s.Field<decimal>("JobCost"),
                                TransTime = Convert.ToDateTime(s.Field<DateTime>("TransTime")).ToString("MM/dd/yyyy HH:mm:ss"),
                                Aging = s.Field<int>("Aging"),
                            }
                          )
                    }
                ),
                ProcessKanbanGroupB = (
                    from r in dtResult.AsEnumerable()
                    where r.Field<string>("AgingGroup") == "GroupB"
                    orderby r.Field<string>("ProdFamily")
                    group r by new { t1 = r.Field<string>("ProdFamily") } into g

                    select new ProcessKanbanEntity
                    {
                        ProductFamily = g.Key.t1,
                        SumQTY = g.Sum(n => n.Field<int>("ReportingQty")),
                        SumJobCost = g.Sum(n => n.Field<decimal>("JobCost")),
                        JobCount = g.Count(n => n.Field<int>("ReportingQty") > 0),
                        ProcessKanbanDetailList = (
                            from s in dtResult.AsEnumerable()
                            orderby s.Field<DateTime>("TransTime")
                            where s.Field<string>("ProdFamily") == g.Key.t1 && s.Field<string>("AgingGroup") == "GroupB"
                            select new ProcessKanbanDetailEntity
                            {
                                ProductFamily = s.Field<string>("ProdFamily"),
                                PartNum = s.Field<string>("PartNum"),
                                TransType = s.Field<string>("TransType"),
                                JobNum = s.Field<string>("JobNum"),
                                OpCode = s.Field<string>("OpCode"),
                                OpCodeNext = s.Field<string>("OpCodeNext"),
                                OprQty = s.Field<int>("OprQty"),
                                ReportingQty = s.Field<int>("ReportingQty"),
                                LaborQty = s.Field<int>("LaborQty"),
                                StdCostUS = s.Field<decimal>("StdCostUS"),
                                JobCost = s.Field<decimal>("JobCost"),
                                TransTime = Convert.ToDateTime(s.Field<DateTime>("TransTime")).ToString("MM/dd/yyyy HH:mm:ss"),
                                Aging = s.Field<int>("Aging"),
                            }
                          )
                    }
                ),
                ProcessKanbanGroupC = (
                    from r in dtResult.AsEnumerable()
                    where r.Field<string>("AgingGroup") == "GroupC"
                    orderby r.Field<string>("ProdFamily")
                    group r by new { t1 = r.Field<string>("ProdFamily") } into g

                    select new ProcessKanbanEntity
                    {
                        ProductFamily = g.Key.t1,
                        SumQTY = g.Sum(n => n.Field<int>("ReportingQty")),
                        SumJobCost = g.Sum(n => n.Field<decimal>("JobCost")),
                        JobCount = g.Count(n => n.Field<int>("ReportingQty") > 0),
                        ProcessKanbanDetailList = (
                            from s in dtResult.AsEnumerable()
                            orderby s.Field<DateTime>("TransTime")
                            where s.Field<string>("ProdFamily") == g.Key.t1 && s.Field<string>("AgingGroup") == "GroupC"
                            select new ProcessKanbanDetailEntity
                            {
                                ProductFamily = s.Field<string>("ProdFamily"),
                                PartNum = s.Field<string>("PartNum"),
                                TransType = s.Field<string>("TransType"),
                                JobNum = s.Field<string>("JobNum"),
                                OpCode = s.Field<string>("OpCode"),
                                OpCodeNext = s.Field<string>("OpCodeNext"),
                                OprQty = s.Field<int>("OprQty"),
                                ReportingQty = s.Field<int>("ReportingQty"),
                                LaborQty = s.Field<int>("LaborQty"),
                                StdCostUS = s.Field<decimal>("StdCostUS"),
                                JobCost = s.Field<decimal>("JobCost"),
                                TransTime = Convert.ToDateTime(s.Field<DateTime>("TransTime")).ToString("MM/dd/yyyy HH:mm:ss"),
                                Aging = s.Field<int>("Aging"),
                            }
                          )
                    }
                )
            };

            return returnVal;  
            
        }

        //public static IEnumerable<ProcessKanbanDetailEntity> getAwaitingByGroup(DataTable dtResult, string strGroup)
        //{
        //    return (
        //        from s in dtResult.AsEnumerable()
        //        orderby s.Field<DateTime>("TransTime")
        //        where s.Field<string>("ProdFamily") == g.Key.t1
        //        select new ProcessKanbanDetailEntity
        //        {
        //            ProductFamily = s.Field<string>("ProdFamily"),
        //            PartNum = s.Field<string>("PartNum"),
        //            TransType = s.Field<string>("TransType"),
        //            JobNum = s.Field<string>("JobNum"),
        //            OpCode = s.Field<string>("OpCode"),
        //            OpCodeNext = s.Field<string>("OpCodeNext"),
        //            OprQty = s.Field<int>("OprQty"),
        //            ReportingQty = s.Field<int>("ReportingQty"),
        //            LaborQty = s.Field<int>("LaborQty"),
        //            StdCostUS = s.Field<decimal>("StdCostUS"),
        //            JobCost = s.Field<decimal>("JobCost"),
        //            TransTime = Convert.ToDateTime(s.Field<DateTime>("TransTime")).ToString("MM/dd/yyyy HH:mm:ss"),
        //        }
        //    );
        //}


    }
}