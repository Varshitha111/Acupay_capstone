using AcuPay_Capstone.Domain;

namespace AcuPay_Capstone.Data;

public class DuplicateInvoiceException : AcuPayException
{
    public DuplicateInvoiceException(string invoiceNumber, string vendorCode)
        : base("DUPLICATE_INVOICE", $"Vendor {vendorCode} has already billed invoice number {invoiceNumber}.") { }
}

public class ReferenceNotFoundException : AcuPayException
{
    public ReferenceNotFoundException(string code, string message) : base(code, message) { }
}
