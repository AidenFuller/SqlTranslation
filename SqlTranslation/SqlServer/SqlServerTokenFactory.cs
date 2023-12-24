using System.Linq.Expressions;
using System.Xml.Linq;

namespace SqlTranslation.SqlServer;
public class SqlServerTokenFactory : IDbTokenFactory
{
    public IDbToken BuildBinary(ExpressionType expressionType, IDbToken left, IDbToken right, Type returnType)
    {
        // Handle for the case that booleans enter as constants rather than conditions
        if (_logicalTypesByExpressionType.ContainsKey(expressionType))
        {
            if (left is ISelectable l && l.ReturnType == typeof(bool))
            {
                left = new SqlServerEqualityCondition(l, EqualityType.Equal, new SqlServerConstant(true, null, typeof(bool)));
            }
            if (right is ISelectable r && r.ReturnType == typeof(bool))
            {
                right = new SqlServerEqualityCondition(r, EqualityType.Equal, new SqlServerConstant(true, null, typeof(bool)));
            }
        }

        if (left is ISelectable leftSelectable && right is ISelectable rightSelectable)
        {
            if (_equalityTypesByExpressionType.TryGetValue(expressionType, out var equalityType))
            {
                if (_nullableEqualityMap.TryGetValue(equalityType, out var nullableEquality) && ((leftSelectable is IDbConstant leftConstant && leftConstant.Value == null) || (rightSelectable is IDbConstant rightConstant && rightConstant.Value == null)))
                {
                    equalityType = nullableEquality;
                }
                return new SqlServerEqualityCondition(leftSelectable, equalityType, rightSelectable);
            }
            else if (_operationTypesByExpressionType.TryGetValue(expressionType, out var operationType))
            {
                return new SqlServerOperation(operationType, [leftSelectable, rightSelectable], returnType);
            }
            else if (_funcNamesByExpressionType.TryGetValue(expressionType, out var funcName))
            {
                return Compress(funcName, leftSelectable, rightSelectable, returnType);
            }
        }
        else if (left is ICondition leftCondition && right is ICondition rightCondition)
        {
            if (_logicalTypesByExpressionType.TryGetValue(expressionType, out var logicalType))
            {
                return new SqlServerLogicalCondition(logicalType, leftCondition, rightCondition);
            }
        }

        throw new NotSupportedException($"Expression type {expressionType} is not supported as a Binary Operation");
    }

    public IDbToken BuildConstant(object? value, string? parameterName, Type returnType)
    {
        return new SqlServerConstant(value, parameterName, returnType);
    }

    public IDbToken BuildMember(IDbToken source, string memberName, Type returnType)
    {
        if (source is TableRef tableRef)
        {
            return new SqlServerColumn(tableRef, memberName, returnType);
        }

        throw new NotSupportedException($"Member expression only supports referring to a table column or runtime variable");        
    }

    public IDbToken BuildStaticMember(Type declaringType, string memberName, Type returnType)
    {
        if (declaringType == typeof(string) && memberName == nameof(string.Empty))
        {
            return new SqlServerConstant(string.Empty, null, returnType);
        }
        else if (declaringType == typeof(DateTime) && memberName == nameof(DateTime.Now))
        {
            return new SqlServerFunc("GETDATE", [], returnType);
        }
        else if (declaringType == typeof(DateTimeOffset) && memberName == nameof(DateTimeOffset.Now))
        {
            return new SqlServerFunc("SYSDATETIMEOFFSET", [], returnType);
        }

        throw new NotSupportedException($"Member expression of {declaringType.FullName}.{memberName} is not supported");
    }

