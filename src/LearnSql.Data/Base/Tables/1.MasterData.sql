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
    Code        NCHAR(3)       NOT NULL COLLATE NOCASE,   -- EUR, USD, CHF
    Name        NVARCHAR(50)   NOT NULL COLLATE NOCASE,
    Symbol      NVARCHAR(5)    NULL,
    CONSTRAINT UQ_Currency_Code UNIQUE (Code)
);

CREATE TABLE TaxRate (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Description NVARCHAR(50)   NOT NULL COLLATE NOCASE,   -- "19% standard rate"
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
    ManagerId   INT            NULL,   -- circular: Employee.DepartmentId <-> Department.ManagerId
    CONSTRAINT UQ_Department_Code UNIQUE (Code),
    CONSTRAINT FK_Department_Manager FOREIGN KEY (ManagerId) REFERENCES Employee (Id) ON DELETE SET NULL
);