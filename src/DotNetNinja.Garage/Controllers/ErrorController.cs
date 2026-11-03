using Microsoft.AspNetCore.Mvc;

namespace DotNetNinja.Garage.Controllers;

public class ErrorController : MvcController<ErrorController>
{
    public ErrorController(IMvcContext<ErrorController> mvcContext) : base(mvcContext)
    {
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View();
    }
}
