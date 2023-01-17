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
    public class LoginController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index(string flag = "", string Action = "", string Controller = "", string strPartNum = "")
        {
            if (1 == 1)
            {
                #region 判断是否能连接到数据库
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

                    string cmdText = "select top 1 * from webjobhead";
                    cmd.CommandText = cmdText;
                    int val = cmd.ExecuteNonQuery();
                }
                #endregion
            }
            if (!string.IsNullOrEmpty(flag))
            {
                ViewBag.Flag = flag;
            }
            else
            {
                ViewBag.Flag = "";
            }
            HttpCookie cookie = Request.Cookies["user"];
            if (cookie != null && cookie.Values["username"] != "" && cookie.Values["password"] != "")
            {
                ViewBag.Username = cookie.Values["username"];
                ViewBag.Password = cookie.Values["password"];
                ViewBag.Remember = "1";
            }
            else
            {
                ViewBag.Remember = "0";
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult DoLogin(string username, string password, string remember)
        {
            string strIP = HttpContext.Request.UserHostAddress;
            string cmdText = string.Format("Login_CheckLogin '{0}','{1}'", username, strIP);
            string strCheckTimes = SqlHelper.ExecteNonQuery2(CommandType.Text, cmdText);

            if (strCheckTimes == "")
            {
                cmdText = string.Format(@"
                        select *
                        from WebUser
                        where UserType in('1','3') and Active = 1 and UserName = '{0}' and Password = '{1}'
                    ",
                        username,
                        password
                    );
                DataTableCollection dtcQuery = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                if (dtcQuery[0].Rows.Count > 0)
                {
                    //先设置cookie
                    HttpCookie user = Request.Cookies["user"];
                    if (user == null)
                    {
                        user = new HttpCookie("user");
                        user.Values["LanguageShow"] = "English";
                        user.Values["Language"] = "en";
                    }
                    if (remember.Equals("1"))
                    {
                        user.Values["username"] = username;
                        user.Values["password"] = password;
                        user.Expires = DateTime.Now.AddYears(1);
                        System.Web.HttpContext.Current.Response.SetCookie(user);
                    }
                    else
                    {
                        user.Values["username"] = "";
                        user.Values["password"] = "";
                        user.Expires = DateTime.Now.AddYears(1);
                        System.Web.HttpContext.Current.Response.SetCookie(user);
                    }

                    DataRow row = dtcQuery[0].Rows[0];

                    SysAdmin admin = new SysAdmin();
                    admin.UserName = row["UserName"].ToString();
                    admin.DisplayName = row["DisplayName"].ToString();
                    admin.UserRole = row["UserRole"].ToString();
                    admin.LangChanged = true;
                    admin.Language = user.Values["Language"];
                    admin.LanguageShow = user.Values["LanguageShow"];

                    cmdText = string.Format(@"SP_GetMenu_ByUser {0}", row["id"]);
                    DataTableCollection dtcUserRole = SqlHelper.GetTable(CommandType.Text, cmdText, null);
                    List<SimpleWebUserRole> userRoles = new List<SimpleWebUserRole>();
                    foreach (DataRow rowUR in dtcUserRole[0].Rows)
                    {
                        SimpleWebUserRole userRole = new SimpleWebUserRole();
                        userRole.id = rowUR["id"].ToString();
                        userRole.UserId = rowUR["UserId"].ToString();
                        userRole.RoleId = rowUR["RoleId"].ToString();
                        userRole.RoleName = rowUR["RoleName"].ToString();
                        userRole.OriName = rowUR["DisplayName"].ToString();
                        userRole.DisplayName = GetResValue(rowUR["DisplayName"].ToString());
                        userRole.RoleType = rowUR["RoleType"].ToString();
                        userRole.RoleAction = rowUR["RoleAction"].ToString();
                        userRole.RoleParameter = rowUR["RoleParameter"].ToString();
                        userRole.DisplayOrder = rowUR["DisplayOrder"].ToString();
                        userRole.SecMenuID = rowUR["SecMenuID"].ToString();
                        userRole.FirMenuID = rowUR["FirMenuID"].ToString();
                        userRole.IconClass = rowUR["IconClass"].ToString();
                        userRoles.Add(userRole);
                    }
                    admin.WebUserRoleList = userRoles;

                    List<SimpleWebUserRole> userRoles2 = new List<SimpleWebUserRole>();
                    foreach (DataRow rowUR in dtcUserRole[1].Rows)
                    {
                        SimpleWebUserRole userRole = new SimpleWebUserRole();
                        userRole.id = rowUR["id"].ToString();
                        userRole.UserId = rowUR["UserId"].ToString();
                        userRole.RoleId = rowUR["RoleId"].ToString();
                        userRole.RoleName = rowUR["RoleName"].ToString();
                        userRole.OriName = rowUR["DisplayName"].ToString();
                        userRole.DisplayName = GetResValue(rowUR["DisplayName"].ToString());
                        userRole.RoleType = rowUR["RoleType"].ToString();
                        userRole.RoleAction = rowUR["RoleAction"].ToString();
                        userRole.RoleParameter = rowUR["RoleParameter"].ToString();
                        userRole.DisplayOrder = rowUR["DisplayOrder"].ToString();
                        userRole.SecMenuID = rowUR["SecMenuID"].ToString();
                        userRole.FirMenuID = rowUR["FirMenuID"].ToString();
                        userRoles2.Add(userRole);
                    }
                    admin.WebUserRoleList2 = userRoles2;

                    List<SimpleWebUserRole> userRoles3 = new List<SimpleWebUserRole>();
                    foreach (DataRow rowUR in dtcUserRole[2].Rows)
                    {
                        SimpleWebUserRole userRole = new SimpleWebUserRole();
                        userRole.id = rowUR["id"].ToString();
                        userRole.UserId = rowUR["UserId"].ToString();
                        userRole.RoleId = rowUR["RoleId"].ToString();
                        userRole.RoleName = rowUR["RoleName"].ToString();
                        userRole.OriName = rowUR["DisplayName"].ToString();
                        userRole.DisplayName = GetResValue(rowUR["DisplayName"].ToString());
                        userRole.RoleType = rowUR["RoleType"].ToString();
                        userRole.RoleAction = rowUR["RoleAction"].ToString();
                        userRole.RoleParameter = rowUR["RoleParameter"].ToString();
                        userRole.DisplayOrder = rowUR["DisplayOrder"].ToString();
                        userRole.SecMenuID = rowUR["SecMenuID"].ToString();
                        userRole.FirMenuID = rowUR["FirMenuID"].ToString();
                        userRoles3.Add(userRole);
                    }
                    admin.WebUserRoleList3 = userRoles3;

                    SetAdminInfo(admin);

                    if (TempData.ContainsKey("Action") && TempData.ContainsKey("Controller")
                        && TempData["Action"].ToString() != "" && TempData["Controller"].ToString() != "")
                    {
                        return RedirectToAction(TempData["Action"].ToString(), TempData["Controller"].ToString(), new { PartNum = TempData["PartNum"].ToString() });
                    }
                    else
                        return RedirectToAction("Home");
                }
                else
                {
                    cmdText = string.Format("Login_SetErrLogin '{0}','{1}'", username, strIP);
                    SqlHelper.ExecteNonQuery(CommandType.Text, cmdText);

                    return RedirectToAction("Index", new { flag = "ERROR" });
                }
            }
            else
            {
                return RedirectToAction("Index", new { flag = "ErrLoginTimes" });
            }
        }

        public ActionResult Home() {
            ViewBag.Module = "";

            return View();
        }

        public ActionResult DoLogout()
        {
            this.SetAdminInfo(null);
            return RedirectToAction("Index");
        }
    }
}