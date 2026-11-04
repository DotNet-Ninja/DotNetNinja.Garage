using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetNinja.Garage.Controllers;

[Authorize]
public class GarageController : MvcController<GarageController>
{
    public GarageController(IMvcContext<GarageController> mvcContext) : base(mvcContext)
    {
    }

    [HttpGet]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Pages");
    }
}
