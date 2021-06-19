namespace Sitecore.Feature.Hubspot.Models
{
  using Sitecore.XConnect;
  using Sitecore.XConnect.Collection.Model;
  using Sitecore.XConnect.Schema;

  // Deploy needed for facet model Models/HubspotScoreFacetModel, 1.0.json
  // to the following paths:
  // <Instance_Name>.xconnect\App_data\Models
  // <Instance_Name>.xconnect\App_data\jobs\continuous\IndexWorker\App_data\Models

  // Sitecore.Feature.Hubspot dll to the following paths near normal deploy:
  // <Instance_Name>.xconnect\App_data\jobs\continuous\AutomationEngine\
  // <Instance_Name>.xconnect\App_data\jobs\continuous\IndexWorker\
  // <Instance_Name>.xconnect\bin

  // For every facet adjustment the json model needs to be regenerated using Sitecore.XConnect.Serialization.XdbModelWriter
  public class HubspotScoreFacetModel
    {
        public static XdbModel Model { get; } = HubspotScoreFacetModel.BuilddCustomModel();
        private static XdbModel BuilddCustomModel()
        {
            XdbModelBuilder xdbModelBuilder = new XdbModelBuilder("HubspotScoreFacetModel", new XdbModelVersion(1, 0));
            xdbModelBuilder.ReferenceModel(CollectionModel.Model);
            xdbModelBuilder.DefineFacet<Contact, ScoreFacet>(ScoreFacet.DefaultFacetKey);
            xdbModelBuilder.DefineFacet<Contact, ScoreNameFacet>(ScoreNameFacet.DefaultFacetKey);
            return xdbModelBuilder.BuildModel();
        }
    }
}
