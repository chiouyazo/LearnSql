CREATE TABLE ItemGroup (
                           Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                           Name            NVARCHAR(100)  NOT NULL COLLATE NOCASE,
                           ParentGroupId   INT            NULL,                -- tree structure
                           CONSTRAINT UQ_ItemGroup_Name UNIQUE (Name),
                           CONSTRAINT FK_ItemGroup_Parent FOREIGN KEY (ParentGroupId) REFERENCES ItemGroup (Id)
);

CREATE TABLE Item (
                      Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                      ItemNo       NVARCHAR(30)   NOT NULL COLLATE NOCASE,
                      Name            NVARCHAR(200)  NOT NULL COLLATE NOCASE,
                      Description     NVARCHAR(2000) NULL,
                      ItemGroupId  INT            NOT NULL,
                      TaxRateId       INT            NOT NULL,
                      Unit            NVARCHAR(10)   NOT NULL DEFAULT 'pcs',
                      SalesPriceNet   DECIMAL(18,2)  NOT NULL CHECK (SalesPriceNet >= 0),
                      PurchasePrice   DECIMAL(18,2)  NULL CHECK (PurchasePrice IS NULL OR PurchasePrice >= 0),
                      IsService       BIT            NOT NULL DEFAULT 0 CHECK (IsService IN (0,1)),
                      IsActive        BIT            NOT NULL DEFAULT 1 CHECK (IsActive IN (0,1)),
                      CreatedAt       DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
                      CONSTRAINT UQ_Item_ItemNo UNIQUE (ItemNo),
                      CONSTRAINT FK_Item_ItemGroup FOREIGN KEY (ItemGroupId) REFERENCES ItemGroup (Id),
                      CONSTRAINT FK_Item_TaxRate      FOREIGN KEY (TaxRateId)      REFERENCES TaxRate (Id)
);
CREATE INDEX IX_Item_ItemGroup ON Item (ItemGroupId);
CREATE INDEX IX_Item_Name ON Item (Name);

CREATE TABLE Warehouse (
                           Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                           Name        NVARCHAR(100)  NOT NULL COLLATE NOCASE,
                           City        NVARCHAR(100)  NULL,
                           CONSTRAINT UQ_Warehouse_Name UNIQUE (Name)
);

CREATE TABLE Stock (
                       Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                       WarehouseId     INT            NOT NULL,
                       ItemId       INT            NOT NULL,
                       Quantity        DECIMAL(18,3)  NOT NULL DEFAULT 0 CHECK (Quantity >= 0),
                       MinQuantity     DECIMAL(18,3)  NOT NULL DEFAULT 0 CHECK (MinQuantity >= 0),
                       LastChangedAt   DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
                       CONSTRAINT UQ_Stock UNIQUE (WarehouseId, ItemId),
                       CONSTRAINT FK_Stock_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouse (Id) ON DELETE CASCADE,
                       CONSTRAINT FK_Stock_Item   FOREIGN KEY (ItemId)   REFERENCES Item (Id)
);

