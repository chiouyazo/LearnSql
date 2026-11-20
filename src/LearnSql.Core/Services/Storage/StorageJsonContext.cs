using System.Text.Json.Serialization;
using LearnSql.Core.Services.Exercises;

namespace LearnSql.Core.Services.Storage;

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(ExerciseProgressDocument))]
internal sealed partial class StorageJsonContext : JsonSerializerContext;
