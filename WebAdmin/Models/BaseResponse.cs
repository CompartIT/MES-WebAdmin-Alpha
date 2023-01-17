using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAdmin.Models
{
    public class BaseResponse<T>
    {
        public string Msg { get; set; }
        public string MsgCode { get; set; }
        public T Data { get; set; }
    }
}