using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DescargasUI.Actions
{
    public class ZipArchiveResult : ActionResult
    {
        private readonly byte[] zip;

        public ZipArchiveResult(byte[] zip)
        {
            this.zip = zip;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            try
            {
                // Siempre comprobar si la conexión sigue en pie
                if (context.HttpContext.Response.IsClientConnected)
                {
                    context.HttpContext.Response.ContentType = "application/zip";
                    context.HttpContext.Response.CacheControl = "private";
                    context.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    context.HttpContext.Response.AddHeader("Content-Length", zip == null ? "0" : zip.Length.ToString());
                    context.HttpContext.Response.BinaryWrite(zip == null ? new byte[1] : zip);
                    //context.HttpContext.Response.TransmitFile();
                    context.HttpContext.Response.Flush();
                }
            }
            catch (HttpException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message, "ZipError");
                Log.Error(ex, ex.Message);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message, "ZipError");
                Log.Error(e, e.Message);
            }
            finally
            {
                context.HttpContext.Response.End();
            }
        }
    }
}