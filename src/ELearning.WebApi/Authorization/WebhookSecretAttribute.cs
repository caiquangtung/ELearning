using ELearning.WebApi.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ELearning.WebApi.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class WebhookSecretAttribute(string configurationKey, string headerName) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expected = configuration[configurationKey];
        var actual = context.HttpContext.Request.Headers[headerName].ToString();

        if (!WebhookSecretValidator.IsValid(expected, actual))
            context.Result = new UnauthorizedResult();
    }
}
