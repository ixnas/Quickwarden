using System.Diagnostics;

namespace Quickwarden.Infrastructure.Internal;

internal static class ShellExecutor
{
    public async static Task<ShellExecuteResult> ExecuteAsync(string command,
                                                              string[] args,
                                                              Dictionary<string, string> environmentVariables)
    {
        var process = new Process();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.FileName = command;
        foreach (var environmentVariable in environmentVariables)
        {
            process.StartInfo.EnvironmentVariables.Add(environmentVariable.Key, environmentVariable.Value);
        }

        foreach (var arg in args)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        process.Start();
        var stdout = new List<string>();
        var stderr = new List<string>();

        while (process.StandardOutput.Peek() > -1)
        {
            stdout.Add(process.StandardOutput.ReadLine());
        }

        while (process.StandardError.Peek() > -1)
        {
            stderr.Add(process.StandardError.ReadLine());
        }

        await process.WaitForExitAsync();

        return new ShellExecuteResult(stdout.ToArray(), stderr.ToArray());
    }
}
