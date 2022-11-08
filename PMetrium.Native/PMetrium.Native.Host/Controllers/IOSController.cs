using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PMetrium.Native.Common.Contracts;
using PMetrium.Native.Common.Helpers.Extensions;
using PMetrium.Native.IOS;
using Serilog;
using static PMetrium.Native.Common.Helpers.PlatformOSHelper;

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
    public async Task Start(
        [Required] string device,
        [Required] string applicationName,
        string? space = "default",
        string? group = "default",
        string? label = "default")
    {
        await _iosMetricsManager.Start(
            device,
            applicationName,
            space,
            group,
            label);
    }

     [HttpGet]
     public async Task<IOSPerformanceResults> Stop([Required] string device)
     {
         return await _iosMetricsManager.Stop(device);
    }
}