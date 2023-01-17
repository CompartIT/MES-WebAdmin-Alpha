using System;
using System.IO;
using System.Data;
using System.Web;
using System.Web.SessionState;
using System.Web.Script.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using O2S.Components.PDFRender4NET;
using WebAdmin.Models;
using System.Globalization;
using Resources;

namespace WebAdmin.Handler
{
    /// <summary>
    /// Summary description for FileUploadHandler
    /// </summary>
    public class FileUploadHandler : IHttpHandler, IRequiresSessionState
    {
        private const string _adminSessionKey = "Admin";
        private const string DefaultLanguage = "en";

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            bool isSavedSuccessfully = true;
            string getFileName = "", attachFileName = "";
            string ErrMsg = "";
            string NewLine = "\r\n";
            JavaScriptSerializer json = new JavaScriptSerializer();
            string strResult = null;
            string strSql = "";

            try
            {
                strSql = string.Format("EWI_Detail_Check {0}", context.Request.Form["RevID"]);
                ErrMsg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);

                if (ErrMsg != "")
                {
                    isSavedSuccessfully = false;
                }
                else
                {
                    int UploadType = Convert.ToInt32(context.Request.Form["UploadType"]);
                    HttpFileCollection httpFileCollection = context.Request.Files;
                    var serverPathPDF = "";
                    if (UploadType == 0)
                        serverPathPDF = HttpContext.Current.Server.MapPath("..\\UploadFiles\\PDF\\");
                    else if(UploadType == 1)
                        serverPathPDF = HttpContext.Current.Server.MapPath("..\\UploadFiles\\CustomerFile\\");
                    else if (UploadType == 2)
                        serverPathPDF = HttpContext.Current.Server.MapPath("..\\UploadFiles\\CAD\\");

                    foreach (string fileName in context.Request.Files)
                    {
                        HttpPostedFile file = httpFileCollection.Get(fileName);
                        if (file != null && file.ContentLength > 0)
                        {
                            FileInfo fileAttachment = new FileInfo(file.FileName);
                            attachFileName = fileAttachment.Name.ToString();

                            var getNoDuplicateFileName = AvoidDuplicateFile(file, serverPathPDF, attachFileName, attachFileName, 1, out getFileName);
                            string ImageList = "";
                            try
                            {
                                file.SaveAs(getNoDuplicateFileName);

                                if (UploadType == 0)
                                {
                                    //转换成图片
                                    PDFFile pdfFile = PDFFile.Open(getNoDuplicateFileName);
                                    for (int index = 0; index < pdfFile.PageCount; index++)
                                    {
                                        Bitmap pageImage = pdfFile.GetPageImage(index, 56 * 10);
                                        pageImage.Save(getNoDuplicateFileName.Replace(".pdf", "_" + index.ToString() + ".png").Replace("\\PDF\\", "\\Images\\"), ImageFormat.Png);
                                        ImageList += (ImageList == "" ? "" : ";") + getNoDuplicateFileName.Replace(".pdf", "_" + index.ToString() + ".png").Replace(serverPathPDF, "");
                                        pageImage.Dispose();
                                    }
                                    pdfFile.Dispose();
                                }

                                //写入数据库[PDF写在数据库，方便修改路径]
                                SimpleCurRight CurRight = GetCurRight();
                                string FilePath = string.Format("{0}://{1}:{2}/UploadFiles/", context.Request.Url.Scheme, context.Request.Url.Host, context.Request.Url.Port);
                                if (UploadType == 0)
                                {
                                    strSql = string.Format("EWI_Detail_InsertMedia {0},'{1}','{2}','{3}','{4}','{5}','{6}'",
                                        context.Request.Form["RevID"], context.Request.Form["OP"],
                                        FilePath, getFileName, ImageList,
                                        (HttpContext.Current.Session["Admin"] as SysAdmin).UserName,
                                        CurRight.Menu);
                                }
                                else if (UploadType == 1) {
                                    strSql = string.Format("EWI_UpdateData N'[CustomerFileName]=N''{0}''',{1},{2},'{3}','{4}'",
                                        getFileName, context.Request.Form["RevID"], 1, (HttpContext.Current.Session["Admin"] as SysAdmin).UserName, CurRight.Menu);
                                }
                                else if (UploadType == 2)
                                {
                                    strSql = string.Format("EWI_UpdateData N'[CADFileName]=N''{0}''',{1},{2},'{3}','{4}'",
                                        getFileName, context.Request.Form["RevID"], 1, (HttpContext.Current.Session["Admin"] as SysAdmin).UserName, CurRight.Menu);
                                }
                                ErrMsg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                                if (ErrMsg != "")
                                    ErrMsg = GetResValue(ErrMsg);

                                if (ErrMsg != "")
                                    isSavedSuccessfully = false;
                            }
                            catch (Exception ex)
                            {
                                ErrMsg += (ErrMsg.Length == 0 ? "" : NewLine) + attachFileName + " : " + ex.Message;
                                isSavedSuccessfully = false;
                            }
                        }
                    }
                }

                if (isSavedSuccessfully)
                {
                    if (attachFileName == getFileName)
                        strResult = json.Serialize(new { Status = "success", NewName = getFileName, ErrMsg = GetResValue("Txt_File") + getFileName });
                    else
                        strResult = json.Serialize(new { Status = "success", NewName = getFileName, ErrMsg = GetResValue("Txt_FileRename") + "[" + getFileName + @"]" });
                    context.Response.StatusCode = 200;
                    context.Response.Write(strResult);
                }
                else
                {
                    strResult = json.Serialize(new { Status = "error", ErrMsg = ErrMsg });
                    context.Response.StatusCode = 400;
                    context.Response.Write(strResult);
                }
            }
            catch(Exception ex)
            {
                strResult = json.Serialize(new { Status = "error", ErrMsg = ex.Message });
                context.Response.StatusCode = 400;
                context.Response.Write(strResult);
            }
        }

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
            if (HttpContext.Current.Session["CurRight"] == null)
                HttpContext.Current.Session["CurRight"] = CurRight;
            else
                CurRight = (SimpleCurRight)HttpContext.Current.Session["CurRight"];

            return CurRight;
        }

        public static string AvoidDuplicateFile(HttpPostedFile file, string serverPathPDF, string originalFileName, string newFileName, int intsequence, out string genNewFileName)
        {
            var getFileFullPath = serverPathPDF + newFileName;

            //If file exists
            if (File.Exists(getFileFullPath))
            {
                var getNewFileName = originalFileName.Insert(originalFileName.LastIndexOf("."), "_" + intsequence.ToString());
                genNewFileName = getNewFileName;

                return AvoidDuplicateFile(file, serverPathPDF, originalFileName, getNewFileName, intsequence + 1, out genNewFileName);
            }
            else
            {
                genNewFileName = newFileName;
                return getFileFullPath;
            }
        }

        /// <summary>
        /// Here to generate a unique numbers
        /// </summary>
        /// <returns>retuns as string type</returns>
        public static string GenerateUniqueNumbers()
        {
            string strReturn = "";

            byte[] buffer = Guid.NewGuid().ToByteArray();
            strReturn = BitConverter.ToInt64(buffer, 0).ToString();

            //Return
            return strReturn;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public string GetResValue(string Key)
        {
            SysAdmin user = (SysAdmin)HttpContext.Current.Session[_adminSessionKey];
            string language;
            if (user != null)
                language = user.Language != null ? user.Language : DefaultLanguage;
            else
                language = DefaultLanguage;

            CultureInfo ci = CultureInfo.GetCultureInfo(language);
            return Resource.ResourceManager.GetString(Key, ci);
        }
    }
}