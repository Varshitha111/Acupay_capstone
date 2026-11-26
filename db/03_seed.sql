USE AcuPay;
GO

INSERT INTO dbo.Users (Username, [Role])
SELECT s.Username, s.[Role]
FROM (VALUES (N'requester1', N'Requester'),
             (N'approver1',  N'Approver'),
             (N'finance1',   N'Finance')) AS s (Username, [Role])
WHERE NOT EXISTS (SELECT 1 FROM dbo.Users u WHERE u.Username = s.Username);
GO

INSERT INTO dbo.Vendors (VendorCode, VendorName)
SELECT s.VendorCode, s.VendorName
FROM (VALUES (N'V001', N'Alpha Infotech'),
             (N'V002', N'Beta Supplies'),
             (N'V003', N'Gamma Traders')) AS s (VendorCode, VendorName)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Vendors v WHERE v.VendorCode = s.VendorCode);
GO

INSERT INTO dbo.PurchaseOrders (PoNumber, VendorId, PoAmount)
SELECT s.PoNumber, v.VendorId, s.PoAmount
FROM (VALUES (N'PO-1001', N'V001', CAST(100000.00 AS DECIMAL(18,2))),
             (N'PO-1002', N'V001', CAST(50000.00  AS DECIMAL(18,2))),
             (N'PO-1003', N'V002', CAST(20000.00  AS DECIMAL(18,2))),
             (N'PO-1004', N'V003', CAST(40000.00  AS DECIMAL(18,2)))) AS s (PoNumber, VendorCode, PoAmount)
JOIN dbo.Vendors v ON v.VendorCode = s.VendorCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.PurchaseOrders p WHERE p.PoNumber = s.PoNumber);
GO

INSERT INTO dbo.Invoices (InvoiceNumber, VendorId, PoId, InvoiceAmount, [Status], MatchNote, CreatedBy)
SELECT s.InvoiceNumber, v.VendorId, p.PoId, s.InvoiceAmount, N'Pending', NULL, s.CreatedBy
FROM (VALUES (1, N'INV-5001', N'V001', N'PO-1001', CAST(101500.00 AS DECIMAL(18,2)), N'requester1'),
             (2, N'INV-5002', N'V001', N'PO-1002', CAST(49000.00  AS DECIMAL(18,2)), N'requester1'),
             (3, N'INV-5003', N'V002', N'PO-1003', CAST(23000.00  AS DECIMAL(18,2)), N'approver1'),
             (4, N'INV-5004', N'V002', NULL,       CAST(12000.00  AS DECIMAL(18,2)), N'requester1'),
             (5, N'INV-5005', N'V001', N'PO-1003', CAST(19000.00  AS DECIMAL(18,2)), N'requester1'),
             (6, N'INV-5006', N'V003', N'PO-1004', CAST(36000.00  AS DECIMAL(18,2)), N'requester1'))
     AS s (SortOrder, InvoiceNumber, VendorCode, PoNumber, InvoiceAmount, CreatedBy)
JOIN dbo.Vendors v ON v.VendorCode = s.VendorCode
LEFT JOIN dbo.PurchaseOrders p ON p.PoNumber = s.PoNumber
WHERE NOT EXISTS (SELECT 1 FROM dbo.Invoices i WHERE i.VendorId = v.VendorId AND i.InvoiceNumber = s.InvoiceNumber)
ORDER BY s.SortOrder;
GO
