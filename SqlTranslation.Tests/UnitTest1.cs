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

        Expression<Func<TestEntity, bool>> expression = x => x.TestBool || x.TestBool == false && (x.TestString ?? "TEST") == "TEST";

        var token = expressionTranslator.Translate(expression.Body);
        Assert.Equal("[x].[TestBool] = 1 OR ([x].[TestBool] = 0 AND [x].[TestString] = 'LOL')", token.ToString());
    }
}

class TestEntity
{
    public bool TestBool { get; set; }
    public int TestInt { get; set; }
    public string TestString { get; set; }
    public DateTime TestDateTime { get; set; }
}