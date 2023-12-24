using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTranslation;
public class ExpressionTranslatorFactory
{
    private readonly IDbTokenFactory _dbTokenFactory;

    public ExpressionTranslatorFactory(IDbTokenFactory dbTokenFactory)
    {
        _dbTokenFactory = dbTokenFactory ?? throw new ArgumentNullException(nameof(dbTokenFactory));
    }

    public ExpressionTranslator Create()
    {
        return new ExpressionTranslator(_dbTokenFactory);
    }
}
