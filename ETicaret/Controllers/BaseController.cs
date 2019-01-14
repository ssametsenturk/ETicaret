using ETicaret.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaret.Controllers
{
    public class BaseController : Controller
    {
        protected ETicaretEntities context { get; private set; }
        public BaseController()
        {
            context = new ETicaretEntities();
            ViewBag.MenuCategories = context.Categories.Where(x => x.Parent_Id == null).ToList();
        }

        protected DB.Members CurrentUser()
        {
            return (DB.Members)Session["LogonUser"];
        }

        protected int CurrentUserId()
        {
            return ((DB.Members)Session["LogonUser"]).Id;
        }

    }
}