using Orleans;

namespace SpawnCloud.ChatService.Hub.Orleans;

internal static class OrleansExtensions
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