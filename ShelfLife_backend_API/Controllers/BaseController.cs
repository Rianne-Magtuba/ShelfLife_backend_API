using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ShellLife_backend_API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected string? GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}