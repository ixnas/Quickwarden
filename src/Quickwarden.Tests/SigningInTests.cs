using System.Security.Cryptography;
using System.Text;
using Quickwarden.Application;
using Quickwarden.Application.Exceptions;
using Quickwarden.Application.PlugIns.FrontEnd;

namespace Quickwarden.Tests;

public class SigningInTests
{
    private readonly ApplicationFixture _fixture = new();
    private ApplicationController _applicationController;
    
    public SigningInTests()
    {
        _applicationController = _fixture.CreateApplicationController();
    }

    [Theory]
    [InlineData("wrongUsername", null, null)]
    [InlineData(null, "wrongPassword", null)]
    [InlineData(null, null, "wrongTotp")]
    [InlineData("", "", null)]
    [InlineData("", null, null)]
    [InlineData(null, "", null)]
    public async Task WrongCredentials(string? username, string? password, string? totp)
    {
        await TestWrongCredentials(username, password, totp);
    }

    [Fact]
    public async Task Missing2Fa()
    {
        await _applicationController.Initialize();
        var firstUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        
        var result = await _applicationController.SignIn(firstUser.Username, firstUser.Password, string.Empty, CancellationToken.None);
        Assert.Equal(SignInResult.Missing2Fa, result);
    }

    [Fact]
    public async Task WrongCredentialsNoAccounts()
    {
        await TestWrongCredentials("wrongUser", null, null);
        var accounts = _applicationController.GetAccounts();
        Assert.Empty(accounts);
    }
    
    [Fact]
    public async Task CorrectCredentialsHasAccounts()
    {
        await TestCorrectCredentials();
        var user = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        var accounts = _applicationController.GetAccounts();
        Assert.Equal([new AccountListModel(user.Key.Id, user.Username)], accounts);
    }

    [Fact]
    public async Task CorrectCredentialsTwice()
    {
        await TestCorrectCredentials();
        var firstUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        var result = await _applicationController.SignIn(firstUser.Username, firstUser.Password, firstUser.Totp, CancellationToken.None);
        Assert.Equal(SignInResult.AlreadySignedIn, result);
        var accounts = _applicationController.GetAccounts();
        Assert.Equal([new AccountListModel(firstUser.Key.Id, firstUser.Username)], accounts);
    }

    [Fact]
    public async Task CorrectCredentialsWithDifferentAccounts()
    {
        await _applicationController.Initialize();
        var firstUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        var firstUserResult = await _applicationController.SignIn(firstUser.Username, firstUser.Password, firstUser.Totp, CancellationToken.None);
        Assert.Equal(SignInResult.Success, firstUserResult);
        
        var secondUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.Skip(1).First();
        var secondUserResult = await _applicationController.SignIn(secondUser.Username, secondUser.Password, secondUser.Totp, CancellationToken.None);
        Assert.Equal(SignInResult.Success, secondUserResult);
        
        var accounts = _applicationController.GetAccounts();
        Assert.Equal([new AccountListModel(firstUser.Key.Id, firstUser.Username), new AccountListModel(secondUser.Key.Id, secondUser.Username)], accounts);
    }
    
    [Fact]
    public async Task WrongCredentialsWithDifferentAccount()
    {
        await _applicationController.Initialize();
        var firstUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        var firstUserResult = await _applicationController.SignIn(firstUser.Username, firstUser.Password, firstUser.Totp, CancellationToken.None);
        Assert.Equal(SignInResult.Success, firstUserResult);
        
        var secondUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.Skip(1).First();
        var secondUserResult = await _applicationController.SignIn(secondUser.Username, "wrong", secondUser.Totp, CancellationToken.None);
        Assert.Equal(SignInResult.WrongCredentials, secondUserResult);
        
        var accounts = _applicationController.GetAccounts();
        Assert.Equal([new AccountListModel(firstUser.Key.Id, firstUser.Username)], accounts);
    }

    [Fact]
    public async Task NotInitialized()
    {
        var firstUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        await Assert.ThrowsAsync<ApplicationNotInitializedException>(() => _applicationController.SignIn(firstUser.Username,
                                                            firstUser.Password,
                                                            firstUser.Totp,
                                                               CancellationToken.None));
    }

