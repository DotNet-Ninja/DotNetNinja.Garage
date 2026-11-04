using Microsoft.AspNetCore.Mvc;

namespace DotNetNinja.Garage.Controllers;

public abstract class MvcController<TController> : Controller where TController : Controller
{
    protected MvcController(IMvcContext<TController> mvcContext)
    {
        Logger = mvcContext.Logger;
    }

    protected ILogger Logger { get; }
}