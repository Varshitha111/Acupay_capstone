using AcuPay_Capstone.Domain;

namespace AcuPay_Capstone.Rules;

public class MatchingRules
{
    private readonly decimal _tolerancePercent;
    private readonly decimal _autoApproveLimit;

    public MatchingRules(decimal tolerancePercent, decimal autoApproveLimit)
    {
        _tolerancePercent = tolerancePercent;
        _autoApproveLimit = autoApproveLimit;
    }

    public MatchResult Match(Invoice invoice, PurchaseOrder? po)
    {
        if (invoice.Status == InvoiceStatus.Approved || invoice.Status == InvoiceStatus.Rejected)
            throw new InvalidStateException(invoice.Status);

        if (invoice.PoId == null || po == null)
            return new MatchResult { Status = InvoiceStatus.Exception, Note = "NO_PO" };

        if (po.VendorId != invoice.VendorId)
            return new MatchResult { Status = InvoiceStatus.Exception, Note = "VENDOR_MISMATCH" };

        decimal variance = Math.Round(
            (invoice.InvoiceAmount - po.PoAmount) / po.PoAmount * 100m, 2, MidpointRounding.AwayFromZero);

        if (variance > _tolerancePercent)
            return new MatchResult { Status = InvoiceStatus.Exception, Note = "OVER_PO_AMOUNT", VariancePercent = variance };

        if (invoice.InvoiceAmount <= _autoApproveLimit)
            return new MatchResult
            {
                Status = InvoiceStatus.Approved,
                Note = "AUTO_APPROVED",
                VariancePercent = variance,
                DecidedBy = "SYSTEM"
            };

        return new MatchResult { Status = InvoiceStatus.Matched, Note = "WITHIN_TOLERANCE", VariancePercent = variance };
    }
}
