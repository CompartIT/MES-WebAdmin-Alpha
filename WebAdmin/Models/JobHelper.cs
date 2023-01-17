using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.IO;

namespace WebAdmin.Models
{
    public class JobHelper
    {
        public static string GetOpGroup(string opCode)
        {
            string cmdText = string.Empty;
            DataTableCollection dtc;

            string opGroup = string.Empty;

            try
            {
                cmdText = string.Format(@"
                        select OpGroup_c,'' as OpTransfer_c
                        from OpMaster
                        where Company = '{0}' and OpCode = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    opCode
                );
                dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

                if (dtc[0].Rows.Count > 0)
                {
                    opGroup = dtc[0].Rows[0]["OpGroup_c"].ToString();
                }

            }
            catch (Exception ex)
            {

            }


            return opGroup;
        }

        public static string GetOpTransfer(string opCode)
        {
            string cmdText = string.Empty;
            DataTableCollection dtc;

            string opTransfer = string.Empty;

            try
            {
                cmdText = string.Format(@"
                        select OpGroup_c,'' as OpTransfer_c
                        from OpMaster
                        where Company = '{0}' and OpCode = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    opCode
                );
                dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

                if (dtc[0].Rows.Count > 0)
                {
                    opTransfer = dtc[0].Rows[0]["OpTransfer_c"].ToString();
                }

            }
            catch (Exception ex)
            {

            }


            return opTransfer;
        }

        public static bool TransferWIP(string Job, string FromOpr, string FromBin, string ToOpr, string ToBin, string Qty)
        {
            bool result = true;

            string cmdText = string.Empty;
            DataTableCollection dtc;

            cmdText = string.Format(@"
                    update WebPartWIP
                    set Quantity = Quantity - {4}
                    where Company = '{0}' and JobNum = '{1}' and OprSeq = {2} and Location = '{3}' 
                ",
                ConfigurationManager.AppSettings["CompanyCode"],
                Job,
                FromOpr,
                FromBin,
                Qty
            );
            SqlHelper.ExecteNonQueryText(cmdText, null);

            cmdText = string.Format(@"
                    select *
                    from WebPartWIP
                     where Company = '{0}' and JobNum = '{1}' and OprSeq = {2} and Location = '{3}' 
                ",
                ConfigurationManager.AppSettings["CompanyCode"],
                Job,
                ToOpr,
                ToBin
            );
            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            if (dtc[0].Rows.Count > 0)
            {
                cmdText = string.Format(@"
                    update WebPartWIP
                    set Quantity = Quantity + {4}
                    where Company = '{0}' and JobNum = '{1}' and OprSeq = {2} and Location = '{3}' 
                ",
                ConfigurationManager.AppSettings["CompanyCode"],
                Job,
                ToOpr,
                ToBin,
                Qty
            );
            }
            else
            {
                cmdText = string.Format(@"
                        insert into WebPartWIP(Company,PartNum,JobNum,AssemblySeq,OprSeq,Location,Quantity)
                        values('{0}','','{1}',0,{2},'{3}',{4})
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    Job,
                    ToOpr,
                    ToBin,
                    Qty
                );
            }
            SqlHelper.ExecteNonQueryText(cmdText, null);

            return result;
        }

        public static string GetNexOper(string JobNum)
        {
            string nextOper = string.Empty;

            string cmdText = string.Empty;
            DataTableCollection dtc;

            cmdText = string.Format(@"
                    select top 1 *
                    from WebJobOper
                    where Company = '{0}' and JobNum = '{1}' and JobStatus = 'INIT'
                    order by OprSeq
                ",
                ConfigurationManager.AppSettings["CompanyCode"],
                JobNum
            );
            dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            if (dtc[0].Rows.Count > 0)
            {
                nextOper = dtc[0].Rows[0]["OprSeq"].ToString();
            }

            return nextOper;
        }

        public static string GetVendorNameByNum(string vendorNum)
        {
            string vendorName = string.Empty;

            string cmdText = string.Empty;
            DataTableCollection dtc;

            try
            {
                cmdText = string.Format(@"
                        select Name
                        from Erp.Vendor
                        where Company = '{0}' and VendorNum = {1}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    vendorNum
                );
                dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

                if (dtc[0].Rows.Count > 0)
                {
                    vendorName = dtc[0].Rows[0]["Name"].ToString();
                }
            }
            catch (Exception ex)
            {

            }

            return vendorName;
        }

        public static string GetVendorNameByPONum(string poNum)
        {
            string vendorName = string.Empty;

            string cmdText = string.Empty;
            DataTableCollection dtc;

            try
            {
                cmdText = string.Format(@"
                        select Name
                        from Vendor as V
                        left join POHeader as PH on V.Company = PH.Company and V.VendorNum = PH.VendorNum
                        where PH.Company = '{0}' and PH.PONum = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    poNum
                );
                dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

                if (dtc[0].Rows.Count > 0)
                {
                    vendorName = dtc[0].Rows[0]["Name"].ToString();
                }
            }
            catch (Exception ex)
            {

            }

            return vendorName;
        }

