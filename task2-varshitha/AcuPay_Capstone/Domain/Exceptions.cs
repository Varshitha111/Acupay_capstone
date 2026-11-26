namespace AcuPay_Capstone.Domain;

public abstract class AcuPayException : Exception
{
    public string Code { get; }

    protected AcuPayException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public class ForbiddenRoleException : AcuPayException
{
    public ForbiddenRoleException()
        : base("FORBIDDEN_ROLE", "Your role is not allowed to do this.") { }
}

public class SelfApprovalBlockedException : AcuPayException
{
    public SelfApprovalBlockedException()
        : base("SELF_APPROVAL_BLOCKED", "You cannot decide an invoice you created.") { }
}

public class InvalidStateException : AcuPayException
{
    public InvalidStateException(InvoiceStatus status)
        : base("INVALID_STATE", $"The invoice is {status}, so this action is not allowed.") { }
}

public class ReasonRequiredException : AcuPayException
{
    public ReasonRequiredException(int minLength)
        : base("REASON_REQUIRED", $"A reason of at least {minLength} characters is required.") { }
}
