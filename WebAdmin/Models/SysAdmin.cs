using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAdmin.Models
{
    public class SysAdmin
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string UserRole { get; set; }
        public string Language { get; set; }
        public string LanguageShow { get; set; }
        public bool LangChanged { get; set; }
        public List<SimpleWebUserRole> WebUserRoleList { get; set; }
        public List<SimpleWebUserRole> WebUserRoleList2 { get; set; }
        public List<SimpleWebUserRole> WebUserRoleList3 { get; set; }

    }
}