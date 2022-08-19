using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Serilog;
using SpawnCloud.ChatService.Grains;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((context, configuration) =>
    {
        if (context.HostingEnvironment.IsDevelopment())
        {
            configuration.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console();
        }
        else
        {
            // TODO: Determine which logging sink and configuration to use for production environments
            throw new NotImplementedException();
        }
    })
    .UseOrleans((context, siloBuilder) =>
    {
        if (context.HostingEnvironment.IsDevelopment())
        {
            siloBuilder.UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = OrleansConstants.DevClusterId;
                    options.ServiceId = OrleansConstants.DevServiceId;
                })
                .UseDashboard(options =>
                {
                    options.Port = 8787;
                });
        }
        else
        {
            // TODO: Determine which clustering method to use for production environments
            throw new NotImplementedException();
        }

        siloBuilder.ConfigureLogging(logging => logging.AddSerilog());
    })
    .Build();

await host.RunAsync();