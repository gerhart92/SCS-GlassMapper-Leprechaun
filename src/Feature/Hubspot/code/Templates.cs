namespace Sitecore.Feature.Hubspot
{
    using Sitecore.Data;
    public static class Templates
    {
        public static class HubspotFormSettings
        {
            public const string Id = "{1DCB9D03-B54F-4BE3-8431-E30209A0C404}";

            public static class Fields
            {
                public const string HubspotFormId = "{0E972407-055B-4A56-B4ED-7CBC37C988CC}";
                public const string HubspotPortalId = "{A96FA9C4-4F5E-46DB-83F8-44304CD6BA8F}";
            }
        }

        public static class HubspotFormConfig
        {
            public struct Fields
            {
                public static readonly ID ApiKey = new ID("{9E220B7F-2D7F-4B2A-A1FB-EA545E7B9564}");
                public static readonly ID PortalId = new ID("{B031D358-EE7D-4C1D-92E9-806B1884D5B5}");
                public static readonly ID ApiUrl = new ID("{8CB9EF53-282C-427B-8E14-D1F79F3A363E}");
                public static readonly ID ScoringThreshold = new ID("{8BBF74AB-D312-4F49-9844-E5FDFAB2C935}");
                public static readonly ID LeadScores = new ID("{C73ED51F-88BE-404C-A7BB-CC619CA4955B}");
                public static readonly ID ContactsByEmailPath = new ID("{09FB0A51-FD4E-43F5-82EB-ADE800D43819}");
                public static readonly ID EmailFieldName = new ID("{97401B8C-E6CB-4BB8-8535-7E30F3F69D1C}");
                public static readonly ID FirstNameFacetFieldName = new ID("{FF121F8A-85C6-4822-A7C4-1004850EABAF}");
                public static readonly ID LastNameFacetFieldName = new ID("{EAE9BB1C-20B0-422A-8486-2923AE703E33}");
            }
        }

        public static class Identity
        {
            public static readonly ID ID = new ID("{6742E0E8-1550-4F09-9D99-0AD9C9BC75A6}");
            public static readonly ID ItemId = new ID("{DEF845B2-AEEC-47E0-B98F-EEC1A8A48A58}");
        }

        public struct ArticleBase
        {
            public struct Fields
            {
                public static readonly ID AttachmentFile = new ID("{68570415-5371-4E77-8FBA-314B26FAE40E}");

            }
        }

        public struct Topics
        {
            public struct Fields 
            {
                public static readonly ID CTA = new ID("");
                public static readonly ID PageLoadEventId = new ID("");
                public static readonly ID CTAEventId = new ID("");
                public static readonly ID DownloadPdfEventId = new ID("");
                public static readonly ID SocialIconsCTAEventId = new ID("");
                
            }
        }

        public struct Page
        {
            public struct Fields
            {
                public static readonly ID Topics = new ID("");
            }
        }

        public struct Topic
        {
            public struct Fields
            {
                public static readonly ID Categories = new ID("");

            }
        }
    }
}
