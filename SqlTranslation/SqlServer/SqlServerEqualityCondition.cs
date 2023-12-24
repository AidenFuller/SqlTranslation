namespace SqlTranslation.SqlServer;
public class SqlServerEqualityCondition(ISelectable left, EqualityType equalityType, ISelectable right) : IEqualityCondition
{
    public ISelectable Left { get; } = left;
    public EqualityType EqualityType { get; } = equalityType;
    public ISelectable Right { get; } = right;

    public override string ToString()
    {
        return $"{Left.ToString()} {EqualityType.ToSql()} {Right.ToString()}";
    }
}
