using Microsoft.AspNetCore.Mvc;

namespace DotNetNinja.Garage.Controllers;

public interface IMvcContext<TController> where TController : Controller
{
    ILogger Logger { get; }
}