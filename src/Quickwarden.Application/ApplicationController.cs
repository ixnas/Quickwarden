using System.Security.Cryptography;
using System.Security.Principal;
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
    private readonly IEncryptionManager _encryptionManager;
    private readonly IQuickwardenEnvironment _environment;
    private readonly IBitwardenInstanceRepository _bitwardenInstanceRepository;
    private readonly IBinaryConfigurationRepository _binaryConfigurationRepository;
    private readonly ITotpGenerator _totpGenerator;
    private readonly List<BitwardenVaultItem> _vaultItems = new();
    private readonly List<RecentVaultEntry> _recentVaultEntries = new();
    private readonly List<Account> _accounts = [];
    private bool _initialized;

    public ApplicationController(IEncryptionManager encryptionManager,
                                 IBitwardenInstanceRepository bitwardenInstanceRepository,
                                 IBinaryConfigurationRepository binaryConfigurationRepository,
                                 ITotpGenerator totpGenerator,
                                 IQuickwardenEnvironment environment)
    {
        _encryptionManager = encryptionManager;
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
        var couldAccessKeychain = await _encryptionManager.Initialize();
        if (!couldAccessKeychain)
            return ApplicationInitializeResult.CouldntAccessKeychain;
        
        return await InitializeFromPreviousState();
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
            await StoreConfiguration();

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
        var vaultItemIds = _vaultItems
                           .Where(item => item.VaultId == id)
                           .Select(item => item.Id);
        _recentVaultEntries.RemoveAll(item => vaultItemIds.Contains(item.Id));
        _vaultItems.RemoveAll(item => item.VaultId == account.Id);
        var key = new BitwardenInstanceKey(account.Id, account.Username, account.Secret);
        await _bitwardenInstanceRepository.Delete(key);
        _accounts.RemoveAll(account => account.Id == id);
        await StoreConfiguration();
    }

    public SearchResultItem[] Search(string query)
    {
        if (!_initialized)
            throw new ApplicationNotInitializedException();
        var recentVaultEntries = _recentVaultEntries
            .Select(item => _vaultItems.Single(vaultItem => vaultItem.Id == item.Id));
        if (string.IsNullOrWhiteSpace(query))
            return ToSearchResultItems(recentVaultEntries);

        var filteredRecent = FilterSearchResults(recentVaultEntries, query);
        var filtered = FilterSearchResults(_vaultItems, query);
        var allResults = filteredRecent.Concat(filtered).Distinct();
        
        return ToSearchResultItems(allResults);
    }

    public async Task<string> GetPassword(string id)
    {
        var item = await GetVaultItem(id);
        if (string.IsNullOrWhiteSpace(item.Password))
            throw new PasswordNotFoundException();
        return item.Password;
    }

    public async Task<string> GetUsername(string id)
    {
        var item = await GetVaultItem(id);
        if (string.IsNullOrWhiteSpace(item.Username))
            throw new UsernameNotFoundException();
        return item.Username;
    }

    public async Task<ITotpCode> GetTotp(string id)
    {
        var item = await GetVaultItem(id);
        if (string.IsNullOrWhiteSpace(item.Totp))
            throw new TotpNotFoundException();
        var totp = _totpGenerator.GenerateFromSecret(item.Totp);
        return totp;
    }

    private async Task<ApplicationInitializeResult> InitializeFromPreviousState()
    {
        var listBytesEncrypted = await _binaryConfigurationRepository.Get();
        if (listBytesEncrypted.Length == 0)
        {
            _initialized = true;
            return ApplicationInitializeResult.Success;
        }

        await LoadConfiguration(listBytesEncrypted);

        var accountKeys = _accounts
                          .Select(x => new BitwardenInstanceKey(x.Id, x.Username, x.Secret))
                          .ToArray();
        var instances = await _bitwardenInstanceRepository.Get(accountKeys);
        await LoadVaults(instances, CancellationToken.None);

        _initialized = true;
        return ApplicationInitializeResult.Success;
    }

    private async Task LoadConfiguration(byte[] listBytesEncrypted)
    {
        try
        {
            var decrypted = await _encryptionManager.Decrypt(listBytesEncrypted);
            var configurationDeserialized =
                JsonSerializer.Deserialize<Configuration>(decrypted,
                    ApplicationJsonSerializerContext.Default.Configuration);
            var accounts = configurationDeserialized?.Accounts ?? [];
            var recentVaultEntries = configurationDeserialized?.RecentVaultEntries ?? [];
            _accounts.AddRange(accounts);
            _recentVaultEntries.AddRange(recentVaultEntries);
        }
        catch (CryptographicException)
        {
        }
    }

    private async Task StoreConfiguration()
    {
        var configuration = new Configuration()
        {
            Version = ConfigurationVersion,
            Accounts = _accounts.ToArray(),
            RecentVaultEntries = _recentVaultEntries.ToArray(),
        };
        var serialized =
            JsonSerializer.Serialize(configuration, ApplicationJsonSerializerContext.Default.Configuration);
        var bytes = Encoding.UTF8.GetBytes(serialized);
        var encrypted = await _encryptionManager.Encrypt(bytes);
        await _binaryConfigurationRepository.Store(encrypted);
    }

    private async Task LoadVaults(IBitwardenInstance[] instances, CancellationToken cancellationToken)
    {
        var allItems = new List<BitwardenVaultItem>();
        foreach (var instance in instances)
        {
            var items = await instance.GetVaultItems(cancellationToken);
            allItems.AddRange(items);
        }

        var vaultIds = instances.Select(x => x.Id).ToArray();
        _vaultItems.RemoveAll(item => vaultIds.Contains(item.VaultId));
        _vaultItems.AddRange(allItems);
        var itemIds = _vaultItems.Select(x => x.Id).ToArray();
        _recentVaultEntries.RemoveAll(item => !itemIds.Contains(item.Id));
    }

    private async Task<BitwardenVaultItem> GetVaultItem(string id)
    {
        if (!_initialized)
            throw new ApplicationNotInitializedException();
        var item = _vaultItems.SingleOrDefault(item => item.Id == id);
        if (item == null)
            throw new KeyNotFoundException();
        var recentEntry = _recentVaultEntries.SingleOrDefault(x => x.Id == item.Id);
        if (recentEntry != null)
            _recentVaultEntries.Remove(recentEntry);
        var recentVaultEntry = new RecentVaultEntry()
        {
            Id = id,
        };
        _recentVaultEntries.Insert(0, recentVaultEntry);
        await StoreConfiguration();
        return item;
    }

    private static IEnumerable<BitwardenVaultItem> FilterSearchResults(IEnumerable<BitwardenVaultItem> results, string query)
    {
        var searchTerms = query.Split(' ');
        return results
            .Where(item => searchTerms.All(term =>
                                               item.Name.Contains(term,
                                                                  StringComparison
                                                                      .InvariantCultureIgnoreCase)
                                               || item.Username?.Contains(term,
                                                   StringComparison.InvariantCultureIgnoreCase)
                                               == true));
    }

    private static SearchResultItem[] ToSearchResultItems(IEnumerable<BitwardenVaultItem> instances)
    {
        return instances
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

    public async Task Sync()
    {
        var keys = _accounts
            .Select(account => new BitwardenInstanceKey(account.Id, account.Username, account.Secret))
            .ToArray();
        var repos = await _bitwardenInstanceRepository.Get(keys);
        await LoadVaults(repos, CancellationToken.None);
        await StoreConfiguration();
    }
}
