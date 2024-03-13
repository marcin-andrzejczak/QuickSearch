using QuickSearch.Binding;
using QuickSearch.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace QuickSearch.Tests.Unit.Binding;

public class OptionsBindersProviderTests
{
    private readonly ModelBinderProviderContext _contextSub = Substitute.For<ModelBinderProviderContext>();

    private readonly OptionsBindersProvider _sut;

    public OptionsBindersProviderTests()
    {
        _sut = new OptionsBindersProvider();
    }

    #region GetBinder

    [Theory]
    [InlineData(typeof(SortOptions<object>))]
    [InlineData(typeof(FilterOptions<object>))]
    [InlineData(typeof(SortOptions<List<object>>))]
    [InlineData(typeof(FilterOptions<List<object>>))]
    public void GetBinder_ValidData_ReturnsProperOptionsModelBinder(Type modelType)
    {
        // Arrange
        _contextSub.Metadata
            .Returns(new EmptyModelMetadataProvider().GetMetadataForType(modelType));

        // Act
        var result = _sut.GetBinder(_contextSub);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BinderTypeModelBinder>(result);
    }

    [Fact]
    public void GetBinder_NoContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _sut.GetBinder(null!)
        );
    }

    [Theory]
    [InlineData(typeof(object))]
    [InlineData(typeof(string))]
    [InlineData(typeof(List<object>))]
    [InlineData(typeof(List<string>))]
    public void GetBinder_NotOptionsModelType_ReturnsNull(Type modelType)
    {
        // Arrange
        _contextSub.Metadata
            .Returns(new EmptyModelMetadataProvider().GetMetadataForType(modelType));

        // Act
        var result = _sut.GetBinder(_contextSub);

        // Assert
        Assert.Null(result);
    }

    #endregion GetBinder
}
