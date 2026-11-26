USE AcuPay;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetInvoices
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT i.InvoiceId,
           i.InvoiceNumber,
           i.VendorId,
           v.VendorCode,
           i.PoId,
           p.PoNumber,
           p.VendorId AS PoVendorId,
           p.PoAmount,
           i.InvoiceAmount,
           CASE WHEN p.PoId IS NULL OR p.VendorId <> i.VendorId OR p.PoAmount = 0 THEN NULL
                ELSE CAST(ROUND((i.InvoiceAmount - p.PoAmount) * 100.0 / p.PoAmount, 2) AS DECIMAL(10,2))
           END AS VariancePercent,
           i.[Status],
           i.MatchNote,
           i.CreatedBy,
           i.DecidedBy,
           i.DecisionReason
    FROM dbo.Invoices AS i
    JOIN dbo.Vendors AS v ON v.VendorId = i.VendorId
    LEFT JOIN dbo.PurchaseOrders AS p ON p.PoId = i.PoId
    WHERE @Status IS NULL OR i.[Status] = @Status
    ORDER BY i.InvoiceId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetInvoiceById
    @InvoiceId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT i.InvoiceId,
           i.InvoiceNumber,
           i.VendorId,
           v.VendorCode,
           i.PoId,
           p.PoNumber,
           p.VendorId AS PoVendorId,
           p.PoAmount,
           i.InvoiceAmount,
           CASE WHEN p.PoId IS NULL OR p.VendorId <> i.VendorId OR p.PoAmount = 0 THEN NULL
                ELSE CAST(ROUND((i.InvoiceAmount - p.PoAmount) * 100.0 / p.PoAmount, 2) AS DECIMAL(10,2))
           END AS VariancePercent,
           i.[Status],
           i.MatchNote,
           i.CreatedBy,
           i.DecidedBy,
           i.DecisionReason
    FROM dbo.Invoices AS i
    JOIN dbo.Vendors AS v ON v.VendorId = i.VendorId
    LEFT JOIN dbo.PurchaseOrders AS p ON p.PoId = i.PoId
    WHERE i.InvoiceId = @InvoiceId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_InsertInvoice
    @InvoiceNumber NVARCHAR(30),
    @VendorCode    NVARCHAR(20),
    @PoNumber      NVARCHAR(20) = NULL,
    @InvoiceAmount DECIMAL(18,2),
    @CreatedBy     NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @VendorId INT = (SELECT VendorId FROM dbo.Vendors WHERE VendorCode = @VendorCode);
    IF @VendorId IS NULL
        THROW 50001, N'Vendor code was not found.', 1;

    DECLARE @PoId INT = NULL;
    IF @PoNumber IS NOT NULL
    BEGIN
        SET @PoId = (SELECT PoId FROM dbo.PurchaseOrders WHERE PoNumber = @PoNumber);
        IF @PoId IS NULL
            THROW 50002, N'Purchase order number was not found.', 1;
    END

    INSERT INTO dbo.Invoices (InvoiceNumber, VendorId, PoId, InvoiceAmount, [Status], CreatedBy)
    VALUES (@InvoiceNumber, @VendorId, @PoId, @InvoiceAmount, N'Pending', @CreatedBy);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS InvoiceId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UpdateInvoiceMatch
    @InvoiceId INT,
    @Status    NVARCHAR(20),
    @MatchNote NVARCHAR(50),
    @DecidedBy NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Invoices
    SET [Status] = @Status,
        MatchNote = @MatchNote,
        DecidedBy = @DecidedBy
    WHERE InvoiceId = @InvoiceId
      AND [Status] IN (N'Pending', N'Matched', N'Exception');

    SELECT @@ROWCOUNT AS RowsChanged;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_DecideInvoice
    @InvoiceId      INT,
    @Status         NVARCHAR(20),
    @DecidedBy      NVARCHAR(50),
    @DecisionReason NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Invoices
    SET [Status] = @Status,
        DecidedBy = @DecidedBy,
        DecisionReason = @DecisionReason
    WHERE InvoiceId = @InvoiceId
      AND [Status] IN (N'Matched', N'Exception');

    SELECT @@ROWCOUNT AS RowsChanged;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetUserByUsername
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT UserId, Username, [Role]
    FROM dbo.Users
    WHERE Username = @Username;
END
GO
