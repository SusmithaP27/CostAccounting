using Microsoft.AspNetCore.Mvc;

namespace CostAccounting.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
