using DescargasUI.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negocio;
using DescargasUI.Filters;

namespace DescargasUI.Controllers
{
    public class DescargasExternosController : Controller
    {
        [SessionSecurityFilter]
        public ActionResult DescargasExternos()
        {
            List<int> Anios = DescargasExternosNegocio.ObtenerAniosXmls();
            ViewBag.anios = Anios;
            return View();
        }

        [HttpPost]
        public ActionResult DescargasExternos(int anioSelect)
        {
            //Console.WriteLine(anioSelect);
            Dictionary<int, string> meses = DescargasExternosNegocio.ObtenerMesesXmls(anioSelect);

            return Json(JsonConvert.SerializeObject(meses));
        }

        [HttpPost]
        public ActionResult ListaPartidas(int anioSelect)
        {
            Dictionary<string, List<string>> ramos = DescargasExternosNegocio.ObtenerPartidas(anioSelect);

            return Json(JsonConvert.SerializeObject(ramos));
        }

        [HttpPost]
        public ActionResult DescargarXml(int anio, int mes, string partidas, int carpetas)
        {

            string[] lista = partidas.Split(',');
            byte[] archivos;

            if (mes == 0)
            {
                archivos = DescargasExternosNegocio.ObtenerXmlsAnio(anio, lista, carpetas);
            }
            else
            {
                archivos = DescargasExternosNegocio.ObtenerXmls(anio, mes, lista, carpetas);
            }


            return new ZipArchiveResult(archivos);
        }

        [HttpPost]
        public ActionResult DescargarXls(int anio, int mes, string partidas)
        {

            string[] lista = partidas.Split(',');
            byte[] archivos;

            archivos = DescargasExternosNegocio.ObtenerXls(anio, mes, lista);
            return File(archivos, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"); ;
        }

        [HttpPost]
        public ActionResult Datos(int anio, int mes, string partidas)
        {

            string[] lista = partidas.Split(',');

            List<Decimal> archivos = DescargasExternosNegocio.ObtenerTotal(anio, mes, lista);

            return Json(JsonConvert.SerializeObject(archivos));
        }
    }
}
