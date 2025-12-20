using FabricMappingService.Core.Attributes;
using FabricMappingService.Core.Services;
using Xunit;

namespace FabricMappingService.Tests;

public class AttributeMappingServiceTests
{
    private class SourceModel
    {
        [MapTo("Id")]
        public int SourceId { get; set; }

        [MapTo("Name")]
        public string SourceName { get; set; } = string.Empty;

        [IgnoreMapping]
        public string IgnoreMe { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;
    }

    private class TargetModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    [Fact]
    public void Map_WithMapToAttribute_ShouldMapCorrectly()
    {
        // Arrange
        var mapper = new AttributeMappingService();
        var source = new SourceModel
        {
            SourceId = 123,
            SourceName = "Test Name",
            IgnoreMe = "Should be ignored",
            Country = "USA"
        };

        // Act
        var result = mapper.Map<SourceModel, TargetModel>(source);

        // Assert
        Assert.Equal(123, result.Id);
        Assert.Equal("Test Name", result.Name);
        Assert.Equal("USA", result.Country);
    }

    [Fact]
    public void Map_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mapper = new AttributeMappingService();
        SourceModel? source = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.Map<SourceModel, TargetModel>(source!));
    }

    [Fact]
    public void MapWithResult_ShouldReturnSuccessResult()
    {
        // Arrange
        var mapper = new AttributeMappingService();
        var source = new SourceModel
        {
            SourceId = 456,
            SourceName = "Test",
            Country = "Canada"
        };

        // Act
        var result = mapper.MapWithResult<SourceModel, TargetModel>(source);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Result);
        Assert.Equal(456, result.Result.Id);
        Assert.Equal("Test", result.Result.Name);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void MapCollection_ShouldMapAllItems()
    {
        // Arrange
        var mapper = new AttributeMappingService();
        var sources = new List<SourceModel>
        {
            new() { SourceId = 1, SourceName = "One", Country = "USA" },
            new() { SourceId = 2, SourceName = "Two", Country = "Canada" },
            new() { SourceId = 3, SourceName = "Three", Country = "Mexico" }
        };

        // Act
        var results = mapper.MapCollection<SourceModel, TargetModel>(sources).ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(1, results[0].Id);
        Assert.Equal("Two", results[1].Name);
        Assert.Equal("Mexico", results[2].Country);
    }

    [Fact]
    public void MapToExisting_ShouldUpdateExistingObject()
    {
        // Arrange
        var mapper = new AttributeMappingService();
        var source = new SourceModel
        {
            SourceId = 789,
            SourceName = "Updated Name",
            Country = "UK"
        };
        var target = new TargetModel
        {
            Id = 0,
            Name = "Old Name",
            Country = "Old Country"
        };

        // Act
        mapper.MapToExisting(source, target);

        // Assert
        Assert.Equal(789, target.Id);
        Assert.Equal("Updated Name", target.Name);
        Assert.Equal("UK", target.Country);
    }
}
