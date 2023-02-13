using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;

using WebAdmin.Models;
namespace WebAdmin.Controllers
{
    public class SecurityController : BaseController
    {
        public JsonResult GetWebUserList(DataTablesParameters param)
        {
            /*2020-03-10*/
            string filter = string.Empty;
            string searchby = string.Empty;

            if (!string.IsNullOrEmpty(param.Search.Value))
            {
                string searchvalue = param.Search.Value;
                string usertype = searchvalue;
                string action = searchvalue;
                string roletype = searchvalue;
                
                if(searchvalue == GetResValue("Txt_WebUser"))
                    usertype = "1";
                if(searchvalue == GetResValue("Txt_Valid"))
                    action = "1";
                if (searchvalue == GetResValue("Txt_Invalid"))
                    action = "0";

                //switch (searchvalue)
                //{
                //    case "Web用户":
                //        usertype = "1";
                //        break;
                //    case "App用户":
                //        break;
                //    case "有效":
                //        action = "1";
                //        break;
                //    case "无效":
                //        action = "0";
                //        break;
                //    default:
                //        break;
                //}

                filter = string.Format(" and (UserName like '%{0}%' or DisplayName like '%{0}%' or EmployeeId like '%{0}%' or UserType like '%{1}%' or Active like '%{2}%' or UserRole like '%{3}%' or OperGroup like '%{0}%') ", param.Search.Value,usertype,action, roletype);
            }
            else if (!string.IsNullOrEmpty(param.Columns[0].Search.Value))
            {
                filter = string.Format(" and UserName like '%{0}%' ", param.Columns[0].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[2].Search.Value))
            {
                filter = string.Format(" and EmployeeId like '%{0}%' ", param.Columns[2].Search.Value);
            }
            else if (!string.IsNullOrEmpty(param.Columns[4].Search.Value))
            {
                filter = string.Format(" and OperGroup like '%{0}%' ", param.Columns[4].Search.Value);
            }

            if (Session["Admin"] is SysAdmin)
            {
                SysAdmin session = (SysAdmin)Session["Admin"];
                if (!session.UserName.Equals("admin"))
                {
                    filter += " and UserName <> 'admin' ";
                }
            }else
            {
                filter += " and UserName <> 'admin' ";
            }
                

            string cmdText = string.Format(@"
                    select count(id) recordsTotal
                    from WebUser
                    where 1=1 {1}
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter
            );
            DataTableCollection dtcQuery = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtcQuery[0].Rows[0]["recordsTotal"].ToString());

            cmdText = string.Format(@"
                        select top {1} *
                        from (
	                        select ROW_NUMBER() over(order by {3} {4}) as rownum,*
	                        from WebUser
                            where  1=1 {5}
                        ) as WebUser
                        where rownum > {2}  {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );

            dtcQuery = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleWebUser> userList = new List<SimpleWebUser>();
            SimpleWebUser user = new SimpleWebUser();

            foreach (DataRow row in dtcQuery[0].Rows)
            {
                user = new SimpleWebUser();
                user.id = row["id"].ToString();
                user.UserName = row["UserName"].ToString();
                user.Password = row["Password"].ToString();
                user.DisplayName = row["DisplayName"].ToString();
                user.EmployeeId = row["EmployeeId"].ToString();
                user.CreateDate = row["CreateDate"].ToString();
                user.UserType = row["UserType"].ToString();
                user.Active = row["Active"].ToString();
                user.UserRole = row["UserRole"].ToString();
                user.OperGroup = row["AccessGroup"].ToString();
                user.LimitedOpCode = row["LimitedOpCode"].ToString();

                userList.Add(user);
            }

            DataTablesResult<SimpleWebUser> result = new DataTablesResult<SimpleWebUser>(param.Draw, recordsTotal, recordsTotal, userList);
            return Json(result);
        }
        public ActionResult WebUserListView()
        {
            //检查菜单权限
            if (!CheckRight("WebUserListView"))
            {
                return Redirect("/Login/Home");
            }

            List<SimpleWebRole> roleList = new List<SimpleWebRole>();
            SimpleWebRole role = new SimpleWebRole();
            
            try
            {
                string cmdText = string.Format(@"
                        select *
                        from WebRole
                        where RoleType = 'Web'
                        order by DisplayOrder
                    "
                );

                DataTableCollection dtcQuery = SqlHelper.GetTable(CommandType.Text, cmdText, null);


                foreach (DataRow row in dtcQuery[0].Rows)
                {
                    role = new SimpleWebRole();
                    role.id = row["id"].ToString();
                    role.RoleName = row["RoleName"].ToString();
                    role.DisplayName = GetResValue(row["DisplayName"].ToString());
                    
                    roleList.Add(role);
                }

                LogHelper.Info(string.Format("Prepare Web User Successfully, total {0} users", dtcQuery.Count));
            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Format("Prepare Web User Error"), ex);
            }

