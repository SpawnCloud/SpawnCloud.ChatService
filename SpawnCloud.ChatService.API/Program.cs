using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Serilog;
using SpawnCloud.Authentication.Validation;
using SpawnCloud.ChatService.API.Orleans;
using SpawnCloud.Shared.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

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

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant()
            );
        }
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseApiVersioning();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();