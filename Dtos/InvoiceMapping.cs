using AcuPay_Capstone.Data;
using AcuPay_Capstone.Domain;

namespace AcuPay_Capstone.Dtos;

public static class InvoiceMapping
{
    public static InvoiceResponse ToResponse(this InvoiceDetails details)
    {
        return new InvoiceResponse
        {
            InvoiceId = details.InvoiceId,
            InvoiceNumber = details.InvoiceNumber,
            VendorCode = details.VendorCode,
            PoNumber = details.PoNumber,
            PoAmount = details.PoAmount,
            InvoiceAmount = details.InvoiceAmount,
            VariancePercent = details.VariancePercent,
            Status = details.Status,
            MatchNote = details.MatchNote,
            CreatedBy = details.CreatedBy,
            DecidedBy = details.DecidedBy,
            DecisionReason = details.DecisionReason
        };
    }

    public static Invoice ToInvoice(this InvoiceDetails details)
    {
        return new Invoice
        {
            InvoiceId = details.InvoiceId,
            InvoiceNumber = details.InvoiceNumber,
            VendorId = details.VendorId,
            PoId = details.PoId,
            InvoiceAmount = details.InvoiceAmount,
            Status = Enum.Parse<InvoiceStatus>(details.Status),
            MatchNote = details.MatchNote,
            CreatedBy = details.CreatedBy,
            DecidedBy = details.DecidedBy,
            DecisionReason = details.DecisionReason
        };
    }

    public static PurchaseOrder? ToPurchaseOrder(this InvoiceDetails details)
    {
        if (details.PoId == null || details.PoVendorId == null || details.PoAmount == null)
            return null;

        return new PurchaseOrder
        {
            PoId = details.PoId.Value,
            PoNumber = details.PoNumber ?? "",
            VendorId = details.PoVendorId.Value,
            PoAmount = details.PoAmount.Value
        };
    }
}
