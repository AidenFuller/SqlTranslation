namespace SqlTranslation;
public class TableRef : IDbToken
{
    public string ReferenceName { get; }
    public TableRef(string referenceName)
    {
        ReferenceName = referenceName;
    }

    public override string ToString()
    {
        return ReferenceName;
    }
}
