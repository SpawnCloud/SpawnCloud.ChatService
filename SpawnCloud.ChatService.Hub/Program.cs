using Serilog;
using SpawnCloud.Authentication.Client;
using SpawnCloud.ChatService.Hub;
using SpawnCloud.ChatService.Hub.Hubs;
using SpawnCloud.ChatService.Web.Shared.Orleans;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Host.UseSerilog((context, configuration) =>
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
});
builder.UseOrleans();

builder.Services.AddSingleton<IHostedService, ObserverHostedService>();
builder.Services.AddSingleton<IChatObserver>(sp => sp.GetRequiredService<ObserverHostedService>());

builder.Services.AddSpawnCloudAuthClient();

var app = builder.Build();
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHttpsRedirection();
}
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();