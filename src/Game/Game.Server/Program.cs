using Game.Shared.Logging;
using Serilog;
using System.Net;
using Game.Shared;

namespace Game.Server;

internal class Program
{
    public static async Task Main(string[] args)
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

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                Log.Logger = LoggerProvider.CreateLogger();
                webBuilder.ConfigureKestrel(serverOptions => {

                    serverOptions.Listen(IPAddress.Any, Constants.WebSocketServerPort);

                }).UseStartup<Startup>();
            });
}