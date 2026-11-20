CREATE TABLE Customer (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
    FirstName    NVARCHAR(50)   NOT NULL COLLATE NOCASE,
    LastName     NVARCHAR(50)   NOT NULL COLLATE NOCASE,
    City         NVARCHAR(50)   NULL COLLATE NOCASE,
    Email        NVARCHAR(100)  NULL COLLATE NOCASE,
    RegisteredOn DATE           NOT NULL
);

CREATE TABLE Product (
    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
    Name      NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    Category  NVARCHAR(30)   NOT NULL COLLATE NOCASE,
    Price     DECIMAL(8,2)   NOT NULL CHECK (Price >= 0),
    Stock     INT            NULL CHECK (Stock IS NULL OR Stock >= 0)
);

CREATE TABLE Orders (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerId  INT            NOT NULL,
    OrderDate   DATE           NOT NULL,
    Status      NVARCHAR(15)   NOT NULL COLLATE NOCASE CHECK (Status IN ('Open', 'Shipped', 'Delivered', 'Cancelled')),
    ShippedOn   DATE           NULL,
    CONSTRAINT FK_Orders_Customer FOREIGN KEY (CustomerId) REFERENCES Customer (Id)
);

CREATE TABLE OrderItem (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    OrderId    INT  NOT NULL,
    ProductId  INT  NOT NULL,
    Quantity   INT  NOT NULL CHECK (Quantity > 0),
    CONSTRAINT UQ_OrderItem UNIQUE (OrderId, ProductId),
    CONSTRAINT FK_OrderItem_Orders FOREIGN KEY (OrderId) REFERENCES Orders (Id),
    CONSTRAINT FK_OrderItem_Product FOREIGN KEY (ProductId) REFERENCES Product (Id)
);

CREATE INDEX IX_Orders_Customer ON Orders (CustomerId);
CREATE INDEX IX_OrderItem_Product ON OrderItem (ProductId);
