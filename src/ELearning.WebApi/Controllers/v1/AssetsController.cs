using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.WebApi.Controllers.v1;

[ApiController]
[ApiVersion(1)]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/assets")]
public sealed class AssetsController(IConfiguration configuration) : ControllerBase
{
    [HttpGet("{storageKey}")]
    public IActionResult Get(string storageKey)
    {
        var root = configuration["Storage:Local:BasePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "storage");

        var fullPath = Path.Combine(root, storageKey);
        if (!System.IO.File.Exists(fullPath))
            return NotFound();

        var contentType = "application/octet-stream";
        return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
    }
}

