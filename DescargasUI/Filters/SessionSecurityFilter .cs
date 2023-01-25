using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc.Filters;
using System.Web.Mvc;
using System.Configuration;
using System.Web.Routing;

namespace DescargasUI.Filters
{

    public class SessionSecurityFilter : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            if (!Convert.ToBoolean(ConfigurationManager.AppSettings["MoodDeveloper"]))
            {
                string nombreVarTokenDescifrado = "TokenDescifrado" + ConfigurationManager.AppSettings["NombreMiProyecto"];
                NegocioLoginCentralizado.DTOs.TokenDTOs.DescifradoTokenDTO Token = (NegocioLoginCentralizado.DTOs.TokenDTOs.DescifradoTokenDTO)filterContext.HttpContext.Session["" + nombreVarTokenDescifrado + ""];

                bool forzarCerradoSesion = false;
                if (Token != null)
                {

                    if (horarioValidoToken(Token.InicioFechaToken, Token.ExpiracionFechaToken))
                    {
                        string nombreVarPermisosEmpleado = "ListaPermisosWeb" + ConfigurationManager.AppSettings["NombreMiProyecto"];
                        List<string> accionesPermitidas = (List<string>)filterContext.HttpContext.Session["" + nombreVarPermisosEmpleado + ""];

                        string controlador = Convert.ToString(filterContext.RouteData.Values["controller"]);
                        string accion = Convert.ToString(filterContext.RouteData.Values["action"]);

                        //aqui deberia ir el contexto de los vistas a donde puede llegar el aplicativo 
                        if (accionesPermitidas.Contains(accion.Trim()))
                        {
                            //El token esta DENTRO DEL HORARIO PERMITIDO y quiere ir a un RECURSO  PERMITIDO
                            filterContext.RouteData.Values["controller"] = controlador;
                            filterContext.RouteData.Values["action"] = accion;
                        }
                        else
                        {
                            //El token esta DENTRO DEL HORARIO PERMITIDO y quiere ir a un RECURSO NO PERMITIDO

                            filterContext.Result = new RedirectToRouteResult(
                            new RouteValueDictionary
                            {
                                     { "controller", "Errores" },
                                     { "action", "RecursoNoPermitido" }
                            });
                        }

                    }
                    else
                    {
                        //FUERA DEL HORARIO PERMITIDO  HAY QUE REGRESARLO AL LOGIN INICIAL PARA QUE SE LE SEA ASIGNADO UN NUEVO LOGIN
                        forzarCerradoSesion = true;
                    }
                }
                else
                {
                    forzarCerradoSesion = true;
                }


                if (forzarCerradoSesion)
                {
                    filterContext.Result = new RedirectToRouteResult(
                          new RouteValueDictionary
                          {
                                     { "controller", "Validador" },
                                     { "action", "CerrarSesion" }
                          });
                }
            }
        }


        public static bool horarioValidoToken(DateTime InicioSesionHora, DateTime ExpiraSesionHora)
        {
            bool esHorarioValido = false;
            DateTime diaHoraActual = DateTime.Now;
            if (diaHoraActual >= InicioSesionHora && diaHoraActual <= ExpiraSesionHora)
            {
                esHorarioValido = true;
            }

            return esHorarioValido;
        }




    }


}