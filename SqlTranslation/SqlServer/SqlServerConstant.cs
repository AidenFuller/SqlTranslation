using System.Globalization;

namespace SqlTranslation.SqlServer;
public class SqlServerConstant(object? value, string? parameterName, Type returnType) : IDbConstant
{
    public object? Value { get; } = value;
    public string? ParameterName { get; set; } = parameterName;
    public Type ReturnType { get; } = returnType;

    public override string ToString()
    {
        return ParameterName != null ? $"@{ParameterName}" : ValueToString();
    }

    private string ValueToString() => Value switch
    {
        null => "NULL",
        bool b => b ? "1" : "0",
        char c => c == '\'' ? "''" : $"'{c}'",
        string s => $"'{s.Replace("'", "''")}'",
        DateOnly d => $"'{d:yyyy-MM-dd}'",
        DateTime d => $"'{d:yyyy-MM-dd HH:mm:ss.fff}'",
        DateTimeOffset d => $"'{d:yyyy-MM-dd HH:mm:ss.fff zzz}'",
        decimal d => d.ToString(CultureInfo.InvariantCulture),
        double d => d.ToString(CultureInfo.InvariantCulture),
        float f => f.ToString(CultureInfo.InvariantCulture),
        Type t => t.Name,
        _ => Value.ToString() ?? "NULL",
    };
}
