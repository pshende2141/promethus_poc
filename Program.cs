using promethus_poc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
using Prometheus;

internal class Program
{
    private static void Main(string[] args)
    {
        var host = CreateHostBuilder(args);
        var app = host.Build();
        app.Run();
        
        // IHost host = Host.CreateDefaultBuilder(args)
        //     .ConfigureServices(services =>
        //     {
        //         services.AddHostedService<Worker>();
        //     })
        //     .Build();     
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder( args)

            .ConfigureServices( (HostExecutionContext, services) =>
            {
                ConfigureServices(services);
            } )
            
            .UseWindowsService() 
            ;

        public static void ConfigureServices(IServiceCollection serviceCollection)
        {
            var server = new MetricServer(8114);
            try
            {
                server.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Exception: {ex.Message}");
            }

            serviceCollection.AddHostedService<Worker>();
            var record_processed = Metrics.CreateCounter("poc_records_counter", "Total number of records processed");
            _ = Task.Run(async delegate
               {
                   while (true)
                   {
                       Console.WriteLine($"Prometheus counter Name={record_processed.Name} and value={record_processed.Value} ");
                       record_processed.Inc();
                       await Task.Delay(10 * 1000);
                   }
               });

        }

}