        public static string GetVendorNameByPONumBak(string poNum)
        {
            string vendorName = string.Empty;

            string cmdText = string.Empty;
            DataTableCollection dtc;

            try
            {
                cmdText = string.Format(@"
                        select Name
                        from Vendor as V
                        left join POHeader as PH on V.Company = PH.Company and V.VendorNum = PH.VendorNum
                        where PH.Company = '{0}' and PH.PONum = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    poNum
                );
                dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

                if (dtc[0].Rows.Count > 0)
                {
                    vendorName = dtc[0].Rows[0]["Name"].ToString();
                }
            }
            catch (Exception ex)
            {

            }

            return vendorName;
        }
        public static string GetVendorIdByPONum(string poNum)
        {
            string vendorId = string.Empty;

            string cmdText = string.Empty;
            DataTableCollection dtc;

            try
            {
                cmdText = string.Format(@"
                        select V.VendorID,V.VendorNum
                        from Vendor as V
                        left join POHeader as PH on V.Company = PH.Company and V.VendorNum = PH.VendorNum
                        where PH.Company = '{0}' and PH.PONum = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    poNum
                );
                dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

                if (dtc[0].Rows.Count > 0)
                {
                    vendorId = dtc[0].Rows[0]["VendorID"].ToString();
                }
            }
            catch (Exception ex)
            {

            }

            return vendorId;
        }

        public static string GetVendorNumByPONum(string poNum)
        {
            string vendorId = string.Empty;

            string cmdText = string.Empty;
            DataTableCollection dtc;

            try
            {
                cmdText = string.Format(@"
                        select V.VendorID,V.VendorNum
                        from Vendor as V
                        left join POHeader as PH on V.Company = PH.Company and V.VendorNum = PH.VendorNum
                        where PH.Company = '{0}' and PH.PONum = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    poNum
                );
                dtc = SqlHelper.GetTable(ConfigurationManager.AppSettings["EpicorConn"], CommandType.Text, cmdText, null);

                if (dtc[0].Rows.Count > 0)
                {
                    vendorId = dtc[0].Rows[0]["VendorNum"].ToString();
                }
            }
            catch (Exception ex)
            {

            }

            return vendorId;
        }

        public static string GetEpicorJobNum(string webJobNum)
        {
            string cmdText = string.Empty;
            DataTableCollection dtc;

            string EpicorJobNum = string.Empty;

            try
            {
                cmdText = string.Format(@"
                        select EpicorJobNum
                        from WebJobHead
                        where Company = '{0}' and JobNum = '{1}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    webJobNum
                );
                dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                if (dtc[0].Rows.Count > 0)
                {
                    EpicorJobNum = dtc[0].Rows[0]["EpicorJobNum"].ToString();
                }

            }
            catch (Exception ex)
            {

            }


            return EpicorJobNum;
        }
        public static SimpleJobOper getJobOperBySeq(string JobNum, string OprSeq)
        {
            SimpleJobOper jobOper = new SimpleJobOper();

            string cmdText = string.Empty;
            DataTableCollection dtc;

            try
            {
                cmdText = string.Format(@"
                        select OpCode,OpDesc
                        from WebJobOper
                        where Company = '{0}' and JobNum = '{1}' and OprSeq = '{2}'
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],
                    JobNum,
                    OprSeq
                );
                dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                if (dtc[0].Rows.Count > 0)
                {
                    jobOper.OprCode = dtc[0].Rows[0]["OpCode"].ToString();
                    jobOper.OprDesc = dtc[0].Rows[0]["OpDesc"].ToString();
                }
            }
            catch (Exception ex)
            {

            }

            return jobOper;
        }

