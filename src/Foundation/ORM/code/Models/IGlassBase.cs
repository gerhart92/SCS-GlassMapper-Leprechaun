namespace Sitecore.Foundation.ORM.Models
{
    using System;
    using System.Collections.Generic;
    using Sitecore.Globalization;

    public interface IGlassBase
    {
        Guid Id { get; set; }
        Language Language { get; }
        int Version { get; set; }
        IEnumerable<Guid> BaseTemplateIds { get; }
        string TemplateName { get; }
        Guid TemplateId { get; set; }
        string Name { get; set; }
        string Url { get; }
        string FullPath { get; }
    }
}
