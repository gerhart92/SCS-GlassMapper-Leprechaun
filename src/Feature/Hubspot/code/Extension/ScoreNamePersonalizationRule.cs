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
  using System.Text.RegularExpressions;

  public class ScoreNamePersonalizationRule<T> : StringOperatorCondition<T> where T : RuleContext
  {
    protected static readonly string ConfigItemId = "Hubspot.ConfigItemId";
    private const string FacetName = "HubspotScoreName";
    private const string FacetValueName = "HubspotScore";
    public string Value { get; set; }

    protected override bool Execute(T ruleContext)
    {
      Assert.ArgumentNotNull((object)ruleContext, "ruleContext");

      var conditionOperator = base.GetOperator();

      using (XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
      {
        var trackerIdentifier = new IdentifiedContactReference(Sitecore.Analytics.XConnect.DataAccess.Constants.IdentifierSource, Tracker.Current.Session.Contact.ContactId.ToString("N"));

        try
        {
          var contact = client.Get<Contact>(trackerIdentifier, new ContactExpandOptions(ScoreNameFacet.DefaultFacetKey, ScoreFacet.DefaultFacetKey));

          if (contact == null)
          {
            Log.Info(this.GetType() + ": contact is null", this);
            return false;
          }

          var facet = contact.GetFacet<ScoreFacet>(FacetValueName);

          if (facet != null)
          {
            var hubspotConfigItemId = Sitecore.Configuration.Settings.GetSetting(ConfigItemId);
            if (string.IsNullOrEmpty(hubspotConfigItemId))
            {
              hubspotConfigItemId = "";
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
          }
          var namefacet = contact.GetFacet<ScoreNameFacet>(FacetName);

          if (namefacet == null)
          {
            Log.Info(string.Format("{0} : cannot find facet {1}", this.GetType(), FacetName), this);
            return false;
          }

          if (!string.IsNullOrEmpty(namefacet.HubspotScoreName) && !string.IsNullOrEmpty(Value))
          {
            return StringCompare(namefacet.HubspotScoreName, Value, conditionOperator);
          }
          else if (string.IsNullOrEmpty(namefacet.HubspotScoreName) && string.IsNullOrEmpty(Value))
          {
            return true;
          }
          else
          {
            return false;
          }
        }
        catch (Exception ex)
        {
          Log.Error("Error at score name personalization rule validation " + ex.ToString(), this);
          return false;
        }
      }
    }

    private bool StringCompare(string value1, string value2, StringConditionOperator conditionOperator)
    {
      switch (conditionOperator)
      {
        case StringConditionOperator.Equals:
          return value1.Equals(value2);
        case StringConditionOperator.CaseInsensitivelyEquals:
          return value1.Equals(value2, StringComparison.InvariantCulture);
        case StringConditionOperator.NotEqual:
          return !value1.Equals(value2);
        case StringConditionOperator.NotCaseInsensitivelyEquals:
          return value1.Equals(value2, StringComparison.InvariantCultureIgnoreCase);
        case StringConditionOperator.Contains:
          return value1.ToLower().Contains(value2.ToLower());
        case StringConditionOperator.StartsWith:
          return value1.ToLower().StartsWith(value2.ToLower());
        case StringConditionOperator.EndsWith:
          return value1.ToLower().EndsWith(value2.ToLower());
        case StringConditionOperator.MatchesRegularExpression:
          return Regex.IsMatch(value1, value2);
        case StringConditionOperator.Unknown:
          return false;
        default:
          return false;
      }
    }
  }
}
