namespace SqlTranslation.SqlServer;
public class SqlServerColumn(TableRef tableRef, string name, Type returnType) : IDbColumn
{
    public TableRef TableRef { get; } = tableRef;
    public string Name { get; } = name;
    public Type ReturnType { get; } = returnType;

    public override string ToString()
    {
        return $"[{TableRef.ReferenceName}].[{Name}]";
    }
}
