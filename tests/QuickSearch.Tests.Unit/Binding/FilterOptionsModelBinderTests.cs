using QuickSearch.Binding;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QuickSearch.Options;

namespace QuickSearch.Tests.Unit.Binding;

public class FilterOptionsModelBinderTests
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

    private readonly FilterOptionsModelBinder<User> _sut;

    public FilterOptionsModelBinderTests()
    {
        _sut = new FilterOptionsModelBinder<User>();
    }

    #region Setup

    public ModelBindingContext CreateQueryModelBindingContext(string query)
    {
        var modelMetadata = new EmptyModelMetadataProvider()
            .GetMetadataForType(typeof(FilterOptions<User>));

        var requestFeature = new HttpRequestFeature
        {
            QueryString = $"?{query}"
        };

        var features = new FeatureCollection();
        features.Set<IHttpRequestFeature>(requestFeature);

        var fakeHttpContext = new DefaultHttpContext(features);

        return new DefaultModelBindingContext
        {
            ModelName = "f",
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
    [InlineData("f.firstName.eq=Test")]
    [InlineData("f.firstName.eq=Test Name")]
    [InlineData("f.firstname.eq=Test Name")]
    [InlineData("f.account.eq=null")]
    [InlineData("f.account.balance.eq=20")]
    [InlineData("f.account.balance.eq=-1")]
    [InlineData("f.account.balance.gt=1&f.account.balance.lt=10")]
    [InlineData("f.account.balance.gt=1&f.account.balance.gt=10")]
    [InlineData("f.firstName.eq=Test&f.account.balance.eq=20")]
    [InlineData("f.firstName.eq=Test Name&f.account.balance.eq=20")]
    [InlineData("f.firstName.eq=Test Name&f.account.balance.eq=-1")]
    public async Task BindModelAsync_ValidData_CreatesFilterOptionsObjectAsResult(string query)
    {
        // Arrange
        var modelBindingContext = CreateQueryModelBindingContext(query);

        // Act
        await _sut.BindModelAsync(modelBindingContext);

        // Assert
        Assert.True(modelBindingContext.Result.IsModelSet);
        Assert.NotNull(modelBindingContext.Result.Model);
        Assert.IsType<FilterOptions<User>>(modelBindingContext.Result.Model);
    }

    [Theory]
    [InlineData("f.firstName=Test")]
    [InlineData("f.eq=Test")]
    [InlineData("f=Test")]
    public async Task BindModelAsync_MissingPropertyOrFilter_AddsError(string query)
    {
        // Arrange
        var expectedErrorMessage = "Invalid filter data, missing property or filter";
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
    [InlineData("f.firstName.notexists=Test")]
    [InlineData("f.firstName.nq=Test")]
    [InlineData("f.firstName.etl=Test")]
    [InlineData("f.firstName.=Test")]
    public async Task BindModelAsync_InvalidFilterType_AddsError(string query)
    {
        // Arrange
        var expectedErrorMessage = "Invalid filter type";
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
    [InlineData("f.notexists.eq=Test")]
    [InlineData("f.firstName.notexists.eq=Test")]
    [InlineData("f.account-balance.eq=20")]
    [InlineData("f.accountBalance.eq=-1")]
    [InlineData("f.account.notexists.eq=-1")]
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
