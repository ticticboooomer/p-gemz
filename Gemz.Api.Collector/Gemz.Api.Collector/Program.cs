using Gemz.Api.Collector.Data;
using Gemz.Api.Collector.Middleware;
using Gemz.Api.Collector.Service;
using Gemz.Api.Collector.Services;
using Gemz.ServiceBus;
using MassTransit;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDataServices(builder.Configuration).AddServices(builder.Configuration).AddServiceBusServices(builder.Configuration);
builder.Services.AddHttpClient();
builder.Logging.AddConsole();
builder.Services.AddMassTransit(x =>
{
    x.UsingAzureServiceBus((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["ServiceBus:ConnectionString"]);
        cfg.ConfigureEndpoints(ctx);
    });

    if (builder.Configuration.GetValue<bool>("ServiceBus:ListenToStripeCollectorQueue"))
    {
        x.AddConsumer<StripeConsumer, StripeConsumerDefinition>();
    }
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.UseAuthorization();

app.Use(TokenMiddleware.ExecuteAsync(app.Configuration, app.Services.GetRequiredService<HttpClient>()));
app.MapControllers();

app.Run();