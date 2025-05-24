using System.Text.Json.Serialization;
using Quickwarden.Application.Internal;

namespace Quickwarden.Application;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Configuration))]
[JsonSerializable(typeof(Account))]
[JsonSerializable(typeof(Account[]))]
[JsonSerializable(typeof(RecentVaultEntry))]
[JsonSerializable(typeof(RecentVaultEntry[]))]
internal partial class ApplicationJsonSerializerContext : JsonSerializerContext
{
}
