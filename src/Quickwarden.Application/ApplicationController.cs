using System.Text;
using System.Text.Json;
using Quickwarden.Application.Exceptions;
using Quickwarden.Application.Internal;
using Quickwarden.Application.PlugIns;
using Quickwarden.Application.PlugIns.Bitwarden;
using Quickwarden.Application.PlugIns.FrontEnd;
using Quickwarden.Application.PlugIns.Totp;

namespace Quickwarden.Application;

public class ApplicationController
{
    private const int ConfigurationVersion = 0;
    private readonly ISecretRepository _secretRepository;
    private readonly IQuickwardenEnvironment _environment;
    private readonly IBitwardenInstanceRepository _bitwardenInstanceRepository;
    private readonly IBinaryConfigurationRepository _binaryConfigurationRepository;
    private readonly ITotpGenerator _totpGenerator;
    private readonly List<BitwardenVaultItem> _vaultItems = new();
    private readonly List<Account> _accounts = [];
    private byte[] _secret = [];
    private bool _initialized;

    public ApplicationController(ISecretRepository secretRepository,
                                 IBitwardenInstanceRepository bitwardenInstanceRepository,
                                 IBinaryConfigurationRepository binaryConfigurationRepository,
                                 ITotpGenerator totpGenerator,
                                 IQuickwardenEnvironment environment)
    {
        _secretRepository = secretRepository;
        _bitwardenInstanceRepository = bitwardenInstanceRepository;
        _binaryConfigurationRepository = binaryConfigurationRepository;
        _totpGenerator = totpGenerator;
        _environment = environment;
    }

    public async Task<ApplicationInitializeResult> Initialize()
    {
        if (_initialized)
            throw new ApplicationAlreadyInitializedException();

        var bitwardenCliInstalled = await _environment.BitwardenCliInstalled();
        if (!bitwardenCliInstalled)
            return ApplicationInitializeResult.BitwardenCliNotFound;

        await _environment.Initialize();
        var previousSecret = await _secretRepository.Get();
        if (previousSecret != null)
            return await InitializeFromPreviousState(previousSecret);

        return await InitializeFromScratch();
    }

    public AccountListModel[] GetAccounts()
    {
        if (!_initialized)
            throw new ApplicationNotInitializedException();
        return _accounts.Select(acc => new AccountListModel(acc.Id, acc.Username)).ToArray();
    }

    public async Task<SignInResult> SignIn(string username,
                                           string password,
                                           string totp,
                                           CancellationToken cancellationToken)
    {
        if (!_initialized)
            throw new ApplicationNotInitializedException();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return SignInResult.WrongCredentials;
        try
        {
            var result = await _bitwardenInstanceRepository.Create(username,
                                                                   password,
                                                                   totp,
                                                                   cancellationToken);
            if (result.ResultType == BitwardenInstanceCreateResultType.WrongCredentials)
                return SignInResult.WrongCredentials;
            if (result.ResultType == BitwardenInstanceCreateResultType.Missing2Fa)
                return SignInResult.Missing2Fa;
            if (_accounts.Any(account => account.Username == username))
                return SignInResult.AlreadySignedIn;
            var account = new Account()
            {
                Id = result.Key!.Id,
                Username = username,
                Secret = result.Key.Secret,
            };
            _accounts.Add(account);

            var repos = await _bitwardenInstanceRepository.Get([result.Key]);
            await LoadVaults(repos, cancellationToken);
            await StoreAccounts();

            return SignInResult.Success;
        }
        catch (TaskCanceledException)
        {
            return SignInResult.Timeout;
        }
    }

    public async Task SignOut(string id)
    {
        if (!_initialized)
            throw new ApplicationNotInitializedException();
        var account = _accounts.SingleOrDefault(a => a.Id == id);
        if (account == null)
            throw new KeyNotFoundException();
        var key = new BitwardenInstanceKey(account.Id, account.Username, account.Secret);
        await _bitwardenInstanceRepository.Delete(key);
        _accounts.RemoveAll(account => account.Id == id);
        await StoreAccounts();
    }

