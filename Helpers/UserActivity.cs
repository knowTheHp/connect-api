using System;
using System.Security.Claims;
using System.Threading.Tasks;
using connect_api.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace connect_api.Helpers
{
    public class UserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var actionExecutedContext = await next();
            var userId = int.Parse(actionExecutedContext.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var connectRepo = actionExecutedContext.HttpContext.RequestServices.GetService<IConnectRepository>();
            var user = await connectRepo.GetUser(userId);
            user.LastActive = DateTime.Now;
            await connectRepo.SaveAll();
        }
    }
}