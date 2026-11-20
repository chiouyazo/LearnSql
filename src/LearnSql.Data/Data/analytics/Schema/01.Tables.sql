CREATE TABLE Customer (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    Country     NVARCHAR(30)   NOT NULL COLLATE NOCASE,
    Segment     NVARCHAR(20)   NULL COLLATE NOCASE CHECK (Segment IS NULL OR Segment IN ('Consumer', 'SMB', 'Enterprise')),
    SignupDate  DATE           NOT NULL
);

CREATE TABLE Product (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    Name       NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    Category   NVARCHAR(30)   NOT NULL COLLATE NOCASE,
    ListPrice  DECIMAL(10,2)  NOT NULL CHECK (ListPrice > 0)
);

CREATE TABLE Orders (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerId   INT            NOT NULL,
    OrderDate    DATE           NOT NULL,
    Channel      NVARCHAR(10)   NULL COLLATE NOCASE CHECK (Channel IS NULL OR Channel IN ('Web', 'Store', 'Phone')),
    ShippedDate  DATE           NULL,
    Status       NVARCHAR(15)   NOT NULL COLLATE NOCASE CHECK (Status IN ('Open', 'Shipped', 'Completed', 'Cancelled')),
    CONSTRAINT FK_Orders_Customer FOREIGN KEY (CustomerId) REFERENCES Customer (Id)
);

CREATE TABLE OrderLine (
    Id               INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderId          INT            NOT NULL,
    ProductId        INT            NOT NULL,
    Quantity         INT            NOT NULL CHECK (Quantity > 0),
    UnitPrice        DECIMAL(10,2)  NOT NULL CHECK (UnitPrice >= 0),
    DiscountPercent  DECIMAL(5,2)   NULL CHECK (DiscountPercent IS NULL OR (DiscountPercent >= 0 AND DiscountPercent <= 100)),
    CONSTRAINT UQ_OrderLine UNIQUE (OrderId, ProductId),
    CONSTRAINT FK_OrderLine_Orders FOREIGN KEY (OrderId) REFERENCES Orders (Id),
    CONSTRAINT FK_OrderLine_Product FOREIGN KEY (ProductId) REFERENCES Product (Id)
);

CREATE INDEX IX_Orders_Customer ON Orders (CustomerId);
CREATE INDEX IX_Orders_Date ON Orders (OrderDate);
CREATE INDEX IX_OrderLine_Product ON OrderLine (ProductId);
