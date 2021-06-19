namespace Sitecore.Feature.Hubspot.Extension
{
    using Sitecore.Feature.Hubspot.Models;
    using Sitecore.Analytics;
    using Sitecore.Diagnostics;
    using Sitecore.Rules;
    using Sitecore.Rules.Conditions;
    using Sitecore.XConnect;
    using Sitecore.XConnect.Client;
    using System;

    public class ScorePersonalizationRule<T> : OperatorCondition<T> where T : RuleContext
  {
    protected static readonly string ConfigItemId = "Hubspot.ConfigItemId";
    private const string FacetName = "HubspotScore";
    public int Value { get; set; }

    protected override bool Execute(T ruleContext)
    {
      Assert.ArgumentNotNull((object)ruleContext, "ruleContext");

      ConditionOperator conditionOperator = base.GetOperator();

      // TODO same as score name
      using (XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
      {
        var trackerIdentifier = new IdentifiedContactReference(Sitecore.Analytics.XConnect.DataAccess.Constants.IdentifierSource, Tracker.Current.Session.Contact.ContactId.ToString("N"));

        try
        {
          var contact = client.Get<Contact>(trackerIdentifier, new ContactExpandOptions(ScoreFacet.DefaultFacetKey));

          if (contact == null)
          {
            Log.Info(this.GetType() + ": contact is null", this);
            return false;
          }
          var facet = contact.GetFacet<ScoreFacet>(FacetName);

          if (facet == null)
          {
            Log.Info(string.Format("{0} : cannot find facet {1}", this.GetType(), FacetName), this);
            return false;
          }

          var hubspotConfigItemId = Sitecore.Configuration.Settings.GetSetting(ConfigItemId);
          if (string.IsNullOrEmpty(hubspotConfigItemId))
          {
            hubspotConfigItemId = "{1F0F4461-60CB-4132-94DB-5C93E185FDA8}";
          }

          var hubspotConfigItem = Sitecore.Context.Database.GetItem(hubspotConfigItemId);

          if (!string.IsNullOrEmpty(hubspotConfigItem[Templates.HubspotFormConfig.Fields.ScoringThreshold]))
          {
            int.TryParse(hubspotConfigItem[Templates.HubspotFormConfig.Fields.ScoringThreshold], out int threshold);
            if (threshold > facet.HubspotScore)
            {
              return false;
            }
          }

          return IntegerCompare(facet.HubspotScore, Value, conditionOperator);
        }
        catch (Exception ex)
        {
          Log.Error("Error at score name personalization rule validation " + ex.ToString(), this);
          return false;
        }
      }
    }

    private bool IntegerCompare(int value1, int value2, ConditionOperator conditionOperator)
    {
      switch (conditionOperator)
      {
        case ConditionOperator.Equal:
          return value1.Equals(value2);
        case ConditionOperator.LessThan:
          return value1 < value2;
        case ConditionOperator.LessThanOrEqual:
          return value1 <= value2;
        case ConditionOperator.GreaterThan:
          return value1 > value2;
        case ConditionOperator.GreaterThanOrEqual:
          return value1 >= value2;
        case ConditionOperator.NotEqual:
          return !value1.Equals(value2);
        default:
          return false;
      }
    }
  }
}
