﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebAdmin.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //string a = Resources.Resource.ResourceManager.GetString("Quantity");
            return View();
        }

        public ActionResult About()
        {
            //ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
           // ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}