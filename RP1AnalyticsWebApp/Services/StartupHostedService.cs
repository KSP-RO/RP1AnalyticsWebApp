using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RP1AnalyticsWebApp.Services
{
    public class StartupHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public StartupHostedService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var rolesTask = EnsureRolesAsync(scope);
            _ = PreloadCacheAsync(scope, cancellationToken);    // Start but do not wait for completion

            await rolesTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private static async Task EnsureRolesAsync(IServiceScope scope)
        {
            var _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<MongoRole>>();
            await Task.WhenAll(EnsureRoleAsync(_roleManager, Constants.Roles.Admin),
                               EnsureRoleAsync(_roleManager, Constants.Roles.Member));
        }

        private static async Task EnsureRoleAsync(RoleManager<MongoRole> _roleManager, string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new MongoRole(roleName));
            }
        }

        private static async Task PreloadCacheAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var cache = scope.ServiceProvider.GetRequiredService<CacheService>();
            await cache.InitCacheAsync(cancellationToken);
        }
    }
}