CREATE TABLE Orders (
                        Id                    INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderNo               NVARCHAR(20)   NOT NULL COLLATE NOCASE,
                        CustomerId            INT            NOT NULL,
                        ContactId             INT            NULL,
                        EmployeeId            INT            NOT NULL,           -- responsible clerk
                        BillingAddressId      INT            NULL,
                        ShippingAddressId     INT            NULL,
                        CurrencyId            INT            NOT NULL,
                        PaymentTermId         INT            NOT NULL,
                        Status                NVARCHAR(20)   NOT NULL DEFAULT 'Open' COLLATE NOCASE
                          CHECK (Status IN ('Open','InProgress','Confirmed','Delivered','Completed','Cancelled')),
                        OrderDate             DATE           NOT NULL DEFAULT (date('now')),
                        RequestedDeliveryDate DATE           NULL,
                        CustomerReference     NVARCHAR(100)  NULL,                -- customer's PO number
    -- contract concept
                        IsContract            BIT            NOT NULL DEFAULT 0 CHECK (IsContract IN (0,1)),
                        ContractStart         DATE           NULL,
                        ContractEnd           DATE           NULL,
                        BillingIntervalMonths INT            NULL CHECK (BillingIntervalMonths IS NULL OR BillingIntervalMonths > 0),
                        IsAutoRenew           BIT            NOT NULL DEFAULT 0 CHECK (IsAutoRenew IN (0,1)),
                        Notes                 NVARCHAR(2000) NULL,
                        CreatedAt             DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        ModifiedAt            DATETIME       NULL,
                        CONSTRAINT UQ_Orders_OrderNo UNIQUE (OrderNo),
                        CONSTRAINT FK_Orders_Customer        FOREIGN KEY (CustomerId)        REFERENCES Customer (Id),
                        CONSTRAINT FK_Orders_Contact         FOREIGN KEY (ContactId)         REFERENCES Contact (Id) ON DELETE SET NULL,
                        CONSTRAINT FK_Orders_Employee        FOREIGN KEY (EmployeeId)        REFERENCES Employee (Id),
                        CONSTRAINT FK_Orders_BillingAddress  FOREIGN KEY (BillingAddressId)  REFERENCES Address (Id),
                        CONSTRAINT FK_Orders_ShippingAddress FOREIGN KEY (ShippingAddressId) REFERENCES Address (Id),
                        CONSTRAINT FK_Orders_Currency        FOREIGN KEY (CurrencyId)        REFERENCES Currency (Id),
                        CONSTRAINT FK_Orders_PaymentTerm     FOREIGN KEY (PaymentTermId)     REFERENCES PaymentTerm (Id),
    -- contracts need a start and billing interval; non-contracts must not carry contract data
                        CONSTRAINT CK_Orders_Contract CHECK (
                            (IsContract = 1 AND ContractStart IS NOT NULL AND BillingIntervalMonths IS NOT NULL)
                                OR
                            (IsContract = 0 AND ContractStart IS NULL AND ContractEnd IS NULL
                                AND BillingIntervalMonths IS NULL AND IsAutoRenew = 0)),
                        CONSTRAINT CK_Orders_ContractPeriod CHECK (ContractEnd IS NULL OR ContractEnd >= ContractStart)
);
CREATE INDEX IX_Orders_Customer ON Orders (CustomerId);
CREATE INDEX IX_Orders_Status   ON Orders (Status);
CREATE INDEX IX_Orders_Date     ON Orders (OrderDate);
CREATE INDEX IX_Orders_Employee ON Orders (EmployeeId);

CREATE TABLE OrderPos (
                          Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                          OrderId    INT            NOT NULL,
                          PosNo          INT            NOT NULL CHECK (PosNo > 0),
                          ItemId       INT            NOT NULL,
                          Description     NVARCHAR(200)  NOT NULL,               -- copy of item name (historical)
                          Quantity        DECIMAL(18,3)  NOT NULL CHECK (Quantity > 0),
                          Unit            NVARCHAR(10)   NOT NULL DEFAULT 'pcs',
                          UnitPriceNet    DECIMAL(18,2)  NOT NULL CHECK (UnitPriceNet >= 0),
                          DiscountPercent DECIMAL(5,2)   NOT NULL DEFAULT 0 CHECK (DiscountPercent >= 0 AND DiscountPercent <= 100),
                          TaxRateId       INT            NOT NULL,
    -- computed column (like MSSQL "AS (...)")
                          LineTotalNet    DECIMAL(18,2)  GENERATED ALWAYS AS
                              (ROUND(Quantity * UnitPriceNet * (1 - DiscountPercent / 100.0), 2)) VIRTUAL,
                          CONSTRAINT UQ_OrderPos_PosNo UNIQUE (OrderId, PosNo),
                          CONSTRAINT FK_OrderPos_Orders FOREIGN KEY (OrderId) REFERENCES Orders (Id) ON DELETE CASCADE,
                          CONSTRAINT FK_OrderPos_Item    FOREIGN KEY (ItemId)    REFERENCES Item (Id),
                          CONSTRAINT FK_OrderPos_TaxRate    FOREIGN KEY (TaxRateId)    REFERENCES TaxRate (Id)
);
CREATE INDEX IX_OrderPos_Item ON OrderPos (ItemId);

