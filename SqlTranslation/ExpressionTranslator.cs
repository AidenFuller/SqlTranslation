using System.Linq.Expressions;

namespace SqlTranslation;
public class ExpressionTranslator : ExpressionVisitor
{
    private readonly IDbTokenFactory _dbTokenFactory;
    private TranslationState _state;
    public ExpressionTranslator(IDbTokenFactory dbTokenFactory)
    {
        _dbTokenFactory = dbTokenFactory ?? throw new ArgumentNullException(nameof(dbTokenFactory));
        _state = new();
    }

    public IDbToken Translate(Expression expression)
    {
        Visit(expression);
        return _state.ResultStack.Pop();
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        Visit(node.Left);
        var left = _state.ResultStack.Pop();

        Visit(node.Right);
        var right = _state.ResultStack.Pop();

        var token = _dbTokenFactory.BuildBinary(node.NodeType, left, right, node.Type);

        _state.ResultStack.Push(token);
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        var value = node.Value;
        var token = _dbTokenFactory.BuildConstant(value, null, node.Type);
        _state.ResultStack.Push(token);
        return node;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        var token = new TableRef(node.Name);
        _state.ResultStack.Push(token);
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        var memberName = node.Member.Name;

        IDbToken token;
        if (node.Expression.NodeType == ExpressionType.Constant)
        {
            var value = ((ConstantExpression)node.Expression).Value;

            var field = value.GetType().GetField(memberName);
            var property = value.GetType().GetProperty(memberName);

            var rawValue = field?.GetValue(value) ?? property?.GetValue(value);
            token = _dbTokenFactory.BuildConstant(rawValue, memberName, node.Type);
        }
        else
        {
            Visit(node.Expression);
            var tableRef = _state.ResultStack.Pop();

            token = _dbTokenFactory.BuildMember(tableRef, memberName, node.Type);
        }

        _state.ResultStack.Push(token);
        return node;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        Visit(node.Operand);
        var operand = _state.ResultStack.Pop();
        var returnType = node.Type;

        var token = _dbTokenFactory.BuildUnary(node.NodeType, operand, returnType);
        _state.ResultStack.Push(token);
        return node;
    }
}
