using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DescargasUI.Filters;
using Entidades.DTOs.SubirXmlsDTOs;
using Entidades.Entidades;
using Negocio;
using Newtonsoft.Json;
using Serilog;

namespace DescargasUI.Controllers
{
    public class SubirXmlsController : Controller
    {
        [SessionSecurityFilter]
        public ActionResult SubirXmls()
        {
            return View();
        }

        public PartialViewResult renderModulo(int modulo)
        {
            if(modulo == 0)
            {
                return PartialView("_FileUploader");
            }
            else
            {
                List<LoteRegistroDTO> registros = SubirXmlsNegocio.ObtenerRegistros();

                return PartialView("_HistorialCargas", registros);
            }
        }

        public ActionResult obtenerUnidades()
        {
            List<string> partidas = SubirXmlsNegocio.obtenerPartidas();

            return Json(JsonConvert.SerializeObject(partidas));
        }

        public JsonResult GuardarRegistro()
        {
            List<ArchivoDTO> xmls = new List<ArchivoDTO>();
            var archivos = Request.Files;
            var unidad = Request.Form.Get("unidad");

            for(int i = 0; i < archivos.Count; i++)
            {
                HttpPostedFileBase file = archivos[i];

                byte[] contenido = null;

                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    contenido = binaryReader.ReadBytes(file.ContentLength);
                }

                xmls.Add( new ArchivoDTO {
                    nombre = file.FileName,
                    contenido = contenido 
                });
            }

            List<ArchivoDTO> response = SubirXmlsNegocio.SubirDatos(xmls,unidad);

            var jsonResult = Json(response);
            jsonResult.MaxJsonLength = 20000000;

            return jsonResult;
        }

        public ActionResult EliminarRegistro(string fecha)
        {
            bool respuesta = SubirXmlsNegocio.EliminarRegistro(fecha);

            return Json(respuesta);
        }

        public PartialViewResult _ListaArchivos(ArchivoLista[] queue)
        {
            return PartialView("_ListaArchivos", queue);
        }

    }
}
