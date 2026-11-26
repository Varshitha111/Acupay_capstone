using AcuPay_Capstone.Domain;
using AcuPay_Capstone.Rules;
using Xunit;

namespace AcuPay_Capstone.Tests;

public class ApprovalRulesTests
{
    private static readonly ApprovalRules Rules = new(10);

    private static Invoice Inv(InvoiceStatus status, string createdBy = "requester1") =>
        new() { InvoiceNumber = "INV-T", Status = status, CreatedBy = createdBy };

    [Fact]
    public void Requester_CannotApprove()
    {
        var ex = Assert.Throws<ForbiddenRoleException>(
            () => Rules.CheckApprove(Inv(InvoiceStatus.Matched), "requester2", "Requester"));
        Assert.Equal("FORBIDDEN_ROLE", ex.Code);
    }

    [Fact]
    public void Approver_CannotDecideOwnInvoice()
    {
        var ex = Assert.Throws<SelfApprovalBlockedException>(
            () => Rules.CheckApprove(Inv(InvoiceStatus.Exception, "approver1"), "approver1", "Approver"));
        Assert.Equal("SELF_APPROVAL_BLOCKED", ex.Code);
    }

    [Theory]
    [InlineData(InvoiceStatus.Approved)]
    [InlineData(InvoiceStatus.Rejected)]
    public void FinalInvoice_CannotBeDecided(InvoiceStatus status)
    {
        var ex = Assert.Throws<InvalidStateException>(
            () => Rules.CheckApprove(Inv(status), "approver1", "Approver"));
        Assert.Equal("INVALID_STATE", ex.Code);
    }

    [Theory]
    [InlineData(InvoiceStatus.Matched)]
    [InlineData(InvoiceStatus.Exception)]
    public void ApproverCanApprove_MatchedAndExceptionInvoices(InvoiceStatus status)
    {
        Rules.CheckApprove(Inv(status), "approver1", "Approver");
    }

    [Fact]
    public void Reject_WithShortReason_Throws()
    {
        var ex = Assert.Throws<ReasonRequiredException>(
            () => Rules.CheckReject(Inv(InvoiceStatus.Matched), "approver1", "Approver", "too short"));
        Assert.Equal("REASON_REQUIRED", ex.Code);
    }

    [Fact]
    public void Reject_WithNoReason_Throws()
    {
        Assert.Throws<ReasonRequiredException>(
            () => Rules.CheckReject(Inv(InvoiceStatus.Matched), "approver1", "Approver", null));
    }

    [Fact]
    public void Reject_WithLongEnoughReason_Passes()
    {
        Rules.CheckReject(Inv(InvoiceStatus.Matched), "approver1", "Approver", "Fifteen chars!!");
    }
}
