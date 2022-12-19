using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using PMetrium.Native.Common.Helpers.Extensions;
using static PMetrium.Native.Common.Helpers.PlatformOSHelper;

namespace PMetrium.Native.Host.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> Android()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        source.CancelAfter(TimeSpan.FromSeconds(10));

        try
        {
            await CreateProcess("adb", "version").StartAndGetOutput(token);
            return "Ok";
        }
        catch
        {
            return NotFound("Android Debug Bridge - not found");
        }
    }

    [HttpGet]
    public async Task<ActionResult<string>> IOS()
    {
        var source = new CancellationTokenSource();
        var token = source.Token;
        source.CancelAfter(TimeSpan.FromSeconds(10));

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return NotFound("PMetrium Native supports IOS only on OSX platforms");

        try
        {
            await CreateProcess("xctrace", "version").StartAndGetOutput(token);
        }
        catch
        {
            return NotFound("xctrace - not found");
        }
        
        var output = await CreateProcess("xctrace", "list templates").StartAndGetOutput(token);
            
        if(!output.Contains("PMetriumNative", StringComparison.InvariantCultureIgnoreCase))
            return NotFound("PMetriumNative instruments template - not found");

        try
        {
            await CreateProcess("ideviceinfo", "--version").StartAndGetOutput(token);
        }
        catch
        {
            return NotFound(
                "libimobiledevice - tools (idevicesyslog and ideviceinfo) not found. \n " +
                "More info: https://github.com/libimobiledevice/libimobiledevice");
        }

        return "Ok";
    }
}