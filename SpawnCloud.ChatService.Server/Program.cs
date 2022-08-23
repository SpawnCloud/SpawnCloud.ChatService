using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Serilog;
using SpawnCloud.ChatService.Grains;
using SpawnCloud.ChatService.Hubs;
using SpawnCloud.ChatService.Server;
using SpawnCloud.ChatService.Server.Grains;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSignalRCore().AddOrleans();
    })
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
        siloBuilder.UseSignalR(signalrSiloBuilder =>
            {
                signalrSiloBuilder.UseFireAndForgetDelivery = true;

                signalrSiloBuilder.Configure((builder, config) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddMemoryGrainStorage(config.StorageProvider);
                        builder.AddMemoryGrainStorage(config.PubSubProvider);
                    }
                    else
                    {
                        // TODO: Determine which grain storage method to use for production environments
                        throw new NotImplementedException();
                    }
                });
            })
            .RegisterHub<ChatHub>();
        
        if (context.HostingEnvironment.IsDevelopment())
        {
            siloBuilder.UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = OrleansConstants.DevClusterId;
                    options.ServiceId = OrleansConstants.DevServiceId;
                })
                .AddMemoryGrainStorage(Constants.ChatGrainStorage)
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
        siloBuilder.ConfigureApplicationParts(parts => parts.AddFrameworkPart(typeof(ChatUserGrain).Assembly).WithReferences());
    })
    .Build();

await host.RunAsync();