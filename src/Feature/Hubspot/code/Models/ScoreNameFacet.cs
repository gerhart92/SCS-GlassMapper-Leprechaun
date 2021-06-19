namespace Sitecore.Feature.Hubspot.Models
{
    using Sitecore.XConnect;
    using System;

    [FacetKey(DefaultFacetKey)]
    [Serializable]
    public class ScoreNameFacet : Facet
    {
        public const string DefaultFacetKey = "HubspotScoreName";
        public ScoreNameFacet() { }
        public string HubspotScoreName { get; set; }
    }
}
