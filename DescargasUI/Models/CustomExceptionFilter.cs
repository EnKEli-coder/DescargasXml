using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Serilog;

namespace DescargasUI.Models
{
    public class CustomExceptionFilter : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.ExceptionHandled)
            {
                Log.Error(filterContext.Exception.ToString(), filterContext.Exception.Message);

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.StatusCode = 500;

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult()
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            Error = filterContext.Exception.Message
                        }
                    };
                }
                else
                {
                    var routeValues = new RouteValueDictionary();

                    routeValues.Add("Action", "SubirXmls");
                    routeValues.Add("Controller", "SubirXmls");

                    filterContext.Result = new RedirectToRouteResult(routeValues);
                }

               
            }
        }
    }
}