    [Fact]
    public async Task CorrectCredentials()
    {
        await TestCorrectCredentials();
    }

    [Fact]
    public async Task SavesUsers()
    {
        await _applicationController.Initialize();
        var firstUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        var firstUserResult = await _applicationController.SignIn(firstUser.Username, firstUser.Password, firstUser.Totp, CancellationToken.None);
        Assert.Equal(SignInResult.Success, firstUserResult);
        
        var secondUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.Skip(1).First();
        var secondUserResult = await _applicationController.SignIn(secondUser.Username, secondUser.Password, secondUser.Totp, CancellationToken.None);
        Assert.Equal(SignInResult.Success, secondUserResult);

        _applicationController = _fixture.CreateApplicationController();
        await _applicationController.Initialize();
        var accounts = _applicationController.GetAccounts();
        Assert.Equal([new AccountListModel(firstUser.Key.Id, firstUser.Username), new AccountListModel(secondUser.Key.Id, secondUser.Username)], accounts);
    }
    
    [Fact]
    public async Task CorruptedConfigurationDiscarded()
    {
        await _applicationController.Initialize();
        var firstUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        var firstUserResult = await _applicationController.SignIn(firstUser.Username, firstUser.Password, firstUser.Totp, CancellationToken.None);
        Assert.Equal(SignInResult.Success, firstUserResult);
        
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        await _fixture.BinaryConfigurationRepository.Store(randomBytes);
        
        var app2 = _fixture.CreateApplicationController();
        var result = await app2.Initialize();
        Assert.Equal(ApplicationInitializeResult.Success, result);
        Assert.Empty(app2.GetAccounts());
    }


    [Fact]
    public async Task SavesEncrypted()
    {
        char[] validFirstCharacters = ['{', '['];
        char[] validLastCharacters = ['}', ']'];
        
        await TestCorrectCredentials();
        var bytes = await _fixture.BinaryConfigurationRepository.Get();
        var str = Encoding.UTF8.GetString(bytes);
        var firstCharacter = str[0];
        var lastCharacter = str[^1];
        var firstCharIsValid = validFirstCharacters.Contains(firstCharacter);
        var lastCharIsValid = validLastCharacters.Contains(lastCharacter);
        
        Assert.False(firstCharIsValid && lastCharIsValid);
    }

    [Fact]
    public async Task Timeout()
    {
        await _applicationController.Initialize();
        var firstUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        _fixture.BitwardenInstanceRepository.EnableLongDelay = true;
        var tokenSource = new CancellationTokenSource();
        await tokenSource.CancelAsync();
        var result = await _applicationController.SignIn(firstUser.Username, firstUser.Password, firstUser.Totp, tokenSource.Token);
        Assert.Equal(SignInResult.Timeout, result);
    }

    private async Task TestCorrectCredentials()
    {
        await _applicationController.Initialize();
        var firstUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        var result = await _applicationController.SignIn(firstUser.Username, firstUser.Password, firstUser.Totp, CancellationToken.None);
        Assert.Equal(SignInResult.Success, result);
    }

    private async Task TestWrongCredentials(string? replaceUser, string? replacePassword, string? replaceTotp)
    {
        await _applicationController.Initialize();
        var firstUser = _fixture.BitwardenInstanceRepository.InstancesWithCredentials.First();
        var username = replaceUser ?? firstUser.Username;
        var password = replacePassword ?? firstUser.Password;
        var totp = replaceTotp ?? firstUser.Totp;
        
        var result = await _applicationController.SignIn(username, password, totp, CancellationToken.None);
        Assert.Equal(SignInResult.WrongCredentials, result);

        var searchEntries = _applicationController.Search("Entry");
        Assert.Empty(searchEntries);
    }
    
    // Search returns password list with entries
    // Sign in with second account should append entries to password list
    // Restarting application returns password list with entries
    
    // BW_NOINTERACTION="true" bw login --raw username password
    // BW_NOINTERACTION="true" bw login --raw username password --method 0 --code totpcode
    // If 2 factor is required: "Login failed. No provider selected."
    // Invalid credentials: "Username or password is incorrect. Try again."
    // Invalid 2fa: "Two-step token is invalid. Try again."
}
