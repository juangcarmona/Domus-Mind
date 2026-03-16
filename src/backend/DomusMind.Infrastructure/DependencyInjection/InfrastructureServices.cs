using DomusMind.Application.Abstractions.Languages;
using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Persistence;
using DomusMind.Infrastructure.Events;
using DomusMind.Infrastructure.Languages;
using DomusMind.Infrastructure.Messaging;
using DomusMind.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DomusMind.Infrastructure.DependencyInjection;

public static class InfrastructureServices
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DomusMindDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("domusmind")));

        services.AddScoped<IDomusMindDbContext>(sp =>
            sp.GetRequiredService<DomusMindDbContext>());

        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IEventLogWriter, EventLogWriter>();

        services.AddScoped<ISupportedLanguageReader, SupportedLanguageReader>();

        return services;
    }
}