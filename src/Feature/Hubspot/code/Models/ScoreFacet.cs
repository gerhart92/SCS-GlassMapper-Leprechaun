namespace Sitecore.Feature.Hubspot.Models
{
    using Sitecore.XConnect;
    using System;

    [FacetKey(DefaultFacetKey)]
    [Serializable]
    public class ScoreFacet : Facet
    {
        public const string DefaultFacetKey = "HubspotScore";
        public ScoreFacet() { }
        public int HubspotScore { get; set; }
    }
}
