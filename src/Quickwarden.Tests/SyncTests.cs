using Quickwarden.Application;
using Quickwarden.Application.PlugIns.Bitwarden;
using Quickwarden.Application.PlugIns.FrontEnd;
using Quickwarden.Tests.Fakes;

namespace Quickwarden.Tests;

public class SyncTests
{
    private readonly ApplicationFixture _fixture = new();
    private ApplicationController _applicationController;

    public SyncTests()
    {
        _applicationController = _fixture.CreateApplicationController();
    }

    [Fact]
    public async Task RemovesRemovedEntries()
    {
        await _applicationController.Initialize();
        await _fixture.SignInAccount1(_applicationController);
        await _fixture.SignInAccount2(_applicationController);
        var results = _applicationController.Search("Vault");
        Assert.Equal(2, results.Length);
        var instance = _fixture.BitwardenInstanceRepository.BitwardenInstances.Single(instance => instance.Key.Id == "id1").Instance;
        ((BitwardenInstanceFake)instance).VaultItems.RemoveAll(item => item.Id == "234978");
        await _applicationController.Sync();
        var results2 = _applicationController.Search("Vault");
        Assert.Single(results2);
    }
    
    [Fact]
    public async Task AddsNewEntry()
    {
        await _applicationController.Initialize();
        await _fixture.SignInAccount1(_applicationController);
        await _fixture.SignInAccount2(_applicationController);
        var results = _applicationController.Search("Vault");
        Assert.Equal(2, results.Length);
        var instance = _fixture.BitwardenInstanceRepository.BitwardenInstances.Single(instance => instance.Key.Id == "id1").Instance;
        ((BitwardenInstanceFake)instance).VaultItems.Add(new BitwardenVaultItem()
        {
            Id = "43948392328",
            Name = "Vault entry 4",
            VaultId = "id1",
        });
        await _applicationController.Sync();
        var results2 = _applicationController.Search("Vault");
        Assert.Equal(3, results2.Length);
    }
}
