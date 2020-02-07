using System;
using System.Threading.Tasks;
using D2M.Data;
using D2M.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace D2M.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args).Build();

            try
            {
                await MigrateDatabase(hostBuilder.Services);
                await SetupBehaviourConfigurations(hostBuilder.Services);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            await hostBuilder.RunAsync();
        }

        private static async Task MigrateDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<EntityContext>();
            await db.Database.MigrateAsync();
        }

        private static async Task SetupBehaviourConfigurations(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IBehaviourConfigurationService>();
            await service.Configure();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
