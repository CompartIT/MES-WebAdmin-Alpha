using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAdmin.Models;
using Resources;
using System.Globalization;

namespace WebAdmin.Controllers
{
    public class BaseController : Controller
    {
        private const string _adminSessionKey = "Admin";
        private const string DefaultLanguage = "en";
        protected internal new ViewResult View()
        {
            SysAdmin user = this.GetAdminInfo();
            ViewBag.DisplayName = "";
            if (user != null)
            {
                ViewBag.DisplayName = user.DisplayName;
                ViewBag.UserRole = user.UserRole;
                ViewBag.Language = user.Language;
                ViewBag.LanguageShow = user.LanguageShow;
                ViewBag.LangChanged = user.LangChanged;
                ViewBag.WebUserRoleList = user.WebUserRoleList;
                ViewBag.WebUserRoleList2 = user.WebUserRoleList2;
                ViewBag.WebUserRoleList3 = user.WebUserRoleList3;
            }
            
            return base.View();
        }

        /// <summary>
        /// 用于检测是否含菜单权限
        /// </summary>
        /// <param name="Menu">菜单名</param>
        /// <returns></returns>
        protected internal Boolean CheckRight(string Menu)
        {
            SysAdmin user = this.GetAdminInfo();
            bool hasRight = false;
            if (user != null)
            {
                foreach (SimpleWebUserRole simpleWebUserRole in user.WebUserRoleList3)
                {
                    if (simpleWebUserRole.RoleName == Menu)
                    {
                        hasRight = true;
                        break;
                    }
                }
            }

            return hasRight;
        }

        protected internal Boolean CheckRight(string[] MenuList)
        {
            SysAdmin user = this.GetAdminInfo();
            bool hasRight = false;
            if (user != null)
            {
                foreach (SimpleWebUserRole simpleWebUserRole in user.WebUserRoleList3)
                {
                    foreach (string Menu in MenuList)
                    {
                        if (simpleWebUserRole.RoleName == Menu)
                        {
                            hasRight = true;
                            break;
                        }
                    }
                }
            }

            return hasRight;
        }

        public SysAdmin GetAdminInfo()
        {
            if (Session[_adminSessionKey] is SysAdmin)
                return Session[_adminSessionKey] as SysAdmin;
            else
                return null;
        }

        public void SetAdminInfo(SysAdmin admin)
        {
            Session[_adminSessionKey] = admin;
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        /// <param name="Language">系统里语言代码</param>
        /// <param name="LanguageShow">显示的语言名称</param>
        public void SetLanguage(string Language, string LanguageShow)
        {
            SysAdmin user = this.GetAdminInfo();
            if (user != null)
            {
                user.Language = Language;
                user.LanguageShow = LanguageShow;
                user.LangChanged = true;
            }
        }

        /// <summary>
        /// 获取resource文件的语言值
        /// </summary>
        /// <param name="Key">Resource中的名称</param>
        /// <returns>Resource中的值</returns>
        public string GetResValue(string Key)
        {
            SysAdmin user = (SysAdmin)Session[_adminSessionKey];
            string language;
            if (user != null)
                language = user.Language != null ? user.Language : DefaultLanguage;
            else
                language = DefaultLanguage;

            CultureInfo ci = CultureInfo.GetCultureInfo(language);
            string result = Resource.ResourceManager.GetString(Key, ci);
            if (result == null)
                return Key;
            else
                return result;
        }
    }
}