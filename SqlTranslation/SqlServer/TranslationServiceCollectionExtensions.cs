using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlTranslation.SqlServer;
public static class TranslationServiceCollectionExtensions
{
    public static IServiceCollection AddTranslationServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<ExpressionTranslatorFactory>()
            .AddSingleton<IDbTokenFactory, SqlServerTokenFactory>();
    }
}
