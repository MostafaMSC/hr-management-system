using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
namespace HR.Infrastructure.Services
{
    public class TokenCleanupService : Microsoft.Extensions.Hosting.BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    }
}
