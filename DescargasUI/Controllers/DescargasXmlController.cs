using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DescargasUI.Actions;
using DescargasUI.Filters;
using Entidades.DTOs;
using Negocio;
using Newtonsoft.Json;

namespace DescargasUI.Controllers
{
    public class DescargasXmlController : Controller
    {
        [SessionSecurityFilter]
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
        public ActionResult DescargarXml(int anio, int mes, string partidas, int carpetas)
        {
           
            string[] lista = partidas.Split(',');
            byte[] archivos;
            
            if(mes == 0)
            {
                archivos = DescargasXmlNegocio.ObtenerXmlsAnio(anio, lista, carpetas);
            }
            else
            {
                archivos = DescargasXmlNegocio.ObtenerXmls(anio, mes, lista, carpetas);
            }
            

            return new ZipArchiveResult(archivos);
        }

        [HttpPost]
        public ActionResult DescargarXls(int anio, int mes, string partidas)
        {

            string[] lista = partidas.Split(',');
            byte[] archivos;
            
            archivos = DescargasXmlNegocio.ObtenerReportes(anio, mes, lista);
            //return File(archivos, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"); ;
            return new ZipArchiveResult(archivos);
        }

        [HttpPost]
        public ActionResult Datos(int anio, int mes, string partidas)
        {

            string[] lista = partidas.Split(',');

            List<Decimal> archivos = DescargasXmlNegocio.ObtenerTotal(anio, mes, lista);

            return Json(JsonConvert.SerializeObject(archivos));
        }
    }
}