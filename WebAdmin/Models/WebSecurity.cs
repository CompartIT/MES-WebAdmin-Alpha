using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAdmin.Models
{
    public class SimpleWebUser
    {
        public string id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string EmployeeId { get; set; }
        public string CreateDate { get; set; }
        public string UserType { get; set; }
        public string Active { get; set; }
        public string UserRole { get; set; }
        public string OperGroup { get; set; }
        public string LimitedOpCode { get; set; }

    }

    public class SimpleWebGroup
    {
        public string id { get; set; }
        public string GroupName { get; set; }
        public string DisplayName { get; set; }
        public string Active { get; set; }
    }

    public class SimpleWebUserGroup
    {
        public string id { get; set; }
        public string GroupId { get; set; }
        public string UserId { get; set; }
    }

    public class SimpleWebRole
    {
        public string id { get; set; }
        public string RoleName { get; set; }
        public string DisplayName { get; set; }
        public string RoleType { get; set; }
        public string RoleAction { get; set; }
        public string RoleParameter { get; set; }
        public string DisplayOrder { get; set; }
    }

    public class SimpleWebGroupRole
    {
        public string id { get; set; }
        public string GroupId { get; set; }
        public string RoleId { get; set; }
    }

    public class SimpleWebUserRole
    {
        public string id { get; set; }
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string OriName { get; set; }
        public string DisplayName { get; set; }
        public string RoleType { get; set; }
        public string RoleAction { get; set; }
        public string RoleParameter { get; set; }
        public string DisplayOrder { get; set; }
        public string SecMenuID { get; set; }
        public string FirMenuID { get; set; }
        public string IconClass { get; set; }
    }
}