using Microsoft.AspNetCore.Mvc.Filters;

namespace Gemz.Api.Creator.Middleware
{
    public class AllowNonCreator : ActionFilterAttribute
    {
    }
}
