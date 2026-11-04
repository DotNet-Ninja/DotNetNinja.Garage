using Microsoft.AspNetCore.Mvc;

namespace DotNetNinja.Garage.Controllers;

public class MvcContext<TController> : IMvcContext<TController> where TController : Controller
{
    public MvcContext(ILogger<TController> logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }
}