CREATE TABLE OrderArc (
                          Id                    INTEGER PRIMARY KEY AUTOINCREMENT,
                          ConfirmationNo        NVARCHAR(20)   NOT NULL COLLATE NOCASE,
                          OrderId          INT            NOT NULL,
                          Version               INT            NOT NULL DEFAULT 1 CHECK (Version >= 1),
                          ConfirmationDate      DATE           NOT NULL DEFAULT (date('now')),
                          ConfirmedDeliveryDate DATE           NULL,
                          CreatedById           INT            NOT NULL,
                          SentAt                DATETIME       NULL,
                          SentToEmail           NVARCHAR(255)  NULL,
                          IsSent                BIT            NOT NULL DEFAULT 0 CHECK (IsSent IN (0,1)),
                          TotalNet              DECIMAL(18,2)  NOT NULL DEFAULT 0 CHECK (TotalNet >= 0),
                          TotalTax              DECIMAL(18,2)  NOT NULL DEFAULT 0 CHECK (TotalTax >= 0),
                          TotalGross            DECIMAL(18,2)  GENERATED ALWAYS AS (TotalNet + TotalTax) VIRTUAL,
                          Notes                 NVARCHAR(2000) NULL,
                          CreatedAt             DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
                          CONSTRAINT UQ_OrderArc_No UNIQUE (ConfirmationNo),
                          CONSTRAINT UQ_OrderArc_Version UNIQUE (OrderId, Version),
                          CONSTRAINT FK_OrderArc_Orders FOREIGN KEY (OrderId) REFERENCES Orders (Id),
                          CONSTRAINT FK_OrderArc_CreatedBy  FOREIGN KEY (CreatedById)  REFERENCES Employee (Id),
                          CONSTRAINT CK_OrderArc_Sent CHECK (IsSent = 0 OR SentAt IS NOT NULL)
);
CREATE INDEX IX_OrderArc_Orders ON OrderArc (OrderId);

CREATE TABLE Invoice (
                         Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                         InvoiceNo           NVARCHAR(20)   NOT NULL COLLATE NOCASE,
                         CustomerId          INT            NOT NULL,
                         OrderId        INT            NULL,               -- credit notes may have no order
                         BillingAddressId    INT            NULL,
                         CurrencyId          INT            NOT NULL,
                         PaymentTermId       INT            NOT NULL,
                         CreatedById         INT            NOT NULL,
                         InvoiceDate         DATE           NOT NULL DEFAULT (date('now')),
                         ServiceDate         DATE           NULL,
                         DueDate             DATE           NOT NULL,
                         Status              NVARCHAR(20)   NOT NULL DEFAULT 'Draft' COLLATE NOCASE
                        CHECK (Status IN ('Draft','Issued','PartiallyPaid','Paid','Cancelled')),
    -- credit note concept
                         IsCreditNote        BIT            NOT NULL DEFAULT 0 CHECK (IsCreditNote IN (0,1)),
                         OriginalInvoiceId   INT            NULL,               -- for credit notes: the invoice being credited
                         CreditNoteReason    NVARCHAR(500)  NULL,
                         TotalNet            DECIMAL(18,2)  NOT NULL DEFAULT 0,
                         TotalTax            DECIMAL(18,2)  NOT NULL DEFAULT 0,
                         TotalGross          DECIMAL(18,2)  GENERATED ALWAYS AS (TotalNet + TotalTax) VIRTUAL,
                         Notes               NVARCHAR(2000) NULL,
                         CreatedAt           DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
                         CONSTRAINT UQ_Invoice_InvoiceNo UNIQUE (InvoiceNo),
                         CONSTRAINT FK_Invoice_Customer       FOREIGN KEY (CustomerId)        REFERENCES Customer (Id),
                         CONSTRAINT FK_Invoice_Orders     FOREIGN KEY (OrderId)      REFERENCES Orders (Id),
                         CONSTRAINT FK_Invoice_BillingAddress FOREIGN KEY (BillingAddressId)  REFERENCES Address (Id),
                         CONSTRAINT FK_Invoice_Currency       FOREIGN KEY (CurrencyId)        REFERENCES Currency (Id),
                         CONSTRAINT FK_Invoice_PaymentTerm    FOREIGN KEY (PaymentTermId)     REFERENCES PaymentTerm (Id),
                         CONSTRAINT FK_Invoice_CreatedBy      FOREIGN KEY (CreatedById)       REFERENCES Employee (Id),
                         CONSTRAINT FK_Invoice_OriginalInvoice FOREIGN KEY (OriginalInvoiceId) REFERENCES Invoice (Id),
                         CONSTRAINT CK_Invoice_DueDate CHECK (DueDate >= InvoiceDate),
    -- a credit note needs a reason and a reference; a normal invoice must have neither
                         CONSTRAINT CK_Invoice_CreditNote CHECK (
                             (IsCreditNote = 1 AND OriginalInvoiceId IS NOT NULL AND CreditNoteReason IS NOT NULL
                                 AND OriginalInvoiceId <> Id)
                                 OR
                             (IsCreditNote = 0 AND OriginalInvoiceId IS NULL AND CreditNoteReason IS NULL)),
    -- invoices are positive, credit notes negative or zero
                         CONSTRAINT CK_Invoice_Sign CHECK (
                             (IsCreditNote = 0 AND TotalNet >= 0) OR (IsCreditNote = 1 AND TotalNet <= 0))
);
CREATE INDEX IX_Invoice_Customer      ON Invoice (CustomerId);
CREATE INDEX IX_Invoice_Orders    ON Invoice (OrderId);
CREATE INDEX IX_Invoice_Status        ON Invoice (Status, DueDate);
CREATE INDEX IX_Invoice_OriginalInvoice ON Invoice (OriginalInvoiceId) WHERE OriginalInvoiceId IS NOT NULL;