            ViewBag.RoleList = roleList;
            ViewBag.InitPassword = ConfigurationManager.AppSettings["Initpwd"];
            return View();
        }

        public ActionResult WebGroupListView()
        {
            return View();
        }

        public JsonResult GetWebRoleList(DataTablesParameters param)
        {
            string filter = string.Empty;
            string searchby = string.Empty;

            string cmdText = string.Format(@"
                    select count(id) recordsTotal
                    from WebRole
                    where 1=1 {1}
                ",
                ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                filter
            );
            DataTableCollection dtcQuery = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            int recordsTotal = int.Parse(dtcQuery[0].Rows[0]["recordsTotal"].ToString());

            cmdText = string.Format(@"
                        select top {1} *
                        from (
	                        select ROW_NUMBER() over(order by {3} {4}) as rownum,*
	                        from WebRole
                            where  1=1 {5}
                        ) as WebRole
                        where rownum > {2}  {5}
                    ",
                    ConfigurationManager.AppSettings["CompanyCode"],    //{0}   CompanyID
                    param.Length,                                       //{1}   PageSize
                    param.Start,                                        //{2}   Start
                    param.OrderBy,                                      //{3}   OrderBy Column
                    param.OrderDir,                                     //{4}   OrderBy Direction ASC/DESC
                    filter                                              //{5}   filter
                );

            dtcQuery = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            List<SimpleWebRole> roleList = new List<SimpleWebRole>();
            SimpleWebRole role = new SimpleWebRole();

            foreach (DataRow row in dtcQuery[0].Rows)
            {
                role = new SimpleWebRole();
                role.id = row["id"].ToString();
                role.RoleName = row["RoleName"].ToString();
                role.DisplayName = row["DisplayName"].ToString();
                role.RoleType = row["RoleType"].ToString();
                role.RoleAction = row["RoleAction"].ToString();
                role.RoleParameter = row["RoleParameter"].ToString();
                role.DisplayOrder = row["DisplayOrder"].ToString();
                roleList.Add(role);
            }

            DataTablesResult<SimpleWebRole> result = new DataTablesResult<SimpleWebRole>(param.Draw, recordsTotal, recordsTotal, roleList);
            return Json(result);
        }

        public ActionResult WebRoleListView()
        {
            List<SimpleWebRole> roleList = new List<SimpleWebRole>();
            SimpleWebRole role = new SimpleWebRole();

            try
            {
                string cmdText = string.Format(@"
                        select *
                        from WebRole
                        order by RoleType,DisplayOrder
                    "
                );

                DataTableCollection dtcQuery = SqlHelper.GetTable(CommandType.Text, cmdText, null);


                foreach (DataRow row in dtcQuery[0].Rows)
                {
                    role = new SimpleWebRole();
                    role.id = row["id"].ToString();
                    role.RoleName = row["RoleName"].ToString();
                    role.DisplayName = row["DisplayName"].ToString();
                    role.RoleType = row["RoleType"].ToString();
                    role.RoleAction = row["RoleAction"].ToString();
                    role.RoleParameter = row["RoleParameter"].ToString();
                    role.DisplayOrder = row["DisplayOrder"].ToString();
                    roleList.Add(role);
                }

                LogHelper.Info(string.Format("Prepare Web Role Successfully, total {0} roles", dtcQuery.Count));
            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Format("Prepare Web Role Error"), ex);
            }

