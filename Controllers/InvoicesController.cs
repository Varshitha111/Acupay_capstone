using AcuPay_Capstone.Data;
using AcuPay_Capstone.Domain;
using AcuPay_Capstone.Dtos;
using AcuPay_Capstone.Exceptions;
using AcuPay_Capstone.Middleware;
using AcuPay_Capstone.Rules;
using Microsoft.AspNetCore.Mvc;

namespace AcuPay_Capstone.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceRepository _repository;
    private readonly MatchingRules _matchingRules;
    private readonly ApprovalRules _approvalRules;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(
        IInvoiceRepository repository,
        MatchingRules matchingRules,
        ApprovalRules approvalRules,
        ILogger<InvoicesController> logger)
    {
        _repository = repository;
        _matchingRules = matchingRules;
        _approvalRules = approvalRules;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<InvoiceResponse>>> GetInvoices([FromQuery] string? status)
    {
        string? statusFilter = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<InvoiceStatus>(status, true, out var parsed) || !Enum.IsDefined(parsed))
                throw new InvalidRequestException("Status must be Pending, Matched, Exception, Approved or Rejected.");

            statusFilter = parsed.ToString();
        }

        var invoices = await _repository.GetInvoicesAsync(statusFilter);
        return Ok(invoices.Select(invoice => invoice.ToResponse()).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceResponse>> GetInvoice(int id)
    {
        var invoice = await GetInvoiceOrThrow(id);
        return Ok(invoice.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceResponse>> Create(CreateInvoiceRequest request)
    {
        var user = HttpContext.GetCurrentUser();
        RequireRole(user, "Requester", "Finance");

        string? poNumber = string.IsNullOrWhiteSpace(request.PoNumber) ? null : request.PoNumber.Trim();

        int newId = await _repository.InsertInvoiceAsync(
            request.InvoiceNumber.Trim(),
            request.VendorCode.Trim(),
            poNumber,
            request.InvoiceAmount,
            user.Username);

        var created = await GetInvoiceOrThrow(newId);
        _logger.LogInformation("Invoice {InvoiceId} created by {User}", newId, user.Username);

        return CreatedAtAction(nameof(GetInvoice), new { id = newId }, created.ToResponse());
    }

    [HttpPost("{id:int}/match")]
    public async Task<ActionResult<InvoiceResponse>> Match(int id)
    {
        var user = HttpContext.GetCurrentUser();
        RequireRole(user, "Requester", "Finance");

        var details = await GetInvoiceOrThrow(id);
        MatchResult result = _matchingRules.Match(details.ToInvoice(), details.ToPurchaseOrder());

        await _repository.UpdateInvoiceMatchAsync(id, result.Status.ToString(), result.Note, result.DecidedBy);
        _logger.LogInformation("Invoice {InvoiceId} matched by {User}: {Status} {Note}", id, user.Username, result.Status, result.Note);

        var updated = await GetInvoiceOrThrow(id);
        return Ok(updated.ToResponse());
    }

    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<InvoiceResponse>> Approve(int id)
    {
        var user = HttpContext.GetCurrentUser();
        var details = await GetInvoiceOrThrow(id);

        _approvalRules.CheckApprove(details.ToInvoice(), user.Username, user.Role);

        await _repository.DecideInvoiceAsync(id, InvoiceStatus.Approved.ToString(), user.Username, null);
        _logger.LogInformation("Invoice {InvoiceId} approved by {User}", id, user.Username);

        var updated = await GetInvoiceOrThrow(id);
        return Ok(updated.ToResponse());
    }

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<InvoiceResponse>> Reject(int id, [FromBody] RejectInvoiceRequest? request)
    {
        var user = HttpContext.GetCurrentUser();
        var details = await GetInvoiceOrThrow(id);
        string? reason = request?.Reason?.Trim();

        _approvalRules.CheckReject(details.ToInvoice(), user.Username, user.Role, reason);

        await _repository.DecideInvoiceAsync(id, InvoiceStatus.Rejected.ToString(), user.Username, reason);
        _logger.LogInformation("Invoice {InvoiceId} rejected by {User}", id, user.Username);

        var updated = await GetInvoiceOrThrow(id);
        return Ok(updated.ToResponse());
    }

    private async Task<InvoiceDetails> GetInvoiceOrThrow(int id)
    {
        var invoice = await _repository.GetInvoiceByIdAsync(id);
        if (invoice == null)
            throw new InvoiceNotFoundException(id);

        return invoice;
    }

    private static void RequireRole(AppUser user, params string[] allowedRoles)
    {
        bool allowed = allowedRoles.Any(role => string.Equals(role, user.Role, StringComparison.OrdinalIgnoreCase));
        if (!allowed)
            throw new ForbiddenRoleException();
    }
}
