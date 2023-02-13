using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using WebAdmin.Models;

namespace WebAdmin.Controllers
{
    public class KanbanController : BaseController
    {
        
        public ActionResult FEMachineStatusView()
        {
            return View();
        }

        public ActionResult FAMachineStatusView()
        {
            return View();
        }

        public ActionResult OrbtialWelderStatusView()
        {
            return View();
        }

        public ActionResult EBWStatusView()
        {
            return View();
        }

        public ActionResult LappingStatusView()
        {
            return View();
        }

        public ActionResult BurnishingStatusView()
        {
            return View();
        }
    }
}