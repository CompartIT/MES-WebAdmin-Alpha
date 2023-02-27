using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebAdmin.Controllers
{
    public class ReportCentreController : BaseController
    {
        // GET: ReportCentre
        public ActionResult WIPReportView()
        {
            return View();
        }

        public ActionResult InputToBEReportView()
        {
            return View();
        }

        public ActionResult MRBReworkReportView()
        {
            return View();
        }

        public ActionResult MRBDetailsReportView()
        {
            return View();
        }

        public ActionResult ProcessTimeComparisonReportView()
        {
            return View();
        }
    }
}