    public IDbToken BuildUnary(ExpressionType expressionType, IDbToken operand, Type returnType)
    {
        if (operand is ISelectable selectable)
        {
            if (expressionType == ExpressionType.IsTrue)
            {
                var trueConstant = new SqlServerConstant(true, null, typeof(bool));
                return new SqlServerEqualityCondition(selectable, EqualityType.Equal, trueConstant);
            }
            else if (expressionType == ExpressionType.IsFalse)
            {
                var falseConstant = new SqlServerConstant(false, null, typeof(bool));
                return new SqlServerEqualityCondition(selectable, EqualityType.Equal, falseConstant);
            }
            else if (expressionType == ExpressionType.Convert)
            {
                var typeConstant = new SqlServerConstant(returnType, null, returnType);
                return new SqlServerFunc("CONVERT", [typeConstant, selectable], returnType);
            }
        }
        throw new NotSupportedException($"Unary expression type {expressionType} is not supported.");
    }

    public IDbToken BuildFunc(string funcName, ISelectable[] parameters, Type returnType)
    {
        return new SqlServerFunc(funcName, parameters, returnType);
    }

    private static readonly Dictionary<ExpressionType, EqualityType> _equalityTypesByExpressionType = new()
    {
        [ExpressionType.Equal] = EqualityType.Equal,
        [ExpressionType.NotEqual] = EqualityType.NotEqual,
        [ExpressionType.GreaterThan] = EqualityType.GreaterThan,
        [ExpressionType.GreaterThanOrEqual] = EqualityType.GreaterThanOrEqual,
        [ExpressionType.LessThan] = EqualityType.LessThan,
        [ExpressionType.LessThanOrEqual] = EqualityType.LessThanOrEqual
    };

    private static readonly Dictionary<ExpressionType, LogicalType> _logicalTypesByExpressionType = new()
    {
        [ExpressionType.AndAlso] = LogicalType.And,
        [ExpressionType.OrElse] = LogicalType.Or,
    };

    private static readonly Dictionary<EqualityType, EqualityType> _nullableEqualityMap = new()
    {
        [EqualityType.Equal] = EqualityType.Is,
        [EqualityType.NotEqual] = EqualityType.IsNot,
    };

    private static readonly Dictionary<ExpressionType, OperationType> _operationTypesByExpressionType = new()
    {
        [ExpressionType.Add] = OperationType.Add,
        [ExpressionType.Subtract] = OperationType.Subtract,
        [ExpressionType.Multiply] = OperationType.Multiply,
        [ExpressionType.Divide] = OperationType.Divide,
        [ExpressionType.And] = OperationType.BitwiseAnd,
        [ExpressionType.Or] = OperationType.BitwiseOr,
        [ExpressionType.ExclusiveOr] = OperationType.BitwiseXor,
        [ExpressionType.LeftShift] = OperationType.LeftShift,
        [ExpressionType.RightShift] = OperationType.RightShift,
    };

    private static readonly Dictionary<ExpressionType, string> _funcNamesByExpressionType = new()
    {
        [ExpressionType.Modulo] = "MOD",
        [ExpressionType.Coalesce] = "ISNULL",
        [ExpressionType.Convert] = "CONVERT"
    };

    private static SqlServerFunc Compress(string parentMethodName, ISelectable left, ISelectable right, Type returnType)
    {
        var coalescenceFuncs = new[] { "ISNULL", "COALESCE" };
        var parameters = new List<ISelectable>();
        if (coalescenceFuncs.Contains(parentMethodName))
        {
            var compressed = false;
            if (left is IDbFunc lFunc && coalescenceFuncs.Contains(lFunc.Name))
            {
                parameters.AddRange(lFunc.Parameters);
                compressed = true;
            }
            else
            {
                parameters.Add(left);
            }
            if (right is IDbFunc rFunc && coalescenceFuncs.Contains(rFunc.Name))
            {
                parameters.AddRange(rFunc.Parameters);
                compressed = true;
            }
            else
            {
                parameters.Add(right);
            }

            var coalesce = compressed ? "COALESCE" : "ISNULL";
            return new SqlServerFunc(coalesce, parameters.ToArray(), returnType);
        }
        return new SqlServerFunc(parentMethodName, [left, right], returnType);
    }
}
