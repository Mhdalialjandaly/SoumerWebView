using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DataAccess.Setup
{
    public static class DependenciesInjector
    {
        public static IServiceCollection AddIInjectableDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(Assembly.GetExecutingAssembly());
            }, Assembly.GetExecutingAssembly());


            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

           

            var assembly = Assembly.GetExecutingAssembly();
            var repositoryTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
                .ToList();

            foreach (var repoType in repositoryTypes)
            {
                var interfaceType = repoType.GetInterface($"I{repoType.Name}");
                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, repoType);
                }
            }        


            return services;
        }
    }
}
