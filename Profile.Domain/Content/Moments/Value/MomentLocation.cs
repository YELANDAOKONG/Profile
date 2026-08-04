namespace Profile.Domain.Content.Moments.Value;

public sealed record MomentLocation
{
    public const decimal MinimumLatitude = -90m;
    public const decimal MaximumLatitude = 90m;
    public const decimal MinimumLongitude = -180m;
    public const decimal MaximumLongitude = 180m;
    public const int MaximumPlaceNameLength = 128;

    public MomentLocation(
        decimal latitude,
        decimal longitude,
        string? placeName = null)
    {
        if (latitude is < MinimumLatitude or > MaximumLatitude)
        {
            throw new ArgumentOutOfRangeException(
                nameof(latitude),
                latitude,
                $"Latitude must be between {MinimumLatitude} and {MaximumLatitude}.");
        }

        if (longitude is < MinimumLongitude or > MaximumLongitude)
        {
            throw new ArgumentOutOfRangeException(
                nameof(longitude),
                longitude,
                $"Longitude must be between {MinimumLongitude} and {MaximumLongitude}.");
        }

        if (placeName is not null && string.IsNullOrWhiteSpace(placeName))
        {
            throw new ArgumentException(
                "Moment place name cannot be empty or whitespace.",
                nameof(placeName));
        }

        if (placeName is not null &&
            (char.IsWhiteSpace(placeName[0]) || char.IsWhiteSpace(placeName[^1])))
        {
            throw new ArgumentException(
                "Moment place name cannot contain surrounding whitespace.",
                nameof(placeName));
        }

        if (placeName?.Length > MaximumPlaceNameLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(placeName),
                placeName.Length,
                $"Moment place name length cannot exceed {MaximumPlaceNameLength} characters.");
        }

        Latitude = latitude;
        Longitude = longitude;
        PlaceName = placeName;
    }

    public decimal Latitude { get; }

    public decimal Longitude { get; }

    public string? PlaceName { get; }
}
