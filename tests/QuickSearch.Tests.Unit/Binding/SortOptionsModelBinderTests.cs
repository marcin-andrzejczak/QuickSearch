using QuickSearch.Binding;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QuickSearch.Options;

namespace QuickSearch.Tests.Unit.Binding;

public class SortOptionsModelBinderTests
{
    internal class Account
    {
        public int? Balance { get; set; }
    }

    internal class User
    {
        public string? FirstName { get; set; }
        public Account? Account { get; set; }
    }

    private readonly SortOptionsModelBinder<User> _sut;

    public SortOptionsModelBinderTests()
    {
        _sut = new SortOptionsModelBinder<User>();
    }

    #region Setup

    public ModelBindingContext CreateQueryModelBindingContext(string query)
    {
        var modelMetadata = new EmptyModelMetadataProvider()
            .GetMetadataForType(typeof(SortOptions<User>));

        var requestFeature = new HttpRequestFeature
        {
            QueryString = $"?{query}"
        };

        var features = new FeatureCollection();
        features.Set<IHttpRequestFeature>(requestFeature);

        var fakeHttpContext = new DefaultHttpContext(features);

        return new DefaultModelBindingContext
        {
            ModelName = "s",
            ModelMetadata = modelMetadata,
            ModelState = new ModelStateDictionary(),
            ActionContext = new ActionContext()
            {
                HttpContext = fakeHttpContext
            }
        };
    }

    #endregion Setup

    #region BindModelAsync

    [Theory]
    [InlineData("s.firstName=asc")]
    [InlineData("s.firstName=desc")]
    [InlineData("s.firstname=desc")]
    [InlineData("s.account.balance=asc")]
    [InlineData("s.account.balance=desc")]
    [InlineData("s.firstName=asc&s.account.balance=desc")]
    [InlineData("s.firstName=asc&s.account.balance=asc")]
    public async Task BindModelAsync_ValidData_CreatesSortOptionsObjectAsResult(string query)
    {
        // Arrange
        var modelBindingContext = CreateQueryModelBindingContext(query);

        // Act
        await _sut.BindModelAsync(modelBindingContext);

        // Assert
        Assert.True(modelBindingContext.Result.IsModelSet);
        Assert.NotNull(modelBindingContext.Result.Model);
        Assert.IsType<SortOptions<User>>(modelBindingContext.Result.Model);
    }

    [Theory]
    [InlineData("s=asc")]
    [InlineData("s.=asc")]
    public async Task BindModelAsync_MissingProperty_AddsError(string query)
    {
        // Arrange
        var expectedErrorMessage = "Missing entity property";
        var modelBindingContext = CreateQueryModelBindingContext(query);

        // Act
        await _sut.BindModelAsync(modelBindingContext);

        // Assert
        Assert.False(modelBindingContext.Result.IsModelSet);
        Assert.Null(modelBindingContext.Result.Model);
        Assert.False(modelBindingContext.ModelState.IsValid);
        Assert.Single(modelBindingContext.ModelState);
        Assert.All(modelBindingContext.ModelState.Values,
            v => Assert.Equal(expectedErrorMessage, v.Errors.Single().ErrorMessage)
        );
    }

    [Theory]
    [InlineData("s.firstName=asc&s.firstName=desc")]
    [InlineData("s.firstName=asc&s.firstName=asc")]
    public async Task BindModelAsync_MultipleSortDirectionsForSingleProperty_AddsError(string query)
    {
        // Arrange
        var expectedErrorMessage = "Property cannot have multiple sorting directions";
        var modelBindingContext = CreateQueryModelBindingContext(query);

        // Act
        await _sut.BindModelAsync(modelBindingContext);

        // Assert
        Assert.False(modelBindingContext.Result.IsModelSet);
        Assert.Null(modelBindingContext.Result.Model);
        Assert.False(modelBindingContext.ModelState.IsValid);
        Assert.Single(modelBindingContext.ModelState);
        Assert.All(modelBindingContext.ModelState.Values,
            v => Assert.Equal(expectedErrorMessage, v.Errors.Single().ErrorMessage)
        );
    }

    [Theory]
    [InlineData("s.firstName=test")]
    [InlineData("s.account.balance=")]
    public async Task BindModelAsync_InvalidSortDirection_AddsError(string query)
    {
        // Arrange
        var expectedErrorMessage = "Unrecognized sort direction value";
        var modelBindingContext = CreateQueryModelBindingContext(query);

        // Act
        await _sut.BindModelAsync(modelBindingContext);

        // Assert
        Assert.False(modelBindingContext.Result.IsModelSet);
        Assert.Null(modelBindingContext.Result.Model);
        Assert.False(modelBindingContext.ModelState.IsValid);
        Assert.Single(modelBindingContext.ModelState);
        Assert.All(modelBindingContext.ModelState.Values,
            v => Assert.Equal(expectedErrorMessage, v.Errors.Single().ErrorMessage)
        );
    }

    [Theory]
    [InlineData("s.notexists=asc")]
    [InlineData("s.firstName.notexists=asc")]
    [InlineData("s.account-balance=asc")]
    [InlineData("s.accountBalance=asc")]
    [InlineData("s.account.notexists=asc")]
    public async Task BindModelAsync_InvalidProperty_AddsError(string query)
    {
        // Arrange
        var expectedErrorMessage = $"Property does not exist on entity '{typeof(User).Name}'";
        var modelBindingContext = CreateQueryModelBindingContext(query);

        // Act
        await _sut.BindModelAsync(modelBindingContext);

        // Assert
        Assert.False(modelBindingContext.Result.IsModelSet);
        Assert.Null(modelBindingContext.Result.Model);
        Assert.False(modelBindingContext.ModelState.IsValid);
        Assert.Single(modelBindingContext.ModelState);
        Assert.All(modelBindingContext.ModelState.Values,
            v => Assert.Equal(expectedErrorMessage, v.Errors.Single().ErrorMessage)
        );
    }

    #endregion BindModelAsync
}
