using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;

namespace SpawnCloud.ChatService.Hub.Orleans;

public static class OrleansExtensions
{
    public static WebApplicationBuilder UseOrleans(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ClusterClientHostedService>();
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<ClusterClientHostedService>());
        builder.Services.AddSingleton<IClusterClient>(sp => sp.GetRequiredService<ClusterClientHostedService>().Client);
        builder.Services.AddSingleton<IGrainFactory>(sp => sp.GetRequiredService<ClusterClientHostedService>().Client);

        return builder;
    }
}