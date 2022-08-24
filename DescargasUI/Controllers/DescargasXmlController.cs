using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DescargasUI.Actions;
using Entidades.DTOs;
using Negocio;
using Newtonsoft.Json;

namespace DescargasUI.Controllers
{
    public class DescargasXmlController : Controller
    {
        public ActionResult DescargasXml()
        {
            List<int> Anios = DescargasXmlNegocio.ObtenerAniosXmls();
            ViewBag.anios = Anios;
            return View();
        }

        [HttpPost]
        public ActionResult DescargasXml(int anioSelect)
        {
            //Console.WriteLine(anioSelect);
            Dictionary<int, string> meses = DescargasXmlNegocio.ObtenerMesesXmls(anioSelect);

            return Json(JsonConvert.SerializeObject(meses));
        }

        [HttpPost]
        public ActionResult ListaPartidas(int anioSelect)
        {
            Dictionary<string, List<string>> ramos = DescargasXmlNegocio.ObtenerPartidas(anioSelect);

            return Json(JsonConvert.SerializeObject(ramos));
        }

        [HttpPost]
        public ActionResult Descargar(int anio, int mes, string partidas, int carpetas)
        {
           
            string[] lista = partidas.Split(',');

            byte[] archivos = DescargasXmlNegocio.ObtenerXmls(anio, mes, lista, carpetas);
            //string fileName = "Descarga_"+mes+"_"+anio; 
            //return Json(new { url = archivos } , JsonRequestBehavior.AllowGet);
            //return File(archivos,"application/zip");

            return new ZipArchiveResult(archivos);
        }
        [HttpPost]
        public ActionResult Datos(int anio, int mes, string partidas)
        {

            string[] lista = partidas.Split(',');

            List<int> archivos = DescargasXmlNegocio.ObtenerTotal(anio, mes, lista);

            return Json(JsonConvert.SerializeObject(archivos));
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        
    }
}