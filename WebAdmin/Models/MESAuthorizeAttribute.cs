using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebAdmin.Models
{
    public class MESAuthorizeAttribute: AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //根据需要添加
            filterContext.HttpContext.Response.Redirect("/Login/Index");

        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //根据需要添加，将自动根据返回值判断用户是否通过验证
            //true：通过
            //false:未通过
            bool result = false;
            if (httpContext.Session["Admin"] != null)
                result = true;
            return result;
        }
    }
}