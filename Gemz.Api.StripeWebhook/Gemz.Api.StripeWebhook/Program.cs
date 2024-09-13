using Gemz.Api.StripeWebhook.Service;
using Gemz.ServiceBus;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddServices(builder.Configuration).AddServices(builder.Configuration)
    .AddServiceBusServices(builder.Configuration);
builder.Logging.AddConsole();
builder.Services.AddMassTransit(x =>
{
    x.UsingAzureServiceBus((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["ServiceBus:ConnectionString"]);
        cfg.ConfigureEndpoints(ctx);
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthorization();

app.MapControllers();

app.Run();
