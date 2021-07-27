namespace Sitecore.Feature.Hubspot.Controllers
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.Text;
  using System.Threading;
  using System.Web;
  using System.Web.Mvc;
  using Sitecore.Feature.Hubspot.Models;
  using Newtonsoft.Json.Linq;
  using Sitecore.Analytics;
  using Sitecore.Analytics.Tracking;
  using System.Collections.Specialized;
  using Sitecore.Configuration;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Links;
  using Sitecore.Mvc.Controllers;
  using Sitecore.Mvc.Presentation;
  using Sitecore.XConnect;
  using Sitecore.XConnect.Client;
  using Sitecore.XConnect.Collection.Model;

  public class HubspotController : SitecoreController
  {

    protected static readonly string ConfigItemId = "Hubspot.ConfigItemId";

    public ActionResult Form()
    {
      var dataSourceId = RenderingContext.CurrentOrNull.Rendering.DataSource;
      var dataSource = Sitecore.Context.Database.Items.GetItem(dataSourceId);
      if (dataSource == null)
      {
        return View();
      }

      var hubspotFormSettings = GetHubspotSettingsModel(dataSource);
      return View(hubspotFormSettings);

    }

    public ActionResult EmbeddedForm()
    {
      var dataSourceId = RenderingContext.CurrentOrNull.Rendering.DataSource;
      var dataSource = Sitecore.Context.Database.Items.GetItem(dataSourceId);
      if (dataSource == null)
      {
        return View();
      }

      var hubspotFormSettings = GetHubspotSettingsModel(dataSource);
      return View(hubspotFormSettings);

    }

    public ActionResult GatedContentForm()
    {
      var dataSourceId = RenderingContext.CurrentOrNull.Rendering.DataSource;
      var dataSource = Sitecore.Context.Database.Items.GetItem(dataSourceId);
      if (dataSource == null)
      {
        return View();
      }
      var hubspotFormSettings = GetHubspotSettingsModel(dataSource);
      return View(hubspotFormSettings);
    }

    [HttpPost]
    public void SaveForm(string formBody)
    {
      try
      {
        if (!string.IsNullOrEmpty(formBody))
        {
          // form body sent by hubspot on the form submit
          var parsedFormFields = HttpUtility.ParseQueryString(formBody);
          if (parsedFormFields != null)
          {
            // email field from hubspot form
            var email = parsedFormFields["email"];
            if (!string.IsNullOrEmpty(email))
            {
              email = WebUtility.HtmlDecode(email);
              var hubspotConfigItemId = Settings.GetSetting(ConfigItemId);
              if (string.IsNullOrEmpty(hubspotConfigItemId))
              {
                hubspotConfigItemId = "";
              }

              var hubspotConfigItem = Sitecore.Context.Database.GetItem(hubspotConfigItemId);
              // api url for hubspot
              var apiUrl = hubspotConfigItem[Templates.HubspotFormConfig.Fields.ApiUrl];
              // rest api path on hubspot for getting contact by email
              var contactsByEmailPath = hubspotConfigItem[Templates.HubspotFormConfig.Fields.ContactsByEmailPath];
              // apikey for the hubspot form
              var apiKey = hubspotConfigItem[Templates.HubspotFormConfig.Fields.ApiKey];
              // primary leadScores - score names which it should be equal to hubspot scores
              var leadScores = hubspotConfigItem[Templates.HubspotFormConfig.Fields.LeadScores];

              if (!string.IsNullOrEmpty(apiUrl) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(leadScores))
              {
                // URL generation for api call to get contact by email
                var requestUrl = apiUrl + contactsByEmailPath + email + "&hapikey=" + apiKey + "&propertyMode=value_only";
                // generating the url to restrict to get contact with scores/leadScores and not the entire object
                NameValueCollection parsedLeadScoresList = Sitecore.Web.WebUtil.ParseUrlParameters(leadScores);
                string[] leadScoresList = null;
                if (parsedLeadScoresList != null)
                {
                  leadScoresList = parsedLeadScoresList.AllKeys;
                }

                requestUrl += "&";
                foreach (var leadScore in leadScoresList)
                {
                  if (leadScore != null)
                  {
                    requestUrl += "property=" + leadScore + "&";
                  }
                }

                requestUrl = requestUrl.TrimEnd('&');
                var maxScoreValue = int.MinValue;
                var maxScoreName = string.Empty;
                var successResponse = false;
                var attempt = 0;
                while (!successResponse && attempt < 10)
                {
                  Thread.Sleep(500);
                  attempt += 1;
                  WebRequest getUserByEmailRequest = WebRequest.Create(requestUrl);
                  getUserByEmailRequest.Method = "GET";
                  getUserByEmailRequest.ContentType = "application/json";
                  using (HttpWebResponse response = (HttpWebResponse)getUserByEmailRequest.GetResponse())
                  using (Stream stream = response.GetResponseStream())
                  using (StreamReader reader = new StreamReader(stream))
                  {
                    var contact = reader.ReadToEnd();
                    try
                    {
                      // parsing the returned contact JSON object
                      var jobj = JObject.Parse(contact);
                      var properties = ((JObject)((JProperty)jobj.First).Value).Property("properties");
                      if (properties != null)
                      {
                        foreach (var leadScore in leadScoresList)
                        {
                          // checking which it's the biggest score/leadScore value to update the custom score facet in sitecore
                          if (properties.Value[leadScore] != null)
                          {
                            try
                            {
                              var leadScoreScoreValue = ((JProperty)properties.Value[leadScore].First).Value.Value<int>();
                              // checking the biggest leadScore/score value
                              if (leadScoreScoreValue > maxScoreValue)
                              {
                                maxScoreValue = leadScoreScoreValue;
                                maxScoreName = parsedLeadScoresList[leadScore];
                              }
                              successResponse = true;
                            }
                            catch (Exception ex)
                            {
                              Sitecore.Diagnostics.Log.Error("Error casting leadScore score value for " + leadScore + " attempt no. " + attempt.ToString() + " " + ex.ToString(), this);
                            }
                          }
                        }
                      }
                      else
                      {
                        Sitecore.Diagnostics.Log.Warn("Properties for contact it's empty, contact email: " + email + " attempt no. " + attempt.ToString(), this);
                      }
                    }
                    catch (Exception ex)
                    {
                      Sitecore.Diagnostics.Log.Error("Error casting properties for contact attempt no. " + attempt.ToString() + " " + ex.ToString(), this);
                    }
                  }
                }

                // if even after 5 sec there is no good response we wait 5 more sec
                if (!successResponse)
                {
                  Thread.Sleep(5000);
                  WebRequest getUserByEmailRequest = WebRequest.Create(requestUrl);
                  getUserByEmailRequest.Method = "GET";
                  getUserByEmailRequest.ContentType = "application/json";
                  using (HttpWebResponse response = (HttpWebResponse)getUserByEmailRequest.GetResponse())
                  using (Stream stream = response.GetResponseStream())
                  using (StreamReader reader = new StreamReader(stream))
                  {
                    var contact = reader.ReadToEnd();
                    try
                    {
                      // parsing the returned contact JSON object
                      var jobj = JObject.Parse(contact);
                      var properties = ((JObject)((JProperty)jobj.First).Value).Property("properties");
                      if (properties != null)
                      {
                        foreach (var leadScore in leadScoresList)
                        {
                          // checking which it's the biggest score/leadScore value to update the custom score facet in sitecore
                          if (properties.Value[leadScore] != null)
                          {
                            try
                            {
                              var leadScoreScoreValue = ((JProperty)properties.Value[leadScore].First).Value.Value<int>();
                              // checking the biggest leadScore/score value
                              if (leadScoreScoreValue > maxScoreValue)
                              {
                                maxScoreValue = leadScoreScoreValue;
                                maxScoreName = parsedLeadScoresList[leadScore];
                              }
                              successResponse = true;
                            }
                            catch (Exception ex)
                            {
                              Sitecore.Diagnostics.Log.Error("Error casting leadScore score value for " + leadScore + ex.ToString(), this);
                            }
                          }
                        }
                      }
                      else
                      {
                        Sitecore.Diagnostics.Log.Warn("Properties for contact it's empty, contact email: " + email, this);
                      }
                    }
                    catch (Exception ex)
                    {
                      Sitecore.Diagnostics.Log.Error("Error casting properties for contact " + ex.ToString(), this);
                    }
                  }
                }

                // save biggest leadScore/score custom facet in sitecore for the tracked contact

                using (XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
                {
                  var trackerIdentifier = new IdentifiedContactReference("hubspotFormUser", email.Trim('.', '_', '@'));

                  try
                  {
                    var contact = client.Get(trackerIdentifier, new ContactExpandOptions(ScoreFacet.DefaultFacetKey, ScoreNameFacet.DefaultFacetKey, PersonalInformation.DefaultFacetKey, EmailAddressList.DefaultFacetKey));

                    if (contact != null)
                    {
                      var manager = Factory.CreateObject("tracking/contactManager", true) as ContactManager;
                      Tracker.Current.Session.Contact = manager.LoadContact(Tracker.Current.Contact.ContactId);
                      var scoreFacet = contact.GetFacet<ScoreFacet>(ScoreFacet.DefaultFacetKey);
                      if (scoreFacet != null)
                      {
                        scoreFacet.HubspotScore = maxScoreValue;
                        client.SetFacet(contact, ScoreFacet.DefaultFacetKey, scoreFacet);
                      }
                      else
                      {
                        client.SetFacet(contact, ScoreFacet.DefaultFacetKey, new ScoreFacet()
                        {
                          HubspotScore = maxScoreValue
                        });
                      }

                      var scoreNameFacet = contact.GetFacet<ScoreNameFacet>(ScoreNameFacet.DefaultFacetKey);
                      if (scoreNameFacet != null)
                      {
                        scoreNameFacet.HubspotScoreName = maxScoreName;
                        client.SetFacet(contact, ScoreNameFacet.DefaultFacetKey, scoreNameFacet);
                      }
                      else
                      {
                        client.SetFacet(contact, ScoreNameFacet.DefaultFacetKey, new ScoreNameFacet()
                        {
                          HubspotScoreName = maxScoreName
                        });
                      }

                      var personalInfoFacet = contact.GetFacet<PersonalInformation>(PersonalInformation.DefaultFacetKey);
                      if (personalInfoFacet != null)
                      {
                        personalInfoFacet.FirstName = parsedFormFields[hubspotConfigItem[Templates.HubspotFormConfig.Fields.FirstNameFacetFieldName]];
                        personalInfoFacet.LastName = parsedFormFields[hubspotConfigItem[Templates.HubspotFormConfig.Fields.LastNameFacetFieldName]];
                        client.SetFacet(contact, PersonalInformation.DefaultFacetKey, personalInfoFacet);
                      }
                      else
                      {
                        client.SetFacet(contact, PersonalInformation.DefaultFacetKey, new PersonalInformation()
                        {
                          FirstName = parsedFormFields[hubspotConfigItem[Templates.HubspotFormConfig.Fields.FirstNameFacetFieldName]],
                          LastName = parsedFormFields[hubspotConfigItem[Templates.HubspotFormConfig.Fields.LastNameFacetFieldName]]
                        });
                      }

                      var emailFacet = contact.GetFacet<EmailAddressList>(EmailAddressList.DefaultFacetKey);
                      if (emailFacet != null)
                      {
                        emailFacet.PreferredEmail = new EmailAddress(email, false);
                        client.SetFacet(contact, EmailAddressList.DefaultFacetKey, emailFacet);
                      }
                      else
                      {
                        client.SetFacet(contact, EmailAddressList.DefaultFacetKey, new EmailAddressList(new EmailAddress(email, false), "Preffered"));
                      }

                      client.Submit();
                      Sitecore.Analytics.Tracker.Current.Session.IdentifyAs("hubspotFormUser", email.Trim('.', '_', '@'));
                      manager.RemoveFromSession(Sitecore.Analytics.Tracker.Current.Contact.ContactId);
                      manager.LoadContact(Sitecore.Analytics.Tracker.Current.Contact.ContactId);
                      Sitecore.Analytics.Tracker.Current.Session.IdentifyAs("hubspotFormUser", email.Trim('.', '_', '@'));
                    }
                    else
                    {
                      var newContact = new Sitecore.XConnect.Contact(new ContactIdentifier("hubspotFormUser", email.Trim('.', '_', '@'), ContactIdentifierType.Known));

                      client.AddContact(newContact);
                      client.SetFacet(newContact, ScoreFacet.DefaultFacetKey, new ScoreFacet()
                      {
                        HubspotScore = maxScoreValue
                      });
                      client.SetFacet(newContact, ScoreNameFacet.DefaultFacetKey, new ScoreNameFacet()
                      {
                        HubspotScoreName = maxScoreName
                      });
                      client.SetFacet(newContact, PersonalInformation.DefaultFacetKey, new PersonalInformation()
                      {
                        FirstName = parsedFormFields[hubspotConfigItem[Templates.HubspotFormConfig.Fields.FirstNameFacetFieldName]],
                        LastName = parsedFormFields[hubspotConfigItem[Templates.HubspotFormConfig.Fields.LastNameFacetFieldName]]
                      });
                      client.SetFacet(newContact, EmailAddressList.DefaultFacetKey, new EmailAddressList(new EmailAddress(email, false), "Preffered"));
                      client.Submit();
                      Sitecore.Analytics.Tracker.Current.Session.IdentifyAs("hubspotFormUser", email.Trim('.', '_', '@'));

                    }
                  }
                  catch (XdbExecutionException ex)
                  {
                    Log.Error("Error at saving facet to xdb: ", ex, this);
                  }
                }

              }
              else
              {
                Sitecore.Diagnostics.Log.Warn("Missing or wrong configuration value for Hubspot Api Url, Api Key and Primary leadScores!", this);
              }
            }
            else
            {
              Sitecore.Diagnostics.Log.Warn("Incorect or missing email sent by Hubspot on form body!", this);
            }
          }
          else
          {
            Sitecore.Diagnostics.Log.Warn("Incorect or missing form body and data sent by Hubspot!", this);
          }
        }
      }
      catch (Exception ex)
      {
        Sitecore.Diagnostics.Log.Error("Error during update of contact: ", ex, this);
      }
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult DownloadPDFDocument(string articleId)
    {
      var articleItem = Sitecore.Context.Database.GetItem(articleId);
      Sitecore.Data.Fields.FileField fileField = articleItem.Fields[Templates.ArticleBase.Fields.AttachmentFile];
      if (fileField != null && fileField.MediaItem != null)
      {
        var fileUrl = Sitecore.Resources.Media.MediaManager.GetMediaUrl(fileField.MediaItem);
        if (!string.IsNullOrEmpty(fileUrl))
        {
          return Content(Sitecore.Resources.Media.HashingUtils.ProtectAssetUrl(fileUrl));
        }
      }

      return Content(LinkManager.GetItemUrl(articleItem));
    }

    [HttpPost]
    public void UpdateContact()
    {
      try
      {
        using (Stream receiveStream = Request.InputStream)
        {
          using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
          {
            var contactToUpdateRequest = readStream.ReadToEnd();
            var jobj = JObject.Parse(contactToUpdateRequest);
            var properties = (JObject)jobj.Property("properties").Value;
            if (properties != null)
            {
              var hubspotConfigItemId = Settings.GetSetting(ConfigItemId);
              if (string.IsNullOrEmpty(hubspotConfigItemId))
              {
                hubspotConfigItemId = "";
              }

              var hubspotConfigItem = Sitecore.Context.Database.GetItem(hubspotConfigItemId);
              var emailFieldName = hubspotConfigItem[Templates.HubspotFormConfig.Fields.EmailFieldName];
              // get contact from sitecore database by email from request
              var userEmail = ((JProperty)properties[emailFieldName].First).Value.Value<string>();
              if (!string.IsNullOrEmpty(userEmail))
              {
                var contactToUpdate = this.GetContact(userEmail);
                if (contactToUpdate != null)
                {
                  var leadScores = hubspotConfigItem[Templates.HubspotFormConfig.Fields.LeadScores];
                  if (!string.IsNullOrEmpty(leadScores))
                  {
                    NameValueCollection parsedLeadScoresList = Sitecore.Web.WebUtil.ParseUrlParameters(leadScores);
                    string[] leadScoresList = null;
                    if (parsedLeadScoresList != null)
                    {
                      leadScoresList = parsedLeadScoresList.AllKeys;
                    }
                    var maxScoreValue = int.MinValue;
                    var maxScoreName = string.Empty;
                    using (Sitecore.XConnect.Client.XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
                    {
                      foreach (var leadScore in leadScoresList)
                      {
                        // checking which it's the biggest score/leadScore value to update the custom score facet in sitecore
                        if (properties[leadScore] != null)
                        {
                          try
                          {
                            var leadScoreScoreValue = ((JProperty)properties[leadScore].First).Value.Value<int>();
                            // checking the biggest leadScore/score value
                            if (leadScoreScoreValue > maxScoreValue)
                            {
                              maxScoreValue = leadScoreScoreValue;
                              maxScoreName = parsedLeadScoresList[leadScore];
                            }
                          }
                          catch (Exception ex)
                          {
                            Sitecore.Diagnostics.Log.Error("Error casting leadScore score value for " + leadScore + ex.ToString(), this);
                          }
                        }
                      }

                      var contactScoreFacet = contactToUpdate.GetFacet<ScoreFacet>(ScoreFacet.DefaultFacetKey);
                      if (contactScoreFacet != null)
                      {
                        contactScoreFacet.HubspotScore = maxScoreValue;
                        client.SetFacet(contactToUpdate, ScoreFacet.DefaultFacetKey, contactScoreFacet);
                      }
                      else
                      {
                        client.SetFacet(contactToUpdate, ScoreFacet.DefaultFacetKey, new ScoreFacet()
                        {
                          HubspotScore = maxScoreValue
                        });
                      }

                      var contactScoreNameFacet = contactToUpdate.GetFacet<ScoreNameFacet>(ScoreNameFacet.DefaultFacetKey);
                      if (contactScoreNameFacet != null)
                      {
                        contactScoreNameFacet.HubspotScoreName = maxScoreName;
                        client.SetFacet(contactToUpdate, ScoreNameFacet.DefaultFacetKey, contactScoreNameFacet);
                      }
                      else
                      {
                        client.SetFacet(contactToUpdate, ScoreNameFacet.DefaultFacetKey, new ScoreNameFacet()
                        {
                          HubspotScoreName = maxScoreName
                        });
                      }

                      // Submit operations as batch
                      client.Submit();
                    }
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Sitecore.Diagnostics.Log.Error("Error during update of contact: ", ex, this);
      }
    }

    [HttpGet]
    public JsonResult GetContactHubspotData(string contactId)
    {
      using (XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
      {
        try
        {
          Sitecore.XConnect.Contact contact = client.Get<Sitecore.XConnect.Contact>(new ContactReference(new Guid(contactId)),
           new ContactExpandOptions(ScoreFacet.DefaultFacetKey, ScoreNameFacet.DefaultFacetKey) { });

          if (contact != null)
          {
            // For each contact, retrieve the facet - will return null if contact does not have this facet set
            var facetValue = contact.GetFacet<ScoreFacet>(ScoreFacet.DefaultFacetKey);
            var facetName = contact.GetFacet<ScoreNameFacet>(ScoreNameFacet.DefaultFacetKey);

            if (facetValue != null && facetValue != null)
            {
              return Json(new Tuple<string, int>(facetName.HubspotScoreName, facetValue.HubspotScore), JsonRequestBehavior.AllowGet);
            }
          }
        }
        catch (XdbExecutionException ex)
        {
          // Handle exceptions
        }
      }
      return null;
    }

    public Sitecore.XConnect.Contact GetContact(string email)
    {
      try
      {
        using (Sitecore.XConnect.Client.XConnectClient client = Sitecore.XConnect.Client.Configuration.SitecoreXConnectClientConfiguration.GetClient())
        {
          try
          {
            if (IsContactInSession(email))
            {
              var reference = new Sitecore.XConnect.ContactReference(Tracker.Current.Session.Contact.ContactId);

              var contact = client.Get(reference, new Sitecore.XConnect.ContactExpandOptions() { });

              return contact;
            }

            // Retrieve contact
            Sitecore.XConnect.Contact existingContact = client.Get(new IdentifiedContactReference("hubspotFormUser", email), new Sitecore.XConnect.ContactExpandOptions(ScoreFacet.DefaultFacetKey, ScoreNameFacet.DefaultFacetKey, PersonalInformation.DefaultFacetKey, EmailAddressList.DefaultFacetKey));

            if (existingContact != null)
            {
              return existingContact;
            }

            return null;

          }
          catch (XdbExecutionException)
          {
            return null;
          }
        }
      }
      catch (Exception ex)
      {
        Sitecore.Diagnostics.Log.Error("Error updating contact score facet with the email:" + email, ex, this);
        return null;
      }

    }

    private bool IsContactInSession(string email)
    {
      var tracker = Tracker.Current;

      if (tracker != null &&
          tracker.IsActive &&
          tracker.Session != null &&
          tracker.Session.Contact != null
          &&
          tracker.Session.Contact.Identifiers != null &&
          tracker.Session.Contact.Identifiers.Any() &&
          tracker.Session.Contact.Identifiers.Any(x =>
                                      x.Identifier.Equals(email, StringComparison.InvariantCultureIgnoreCase)
                                      && x.Source.Equals("hubspotFormUser")))
      {
        return true;
      }

      return false;
    }

    private HubspotSettingsModel GetHubspotSettingsModel(Item dataSource)
    {
      var hubspotConfigItemId = Settings.GetSetting(ConfigItemId);
      if (string.IsNullOrEmpty(hubspotConfigItemId))
      {
        hubspotConfigItemId = "";
      }

      var hubspotConfigItem = Sitecore.Context.Database.GetItem(hubspotConfigItemId);
      var portalId = (!string.IsNullOrWhiteSpace(dataSource[Templates.HubspotFormSettings.Fields.HubspotPortalId]) 
                        ? dataSource[Templates.HubspotFormSettings.Fields.HubspotPortalId]
                        : hubspotConfigItem[Templates.HubspotFormConfig.Fields.PortalId]);

      var hubspotFormSettings = new HubspotSettingsModel
      {
        Rendering = RenderingContext.CurrentOrNull.Rendering,
        HubspotPortalId = portalId,
        HubspotFormId = dataSource[Templates.HubspotFormSettings.Fields.HubspotFormId],
        ContextItemId = Sitecore.Context.Item.ID.ToString()
      };

      return hubspotFormSettings;

    }
  }
}
