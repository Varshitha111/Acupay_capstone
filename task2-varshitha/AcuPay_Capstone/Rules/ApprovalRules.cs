using AcuPay_Capstone.Domain;

namespace AcuPay_Capstone.Rules;

public class ApprovalRules
{
    private readonly int _minReasonLength;

    public ApprovalRules(int minReasonLength)
    {
        _minReasonLength = minReasonLength;
    }

    public void CheckApprove(Invoice invoice, string username, string role)
    {
        CheckRole(role);
        CheckNotOwnInvoice(invoice, username);
        CheckState(invoice);
    }

    public void CheckReject(Invoice invoice, string username, string role, string? reason)
    {
        CheckApprove(invoice, username, role);

        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < _minReasonLength)
            throw new ReasonRequiredException(_minReasonLength);
    }

    private static void CheckRole(string role)
    {
        bool allowed = string.Equals(role, "Approver", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(role, "Finance", StringComparison.OrdinalIgnoreCase);
        if (!allowed) throw new ForbiddenRoleException();
    }

    private static void CheckNotOwnInvoice(Invoice invoice, string username)
    {
        if (string.Equals(invoice.CreatedBy, username, StringComparison.OrdinalIgnoreCase))
            throw new SelfApprovalBlockedException();
    }

    private static void CheckState(Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.Matched && invoice.Status != InvoiceStatus.Exception)
            throw new InvalidStateException(invoice.Status);
    }
}
