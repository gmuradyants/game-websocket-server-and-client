using Game.Shared.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Game.Client;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }
        catch (OperationCanceledException)
        {
            // skip cancelling
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Exception was thrown in the Program class");
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureServices(
                (_, services) =>
                {
                    Log.Logger = LoggerProvider.CreateLogger();
                    services.ConfigureServices();
                    services.AddHostedService<ClientHostedService>();
                }).UseConsoleLifetime();
    }
}