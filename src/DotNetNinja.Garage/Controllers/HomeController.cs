using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace DotNetNinja.Garage.Controllers;

public class HomeController : MvcController<HomeController>
{
    public HomeController(IMvcContext<HomeController> mvcContext) : base(mvcContext)
    {
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult About()
    {
        ViewData["Version"] = typeof(HomeController).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "Unknown";
        return View();
    }
}
