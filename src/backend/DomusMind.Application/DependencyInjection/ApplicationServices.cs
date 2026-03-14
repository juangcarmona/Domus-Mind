using Microsoft.Extensions.DependencyInjection;

namespace DomusMind.Application.DependencyInjection;

public static class ApplicationServices
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        return services;
    }
}
