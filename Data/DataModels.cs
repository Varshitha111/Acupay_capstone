namespace AcuPay_Capstone.Data;

public class InvoiceDetails
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public int VendorId { get; set; }
    public string VendorCode { get; set; } = "";
    public int? PoId { get; set; }
    public string? PoNumber { get; set; }
    public int? PoVendorId { get; set; }
    public decimal? PoAmount { get; set; }
    public decimal InvoiceAmount { get; set; }
    public decimal? VariancePercent { get; set; }
    public string Status { get; set; } = "";
    public string? MatchNote { get; set; }
    public string CreatedBy { get; set; } = "";
    public string? DecidedBy { get; set; }
    public string? DecisionReason { get; set; }
}

public class AppUser
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
}
