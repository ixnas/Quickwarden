using Quickwarden.Application;
using Quickwarden.Application.PlugIns.Bitwarden;
using Quickwarden.Infrastructure;
using Quickwarden.Tests.Fakes;

namespace Quickwarden.Tests;

public class ApplicationFixture
{
    public StaticClockFake StaticClock { get; }
    public SecretRepositoryFake SecretRepository { get; }
    public BitwardenInstanceRepositoryFake BitwardenInstanceRepository { get; }
    public BinaryConfigurationRepositoryFake BinaryConfigurationRepository { get; }
    public QuickwardenEnvironmentFake QuickwardenEnvironment { get; }

    public ApplicationFixture()
    {
        StaticClock = new StaticClockFake();
        SecretRepository = new SecretRepositoryFake();
        BitwardenInstanceRepository = new BitwardenInstanceRepositoryFake();
        BinaryConfigurationRepository = new BinaryConfigurationRepositoryFake();
        QuickwardenEnvironment = new QuickwardenEnvironmentFake();
        AddDefaultUsersToRepo();
    }

    public ApplicationController CreateApplicationController()
    {
        return new ApplicationController(SecretRepository,
                                         BitwardenInstanceRepository,
                                         BinaryConfigurationRepository,
                                         new TotpGenerator(StaticClock),
                                         QuickwardenEnvironment);
    }

    private void AddDefaultUsersToRepo()
    {
        var key = new BitwardenInstanceKey("id1", "sjoerd", "secret1");
        var instance = new BitwardenInstanceFake()
        {
            Id = key.Id,
            Username = key.Username,
        };
        var instanceWithCredentials = new InstanceWithCredentials("sjoerd",
                                                                  " pass",
                                                                  "237489",
                                                                  instance,
                                                                  key);
        ((BitwardenInstanceFake)instanceWithCredentials.Instance).VaultItems.Add(new BitwardenVaultItem()
        {
            Id = "234978",
            Name = "Vault entry 1",
            Username = "sjoerd@entry1site.com",
            Password = "password1",
            Totp = "W5WQ W3P4 3M3I 2A6M 4SSD 4SM2 SJT6 OZZH 3ASR ZURK 24JR AYU5 WSKA",
            Notes = "Secret notes!",
            VaultId = "id1",
        });

        ((BitwardenInstanceFake)instanceWithCredentials.Instance).VaultItems.Add(new BitwardenVaultItem()
        {
            Id = "483938",
            Name = "NoPass",
            Username = "sjoerd@nopass.com",
            VaultId = "id1",
        });

        ((BitwardenInstanceFake)instanceWithCredentials.Instance).VaultItems.Add(new BitwardenVaultItem()
        {
            Id = "348948",
            Name = "NoUser",
            Password = "password3",
            VaultId = "id1",
        });

        BitwardenInstanceRepository.InstancesWithCredentials.Add(instanceWithCredentials);

        var key2 = new BitwardenInstanceKey("id2", "hannie", "secret2");

        var instance2 = new BitwardenInstanceFake()
        {
            Id = key2.Id,
            Username = key2.Username,
        };

        var instanceWithCredentials2 = new InstanceWithCredentials("hannie",
                                                                   "pass2",
                                                                   "473829",
                                                                   instance2,
                                                                   key2);
        ((BitwardenInstanceFake)instanceWithCredentials2.Instance).VaultItems.Add(new BitwardenVaultItem()
        {
            Id = "23847837",
            Name = "Vault entry 2",
            Username = "hannie@entry2site.com",
            Password = "password2",
            VaultId = "id2",
        });

        BitwardenInstanceRepository.InstancesWithCredentials.Add(instanceWithCredentials2);
    }

    public async Task SignInAccount1(ApplicationController controller)
    {
        await controller.SignIn("sjoerd",
                                                   " pass",
                                                   "237489",
                                                   CancellationToken.None);
    }

    public async Task SignInAccount2(ApplicationController controller)
    {
        await controller.SignIn("hannie",
                                                   "pass2",
                                                   "473829",
                                                   CancellationToken.None);
    }
}

public class QuickwardenEnvironmentFake : IQuickwardenEnvironment
{
    public bool Initialized { get; private set; }
    public bool TestBitwardenCliInstalled { get; set; } = true;

    public Task Initialize()
    {
        Initialized = true;
        return Task.CompletedTask;
    }

    public Task<bool> BitwardenCliInstalled()
    {
        return Task.FromResult(TestBitwardenCliInstalled);
    }
}
