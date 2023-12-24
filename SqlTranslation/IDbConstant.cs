namespace SqlTranslation;
public interface IDbConstant : ISelectable
{
    object? Value { get; }
    string? ParameterName { get; set; }
}
