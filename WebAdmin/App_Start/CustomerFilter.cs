using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.Mvc;
using WebAdmin.Models;
using WebAdmin.Controllers;
using System.Globalization;
using System.Data;

namespace WebAdmin.App_Start
{
    public class CustomerFilter : ActionFilterAttribute
    {
        private const string _adminSessionKey = "Admin";
        private const string DefaultLanguage = "en";

        //需要每个行动都执行一次，否则自动重置
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SysAdmin user = (SysAdmin)filterContext.HttpContext.Session[_adminSessionKey];
            if (user == null ) { return; }//|| (user.LangChanged == false)

            string language = user.Language != null ? user.Language : DefaultLanguage;
            CultureInfo ci = CultureInfo.GetCultureInfo(language);
            if (user.LangChanged == true)
            {
                user.LangChanged = false;
                //修改菜单
                List<SimpleWebUserRole> userRoles = user.WebUserRoleList;
                for (int i = 0; i < userRoles.Count; i++)
                {
                    userRoles[i].DisplayName = Resources.Resource.ResourceManager.GetString(userRoles[i].OriName, ci);
                }
                user.WebUserRoleList = userRoles;

                userRoles = user.WebUserRoleList2;
                for (int i = 0; i < userRoles.Count; i++)
                {
                    userRoles[i].DisplayName = Resources.Resource.ResourceManager.GetString(userRoles[i].OriName, ci);
                }
                user.WebUserRoleList2 = userRoles;

                userRoles = user.WebUserRoleList3;
                for (int i = 0; i < userRoles.Count; i++)
                {
                    userRoles[i].DisplayName = Resources.Resource.ResourceManager.GetString(userRoles[i].OriName, ci);
                }
                user.WebUserRoleList3 = userRoles;
                filterContext.HttpContext.Session[_adminSessionKey] = user;

                //修改cookie，保存默认语言设置
                HttpCookie userHc = filterContext.HttpContext.Request.Cookies["user"];
                if (userHc != null)
                {
                    userHc.Values["LanguageShow"] = user.LanguageShow;
                    userHc.Values["Language"] = user.Language;
                    filterContext.HttpContext.Response.SetCookie(userHc);
                }
            }

            //当前线程的语言采用哪种语言（比如zh，en等）
            Thread.CurrentThread.CurrentUICulture = ci;
            //决定各种数据类型是何组织，如数字与日期
            Thread.CurrentThread.CurrentCulture = ci;
        }
    }
}