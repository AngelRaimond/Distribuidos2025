using Microsoft.AspNetCore.Mvc;

namespace PlantApi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
