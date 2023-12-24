
namespace SqlTranslation.SqlServer;
public class SqlServerOperation(OperationType operationType, ISelectable[] parameters, Type returnType, string? alias = null) : IOperation
{
    public OperationType OperationType { get; } = operationType;
    public ISelectable[] Parameters { get; } = parameters;
    public Type ReturnType { get; } = returnType;
    public string? Alias { get; set; } = alias;

    public override string ToString()
    {
        return string.Join($" {OperationType.ToSql()} ", Parameters.Select(x => x.ToString()));
    }
}
