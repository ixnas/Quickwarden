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
    public async Task NotInitialized()
    {
        _applicationController = _fixture.CreateApplicationController();
        await Assert.ThrowsAsync<ApplicationNotInitializedException>(() => _applicationController.GetPassword("234978"));
        await Assert.ThrowsAsync<ApplicationNotInitializedException>(() => _applicationController.GetUsername("234978"));
        await Assert.ThrowsAsync<ApplicationNotInitializedException>(() => _applicationController.GetTotp("234978"));
    }
    
    [Fact]
    public async Task NotFound()
    {
        await SignInAccount1();
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _applicationController.GetPassword("anId"));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _applicationController.GetUsername("anId"));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _applicationController.GetTotp("anId"));
    }
    
    [Fact]
    public async Task ReturnsPassword()
    {
        await SignInAccount1();
        var username = await _applicationController.GetUsername("234978");
        Assert.Equal("sjoerd@entry1site.com", username);
        var password = await _applicationController.GetPassword("234978");
        Assert.Equal("password1", password);
        var totp = await _applicationController.GetTotp("234978");
        Assert.Equal("076986", totp.Code);
        Assert.Equal(30, totp.SecondsRemaining);
    }

    [Fact]
    public async Task TotpNotFound()
    {
        await SignInAccount2();
        await Assert.ThrowsAsync<TotpNotFoundException>(() => _applicationController.GetTotp("23847837"));
    }
    
    [Fact]
    public async Task UsernameNotFound()
    {
        await SignInAccount1();
        await Assert.ThrowsAsync<UsernameNotFoundException>(() => _applicationController.GetUsername("348948"));
    }
    
    [Fact]
    public async Task PasswordNotFound()
    {
        await SignInAccount1();
        await Assert.ThrowsAsync<PasswordNotFoundException>(() => _applicationController.GetPassword("483938"));
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
