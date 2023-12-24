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
        var name = node.Name;
        if (name == null) return node;

        var token = new TableRef(name);
        _state.ResultStack.Push(token);
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        var memberName = node.Member.Name;

        IDbToken? token = null;
        
        if (node.Expression == null) // Static member
        {
            var declaringType = node.Member.DeclaringType!;
            token = _dbTokenFactory.BuildStaticMember(declaringType, memberName, node.Type);
        }
        else if (node.Expression.NodeType == ExpressionType.Constant)
        {
            var value = ((ConstantExpression)node.Expression).Value;

            var field = value?.GetType().GetField(memberName);
            var property = value?.GetType().GetProperty(memberName);

            var rawValue = field?.GetValue(value) ?? property?.GetValue(value);
            token = _dbTokenFactory.BuildConstant(rawValue, memberName, node.Type);
        }
        else
        {
            Visit(node.Expression);
            var tableRef = _state.ResultStack.Pop();

            token = _dbTokenFactory.BuildMember(tableRef, memberName, node.Type);
        }

        if (token != null)
        {
            _state.ResultStack.Push(token);
            return node;
        }
        throw new NotSupportedException("Member expression of this kind is not supported");
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

    protected override Expression VisitNew(NewExpression node)
    {
        var isAnonymous = node.Type.Name.StartsWith("<>f__AnonymousType");

        if (isAnonymous)
        {
            var selectables = new List<ISelectable>();

            var memberValuePairs = node.Members!.Zip(node.Arguments);
            foreach (var (member, value) in memberValuePairs)
            {
                var alias = member.Name;
                Visit(value);
                var selectable = (ISelectable)_state.ResultStack.Pop();

                selectables.Add(new AliasedSelectable(selectable, alias));
            }

            var token = new SelectableCollection(selectables.ToArray());
            _state.ResultStack.Push(token);
            return node;
        }

        throw new NotSupportedException("New expression of this kind is not supported");
    }
}
