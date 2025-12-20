using FabricMappingService.Core.Converters;
using Xunit;

namespace FabricMappingService.Tests;

public class DefaultPropertyConverterTests
{
    [Fact]
    public void Convert_SameType_ShouldReturnSameValue()
    {
        // Arrange
        var converter = new DefaultPropertyConverter();
        var value = "test string";

        // Act
        var result = converter.Convert(value, typeof(string));

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Convert_IntToString_ShouldConvert()
    {
        // Arrange
        var converter = new DefaultPropertyConverter();
        var value = 123;

        // Act
        var result = converter.Convert(value, typeof(string));

        // Assert
        Assert.Equal("123", result);
    }

    [Fact]
    public void Convert_StringToInt_ShouldConvert()
    {
        // Arrange
        var converter = new DefaultPropertyConverter();
        var value = "456";

        // Act
        var result = converter.Convert(value, typeof(int));

        // Assert
        Assert.Equal(456, result);
    }

    [Fact]
    public void Convert_StringToBool_ShouldConvert()
    {
        // Arrange
        var converter = new DefaultPropertyConverter();
        var value = "true";

        // Act
        var result = converter.Convert(value, typeof(bool));

        // Assert
        Assert.True((bool)result!);
    }

    [Fact]
    public void Convert_StringToDecimal_ShouldConvert()
    {
        // Arrange
        var converter = new DefaultPropertyConverter();
        var value = "123.45";

        // Act
        var result = converter.Convert(value, typeof(decimal));

        // Assert
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void Convert_NullToValueType_ShouldReturnDefault()
    {
        // Arrange
        var converter = new DefaultPropertyConverter();

        // Act
        var result = converter.Convert(null, typeof(int));

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CanConvert_StringToInt_ShouldReturnTrue()
    {
        // Arrange
        var converter = new DefaultPropertyConverter();

        // Act
        var result = converter.CanConvert(typeof(string), typeof(int));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_SameTypes_ShouldReturnTrue()
    {
        // Arrange
        var converter = new DefaultPropertyConverter();

        // Act
        var result = converter.CanConvert(typeof(string), typeof(string));

        // Assert
        Assert.True(result);
    }
}
