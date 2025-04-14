using System.Text.Json.Serialization;

namespace Quickwarden.Application;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Account))]
[JsonSerializable(typeof(Account[]))]
internal partial class ApplicationJsonSerializerContext : JsonSerializerContext
{
}
