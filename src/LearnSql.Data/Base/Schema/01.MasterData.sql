PRAGMA foreign_keys = ON;

CREATE TABLE Country (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Iso2        NCHAR(2)       NOT NULL COLLATE NOCASE,
    Iso3        NCHAR(3)       NOT NULL COLLATE NOCASE,
    Name        NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    IsEU        BIT            NOT NULL DEFAULT 0 CHECK (IsEU IN (0,1)),
    CONSTRAINT UQ_Country_Iso2 UNIQUE (Iso2),
    CONSTRAINT UQ_Country_Iso3 UNIQUE (Iso3)
);

CREATE TABLE Currency (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Code        NCHAR(3)       NOT NULL COLLATE NOCASE,
    Name        NVARCHAR(50)   NOT NULL COLLATE NOCASE,
    Symbol      NVARCHAR(5)    NULL,
    CONSTRAINT UQ_Currency_Code UNIQUE (Code)
);

CREATE TABLE TaxRate (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Description NVARCHAR(50)   NOT NULL COLLATE NOCASE,
    Percent     DECIMAL(5,2)   NOT NULL CHECK (Percent >= 0 AND Percent <= 100),
    ValidFrom   DATE           NOT NULL DEFAULT (date('now')),
    ValidTo     DATE           NULL,
    CONSTRAINT CK_TaxRate_Period CHECK (ValidTo IS NULL OR ValidTo >= ValidFrom)
);

CREATE TABLE PaymentTerm (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Description     NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    NetDays         INT            NOT NULL DEFAULT 30 CHECK (NetDays >= 0),
    DiscountDays    INT            NULL CHECK (DiscountDays IS NULL OR DiscountDays >= 0),
    DiscountPercent DECIMAL(5,2)   NULL CHECK (DiscountPercent IS NULL OR (DiscountPercent >= 0 AND DiscountPercent <= 100)),
    CONSTRAINT CK_PaymentTerm_Discount CHECK (
     (DiscountDays IS NULL AND DiscountPercent IS NULL) OR
     (DiscountDays IS NOT NULL AND DiscountPercent IS NOT NULL AND DiscountDays <= NetDays))
);

CREATE TABLE Department (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Code        NVARCHAR(10)   NOT NULL COLLATE NOCASE,
    Name        NVARCHAR(100)  NOT NULL COLLATE NOCASE,
    ManagerId   INT            NULL,
    CONSTRAINT UQ_Department_Code UNIQUE (Code),
    CONSTRAINT FK_Department_Manager FOREIGN KEY (ManagerId) REFERENCES Employee (Id) ON DELETE SET NULL
);

CREATE TABLE ItemGroup (
                           Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                           Name            NVARCHAR(100)  NOT NULL COLLATE NOCASE,
                           ParentGroupId   INT            NULL,
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
