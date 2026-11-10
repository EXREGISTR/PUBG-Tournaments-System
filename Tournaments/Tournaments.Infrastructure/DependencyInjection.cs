using Application.Common;
using Application.Common.Repositories;
using Application.Common.Repositories.Tournaments;
using Application.Teams;
using Application.Tournaments;
using Infrastructure.BackgroundJobs.Tournaments;
using Infrastructure.Clients;
using Infrastructure.Database;
using Infrastructure.Database.Repositories;
using Infrastructure.Database.Repositories.Tournaments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Infrastructure;

public static class DependencyInjection {
    extension(IServiceCollection services) {
        public IServiceCollection AddInfrastructure(IConfiguration configuration, string environment) {
            var connectionString = configuration.GetConnectionString("Database");
            
            if (connectionString == null) {
                throw new NullReferenceException("Database connection string is null");
            }

            services
                .AddBackgroundJobs(connectionString)
                .AddDatabase(connectionString)
                .AddUsersClient(configuration, environment);
            
            return services;
        }

        private IServiceCollection AddBackgroundJobs(string connectionString) {
            services.AddQuartz(quartz => {
                quartz.UsePersistentStore(store => {
                    store.UsePostgres(connectionString);
                    store.UseNewtonsoftJsonSerializer();
                });
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
            
            services.AddSingleton<ITournamentsScheduler, TournamentsScheduler>();
            
            return services;
        }

        private IServiceCollection AddUsersClient(IConfiguration configuration, string environment) {
            if (environment == "Development") {
                services.AddScoped<IUsersClient, MockUsersClient>();
            } else {
                var usersClientConfig = configuration.GetSection("UsersClient");
                
                services.AddHttpClient<UsersClient>(options => {
                    options.BaseAddress = new Uri(
                        usersClientConfig["BaseUrl"] 
                        ?? throw new NullReferenceException("UsersClient section has errors"));
                });
                
                services.AddScoped<IUsersClient, UsersClient>();
            }
            
            return services;
        }

        private IServiceCollection AddDatabase(string connectionString) {
            services
                .AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString))
                .AddScoped<IUnitOfWork>(x => x.GetRequiredService<AppDbContext>());
            
            services
                .AddScoped<ITournamentsRepository, TournamentsRepository>()
                .AddScoped<ITournamentsQueryRepository, TournamentsQueryRepository>()
                .AddScoped<ITournamentStagesQueryRepository, TournamentStagesQueryRepository>();

            services.AddScoped<ITeamsRepository, TeamsRepository>();
            
            services.AddScoped<IMapsRepository, MapsRepository>();
            
            return services;
        }
    }
}