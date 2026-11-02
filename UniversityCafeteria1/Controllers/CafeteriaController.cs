using Microsoft.AspNetCore.Mvc;

namespace UniversityCafeteria1.Controllers
{
    public class CafeteriaController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult Products() => View();

        public IActionResult Suppliers() => View();

        public IActionResult SupplyStatus() => View();
    }
}