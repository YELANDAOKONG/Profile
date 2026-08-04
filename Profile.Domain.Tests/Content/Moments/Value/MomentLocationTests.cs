using Profile.Domain.Content.Moments.Value;

namespace Profile.Domain.Tests.Content.Moments.Value;

public sealed class MomentLocationTests
{
    [Theory]
    [InlineData(-90, -180)]
    [InlineData(0, 0)]
    [InlineData(90, 180)]
    public void Constructor_WithValidCoordinates_PreservesValues(
        int latitude,
        int longitude)
    {
        var location = new MomentLocation(latitude, longitude, "Taipei");

        Assert.Equal(latitude, location.Latitude);
        Assert.Equal(longitude, location.Longitude);
        Assert.Equal("Taipei", location.PlaceName);
    }

    [Theory]
    [InlineData(-90.0001)]
    [InlineData(90.0001)]
    public void Constructor_WithLatitudeOutsideRange_ThrowsArgumentOutOfRangeException(
        double latitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MomentLocation((decimal)latitude, 0));
    }

    [Theory]
    [InlineData(-180.0001)]
    [InlineData(180.0001)]
    public void Constructor_WithLongitudeOutsideRange_ThrowsArgumentOutOfRangeException(
        double longitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MomentLocation(0, (decimal)longitude));
    }

    [Fact]
    public void Constructor_WithoutPlaceName_AllowsValue()
    {
        var location = new MomentLocation(25.033m, 121.5654m);

        Assert.Null(location.PlaceName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" Taipei")]
    [InlineData("Taipei ")]
    public void Constructor_WithInvalidPlaceName_ThrowsArgumentException(
        string placeName)
    {
        Assert.Throws<ArgumentException>(
            () => new MomentLocation(25.033m, 121.5654m, placeName));
    }

    [Fact]
    public void Constructor_WithMaximumPlaceNameLength_AllowsValue()
    {
        var placeName = new string('a', MomentLocation.MaximumPlaceNameLength);

        var location = new MomentLocation(25.033m, 121.5654m, placeName);

        Assert.Equal(placeName, location.PlaceName);
    }

    [Fact]
    public void Constructor_WithPlaceNameOverMaximumLength_ThrowsArgumentOutOfRangeException()
    {
        var placeName = new string(
            'a',
            MomentLocation.MaximumPlaceNameLength + 1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MomentLocation(25.033m, 121.5654m, placeName));
    }
}
