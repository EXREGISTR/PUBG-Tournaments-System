using Microsoft.Extensions.DependencyInjection;

namespace GlobalExceptionHandler;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services) 
        => services.AddExceptionHandler<GlobalExceptionHandler>();
}