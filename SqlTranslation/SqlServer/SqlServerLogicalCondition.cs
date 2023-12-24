namespace SqlTranslation.SqlServer;
public class SqlServerLogicalCondition(LogicalType logicalType, params ICondition[] conditions) : ILogicalCondition
{
    public ICondition[] Conditions { get; } = conditions;
    public LogicalType LogicalType { get; } = logicalType;

    public override string ToString()
    {
        var bracketConditions = Conditions.Select(c => c is ILogicalCondition lc && lc.LogicalType != LogicalType ? $"({c.ToString()})" : c.ToString());
        return $"{string.Join($" {LogicalType.ToSql()} ", bracketConditions)}";
    }
}
