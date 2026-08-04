namespace Profile.Domain.Content.Pages.Value;

public sealed record PagePublication
{
    private PagePublication(
        PagePublicationStatus status,
        DateTimeOffset? firstPublishedAt,
        DateTimeOffset? lastPublishedAt)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Page publication status is not supported.");
        }

        if (status is PagePublicationStatus.Published &&
            (firstPublishedAt is null || lastPublishedAt is null))
        {
            throw new ArgumentException(
                "A published page must have first and last publish times.",
                nameof(firstPublishedAt));
        }

        if ((firstPublishedAt is null) != (lastPublishedAt is null))
        {
            throw new ArgumentException(
                "First and last publish times must be set together.",
                nameof(lastPublishedAt));
        }

        if (firstPublishedAt > lastPublishedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lastPublishedAt),
                lastPublishedAt,
                "Last publish time cannot be earlier than the first publish time.");
        }

        Status = status;
        FirstPublishedAt = firstPublishedAt;
        LastPublishedAt = lastPublishedAt;
    }

    public PagePublicationStatus Status { get; }

    public DateTimeOffset? FirstPublishedAt { get; }

    public DateTimeOffset? LastPublishedAt { get; }

    public static PagePublication CreateDraft() =>
        new(PagePublicationStatus.Draft, null, null);

    public static PagePublication Reconstitute(
        PagePublicationStatus status,
        DateTimeOffset? firstPublishedAt,
        DateTimeOffset? lastPublishedAt) =>
        new(status, firstPublishedAt, lastPublishedAt);

    public PagePublication Publish(DateTimeOffset publishedAt)
    {
        EnsureTransition(
            PagePublicationStatus.Draft,
            PagePublicationStatus.Published);

        if (publishedAt < LastPublishedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(publishedAt),
                publishedAt,
                "Publish time cannot be earlier than the previous publish time.");
        }

        return new PagePublication(
            PagePublicationStatus.Published,
            FirstPublishedAt ?? publishedAt,
            publishedAt);
    }

    public PagePublication Unpublish()
    {
        EnsureTransition(
            PagePublicationStatus.Published,
            PagePublicationStatus.Draft);

        return new PagePublication(
            PagePublicationStatus.Draft,
            FirstPublishedAt,
            LastPublishedAt);
    }

    private void EnsureTransition(
        PagePublicationStatus from,
        PagePublicationStatus to)
    {
        if (Status != from)
        {
            throw new InvalidOperationException(
                $"Page publication cannot transition from {Status} to {to}.");
        }
    }
}
