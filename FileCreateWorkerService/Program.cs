using FileCreateWorkerService;
using FileCreateWorkerService.Models;
using FileCreateWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configuration
        var configuration = builder.Configuration;

        // DbContext
        builder.Services.AddDbContext<AdventureWorks2019Context>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("SqlServer")
            );
        });

        // RabbitMQ
        builder.Services.AddSingleton(sp => new ConnectionFactory
        {
            Uri = new Uri(configuration.GetConnectionString("RabbitMQ")),
            //DispatchConsumersAsync = true // varsa paket sürümü destekliyorsa
        });

        builder.Services.AddSingleton<RabbitMQClientService>();

        // Worker
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}
