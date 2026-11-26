namespace AcuPay_Capstone.Domain;

public enum InvoiceStatus { Pending, Matched, Exception, Approved, Rejected }

public class Invoice
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public int VendorId { get; set; }
    public int? PoId { get; set; }
    public decimal InvoiceAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public string? MatchNote { get; set; }
    public string CreatedBy { get; set; } = "";
    public string? DecidedBy { get; set; }
    public string? DecisionReason { get; set; }
}

public class PurchaseOrder
{
    public int PoId { get; set; }
    public string PoNumber { get; set; } = "";
    public int VendorId { get; set; }
    public decimal PoAmount { get; set; }
}

public class MatchResult
{
    public InvoiceStatus Status { get; set; }
    public string Note { get; set; } = "";
    public decimal? VariancePercent { get; set; }
    public string? DecidedBy { get; set; }
}
