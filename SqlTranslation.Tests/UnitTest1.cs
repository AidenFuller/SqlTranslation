using SqlTranslation.SqlServer;
using System.Linq.Expressions;

namespace SqlTranslation.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var tokenFactory = new SqlServerTokenFactory();
        var expressionTranslator = new ExpressionTranslator(tokenFactory);

        var repo = new Repository(expressionTranslator);

        repo.Select(x => new { x.TestString, T1 = x.TestInt + 1 })
            .Where(x => x.TestString == null);

        var str = repo.ToString();

        Assert.Empty(str);
    }
}

class Repository(ExpressionTranslator translator)
{
    private readonly ExpressionTranslator _translator = translator;
    private IDbToken? _selection = null;
    private IDbToken? _filter = null;

    public Repository Select(Expression<Func<TestEntity, object>> expression)
    {
        var token = _translator.Translate(expression.Body, false);
        if (token is not ISelectable && token is not SelectableCollection)
        {
            throw new InvalidOperationException();
        }

        _selection = token;

        return this;
    }

    public Repository Where(Expression<Func<TestEntity, bool>> expression)
    {
        var token = _translator.Translate(expression.Body, true);
        _filter = token;
        return this;
    }

    public override string ToString()
    {
        return $"SELECT {_selection} FROM TEST WHERE {_filter}";
    }
}

class TestEntity
{
    public bool TestBool { get; set; }
    public int TestInt { get; set; }
    public string TestString { get; set; }
    public DateTime TestDateTime { get; set; }
}