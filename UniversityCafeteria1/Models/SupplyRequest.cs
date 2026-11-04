using System.ComponentModel.DataAnnotations;

namespace UniversityCafeteria1.Models;

public class SupplyItem
{
    [Required, StringLength(100)]
    public string ProductName { get; set; } = "";
    [Required, StringLength(60)]
    public string Category { get; set; } = "";
    [Range(1, 1000000)]
    public int Quantity { get; set; } = 1;
    [Range(typeof(decimal), "0.001", "1000000")]
    public decimal UnitPrice { get; set; }
    [Required, RegularExpression("^(Normal|Urgent|Emergency)$")]
    public string Priority { get; set; } = "Normal";
    public decimal Total { get; set; }
}

public class SupplyRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<SupplyItem> Items { get; set; } = new();
    public string Priority { get; set; } = "Normal";
    public decimal Subtotal { get; set; }
    public decimal PreparationFee { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalTotal { get; set; }
    public string Status { get; set; } = "Draft";
}

public class SupplyRequestPage
{
    public SupplyItem Item { get; set; } = new();
    public SupplyRequest Draft { get; set; } = new();
    public string PreferredPriority { get; set; } = "Normal";
}

// Application-wide in-memory list. Confirmed requests reset when the app restarts.
public class SupplyRequestStore
{
    private readonly List<SupplyRequest> requests = new();
    private readonly object sync = new();

    public bool Add(SupplyRequest request)
    {
        lock (sync)
        {
            if (requests.Any(r => r.Id == request.Id)) return false;
            requests.Add(request);
            return true;
        }
    }

    public List<SupplyRequest> GetAll()
    {
        lock (sync) return requests.ToList();
    }
}
