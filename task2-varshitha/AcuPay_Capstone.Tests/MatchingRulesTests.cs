using AcuPay_Capstone.Domain;
using AcuPay_Capstone.Rules;
using Xunit;

namespace AcuPay_Capstone.Tests;

public class MatchingRulesTests
{
    private static readonly MatchingRules Rules = new(2.00m, 50000.00m);

    private static Invoice Inv(decimal amount, int vendorId, int? poId) =>
        new() { InvoiceNumber = "INV-T", VendorId = vendorId, PoId = poId, InvoiceAmount = amount, CreatedBy = "requester1" };

    private static PurchaseOrder Po(int vendorId, decimal amount) =>
        new() { PoId = 1, PoNumber = "PO-T", VendorId = vendorId, PoAmount = amount };

    [Fact]
    public void NoPo_IsException_NO_PO()
    {
        var r = Rules.Match(Inv(12000m, 2, null), null);
        Assert.Equal(InvoiceStatus.Exception, r.Status);
        Assert.Equal("NO_PO", r.Note);
        Assert.Null(r.VariancePercent);
    }

    [Fact]
    public void PoOfOtherVendor_IsException_VENDOR_MISMATCH()
    {
        var r = Rules.Match(Inv(19000m, 1, 3), Po(2, 20000m));
        Assert.Equal(InvoiceStatus.Exception, r.Status);
        Assert.Equal("VENDOR_MISMATCH", r.Note);
        Assert.Null(r.VariancePercent);
    }

    [Fact]
    public void FifteenPercentOver_IsException_OVER_PO_AMOUNT()
    {
        var r = Rules.Match(Inv(23000m, 2, 3), Po(2, 20000m));
        Assert.Equal(InvoiceStatus.Exception, r.Status);
        Assert.Equal("OVER_PO_AMOUNT", r.Note);
        Assert.Equal(15.00m, r.VariancePercent);
    }

    [Fact]
    public void MinusTwoPercentBoundary_Matches_AndIsAutoApproved()
    {
        var r = Rules.Match(Inv(49000m, 1, 2), Po(1, 50000m));
        Assert.Equal(-2.00m, r.VariancePercent);
        Assert.Equal(InvoiceStatus.Approved, r.Status);
        Assert.Equal("AUTO_APPROVED", r.Note);
        Assert.Equal("SYSTEM", r.DecidedBy);
    }

    [Fact]
    public void PlusTwoPercentBoundary_StillMatches()
    {
        var r = Rules.Match(Inv(102000m, 1, 1), Po(1, 100000m));
        Assert.Equal(2.00m, r.VariancePercent);
        Assert.Equal(InvoiceStatus.Matched, r.Status);
    }

    [Fact]
    public void JustOverTolerance_IsException()
    {
        var r = Rules.Match(Inv(102010m, 1, 1), Po(1, 100000m));
        Assert.Equal(InvoiceStatus.Exception, r.Status);
    }

    [Fact]
    public void MatchedOverLimit_StaysMatched()
    {
        var r = Rules.Match(Inv(101500m, 1, 1), Po(1, 100000m));
        Assert.Equal(1.50m, r.VariancePercent);
        Assert.Equal(InvoiceStatus.Matched, r.Status);
        Assert.Equal("WITHIN_TOLERANCE", r.Note);
        Assert.Null(r.DecidedBy);
    }

    [Fact]
    public void AmountExactlyAtLimit_IsAutoApproved()
    {
        var r = Rules.Match(Inv(50000.00m, 1, 1), Po(1, 50000m));
        Assert.Equal(InvoiceStatus.Approved, r.Status);
    }

    [Fact]
    public void AmountOneCentOverLimit_StaysMatched()
    {
        var r = Rules.Match(Inv(50000.01m, 1, 1), Po(1, 60000m));
        Assert.Equal(InvoiceStatus.Matched, r.Status);
    }

    [Fact]
    public void TenPercentUnder_Matches()
    {
        var r = Rules.Match(Inv(36000m, 3, 4), Po(3, 40000m));
        Assert.Equal(-10.00m, r.VariancePercent);
        Assert.Equal(InvoiceStatus.Approved, r.Status);
    }

    [Fact]
    public void ExceptionIsNeverAutoApproved_EvenWhenSmall()
    {
        var r = Rules.Match(Inv(1500m, 1, 1), Po(1, 1000m));
        Assert.Equal(InvoiceStatus.Exception, r.Status);
        Assert.Null(r.DecidedBy);
    }

    [Theory]
    [InlineData(InvoiceStatus.Approved)]
    [InlineData(InvoiceStatus.Rejected)]
    public void FinalInvoice_CannotBeMatchedAgain(InvoiceStatus status)
    {
        var invoice = Inv(1000m, 1, 1);
        invoice.Status = status;

        var ex = Assert.Throws<InvalidStateException>(() => Rules.Match(invoice, Po(1, 1000m)));
        Assert.Equal("INVALID_STATE", ex.Code);
    }
}
