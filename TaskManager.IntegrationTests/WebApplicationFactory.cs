using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Infrastructure.Data;

namespace TaskManager.IntegrationTests;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the actual database context registration - need to find it by implementation type
            var descriptor = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                .ToList();

            foreach (var d in descriptor)
            {
                services.Remove(d);
            }

            // Remove DbContext registration
            var dbContextDescriptor = services
                .Where(d => d.ServiceType == typeof(ApplicationDbContext))
                .ToList();

            foreach (var d in dbContextDescriptor)
            {
                services.Remove(d);
            }

            // Add in-memory database with a unique name for each test
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
            });
        });

        builder.UseEnvironment("Development");
    }
}

