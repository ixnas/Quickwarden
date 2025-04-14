namespace Quickwarden.Application.PlugIns.Bitwarden;

public interface IBitwardenInstanceRepository
{
    Task<BitwardenInstanceCreateResult> Create(string username, string password, string totp, CancellationToken cancellationToken);
    Task<IBitwardenInstance[]> Get(BitwardenInstanceKey[] keys);
    Task Delete(string id);
}

public record BitwardenInstanceKey(string Id, string Username, string Secret);
public record BitwardenInstanceCreateResult(BitwardenInstanceCreateResultType ResultType, BitwardenInstanceKey? Key);

public enum BitwardenInstanceCreateResultType
{
    Success,
    WrongCredentials,
    Missing2Fa
}