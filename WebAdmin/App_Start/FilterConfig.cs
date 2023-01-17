using System.Web;
using System.Web.Mvc;
using WebAdmin.Models;
using WebAdmin.App_Start;

namespace WebAdmin
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MESAuthorizeAttribute());
            filters.Add(new CustomerFilter());
        }
    }
}
