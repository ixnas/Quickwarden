using Quickwarden.Application;
using Quickwarden.Application.PlugIns.FrontEnd;

namespace Quickwarden.Tests;

public class RecentTests
{
    public enum Credentials
    {
        Username,
        Password,
        Totp,
    }

    private Dictionary<Credentials, Action<string>> GetCredentialsFns => new()
    {
        [Credentials.Username] = (id) => { _applicationController.GetUsername(id); },
        [Credentials.Password] = (id) => { _applicationController.GetPassword(id); },
        [Credentials.Totp] = (id) => { _applicationController.GetTotp(id); }
    };
    
    private readonly ApplicationFixture _fixture = new();
    private ApplicationController _applicationController;

    public RecentTests()
    {
        _applicationController = _fixture.CreateApplicationController();
    }

    [Theory]
    [InlineData(Credentials.Username)]
    [InlineData(Credentials.Password)]
    public async Task AddsMultipleEntries(Credentials credential)
    {
        await _applicationController.Initialize();
        await SignInAccount1();
        await SignInAccount2();
        var search1 = _applicationController.Search(string.Empty);
        Assert.Empty(search1);
        GetCredentialsFns[credential]("234978");
        var search2 = _applicationController.Search(string.Empty);
        var entry = search2.Single();
        Assert.Equal("234978", entry.Id);
        GetCredentialsFns[credential]("23847837");
        var search3 = _applicationController.Search(string.Empty);
        Assert.Equal("23847837", search3[0].Id);
        Assert.Equal("234978", search3[1].Id);
    }
    
    [Theory]
    [InlineData(Credentials.Username)]
    [InlineData(Credentials.Password)]
    public async Task MovesReusedToTop(Credentials credential)
    {
        await _applicationController.Initialize();
        await SignInAccount1();
        await SignInAccount2();
        var search1 = _applicationController.Search(string.Empty);
        Assert.Empty(search1);
        GetCredentialsFns[credential]("234978");
        var search2 = _applicationController.Search(string.Empty);
        var entry = search2.Single();
        Assert.Equal("234978", entry.Id);
        GetCredentialsFns[credential]("23847837");
        var search3 = _applicationController.Search(string.Empty);
        Assert.Equal("23847837", search3[0].Id);
        Assert.Equal("234978", search3[1].Id);
        GetCredentialsFns[credential]("234978");
        var search4 = _applicationController.Search(string.Empty);
        Assert.Equal("234978", search4[0].Id);
        Assert.Equal("23847837", search4[1].Id);
    }

    [Theory]
    [InlineData(Credentials.Username)]
    [InlineData(Credentials.Password)]
    [InlineData(Credentials.Totp)]
    public async Task NoDuplicates(Credentials credential)
    {
        await _applicationController.Initialize();
        await SignInAccount1();
        await SignInAccount2();
        var search1 = _applicationController.Search(string.Empty);
        Assert.Empty(search1);
        GetCredentialsFns[credential]("234978");
        var search2 = _applicationController.Search(string.Empty);
        var entry2 = search2.Single();
        Assert.Equal("234978", entry2.Id);
        GetCredentialsFns[credential]("234978");
        var search3 = _applicationController.Search(string.Empty);
        var entry3 = search3.Single();
        Assert.Equal("234978", entry3.Id);
    }

    [Theory]
    [InlineData(Credentials.Username)]
    [InlineData(Credentials.Password)]
    public async Task SavesRecentEntries(Credentials credential)
    {
        await _applicationController.Initialize();
        await SignInAccount1();
        await SignInAccount2();
        GetCredentialsFns[credential]("234978");
        GetCredentialsFns[credential]("23847837");
        _applicationController = _fixture.CreateApplicationController();
        await _applicationController.Initialize();
        var search1 = _applicationController.Search(string.Empty);
        Assert.Equal("23847837", search1[0].Id);
        Assert.Equal("234978", search1[1].Id);
    }

    [Theory]
    [InlineData(Credentials.Username)]
    [InlineData(Credentials.Password)]
    public async Task SearchesRecentFirst(Credentials credential)
    {
        await _applicationController.Initialize();
        await SignInAccount1();
        await SignInAccount2();
        GetCredentialsFns[credential]("23847837");
        var search = _applicationController.Search("Vault");
        Assert.Equal(2, search.Length);
        Assert.Equal("23847837", search[0].Id);
        Assert.Equal("234978", search[1].Id);
    }
    
    [Theory]
    [InlineData(Credentials.Username)]
    [InlineData(Credentials.Password)]
    [InlineData(Credentials.Totp)]
    public async Task RemovesEntriesAfterSignOut(Credentials credential)
    {
        await _applicationController.Initialize();
        await SignInAccount1();
        await SignInAccount2();
        GetCredentialsFns[credential]("234978");
        await _applicationController.SignOut("id1");
        var search1 = _applicationController.Search(string.Empty);
        Assert.Empty(search1);
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
