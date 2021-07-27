namespace Sitecore.Feature.Hubspot.Models
{
    using Sitecore.Mvc.Presentation;

    public class HubspotSettingsModel
    {
        public Rendering Rendering { get; set; }

        public string HubspotPortalId { get; set; }

        public string HubspotFormId { get; set; }

        public string HubspotFormApi { get; set; }

        public string ContextItemId { get; set; }
    }
}
