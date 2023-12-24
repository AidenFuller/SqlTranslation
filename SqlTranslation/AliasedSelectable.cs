namespace SqlTranslation;
public class AliasedSelectable(ISelectable selectable, string? alias) : ISelectable
{
    public string? Alias { get; } = alias;
    public ISelectable Selectable { get; } = selectable;
    public Type ReturnType => Selectable.ReturnType;

    public override string ToString()
    {
        return !string.IsNullOrEmpty(Alias) ? $"{Selectable.ToString()} AS {Alias}" : Selectable.ToString();
    }
}
