using Quickwarden.Application.PlugIns.Bitwarden;
using Quickwarden.Infrastructure.Internal;

namespace Quickwarden.Infrastructure;

public class BitwardenInstanceRepository : IBitwardenInstanceRepository
{
    public async Task<BitwardenInstanceCreateResult> Create(string username,
                                                            string password,
                                                            string totp,
                                                            CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid().ToString();
        var vaultPath = Path.Join(QuickwardenEnvironment.VaultsPath, id);
        Directory.CreateDirectory(vaultPath);

        try
        {
            var env = new Dictionary<string, string>()
            {
                ["BITWARDENCLI_APPDATA_DIR"] = vaultPath,
                ["BW_NOINTERACTION"] = "true"
            };
            var cmd = "bw";
            var args = GetArgs(username, password, totp);

            var loginResult = await ShellExecutor.ExecuteAsync(cmd, args, env);
            var result = GetCreateResult(id, username, loginResult);
            if (result.ResultType != BitwardenInstanceCreateResultType.Success)
                Directory.Delete(vaultPath, true);

            return result;
        }
        catch (Exception)
        {
            Directory.Delete(vaultPath, true);
            throw;
        }
    }

    private BitwardenInstanceCreateResult GetCreateResult(string id,
                                                          string username,
                                                          ShellExecuteResult loginResult)
    {
        if (loginResult.StdOutLines.Length == 1)
        {
            var secret = loginResult.StdOutLines[0];
            return new BitwardenInstanceCreateResult(BitwardenInstanceCreateResultType.Success,
                                                     new BitwardenInstanceKey(id, username, secret));
        }

        if (loginResult.StdErrLines.Contains("Email address is invalid.")
            || loginResult.StdErrLines.Contains("Username or password is incorrect. Try again.")
            || loginResult.StdErrLines.Contains("Two-step token is invalid. Try again."))
            return new BitwardenInstanceCreateResult(BitwardenInstanceCreateResultType.WrongCredentials,
                                                     null);

        if (loginResult.StdErrLines.Contains("Login failed. No provider selected.")
            || loginResult.StdErrLines.Contains("Code is required."))
            return new BitwardenInstanceCreateResult(BitwardenInstanceCreateResultType.Missing2Fa,
                                                     null);

        throw new BitwardenCliError(loginResult.StdErrLines, loginResult.StdOutLines);
    }

    private string[] GetArgs(string username, string password, string totp)
    {
        if (string.IsNullOrWhiteSpace(totp))
            return ["login", "--raw", username, password];

        return ["login", "--raw", username, password, "--method", "0", "--code", totp];
    }

    public Task<IBitwardenInstance[]> Get(BitwardenInstanceKey[] keys)
    {
        List<IBitwardenInstance> instances = [];

        foreach (var key in keys)
        {
            var vaultDirectory = Path.Join(QuickwardenEnvironment.VaultsPath, key.Id);
            if (Directory.Exists(vaultDirectory))
                instances.Add(new BitwardenInstance(key));
        }

        return Task.FromResult(instances.ToArray());
    }

    public async Task Delete(BitwardenInstanceKey key)
    {
        var vaultDirectory = Path.Join(QuickwardenEnvironment.VaultsPath, key.Id);
        var env = new Dictionary<string, string>()
        {
            ["BITWARDENCLI_APPDATA_DIR"] = vaultDirectory,
            ["BW_NOINTERACTION"] = "true",
            ["BW_SESSION"] = key.Secret,
        };
        var cmd = "bw";
        string[] args = ["logout"];
        await ShellExecutor.ExecuteAsync(cmd, args, env);
        Directory.Delete(vaultDirectory, true);
    }
}

internal class BitwardenCliError : Exception
{
    public BitwardenCliError(string[] stdErrLines, string[] stdOutLines)
        : base("Bitwarden CLI returned an unexpected error:\r\n"
               + string.Join("\r\n", stdErrLines)
               + ((stdOutLines.Length > 0 && !string.IsNullOrWhiteSpace(stdOutLines[0])
                       ? "\r\n\r\nConsole output:\r\n" + string.Join("\r\n", stdOutLines)
                       : string.Empty)))
    {
    }
}
