using DescargasUI.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Negocio;
using DescargasUI.Filters;
using System.Threading.Tasks;

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
        public async Task<ActionResult> DescargasExternos(int anioSelect)
        {
            //Console.WriteLine(anioSelect);
            Dictionary<int, string> meses = await DescargasExternosNegocio.ObtenerMesesXmls(anioSelect);

            return Json(JsonConvert.SerializeObject(meses));
        }

        [HttpPost]
        public async Task<ActionResult> ListaPartidas(int anioSelect)
        {
            Dictionary<string, List<string>> ramos = await DescargasExternosNegocio.ObtenerPartidas(anioSelect);

            return Json(JsonConvert.SerializeObject(ramos));
        }

        [HttpPost]
        public async Task<ActionResult> DescargarXml(int anio, int mes, string partidas, int carpetas)
        {

            string[] lista = partidas.Split(',');
            byte[] archivos;

            if (mes == 0)
            {
                archivos = await DescargasExternosNegocio.ObtenerXmlsAnio(anio, lista, carpetas);
            }
            else
            {
                archivos = await DescargasExternosNegocio.ObtenerXmls(anio, mes, lista, carpetas);
            }


            return new ZipArchiveResult(archivos);
        }

        [HttpPost]
        public async Task<ActionResult> DescargarXls(int anio, int mes, string partidas)
        {

            string[] lista = partidas.Split(',');
            byte[] archivos;

            archivos = await DescargasExternosNegocio.ObtenerReportes(anio, mes, lista);
            return new ZipArchiveResult(archivos);
        }

        [HttpPost]
        public async Task<ActionResult> Datos(int anio, int mes, string partidas)
        {

            string[] lista = partidas.Split(',');

            List<Decimal> archivos = await DescargasExternosNegocio.ObtenerTotal(anio, mes, lista);

            return Json(JsonConvert.SerializeObject(archivos));
        }
    }
}
