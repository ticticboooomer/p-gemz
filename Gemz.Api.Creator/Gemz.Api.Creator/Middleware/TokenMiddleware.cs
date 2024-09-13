using System.Net;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Gemz.Api.Creator.Middleware;

public class TokenMiddleware
{
    public static Func<HttpContext, RequestDelegate, Task> ExecuteAsync(IConfiguration config, HttpClient httpClient)
    {
        return async (context, next) =>
        {
            var checkUnauthorizedAttribute = context.GetEndpoint()?.Metadata.OfType<AllowUnauthorizedAttribute>();
            if (checkUnauthorizedAttribute != null && checkUnauthorizedAttribute.Any())
            {
                await next(context);
                return;
            }

            var hasAllowNonCreatorAttribute = false;
            var checkAllowNonCreatorAttribute = context.GetEndpoint()?.Metadata.OfType<AllowNonCreator>();
            if (checkAllowNonCreatorAttribute != null && checkAllowNonCreatorAttribute.Any())
            {
                hasAllowNonCreatorAttribute = true;

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

            if (token.Claims.First(x => x.Type == "iscr").Value.ToLower() == "false" && !hasAllowNonCreatorAttribute)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            if (token.Claims.First(x => x.Type == "resstatus").Value == "2")
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            var endPointAttribute = context.GetEndpoint()?.Metadata.OfType<LimitedAccessAttribute>();
            if (endPointAttribute != null && endPointAttribute.Any())
            {
                if (token.Claims.First(c => c.Type == "resstatus").Value == "1" ||
                    (token.Claims.First(c => c.Type == "obstatus").Value != "2"))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("accessdenied");
                    return;
                }
            }

            context.Items["Account"] = token.Subject;
            await next(context);
        };
    }
}