using Gemz.Api.Auth.Data;
using Gemz.Api.Auth.Middleware;
using Gemz.Api.Auth.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDataServices(builder.Configuration).AddServices(builder.Configuration);
builder.Logging.AddConsole();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthorization();
app.Use(TokenMiddleware.ExecuteAsync(app.Configuration, app.Services.GetRequiredService<HttpClient>()));
app.MapControllers();

app.Run();