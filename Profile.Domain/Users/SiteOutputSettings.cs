namespace Profile.Domain.Users;

public sealed record SiteOutputSettings(
    bool RssAndAtomEnabled,
    bool SitemapEnabled,
    bool ArchivePagesEnabled,
    bool SearchEnabled);
