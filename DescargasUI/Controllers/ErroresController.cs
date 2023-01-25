using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DescargasUI.Controllers
{
    public class ErroresController : Controller
    {
        // GET: Errores
        public ActionResult RecursoNoPermitido()
        {
            return View();
        }
    }
}