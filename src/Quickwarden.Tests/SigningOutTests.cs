using Quickwarden.Application;
using Quickwarden.Application.Exceptions;
using Quickwarden.Application.PlugIns.FrontEnd;

namespace Quickwarden.Tests;

public class SigningOutTests : IAsyncLifetime
{
    private readonly ApplicationFixture _fixture = new();
    private ApplicationController _applicationController;

    public SigningOutTests()
    {
        _applicationController = _fixture.CreateApplicationController();
    }

    [Fact]
    public async Task NotInitialized()
    {
        _applicationController = _fixture.CreateApplicationController();
        await Assert.ThrowsAsync<ApplicationNotInitializedException>(() =>
                                                                         _applicationController
                                                                             .SignOut("id1"));
    }

    [Fact]
    public async Task RemovesAccount()
    {
        await SignInAccount1();
        await SignInAccount2();

        await _applicationController.SignOut("id1");
        Assert.DoesNotContain(_fixture.BitwardenInstanceRepository.BitwardenInstances,
                              item => item.Instance.Id == "id1");
        var accounts = _applicationController.GetAccounts();
        Assert.Single(accounts);
        Assert.Equal("id2", accounts[0].Id);
        Assert.Equal("hannie", accounts[0].Username);

        await _applicationController.SignOut("id2");
        accounts = _applicationController.GetAccounts();
        Assert.Empty(accounts);
        Assert.Empty(_fixture.BitwardenInstanceRepository.BitwardenInstances);
    }

    [Fact]
    public async Task RemovesSavedAccount()
    {
        await SignInAccount1();
        await SignInAccount2();

        await _applicationController.SignOut("id1");

        _applicationController = _fixture.CreateApplicationController();
        await _applicationController.Initialize();

        var accounts = _applicationController.GetAccounts();
        Assert.Single(accounts);
        Assert.Equal("id2", accounts[0].Id);
        Assert.Equal("hannie", accounts[0].Username);

        _applicationController = _fixture.CreateApplicationController();
        await _applicationController.Initialize();

        await _applicationController.SignOut("id2");
        accounts = _applicationController.GetAccounts();
        Assert.Empty(accounts);
    }

    [Fact]
    public async Task NonExistingId()
    {
        await SignInAccount1();
        await SignInAccount2();
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _applicationController.SignOut("Hey"));
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
