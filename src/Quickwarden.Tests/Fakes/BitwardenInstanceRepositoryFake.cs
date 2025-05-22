using Quickwarden.Application.PlugIns.Bitwarden;

namespace Quickwarden.Tests.Fakes;

public class BitwardenInstanceRepositoryFake : IBitwardenInstanceRepository
{
    public bool EnableLongDelay { get; set; }
    public List<InstanceWithCredentials> InstancesWithCredentials { get; } = [];
    public List<InstanceWithCredentials> BitwardenInstances { get; } = [];

    public async Task<BitwardenInstanceCreateResult> Create(string username,
                                                            string password,
                                                            string totp,
                                                            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException();
        if (EnableLongDelay)
        {
            var delayTask = Task.Delay(1, cancellationToken);
            await delayTask;
            if (delayTask.IsCanceled)
                throw new TaskCanceledException();
        }

        var instance = InstancesWithCredentials.SingleOrDefault(instance =>
                                                                    instance.Username == username
                                                                    && instance.Password == password);
        if (instance == null)
            return new(BitwardenInstanceCreateResultType.WrongCredentials, null);

        if (totp == string.Empty)
            return new(BitwardenInstanceCreateResultType.Missing2Fa, null);
        if (totp != instance.Totp)
            return new(BitwardenInstanceCreateResultType.WrongCredentials, null);

        BitwardenInstances.Add(instance);
        return new(BitwardenInstanceCreateResultType.Success, instance.Key);
    }

    public Task<IBitwardenInstance[]> Get(BitwardenInstanceKey[] keys)
    {
        return Task.FromResult(BitwardenInstances
                               .Where(instance => keys.Contains(instance.Key))
                               .Select(instance => instance.Instance)
                               .ToArray());
    }

    public Task Delete(BitwardenInstanceKey key)
    {
        var found = InstancesWithCredentials.Any(i => i.Key == key);
        if (found)
            BitwardenInstances.RemoveAll(i => i.Instance.Id == key.Id);
        return Task.CompletedTask;
    }
}