            ViewBag.RoleList = roleList;
            return View();
        }

        public ActionResult WebGroupRoleListView()
        {
            return View();
        }

        public string UpdateUser(SimpleWebUser user)
        {
            string cmdText = string.Empty;

            try
            {
                if (user.id == null)
                {
                    cmdText = string.Format(@"
                        insert into WebUser(UserName,Password,DisplayName,EmployeeId,UserType,Active,UserRole,AccessGroup,LimitedOpCode)
                        values('{0}','{1}',N'{2}','{3}','{4}','{5}','{6}','{7}','{8}')
                    ",
                    user.UserName,          //0:UserName
                    user.Password,          //1:Password
                    user.DisplayName,       //2:DisplayName
                    user.EmployeeId,        //3:EmployeeId
                    user.UserType,          //4:UserType
                    user.Active,            //5:Active
                    user.UserRole,          //6:UserRole
                    user.OperGroup,         //7:OperGroup
                    user.LimitedOpCode      //8:LimitedOpCode
                    );
                }
                else
                {
                    cmdText = string.Format(@"
                        update WebUser
                        set UserName='{0}',
                            Password = '{1}',
                            DisplayName = N'{2}',
                            EmployeeId = '{3}',
                            UserType = '{4}',
                            Active = '{5}',
                            UserRole = '{6}',
                            AccessGroup = '{7}',
                            LimitedOpCode = '{8}'
                        where id = {9}
                    ",
                    user.UserName,          //0:UserName
                    user.Password,          //1:Password
                    user.DisplayName,       //2:DisplayName
                    user.EmployeeId,        //3:EmployeeId
                    user.UserType,          //4:UserType
                    user.Active,            //5:Active
                    user.UserRole,          //6:UserRole
                    user.OperGroup,         //7:OperGroup
                    user.LimitedOpCode,     //8:LimitedOpCode
                    user.id                 //9:id
                    );
                }

                SqlHelper.ExecteNonQueryText(cmdText, null);
            }
            catch(Exception ex)
            {

            }
            

            return "";
        }

        public string GetUserById(string username)
        {
            string result = string.Empty;

            string cmdText = string.Format(@"
                        select *
                        from WebUser
                        where UserName = '{0}'
                    ",
                    username
                );

            DataTableCollection dtcQuery = SqlHelper.GetTable(CommandType.Text, cmdText, null);

            if (dtcQuery[0].Rows.Count > 0)
            {
                result = "user exist";
            }
            return result;
        }

        public string UpdateUserRole(List<SimpleWebUserRole> userRoles)
        {
            string userList = string.Empty;

            foreach (SimpleWebUserRole userRole in userRoles)
            {
                userList += string.Format("{0},", userRole.UserId);
            }

            userList = string.Format("UserId in ({0})", userList.Substring(0, userList.Length - 1));

            string cmdText = string.Format(@"
                    delete from WebUserRole
                    where {0}
                ",
                userList
            );

            SqlHelper.ExecteNonQueryText(cmdText, null);

            foreach(SimpleWebUserRole userRole in userRoles)
            {
                cmdText = string.Format(@"
                        insert into WebUserRole(UserId,RoleId)
                        values({0},{1})
                    ",
                    userRole.UserId,
                    userRole.RoleId
                );

                SqlHelper.ExecteNonQueryText(cmdText, null);
            }
            
            return "";
        }

        public JsonResult GetWebUserRoleById(string UserId)
        {

            List<SimpleWebUserRole> result = new List<SimpleWebUserRole>();

            try
            {
                string cmdText = string.Format(@"
                        select *
                        from WebUserRole
                        where UserId = {0}
                    ",
                    UserId
                );

                DataTableCollection dtc = SqlHelper.GetTable(CommandType.Text, cmdText, null);

                foreach (DataRow row in dtc[0].Rows)
                {
                    SimpleWebUserRole userRole = new SimpleWebUserRole();
                    userRole.id = row["id"].ToString();
                    userRole.UserId = row["UserId"].ToString();
                    userRole.RoleId = row["RoleId"].ToString();
                    result.Add(userRole);
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex);
            }
            

            return Json(result);
        }

        public JsonResult UpdatePassword(ChangePasswordViewModel ChangePassword)
        {
            BaseResponse<string> returnResponse = new BaseResponse<string>();
            returnResponse.Msg = "";

            SysAdmin user = GetAdminInfo();
            if (user == null)
                returnResponse.Msg = GetResValue("Txt_SessionExpired");
            else
            {
                //至少8个字符，至少1个大写字母，1个小写字母，1个数字和1个特殊字符：
                //string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$,./<>?;':!@#$%*&]{8,}";
                string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d,./<>?:;'!@_]{8,}";
                string patternEasy = @"^[A-Za-z\d,./<>?:;'!@_]{6,}";

                string strSql = string.Format("SP_User_ChangePwd '{0}','','',{1}",
                        user.UserName, 0);
                DataSet ds = SqlHelper.ExecuteDataSet(CommandType.Text, strSql);

                string UserType = "";
                if (ds.Tables[0].Rows.Count != 0)
                    UserType = ds.Tables[0].Rows[0][0].ToString();

                if (ChangePassword.NewPassword != ChangePassword.ConfirmPassword)
                {
                    returnResponse.Msg = GetResValue("Txt_PasswordNotMatch");
                }
                else if (UserType == "1" && !Regex.IsMatch(ChangePassword.NewPassword, pattern))
                {
                    returnResponse.Msg = GetResValue("Txt_ErrPwdRegular");
                }
                else if ((UserType == "2" || UserType == "3") 
                    && !Regex.IsMatch(ChangePassword.NewPassword, patternEasy))
                {
                    returnResponse.Msg = GetResValue("Txt_ErrPwdRegularEasy");
                }
                else
                {
                    strSql = string.Format("SP_User_ChangePwd '{0}','{1}','{2}'",
                        user.UserName, ChangePassword.OldPassword, ChangePassword.NewPassword.Replace("'","''"));
                    returnResponse.Msg = SqlHelper.ExecteNonQuery2(CommandType.Text, strSql);
                    if (returnResponse.Msg != "")
                        returnResponse.Msg = GetResValue(returnResponse.Msg);
                    else {
                        HttpCookie HCuser = Request.Cookies["user"];
                        HCuser.Values["password"] = ChangePassword.NewPassword;
                        System.Web.HttpContext.Current.Response.SetCookie(HCuser);
                    }
                }
            }
            return Json(returnResponse, JsonRequestBehavior.AllowGet);
        }
    }
}