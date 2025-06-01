using Quickwarden.Application;
using Quickwarden.Application.Exceptions;

namespace Quickwarden.Tests;

public class AppInitializeTests
{
    private readonly ApplicationController _applicationController;
    private readonly ApplicationFixture _fixture = new();

    public AppInitializeTests()
    {
        _applicationController = _fixture.CreateApplicationController();
    }

    [Fact]
    public async Task InitializesEnvironment()
    {
        Assert.False(_fixture.QuickwardenEnvironment.Initialized);
        await _applicationController.Initialize();
        Assert.True(_fixture.QuickwardenEnvironment.Initialized);
    }
    
    [Fact]
    public async Task ChecksBitwardenCliInstaled()
    {
        _fixture.QuickwardenEnvironment.TestBitwardenCliInstalled = false;
        var result = await _applicationController.Initialize();
        Assert.Equal(ApplicationInitializeResult.BitwardenCliNotFound, result);
    }

    [Fact]
    public async Task DenyAccess()
    {
        _fixture.SecretRepository.CanGet = false;
        var result = await _applicationController.Initialize();
        Assert.Equal(ApplicationInitializeResult.CouldntAccessKeychain, result);
    }

    [Fact]
    public async Task NoAccounts()
    {
        await _applicationController.Initialize();
        Assert.Empty(_applicationController.GetAccounts());
    }

    [Fact]
    public async Task NoAccountsSecondRun()
    {
        await _applicationController.Initialize();
        var app2 = _fixture.CreateApplicationController();
        await app2.Initialize();
        Assert.Empty(app2.GetAccounts());
    }

    [Fact]
    public void CallingMethodsBeforeInitialize()
    {
        Assert.Throws<ApplicationNotInitializedException>(() => _applicationController.GetAccounts());
    }

    [Fact]
    public async Task FirstRun()
    {
        var result = await _applicationController.Initialize();
        Assert.Equal(ApplicationInitializeResult.Success, result);
    }

    [Fact]
    public async Task SecondRun()
    {
        await _applicationController.Initialize();
        var app2 = _fixture.CreateApplicationController();
        var result = await app2.Initialize();
        Assert.Equal(ApplicationInitializeResult.Success, result);
    }
    
    [Fact]
    public async Task InitializeTwice()
    {
        await _applicationController.Initialize();
        await Assert.ThrowsAsync<ApplicationAlreadyInitializedException>(() => _applicationController
                                                                             .Initialize());
    }

    // Created accounts are saved
    // Configuration versioning!
}
