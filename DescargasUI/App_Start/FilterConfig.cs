using DescargasUI.Models;
using System.Web;
using System.Web.Mvc;

namespace DescargasUI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new CustomExceptionFilter());
        }
    }
}
