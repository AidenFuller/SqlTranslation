namespace SqlTranslation;
public interface IDbColumn : ISelectable
{
    public TableRef TableRef { get; }
    public string Name { get; }
}
