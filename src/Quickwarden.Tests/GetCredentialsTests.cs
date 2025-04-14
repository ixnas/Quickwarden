using Quickwarden.Application;
using Quickwarden.Application.Exceptions;
using Quickwarden.Application.PlugIns.FrontEnd;

namespace Quickwarden.Tests;

public class GetCredentialsTests : IAsyncLifetime
{
    private readonly ApplicationFixture _fixture = new();
    private ApplicationController _applicationController;

    public GetCredentialsTests()
    {
        _applicationController = _fixture.CreateApplicationController();
    }
    
    [Fact]
    public void NotInitialized()
    {
        _applicationController = _fixture.CreateApplicationController();
        Assert.Throws<ApplicationNotInitializedException>(() => _applicationController.GetPassword("234978"));
        Assert.Throws<ApplicationNotInitializedException>(() => _applicationController.GetUsername("234978"));
        Assert.Throws<ApplicationNotInitializedException>(() => _applicationController.GetTotp("234978"));
    }
    
    [Fact]
    public async Task NotFound()
    {
        await SignInAccount1();
        Assert.Throws<KeyNotFoundException>(() => _applicationController.GetPassword("anId"));
        Assert.Throws<KeyNotFoundException>(() => _applicationController.GetUsername("anId"));
        Assert.Throws<KeyNotFoundException>(() => _applicationController.GetTotp("anId"));
    }
    
    [Fact]
    public async Task ReturnsPassword()
    {
        await SignInAccount1();
        var username = _applicationController.GetUsername("234978");
        Assert.Equal("sjoerd@entry1site.com", username);
        var password = _applicationController.GetPassword("234978");
        Assert.Equal("password1", password);
        var totp = _applicationController.GetTotp("234978");
        Assert.Equal("076986", totp.Code);
        Assert.Equal(30, totp.SecondsRemaining);
    }

    [Fact]
    public async Task TotpNotFound()
    {
        await SignInAccount2();
        Assert.Throws<TotpNotFoundException>(() => _applicationController.GetTotp("23847837"));
    }
    
    [Fact]
    public async Task UsernameNotFound()
    {
        await SignInAccount1();
        Assert.Throws<UsernameNotFoundException>(() => _applicationController.GetUsername("348948"));
    }
    
    [Fact]
    public async Task PasswordNotFound()
    {
        await SignInAccount1();
        Assert.Throws<PasswordNotFoundException>(() => _applicationController.GetPassword("483938"));
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
