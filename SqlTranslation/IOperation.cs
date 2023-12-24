namespace SqlTranslation;
public interface IOperation : ISelectable
{
    OperationType OperationType { get; }
    ISelectable[] Parameters { get; }
}

public enum OperationType
{
    Add,
    Subtract,
    Multiply,
    Divide,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    LeftShift,
    RightShift
}

public static class OperationTypeExtensions
{
    public static string ToSql(this OperationType operationType)
    {
        return operationType switch
        {
            OperationType.Add => "+",
            OperationType.Subtract => "-",
            OperationType.Multiply => "*",
            OperationType.Divide => "/",
            OperationType.BitwiseAnd => "&",
            OperationType.BitwiseOr => "|",
            OperationType.BitwiseXor => "^",
            OperationType.LeftShift => "<<",
            OperationType.RightShift => ">>",
            _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
        };
    }
}