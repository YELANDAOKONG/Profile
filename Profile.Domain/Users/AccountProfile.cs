using Profile.Domain.Content.Value;
using Profile.Domain.Users.Value;

namespace Profile.Domain.Users;

public sealed class AccountProfile
{
    public const int MaximumNicknameLength = 64;
    public const int MaximumBioLength = 2_048;
    public const int MaximumLocationLength = 128;

    public AccountProfile(
        UserIdentity ownerId,
        string nickname,
        MediaReference? avatar,
        string? bio,
        string? location,
        PersonalLink? personalLink,
        MediaReference? banner)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        ArgumentNullException.ThrowIfNull(nickname);

        ValidateMaximumLength(
            nickname,
            MaximumNicknameLength,
            nameof(nickname),
            "Nickname");
        ValidateOptionalMaximumLength(
            bio,
            MaximumBioLength,
            nameof(bio),
            "Bio");
        ValidateOptionalMaximumLength(
            location,
            MaximumLocationLength,
            nameof(location),
            "Location");

        OwnerId = ownerId;
        Nickname = nickname;
        Avatar = avatar;
        Bio = bio;
        Location = location;
        PersonalLink = personalLink;
        Banner = banner;
    }

    public UserIdentity OwnerId { get; }

    public string Nickname { get; private set; }

    public MediaReference? Avatar { get; private set; }

    public string? Bio { get; private set; }

    public string? Location { get; private set; }

    public PersonalLink? PersonalLink { get; private set; }

    public MediaReference? Banner { get; private set; }

    public void Update(
        string nickname,
        MediaReference? avatar,
        string? bio,
        string? location,
        PersonalLink? personalLink,
        MediaReference? banner)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        ValidateMaximumLength(
            nickname,
            MaximumNicknameLength,
            nameof(nickname),
            "Nickname");
        ValidateOptionalMaximumLength(
            bio,
            MaximumBioLength,
            nameof(bio),
            "Bio");
        ValidateOptionalMaximumLength(
            location,
            MaximumLocationLength,
            nameof(location),
            "Location");

        Nickname = nickname;
        Avatar = avatar;
        Bio = bio;
        Location = location;
        PersonalLink = personalLink;
        Banner = banner;
    }

    private static void ValidateOptionalMaximumLength(
        string? value,
        int maximumLength,
        string parameterName,
        string displayName)
    {
        if (value is not null)
        {
            ValidateMaximumLength(value, maximumLength, parameterName, displayName);
        }
    }

    private static void ValidateMaximumLength(
        string value,
        int maximumLength,
        string parameterName,
        string displayName)
    {
        if (value.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value.Length,
                $"{displayName} length cannot exceed {maximumLength} characters.");
        }
    }
}
