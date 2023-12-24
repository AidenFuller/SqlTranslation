using System.Linq.Expressions;

namespace SqlTranslation;
public interface IDbTokenFactory
{
    //ILogicalCondition BuildLogicalCondition(LogicalType logicalType, params ICondition[] conditions);
    //IEqualityCondition BuildEqualityCondition(ISelectable left, EqualityType equalityType, ISelectable right);
    //IDbColumn BuildColumn(TableRef tableRef, string columnName);
    IDbToken BuildFunc(string name, ISelectable[] parameters, Type returnType);
    IDbToken BuildConstant(object? value, string? parameterName, Type returnType);
    IDbToken BuildBinary(ExpressionType expressionType, IDbToken left, IDbToken right, Type returnType);
    IDbToken BuildUnary(ExpressionType expressionType, IDbToken operand, Type returnType);
    IDbToken BuildMember(IDbToken source, string memberName, Type returnType);
    IDbToken BuildStaticMember(Type declaringType, string memberName, Type returnType);
}
