using Microsoft.AspNetCore.Mvc;

namespace CDB_Management.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