        public static void SyncSubconReceipt(string PackNum)
        {
            string pathTemplateHeader = string.Format("{0}\\Resource\\XMLTemplate\\SubcontractReceiptHeader.xml", System.AppDomain.CurrentDomain.BaseDirectory.ToString());
            string pathTemplateLine = string.Format("{0}\\Resource\\XMLTemplate\\SubcontractReceiptLine.xml", System.AppDomain.CurrentDomain.BaseDirectory.ToString());

            string pathInputChannel = ConfigurationManager.AppSettings["InputChannelPath"];
            string xmlHeader = File.ReadAllText(pathTemplateHeader);
            string xmlLine = File.ReadAllText(pathTemplateLine);
            string xmlLines = string.Empty;


            string cmdText = string.Empty;
            DataTableCollection dtc;

            if (string.IsNullOrEmpty(PackNum))
            {
                return;
            }

            try
            {
                cmdText = string.Format(@"
                            select *,JH.EpicorJobNum as EpicorJobNum
                            from WebSubconReceipt as SR
                            left join WebJobHead as JH on SR.Company = JH.Company and SR.JobNum = JH.JobNum
                            where SR.Company = '{0}' and SR.PackNum = '{1}' and SR.Sync = 1
                        ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        PackNum
                    );
                dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                foreach(DataRow row in dtc[0].Rows)
                {
                    string line = string.Empty;
                    int LaborQty = 0;
                    int InternalReworkQty = 0;

                    int.TryParse(row["LaborQty"].ToString(), out LaborQty);
                    int.TryParse(row["InternalReworkQty"].ToString(), out InternalReworkQty);

                    int ourQty = 0;
                    if (row["ReturnToVendor"].ToString().Equals("True"))
                    {
                        ourQty = LaborQty;
                    }
                    else
                    {
                        ourQty = LaborQty + InternalReworkQty;
                    }
                    line = string.Format(xmlLine,
                            ConfigurationManager.AppSettings["CompanyCode"],         //0:Company
                            GetVendorIdByPONum(row["PONum"].ToString()),             //1:VendorNumVendorID
                            "TRUE",                 //2:Received
                            "manager",              //3:ReceivePerson
                            "0000",                 //4:BinNum
                            row["PONum"],           //5:PONum
                            row["POLine"],          //6:POLine
                            row["PORelNum"],        //7:PORelNum
                            ourQty,                 //8:InputOurQty
                            "TRUE",                 //9:Received
                            row["EpicorJobNum"],    //10:JobNum
                            "0",                    //11:AssemblySeq
                            row["OprSeq"],          //12:JobSeq
                            "each",                 //13:IUM
                            row["PackNum"],         //14:PackNum
                            row["JobNum"],          //15:JobNum
                            row["id"]               //16:id
                        );
                    xmlLines += line;
                }

                xmlHeader = string.Format(xmlHeader,xmlLines);
                File.WriteAllText(string.Format("{0}\\OSReceipt-{1}.xml", pathInputChannel, Guid.NewGuid()), xmlHeader);


                cmdText = string.Format(@"                        
                            update WebSubconReceipt
                            set Sync = '2'
                            where Company = '{0}' and PackNum = '{1}'  and Sync = 1
                        ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        PackNum
                    );
                SqlHelper.ExecteNonQueryText(cmdText, null);
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        public static void SyncSubconShipment(string PackNum)
        {
            string pathTemplateHeader = string.Format("{0}\\Resource\\XMLTemplate\\SubcontractShipmentHeader.xml", System.AppDomain.CurrentDomain.BaseDirectory.ToString());
            string pathTemplateLine = string.Format("{0}\\Resource\\XMLTemplate\\SubcontractShipmentLine.xml", System.AppDomain.CurrentDomain.BaseDirectory.ToString());

            string pathInputChannel = ConfigurationManager.AppSettings["InputChannelPathShipment"];
            string xmlHeader = File.ReadAllText(pathTemplateHeader);
            string xmlLine = File.ReadAllText(pathTemplateLine);
            string xmlLines = string.Empty;


            string cmdText = string.Empty;
            DataTableCollection dtc;

            if (string.IsNullOrEmpty(PackNum))
            {
                return;
            }

            try
            {
                cmdText = string.Format(@"
                            select *
                            from WebSubconShipment
                            where Company = '{0}' and PackNum = '{1}' and Sync = 1
                        ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        PackNum
                    );
                dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                foreach (DataRow row in dtc[0].Rows)
                {
                    string line = string.Empty;

                    line = string.Format(xmlLine,
                            ConfigurationManager.AppSettings["CompanyCode"],         //0:Company
                            GetVendorNumByPONum(row["PONum"].ToString()),             //1:VendorNumVendorID
                            "manager",              //2:EntryPerson
                            "",                     //3:ShipComment
                            row["PONum"],           //4:PONum
                            row["POLine"],          //5:POLine
                            row["PORelNum"],        //6:PORelNum
                            row["Qty"],             //7:ShipQty
                            GetEpicorJobNum(row["JobNum"].ToString()),          //8:JobNum
                            "0",                    //9:AssemblySeq
                            row["OprSeq"],          //10:OprSeq
                            GetEpicorJobNum(row["JobNum"].ToString()),          //11:LotNum
                            row["JobNum"],          //12:WebJobNum
                            PackNum                 //13:PackNum
                        );
                    xmlLines += line;
                }

                xmlHeader = string.Format(xmlHeader, xmlLines);
                File.WriteAllText(string.Format("{0}\\Shipment-{1}.xml", pathInputChannel, Guid.NewGuid()), xmlHeader);


                cmdText = string.Format(@"                        
                            update WebSubconShipment
                            set Sync = '2'
                            where Company = '{0}' and PackNum = '{1}'  and Sync = 1
                        ",
                        ConfigurationManager.AppSettings["CompanyCode"],
                        PackNum
                    );
                SqlHelper.ExecteNonQueryText(cmdText, null);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }
    }
}