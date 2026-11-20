namespace LearnSql.Sql.Execution;

public sealed class SandboxInitializationException : Exception
{
    public SandboxInitializationException(string message, Exception innerException)
        : base(message, innerException) { }
}
