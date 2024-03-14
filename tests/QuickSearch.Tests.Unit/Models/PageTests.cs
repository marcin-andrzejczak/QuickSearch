using Bogus;
using QuickSearch.Pagination;

namespace QuickSearch.Tests.Unit.Models;

internal class SourceType
{
    public string? Property { get; set; }
}

internal class TargetType
{
    public string? AnotherProperty { get; set; }
}

public class PageTests
{
    public PageTests()
    {
        Randomizer.Seed = new Random(12345);
    }

    [Fact]
    public void MapTo_ValidData_ReturnsDesiredTypePage()
    {
        // Arrange
        var items = new Faker<SourceType>()
            .RuleFor(x => x.Property, f => f.Random.Guid().ToString())
            .Generate(10);

        var source = new Page<SourceType>(items, 1, items.Count, items.Count);

        // Act
        var target = source.MapTo(x => new TargetType { AnotherProperty = x.Property });

        // Assert
        Assert.NotNull(target);
        Assert.IsType<Page<TargetType>>(target);
        Assert.Equal(source.CurrentPage, target.CurrentPage);
        Assert.Equal(source.PageSize, target.PageSize);
        Assert.Equal(source.TotalItems, target.TotalItems);
        Assert.Equal(source.TotalPages, target.TotalPages);
        Assert.All(source.Items, 
            si => Assert.Single(target.Items, ti => ti.AnotherProperty == si.Property)
        );
    }
}
