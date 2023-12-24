namespace SqlTranslation;
public class SelectableCollection(ISelectable[] selectables) : IDbToken
{
    public ISelectable[] Selectables { get; } = selectables;

    public override string ToString()
    {
        return string.Join(", ", Selectables.Select(x => x.ToString()));
    }
}
