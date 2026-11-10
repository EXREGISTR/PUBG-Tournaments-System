using Application.Maps;
using Application.Teams;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection {
    public static IServiceCollection AddApplication(this IServiceCollection services) {
        services
            .AddScoped<TeamsService>()
            .AddScoped<MapsService>()
            .AddMediatR(x => x.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        return services;
    }
}