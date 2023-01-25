using Microsoft.Ajax.Utilities;
using NegocioLoginCentralizado;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DescargasUI.Filters;
using DescargasUI.Models;

namespace DescargasUI.Controllers
{
    public class ValidadorController : Controller
    {
        public ActionResult InicioAplicacion()
        {
            string nombreSesionApp = "TokenDescifrado" + ConfigurationManager.AppSettings["NombreMiProyecto"];
            NegocioLoginCentralizado.DTOs.TokenDTOs.DescifradoTokenDTO tokenDescifrado = (NegocioLoginCentralizado.DTOs.TokenDTOs.DescifradoTokenDTO)Session["" + nombreSesionApp + ""];

            if (!Convert.ToBoolean(ConfigurationManager.AppSettings["MoodDeveloper"]))
            {
                if (tokenDescifrado == null || !SessionSecurityFilter.horarioValidoToken(tokenDescifrado.InicioFechaToken, tokenDescifrado.ExpiracionFechaToken))
                {
                    string direccionservervidor = System.Diagnostics.Debugger.IsAttached ? "localhost:44312" : "172.19.3.171:85";/* ruta que tendra dentro del servidor*/
                    return Redirect("http://" + direccionservervidor + "/SesionUsuario/InicioApp");
                }

                //OBTENER LISTA DE PERMISOS SEGUN LAS NECESIDADES DEL APLICATIVO SI ASI SE REQUIERE 
                List<string> accionesPermitidas = new List<string> { "DescargasExternos", "DescargasXml", "SubirXmls" };
                string nombreVarPermisosEmpleado = "ListaPermisosWeb" + ConfigurationManager.AppSettings["NombreMiProyecto"];
                Session["" + nombreVarPermisosEmpleado + ""] = accionesPermitidas;
            }

            InformacionPersonalInicialesModel infoPersonal = new InformacionPersonalInicialesModel();
            infoPersonal.NombreEmpleado = tokenDescifrado != null ? tokenDescifrado.NombreEmpleado : "Mood Developer";
            infoPersonal.PuestoEmpleado = tokenDescifrado != null ? tokenDescifrado.PuestoEmpleado : "DEVELOPER";
            

            // ************************************************************************************************************* //
            // EL MODELO QUE SE ENVIA ES PARA QUE EL SIDEBAR CONTENGA LOS LINK’S REFERENTE A LOS PERMISOS QUE PUEDE VISITAR  //
            // ************************************************************************************************************ //
            return View(infoPersonal);
        }

        public async Task<ActionResult> ValidaToken(string token)
        {
            string direccionservervidor = "";
            if (!string.IsNullOrEmpty(token))
            {
                NegocioLoginCentralizado.DTOs.TokenDTOs.DescifradoTokenDTO tokenDescifrado = await TokenNegocio.DescifrarToken(token);


                if (!tokenDescifrado.NumEmpleado.IsNullOrWhiteSpace())
                {
                    if (SessionSecurityFilter.horarioValidoToken(tokenDescifrado.InicioFechaToken, tokenDescifrado.ExpiracionFechaToken))
                    {
                        string nombreVarTokenDescifrado = "TokenDescifrado" + ConfigurationManager.AppSettings["NombreMiProyecto"];
                        Session["" + nombreVarTokenDescifrado + ""] = tokenDescifrado;

                        string nombreVarPermisosEmpleado = "ListaPermisosWeb" + ConfigurationManager.AppSettings["NombreMiProyecto"];
                        Session["" + nombreVarPermisosEmpleado + ""] = null;

                        /** SIN USO POR EL MOMENTO**/
                        //string nombreVarDatosEmpleado = "UsuarioAPP" + ConfigurationManager.AppSettings["NombreMiProyecto"];
                        //Session["" + nombreVarDatosEmpleado + ""] = null;

                        //Reenviar a la pagina principal del aplicativo 
                        return RedirectToAction("InicioAplicacion", "Validador");

                    }
                    else
                    {
                        direccionservervidor = System.Diagnostics.Debugger.IsAttached ? "localhost:44312" : "172.19.3.171:85";/* ruta que tendra dentro del servidor*/
                        return Redirect("http://" + direccionservervidor + "/SesionUsuario/InicioApp");

                    }
                }

            }

            //direccionservervidor = System.Diagnostics.Debugger.IsAttached ? "localhost:44312" : "172.19.3.171:85";/* ruta que tendra dentro del servidor*/
            direccionservervidor = System.Diagnostics.Debugger.IsAttached ? "localhost:44312" : "172.19.3.171:85";/* ruta que tendra dentro del servidor*/
            return Redirect("http://" + direccionservervidor + "/SesionUsuario/InicioApp");
        }


        public ActionResult CerrarSesion()
        {
            string nombreVarTokenDescifrado = "TokenDescifrado" + ConfigurationManager.AppSettings["NombreMiProyecto"];
            Session[""+ nombreVarTokenDescifrado + ""] = null;
            Session.Remove(""+ nombreVarTokenDescifrado + "");

            string nombreVarPermisosEmpleado = "ListaPermisosWeb" + ConfigurationManager.AppSettings["NombreMiProyecto"];
            Session[""+ nombreVarPermisosEmpleado + ""] = null;
            Session.Remove("" + nombreVarPermisosEmpleado + "");

            /** SIN USO POR EL MOMENTO**/
            //string nombreVarDatosEmpleado = "UsuarioAPP" + ConfigurationManager.AppSettings["NombreMiProyecto"];
            //Session["" + nombreVarDatosEmpleado + ""] = null;
            //Session.Remove(""+ nombreVarDatosEmpleado + "");

            HttpContext.Session.Abandon();
            //FormsAuthentication.SignOut();
            //string direccionservervidor = System.Diagnostics.Debugger.IsAttached ? "localhost:44312" : "172.19.3.171:85";/* ruta que tendra dentro del servidor*/
            string direccionservervidor = System.Diagnostics.Debugger.IsAttached ? "localhost:44312" : "172.19.3.171:85";/* ruta que tendra dentro del servidor*/
            return Redirect("http://" + direccionservervidor + "/SesionUsuario/InicioApp");
        }

    }
}