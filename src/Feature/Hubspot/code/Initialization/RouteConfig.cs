namespace Sitecore.Feature.Hubspot.Initialization
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "Hubspot",
                url: "api/{controller}/{action}",
                defaults: new { controller = "Hubspot", action = "SaveForm" }
            );
        }
    }
}
