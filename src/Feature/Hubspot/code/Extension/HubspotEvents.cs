namespace Sitecore.Feature.Hubspot.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Feature.Hubspot.Models;

    public static class HubspotEvents
    {
        public static List<string> GetEvents(this Item item, EventTypes eventType)
        {
            var eventsAdded = new List<string>();
            var eventTypeId = Templates.Topics.Fields.PageLoadEventId;
            switch (eventType)
            {
                case EventTypes.CTA:
                    eventTypeId = Templates.Topics.Fields.CTAEventId;
                    break;
                case EventTypes.PdfDownload:
                    eventTypeId = Templates.Topics.Fields.DownloadPdfEventId;
                    break;
                case EventTypes.SocialShare:
                    eventTypeId = Templates.Topics.Fields.SocialIconsCTAEventId;
                    break;
                default:
                    eventTypeId = Templates.Topics.Fields.PageLoadEventId;
                    break;
            }
            if (item != null && !string.IsNullOrEmpty(item[Templates.Page.Fields.Topics]))
            {
                var topics = item.GetMultiListValueItems(Templates.Page.Fields.Topics);
                foreach (var topic in topics)
                {
                    if (topic != null && !string.IsNullOrEmpty(topic[Templates.Topic.Fields.Categories]))
                    {
                        var topicCategories = topic.GetMultiListValueItems(Templates.Topic.Fields.Categories);
                        foreach (var topicCategory in topicCategories)
                        {
                            if (topicCategory != null
                               && !string.IsNullOrEmpty(topicCategory[eventTypeId])
                               && !eventsAdded.Contains(topicCategory[eventTypeId]))
                            {
                                eventsAdded.Add(topicCategory[eventTypeId]);
                            }
                        }
                    }
                }
            }

            return eventsAdded;
        }

        public static IEnumerable<Item> GetMultiListValueItems(this Item item, ID fieldId)
        {
            return new MultilistField(item.Fields[fieldId]).GetItems();
        }
    }
}
