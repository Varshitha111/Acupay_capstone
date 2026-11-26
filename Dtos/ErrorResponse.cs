namespace AcuPay_Capstone.Dtos;

public record ErrorBody(string Code, string Message, string CorrelationId);

public record ErrorResponse(ErrorBody Error);
