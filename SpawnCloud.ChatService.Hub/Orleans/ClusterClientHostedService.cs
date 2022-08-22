using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Serilog;
using SpawnCloud.ChatService.Grains;

namespace SpawnCloud.ChatService.Hub.Orleans;

public class ClusterClientHostedService : IHostedService
{
    public IClusterClient Client { get; }

    public ClusterClientHostedService(IHostEnvironment hostEnvironment)
    {
        var builder = new ClientBuilder();

        if (hostEnvironment.IsDevelopment())
        {
            builder.UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = OrleansConstants.DevClusterId;
                    options.ServiceId = OrleansConstants.DevServiceId;
                })
                .AddSimpleMessageStreamProvider("chat");
        }
        else
        {
            // TODO: Determine which clustering method to use for production environments
            throw new NotImplementedException();
        }

        builder.ConfigureLogging(logging => logging.AddSerilog());
        builder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IChatUserGrain).Assembly).WithReferences());
        builder.UseSignalR();
        Client = builder.Build();
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var retryCounter = 30;
        await Client.Connect(async exception =>
        {
            if (retryCounter-- <= 0)
                return false;
            
            await Task.Delay(1000, cancellationToken);
            return true;
        });
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Client.Close();
        Client.Dispose();
    }
}