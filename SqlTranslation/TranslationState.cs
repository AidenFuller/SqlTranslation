namespace SqlTranslation;

public class TranslationState
{
    public Stack<IDbToken> ResultStack { get; set; } = new();
    public List<IDbConstant> Variables { get; set; } = new();
}