CREATE TABLE InvoicePos (
                            Id                INTEGER PRIMARY KEY AUTOINCREMENT,
                            InvoiceId         INT            NOT NULL,
                            PosNo            INT            NOT NULL CHECK (PosNo > 0),
                            OrderPosId  INT            NULL,                   -- link back to the order position
                            ItemId         INT            NOT NULL,
                            Description       NVARCHAR(200)  NOT NULL,
                            Quantity          DECIMAL(18,3)  NOT NULL CHECK (Quantity <> 0),   -- may be negative on credit notes
                            Unit              NVARCHAR(10)   NOT NULL DEFAULT 'pcs',
                            UnitPriceNet      DECIMAL(18,2)  NOT NULL CHECK (UnitPriceNet >= 0),
                            DiscountPercent   DECIMAL(5,2)   NOT NULL DEFAULT 0 CHECK (DiscountPercent >= 0 AND DiscountPercent <= 100),
                            TaxRateId         INT            NOT NULL,
                            LineTotalNet      DECIMAL(18,2)  GENERATED ALWAYS AS
                                (ROUND(Quantity * UnitPriceNet * (1 - DiscountPercent / 100.0), 2)) VIRTUAL,
                            CONSTRAINT UQ_InvoicePos_PosNo UNIQUE (InvoiceId, PosNo),
                            CONSTRAINT FK_InvoicePos_Invoice        FOREIGN KEY (InvoiceId)        REFERENCES Invoice (Id) ON DELETE CASCADE,
                            CONSTRAINT FK_InvoicePos_OrderPos FOREIGN KEY (OrderPosId) REFERENCES OrderPos (Id) ON DELETE SET NULL,
                            CONSTRAINT FK_InvoicePos_Item        FOREIGN KEY (ItemId)        REFERENCES Item (Id),
                            CONSTRAINT FK_InvoicePos_TaxRate        FOREIGN KEY (TaxRateId)        REFERENCES TaxRate (Id)
);
CREATE INDEX IX_InvoicePos_Item ON InvoicePos (ItemId);

CREATE TABLE Payment (
                         Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                         InvoiceId       INT            NOT NULL,
                         PaymentDate     DATE           NOT NULL DEFAULT (date('now')),
                         Amount          DECIMAL(18,2)  NOT NULL CHECK (Amount > 0),
                         Method          NVARCHAR(20)   NOT NULL COLLATE NOCASE
                    CHECK (Method IN ('BankTransfer','DirectDebit','CreditCard','Cash','Offset')),
                         Reference       NVARCHAR(200)  NULL,
                         RecordedById    INT            NOT NULL,
                         RecordedAt      DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,
                         CONSTRAINT FK_Payment_Invoice    FOREIGN KEY (InvoiceId)    REFERENCES Invoice (Id),
                         CONSTRAINT FK_Payment_RecordedBy FOREIGN KEY (RecordedById) REFERENCES Employee (Id)
);
CREATE INDEX IX_Payment_Invoice ON Payment (InvoiceId);

-- keep ModifiedAt up to date (like an update trigger in MSSQL)
CREATE TRIGGER TR_Customer_ModifiedAt AFTER UPDATE ON Customer
    FOR EACH ROW WHEN NEW.ModifiedAt IS OLD.ModifiedAt
BEGIN UPDATE Customer SET ModifiedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id; END;

CREATE TRIGGER TR_Orders_ModifiedAt AFTER UPDATE ON Orders
    FOR EACH ROW WHEN NEW.ModifiedAt IS OLD.ModifiedAt
BEGIN UPDATE Orders SET ModifiedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id; END;