using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<ActionResult> DescargasXml(int anioSelect)
        {
            //Console.WriteLine(anioSelect);
            Dictionary<int, string> meses = await DescargasXmlNegocio.ObtenerMesesXmls(anioSelect);

            return Json(JsonConvert.SerializeObject(meses));
        }

        [HttpPost]
        public  async Task<ActionResult> ListaPartidas(int anioSelect)
        {
            Dictionary<string, List<string>> ramos = await DescargasXmlNegocio.ObtenerPartidas(anioSelect);

            return Json(JsonConvert.SerializeObject(ramos));
        }

        [HttpPost]
        public async Task<ActionResult> DescargarXml(int anio, int mes, string partidas, int carpetas)
        {
           
            string[] lista = partidas.Split(',');
            byte[] archivos;
            
            if(mes == 0)
            {
                archivos = await DescargasXmlNegocio.ObtenerXmlsAnio(anio, lista, carpetas);
            }
            else
            {
                archivos = await DescargasXmlNegocio.ObtenerXmls(anio, mes, lista, carpetas);
            }
            

            return new ZipArchiveResult(archivos);
        }

        [HttpPost]
        public async Task<ActionResult> DescargarXls(int anio, int mes, string partidas)
        {
            string[] lista = partidas.Split(',');
            byte[] archivos;
            
            archivos = await DescargasXmlNegocio.ObtenerReportes(anio, mes, lista);
            //return File(archivos, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"); ;
            return new ZipArchiveResult(archivos);
        }

        [HttpPost]
        public async Task<ActionResult> Datos(int anio, int mes, string partidas)
        {

            string[] lista = partidas.Split(',');

            List<Decimal> archivos = await DescargasXmlNegocio.ObtenerTotal(anio, mes, lista);

            return Json(JsonConvert.SerializeObject(archivos));
        }
    }
}