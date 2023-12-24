namespace SqlTranslation;

public interface ILogicalCondition : ICondition
{
    ICondition[] Conditions { get; }
    LogicalType LogicalType { get; }
}

public enum LogicalType
{
    And,
    Or
}

public static class LogicalTypeExtensions
{
    public static string ToSql(this LogicalType logicalType)
    {
        return logicalType switch
        {
            LogicalType.And => "AND",
            LogicalType.Or => "OR",
            _ => throw new ArgumentOutOfRangeException(nameof(logicalType), logicalType, null)
        };
    }
}