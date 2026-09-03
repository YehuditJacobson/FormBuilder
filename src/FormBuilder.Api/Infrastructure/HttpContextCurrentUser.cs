using System.Security.Claims;
using FormBuilder.Application.Common;

namespace FormBuilder.Api.Infrastructure;

/// <summary>
/// Resolves the acting user from the current request: a name-identifier claim, then an
/// <c>X-User-Id</c> header, then a <c>system</c> fallback. Registered after
/// <c>AddInfrastructure</c> so it wins over the infrastructure default.
/// </summary>
internal sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private const string SystemUser = "system";

    public string Id
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context is null)
            {
                return SystemUser;
            }

            var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(claim))
            {
                return claim;
            }

            var header = context.Request.Headers["X-User-Id"].ToString();
            return string.IsNullOrWhiteSpace(header) ? SystemUser : header;
        }
    }
}
