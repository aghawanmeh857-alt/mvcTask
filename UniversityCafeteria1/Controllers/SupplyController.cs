using Microsoft.AspNetCore.Mvc;

namespace UniversityCafeteria1.Controllers
{
    public class SupplyController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Categories = new string[]
            {
        "Drinks",
        "Food",
        "Snacks",
        "Hot Drinks",
        "Cold Drinks"
            };

            return View();
        }

        public IActionResult Products()
        {
            ViewData["Title"] = "Cafeteria Products";
            ViewData["ManagerName"] = "Ahmad Ali";
            ViewData["ProductCount"] = 5;

            ViewData["Products"] = new string[]
            {
        "Water",
        "Orange Juice",
        "Coffee",
        "Sandwich",
        "Chocolate"
            };

            return View();
        }

        public IActionResult Suppliers()
        {
            ViewBag.Title = "Our Suppliers";
            ViewBag.SupplierCount = 3;

            ViewBag.Suppliers = new string[]
            {
        "Fresh Food Company",
        "Happy Drinks Company",
        "Daily Supplies Company"
            };

            return View();
        }
        [HttpGet]
        public IActionResult CreateRequest() => View();

        [HttpPost]
        public IActionResult SubmitRequest()
        {
            string employeeName = Request.Form["EmployeeName"].ToString();
            string productName = Request.Form["ProductName"].ToString();
            string quantityText = Request.Form["Quantity"].ToString();
            string requiredDate = Request.Form["RequiredDate"].ToString();

            if (string.IsNullOrWhiteSpace(employeeName))
            {
                ViewBag.Error = "Employee name is required.";
                return View();
            }

            if (!int.TryParse(quantityText, out int quantity))
            {
                ViewBag.Error = "Please enter a valid quantity.";
                return View();
            }

            TempData["Message"] =
                $"Request submitted: {quantity} {productName} for {employeeName}. Delivery date: {requiredDate}.";

            return RedirectToAction("About");
        }

        public IActionResult About() => View();


        [HttpGet]
        public IActionResult CheckQuantity(int quantity)
        {
            if (quantity < 20)
            {
                return Content("Small Order");
            }
            else if (quantity <= 50)
            {
                return Content("Medium Order");
            }
            else
            {
                return Content("Large Order");
            }
        }
    }
}