using System.Net;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Gemz.Api.Auth.Middleware;

public class TokenMiddleware
{
    public static Func<HttpContext, RequestDelegate, Task> ExecuteAsync(IConfiguration config, HttpClient httpClient)
    {
        return async (context, next) =>
        {
            var check = context.GetEndpoint()?.Metadata.OfType<AllowUnauthorizedAttribute>();
            if (check != null && check.Any())
            {
                await next(context);
                return;
            }
            var header = context.Request.Headers["Authorization"].FirstOrDefault();
            if (header is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
            
            var jwt = header.Split(" ").Skip(1).FirstOrDefault();

            if (jwt is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var validateEndpoint = config.GetValue<string>("ValidateEndpoint");
            var response = await httpClient.PostAsync($"{validateEndpoint}?token={jwt}", new StringContent(""));
            if (!response.IsSuccessStatusCode)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("Forbidden");
                return;
            }

            var handler = new JsonWebTokenHandler();
            var token = handler.ReadJsonWebToken(jwt);
            context.Items["Account"] = token.Subject;
            await next(context);
        };
    }
}