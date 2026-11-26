using System.ComponentModel.DataAnnotations;

namespace AcuPay_Capstone.Dtos;

public class CreateInvoiceRequest
{
    [Required]
    [StringLength(30)]
    public string InvoiceNumber { get; set; } = "";

    [Required]
    [StringLength(20)]
    public string VendorCode { get; set; } = "";

    [StringLength(20)]
    public string? PoNumber { get; set; }

    [Range(0.01, 1000000000, ErrorMessage = "InvoiceAmount must be greater than zero.")]
    public decimal InvoiceAmount { get; set; }
}

public class RejectInvoiceRequest
{
    [StringLength(500)]
    public string? Reason { get; set; }
}

public class InvoiceResponse
{
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public string VendorCode { get; set; } = "";
    public string? PoNumber { get; set; }
    public decimal? PoAmount { get; set; }
    public decimal InvoiceAmount { get; set; }
    public decimal? VariancePercent { get; set; }
    public string Status { get; set; } = "";
    public string? MatchNote { get; set; }
    public string CreatedBy { get; set; } = "";
    public string? DecidedBy { get; set; }
    public string? DecisionReason { get; set; }
}

public class HealthResponse
{
    public string Status { get; set; } = "";
    public string Environment { get; set; } = "";
    public string Database { get; set; } = "";
}
