namespace Sitecore.Feature.Hubspot.Initialization
{
  using Sitecore.Feature.Hubspot.Initialization;
  using Sitecore.Pipelines;
    using System.Web.Routing;

    public class PipelineRegistration
    {
        public void Process(PipelineArgs args)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
