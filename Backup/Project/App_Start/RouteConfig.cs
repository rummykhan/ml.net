using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Project
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            var FrontEndNamespaces = new string[] { typeof(Project.Controllers.AccountController).Namespace };
            var AdminEndNamespaces = new string[] { typeof(Project.Areas.manage.Controllers.AccountController).Namespace };

            //Index of Site
            routes.MapRoute("Home", "", new { controller = "Home", action = "Index" }, FrontEndNamespaces);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: FrontEndNamespaces
            );
        }
    }
}