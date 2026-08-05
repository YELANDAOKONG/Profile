using System.Collections.ObjectModel;

using Profile.Domain.Content.Comments.Value;
using Profile.Domain.Content.Value;

namespace Profile.Domain.Content.Comments;

internal static class CommentDomainRules
{
    public const int MaximumBodyLength = 4_096;
    public const int MaximumMediaCount = 4;

    public static ReadOnlyCollection<MediaReference> CopyMedia(
        IEnumerable<MediaReference> media)
    {
        ArgumentNullException.ThrowIfNull(media);

        var items = media.ToArray();

        if (items.Length > MaximumMediaCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(media),
                items.Length,
                $"Comment media count cannot exceed {MaximumMediaCount}.");
        }

        if (items.Any(static item => item is null))
        {
            throw new ArgumentException(
                "Comment media cannot contain a null item.",
                nameof(media));
        }

        if (items.Select(static item => item.MediaId).Distinct().Count() !=
            items.Length)
        {
            throw new ArgumentException(
                "Comment media cannot contain duplicate media identities.",
                nameof(media));
        }

        return Array.AsReadOnly(items);
    }

    public static void ValidateState(
        ContentBody? body,
        IReadOnlyCollection<MediaReference> media,
        CommentStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "Comment status is not supported.");
        }

        if (status is CommentStatus.Deleted)
        {
            if (body is not null || media.Count != 0)
            {
                throw new ArgumentException(
                    "A deleted comment can retain only placeholder data.",
                    nameof(status));
            }

            return;
        }

        ArgumentNullException.ThrowIfNull(body);

        if (string.IsNullOrWhiteSpace(body.Source))
        {
            throw new ArgumentException(
                "Comment body cannot be empty or whitespace.",
                nameof(body));
        }

        if (body.Source.Length > MaximumBodyLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(body),
                body.Source.Length,
                $"Comment body length cannot exceed {MaximumBodyLength} characters.");
        }
    }

    public static void EnsureCanReplyTo(CommentStatus parentStatus)
    {
        if (parentStatus is not (CommentStatus.Approved or CommentStatus.Deleted))
        {
            throw new InvalidOperationException(
                "A comment can reply only to an approved or deleted parent comment.");
        }
    }

    public static CommentStatus Approve(CommentStatus status) => status switch
    {
        CommentStatus.Pending or CommentStatus.Spam => CommentStatus.Approved,
        _ => throw new InvalidOperationException(
            "Only a pending or spam comment can be approved.")
    };

    public static CommentStatus MarkAsSpam(CommentStatus status) => status switch
    {
        CommentStatus.Pending or CommentStatus.Approved => CommentStatus.Spam,
        _ => throw new InvalidOperationException(
            "Only a pending or approved comment can be marked as spam.")
    };

    public static CommentStatus Delete(CommentStatus status) => status switch
    {
        CommentStatus.Pending or CommentStatus.Approved or CommentStatus.Spam =>
            CommentStatus.Deleted,
        _ => throw new InvalidOperationException(
            "A deleted comment cannot be deleted again.")
    };
}
