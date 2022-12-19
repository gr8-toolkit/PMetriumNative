using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PMetrium.Native.Metrics.Android;
using PMetrium.Native.Metrics.Android.Contracts;

namespace PMetrium.Native.Host.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AndroidController : ControllerBase
{
    private IAndroidMetricsManager _androidPerformanceMetricsManager;
    
    public AndroidController(IAndroidMetricsManager performanceMetricsManager)
    {
        _androidPerformanceMetricsManager = performanceMetricsManager;
    }
    
    [HttpGet]
    public async Task<ActionResult<string>> Start(
        [Required] string device,
        string applicationName = "system",
        bool cpuApp = true,
        bool cpuTotal = true,
        bool ramTotal = true,
        bool ramApp = true,
        bool networkTotal = true,
        bool networkApp = true,
        bool batteryApp = true,
        bool framesApp = true,
        string? space = "default",
        string? group = "default",
        string? label = "default")
    {
        await _androidPerformanceMetricsManager.Start(
            device,
            applicationName,
            cpuApp,
            cpuTotal,
            ramTotal,
            ramApp,
            networkTotal,
            networkApp,
            batteryApp,
            framesApp,
            space,
            group,
            label);
        
        return "Started";
    }
    
    [HttpGet]
    public async Task<AndroidPerformanceResults> Stop([Required] string device)
    {
        return await _androidPerformanceMetricsManager.Stop(device);
    }
}