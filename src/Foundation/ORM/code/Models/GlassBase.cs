namespace Sitecore.Foundation.ORM.Models
{
    using System;
    using System.Collections.Generic;

    using Glass.Mapper.Sc.Configuration;
    using Glass.Mapper.Sc.Configuration.Attributes;
    using Sitecore.Globalization;

    public abstract partial class GlassBase : IGlassBase
    {
        [SitecoreId]
        public virtual Guid Id { get; set; }

        [SitecoreInfo(SitecoreInfoType.Language)]
        public virtual Language Language { get; private set; }

        [SitecoreInfo(SitecoreInfoType.Version)]
        public virtual int Version { get; set; }

        [SitecoreInfo(SitecoreInfoType.BaseTemplateIds)]
        public virtual IEnumerable<Guid> BaseTemplateIds { get; private set; }

        [SitecoreInfo(SitecoreInfoType.TemplateName)]
        public virtual string TemplateName { get; private set; }

        [SitecoreInfo(SitecoreInfoType.TemplateId)]
        public virtual Guid TemplateId { get; set; }

        [SitecoreInfo(SitecoreInfoType.Name)]
        public virtual string Name { get; set; }

        [SitecoreInfo(SitecoreInfoType.Url)]
        public virtual string Url { get; private set; }

        [SitecoreInfo(SitecoreInfoType.FullPath)]
        public virtual string FullPath { get; private set; }
    }
}
