using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using PMetrium.Native.IOS;
using PMetrium.Native.IOS.Contracts;

namespace PMetrium.Native.Host.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class IOSController : ControllerBase
{
    private IIOSMetricsManager _iosMetricsManager;

    public IOSController(IIOSMetricsManager iosMetricsManager)
    {
        _iosMetricsManager = iosMetricsManager;
    }

    [HttpGet]
    public async Task<ActionResult<string>> Start(
        [Required] string device,
        string applicationName,
        string? space = "default",
        string? group = "default",
        string? label = "default")
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return NotFound("PMetrium Native supports IOS only on OSX platforms");
        
        await _iosMetricsManager.Start(
            device,
            applicationName,
            space,
            group,
            label);

        return "Started";
    }

    [HttpGet]
    public async Task<IOSPerformanceResults> Stop([Required] string device)
    {
        return await _iosMetricsManager.Stop(device);
    }
}