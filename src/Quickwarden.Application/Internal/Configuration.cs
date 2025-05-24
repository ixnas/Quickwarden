namespace Quickwarden.Application.Internal;

internal record Configuration()
{
    public int Version { get; init; }
    public Account[] Accounts { get; init; } = [];
    public RecentVaultEntry[] RecentVaultEntries { get; init; } = [];
}

internal record RecentVaultEntry
{
    public required string Id { get; init; }
}