using System.Text.Json.Serialization.Metadata;

namespace LearnSql.Core.Services.Storage;

public sealed record StorageKey<T>(string Name, JsonTypeInfo<T> TypeInfo);
