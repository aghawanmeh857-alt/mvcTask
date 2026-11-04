using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using UniversityCafeteria1.Models;

namespace UniversityCafeteria1.Controllers;

public class SupplyRequestController : Controller
{
    private const string DraftKey = "SupplyRequestDraft";
    private readonly SupplyRequestStore store;

    public SupplyRequestController(SupplyRequestStore store)
    {
        this.store = store;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(CreatePage());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AddItem([Bind(Prefix = "Item")] SupplyItem item)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", CreatePage(item));
        }
        var draft = ReadDraft();
        item.ProductName = item.ProductName.Trim();
        item.Category = item.Category.Trim();
        draft.Items.Add(item);
        Calculate(draft);
        // نحول المسودة إلى نص JSON حتى نستطيع حفظها داخل Session.
        HttpContext.Session.SetString(DraftKey, JsonSerializer.Serialize(draft));
        TempData["Message"] = "Product added to your draft.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Review()
    {
        var draft = ReadDraft();
        Calculate(draft);
        return View(draft);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ConfirmRequest(Guid id)
    {
        var draft = ReadDraft();
        if (draft.Items.Count == 0 || draft.Id != id)
        {
            TempData["Message"] = "Add products and review the current draft before confirming.";
            return RedirectToAction(nameof(Review));
        }
        Calculate(draft);
        draft.Status = "Confirmed";
        // نحفظ الطلب المؤكد في قائمة التطبيق، ثم نحذف المسودة فقط.
        store.Add(draft);
        HttpContext.Session.Remove(DraftKey);
        TempData["Message"] = "Supply request confirmed successfully.";
        return RedirectToAction(nameof(MyRequests));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ClearDraft()
    {
        HttpContext.Session.Remove(DraftKey);
        TempData["Message"] = "Draft cleared.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult SavePreference(string priority)
    {
        if (!ValidPriority(priority))
        {
            return BadRequest("Choose a valid priority.");
        }
        // نحفظ أولوية الموظف لمدة سبعة أيام في المتصفح.
        Response.Cookies.Append("PreferredPriority", priority, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            HttpOnly = true,
            IsEssential = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });
        TempData["Message"] = $"Your preferred request priority is {priority}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult MyRequests()
    {
        return View(store.GetAll());
    }

    [HttpGet]
    public IActionResult About()
    {
        return View();
    }

    private SupplyRequest ReadDraft()
    {
        var json = HttpContext.Session.GetString(DraftKey);
        if (json == null)
        {
            return new SupplyRequest();
        }
        return JsonSerializer.Deserialize<SupplyRequest>(json) ?? new SupplyRequest();
    }

    private SupplyRequestPage CreatePage(SupplyItem? item = null)
    {
        string preferred = Request.Cookies["PreferredPriority"] ?? "Normal";
        if (!ValidPriority(preferred))
        {
            preferred = "Normal";
        }
        var draft = ReadDraft();
        Calculate(draft);
        return new SupplyRequestPage
        {
            Item = item ?? new SupplyItem { Priority = preferred },
            Draft = draft,
            PreferredPriority = preferred
        };
    }

    private static bool ValidPriority(string? priority)
    {
        return priority == "Normal" || priority == "Urgent" || priority == "Emergency";
    }

    private static void Calculate(SupplyRequest request)
    {
        foreach (var item in request.Items)
        {
            item.Total = item.Quantity * item.UnitPrice;
        }
        request.Subtotal = request.Items.Sum(item => item.Total);
        // أعلى أولوية بين المنتجات تحدد رسوم الطلب مرة واحدة.
        if (request.Items.Any(item => item.Priority == "Emergency"))
        {
            request.Priority = "Emergency";
        }
        else if (request.Items.Any(item => item.Priority == "Urgent"))
        {
            request.Priority = "Urgent";
        }
        else
        {
            request.Priority = "Normal";
        }
        switch (request.Priority)
        {
            case "Emergency":
                request.PreparationFee = 10m;
                break;
            case "Urgent":
                request.PreparationFee = 5m;
                break;
            default:
                request.PreparationFee = 2m;
                break;
        }
        // الخصم يحسب على مجموع المنتجات قبل إضافة رسوم التجهيز.
        if (request.Subtotal > 200m)
        {
            request.Discount = request.Subtotal * 0.15m;
        }
        else if (request.Subtotal > 100m)
        {
            request.Discount = request.Subtotal * 0.10m;
        }
        else
        {
            request.Discount = 0m;
        }
        request.Discount = decimal.Round(request.Discount, 3, MidpointRounding.AwayFromZero);
        request.FinalTotal = request.Subtotal + request.PreparationFee - request.Discount;
    }
}