    public SearchResultItem[] Search(string query)
    {
        if (!_initialized)
            throw new ApplicationNotInitializedException();
        if (!_accounts.Any() || string.IsNullOrWhiteSpace(query))
            return [];

        var searchTerms = query.Split(' ');

        return _vaultItems
               .Where(item => searchTerms.All(term =>
                                                  item.Name.Contains(term,
                                                                     StringComparison
                                                                         .InvariantCultureIgnoreCase)
                                                  || item.Username?.Contains(term,
                                                           StringComparison.InvariantCultureIgnoreCase)
                                                  == true))
               .Select(item => new SearchResultItem()
               {
                   Id = item.Id,
                   Name = item.Name,
                   Username = item.Username ?? string.Empty,
                   HasTotp = !string.IsNullOrWhiteSpace(item.Totp),
                   HasPassword = !string.IsNullOrWhiteSpace(item.Password),
                   HasUsername = !string.IsNullOrWhiteSpace(item.Username),
               }).ToArray();
    }

    public string GetPassword(string id)
    {
        var item = GetVaultItem(id);
        if (string.IsNullOrWhiteSpace(item.Password))
            throw new PasswordNotFoundException();
        return item.Password;
    }

    public string GetUsername(string id)
    {
        var item = GetVaultItem(id);
        if (string.IsNullOrWhiteSpace(item.Username))
            throw new UsernameNotFoundException();
        return item.Username;
    }

    public ITotpCode GetTotp(string id)
    {
        var item = GetVaultItem(id);
        if (string.IsNullOrWhiteSpace(item.Totp))
            throw new TotpNotFoundException();
        var totp = _totpGenerator.GenerateFromSecret(item.Totp);
        return totp;
    }

    private async Task<ApplicationInitializeResult> InitializeFromScratch()
    {
        var secret = GenerateSecret();
        _secret = Convert.FromHexString(secret);

        var couldWrite = await _secretRepository.Store(secret);
        if (!couldWrite)
            return ApplicationInitializeResult.CouldntWriteToKeychain;

        _initialized = true;
        return ApplicationInitializeResult.Success;
    }

    private async Task<ApplicationInitializeResult> InitializeFromPreviousState(string previousSecret)
    {
        _secret = Convert.FromHexString(previousSecret);
        var listBytesEncrypted = await _binaryConfigurationRepository.Get();
        if (listBytesEncrypted.Length == 0)
        {
            _initialized = true;
            return ApplicationInitializeResult.Success;
        }

        await LoadAccounts(listBytesEncrypted);

        var accountKeys = _accounts.Select(x => new BitwardenInstanceKey(x.Id, x.Username, x.Secret))
                                   .ToArray();
        var instances = await _bitwardenInstanceRepository.Get(accountKeys);
        await LoadVaults(instances, CancellationToken.None);

        _initialized = true;
        return ApplicationInitializeResult.Success;
    }

    private async Task LoadAccounts(byte[] listBytesEncrypted)
    {
        var decryptor = new Decryptor(_secret);
        var decrypted = await decryptor.Decrypt(listBytesEncrypted);
        var configurationDeserialized =
            JsonSerializer.Deserialize<Configuration>(decrypted,
                                                      ApplicationJsonSerializerContext.Default.Configuration);
        var accounts = configurationDeserialized.Accounts;
        _accounts.AddRange(accounts);
    }

    private async Task StoreAccounts()
    {
        var configuration = new Configuration()
        {
            Version = ConfigurationVersion,
            Accounts = _accounts.ToArray(),
        };
        var serialized =
            JsonSerializer.Serialize(configuration, ApplicationJsonSerializerContext.Default.Configuration);
        var bytes = Encoding.UTF8.GetBytes(serialized);
        var encryptor = new Encryptor(_secret);
        var encrypted = await encryptor.Encrypt(bytes);
        await _binaryConfigurationRepository.Store(encrypted);
    }

    private static string GenerateSecret()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes);
    }

    private async Task LoadVaults(IBitwardenInstance[] instances, CancellationToken cancellationToken)
    {
        var allItems = new List<BitwardenVaultItem>();
        foreach (var instance in instances)
        {
            var items = await instance.GetVaultItems(cancellationToken);
            allItems.AddRange(items);
        }

        _vaultItems.AddRange(allItems);
    }

    private BitwardenVaultItem GetVaultItem(string id)
    {
        if (!_initialized)
            throw new ApplicationNotInitializedException();
        var item = _vaultItems.SingleOrDefault(item => item.Id == id);
        if (item == null)
            throw new KeyNotFoundException();
        return item;
    }
}
