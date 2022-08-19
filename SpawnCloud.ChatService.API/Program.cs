using Serilog;
using SpawnCloud.ChatService.API.Orleans;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

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
app.UseRouting();
app.MapControllers();

app.Run();