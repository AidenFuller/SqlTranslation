namespace SqlTranslation;

public interface IEqualityCondition : ICondition
{
    ISelectable Left { get; }
    ISelectable Right { get; }
    EqualityType EqualityType { get; }
}

public enum EqualityType
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Like,
    NotLike,
    In,
    NotIn,
    Between,
    NotBetween
}

public static class EqualityTypeExtensions
{
    public static string ToSql(this EqualityType equalityType)
    {
        return equalityType switch
        {
            EqualityType.Equal => "=",
            EqualityType.NotEqual => "<>",
            EqualityType.GreaterThan => ">",
            EqualityType.GreaterThanOrEqual => ">=",
            EqualityType.LessThan => "<",
            EqualityType.LessThanOrEqual => "<=",
            EqualityType.Like => "LIKE",
            EqualityType.NotLike => "NOT LIKE",
            EqualityType.In => "IN",
            EqualityType.NotIn => "NOT IN",
            EqualityType.Between => "BETWEEN",
            EqualityType.NotBetween => "NOT BETWEEN",
            _ => throw new NotImplementedException(),
        };
    }
}