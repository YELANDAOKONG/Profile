namespace Profile.Domain.Content.Value;

// Implements the restricted transition matrix from DESIGN §6.4. Every
// transition returns a new value; the owning aggregate replaces its current
// Publication, matching how Account replaces AccountEmail.
public sealed record Publication
{
    private Publication(
        PublicationStatus status,
        DateTimeOffset? scheduledAt,
        DateTimeOffset? firstPublishedAt,
        DateTimeOffset? lastPublishedAt)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Publication status is not supported.");
        }

        if (status is PublicationStatus.Scheduled && scheduledAt is null)
        {
            throw new ArgumentException(
                "Scheduled content must have a scheduled publish time.",
                nameof(scheduledAt));
        }

        if (status is not PublicationStatus.Scheduled && scheduledAt is not null)
        {
            throw new ArgumentException(
                "Only scheduled content can have a scheduled publish time.",
                nameof(scheduledAt));
        }

        if (status is PublicationStatus.Published &&
            (firstPublishedAt is null || lastPublishedAt is null))
        {
            throw new ArgumentException(
                "Published content must have first and last publish times.",
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
        ScheduledAt = scheduledAt;
        FirstPublishedAt = firstPublishedAt;
        LastPublishedAt = lastPublishedAt;
    }

    public PublicationStatus Status { get; }

    public DateTimeOffset? ScheduledAt { get; }

    public DateTimeOffset? FirstPublishedAt { get; }

    public DateTimeOffset? LastPublishedAt { get; }

    public static Publication CreateDraft() =>
        new(PublicationStatus.Draft, null, null, null);

    public static Publication Reconstitute(
        PublicationStatus status,
        DateTimeOffset? scheduledAt,
        DateTimeOffset? firstPublishedAt,
        DateTimeOffset? lastPublishedAt) =>
        new(status, scheduledAt, firstPublishedAt, lastPublishedAt);

    public Publication Schedule(DateTimeOffset scheduledAt, DateTimeOffset now)
    {
        EnsureTransition(PublicationStatus.Draft, PublicationStatus.Scheduled);

        if (scheduledAt <= now)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scheduledAt),
                scheduledAt,
                "Scheduled publish time must be in the future.");
        }

        return new Publication(
            PublicationStatus.Scheduled,
            scheduledAt,
            FirstPublishedAt,
            LastPublishedAt);
    }

    public Publication Unschedule()
    {
        EnsureTransition(PublicationStatus.Scheduled, PublicationStatus.Draft);

        return new Publication(
            PublicationStatus.Draft,
            null,
            FirstPublishedAt,
            LastPublishedAt);
    }

    public Publication SubmitForReview()
    {
        EnsureTransition(PublicationStatus.Draft, PublicationStatus.PendingReview);

        return new Publication(
            PublicationStatus.PendingReview,
            null,
            FirstPublishedAt,
            LastPublishedAt);
    }

    public Publication Approve(DateTimeOffset publishedAt)
    {
        EnsureTransition(PublicationStatus.PendingReview, PublicationStatus.Published);

        return PublishAt(publishedAt);
    }

    public Publication PublishScheduled(DateTimeOffset publishedAt)
    {
        EnsureTransition(PublicationStatus.Scheduled, PublicationStatus.Published);

        if (publishedAt < ScheduledAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(publishedAt),
                publishedAt,
                "Scheduled content cannot be published before its scheduled time.");
        }

        return PublishAt(publishedAt);
    }

    public Publication ReturnToDraft()
    {
        EnsureTransition(PublicationStatus.PendingReview, PublicationStatus.Draft);

        return new Publication(
            PublicationStatus.Draft,
            null,
            FirstPublishedAt,
            LastPublishedAt);
    }

    public Publication Unpublish()
    {
        EnsureTransition(PublicationStatus.Published, PublicationStatus.Draft);

        // Publish history is retained because unpublishing must not erase it.
        // Discarding instead of saving as a draft is expressed through
        // ContentDeletion on the owning aggregate, not through a transition.
        return new Publication(
            PublicationStatus.Draft,
            null,
            FirstPublishedAt,
            LastPublishedAt);
    }

    private Publication PublishAt(DateTimeOffset publishedAt)
    {
        if (publishedAt < LastPublishedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(publishedAt),
                publishedAt,
                "Publish time cannot be earlier than the previous publish time.");
        }

        return new Publication(
            PublicationStatus.Published,
            null,
            FirstPublishedAt ?? publishedAt,
            publishedAt);
    }

    private void EnsureTransition(PublicationStatus from, PublicationStatus to)
    {
        if (Status != from)
        {
            throw new InvalidOperationException(
                $"Publication cannot transition from {Status} to {to}.");
        }
    }
}
