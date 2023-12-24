namespace SqlTranslation;
public interface IDbToken
{
    string ToString();
}

public static class IDbTokenExtensions
{
    public static ISelectable AsSelectable(this IDbToken token)
    {
        if (token is ISelectable selectable)
            return selectable;
        throw new ArgumentException($"The token {token} is not a selectable.");
    }

    public static ICondition AsCondition(this IDbToken token)
    {
        if (token is ICondition condition)
            return condition;
        throw new ArgumentException($"The token {token} is not a condition.");
    }
}