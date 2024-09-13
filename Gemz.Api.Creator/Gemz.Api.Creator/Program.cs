using Gemz.Api.Creator.Data;
using Gemz.Api.Creator.Middleware;
using Gemz.Api.Creator.Service;
using Gemz.Api.Creator.Services;
using Gemz.ServiceBus;
using MassTransit;
using MassTransit.Configuration;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddTransient<OverlayWsService>();
builder.Services.AddDataServices(builder.Configuration).AddServices(builder.Configuration)
    .AddServiceBus(builder.Configuration);
builder.Services.AddHttpClient();
builder.Logging.AddConsole();
builder.Services.AddSingleton<NotifyHostedService>();
builder.Services.AddHostedService(p => p.GetService<NotifyHostedService>());
builder.Services.AddMassTransit(x =>
{
    x.UsingAzureServiceBus((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["ServiceBus:ConnectionString"]);
        cfg.ConfigureEndpoints(ctx);
    });
    if (builder.Configuration.GetValue<bool>("ServiceBus:ListenToNotifyOrderQueue"))
    {
        x.RegisterConsumer<NotifyConsumer, NotifyConsumerDefinition>();
    }

    if (builder.Configuration.GetValue<bool>("ServiceBus:ListenToStripeCreatorQueue"))
    {
        x.RegisterConsumer<StripeAccountConsumer, StripeAccountConsumerDefinition>();
    }
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var wsOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(20)
};
wsOptions.AllowedOrigins.Add(app.Configuration.GetValue<string>("WebSocket:AllowedOrigin"));
app.UseWebSockets(wsOptions);
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using (var scope = app.Services.CreateScope())
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var service = scope.ServiceProvider.GetRequiredService<OverlayWsService>();
                var tcs = new TaskCompletionSource();
                await service.AwaitAuth(webSocket, tcs);
                await tcs.Task;
            }
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }
});
app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthorization();
app.Use(TokenMiddleware.ExecuteAsync(app.Configuration, app.Services.GetRequiredService<HttpClient>()));
app.MapControllers();
app.Run();