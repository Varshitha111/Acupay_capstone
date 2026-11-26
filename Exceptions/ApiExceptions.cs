using AcuPay_Capstone.Domain;

namespace AcuPay_Capstone.Exceptions;

public class UserHeaderRequiredException : AcuPayException
{
    public UserHeaderRequiredException()
        : base("USER_HEADER_REQUIRED", "The X-User header is required.") { }
}

public class UserNotFoundException : AcuPayException
{
    public UserNotFoundException(string username)
        : base("USER_NOT_FOUND", $"User '{username}' does not exist.") { }
}

public class InvoiceNotFoundException : AcuPayException
{
    public InvoiceNotFoundException(int invoiceId)
        : base("INVOICE_NOT_FOUND", $"Invoice {invoiceId} was not found.") { }
}

public class InvalidRequestException : AcuPayException
{
    public InvalidRequestException(string message)
        : base("VALIDATION_ERROR", message) { }
}
