namespace SqlTranslation;
public interface ISelectable : IDbToken
{
    Type ReturnType { get; }
}
