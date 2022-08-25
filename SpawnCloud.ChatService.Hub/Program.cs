using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orleans.Hosting;
using Serilog;
using SpawnCloud.Authentication.Validation;
using SpawnCloud.ChatService.Hub;
using SpawnCloud.ChatService.Hub.Orleans;
using SpawnCloud.ChatService.Hubs;
using SpawnCloud.Shared.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSpawnCloudAuthValidation();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ChatPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireScope("user.chat");
    });
});
builder.Services.AddSpawnCloudAuthorizationHandlers();
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
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
builder.Services.AddSignalR().AddOrleans();

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