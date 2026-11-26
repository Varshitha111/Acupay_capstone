USE AcuPay;
GO

DELETE FROM dbo.Invoices;
DBCC CHECKIDENT (N'dbo.Invoices', RESEED, 0);
GO
