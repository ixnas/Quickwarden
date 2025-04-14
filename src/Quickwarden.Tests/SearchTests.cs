using Quickwarden.Application;
using Quickwarden.Application.Exceptions;
using Quickwarden.Application.PlugIns.FrontEnd;

namespace Quickwarden.Tests;

public class SearchTests : IAsyncLifetime
{
    private readonly ApplicationFixture _fixture = new();
    private ApplicationController _applicationController;

    public SearchTests()
    {
        _applicationController = _fixture.CreateApplicationController();
    }

    [Fact]
    public async Task NotInitialized()
    {
        _applicationController = _fixture.CreateApplicationController();
        Assert.Throws<ApplicationNotInitializedException>(() => _applicationController.Search("sjoerd"));
    }

    [Fact]
    public async Task SearchSingleAccount()
    {
        await SignInAccount1();
        var searchResult = _applicationController.Search("No");
        Assert.Equal(2, searchResult.Length);
        
        var entry1 = searchResult.Single(result => result.Name == "NoPass");
        Assert.Equal("NoPass", entry1.Name);
        Assert.Equal("sjoerd@nopass.com", entry1.Username);
        Assert.False(entry1.HasTotp);
        Assert.False(entry1.HasPassword);
        Assert.True(entry1.HasUsername);
        Assert.False(string.IsNullOrWhiteSpace(entry1.Id));
        
        var entry2 = searchResult.Single(result => result.Name == "NoUser");
        Assert.Equal("NoUser", entry2.Name);
        Assert.Empty(entry2.Username);
        Assert.False(entry2.HasTotp);
        Assert.True(entry2.HasPassword);
        Assert.False(entry2.HasUsername);
        Assert.False(string.IsNullOrWhiteSpace(entry2.Id));
    }

    [Fact]
    public async Task SearchMultipleAccounts()
    {
        await SignInAccount1();
        await SignInAccount2();
        
        var searchResult = _applicationController.Search("Entry");
        Assert.Equal(2, searchResult.Count());

        var entry1 = searchResult.Single(item => item.Name == "Vault entry 1");
        Assert.Equal("sjoerd@entry1site.com", entry1.Username);
        Assert.True(entry1.HasTotp);
        Assert.True(entry1.HasPassword);
        Assert.True(entry1.HasUsername);
        Assert.False(string.IsNullOrWhiteSpace(entry1.Id));
        
        var entry2 = searchResult.Single(item => item.Name == "Vault entry 2");
        Assert.Equal("hannie@entry2site.com", entry2.Username);
        Assert.False(entry2.HasTotp);
        Assert.True(entry2.HasPassword);
        Assert.True(entry2.HasUsername);
        Assert.False(string.IsNullOrWhiteSpace(entry2.Id));
    }

    [Fact]
    public void SearchNoAccounts()
    {
        var searchResults = _applicationController.Search("Entry");
        Assert.Empty(searchResults);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task SearchEmptyQuery(string? query)
    {
        await SignInAccount1();
        await SignInAccount2();
        var searchResults = _applicationController.Search(query);
        Assert.Empty(searchResults);
    }

    [Fact]
    public async Task SearchNonMatchingQuery()
    {
        await SignInAccount1();
        await SignInAccount2();
        var searchResults = _applicationController.Search("Something");
        Assert.Empty(searchResults);
    }
    
    [Fact]
    public async Task SearchByName()
    {
        await SignInAccount1();
        await SignInAccount2();
        var searchResults = _applicationController.Search("Entry1site.com");
        
        var entry1 = searchResults.Single(item => item.Name == "Vault entry 1");
        Assert.Equal("sjoerd@entry1site.com", entry1.Username);
        Assert.True(entry1.HasTotp);
        Assert.True(entry1.HasPassword);
        Assert.False(string.IsNullOrWhiteSpace(entry1.Id));
    }
    
    [Fact]
    public async Task SearchAfterRestart()
    {
        await SignInAccount1();
        await SignInAccount2();
        
        _applicationController = _fixture.CreateApplicationController();
        await _applicationController.Initialize();
        
        var searchResult = _applicationController.Search("Entry");
        Assert.Equal(2, searchResult.Count());

        var entry1 = searchResult.Single(item => item.Name == "Vault entry 1");
        Assert.Equal("sjoerd@entry1site.com", entry1.Username);
        Assert.True(entry1.HasTotp);
        Assert.True(entry1.HasPassword);
        Assert.True(entry1.HasUsername);
        Assert.False(string.IsNullOrWhiteSpace(entry1.Id));
        
        var entry2 = searchResult.Single(item => item.Name == "Vault entry 2");
        Assert.Equal("hannie@entry2site.com", entry2.Username);
        Assert.False(entry2.HasTotp);
        Assert.True(entry2.HasPassword);
        Assert.True(entry2.HasUsername);
        Assert.False(string.IsNullOrWhiteSpace(entry2.Id));
    }
    
    public async Task InitializeAsync()
    {
        await _applicationController.Initialize();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SignInAccount1()
    {
        var signInResult = await _applicationController.SignIn("sjoerd",
                                                               " pass",
                                                               "237489",
                                                               CancellationToken.None);
        Assert.Equal(SignInResult.Success, signInResult);
    }

    private async Task SignInAccount2()
    {
        var signInResult = await _applicationController.SignIn("hannie",
                                                                "pass2",
                                                                "473829",
                                                                CancellationToken.None);
        Assert.Equal(SignInResult.Success, signInResult);
    }
}
