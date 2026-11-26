IF DB_ID(N'AcuPay') IS NULL
    CREATE DATABASE AcuPay;
GO

USE AcuPay;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users
(
    UserId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Username NVARCHAR(50)      NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
    [Role]   NVARCHAR(20)      NOT NULL CONSTRAINT CK_Users_Role CHECK ([Role] IN (N'Requester', N'Approver', N'Finance'))
);
GO

IF OBJECT_ID(N'dbo.Vendors', N'U') IS NULL
CREATE TABLE dbo.Vendors
(
    VendorId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vendors PRIMARY KEY,
    VendorCode NVARCHAR(20)      NOT NULL CONSTRAINT UQ_Vendors_VendorCode UNIQUE,
    VendorName NVARCHAR(100)     NOT NULL
);
GO

IF OBJECT_ID(N'dbo.PurchaseOrders', N'U') IS NULL
CREATE TABLE dbo.PurchaseOrders
(
    PoId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PurchaseOrders PRIMARY KEY,
    PoNumber NVARCHAR(20)      NOT NULL CONSTRAINT UQ_PurchaseOrders_PoNumber UNIQUE,
    VendorId INT               NOT NULL CONSTRAINT FK_PurchaseOrders_Vendors REFERENCES dbo.Vendors (VendorId),
    PoAmount DECIMAL(18,2)     NOT NULL
);
GO

IF OBJECT_ID(N'dbo.Invoices', N'U') IS NULL
CREATE TABLE dbo.Invoices
(
    InvoiceId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Invoices PRIMARY KEY,
    InvoiceNumber  NVARCHAR(30)      NOT NULL,
    VendorId       INT               NOT NULL CONSTRAINT FK_Invoices_Vendors REFERENCES dbo.Vendors (VendorId),
    PoId           INT               NULL     CONSTRAINT FK_Invoices_PurchaseOrders REFERENCES dbo.PurchaseOrders (PoId),
    InvoiceAmount  DECIMAL(18,2)     NOT NULL CONSTRAINT CK_Invoices_Amount CHECK (InvoiceAmount > 0),
    [Status]       NVARCHAR(20)      NOT NULL CONSTRAINT DF_Invoices_Status DEFAULT N'Pending'
                                              CONSTRAINT CK_Invoices_Status CHECK ([Status] IN (N'Pending', N'Matched', N'Exception', N'Approved', N'Rejected')),
    MatchNote      NVARCHAR(50)      NULL,
    CreatedBy      NVARCHAR(50)      NOT NULL,
    DecidedBy      NVARCHAR(50)      NULL,
    DecisionReason NVARCHAR(500)     NULL,
    CONSTRAINT UQ_Invoices_Vendor_Number UNIQUE (VendorId, InvoiceNumber)
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Invoices_Status' AND object_id = OBJECT_ID(N'dbo.Invoices'))
    CREATE INDEX IX_Invoices_Status ON dbo.Invoices ([Status]);
GO
