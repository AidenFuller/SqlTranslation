namespace SqlTranslation;
public interface IDbFunc : ISelectable
{
    string Name { get; }
    ISelectable[] Parameters { get; }
}
