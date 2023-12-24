namespace SqlTranslation.SqlServer;
public class SqlServerFunc(string name, ISelectable[] parameters, Type returnType) : IDbFunc
{
    public string Name { get; } = name;
    public ISelectable[] Parameters { get; } = parameters;
    public Type ReturnType { get; } = returnType;

    public override string ToString()
    {
        return $"{Name}({string.Join(", ", Parameters.Select(p => p.ToString()))})";
    }
}
